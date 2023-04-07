using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;
internal class TestData : IDisposable
{
    public readonly Stream Stream;

    public BinaryViewWriter Writer => new BinaryViewWriter(Stream);
    public BinaryViewReader Reader => new BinaryViewReader(Stream);

    public BinaryView ViewWriter => new BinaryView(Stream, ViewMode.Write);
    public BinaryView ViewReader => new BinaryView(Stream, ViewMode.Read);

    public int Position
    {
        get => (int)Stream.Position;
    }

    public TestData(string file)
    {
        Stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
    }

    public TestData(Stream stream)
    {
        Stream = stream;
    }

    public TestData(int size = 0)
    {
        Stream = new MemoryStream();

        for (int i = 0; i < size; i++)
            Stream.WriteByte((byte)i);

        Stream.Position = 0;
    }

    public void Seek(long offset, SeekOrigin origin = SeekOrigin.Begin) => Stream.Seek(offset, origin);

    public void Setup<T>(IList<T> data) where T : unmanaged 
    {
        using (var bw = new BinaryViewWriter(Stream))
        {
            bw.WriteIList(data, LengthPrefix.None);
        }
        ResetPos();
    }

    public int PopPos()
    {
        int pos = (int)Stream.Position;
        ResetPos();
        return pos;
    }

    public void ResetPos()
    {
        Stream.Position = 0;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
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
