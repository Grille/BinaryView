using System;

namespace BinaryView_Tests;

class Program
{
    static void Main(string[] args)
    {
        TUtils.CatchExeptions = false;

        Section.PrimitiveTypes();
        Section.GenericTypes();
        Section.Endianness();
        Section.Strings();
        Section.Arrays();
        Section.IList();
        Section.IViewObject();
        Section.Serializble();
        Section.Compresion();
        Section.View();
        Section.Map();
        Section.Speed();

        TUtils.WriteResults();
    }
}

