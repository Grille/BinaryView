using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace GGL.IO;
public class StreamStackEntry : IDisposable
{
    public StreamStack Owner { get; }
    public Stream Stream { get; }
    public bool LeaveOpen { get; }

    public bool IsPeak => Owner.Peak == this;

    public bool IsDisposed => disposedValue;
    

    public StreamStackEntry(StreamStack owner, Stream stream, bool leaveOpen)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

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

    public void Dispose()
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
    #endregion
}
