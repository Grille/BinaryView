using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGL.IO;
namespace ByteStream_Tests
{
    static class Tests
    {
        static int testOkCount = 0, testFailCount = 0, testErrorCount = 0;
        static ByteStream byteStream;
        private static string text;
        const bool enableExeptions = false;
        public static void Run()
        {
            byteStream = new ByteStream();
            Console.WriteLine("Run tests...\n");

            Console.WriteLine("test types");
            testTyp<byte>("byte", byteStream.WriteByte, byteStream.ReadByte, 4);
            testTyp<int>("int", byteStream.WriteInt, byteStream.ReadInt, 4000);
            testTyp<int>("int", byteStream.WriteInt, byteStream.ReadInt, -4000);
            testTyp<float>("float", byteStream.WriteFloat, byteStream.ReadFloat, 8.78f);
            testTyp<float>("float", byteStream.WriteFloat, byteStream.ReadFloat, -8.78f);
            testTyp<string>("string", byteStream.WriteString, byteStream.ReadString, "testString123");
            Console.WriteLine();

            Console.WriteLine("test arrays");
            testArray<byte>("byte", byteStream.WriteByteArray, byteStream.ReadByteArray, new byte[] { 0, 2, 4, 6 });
            testArray<int>("int", byteStream.WriteIntArray, byteStream.ReadIntArray, new int[] { 0, -2, 4, -6 });
            testArray<float>("float", byteStream.WriteFloatArray, byteStream.ReadFloatArray, new float[] { 0, -2.5f, 4.25f, -6.66f });
            testArray<string>("string", byteStream.WriteStringArray, byteStream.ReadStringArray, new string[] { "ab", "cd", "ef", "gh" });
            Console.WriteLine();

            Console.WriteLine("test compression");
            testArray<byte>("byte", byteStream.WriteByteArray, CompressMode.None, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            testArray<byte>("byte", byteStream.WriteByteArray, CompressMode.RLE, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            test("read/write byte[] -RLE (size > 256)", () =>
            {
                byte[] array = new byte[1000];
                for (int i = 0; i < array.Length; i++)
                    array[i] = 1;
                byteStream.WriteByteArray(array, CompressMode.RLE);
                byteStream.ResetIndex();
                bool result = isArrayEqual(byteStream.ReadByteArray(),array);
                if (result) printTest(0);
                else printTest(1);
            });
            test("auto select (0,1,0,1..) = None", () =>
            {
                byte[] array = new byte[] { 0, 1,0,1,0,1 };
                byteStream.WriteByteArray(array, CompressMode.Auto);
                bool result = byteStream.Index == 8;
                if (result) printTest(0);
                else printTest(1, "" + byteStream.Index);
            });
            test("auto select (0..0,1..1) = RLE", () =>
            {
                byte[] array = new byte[] { 0, 0, 0, 1 ,1,1};
                byteStream.WriteByteArray(array, CompressMode.Auto);
                bool result = byteStream.Index == 6;
                if (result) printTest(0);
                else printTest(1, ""+byteStream.Index);
            });
            Console.WriteLine();
            
            Console.WriteLine("complex test");
            int size = 64;
            for (int it = 0; it < 4; it++) {
                test("map " + size, () =>
                  {
                      byte[] mapLayer1 = new byte[size];
                      byte[] mapLayer2 = new byte[size];
                      byte[] mapLayer3 = new byte[size];
                      Random rnd = new Random(1);
                      for (int i = 0; i < size; i++)
                          mapLayer1[i] = (byte)(rnd.NextDouble() * 255f);
                      rnd = new Random(2);
                      for (int i = 0; i < size; i++)
                          mapLayer2[i] = (byte)(rnd.NextDouble() * 2f);

                      byteStream = new ByteStream();
                      byteStream.WriteString("map");
                      byteStream.WriteInt(size);
                      byteStream.WriteFloat(0.45f);
                      byteStream.WriteByteArray(mapLayer1, CompressMode.None);
                      byteStream.WriteByteArray(mapLayer2, CompressMode.RLE);
                      byteStream.WriteByteArray(mapLayer3, CompressMode.RLE);

                      byte[] file = byteStream.GetBytes();

                      byteStream = new ByteStream(file);
                      bool result = true;
                      result &= byteStream.ReadString() == "map";
                      result &= byteStream.ReadInt() == size;
                      result &= byteStream.ReadFloat() == 0.45f;
                      result &= isArrayEqual(mapLayer1, byteStream.ReadByteArray());
                      result &= isArrayEqual(mapLayer2, byteStream.ReadByteArray());
                      result &= isArrayEqual(mapLayer3, byteStream.ReadByteArray());

                      if (result) printTest(0);
                      else printTest(1);
                  });
                  size *= 2;
            }

            float count = testOkCount + testFailCount + testErrorCount;
            Console.WriteLine("\nExecuted tests: " + count);
            Console.WriteLine("ok: " + testOkCount + " | " + 100 * Math.Round((double)(testOkCount / count), 2) + "%");
            Console.WriteLine("fail: " + testFailCount + " | " + 100 * Math.Round((double)(testFailCount / count), 2) + "%");
            Console.WriteLine("error: " + testErrorCount + " | " + 100 * Math.Round((double)(testErrorCount / count), 2) + "%");
        }

        private static void test(string name, Action method)
        {

            text = name;
            if (enableExeptions)
                method();
            else
                try
                {
                    method();
                }
                catch (Exception e) { printTest(2, e.Message); }
            byteStream = new ByteStream();
        }

        private static void testTyp<T>(string typ, Action<T> write, Func<T> read, T input)
        {
           
            test("read/write " + typ+" ("+ input + ")", () =>
              {
                  byteStream.ResetIndex();
                  write(input);
                  byteStream.ResetIndex();
                  T result = read();
                  if ("" + result == "" + input) printTest(0);
                  else printTest(1, "" + result);
              });
        }
        private static void testArray<T>(string typ, Action<T[]> write, Func<T[]> read, T[] input)
        {
            test("read/write " + typ + "[] (" + arrayToString(input)+")", () =>
            {
                byteStream.ResetIndex();
                write(input);
                byteStream.ResetIndex();
                T[] result = read();
                if (input.Length!= result.Length)
                    printTest(1, "length not equal" + input.Length+"!="+ result.Length);
                if (isArrayEqual(input, result)) printTest(0);
                else printTest(1, "array("+ arrayToString(result) + ")");
            });
        }

        private static void testArray<T>(string typ, Action<T[],CompressMode> write, CompressMode option, Func<T[]> read, T[] input)
        {
            test("read/write " + typ + "[] -"+option+" (" + arrayToString(input) + ")", () =>
            {
                byteStream.ResetIndex();
                write(input,option);
                byteStream.ResetIndex();
                T[] result = read();
                if (input.Length != result.Length)
                    printTest(1, "length not equal" + input.Length + "!=" + result.Length);
                if (isArrayEqual(input,result)) printTest(0);
                else printTest(1, "array(" + arrayToString(result) + ")");
            });
            
        }

        private static void printTest(int state)
        {
            printTest(state, null);
        }
        private static void printTest(int state, string message)
        {
            switch (state)
            {
                case 0:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(text + " OK");
                    testOkCount++;
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(text + " FAIL");
                    testFailCount++;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(text + " ERROR");
                    testErrorCount++;
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            if (message != null) Console.Write(" -> " + message);
            Console.WriteLine();
        }
        private static bool isArrayEqual<T>(T[] array1,T[] array2)
        {
            if (array1.Length != array2.Length)
                return false;
            for (int i = 0; i < array2.Length; i++)
                if ("" + array2[i] != "" + array1[i])
                    return false;
            return true;
        }
        private static string arrayToString<T>(T[] array)
        {
            string result = "";
            for (int i = 0; i < array.Length; i++)
            {
                result += "" + array[i];
                if (i < array.Length-1) result += ",";
            }
            return result;
        }
    }
}
