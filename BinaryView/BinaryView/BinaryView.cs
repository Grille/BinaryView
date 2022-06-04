using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GGL.IO;
public class BinaryView
{
    public BinaryViewWriter Writer;
    public BinaryViewReader Reader;

    public BinaryView()
    {
        _Init(new MemoryStream());
    }
    /// <summary>Initialize BinaryView with a FileStream</summary>
    /// <param name="path">File path</param>
    public BinaryView(string path)
    {
        _Init(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite));
    }

    /// <summary>Initialize BinaryView with a Stream</summary>
    public BinaryView(Stream stream, bool closeStream = false)
    {
        _Init(stream, closeStream);
    }

    private void _Init(Stream stream, bool closable = true)
    {
        var streamStack = new StreamStack(stream, closable);
        Writer = new BinaryViewWriter(streamStack);
        Reader = new BinaryViewReader(streamStack);
    }
}
