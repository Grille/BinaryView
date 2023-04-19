using System;
using System.IO;
using System.IO.Compression;


namespace GGL.IO.Compression;
internal static class CompressionFactory
{
    public static Stream CreateDecompresser(CompressionType type, Stream baseStream, bool leaveOpen) => type switch
    {
        CompressionType.Deflate => new DeflateStream(baseStream, CompressionMode.Decompress, leaveOpen),
        CompressionType.GZip => new GZipStream(baseStream, CompressionMode.Decompress, leaveOpen),
#if NETCOREAPP2_1_OR_GREATER
        CompressionType.Brotli => new BrotliStream(baseStream, CompressionMode.Decompress, leaveOpen),
#endif
#if NET6_0_OR_GREATER
        CompressionType.ZLib => new ZLibStream(baseStream, CompressionMode.Decompress, leaveOpen),
#endif
        _ => throw new NotImplementedException($"{type} not available in this version of net."),
    };

    public static Stream CreateCompressor(CompressionType type, Stream baseStream, CompressionLevel level, bool leaveOpen) => type switch
    {
        CompressionType.Deflate => new DeflateStream(baseStream, level, leaveOpen),
        CompressionType.GZip => new GZipStream(baseStream, level, leaveOpen),
#if NETCOREAPP2_1_OR_GREATER
        CompressionType.Brotli => new BrotliStream(baseStream, level, leaveOpen),
#endif
#if NET6_0_OR_GREATER
        CompressionType.ZLib => new ZLibStream(baseStream, level, leaveOpen),
#endif
        _ => throw new NotImplementedException($"{type} not available in this version of net."),
    };
}
