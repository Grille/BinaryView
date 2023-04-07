using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGL.IO.Compression;
internal class CompressorStackEntry : StreamStackEntry
{
    public readonly BinaryViewWriter Writer;
    public readonly CompressionType Type;
    public readonly CompressionLevel Level;
    public readonly LengthPrefix LengthPrefix;
    public readonly bool UsesPerfix;

    public CompressorStackEntry(BinaryViewWriter bw, CompressionType type, CompressionLevel level) : base(bw.StreamStack, new MemoryStream(), false)
    {
        Writer = bw;
        Type = type;
        Level = level;
        UsesPerfix = false;
    }

    public CompressorStackEntry(BinaryViewWriter bw, CompressionType type, CompressionLevel level, LengthPrefix lengthPrefix) : base(bw.StreamStack, new MemoryStream(), false)
    {
        Writer = bw;
        Type = type;
        Level = level;
        LengthPrefix = lengthPrefix;
        UsesPerfix = true;
    }

    protected override void BeforeDispose()
    {
        using (var compressedStream = new MemoryStream())
        {
            using (var compressor = CompressionFactory.CreateCompressor(Type, compressedStream, (System.IO.Compression.CompressionLevel)Level, true))
            {
                Stream.Seek(0, SeekOrigin.Begin);
                Stream.CopyTo(compressor);
            }

            compressedStream.Seek(0, SeekOrigin.Begin);

            if (UsesPerfix)
            {
                Writer.WriteLengthPrefix(LengthPrefix, compressedStream.Length);
            }
            Writer.StreamStack.CopyToPeak(compressedStream, false);
        }
    }
}
