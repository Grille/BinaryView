# BinaryView
Libary to easily write and read binary data from streams and files.
Available as a [NuGet Package](https://www.nuget.org/packages/Grille.IO.BinaryView/).

This library basically provides more advanced versions of the [`System.IO.BinaryWriter`](https://learn.microsoft.com/en-us/dotnet/api/system.io.binarywriter) and [`System.IO.BinaryReader`](https://learn.microsoft.com/en-us/dotnet/api/system.io.binaryreader) classes.

[Documentation](https://github.com/Grille/BinaryView/wiki)

## Features
* Symmetrical write/read functions.
* Simple to use string functions with encoding options.
* Generic functions to write whole lists and (unmanaged) structs.
* Easy compresion/decompresion of sections or whole stream's with GZip, Deflate, Brotli or ZLib.
* Support of ISerializable, (when ISerializable is not implemented, the size is usually pretty bloated in comparision to other functions.)
* Smart list-length prefixes depending on array size. (42 takes 1 byte, 3000 takes 2 etc.)
* Separate support for byte and bit endianness/order.

## Example Write/Read (asymmetrical)
```cs
using Grille.IO;
using Grille.IO.Compression;
```
Write

```cs
// Open a file to write
using (var bw = new BinaryViewWriter("file.bin"))
{
    // Type used for LengthPrefix by Strings and Arrays
    bw.DefaultLengthPrefix = LengthPrefix.UInt32;

    // Write data in the file
    bw.WriteString(Name);
    bw.WriteInt32(Age);
    bw.Write<Vector2>(Pos);
    
    // Compress section
    bw.BeginCompressedSection(CompressionType.Deflate);
    
    bw.WriteArray<byte>(Data0);

    // Override default prefix to use byte instead
    bw.WriteArray<Vector2>(Data1, LengthPrefix.Byte);

    // Write length manuel
    bw.WriteInt32(Data2.Length);
    bw.WriteArray<float>(Data2, LengthPrefix.None);

    bw.EndCompressedSection();
}
```
Read

```cs
// Open a file to read
using (var br = new BinaryViewReader("file.bin"))
{
    br.DefaultLengthPrefix = LengthPrefix.UInt32;

    // Read the data in same order of how they were written
    Name = br.ReadString();
    Age = br.ReadInt32();
    Pos = br.Read<Vector2>()
    
    // Decompress section
    br.BeginCompressedSection(CompressionType.Deflate);
    
    Data0 = br.ReadArray<byte>();

    // Read prefix-type must match written one.
    Data1 = br.ReadArray<Vector2>(LengthPrefix.Byte);

    // Read length manuel
    int length = br.ReadInt32();
    Data2 = br.WriteArray<float>(length);

    br.EndCompressedSection();
}
```
## Example View (symmetrical)
Uses same code for write and read operations.

```cs
using GGL.IO;
using GGL.IO.Compression;
```
View

```cs
// Open a file to read
using (var view = new BinaryView("file.bin", ViewMode.Read /*ViewMode.Write*/))
{
    view.DefaultLengthPrefix = LengthPrefix.UInt32;

    view.String(ref Name);
    view.Int32(ref Age);
    view.Struct<Vector2>(ref Pos);
    
    view.BeginCompressedSection(CompressionType.Deflate);
    
    view.Array<byte>(ref Data0);
    view.Array<Vector2>(ref Data1, LengthPrefix.Byte);

    // Switch when different operations are needed for read and write.
    if (view.Mode == ViewMode.Read){
        var br = view.Reader;
        int length = br.ReadInt32();
        Data2 = br.WriteArray<float>(length);
    }
    else {
        var bw = view.Writer;
        bw.WriteInt32(Data2.Length);
        bw.WriteArray<float>(Data2, LengthPrefix.None);
    }

    // Above can also be solved this way:
    /*
    int length = Data2.Length;
    view.Int32(ref length);
    view.Array<float>(ref Data2, length);
    */

    view.EndCompressedSection();
}
``` 