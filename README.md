# BinaryView
class to easily write and read binary data from stream

## Examples
Write
```cs
using (var view = new BinaryView("file.bin", true))
{
    view.WriteString(Name);
    view.WriteInt32(Size);
    view.WriteArray<byte>(Data);
    view.Compress();
}
```
Read
```cs
using (var view = new BinaryView("file.bin", false))
{
    view.Decompress();
    Data = view.ReadArray<byte>();
    Size = view.ReadInt32();
    Name = view.ReadString();
}
```