using System;
using System.IO;
using System.IO.Compression;
using SysIOC = System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;

namespace GGL.IO;

public class BinaryViewWriter : StreamStackUser
{
    record struct DflateSectionInfo(CompressionLevel Level, LengthPrefix LengthPrefix);

    private byte[] writeBuffer = new byte[EndianUtils.DefaultBufferSize];

    private bool deflateAllMode = false;

    private int _bufferSize = EndianUtils.DefaultBufferSize;
    private LengthPrefix _lengthPrefix = LengthPrefix.UInt32;
    private CharSize _charSizePrefix = CharSize.Char;
    private Endianness _bitOrder = Endianness.Default;
    private Endianness _byteOrder = Endianness.Default;
    private bool needBitReorder = false;
    private bool needByteReorder = false;
    private bool needReorder = false;

    public IFormatter Formatter = new BinaryFormatter();

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

    public CharSize DefaultCharSize
    {
        get => _charSizePrefix;
        set
        {
            if (value == CharSize.Default)
                throw new ArgumentException("DefaultCharSize can't be set to Default!");
            _charSizePrefix = value;
        }
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

    public BinaryViewWriter(byte[] bytes):
        this(new StreamStack(new MemoryStream(bytes), true))
    { }

    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryViewWriter(Stream stream, bool closeStream = false) :
        this(new StreamStack(stream, closeStream))
    { }

    public BinaryViewWriter(StreamStack stack) : 
        base(stack) 
    { }

    #region write

    /// <summary>Writes a primitive or unmanaged struct to the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="obj">Struct to write</param>
    public unsafe void Write<T>(T obj) where T : unmanaged
    {
        int size = sizeof(T);
        var ptr = (byte*)&obj;
        WriteFromPtr(ptr, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteFromPtr(byte* ptr, int size)
    {
        switch (size)
        {
            case 1:
            {
                byte value = *(ptr);
                if (needBitReorder)
                    value = EndianUtils.BitReverseTable[value];

                PeakStream.WriteByte(value);
                return;
            }
            default:
            {
                if (needReorder)
                    EndianUtils.ReverseObjBits(ptr, size, needByteReorder, needBitReorder);

                for (int i = 0; i < size; i++)
                    writeBuffer[i] = *(ptr + i);

                PeakStream.Write(writeBuffer, 0, size);
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIView<T>(T obj) where T : IViewWritable
    {
        obj.WriteToView(this);
    }

    /// <summary>Writes any object to the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="obj">Object to write</param>
    /// <remarks>WARNING Serialize can be very inefficient, use Write() instead when possible!</remarks>
    public void Serialize<T>(T obj)
    {
        Formatter.Serialize(PeakStream, obj);
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
    public void WriteString(string input, LengthPrefix lengthPrefix = LengthPrefix.Default, CharSize charSize = CharSize.Default)
    {
        WriteString(input, input.Length, lengthPrefix, charSize);
    }

    public void WriteString(string input, long length, LengthPrefix lengthPrefix = LengthPrefix.Default, CharSize charSize = CharSize.Default)
    {
        writeLengthPrefix(lengthPrefix, input.Length);

        if (charSize == CharSize.Default)
            charSize = DefaultCharSize;

        if (charSize == CharSize.Char)
            for (int i = 0; i < length; i++)
                WriteChar((char)input[i]);
        else
            for (int i = 0; i < length; i++)
                WriteByte((byte)input[i]);
    }

    /// <summary>Writes a array of strings</summary>
    public void WriteStringArray(string[] input)
    {
        writeLengthPrefix(LengthPrefix.UInt32, input.Length);
        for (int i = 0; i < input.Length; i++) WriteString(input[i], LengthPrefix.UInt32, CharSize.Char);
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

    /// <summary>Save data as binary file to the specified path</summary>
    public void Save(string path)
    {
        var fs = File.Open(path, FileMode.Create, FileAccess.Write);
        PeakStream.CopyTo(fs);
        fs.Dispose();
    }

    public byte[] ToArray()
    {
        return ((MemoryStream)PeakStream).ToArray();
    }

    /// <summary>Compress all data with DeflateStream</summary>
    public void CompressAll(CompressionLevel level = CompressionLevel.Optimal)
    {
        deflateAllMode = true;
        BeginDeflateSection(level, LengthPrefix.None);
    }

    public void BeginInsert()
    {
        StreamStack.Create();
    }

    public void EndInsert()
    {
        var peak = StreamStack.Pop();
        var stream = peak.Stream;
        stream.Seek(0, SeekOrigin.Begin);
        StreamStack.InsertToPeak(stream);
        peak.Dispose();
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
    protected override void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (deflateAllMode)
                EndDeflateSection();

            StreamStack.Dispose();

            DisposedValue = true;
        }
    }
    #endregion


}

