using System;
using System.Collections.Generic;
using System.Text;
using SysIOC = System.IO.Compression;

namespace GGL.IO;

public enum ViewMode
{
    Read,
    Write,
}

public enum StringLengthMode
{
    ByteCount,
    CharCount,
}

public enum LengthPrefix 
{
    /// <summary>Uses defult length-prefix of object</summary>
    Default,

    /// <summary>Writes no length-prefix, can only be used for write operations.</summary>
    None,

    SByte,
    Byte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,

    Single,
    Double,

    /// <summary>Automatically choses the smallest length-prefix. 1bit selector and 7 or 15 bit length.</summary>
    UIntSmart15,

    /// <summary>Automatically choses the smallest length-prefix. 2bit selector and 6, 14, 30 or 62 bit length.</summary>
    UIntSmart62,
}

public enum Endianness
{
    /// <summary>Use endianness of current computer architecture.</summary>
    System,
    LittleEndian,
    BigEndian,
}
