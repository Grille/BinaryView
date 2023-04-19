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

namespace GGL.IO;

public class BinaryViewReader : StreamStackUser
{
    public long LengthPrefixSafetyMaxValue = long.MaxValue;


    /// <summary>Initialize BinaryView with a empty MemoryStream</summary>
    public BinaryViewReader() :
        this(new StreamStack(new MemoryStream(), false))
    { }

    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryViewReader(string path) :
        this(new StreamStack(new FileStream(path, FileMode.Open, FileAccess.Read), false))
    { }

    /// <summary>Initialize BinaryView with a MemoryStream filled with bytes from array</summary>
    /// <param name="bytes">Base array</param>
    public BinaryViewReader(byte[] bytes) :
        this(new StreamStack(new MemoryStream(bytes), false))
    { }

    /// <summary>Initialize BinaryView with a Stream</summary>
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

    public T ReadIView<T>() where T : IViewReadable, new()
    {
        return ReadToIView(new T());
    }

    public T ReadToIView<T>(T obj) where T : IViewReadable
    {
        obj.ReadFromView(this);
        return obj;
    }

    /// <summary>Reads an serialized object from the stream and increases the position by the size of the data</summary>
    /// <typeparam name="T"></typeparam> Type
    public T Deserialize<T>()
    {
        return Deserialize<T>(DefaultFormatter);
    }

    public T Deserialize<T>(IFormatter formatter)
    {
        return (T)formatter.Deserialize(PeakStream);
    }


    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, get amount of elements from prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    public unsafe T[] ReadArray<T>(LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged
    {
        long length = ReadLengthPrefix(lengthPrefix);
        return ReadArray<T>(length);
    }

    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="length">Amount of elements to read</param>
    public unsafe T[] ReadArray<T>(long length) where T : unmanaged
    {
        T[] array = new T[length];
        for (int i = 0; i < array.Length; i++) array[i] = Read<T>();
        return array;
    }

    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, get amount of elements from prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">Pointer to existing list to write in</param>
    public unsafe void ReadToIList<T>(IList<T> list, LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged
    {
        long length = ReadLengthPrefix(lengthPrefix);
        ReadToIList(list, 0, length);
    }

    /// <summary>Reads a list of unmanaged structs from the stream and increases the position by the size of the array elements, reads no length prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="dstList">Pointer to existing list to write in</param>
    /// <param name="offset">Offset in list</param>
    /// <param name="count">Amount of elements to read</param>
    public unsafe void ReadToIList<T>(IList<T> dstList, int offset, long count) where T : unmanaged
    {
        for (int i = 0; i < count; i++)
        {
            int idx = offset + i;
            var item = Read<T>();
            if (idx >= dstList.Count)
                dstList.Add(item);
            else
                dstList[idx] = item;
        }
    }

    /// <summary>Reads a list of unmanaged structs from the stream and increases the position by the size of the array elements, reads no length prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="dstList">Pointer to existing list to write in</param>
    /// <param name="offset">Offset in dstList</param>
    public unsafe void ReadRemainderToIList<T>(IList<T> dstList, int offset) where T : unmanaged
    {
        long count = Remaining / sizeof(T);
        ReadToIList(dstList, offset, count);
    }

    /// <summary>Reads remaining bytes</summary>
    public unsafe byte[] ReadRemainder()
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

    /// <summary>Reads a byte-array as string from the stream</summary>
    public string ReadString(LengthPrefix lengthPrefix = LengthPrefix.Default, Encoding encoding = null)
    {
        long length = ReadLengthPrefix(lengthPrefix);
        return ReadString(length, encoding);
    }

    public string ReadString(long length, Encoding encoding = null)
    {
        if (encoding == null)
            encoding = DefaultEncoding;

        var bytes = ReadArray<byte>(length);

        return encoding.GetString(bytes);
    }

    public string ReadTerminatedString(Encoding encoding = null)
    {
        if (encoding == null)
            encoding = DefaultEncoding;

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
    public string[] ReadStringArray()
    {
        long length = ReadLengthPrefix(LengthPrefix.UInt32);
        string[] retData = new string[length];
        for (int i = 0; i < retData.Length; i++) retData[i] = ReadString(LengthPrefix.UInt32);
        return retData;
    }

    public long ReadLengthPrefix(LengthPrefix lengthPrefix)
    {
        long length = lengthPrefix switch
        {
            LengthPrefix.Default => ReadLengthPrefix(DefaultLengthPrefix),
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

        if (length - 1 > LengthPrefixSafetyMaxValue)
            throw new InvalidDataException($"Length: {length - 1} is over LengthPrefixSafetyMaxValue:{LengthPrefixSafetyMaxValue}");

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

