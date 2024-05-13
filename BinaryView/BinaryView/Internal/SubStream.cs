using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Grille.IO.Internal;

internal class ReadonlySubStream : Stream
{
    Stream baseStream;
    long offset, length, position = 0;
    public ReadonlySubStream(Stream baseStream, long offset, long length)
    {
        if (baseStream == null)
            throw new ArgumentNullException(nameof(baseStream));

        this.baseStream = baseStream;
        this.offset = offset;
        this.length = length;

        baseStream.Seek(offset, SeekOrigin.Begin);
    }

    public override int ReadByte()
    {
        var remaining = length - (position + offset);
        if (remaining > 0)
            return baseStream.ReadByte();
        return -1;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var remaining = length - (position + offset);
        count = Math.Min(count, (int)remaining);
        if (count <= 0)
            return 0;
        var read = baseStream.Read(buffer, offset, count);
        position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var pos = position;

        if (origin == SeekOrigin.Begin)
            pos = offset;
        else if (origin == SeekOrigin.End)
            pos = length + offset;
        else if (origin == SeekOrigin.Current)
            pos += offset;

        if (pos < 0) pos = 0;
        else if (pos >= length) pos = length - 1;

        position = baseStream.Seek(this.offset + pos, SeekOrigin.Begin) - this.offset;

        return pos;
    }

    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => false;

    public override long Length => length;

    public override long Position { get => position; set { position = Seek(value, SeekOrigin.Begin); } }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}

