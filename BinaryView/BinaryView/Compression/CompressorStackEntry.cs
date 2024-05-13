using System.IO;
using System.IO.Compression;
using Grille.IO.Internal;

namespace Grille.IO.Compression;
public class CompressorStackEntry : StreamStackEntry
{
    public BinaryViewWriter Writer { get; }
    public CompressionType Type { get; }
    public CompressionLevel Level { get; }
    public LengthPrefix LengthPrefix { get; }
    public bool WriteLengthPerfix { get; }

    public long ContentLength { get; private set; }
    public long CompressedLength { get; private set; }

    public CompressorStackEntry(BinaryViewWriter bw, CompressionType type, CompressionLevel level) : base(bw.StreamStack, new MemoryStream(), false)
    {
        Writer = bw;
        Type = type;
        Level = level;
        WriteLengthPerfix = false;
    }

    public CompressorStackEntry(BinaryViewWriter bw, CompressionType type, CompressionLevel level, LengthPrefix lengthPrefix) : base(bw.StreamStack, new MemoryStream(), false)
    {
        Writer = bw;
        Type = type;
        Level = level;
        LengthPrefix = lengthPrefix;
        WriteLengthPerfix = true;
    }

    protected override void BeforeDispose()
    {
        using (var compressedStream = new MemoryStream())
        {
            using (var compressor = CompressionFactory.CreateCompressor(Type, compressedStream, Level, true))
            {
                Stream.Seek(0, SeekOrigin.Begin);
                Stream.CopyTo(compressor);
            }

            compressedStream.Seek(0, SeekOrigin.Begin);

            if (WriteLengthPerfix)
            {
                Writer.WriteLengthPrefix(compressedStream.Length, LengthPrefix);
            }
            Writer.StreamStack.CopyToPeak(compressedStream, false);

            ContentLength = Stream.Length;
            CompressedLength = compressedStream.Length;
        }
    }
}
