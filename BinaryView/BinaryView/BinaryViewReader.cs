using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace GGL.IO;

public class BinaryViewReader : IDisposable
{

    private byte[] readBuffer = new byte[16];
    private Stream baseStream;
    private Stream readStream;

    private Stack<(Stream, object)> streamStack = new();

    private bool closeStream = true;
    private BinaryFormatter formatter = new BinaryFormatter();

    public long Position
    {
        get => readStream.Position;
        set => readStream.Position = value;
    }
    public long Length
    {
        get => readStream.Length;
        set => readStream.SetLength(value);
    }

    private LengthPrefix _lengthPrefix = LengthPrefix.UInt32;
    public LengthPrefix DefaultLengthPrefix
    {
        get => _lengthPrefix;
        set
        {
            if (value == LengthPrefix.Default)
                throw new InvalidOperationException("DefaultLengthPrefix can't set to Default!");
            _lengthPrefix = value;
        }
    }

    private CharSizePrefix _charSizePrefix = CharSizePrefix.Char;
    public CharSizePrefix DefaultCharSizePrefix
    {
        get => _charSizePrefix;
        set
        {
            if (value == CharSizePrefix.Default)
                throw new InvalidOperationException("DefaultCharSizePrefix can't set to Default!");
            _charSizePrefix = value;
        }
    }

    /// <summary>Initialize BinaryView with a empty MemoryStream</summary>
    public BinaryViewReader()
    {
        baseStream = new MemoryStream();
        PushStream(baseStream);
    }
    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryViewReader(string path)
    {
        baseStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        PushStream(baseStream);
    }
    /// <summary>Initialize BinaryView with a MemoryStream filled with bytes from array</summary>
    /// <param name="bytes">Base array</param>
    public BinaryViewReader(byte[] bytes)
    {
        baseStream = new MemoryStream(bytes);
        PushStream(baseStream);
    }
    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryViewReader(Stream stream, bool closeStream = false)
    {
        baseStream = stream;
        PushStream(baseStream);
        this.closeStream = closeStream;
    }

    #region read
    /// <summary>Reads a primitive or unmanaged struct from the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    public unsafe T Read<T>() where T : unmanaged
    {
        int size = sizeof(T);
        var obj = new T();
        var ptr = new IntPtr(&obj);
        for (int i = 0; i < size; i++) Marshal.WriteByte(ptr, i, ReadByte());
        return obj;
    }

    /// <summary>Reads an serialized object from the stream and increases the position by the size of the data</summary>
    /// <typeparam name="T"></typeparam> Type
    public T Deserialize<T>()
    {
        return (T)formatter.Deserialize(readStream);
    }


    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, get amount of elements from prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    public unsafe T[] ReadArray<T>(LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged
    {
        long length = readLengthPrefix(lengthPrefix);
        T[] array = new T[length];
        for (int i = 0; i < array.Length; i++) array[i] = Read<T>();
        return array;
    }

    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, get amount of elements from prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">Pointer to existing list to write in</param>
    /// <param name="offset">Offset in array</param>
    public unsafe void ReadToIList<T>(IList<T> list, int offset = 0, LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged
    {
        long length = readLengthPrefix(lengthPrefix);
        ReadToIList(list, offset, length);
    }

    /// <summary>Reads a list of unmanaged structs from the stream and increases the position by the size of the array elements, read no length prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="dstList">Pointer to existing list to write in</param>
    /// <param name="offset">Offset in dstList</param>
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

    /// <summary>Reads a list of unmanaged structs from the stream and increases the position by the size of the array elements, read no length prefix</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="dstList">Pointer to existing list to write in</param>
    /// <param name="offset">Offset in dstList</param>
    public unsafe void ReadRemainderToIList<T>(IList<T> dstList, int offset) where T : unmanaged
    {
        long count = (readStream.Length - readStream.Position) / sizeof(T);
        ReadToIList(dstList, offset, count);
    }

    /// <summary>Reads a bool from the stream and increases the position by one byte</summary>
    public bool ReadBoolean()
    {
        readStream.Read(readBuffer, 0, sizeof(bool));
        return BitConverter.ToBoolean(readBuffer, 0);
    }

    /// <summary>Reads a char from the stream and increases the position by two bytes</summary>
    public char ReadChar()
    {
        readStream.Read(readBuffer, 0, sizeof(char));
        return BitConverter.ToChar(readBuffer, 0);
    }

    /// <summary>Reads a byte from the stream and increases the position by one byte</summary>
    public byte ReadByte() => (byte)readStream.ReadByte();

    /// <summary>Reads a sbyte from the stream and increases the position by one byte</summary>
    public sbyte ReadSByte() => (sbyte)readStream.ReadByte();

    /// <summary>Reads a ushort from the stream and increases the position by two bytes</summary>
    public ushort ReadUInt16()
    {
        readStream.Read(readBuffer, 0, sizeof(ushort));
        return BitConverter.ToUInt16(readBuffer, 0);
    }

    /// <summary>Reads a short from the stream and increases the position by two bytes</summary>
    public short ReadInt16()
    {
        readStream.Read(readBuffer, 0, sizeof(short));
        return BitConverter.ToInt16(readBuffer, 0);
    }

    /// <summary>Reads a uint from the stream and increases the position by four bytes</summary>
    public uint ReadUInt32()
    {
        readStream.Read(readBuffer, 0, sizeof(uint));
        return BitConverter.ToUInt32(readBuffer, 0);
    }

    /// <summary>Reads a int from the stream and increases the position by four bytes</summary>
    public int ReadInt32()
    {
        readStream.Read(readBuffer, 0, sizeof(int));
        return BitConverter.ToInt32(readBuffer, 0);
    }

    /// <summary>Reads a ulong from the stream and increases the position by eight bytes</summary>
    public ulong ReadUInt64()
    {
        readStream.Read(readBuffer, 0, sizeof(ulong));
        return BitConverter.ToUInt64(readBuffer, 0);
    }

    /// <summary>Reads a long from the stream and increases the position by eight bytes</summary>
    public long ReadInt64()
    {
        readStream.Read(readBuffer, 0, sizeof(long));
        return BitConverter.ToInt64(readBuffer, 0);
    }

    /// <summary>Reads a float from the stream and increases the position by four bytes</summary>
    public float ReadSingle()
    {
        readStream.Read(readBuffer, 0, sizeof(float));
        return BitConverter.ToSingle(readBuffer, 0);
    }

    /// <summary>Reads a double from the stream and increases the position by eight bytes</summary>
    public double ReadDouble()
    {
        readStream.Read(readBuffer, 0, sizeof(double));
        return BitConverter.ToDouble(readBuffer, 0);
    }

    /// <summary>Reads a decimal from the stream and increases the position by sixteen bytes</summary>
    public decimal ReadDecimal()
    {
        return Read<decimal>();
    }

    /// <summary>Reads a string from the stream</summary>
    public string ReadString(LengthPrefix lengthPrefix = LengthPrefix.Default, CharSizePrefix charSizePrefix = CharSizePrefix.Default)
    {
        long length = readLengthPrefix(lengthPrefix);
        if (charSizePrefix == CharSizePrefix.Default)
            charSizePrefix = DefaultCharSizePrefix;

        char[] retData = new char[length];
        if (charSizePrefix == CharSizePrefix.Char)
            for (int i = 0; i < retData.Length; i++)
                retData[i] = (char)ReadChar();
        else
            for (int i = 0; i < retData.Length; i++)
                retData[i] = (char)ReadByte();

        return new string(retData);
    }
    /// <summary>Reads a array of string from the stream</summary>
    public string[] ReadStringArray()
    {
        long length = readLengthPrefix(LengthPrefix.UInt32);
        string[] retData = new string[length];
        for (int i = 0; i < retData.Length; i++) retData[i] = ReadString(LengthPrefix.UInt32, CharSizePrefix.Char);
        return retData;
    }

    internal long readLengthPrefix(LengthPrefix lengthPrefix)
    {
        switch (lengthPrefix)
        {
            case LengthPrefix.Default:
                return readLengthPrefix(DefaultLengthPrefix);
            case LengthPrefix.SByte:
                return ReadSByte();
            case LengthPrefix.Byte:
                return ReadByte();
            case LengthPrefix.Int16:
                return ReadInt16();
            case LengthPrefix.UInt16:
                return ReadUInt16();
            case LengthPrefix.Int32:
                return ReadInt32();
            case LengthPrefix.UInt32:
                return ReadUInt32();
            case LengthPrefix.Int64:
                return ReadInt64();
            case LengthPrefix.UInt64:
                return (long)ReadUInt64();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    /// <summary>Decompress all data with DeflateStream, must be executet before any read operation</summary>
    public void DecompressAll()
    {
        var decompressedStream = new MemoryStream();

        using (var decompressor = new DeflateStream(baseStream, CompressionMode.Decompress, true))
        {
            //readStream.Seek(0, SeekOrigin.Begin);
            decompressor.CopyTo(decompressedStream);
        }
        decompressedStream.Seek(0, SeekOrigin.Begin);

        // replace compressed baseStream with decompressedStream 
        baseStream.Dispose();
        baseStream = decompressedStream;
        readStream = baseStream;
    }
    /// <summary>Decompress data with DeflateStream, position will reset</summary>
    public void BeginDeflateSection()
    {
        var DecompressedStream = new MemoryStream();

        long length = ReadInt64();
        using (var compressedSection = ReadStream(length))
        {
            using (var decompressStream = new DeflateStream(compressedSection, CompressionMode.Decompress, true))
            {
                decompressStream.CopyTo(DecompressedStream);
            }
        }
        DecompressedStream.Seek(0, SeekOrigin.Begin);
        PushStream(DecompressedStream);
    }

    public void EndDeflateSection()
    {
        CloseStream();
    }

    public void PushStream(Stream stream, object args = null)
    {
        streamStack.Push((stream, args));
        readStream = stream;
    }

    public (Stream stream, object args) PeekStream()
    {
        return streamStack.Peek();
    }

    public void CloseStream()
    {
        (var stream, _) = streamStack.Pop();
        stream.Dispose();
        (readStream, _) = PeekStream();
    }

    public Stream ReadStream(long length)
    {
        var stream = new SubStream(readStream, readStream.Position, length);
        return stream;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            baseStream.Dispose();

            disposedValue = true;
        }
    }

    ~BinaryViewReader()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion


}

