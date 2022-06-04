using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.CompilerServices;

namespace GGL.IO;

public class BinaryViewReader : IDisposable
{

    private byte[] readBuffer = new byte[Utils.DefaultBufferSize];
    private Stream readStream;

    private BinaryFormatter formatter = new BinaryFormatter();

    private int _bufferSize = Utils.DefaultBufferSize;
    private LengthPrefix _lengthPrefix = LengthPrefix.UInt32;
    private CharSizePrefix _charSizePrefix = CharSizePrefix.Char;
    private Endianness _bitOrder = Endianness.Default;
    private Endianness _byteOrder = Endianness.Default;
    private bool needBitReorder = false;
    private bool needByteReorder = false;
    private bool needReorder = false;

    public readonly StreamStack StreamStack;
    public Endianness BitOrder
    {
        get => _bitOrder;
        set
        {
            _bitOrder = value;
            needBitReorder = _bitOrder != Endianness.Default;
            needReorder = _bitOrder != Endianness.Default || _byteOrder != Endianness.Default;
        }
    }
    public Endianness ByteOrder
    {
        get => _bitOrder;
        set
        {
            _byteOrder = value;
            needByteReorder = _byteOrder != Endianness.Default;
            needReorder = _bitOrder != Endianness.Default || _byteOrder != Endianness.Default;
        }
    }

    public int BufferSize
    {
        get => _bufferSize;
        set {
            _bufferSize = value;
            readBuffer = new byte[_bufferSize];
        }
    }

    public LengthPrefix DefaultLengthPrefix
    {
        get => _lengthPrefix;
        set
        {
            if (value == LengthPrefix.Default)
                throw new ArgumentException("DefaultLengthPrefix can't be set to Default!");
            _lengthPrefix = value;
        }
    }

    public CharSizePrefix DefaultCharSizePrefix
    {
        get => _charSizePrefix;
        set
        {
            if (value == CharSizePrefix.Default)
                throw new ArgumentException("DefaultCharSizePrefix can't be set to Default!");
            _charSizePrefix = value;
        }
    }

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


    /// <summary>Initialize BinaryView with a empty MemoryStream</summary>
    public BinaryViewReader() : 
        this(new StreamStack(new MemoryStream(),true)) 
    { }

    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryViewReader(string path) : 
        this(new StreamStack(new FileStream(path, FileMode.Open, FileAccess.Read), true)) 
    { }

    /// <summary>Initialize BinaryView with a MemoryStream filled with bytes from array</summary>
    /// <param name="bytes">Base array</param>
    public BinaryViewReader(byte[] bytes) : 
        this(new StreamStack(new MemoryStream(bytes), true)) 
    { }

    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryViewReader(Stream stream, bool closeStream = false) :
        this(new StreamStack(stream, closeStream))
    { }

    public BinaryViewReader(StreamStack stack)
    {
        StreamStack = stack;
        StreamStack.StackChanged += (object sender, StreamStackEntry e) =>
        {
            readStream = e.Stream;
        };
        readStream = StreamStack.Peek().Stream;
    }


    #region read
    /// <summary>Reads a primitive or unmanaged struct from the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    public unsafe T Read<T>() where T : unmanaged
    {
        int size = sizeof(T);

        switch (size)
        {
            case 1:
            {
                byte data = (byte)readStream.ReadByte();

                if (needBitReorder)
                    data = Utils.BitReverseTable[data];

                var obj = *(T*)&data;
                return obj;
            }
            default:
            {
                readStream.Read(readBuffer, 0, size);

                fixed (byte* dataPtr = readBuffer)
                {
                    var obj = *(T*)&dataPtr[0];

                    if (needReorder)
                        Utils.ReverseObjBits(&obj, needByteReorder, needBitReorder);

                    return obj;
                }
            }
        }
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

    /// <summary>Reads a float from the stream and increases the position by four bytes</summary>
    public unsafe float ReadSingle() => Read<float>();

    /// <summary>Reads a double from the stream and increases the position by eight bytes</summary>
    public unsafe double ReadDouble() => Read<double>();

    /// <summary>Reads a decimal from the stream and increases the position by sixteen bytes</summary>
    public unsafe decimal ReadDecimal() => Read<decimal>();


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


    public long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        return readStream.Seek(offset, origin);
    }

    public long Exch(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        long pos = readStream.Position;
        readStream.Seek(offset, origin);
        return pos;
    }


    /// <summary>Decompress all data with DeflateStream, must be executet before any read operation</summary>
    public void DecompressAll()
    {
        BeginDeflateSection(readStream.Length);
    }
    /// <summary>Decompress data with DeflateStream, position will reset</summary>
    public void BeginDeflateSection(LengthPrefix lengthPrefix = LengthPrefix.Default)
    {
        long length = readLengthPrefix(lengthPrefix);
        BeginDeflateSection(length);
    }

    public void BeginDeflateSection(long length)
    {
        using (var compressedSection = StreamStack.GetSubStream(length))
        {
            StreamStack.Create();
            using (var decompressStream = new DeflateStream(compressedSection, CompressionMode.Decompress, true))
            {
                StreamStack.CopyToPeak(decompressStream, true);
            }
        }
    }

    public void EndDeflateSection()
    {
        StreamStack.DisposePeak();
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            StreamStack.Dispose();

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

