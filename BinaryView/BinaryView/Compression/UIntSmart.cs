using System;
using System.Runtime.InteropServices;

namespace GGL.IO.Compression;

public struct UIntSmart15 : IViewObject
{
    public const short MaxValue15Bit = (1 << 15) - 1;
    public const short MaxValue7Bit = (1 << 7) - 1;

    public const short MaxValue = (1 << 15) - 1;
    public const short MinValue = 0;

    short value;

    public UIntSmart15(short value)
    {
        if (value > MaxValue)
            value = MaxValue;
        else if (value < MinValue)
            value = MinValue;
        this.value = value;
    }

    public static explicit operator UIntSmart15(short a) => new(a);
    public static explicit operator short(UIntSmart15 a) => a.value;

    public void ReadFromView(BinaryViewReader br)
    {
        var bytes = new byte[2];
        bytes[0] = br.ReadByte();
        var prefix = bytes[0] & 0b0000_0001;

        if (prefix == 1)
            bytes[1] = br.ReadByte();


        value = (short)(BitConverter.ToUInt16(bytes, 0) >> 1);
    }

    public void WriteToView(BinaryViewWriter bw)
    {
        if (value > MaxValue15Bit)
        {
            throw new Exception();
        }
        else if (value > MaxValue7Bit)
        {
            var res = (ushort)(1 | value << 1);
            bw.WriteUInt16(res);
        }
        else if (value >= MinValue)
        {
            bw.WriteByte((byte)(value << 1));
        }
        else
        {
            throw new Exception();
        }
    }
}


public struct UIntSmart62 : IViewObject
{
    public const long MaxValue62Bit = (1L << 62) - 1;
    public const long MaxValue30Bit = (1L << 30) - 1;
    public const long MaxValue14Bit = (1L << 14) - 1;
    public const long MaxValue6Bit = (1L << 6) - 1;

    public const long MaxValue = (1L << 62) - 1;
    public const long MinValue = 0;

    long value;

    public UIntSmart62(long value)
    {
        if (value > MaxValue)
            value = MaxValue;
        else if (value < MinValue)
            value = MinValue;
        this.value = value;
    }

    public static explicit operator UIntSmart62(long a) => new(a);
    public static explicit operator long(UIntSmart62 a) => a.value;

    public void ReadFromView(BinaryViewReader br)
    {
        var bytes = new byte[8];
        bytes[0] = br.ReadByte();
        var prefix = bytes[0] & 0b0000_0011;

        if (prefix > 0)
            br.ReadToArray(bytes, (1 << prefix) - 1, 1);

        value = (long)(BitConverter.ToUInt64(bytes, 0) >> 2);
    }

    public void WriteToView(BinaryViewWriter bw)
    {
        if (value > MaxValue62Bit)
        {
            throw new Exception();
        }
        else if (value > MaxValue30Bit)
        {
            var res = (ulong)(3L | value << 2);
            bw.WriteUInt64(res);
        }
        else if (value > MaxValue14Bit)
        {
            var res = (uint)(2 | value << 2);
            bw.WriteUInt32(res);
        }
        else if (value > MaxValue6Bit)
        {
            var res = (ushort)(1 | value << 2);
            bw.WriteUInt16(res);
        }
        else if (value >= 0)
        {
            bw.WriteByte((byte)(value << 2));
        }
        else
        {
            throw new Exception();
        }
    }
}
