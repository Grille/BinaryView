using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;
internal class TestData
{
    public Stream Stream;
    public BinaryViewWriter Writer;
    public BinaryViewReader Reader;

    public TestData()
    {
        Stream = new MemoryStream();
        Writer = new BinaryViewWriter(Stream);
        Reader = new BinaryViewReader(Stream);
    }

    public void Destroy()
    {
        Writer.Dispose();
        Reader.Dispose();
        Stream.Dispose();
    }
}
