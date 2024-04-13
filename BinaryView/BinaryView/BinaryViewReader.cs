using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using GGL.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Collections.ObjectModel;

namespace GGL.IO;

public sealed class BinaryViewReader : StreamStackUser
{
    /// <summary>
    /// If a <see cref="LengthPrefix"/> above this value is read, an <see cref="InvalidDataException"/> will be thrown.
    /// </summary>
    public long LengthPrefixMaxValue { get; set; } = long.MaxValue;


    /// <summary>Initialize BinaryView with a new empty <see cref="MemoryStream"/>.</summary>
    public BinaryViewReader() :
        this(new StreamStack(new MemoryStream(), false))
    { }

    /// <summary>Initialize BinaryView with a new <see cref="FileStream"/>.</summary>
    /// <param name="path">File path</param>
    public BinaryViewReader(string path) :
        this(new StreamStack(new FileStream(path, FileMode.Open, FileAccess.Read), false))
    { }

    /// <summary>Initialize BinaryView with a new <see cref="MemoryStream"/> from the byte array.</summary>
    /// <param name="bytes">Base array</param>
    public BinaryViewReader(byte[] bytes) :
        this(new StreamStack(new MemoryStream(bytes), false))
    { }

    /// <summary>Initialize BinaryView with a Stream.</summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="leaveOpen">Leave the stream open after the BinaryViewReader object is disposed.</param>
    public BinaryViewReader(Stream stream, bool leaveOpen = true) :
        this(new StreamStack(stream, leaveOpen))
    { }

    public BinaryViewReader(StreamStack stack) :
        base(stack)
    { }


    #region read
    /// <summary>Reads a primitive or unmanaged struct from the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T Read<T>() where T : unmanaged
    {
        int size = sizeof(T);
        switch (size)
        {
            case 1:
            {
                byte data = (byte)PeakStream.ReadByte();

                if (NeedBitReorder)
                    data = EndianUtils.BitReverseTable[data];

                var obj = *(T*)&data;
                return obj;
            }
            default:
            {
                AssureBufferSize(size);
                PeakStream.Read(Buffer, 0, size);

                fixed (byte* dataPtr = Buffer)
                {
                    var obj = *(T*)&dataPtr[0];

                    if (NeedReorder)
                        EndianUtils.ReverseObjBits(&obj, size, NeedByteReorder, NeedBitReorder);

                    return obj;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void ReadToPtr(void* ptr, int size)
    {
        switch (size)
        {
            case 1:
            {
                byte data = (byte)PeakStream.ReadByte();

                if (NeedBitReorder)
                    data = EndianUtils.BitReverseTable[data];

                *(byte*)ptr = data;
                return;
            }
            default:
            {
                AssureBufferSize(size);
                PeakStream.Read(Buffer, 0, size);

                fixed (byte* dataPtr = Buffer)
                {
                    for (int i = 0; i < size; i++)
                        *((byte*)ptr + i) = Buffer[i];

                    if (NeedReorder)
                        EndianUtils.ReverseObjBits(&ptr, size, NeedByteReorder, NeedBitReorder);

                    return;
                }
            }
        }
    }

    public unsafe void ReadToPtr(void* ptr, int size, int offset)
    {
        ReadToPtr((byte*)ptr + offset, size);
    }

    public unsafe void ReadToPtr(IntPtr ptr, int size, int offset)
    {
        ReadToPtr((byte*)ptr + offset, size);
    }

    public T ReadIView<T>() where T : IViewReadable, new()
    {
        return ReadToIView(new T());
    }

    public T ReadToIView<T>(T obj) where T : IViewReadable
    {
        obj.ReadFromView(this);
        return obj;
    }

    public unsafe T[] ReadArray<T>() where T : unmanaged => ReadArray<T>(LengthPrefix);

    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, get amount of elements from prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    public unsafe T[] ReadArray<T>(LengthPrefix lengthPrefix) where T : unmanaged
    {
        long length = ReadLengthPrefix(lengthPrefix);
        return ReadArray<T>(length);
    }

    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="length">Amount of elements to read</param>
    public T[] ReadArray<T>(long length) where T : unmanaged
    {
        T[] array = new T[length];
        for (int i = 0; i < array.Length; i++) array[i] = Read<T>();
        return array;
    }

    public void ReadToArray<T>(T[] array, int count, int offset) where T : unmanaged
    {
        for (int i = 0; i < count; i++) array[offset + i] = Read<T>();
    }

    public void ReadToIList<T>(IList<T> list) where T : unmanaged => ReadToIList(list, LengthPrefix);

    public void ReadToIList<T>(IList<T> list, LengthPrefix lengthPrefix, int offset) where T : unmanaged
    {
        long length = ReadLengthPrefix(lengthPrefix);
        ReadToIList(list, length, offset);
    }


    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, get amount of elements from prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">Pointer to existing list to write in</param>
    /// <param name="lengthPrefix"></param>
    public void ReadToIList<T>(IList<T> list, LengthPrefix lengthPrefix) where T : unmanaged
    {
        int listCount = list.Count;
        long length = ReadLengthPrefix(lengthPrefix);
        for (int i = 0; i < length; i++)
        {
            if (i >= listCount)
                list.Add(Read<T>());
            else
                list[i] = Read<T>();
        }
    }

    public void ReadToIList<T>(IList<T> dstList, long count) where T : unmanaged => ReadToIList(dstList, count, 0);

    /// <summary>Reads a list of unmanaged structs from the stream and increases the position by the size of the array elements, reads no length prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="dstList">Pointer to existing list to write in</param>
    /// <param name="offset">Offset in list</param>
    /// <param name="count">Amount of elements to read</param>
    public void ReadToIList<T>(IList<T> dstList, long count, int offset) where T : unmanaged
    {
        int listCount = dstList.Count;
        for (int i = 0; i < count; i++)
        {
            int idx = offset + i;
            var item = Read<T>();
            if (idx >= listCount)
                dstList.Add(item);
            else
                dstList[idx] = item;
        }
    }

    public void ReadToICollection<T>(ICollection<T> collection) where T : unmanaged => ReadToICollection(collection, LengthPrefix);

    public void ReadToICollection<T>(ICollection<T> collection, LengthPrefix lengthPrefix) where T : unmanaged
    {
        long length = ReadLengthPrefix(lengthPrefix);
        ReadToICollection(collection, length);
    }

    public void ReadToICollection<T>(ICollection<T> collection, long count) where T : unmanaged
    {
        for (int i = 0; i < count; i++)
        {
            collection.Add(Read<T>());
        }
    }

    /// <summary>Reads a list of unmanaged structs from the stream and increases the position by the size of the array elements, reads no length prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="dstList">Pointer to existing list to write in</param>
    /// <param name="offset">Offset in dstList</param>
    public unsafe void ReadRemainderToIList<T>(IList<T> dstList, int offset) where T : unmanaged
    {
        long count = Remaining / sizeof(T);
        ReadToIList(dstList, count, offset);
    }

    /// <summary>Reads remaining bytes</summary>
    public byte[] ReadRemainder()
    {
        long count = Remaining;
        return ReadArray<byte>(count);
    }

    /// <summary>Reads a bool from the stream and increases the position by one byte</summary>
    public unsafe bool ReadBoolean() => Read<bool>();

    /// <summary>Reads a char from the stream and increases the position by two bytes</summary>
    public unsafe char ReadChar() => Read<char>();

    /// <summary>Reads a byte from the stream and increases the position by one byte</summary>
    public unsafe byte ReadByte() => Read<byte>();

    /// <summary>Reads a sbyte from the stream and increases the position by one byte</summary>
    public unsafe sbyte ReadSByte() => Read<sbyte>();

    /// <summary>Reads a ushort from the stream and increases the position by two bytes</summary>
    public unsafe ushort ReadUInt16() => Read<ushort>();

    /// <summary>Reads a short from the stream and increases the position by two bytes</summary>
    public unsafe short ReadInt16() => Read<short>();

    /// <summary>Reads a uint from the stream and increases the position by four bytes</summary>
    public unsafe uint ReadUInt32() => Read<uint>();

    /// <summary>Reads a int from the stream and increases the position by four bytes</summary>
    public unsafe int ReadInt32() => Read<int>();

    /// <summary>Reads a ulong from the stream and increases the position by eight bytes</summary>
    public unsafe ulong ReadUInt64() => Read<ulong>();

    /// <summary>Reads a long from the stream and increases the position by eight bytes</summary>
    public unsafe long ReadInt64() => Read<long>();

#if NET5_0_OR_GREATER
    /// <summary>Reads a half from the stream and increases the position by two bytes</summary>
    public unsafe Half ReadHalf() => Read<Half>();
#endif

    /// <summary>Reads a float from the stream and increases the position by four bytes</summary>
    public unsafe float ReadSingle() => Read<float>();

    /// <summary>Reads a double from the stream and increases the position by eight bytes</summary>
    public unsafe double ReadDouble() => Read<double>();

    /// <summary>Reads a decimal from the stream and increases the position by sixteen bytes</summary>
    public unsafe decimal ReadDecimal() => Read<decimal>();

    public string ReadString() => ReadString(LengthPrefix, Encoding);

    /// <summary>Reads a byte-array as string from the stream</summary>
    public string ReadString(LengthPrefix lengthPrefix, Encoding encoding)
    {
        long length = ReadLengthPrefix(lengthPrefix);
        return ReadString(length, encoding);
    }

    public string ReadString(long length) => ReadString(length, Encoding);

    public string ReadString(long length, Encoding encoding)
    {
        if (StringLengthMode == StringLengthMode.CharCount)
        {
            if (encoding.IsSingleByte)
            {
                var bytes = ReadArray<byte>(length);
                return encoding.GetString(bytes);
            }
            else if (encoding == Encoding.Unicode)
            {
                var bytes = ReadArray<byte>(length * 2);
                return encoding.GetString(bytes);
            }
            else if (encoding == Encoding.UTF32)
            {
                var bytes = ReadArray<byte>(length * 4);
                return encoding.GetString(bytes);
            }
            else
            {
                int maxbyte = Math.Min(encoding.GetMaxByteCount((int)length), (int)Remaining);
                var bytes = ReadArray<byte>(maxbyte);
                var chars = encoding.GetChars(bytes);

                if (chars.Length < length)
                    throw new InvalidDataException();

                var str = new string(chars, 0, (int)length);
                int bytecount = encoding.GetByteCount(str);

                int diff = maxbyte - bytecount;
                PeakStream.Seek(-diff, SeekOrigin.Current);

                return str;
            }
        }
        else
        {
            var bytes = ReadArray<byte>(length);
            return encoding.GetString(bytes);
        }
    }

    public string ReadTerminatedString() => ReadTerminatedString(Encoding);

    public string ReadTerminatedString(Encoding encoding)
    {
        var bytes = new List<byte>();
        while (true)
        {
            byte b = ReadByte();
            if (b == 0) break;
            bytes.Add(b);
        }

        return encoding.GetString(bytes.ToArray());
    }

    /// <summary>Reads a array of string from the stream</summary>
    public string[] ReadStringArray(LengthPrefix arrayPrefix = LengthPrefix.Default, LengthPrefix stringPrefix = LengthPrefix.Default)
    {
        var encoding = Encoding;
        long length = ReadLengthPrefix(arrayPrefix);
        string[] retData = new string[length];
        for (int i = 0; i < retData.Length; i++) retData[i] = ReadString(stringPrefix, encoding);
        return retData;
    }

    public long ReadLengthPrefix(LengthPrefix lengthPrefix)
    {
        long length = lengthPrefix switch
        {
            LengthPrefix.Default => ReadLengthPrefix(LengthPrefix),
            LengthPrefix.SByte => ReadSByte(),
            LengthPrefix.Byte => ReadByte(),
            LengthPrefix.Int16 => ReadInt16(),
            LengthPrefix.UInt16 => ReadUInt16(),
            LengthPrefix.Int32 => ReadInt32(),
            LengthPrefix.UInt32 => ReadUInt32(),
            LengthPrefix.Int64 => ReadInt64(),
            LengthPrefix.UInt64 => (long)ReadUInt64(),
            LengthPrefix.Single => (long)ReadSingle(),
            LengthPrefix.Double => (long)ReadDouble(),
            LengthPrefix.UIntSmart15 => (long)ReadIView<UIntSmart15>(),
            LengthPrefix.UIntSmart62 => (long)ReadIView<UIntSmart62>(),
            _ => throw new ArgumentOutOfRangeException(nameof(lengthPrefix))
        };

        if (length - 1 > LengthPrefixMaxValue)
            throw new InvalidDataException($"Length: {length - 1} is over LengthPrefixSafetyMaxValue:{LengthPrefixMaxValue}");

        return length;
    }
#endregion


#region IDisposable Support
    protected override void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (disposing)
            {
                StreamStack.Dispose();
            }

            DisposedValue = true;
        }
    }
#endregion


}

