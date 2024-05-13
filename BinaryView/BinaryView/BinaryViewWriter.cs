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
using Grille.IO.Compression;
using Grille.IO.Interfaces;
using Grille.IO.Internal;

namespace Grille.IO;

public sealed class BinaryViewWriter : StreamStackUser
{
    /// <summary>
    /// Gets or Sets a value indicating whether to ensure that the writen value fits in the <see cref="LengthPrefix"/> type range.<br/>
    /// Default value is <c>true</c>
    /// </summary>
    /// <remarks>
    /// If undetected, this will corrupt your stream, attempts to later read any data after will probably fail.
    /// </remarks>
    public bool ValidateLengthPrefix { get; set; } = true;

    /// <summary>
    /// Gets or Sets a value indicating whether  that the string will still be the same when read again with the same encoding.<br/>
    /// Default value is <c>false</c>
    /// </summary>
    /// <remarks>
    /// This check is relatively expensive and is only useful when using Encodings with a limited character set like <see cref="Encoding.ASCII"/>.<br/>
    /// A failure here will not result in a corrupted stream, only the read string will have an incorrect value.
    /// </remarks>
    public bool ValidateEncoding { get; set; } = false;

    /// <summary>
    /// Gets or Sets a value indicating whether <c>WriteTerminatedString</c> should check, if the string contains escape characters.<br/>
    /// Default value is <c>true</c>
    /// </summary>
    /// <remarks>
    /// If undetected, this will corrupt your stream, later attempts to read this string or any data after will probably fail.
    /// </remarks>
    public bool ValidateTerminatedString { get; set; } = true;

    /// <summary>Initialize BinaryView with a new empty <see cref="MemoryStream"/></summary>
    public BinaryViewWriter() :
        this(new StreamStack(new MemoryStream(), false))
    { }
    /// <summary>Initialize BinaryView with a new <see cref="FileStream"/></summary>
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
        WriteFromPtr(&obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteFromPtr(void* ptr, int byteSize)
    {
        var bytePtr = (byte*)ptr;
        switch (byteSize)
        {
            case 1:
            {
                byte value = *bytePtr;
                if (NeedBitReorder)
                    value = EndianUtils.BitReverseTable[value];

                PeakStream.WriteByte(value);
                return;
            }
            default:
            {
                AssureBufferSize(byteSize);
                if (NeedReorder)
                    EndianUtils.ReverseBits(ptr, byteSize, NeedByteReorder, NeedBitReorder);

                for (int i = 0; i < byteSize; i++)
                    Buffer[i] = bytePtr[i];

                PeakStream.Write(Buffer, 0, byteSize);
                return;
            }
        }
    }

    public unsafe void WriteFromPtr(IntPtr ptr, int byteSize)
    {
        WriteFromPtr((void*)ptr, byteSize);
    }

    public unsafe void WriteFromPtr<T>(T* ptr) where T : unmanaged
    {
        WriteFromPtr(ptr, sizeof(T));
    }

    public unsafe void WriteFromRef<T>(ref T obj) where T : unmanaged
    {
        fixed (T* ptr = &obj)
        {
            WriteFromPtr(ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIView<T>(T obj) where T : IBinaryViewWritable
    {
        obj.WriteToView(this);
    }

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="array">Array of unmanaged structs to write</param>
    public void WriteArray<T>(T[] array) where T : unmanaged => WriteArray(array, LengthPrefix);

    /// <inheritdoc cref="WriteArray{T}(T[])"></inheritdoc>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="array">Array of unmanaged structs to write</param>
    /// <param name="lengthPrefix"></param>
    public void WriteArray<T>(T[] array, LengthPrefix lengthPrefix) where T : unmanaged
    {
        WriteLengthPrefix(array.Length, lengthPrefix);
        for (int i = 0; i < array.Length; i++) Write(array[i]);
    }

    public void WriteIList<T>(IList<T> list) where T : unmanaged => WriteIList(list, LengthPrefix);

    /// <summary>Writes a list of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">Array of unmanaged structs to write</param>
    /// <param name="lengthPrefix"></param>
    public void WriteIList<T>(IList<T> list, LengthPrefix lengthPrefix) where T : unmanaged
    {
        WriteLengthPrefix(list.Count, lengthPrefix);
        for (int i = 0; i < list.Count; i++) Write(list[i]);
    }

    /// <summary>Writes a array of unmanaged structs into the stream and increases the position by the size of the array elements, add prefix for length</summary>
    /// <typeparam name="T"></typeparam> Type of unmanaged struct
    /// <param name="list">List of unmanaged structs to write</param>
    /// <param name="offset">start offset in the array</param>
    /// <param name="count">number of elements to be written</param>
    public void WriteIList<T>(IList<T> list, int count, int offset = 0) where T : unmanaged
    {
        for (int i = 0; i < count; i++) Write(list[i + offset]);
    }

    public void WriteICollection<T>(ICollection<T> collection) where T : unmanaged => WriteICollection(collection, LengthPrefix);

    public void WriteICollection<T>(ICollection<T> collection, LengthPrefix lengthPrefix) where T : unmanaged
    {
        WriteLengthPrefix(collection.Count, lengthPrefix);
        foreach (var item in collection)
        {
            Write(item);
        }
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

    private byte[] GetEncodingBytes(string input, Encoding encoding)
    {
        if (ValidateEncoding)
        {
            var bytes = encoding.GetBytes(input);
            var valstr = encoding.GetString(bytes);
            if (input != valstr)
                throw new ArgumentException($"Encoding to {encoding.EncodingName} failed.");
            else return bytes;
        }
        else
            return encoding.GetBytes(input);
    }

    public void WriteString(string input) => WriteString(input, LengthPrefix, Encoding);

    public void WriteString(string input, LengthPrefix lengthPrefix) => WriteString(input, lengthPrefix, Encoding);

    public void WriteString(string input, Encoding encoding) => WriteString(input, LengthPrefix, encoding);

    /// <summary>Writes a string as byte array with an length prefix.</summary>
    public void WriteString(string input, LengthPrefix lengthPrefix, Encoding encoding)
    {
        var bytes = GetEncodingBytes(input, encoding);

        if (StringLengthMode == StringLengthMode.CharCount)
            WriteLengthPrefix(input.Length, lengthPrefix);
        else
            WriteLengthPrefix(bytes.Length, lengthPrefix);

        WriteArray(bytes, LengthPrefix.None);
    }

    public void WriteTerminatedString(string input) => WriteTerminatedString(input, Encoding);
    /// <summary>
    /// Writes an string as byte array and terminates with null.
    /// </summary>
    /// <remarks>null-terminated strings are error prone, if possible use WriteString instead.</remarks>
    /// <param name="input"></param>
    /// <param name="encoding"></param>
    public void WriteTerminatedString(string input, Encoding encoding)
    {
        var bytes = GetEncodingBytes(input, encoding);

        if (ValidateTerminatedString)
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] == 0)
                    throw new ArgumentException($"Unexpected null terminator found at {i}/{bytes.Length-1} in string.", nameof(input));

        WriteArray(bytes, LengthPrefix.None);
        WriteByte(0);
    }

    /// <summary>Writes a array of strings</summary>
    public void WriteStringArray(string[] input)
    {
        WriteLengthPrefix(input.Length);
        for (int i = 0; i < input.Length; i++) WriteString(input[i], Encoding);
    }

    private bool DoValidateLengthPrefix(LengthPrefix lengthPrefix, long length) => lengthPrefix switch
    {
        LengthPrefix.None => true,
        LengthPrefix.Default => DoValidateLengthPrefix(LengthPrefix, length),
        LengthPrefix.SByte => length == (sbyte)length,
        LengthPrefix.Byte => length == (byte)length,
        LengthPrefix.Int16 => length == (short)length,
        LengthPrefix.UInt16 => length == (ushort)length,
        LengthPrefix.Int32 => length == (int)length,
        LengthPrefix.UInt32 => length == (uint)length,
        LengthPrefix.Int64 => true,
        LengthPrefix.UInt64 => length >= 0,
        LengthPrefix.Single => length == (long)(float)length,
        LengthPrefix.Double => length == (long)(double)length,
        LengthPrefix.UIntSmart15 => length == (long)(UIntSmart15)length,
        LengthPrefix.UIntSmart62 => length == (long)(UIntSmart62)length,
        LengthPrefix.Custom => true,
        _ => throw new ArgumentOutOfRangeException(nameof(lengthPrefix), lengthPrefix.ToString())
    };

    private void WriteCustomLengthPrefix(long length)
    {
        if (CustomLengthPrefixHandler == null)
            throw new InvalidOperationException("CustomLengthPrefixHandler is not set.");

        CustomLengthPrefixHandler.Length = length;
        WriteIView(CustomLengthPrefixHandler);
    }

    public void WriteLengthPrefix(long length) => WriteLengthPrefix(length, LengthPrefix);

    public void WriteLengthPrefix(long length, LengthPrefix lengthPrefix)
    {
        if (ValidateLengthPrefix && !DoValidateLengthPrefix(lengthPrefix, length))
        {
            throw new InvalidCastException($"Value {length} can't be casted to {lengthPrefix}.");
        }

        switch (lengthPrefix)
        {
            case LengthPrefix.None:
                return;
            case LengthPrefix.Default:
                WriteLengthPrefix(length, LengthPrefix);
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
            case LengthPrefix.Custom:
                WriteCustomLengthPrefix(length);
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
                BufferSize = 0;
            }

            DisposedValue = true;
        }
    }
    #endregion


}

