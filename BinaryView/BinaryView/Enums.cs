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

public enum LengthPrefix 
{
    Default,
    None,
    SByte,
    Byte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
}

public enum CharSizePrefix
{
    Default,
    Byte,
    Char,
}

public enum Endianness
{
    LittleEndian,
    BigEndian,
    Default = LittleEndian,
}
