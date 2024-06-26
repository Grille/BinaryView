﻿using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using Grille.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Collections.ObjectModel;
using Grille.IO.Interfaces;
using Grille.IO.Internal;

namespace Grille.IO;

public sealed class BinaryViewReader : StreamStackUser
{
    /// <summary>
    /// If a <see cref="LengthPrefix"/> above this value is read, an <see cref="InvalidDataException"/> will be thrown.<br/>
    /// Default value is <see cref="long.MaxValue"/>
    /// </summary>
    /// <remarks>
    /// Useful to fail fast on corrupted data, opposed to having the program hang on trying to read gigabytes of wrong data.
    /// </remarks>
    public long LengthPrefixMaxValue { get; set; } = long.MaxValue;

    /// <summary>
    /// Throw an <see cref="EndOfStreamException"/> on attempt to read beyond end of stream.<br/>
    /// Default value is <c>true</c>
    /// </summary>
    public bool ValidateNotEndOfStream { get; set; } = true;

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
        AssertBytesInStream(size);
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
                        EndianUtils.ReverseBits(&obj, size, NeedByteReorder, NeedBitReorder);

                    return obj;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void ReadToPtr(void* ptr, int byteSize)
    {
        AssertBytesInStream(byteSize);

        var bytePtr = (byte*)ptr;
        switch (byteSize)
        {
            case 1:
            {
                byte data = (byte)PeakStream.ReadByte();

                if (NeedBitReorder)
                    data = EndianUtils.BitReverseTable[data];

                *bytePtr = data;
                return;
            }
            default:
            {
                AssureBufferSize(byteSize);
                PeakStream.Read(Buffer, 0, byteSize);

                for (int i = 0; i < byteSize; i++)
                    bytePtr[i] = Buffer[i];

                if (NeedReorder)
                    EndianUtils.ReverseBits(&ptr, byteSize, NeedByteReorder, NeedBitReorder);

                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertBytesInStream(int count)
    {
        if (ValidateNotEndOfStream && PeakStream.Position + count > PeakStream.Length)
        {
            throw new EndOfStreamException($"The attempted read operation would have read {PeakStream.Position + count - PeakStream.Length} bytes over the end of the stream.");
        }
    }

    public unsafe void ReadToPtr(IntPtr ptr, int byteSize)
    {
        ReadToPtr((void*)ptr, byteSize);
    }

    public unsafe void ReadToPtr<T>(T* ptr) where T : unmanaged
    {
        ReadToPtr(ptr, sizeof(T));
    }

    public unsafe void ReadToRef<T>(ref T obj) where T : unmanaged
    {
        fixed (T* ptr = &obj)
        {
            ReadToPtr(ptr);
        }
    }

    public T ReadIView<T>() where T : IBinaryViewReadable, new()
    {
        return ReadToIView(new T());
    }

    public T ReadToIView<T>(T obj) where T : IBinaryViewReadable
    {
        obj.ReadFromView(this);
        return obj;
    }

    public T[] ReadArray<T>() where T : unmanaged => ReadArray<T>(LengthPrefix);

    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, get amount of elements from prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    public T[] ReadArray<T>(LengthPrefix lengthPrefix) where T : unmanaged
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

    /// <summary>
    /// Reads data from the underlying stream into the specified array.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The destination array where data will be read into.</param>
    /// <param name="count">The number of elements to read.</param>
    /// <param name="offset">The index in the array at which to start writing.</param>
    /// <remarks>
    /// This method reads data from an unmanaged memory stream into the specified array. It is important to ensure that the destination array is large enough to hold the specified number of elements starting at the given offset.
    /// </remarks>
    public void ReadToArray<T>(T[] array, int count, int offset = 0) where T : unmanaged
    {
        for (int i = 0; i < count; i++) array[offset + i] = Read<T>();
    }

    public void ReadToIList<T>(IList<T> list) where T : unmanaged => ReadToIList(list, LengthPrefix);

    public void ReadToIList<T>(IList<T> list, LengthPrefix lengthPrefix, int offset = 0) where T : unmanaged
    {
        long length = ReadLengthPrefix(lengthPrefix);
        ReadToIList(list, length, offset);
    }


    /// <summary>Reads a list of unmanaged structs from the stream and increases the stream position by the size of the list elements, get amount of elements from prefix.</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">List to write in</param>
    /// <param name="lengthPrefix"></param>
    /// <remarks></remarks>
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

    /// <summary>Reads a list of unmanaged structs from the stream and increases the stream position by the size of the list elements, reads no length prefix.</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">List to write in</param>
    /// <param name="offset">Offset in list</param>
    /// <param name="count">Amount of elements to read</param>
    public void ReadToIList<T>(IList<T> list, long count, int offset = 0) where T : unmanaged
    {
        int listCount = list.Count;
        for (int i = 0; i < count; i++)
        {
            int idx = offset + i;
            var item = Read<T>();
            if (idx >= listCount)
                list.Add(item);
            else
                list[idx] = item;
        }
    }

    public void ReadToICollection<T>(ICollection<T> collection) where T : unmanaged => ReadToICollection(collection, LengthPrefix);

    /// <summary>Reads a list of unmanaged structs from the stream and increases the stream position by the size of the list elements, get amount of elements from prefix.</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="collection">The destination collection where data will be added.</param>
    /// <param name="lengthPrefix">Specifies how the length of the data is represented.</param>
    public void ReadToICollection<T>(ICollection<T> collection, LengthPrefix lengthPrefix) where T : unmanaged
    {
        long length = ReadLengthPrefix(lengthPrefix);
        ReadToICollection(collection, length);
    }

    /// <summary>Reads a list of unmanaged structs from the stream and increases the stream position by the size of the list elements, reads no length prefix.</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="collection">The collection where the read items will be added.</param>
    /// <param name="count">The number of items to read from the source and add to the collection.</param>
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
    public unsafe void ReadRemainderToIList<T>(IList<T> dstList, int offset = 0) where T : unmanaged
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
    public bool ReadBoolean() => Read<bool>();

    /// <summary>Reads a char from the stream and increases the position by two bytes</summary>
    public char ReadChar() => Read<char>();

    /// <summary>Reads a byte from the stream and increases the position by one byte</summary>
    public byte ReadByte() => Read<byte>();

    /// <summary>Reads a sbyte from the stream and increases the position by one byte</summary>
    public sbyte ReadSByte() => Read<sbyte>();

    /// <summary>Reads a ushort from the stream and increases the position by two bytes</summary>
    public ushort ReadUInt16() => Read<ushort>();

    /// <summary>Reads a short from the stream and increases the position by two bytes</summary>
    public short ReadInt16() => Read<short>();

    /// <summary>Reads a uint from the stream and increases the position by four bytes</summary>
    public uint ReadUInt32() => Read<uint>();

    /// <summary>Reads a int from the stream and increases the position by four bytes</summary>
    public int ReadInt32() => Read<int>();

    /// <summary>Reads a ulong from the stream and increases the position by eight bytes</summary>
    public ulong ReadUInt64() => Read<ulong>();

    /// <summary>Reads a long from the stream and increases the position by eight bytes</summary>
    public long ReadInt64() => Read<long>();

#if NET5_0_OR_GREATER
    /// <summary>Reads a half from the stream and increases the position by two bytes</summary>
    public unsafe Half ReadHalf() => Read<Half>();
#endif

    /// <summary>Reads a float from the stream and increases the position by four bytes</summary>
    public float ReadSingle() => Read<float>();

    /// <summary>Reads a double from the stream and increases the position by eight bytes</summary>
    public double ReadDouble() => Read<double>();

    /// <summary>Reads a decimal from the stream and increases the position by sixteen bytes</summary>
    public decimal ReadDecimal() => Read<decimal>();

    public string ReadString() => ReadString(LengthPrefix, Encoding);

    public string ReadString(Encoding encoding) => ReadString(LengthPrefix, encoding);

    public string ReadString(LengthPrefix lengthPrefix) => ReadString(lengthPrefix, Encoding);

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
    public string[] ReadStringArray()
    {
        var encoding = Encoding;
        long length = ReadLengthPrefix();
        string[] retData = new string[length];
        for (int i = 0; i < retData.Length; i++) retData[i] = ReadString(encoding);
        return retData;
    }

    private long ReadCustomLengthPrefix()
    {
        if (CustomLengthPrefixHandler == null)
            throw new InvalidOperationException("CustomLengthPrefixHandler is not set.");

        CustomLengthPrefixHandler.ReadFromView(this);
        return CustomLengthPrefixHandler.Length;
    }

    public long ReadLengthPrefix() => ReadLengthPrefix(LengthPrefix);

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
            LengthPrefix.Custom => ReadCustomLengthPrefix(),
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
                BufferSize = 0;
            }

            DisposedValue = true;
        }
    }
#endregion


}

