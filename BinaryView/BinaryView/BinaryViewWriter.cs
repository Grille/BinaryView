using System;
using System.IO;
using System.IO.Compression;
using SysIOC = System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GGL.IO;

public class BinaryViewWriter : IDisposable
{
    record struct DflateSectionInfo(CompressionLevel Level, LengthPrefix LengthPrefix);

    private byte[] writeBuffer = new byte[Utils.DefaultBufferSize];
    private Stream writeStream;

    private bool deflateAllMode = false;

    private int _bufferSize = Utils.DefaultBufferSize;
    private LengthPrefix _lengthPrefix = LengthPrefix.UInt32;
    private CharSizePrefix _charSizePrefix = CharSizePrefix.Char;
    private Endianness _bitOrder = Endianness.Default;
    private Endianness _byteOrder = Endianness.Default;
    private bool needBitReorder = false;
    private bool needByteReorder = false;
    private bool needReorder = false;

    private BinaryFormatter formatter = new BinaryFormatter();


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
        set
        {
            _bufferSize = value;
            writeBuffer = new byte[_bufferSize];
        }
    }

    public LengthPrefix DefaultLengthPrefix { 
        get => _lengthPrefix; 
        set {
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
        get => writeStream.Position;
        set => writeStream.Position = value;
    }
    public long Length
    {
        get => writeStream.Length;
        set => writeStream.SetLength(value);
    }


    /// <summary>Initialize BinaryView with a empty MemoryStream</summary>
    public BinaryViewWriter() :
        this(new StreamStack(new MemoryStream(), true))
    { }
    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryViewWriter(string path) :
        this(new StreamStack(new FileStream(path, FileMode.Create, FileAccess.Write), true))
    { }

    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryViewWriter(Stream stream, bool closeStream = false) :
        this(new StreamStack(stream, closeStream))
    { }

    public BinaryViewWriter(StreamStack stack)
    {
        StreamStack = stack;
        StreamStack.StackChanged += (object sender, StreamStackEntry e) =>
        {
            writeStream = e.Stream;
        };
        writeStream = StreamStack.Peek().Stream;
    }

    #region write

    /// <summary>Writes a primitive or unmanaged struct to the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="obj">Struct to write</param>
    public unsafe void Write<T>(T obj) where T : unmanaged
    {

        int size = sizeof(T);
        var ptr = (byte*)&obj;
        switch (size)
        {
            case 1:
            {
                if (needBitReorder)
                    *ptr = Utils.BitReverseTable[*ptr];

                writeStream.WriteByte(*ptr);
                return;
            }
            default:
            {
                if (needReorder)
                    Utils.ReverseObjBits(&obj, needByteReorder, needBitReorder);

                for (int i = 0; i < size; i++)
                    writeBuffer[i] = *(ptr + i);

                writeStream.Write(writeBuffer, 0, size);
                return;
            }
        }
    }

    /// <summary>Writes any object to the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="obj">Object to write</param>
    /// <remarks>WARNING Serialize can be very inefficient, use Write() instead when possible!</remarks>
    public void Serialize<T>(T obj)
    {
        formatter.Serialize(writeStream, obj);
    }

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="array">Array of unmanaged structs to write</param>
    public void WriteArray<T>(T[] array, LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged => WriteIList(array, 0, array.Length, lengthPrefix);

    /// <summary>Writes a list of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">Array of unmanaged structs to write</param>
    public void WriteIList<T>(IList<T> list, LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged => WriteIList(list, 0, list.Count, lengthPrefix);

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">List of unmanaged structs to write</param>
    /// <param name="offset">start offset in the array</param>
    /// <param name="count">number of elements to be written</param>
    public void WriteIList<T>(IList<T> list, int offset, int count, LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged
    {
        writeLengthPrefix(lengthPrefix, count);
        for (int i = 0; i < count; i++) Write(list[i + offset]);
    }

    /// <summary>Writes a bool to the stream and increases the position by one byte</summary>
    public unsafe void WriteBoolean(bool input) => Write(input);

    /// <summary>Writes a char to the stream and increases the position by two bytes</summary>
    public unsafe void WriteChar(char input) => Write(input);

    /// <summary>Writes a byte to the stream and increases the position by one byte</summary>
    public unsafe void WriteByte(byte input) => Write(input);

    /// <summary>Writes a sbyte to the stream and increases the position by one byte</summary>
    public unsafe void WriteSByte(sbyte input) => Write(input);

    /// <summary>Writes a ushort to the stream and increases the position by two bytes</summary>
    public unsafe void WriteUInt16(ushort input) => Write(input);

    /// <summary>Writes a short to the stream and increases the position by two bytes</summary>
    public unsafe void WriteInt16(short input) => Write(input);

    /// <summary>Writes a uint to the stream and increases the position by four bytes</summary>
    public unsafe void WriteUInt32(uint input) => Write(input);

    /// <summary>Writes a int to the stream and increases the position by four bytes</summary>
    public unsafe void WriteInt32(int input) => Write(input);

    /// <summary>Writes a ulong to the stream and increases the position by eight bytes</summary>
    public unsafe void WriteUInt64(ulong input) => Write(input);

    /// <summary>Writes a long to the stream and increases the position by eight bytes</summary>
    public unsafe void WriteInt64(long input) => Write(input);

    /// <summary>Writes a float to the stream and increases the position by four bytes</summary>
    public unsafe void WriteSingle(float input) => Write(input);

    /// <summary>Writes a double to the stream and increases the position by eight byte</summary>
    public unsafe void WriteDouble(double input) => Write(input);

    /// <summary>Writes a decimal to the stream and increases the position by sixteen bytes</summary>
    public unsafe void WriteDecimal(decimal input) => Write(input);

    /// <summary>Writes a string as char array to the stream</summary>
    public void WriteString(string input, LengthPrefix lengthPrefix = LengthPrefix.Default, CharSizePrefix charSizePrefix = CharSizePrefix.Default)
    {
        writeLengthPrefix(lengthPrefix, input.Length);

        if (charSizePrefix == CharSizePrefix.Default)
            charSizePrefix = DefaultCharSizePrefix;

        if (charSizePrefix == CharSizePrefix.Char)
            for (int i = 0; i < input.Length; i++)
                WriteChar((char)input[i]);
        else
            for (int i = 0; i < input.Length; i++)
                WriteByte((byte)input[i]);
    }

    /// <summary>Writes a array of strings</summary>
    public void WriteStringArray(string[] input)
    {
        writeLengthPrefix(LengthPrefix.UInt32, input.Length);
        for (int i = 0; i < input.Length; i++) WriteString(input[i], LengthPrefix.UInt32, CharSizePrefix.Char);
    }

    internal void writeLengthPrefix(LengthPrefix lengthPrefix, long length)
    {
        switch (lengthPrefix)
        {
            case LengthPrefix.None:
                return;
            case LengthPrefix.Default:
                writeLengthPrefix(DefaultLengthPrefix, length);
                return;
            case LengthPrefix.SByte:
                WriteSByte((sbyte)length);
                return;
            case LengthPrefix.Byte:
                WriteByte((byte)length);
                return;
            case LengthPrefix.Int16:
                WriteInt16((short)length);
                return;
            case LengthPrefix.UInt16:
                WriteUInt16((ushort)length);
                return;
            case LengthPrefix.Int32:
                WriteInt32((int)length);
                return;
            case LengthPrefix.UInt32:
                WriteUInt32((uint)length);
                return;
            case LengthPrefix.Int64:
                WriteInt64((long)length);
                return;
            case LengthPrefix.UInt64:
                WriteUInt64((ulong)length);
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    public void Close()
    {
        while (StreamStack.Count > 0)
            StreamStack.Pop();
    }

    /// <summary>Save data as binary file to the specified path</summary>
    public void Save(string path)
    {
        var fs = File.Open(path, FileMode.Create, FileAccess.Write);
        writeStream.CopyTo(fs);
        fs.Dispose();
    }

    public byte[] ToArray()
    {
        return ((MemoryStream)writeStream).ToArray();
    }

    /// <summary>Compress all data with DeflateStream</summary>
    public void CompressAll(CompressionLevel level = CompressionLevel.Optimal)
    {
        deflateAllMode = true;
        BeginDeflateSection(level, LengthPrefix.None);
    }

    public void BeginDeflateSection(CompressionLevel level = CompressionLevel.Optimal, LengthPrefix lengthPrefix = LengthPrefix.Default)
    {
        StreamStack.Create(new DflateSectionInfo(level, lengthPrefix));
    }

    public void EndDeflateSection()
    {
        using (var compressedStream = new MemoryStream())
        {
            var peakStream = StreamStack.Pop();
            var args = (DflateSectionInfo)peakStream.Args;
            using (var compressor = new DeflateStream(compressedStream, (SysIOC.CompressionLevel)args.Level, true))
            {
                peakStream.Stream.Seek(0, SeekOrigin.Begin);
                peakStream.Stream.CopyTo(compressor);
            }
            peakStream.Dispose();
            compressedStream.Seek(0, SeekOrigin.Begin);
            writeLengthPrefix(args.LengthPrefix, compressedStream.Length);
            StreamStack.CopyToPeak(compressedStream, false);
        }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (deflateAllMode)
                EndDeflateSection();

            StreamStack.Dispose();

            disposedValue = true;
        }
    }

    ~BinaryViewWriter()
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

