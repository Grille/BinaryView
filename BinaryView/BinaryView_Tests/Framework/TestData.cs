using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;
internal class TestData
{
    public Stream Stream;
    public BinaryView View;
    public BinaryViewWriter Writer;
    public BinaryViewReader Reader;

    public int Ptr
    {
        get => (int)Stream.Position;
    }

    public TestData()
    {
        Stream = new MemoryStream();
        //View = new BinaryView(Stream);
        Writer = new BinaryViewWriter(Stream);
        Reader = new BinaryViewReader(Stream);
    }

    public void Destroy()
    {
        Writer.Dispose();
        Reader.Dispose();
        Stream.Dispose();
    }

    public int PopPtr()
    {
        int pos = (int)Stream.Position;
        ResetPtr();
        return pos;
    }

    public void ResetPtr()
    {
        Stream.Position = 0;
    }
}
