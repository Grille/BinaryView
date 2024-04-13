using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace GGL.IO;
public class StreamStack : IDisposable
{
    readonly Stack<StreamStackEntry> _stack;
    public StreamStack(Stream stream, bool leaveOpen)
    {
        _stack = new Stack<StreamStackEntry>();

        Push(new StreamStackEntry(this, stream, leaveOpen));
        if (Peak == null)
        {
            throw new NullReferenceException();
        }
    }

    public int Count => _stack.Count;

    public StreamStackEntry Peak { private set; get; }

    /// <summary>
    /// Creates and push new MemoryStream
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public StreamStackEntry Create()
    {
        var obj = new StreamStackEntry(this, new MemoryStream(), true);
        Push(obj);
        return obj;
    }

    public StreamStackEntry CreateFrom(Stream stream)
    {
        var obj = Create();
        CopyToPeak(stream);
        Peak.Stream.Seek(0, SeekOrigin.Begin);
        return obj;
    }

    public StreamStackEntry Peek() => _stack.Peek();

    public void Push(Stream stream, bool leaveOpen)
    {
        Push(new(this, stream, leaveOpen));
    }

    public void Push(StreamStackEntry entry)
    {
        if (entry.Owner != this)
            throw new ArgumentException("Owner not this.");

        _stack.Push(entry);
        Peak = entry;
    }

    /// <inheritdoc/>
    public StreamStackEntry Pop()
    {
        //if (Count <= 1)
        //    throw new InvalidOperationException("Only 1 element left, can't empty stack");

        var entry = _stack.Pop();
        if (Count > 0)
        {
            Peak = Peek();
        }
        else
        {
            Peak = null!;
        }
        return entry;
    }

    public void CopyToPeak(Stream dataStream, bool keepPosition = false)
    {
        var peakStream = Peak.Stream;
        long pos = peakStream.Position;
        dataStream.CopyTo(peakStream);
        if (keepPosition)
            peakStream.Position = pos;
    }

    public void InsertToPeak(Stream dataStream)
    {
        var dstStream = Peak.Stream;

        int pos = (int)dstStream.Position;
        var buffer = new MemoryStream();

        dstStream.CopyTo(buffer);

        dstStream.Position = pos;
        dataStream.CopyTo(dstStream);

        buffer.Position = 0;
        buffer.CopyTo(dstStream);
    }

    /// <summary>
    /// Pop and dispose top element
    /// </summary>
    public void DisposePeak()
    {
        Pop().Dispose();
    }

    public Stream GetSubStream(long length)
    {
        var peakStream = Peek().Stream;
        var subStream = new ReadonlySubStream(peakStream, peakStream.Position, length);
        return subStream;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            while (Count > 0)
            {
                var stream = Pop();
                stream.Dispose();
            }

            disposedValue = true;
        }
    }

    ~StreamStack()
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
