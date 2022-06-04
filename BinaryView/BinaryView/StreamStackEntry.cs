using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GGL.IO;
public class StreamStackEntry : IDisposable
{
    public readonly Stream Stream;
    public readonly object Args;
    public readonly bool Closeable;

    public readonly BufferedStream WriteBuffer;
    public readonly BufferedStream ReadBuffer;

    public bool IsDisposed
    {
        get => disposedValue;
    }

    public StreamStackEntry(Stream stream, bool closeable, object args = null)
    {
        Stream = stream;
        Args = args;
        Closeable = closeable;
    }

    public void Deconstruct(out Stream stream, out object args)
    {
        stream = Stream;
        args = Args;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (Closeable)
                Stream.Dispose();
            disposedValue = true;
        }
    }

    ~StreamStackEntry()
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
