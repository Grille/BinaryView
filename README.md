# BinaryView
Class to easily write and read binary data from streams.
Available as a [NuGet Package](https://www.nuget.org/packages/GGL.BinaryView/).

## Features
* Autoincrement of Position
* Generic functions to write whole arrays or (unmanaged) structs
* Easy compresion with Deflate

## Examples
Write
```cs
//Load a file to write, set useCopy flag to false so that changes can written in the file
using (var view = new BinaryView("file.bin", false))
{
    //Write data in the file
    view.WriteString(Name);
    view.WriteInt32(Size);
    view.WriteArray<byte>(Data);

    //Compress the file at the end
    view.Compress();
}
```
Read
```cs
//Load a file to read, set useCopy flag to true so that view.Decompress() will not decompress the actually file
using (var view = new BinaryView("file.bin", true))
{
    //Decompress the stream at begining so that the data can be read
    view.Decompress();

    //Read the data in reverse order of how they were written
    Data = view.ReadArray<byte>();
    Size = view.ReadInt32();
    Name = view.ReadString();
}
```