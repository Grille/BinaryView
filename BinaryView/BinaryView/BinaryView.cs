using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GGL.IO;
public class BinaryView : IDisposable
{
    public readonly StreamStack StreamStack;

    public BinaryViewWriter Writer;
    public BinaryViewReader Reader;


    public Endianness BitOrder
    {
        set => Writer.BitOrder = Reader.BitOrder = value;
    }
    public Endianness ByteOrder
    {
        set => Writer.ByteOrder = Reader.ByteOrder = value;
    }
    public int BufferSize
    {
        set => Writer.BufferSize = Reader.BufferSize = value;
    }
    public LengthPrefix DefaultLengthPrefix
    {
        set => Writer.DefaultLengthPrefix = Reader.DefaultLengthPrefix = value;
    }
    public CharSizePrefix DefaultCharSizePrefix
    {
        set => Writer.DefaultCharSizePrefix = Reader.DefaultCharSizePrefix = value;
    }


    public long Position
    {
        get => StreamStack.Peak.Stream.Position;
        set => StreamStack.Peak.Stream.Position = value;
    }
    public long Length
    {
        get => StreamStack.Peak.Stream.Length;
        set => StreamStack.Peak.Stream.SetLength(value);
    }


    public BinaryView() :
        this(new MemoryStream())
    { }

    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryView(string path) :
        this(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
    { }


    public BinaryView(byte[] bytes) :
        this(new MemoryStream(bytes))
    { }


    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryView(Stream stream, bool closeStream = false)
    {
        StreamStack = new StreamStack(stream, closeStream);
        Writer = new BinaryViewWriter(StreamStack);
        Reader = new BinaryViewReader(StreamStack);
    }


    public long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        return StreamStack.Peak.Stream.Seek(offset, origin);
    }

    public long Exch(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        long pos = StreamStack.Peak.Stream.Position;
        StreamStack.Peak.Stream.Seek(offset, origin);
        return pos;
    }


    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            Writer.Dispose();
            Reader.Dispose();
            StreamStack.Dispose();

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
