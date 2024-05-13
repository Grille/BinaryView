using System;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Running;

namespace BinaryView_Tests;

class Program
{



    static unsafe void Main(string[] args)
    {
        Printer = new StandardConsolePrinter()
        {
            PrintFailAsException = false,
        };

        Section.CreateDispose();
        Section.PrimitiveTypes();
        Section.GenericTypes();
        Section.Unsafe();
        Section.Endianness();
        Section.Prefix();
        Section.Strings();
        Section.Arrays();
        Section.IList();
        Section.ICollection();
        Section.IViewObject();
        Section.Serializble();
        Section.Compresion();
        Section.Combined();
        Section.Map();

        RunTestsSynchronously();
    }
}

