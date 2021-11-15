using System;
using System.Collections.Generic;
using System.Text;
using SysIOC = System.IO.Compression;

namespace GGL.IO;


public enum CompressionLevel
{
    Fastest = SysIOC.CompressionLevel.Fastest,
    Optimal = SysIOC.CompressionLevel.Optimal,
}

