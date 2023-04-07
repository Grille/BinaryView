using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GGL.IO;
public class StreamStackEntry : IDisposable
{
    public readonly StreamStack Owner;
    public readonly Stream Stream;
    public readonly bool LeaveOpen;

    public bool IsPeak => Owner.Peak == this;

    public bool IsDisposed => disposedValue;
    

    public StreamStackEntry(StreamStack owner, Stream stream, bool leaveOpen)
    {
        Owner = owner;
        Stream = stream;
        LeaveOpen = leaveOpen;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    /// <summary>
    /// Entry alredy poped from stack, and will be disposed next.
    /// </summary>
    protected virtual void BeforeDispose()
    {

    }

    private void dispose()
    {
        if (disposedValue)
            return;

        // pop if still on stack, to support using syntax.
        if (IsPeak)
            Owner.Pop();

        BeforeDispose();

        if (!LeaveOpen)
            Stream.Dispose();

        disposedValue = true;
    }

    ~StreamStackEntry()
    {
        dispose();
    }

    public void Dispose()
    {
        dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}
