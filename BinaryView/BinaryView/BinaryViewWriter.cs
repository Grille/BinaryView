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
    private Stream baseStream;
    private Stream writeStream;
    private Stream deflateInputStream;

    private bool ownStream = true;
    private bool deflateAllMode = false;

    private LengthPrefix _lengthPrefix = LengthPrefix.UInt32;
    public LengthPrefix DefaultLengthPrefix { 
        get => _lengthPrefix; 
        set {
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

    private CompressionLevel deflateLevel;

    private BinaryFormatter formatter = new BinaryFormatter();

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
    public BinaryViewWriter()
    {
        baseStream = new MemoryStream();
        writeStream = baseStream;
    }
    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryViewWriter(string path)
    {
        baseStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        writeStream = baseStream;
    }

    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryViewWriter(Stream stream)
    {
        baseStream = stream;
        writeStream = baseStream;
        ownStream = false;
    }


    #region write

    /// <summary>Writes a primitive or unmanaged struct to the stream and increases the position by the size of the struct</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="obj">Struct to write</param>
    public unsafe void Write<T>(T obj) where T : unmanaged
    {
        int size = sizeof(T);
        var ptr = new IntPtr(&obj);
        if (size == 1) WriteByte(Marshal.ReadByte(ptr, 0));
        else for (int i = 0; i < size; i++) WriteByte(Marshal.ReadByte(ptr, i));
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

    /// <summary>Writes a char to the stream and increases the position by two bytes</summary>
    public void WriteChar(char input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(char));

    /// <summary>Writes a byte to the stream and increases the position by one byte</summary>
    public void WriteByte(byte input) => writeStream.WriteByte(input);

    /// <summary>Writes a sbyte to the stream and increases the position by one byte</summary>
    public void WriteSByte(sbyte input) => writeStream.WriteByte((byte)input);

    /// <summary>Writes a ushort to the stream and increases the position by two bytes</summary>
    public void WriteUInt16(ushort input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(ushort));

    /// <summary>Writes a short to the stream and increases the position by two bytes</summary>
    public void WriteInt16(short input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(short));

    /// <summary>Writes a uint to the stream and increases the position by four bytes</summary>
    public void WriteUInt32(uint input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(uint));

    /// <summary>Writes a int to the stream and increases the position by four bytes</summary>
    public void WriteInt32(int input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(int));

    /// <summary>Writes a ulong to the stream and increases the position by eight bytes</summary>
    public void WriteUInt64(ulong input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(ulong));

    /// <summary>Writes a long to the stream and increases the position by eight bytes</summary>
    public void WriteInt64(long input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(long));

    /// <summary>Writes a float to the stream and increases the position by four bytes</summary>
    public void WriteSingle(float input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(float));

    /// <summary>Writes a double to the stream and increases the position by eight byte</summary>
    public void WriteDouble(double input) => writeStream.Write(BitConverter.GetBytes(input), 0, sizeof(double));

    /// <summary>Writes a decimal to the stream and increases the position by sixteen bytes</summary>
    public void WriteDecimal(decimal input) => Write(input);

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
        if (deflateAllMode)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var compressor = new DeflateStream(compressedStream, (SysIOC.CompressionLevel)deflateLevel, true))
                {
                    writeStream.Seek(0, SeekOrigin.Begin);
                    writeStream.CopyTo(compressor);
                }
                compressedStream.Seek(0, SeekOrigin.Begin);
                compressedStream.CopyTo(baseStream);
            }
            writeStream.Dispose();
            writeStream = baseStream;
        }

        baseStream.Close();
    }

    /// <summary>Save data as binary file to the specified path</summary>
    public void Save(string path)
    {
        var fs = File.Open(path, FileMode.Create, FileAccess.Write);
        baseStream.CopyTo(fs);
        fs.Dispose();
    }

    public byte[] ToArray()
    {
        return ((MemoryStream)baseStream).ToArray();
    }

    /// <summary>Compress all data with DeflateStream</summary>
    public void CompressAll(CompressionLevel level = CompressionLevel.Optimal)
    {
        writeStream = new MemoryStream();
        deflateLevel = level;
        deflateAllMode = true;
    }

    public void BeginDeflateSection(CompressionLevel level = CompressionLevel.Optimal)
    {
        deflateLevel = level;
        deflateInputStream = new MemoryStream();
        writeStream = deflateInputStream;
    }

    public void EndDeflateSection()
    {
        using (var compressedStream = new MemoryStream())
        {
            using (var compressor = new DeflateStream(compressedStream, (SysIOC.CompressionLevel)deflateLevel, true))
            {
                writeStream.Seek(0, SeekOrigin.Begin);
                writeStream.CopyTo(compressor);
            }
            writeStream.Dispose();
            writeStream = baseStream;
            WriteInt64(compressedStream.Length);
            compressedStream.Seek(0, SeekOrigin.Begin);
            compressedStream.CopyTo(baseStream);
        }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Close();
            }
            if (ownStream)
                baseStream.Dispose();

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

