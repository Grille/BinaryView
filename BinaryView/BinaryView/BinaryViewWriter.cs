using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using SysIOC = System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using GGL.IO.Compression;

namespace GGL.IO;

public class BinaryViewWriter : StreamStackUser
{
    /// <summary>Initialize BinaryView with a empty MemoryStream</summary>
    public BinaryViewWriter() :
        this(new StreamStack(new MemoryStream(), false))
    { }
    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryViewWriter(string path) :
        this(new StreamStack(new FileStream(path, FileMode.Create, FileAccess.Write), false))
    { }

    public BinaryViewWriter(byte[] bytes) :
        this(new StreamStack(new MemoryStream(bytes), false))
    { }

    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryViewWriter(Stream stream, bool leaveOpen = true) :
        this(new StreamStack(stream, leaveOpen))
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
        WriteFromPtr(&obj, sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteFromPtr(void* ptr, int size)
    {
        switch (size)
        {
            case 1:
            {
                byte value = *(byte*)ptr;
                if (NeedBitReorder)
                    value = EndianUtils.BitReverseTable[value];

                PeakStream.WriteByte(value);
                return;
            }
            default:
            {
                AssureBufferSize(size);
                if (NeedReorder)
                    EndianUtils.ReverseObjBits(ptr, size, NeedByteReorder, NeedBitReorder);

                for (int i = 0; i < size; i++)
                    Buffer[i] = *((byte*)ptr + i);

                PeakStream.Write(Buffer, 0, size);
                return;
            }
        }
    }

    public unsafe void WriteFromPtr(void* ptr, int size, int offset)
    {
        WriteFromPtr((byte*)ptr + offset, size);
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
        Serialize(obj, DefaultFormatter);
    }

    public void Serialize<T>(T obj, IFormatter formatter)
    {
        formatter.Serialize(PeakStream, obj);
    }

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="array">Array of unmanaged structs to write</param>
    public void WriteArray<T>(T[] array, LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged
    {
        WriteLengthPrefix(lengthPrefix, array.Length);
        for (int i = 0; i < array.Length; i++) Write(array[i]);
    }

    /// <summary>Writes a list of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">Array of unmanaged structs to write</param>
    public void WriteIList<T>(IList<T> list, LengthPrefix lengthPrefix = LengthPrefix.Default) where T : unmanaged
    {
        WriteLengthPrefix(lengthPrefix, list.Count);
        for (int i = 0; i < list.Count; i++) Write(list[i]);
    }

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">List of unmanaged structs to write</param>
    /// <param name="offset">start offset in the array</param>
    /// <param name="count">number of elements to be written</param>
    public void WriteIList<T>(IList<T> list, int offset, int count) where T : unmanaged
    {
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

#if NET5_0_OR_GREATER
    /// <summary>Writes a float to the stream and increases the position by four bytes</summary>
    public unsafe void WriteHalf(Half input) => Write(input);
#endif

    /// <summary>Writes a float to the stream and increases the position by four bytes</summary>
    public unsafe void WriteSingle(float input) => Write(input);

    /// <summary>Writes a double to the stream and increases the position by eight byte</summary>
    public unsafe void WriteDouble(double input) => Write(input);

    /// <summary>Writes a decimal to the stream and increases the position by sixteen bytes</summary>
    public unsafe void WriteDecimal(decimal input) => Write(input);

    /// <summary>Writes a string as byte array with an length prefix.</summary>
    public void WriteString(in string input, LengthPrefix lengthPrefix = LengthPrefix.Default, Encoding encoding = null)
    {
        if (encoding == null)
            encoding = DefaultEncoding;

        var bytes = encoding.GetBytes(input);

        WriteArray(bytes, lengthPrefix);
    }

    /// <summary>
    /// Writes an string as byte array and terminates with null.
    /// </summary>
    /// <remarks>null-terminated strings are error prone, if possible use WriteString instead.</remarks>
    /// <param name="input"></param>
    /// <param name="encoding"></param>
    public void WriteTerminatedString(in string input, Encoding encoding = null)
    {
        if (encoding == null)
            encoding = DefaultEncoding;

        var bytes = encoding.GetBytes(input);
        WriteArray(bytes, LengthPrefix.None);
        WriteByte(0);
    }

    /// <summary>Writes a array of strings</summary>
    public void WriteStringArray(string[] input)
    {
        WriteLengthPrefix(LengthPrefix.UInt32, input.Length);
        for (int i = 0; i < input.Length; i++) WriteString(input[i], LengthPrefix.UInt32);
    }



    public void WriteLengthPrefix(LengthPrefix lengthPrefix, long length)
    {
        switch (lengthPrefix)
        {
            case LengthPrefix.None:
                return;
            case LengthPrefix.Default:
                WriteLengthPrefix(DefaultLengthPrefix, length);
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
            case LengthPrefix.Single:
                WriteSingle(length);
                return;
            case LengthPrefix.Double:
                WriteDouble(length);
                return;
            case LengthPrefix.UIntSmart15:
                WriteIView((UIntSmart15)length);
                return;
            case LengthPrefix.UIntSmart62:
                WriteIView((UIntSmart62)length);
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

