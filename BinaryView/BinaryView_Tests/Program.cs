using System;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Running;

namespace BinaryView_Tests;

class Program
{
    static unsafe void Main(string[] args)
    {
        TestSys.CatchExeptions = false;

        Section.PrimitiveTypes();
        Section.GenericTypes();
        Section.Endianness();
        Section.Prefix();
        Section.Strings();
        Section.Arrays();
        Section.IList();
        Section.IViewObject();
        Section.Serializble();
        Section.Compresion();
        Section.Combined();
        Section.Map();

        TestSys.WriteResults();

        Console.WriteLine();
        //BenchmarkRunner.Run<Benchmarks>();
    }
}

