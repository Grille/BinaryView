using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Grille.IO;

public class BinaryView : IDisposable
{

    private byte[] readBuffer = new byte[16];
    public Stream BaseStream { private set; get; }
    private BinaryFormatter formatter = new BinaryFormatter();

    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }
    public long Length
    {
        get => BaseStream.Length;
        set => BaseStream.SetLength(value);
    }

    /// <summary>Initialize BinaryView with a empty MemoryStream</summary>
    public BinaryView()
    {
        BaseStream = new MemoryStream();
    }
    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    /// <param name="useCopy">Use a memory copy to avoid changes to the file</param>
    public BinaryView(string path, bool useCopy = true)
    {
        if (useCopy)
        {
            BaseStream = new MemoryStream();
            using (var fileStream = new FileStream(path, FileMode.OpenOrCreate))
            {
                fileStream.CopyTo(BaseStream);
            }
        }
        else
        {
            BaseStream = new FileStream(path, FileMode.OpenOrCreate);
        }
    }
    /// <summary>Initialize BinaryView with a MemoryStream filled with bytes from array</summary>
    /// <param name="bytes">Base array</param>
    /// <param name="useCopy">Use a memory copy to avoid changes to the array</param>
    public BinaryView(byte[] bytes, bool useCopy = true)
    {
        if (useCopy)
        {
            BaseStream = new MemoryStream(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                WriteByte(bytes[i]);
            }
        }
        else
        {
            BaseStream = new MemoryStream(bytes);
        }
    }
    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryView(Stream stream)
    {
        this.BaseStream = stream;
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
        formatter.Serialize(BaseStream, obj);
    }

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="array">Array of unmanaged structs to write</param>
    public void WriteArray<T>(T[] array) where T : unmanaged => WriteArray(array, 0, array.Length);

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="array">Array of unmanaged structs to write</param>
    /// <param name="offset">start offset in the array</param>
    /// <param name="count">number of elements to be written</param>
    public void WriteArray<T>(T[] array, int offset, int count) where T : unmanaged
    {
        WriteInt32(count);
        for (int i = 0; i < count; i++) Write(array[i + offset]);
    }

    /// <summary>Writes a char to the stream and increases the position by two bytes</summary>
    public void WriteChar(char input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(char));

    /// <summary>Writes a byte to the stream and increases the position by one byte</summary>
    public void WriteByte(byte input) => BaseStream.WriteByte(input);

    /// <summary>Writes a sbyte to the stream and increases the position by one byte</summary>
    public void WriteSByte(sbyte input) => BaseStream.WriteByte((byte)input);

    /// <summary>Writes a ushort to the stream and increases the position by two bytes</summary>
    public void WriteUInt16(ushort input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(ushort));

    /// <summary>Writes a short to the stream and increases the position by two bytes</summary>
    public void WriteInt16(short input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(short));

    /// <summary>Writes a uint to the stream and increases the position by four bytes</summary>
    public void WriteUInt32(uint input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(uint));

    /// <summary>Writes a int to the stream and increases the position by four bytes</summary>
    public void WriteInt32(int input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(int));

    /// <summary>Writes a ulong to the stream and increases the position by eight bytes</summary>
    public void WriteUInt64(ulong input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(ulong));

    /// <summary>Writes a long to the stream and increases the position by eight bytes</summary>
    public void WriteInt64(long input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(long));

    /// <summary>Writes a float to the stream and increases the position by four bytes</summary>
    public void WriteSingle(float input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(float));

    /// <summary>Writes a double to the stream and increases the position by eight byte</summary>
    public void WriteDouble(double input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(double));

    /// <summary>Writes a decimal to the stream and increases the position by sixteen bytes</summary>
    public void WriteDecimal(decimal input) => Write(input);

    /// <summary>Writes a string as char array to the stream</summary>
    public void WriteString(string input)
    {
        char[] stringData = input.ToCharArray();

        int max = 0;
        for (int i = 0; i < stringData.Length; i++)
            if (stringData[i] > max) max = stringData[i];

        byte lengthSizeBit = 0;
        byte charSizeBit = 0;

        if (input.Length > byte.MaxValue)
            lengthSizeBit = 1;
        if (max > byte.MaxValue)
            charSizeBit = 1;

        byte meta = (byte)(lengthSizeBit << 0 | charSizeBit << 1);
        WriteByte(meta);


        if (lengthSizeBit == 1)
            WriteInt32((int)input.Length);
        else
            WriteByte((byte)input.Length);

        if (charSizeBit == 1)
            for (int i = 0; i < stringData.Length; i++)
                WriteChar((char)stringData[i]);
        else
            for (int i = 0; i < stringData.Length; i++)
                WriteByte((byte)stringData[i]);
    }

    /// <summary>Writes a array of strings</summary>
    public void WriteStringArray(string[] input)
    {
        WriteInt32((int)input.Length);
        for (int i = 0; i < input.Length; i++) WriteString(input[i]);
    }
    #endregion


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
        return (T)formatter.Deserialize(BaseStream);
    }


    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    public unsafe T[] ReadArray<T>() where T : unmanaged
    {
        int length = ReadInt32();
        T[] array = new T[length];
        for (int i = 0; i < array.Length; i++) array[i] = Read<T>();
        return array;
    }

    /// <summary>Reads a array of unmanaged structs from the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="array">Pointer to existing array to write in</param>
    /// <param name="offset">Offset in array</param>
    public unsafe void ReadArray<T>(T[] array, int offset = 0) where T : unmanaged
    {
        int length = ReadInt32();
        for (int i = 0; i < length; i++) array[i + offset] = Read<T>();
    }

    /// <summary>Reads a char from the stream and increases the position by two bytes</summary>
    public char ReadChar()
    {
        BaseStream.Read(readBuffer, 0, sizeof(char));
        return BitConverter.ToChar(readBuffer, 0);
    }

    /// <summary>Reads a byte from the stream and increases the position by one byte</summary>
    public byte ReadByte() => (byte)BaseStream.ReadByte();

    /// <summary>Reads a sbyte from the stream and increases the position by one byte</summary>
    public sbyte ReadSByte() => (sbyte)BaseStream.ReadByte();

    /// <summary>Reads a ushort from the stream and increases the position by two bytes</summary>
    public ushort ReadUInt16()
    {
        BaseStream.Read(readBuffer, 0, sizeof(ushort));
        return BitConverter.ToUInt16(readBuffer, 0);
    }

    /// <summary>Reads a short from the stream and increases the position by two bytes</summary>
    public short ReadInt16()
    {
        BaseStream.Read(readBuffer, 0, sizeof(short));
        return BitConverter.ToInt16(readBuffer, 0);
    }

    /// <summary>Reads a uint from the stream and increases the position by four bytes</summary>
    public uint ReadUInt32()
    {
        BaseStream.Read(readBuffer, 0, sizeof(uint));
        return BitConverter.ToUInt32(readBuffer, 0);
    }

    /// <summary>Reads a int from the stream and increases the position by four bytes</summary>
    public int ReadInt32()
    {
        BaseStream.Read(readBuffer, 0, sizeof(int));
        return BitConverter.ToInt32(readBuffer, 0);
    }

    /// <summary>Reads a ulong from the stream and increases the position by eight bytes</summary>
    public ulong ReadUInt64()
    {
        BaseStream.Read(readBuffer, 0, sizeof(ulong));
        return BitConverter.ToUInt64(readBuffer, 0);
    }

    /// <summary>Reads a long from the stream and increases the position by eight bytes</summary>
    public long ReadInt64()
    {
        BaseStream.Read(readBuffer, 0, sizeof(long));
        return BitConverter.ToInt64(readBuffer, 0);
    }

    /// <summary>Reads a float from the stream and increases the position by four bytes</summary>
    public float ReadSingle()
    {
        BaseStream.Read(readBuffer, 0, sizeof(float));
        return BitConverter.ToSingle(readBuffer, 0);
    }

    /// <summary>Reads a double from the stream and increases the position by eight bytes</summary>
    public double ReadDouble()
    {
        BaseStream.Read(readBuffer, 0, sizeof(double));
        return BitConverter.ToDouble(readBuffer, 0);
    }

    /// <summary>Reads a decimal from the stream and increases the position by sixteen bytes</summary>
    public decimal ReadDecimal()
    {
        return Read<decimal>();
    }

    /// <summary>Reads a string from the stream</summary>
    public string ReadString()
    {
        byte meta = ReadByte();

        int lengthSizeBit = (meta >> 0) & 1;
        int charSizeBit = (meta >> 1) & 1;

        int length;
        if (lengthSizeBit == 1) length = ReadInt32();
        else length = ReadByte();

        char[] retData = new char[length];
        if (charSizeBit == 1)
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
        int length = ReadInt32();
        string[] retData = new string[length];
        for (int i = 0; i < retData.Length; i++) retData[i] = ReadString();
        return retData;
    }
    #endregion

    /// <summary>Delete all content and reset the position</summary>
    public void Clear()
    {
        Position = 0;
        Length = 0;
    }
    public void Flush()
    {
        BaseStream.Flush();
    }
    /// <summary>Get data as byte array</summary>
    public byte[] GetBytes()
    {
        byte[] result = new byte[BaseStream.Length];
        long backup = Position;
        Position = 0;
        BaseStream.Read(result, 0, (int)BaseStream.Length);
        Position = backup;
        return result;
    }
    /// <summary>Get data as string</summary>
    public string GetString()
    {
        byte[] data = GetBytes();
        char[] saveData = new char[data.Length];
        for (int i = 0; i < saveData.Length; i++)
        {
            saveData[i] = (char)data[i];
        }
        return new string(saveData);

    }

    /// <summary>Save data as binary file to the specified path</summary>
    public void Save(string path)
    {
        File.WriteAllBytes(path, GetBytes());
    }
    /// <summary>Compress data with DeflateStream, position will reset</summary>
    public void Compress(CompressionLevel level = CompressionLevel.Optimal)
    {
        using (var resultStream = new MemoryStream())
        {
            BaseStream.Position = 0;
            using (var compressStream = new DeflateStream(resultStream, level, true))
            {
                BaseStream.CopyTo(compressStream);
            }
            BaseStream.SetLength(0);
            resultStream.Position = 0;
            resultStream.CopyTo(BaseStream);
            BaseStream.Position = 0;
        }
    }
    /// <summary>Decompress data with DeflateStream, position will reset</summary>
    public void Decompress()
    {
        using (var resultStream = new MemoryStream())
        {
            BaseStream.Position = 0;
            using (var decompressStream = new DeflateStream(BaseStream, CompressionMode.Decompress, true))
            {
                decompressStream.CopyTo(resultStream);
            }
            BaseStream.SetLength(0);
            resultStream.Position = 0;
            resultStream.CopyTo(BaseStream);
            BaseStream.Position = 0;
        }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            BaseStream.Dispose();

            disposedValue = true;
        }
    }

    ~BinaryView()
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

