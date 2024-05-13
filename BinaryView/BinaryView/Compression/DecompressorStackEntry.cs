using System.IO;
using Grille.IO.Internal;

namespace Grille.IO.Compression;
public class DecompressorStackEntry : StreamStackEntry
{
    public BinaryViewReader Reader { get; }
    public CompressionType Type { get; }
    public long Length { get; }

    public DecompressorStackEntry(BinaryViewReader br, CompressionType type, long length) : base(br.StreamStack, new MemoryStream(), false)
    {
        Reader = br;
        Type = type;
        Length = length;

        using (var compressedSection = Reader.StreamStack.GetSubStream(Length))
        {
            using (var decompressStream = CompressionFactory.CreateDecompresser(Type, compressedSection, true))
            {
                decompressStream.CopyTo(Stream);
                Stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
