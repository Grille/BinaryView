using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace BinaryView_Tests;
public class Benchmarks
{
    TestData data;

    [IterationSetup]
    public void Setup()
    {
        data = new TestData(1024);
    }

    [IterationCleanup]
    public void Cleanup()
    {
        data.Dispose();
    }

    [Benchmark]
    public unsafe void WriteByte()
    {
        for (int i = 0; i < 1000; i++)
            data.Writer.WriteByte(0);
    }

    [Benchmark]
    public void WriteIByteI()
    {
        for (int i = 0; i < 1000; i++)
            data.Writer.Write<byte>(0);
    }

    [Benchmark]
    public void ReadByte()
    {
        for (int i = 0; i < 1000; i++)
            data.Reader.ReadByte();
    }

    [Benchmark]
    public void ReadIByteI()
    {
        for (int i = 0; i < 1000; i++)
            data.Reader.Read<byte>();
    }

    [Benchmark]
    public unsafe void WriteDouble()
    {
        for (int i = 0; i < 1000; i++)
            data.Writer.WriteDouble(0);
    }
    [Benchmark]
    public void WriteIDoubleI()
    {
        for (int i = 0; i < 1000; i++)
            data.Writer.Write<double>(0);
    }
    [Benchmark]
    public void ReadDouble()
    {
        for (int i = 0; i < 1000; i++)
            data.Reader.ReadDouble();
    }
    [Benchmark]
    public void ReadIDoubleI()
    {
        for (int i = 0; i < 1000; i++)
            data.Reader.Read<double>();
    }


}
