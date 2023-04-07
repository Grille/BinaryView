using System;
using System.Collections.Generic;
using System.Text;
using SysIOC = System.IO.Compression;

namespace GGL.IO.Compression;

public enum CompressionLevel
{
    Optimal = SysIOC.CompressionLevel.Optimal,
    Fastest = SysIOC.CompressionLevel.Fastest,
    NoCompression = SysIOC.CompressionLevel.NoCompression,
}

public enum CompressionType
{
    GZip,
    Deflate,
    Brotli,
    ZLib,
}
