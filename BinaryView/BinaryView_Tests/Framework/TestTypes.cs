using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGL.IO;
using BinaryView_Tests.Framework;
using System.Drawing;

namespace BinaryView_Tests;

public record struct Int32Single(int A, float B)
{
    public override string ToString() => "{A:" + A + ";B:" + B + "}";
}

public unsafe struct UInt24
{
    fixed byte _[3];
    public static unsafe implicit operator UInt24(uint value) => *(UInt24*)&value;
    public static unsafe implicit operator uint(UInt24 value)
    {
        uint dst = 0;
        *(UInt24*)&dst = value;
        return dst;
    }

    public override string ToString() => ((uint)this).ToString();
    public override bool Equals(object obj)
    {
        if (obj is UInt24) 
            return (uint)this == (uint)(UInt24)obj;
        else
            return (uint)this == (uint)obj;
    }

    public static bool operator ==(UInt24 left, UInt24 right) => left.Equals(right);

    public static bool operator !=(UInt24 left, UInt24 right) => !(left == right);

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}

public class InterfaceImplementationView : IViewObject
{
    public string Name;
    public int A, B;

    public int Count;
    public byte[] Array;

    public void View(BinaryView view)
    {
        view.String(ref Name);

        view.Int32(ref A);
        view.Int32(ref B);


        view.BeginCompressedSection(CompressionType.Deflate);

        view.Int32(ref Count);
        view.Array(ref Array, Count);

        view.EndCompressedSection();
    }

    public void ReadFromView(BinaryViewReader br) => View(br);

    public void WriteToView(BinaryViewWriter bw) => View(bw);
}

public class InterfaceImplementation : IViewObject
{

    public int A, B;

    public void ReadFromView(BinaryViewReader br)
    {
        A = br.Read<int>();
        B = br.Read<int>();
    }

    public void WriteToView(BinaryViewWriter bw)
    {
        bw.Write(A);
        bw.Write(B);
    }
}

public class Map
{
    public string Name;
    public int Size;
    public float Float;
    public byte[] Layer0;
    public byte[] Layer1;
    public byte[] Layer2;

    public Map() { }

    public Map(int size)
    {
        Name = "Map";
        Size = size;
        Float = 1.45f;

        Layer0 = new byte[Size];
        Layer1 = new byte[Size];
        Layer2 = new byte[Size];

        var rnd = new Random(1);

        for (int i = 0; i < Size; i++)
            Layer0[i] = (byte)(rnd.NextDouble() * 255f);

        for (int i = 0; i < Size; i++)
            Layer2[i] = (byte)(rnd.NextDouble() * 2f);
    }
}
