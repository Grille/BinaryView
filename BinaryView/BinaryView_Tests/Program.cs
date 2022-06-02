using System;

namespace BinaryView_Tests;

class Program
{
    static void Main(string[] args)
    {
        TUtils.CatchExeptions = false;

        Section.S00Types();
        Section.S01GenericTypes();
        Section.S02Strings();
        Section.S03Arrays();
        Section.S04IList();
        Section.S05Serializble();
        Section.S06Compresion();
        Section.S07Map();
        Section.S08Speed();

        TUtils.WriteResults();
    }
}

