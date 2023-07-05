using System.IO;
using System.IO.Compression;

namespace GGL.IO.Compression;
public class CompressorStackEntry : StreamStackEntry
{
    public readonly BinaryViewWriter Writer;
    public readonly CompressionType Type;
    public readonly CompressionLevel Level;
    public readonly LengthPrefix LengthPrefix;
    public readonly bool WriteLengthPerfix;

    public long ContentLength { get;private set; }
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
                Writer.WriteLengthPrefix(LengthPrefix, compressedStream.Length);
            }
            Writer.StreamStack.CopyToPeak(compressedStream, false);

            ContentLength = Stream.Length;
            CompressedLength = compressedStream.Length;
        }
    }
}
