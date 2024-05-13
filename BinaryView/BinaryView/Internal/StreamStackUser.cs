using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Grille.IO.Interfaces;

namespace Grille.IO.Internal;

/// <summary>
/// Base class for <see cref="BinaryView"/>, <see cref="BinaryViewReader"/> and <see cref="BinaryViewWriter"/>, should not be used directly.
/// </summary>
public abstract class StreamStackUser : IDisposable
{
    protected byte[] Buffer { get; private set; }

    private Encoding _encoding = Encoding.Default;
    private int _bufferSize = 0;
    private LengthPrefix _lengthPrefix = LengthPrefix.UInt32;
    private Endianness _byteOrder = Endianness.System;

    public ILengthPrefix? CustomLengthPrefixHandler;

    protected bool NeedBitReorder { get; private set; } = false;
    protected bool NeedByteReorder { get; private set; } = false;
    protected bool NeedReorder { get; private set; } = false;

    public bool ReverseBitsPerByte
    {
        get => NeedBitReorder;
        set
        {
            NeedBitReorder = value;
            NeedReorder = NeedByteReorder || NeedBitReorder;
        }
    }

    public Endianness Endianness
    {
        get => _byteOrder;
        set
        {
            _byteOrder = value;
            NeedByteReorder = EndianUtils.NeedByteReorder(_byteOrder);
            NeedReorder = NeedByteReorder || NeedBitReorder;
        }
    }

    public StringLengthMode StringLengthMode { get; set; }

    /// <summary>
    /// Size of buffer for write and read operations, grows dynamically.
    /// </summary>
    public int BufferSize
    {
        get => _bufferSize;
        set
        {
            _bufferSize = value;
            Buffer = new byte[_bufferSize];
        }
    }

    /// <summary>
    /// Encoding used by Strings, if not specified as parameter.
    /// </summary>
    public Encoding Encoding
    {
        get => _encoding;
        set
        {
            _encoding = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// LengthPrefix used by (String, Array, etc.) functions, if not specified as parameter.
    /// </summary>
    public LengthPrefix LengthPrefix
    {
        get => _lengthPrefix;
        set
        {
            if (value == LengthPrefix.Default)
                throw new ArgumentException($"{nameof(LengthPrefix)} can't be set to Default.");
            _lengthPrefix = value;
        }
    }

    public StreamStack StreamStack { get; }

    public Stream PeakStream => StreamStack.Peak.Stream;

    /// <inheritdoc cref="MemoryStream.Position"/>
    public long Position
    {
        get => PeakStream.Position;
        set => PeakStream.Position = value;
    }

    /// <inheritdoc cref="MemoryStream.Length"/>
    public long Length
    {
        get => PeakStream.Length;
        set => PeakStream.SetLength(value);
    }

    /// <summary>
    /// Count of remaining bytes in PeakStream.
    /// </summary>
    public long Remaining => Length - Position;

    public StreamStackUser(StreamStack stack) : this(stack, 16) { }

    public StreamStackUser(StreamStack stack, int bufferSize)
    {
        StreamStack = stack;
        if (bufferSize >= 0)
        {
            BufferSize = bufferSize;
        }
        if (Buffer == null)
        {
            throw new ArgumentException("bufferSize must be >= 0", nameof(bufferSize));
        }
    }

    public void AssureBufferSize(int size)
    {
        if (_bufferSize < size)
        {
            BufferSize = size;
        }
    }

    /// <inheritdoc cref="MemoryStream.Seek(long, SeekOrigin)"/>
    public long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        return PeakStream.Seek(offset, origin);
    }

    /// <summary>Returns current Position, executes Stream.Seek after.</summary>
    public long Exch(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        var pos = PeakStream.Position;
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

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
