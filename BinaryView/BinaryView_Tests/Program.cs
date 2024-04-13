using System;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Running;

namespace BinaryView_Tests;

class Program
{



    static unsafe void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();

        Printer = new StandardConsolePrinter()
        {
            PrintFailAsException = false,
        };

        Section.CreateDispose();
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

        RunTestsSynchronously();
    }
}

