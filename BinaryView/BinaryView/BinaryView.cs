using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GGL.IO;
public sealed class BinaryView : StreamStackUser
{
    readonly bool OwnsStreamStack;

    readonly BinaryViewWriter? _writer;
    readonly BinaryViewReader? _reader;

    public BinaryViewWriter Writer { get => _writer != null ? _writer : throw new NotSupportedException("BinaryView not in write mode."); }
    public BinaryViewReader Reader { get => _reader != null ? _reader : throw new NotSupportedException("BinaryView not in read mode."); }

    public ViewMode Mode { get; }

    public bool IsReading => Mode == ViewMode.Read;

    public BinaryView(Stream stream, ViewMode mode, bool leaveOpen = true) : base(new StreamStack(stream, leaveOpen), 0)
    {
        OwnsStreamStack = true;
        Mode = mode;
        if (Mode == ViewMode.Read)
            _reader = new BinaryViewReader(StreamStack);
        else
            _writer = new BinaryViewWriter(StreamStack);
    }

    public BinaryView(BinaryViewReader reader) : base(reader.StreamStack, 0)
    {
        OwnsStreamStack = false;
        _reader = reader;
        Mode = ViewMode.Read;
    }

    public BinaryView(BinaryViewWriter writer) : base(writer.StreamStack, 0)
    {
        OwnsStreamStack = false;
        _writer = writer;
        Mode = ViewMode.Write;
    }

    public static implicit operator BinaryViewReader?(BinaryView view) => view.Reader;
    public static implicit operator BinaryViewWriter?(BinaryView view) => view.Writer;

    public static implicit operator BinaryView(BinaryViewReader reader) => new(reader);
    public static implicit operator BinaryView(BinaryViewWriter writer) => new(writer);

    public void String(ref string str) => String(ref str, LengthPrefix, Encoding);

    public void String(ref string str, LengthPrefix lengthPrefix, Encoding encoding)
    {
        if (Mode == ViewMode.Read)
            str = Reader.ReadString(lengthPrefix, encoding);
        else
            Writer.WriteString(str, lengthPrefix, encoding);
    }

    public void String(ref string str, long length) => String(ref str, length, Encoding);

    public void String(ref string str, long length, Encoding encoding)
    {
        if (Mode == ViewMode.Read)
            str = Reader.ReadString(length, encoding);
        else
            Writer.WriteString(str, IO.LengthPrefix.None, encoding);
    }

    public void TerminatedString(ref string str) => TerminatedString(ref str, Encoding);
    public void TerminatedString(ref string str, Encoding encoding)
    {
        if (Mode == ViewMode.Read)
            str = Reader.ReadTerminatedString(encoding);
        else
            Writer.WriteTerminatedString(str, encoding);
    }

    public void Struct<T>(ref T obj) where T : unmanaged
    {
        if (Mode == ViewMode.Read)
            obj = Reader.Read<T>();
        else
            Writer.Write(obj);
    }

    public unsafe void StructPtr<T>(void* ptr, int size) where T : unmanaged
    {
        if (Mode == ViewMode.Read)
            Reader.ReadToPtr(ptr, size);
        else
            Writer.WriteFromPtr(ptr, size);
    }

    public unsafe void StructPtr<T>(void* ptr, int size, int offset) where T : unmanaged
    {
        if (Mode == ViewMode.Read)
            Reader.ReadToPtr(ptr, size, offset);
        else
            Writer.WriteFromPtr(ptr, size, offset);
    }

    public void Boolean(ref bool value) => Struct(ref value);
    public void Char(ref char value) => Struct(ref value);
    public void SByte(ref sbyte value) => Struct(ref value);
    public void Int16(ref short value) => Struct(ref value);
    public void Int32(ref int value) => Struct(ref value);
    public void Int64(ref long value) => Struct(ref value);
    public void Byte(ref byte value) => Struct(ref value);
    public void UInt16(ref ushort value) => Struct(ref value);
    public void UInt32(ref uint value) => Struct(ref value);
    public void UInt64(ref ulong value) => Struct(ref value);
    public void Single(ref float value) => Struct(ref value);

#if NET5_0_OR_GREATER
    public void Half(ref Half value) => Struct(ref value);
#endif

    public void Double(ref double value) => Struct(ref value);
    public void Decimal(ref decimal value) => Struct(ref value);

    public void Array<T>(ref T[] array) where T : unmanaged => Array(ref array, LengthPrefix);

    public void Array<T>(ref T[] array, LengthPrefix lengthPrefix) where T : unmanaged
    {
        if (Mode == ViewMode.Read)
            array = Reader.ReadArray<T>(lengthPrefix);
        else
            Writer.WriteArray(array, lengthPrefix);
    }

    public void Array<T>(ref T[] array, long length) where T : unmanaged
    {
        if (Mode == ViewMode.Read)
            array = Reader.ReadArray<T>(length);
        else
            Writer.WriteArray(array, IO.LengthPrefix.None);
    }

    public void IList<T>(IList<T> list) where T : unmanaged => IList(list, LengthPrefix);

    public void IList<T>(IList<T> list, LengthPrefix lengthPrefix) where T : unmanaged
    {
        if (Mode == ViewMode.Read)
            Reader.ReadToIList(list, lengthPrefix);
        else
            Writer.WriteIList(list, lengthPrefix);
    }

    public void IList<T>(IList<T> list, int offset, int count) where T : unmanaged
    {
        if (Mode == ViewMode.Read)
            Reader.ReadToIList(list, offset, count);
        else
            Writer.WriteIList(list, offset, count);
    }

    public void IView<T>(T obj) where T : class, IViewObject
    {
        if (Mode == ViewMode.Read)
            Reader.ReadToIView(obj);
        else
            Writer.WriteIView(obj);
    }

    public void ReadWriteLengthPrefix(ref long length, LengthPrefix lengthPrefix)
    {
        if (Mode == ViewMode.Read)
            length = Reader.ReadLengthPrefix(lengthPrefix);
        else
            Writer.WriteLengthPrefix(lengthPrefix, length);
    }

    protected override void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (disposing)
            {
                if (OwnsStreamStack)
                {
                    _reader?.Dispose();
                    _writer?.Dispose();
                }
            }
            DisposedValue = true;
        }
    }
}
