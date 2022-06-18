using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GGL.IO;
public class BinaryView : StreamStackUser
{

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
    public CharSize DefaultCharSize
    {
        set => Writer.DefaultCharSize = Reader.DefaultCharSize = value;
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
    public BinaryView(Stream stream, bool closeStream = false) : base(new StreamStack(stream, closeStream))
    {
        Writer = new BinaryViewWriter(StreamStack);
        Reader = new BinaryViewReader(StreamStack);
    }


    #region IDisposable Support
    protected override void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            Writer.Dispose();
            Reader.Dispose();
            StreamStack.Dispose();

            DisposedValue = true;
        }
    }
    #endregion
}
