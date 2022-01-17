# BinaryView
Low-level libary to easily write and read binary data from streams and files.<br>
Available as a [NuGet Package](https://www.nuget.org/packages/GGL.BinaryView/).
<br>

## Features
* write/read functions for all value types (bool, float, int, decimal, usw...)
* Generic functions to write whole lists and (unmanaged) structs
* Easy compresion/decompresion of section or whole stram with Deflate
* Support ISerializable, Warning size is usually pretty bloated in comparision to generic and native functions!
* Customizable list-length prefixes and string char size
<br>

## Warning
This library has basically no error checking...<br>
If you mess up the write/read order, or try to read a corrupted/wrong file things will break!<br>
Then read functions will give your wrong values, or ReadArray hangs because it try's to read an insanely huge array!<br>

## Documentation
This project has no proper documentation yet, but most functions have descriptions, and should be pretty self-explanatory<br>

## Examples
```cs
using GGL.IO;
```
Write
```cs
// Open a file to write
using (var view = new BinaryViewWriter("file.bin"))
{
    // Type used for LengthPrefix by String and Array
    view.DefaultLengthPrefix = LengthPrefix.UInt32;

    // Write data in the file
    view.WriteString(Name);
    view.WriteInt32(Size);
    view.Write<Vector2>(Vec);
    
    // Compress section
    view.BeginDeflateSection();
    
    view.WriteArray<byte>(Data0);

    // Override default prefix to use byte instead
    view.WriteArray<Vector2>(Data1, LengthPrefix.Byte);

    view.EndDeflateSection();
}
```
Read
```cs
// Open a file to read
using (var view = new BinaryViewReader("file.bin"))
{
    view.DefaultLengthPrefix = LengthPrefix.UInt32;

    // Read the data in same order of how they were written
    Name = view.ReadString();
    Size = view.ReadInt32();
    Vec = view.Read<Vector2>()
    
    // Decompress section
    view.BeginDeflateSection();
    
    Data0 = view.ReadArray<byte>();

    // Read prefix-type must match written one, otherwise things will explode!
    Data1 = view.ReadArray<Vector2>(LengthPrefix.Byte);

    view.EndDeflateSection();
}
```
