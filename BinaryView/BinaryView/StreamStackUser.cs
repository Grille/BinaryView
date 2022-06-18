using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GGL.IO;
public abstract class StreamStackUser : IDisposable
{
    public readonly StreamStack StreamStack;

    public Stream PeakStream;

    public long Position
    {
        get => PeakStream.Position;
        set => PeakStream.Position = value;
    }
    public long Length
    {
        get => PeakStream.Length;
        set => PeakStream.SetLength(value);
    }

    public StreamStackUser(StreamStack stack)
    {
        StreamStack = stack;
        StreamStack.PeakChanged += (object sender, StreamStackEntry e) =>
        {
            PeakStream = e.Stream;
        };
        PeakStream = StreamStack.Peek().Stream;
    }



    public long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        return PeakStream.Seek(offset, origin);
    }

    public long Exch(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        long pos = PeakStream.Position;
        PeakStream.Seek(offset, origin);
        return pos;
    }

    #region IDisposable Support
    protected bool DisposedValue = false; // To detect redundant calls

    protected abstract void Dispose(bool disposing);

    ~StreamStackUser()
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
