using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;
internal class TestData : IDisposable
{
    public Stream Stream;
    public BinaryView View;
    public BinaryViewWriter Writer;
    public BinaryViewReader Reader;

    public int Ptr
    {
        get => (int)Stream.Position;
    }

    public TestData(int size = 0)
    {
        Stream = new MemoryStream();
        View = new BinaryView(Stream);
        Writer = View.Writer;
        Reader = View.Reader;

        for (int i = 0; i < size; i++)
            Stream.WriteByte((byte)i);

        Stream.Position = 0;
    }

    public void Setup<T>(IList<T> data) where T : unmanaged 
    {
        Writer.WriteIList(data, LengthPrefix.None);
        ResetPtr();
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

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            View.Dispose();
            Writer.Dispose();
            Reader.Dispose();
            Stream.Dispose();

            disposedValue = true;
        }
    }

    ~TestData()
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
