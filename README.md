# BinaryView
Class to easily write and read binary data from streams.<br>
Available as a [NuGet Package](https://www.nuget.org/packages/GGL.BinaryView/).
<br>

## Features
* Generic functions to write whole arrays and (unmanaged) structs
* Easy compresion/decompresion with Deflate
<br>

## Examples
```cs
using GGL.IO;
```
Write
```cs
//Open a file to write
using (var view = new BinaryViewWriter("file.bin"))
{
    //Write data in the file
    view.WriteString(Name);
    view.WriteInt32(Size);
    
    //Compress section
    view.BeginDeflateSection();
    
    view.WriteArray<byte>(Data);
    
    view.EndDeflateSection();
}
```
Read
```cs
//Open a file to read
using (var view = new BinaryViewReader("file.bin"))
{
    //Read the data in same order of how they were written
    Name = view.ReadString();
    Size = view.ReadInt32();
    
    //Decompress section
    view.BeginDeflateSection();
    
    Data = view.ReadArray<byte>();
    
    view.EndDeflateSection();
}
```
