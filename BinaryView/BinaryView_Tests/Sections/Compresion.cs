
using System.IO.Compression;
using BinaryView_Tests.Framework;

namespace BinaryView_Tests;
partial class Section
{
    public static void Compresion()
    {
        Section("test compresion");

        new CompresionTests("Deflate", CompressionType.Deflate).Run();
        new CompresionTests("GZip", CompressionType.GZip).Run();
#if NETCOREAPP2_1_OR_GREATER
        new CompresionTests("Brotli", CompressionType.Brotli).Run();
#endif
#if NET6_0_OR_GREATER
        new CompresionTests("ZLib", CompressionType.ZLib).Run();
#endif
    }
}
