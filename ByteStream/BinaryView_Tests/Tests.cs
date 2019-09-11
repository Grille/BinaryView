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
        struct Struct
        {
            public int A;
            public float B;
            public override string ToString() => "{A:" + A + ";B:" + B+"}";
            public override bool Equals(object obj) => A == ((Struct)obj).A && B == ((Struct)obj).B;
        }
        static int testOkCount = 0, testFailCount = 0, testErrorCount = 0;
        static BinaryView byteStream;
        private static string text;
        const bool enableExeptions = false;
        public static void Run()
        {
            byteStream = new BinaryView();
            Console.WriteLine("Run tests...\n");

            Console.WriteLine("test types");
            testTyp(byteStream.WriteByte, byteStream.ReadByte, byte.MinValue);
            testTyp(byteStream.WriteByte, byteStream.ReadByte, byte.MaxValue);
            testTyp(byteStream.WriteInt32, byteStream.ReadInt32, int.MinValue);
            testTyp(byteStream.WriteInt32, byteStream.ReadInt32, int.MaxValue);
            testTyp(byteStream.WriteSingle, byteStream.ReadSingle, float.MinValue);
            testTyp(byteStream.WriteSingle, byteStream.ReadSingle, float.MaxValue);
            testTyp(byteStream.WriteString, byteStream.ReadString, "testString123");
            Console.WriteLine();
            
            Console.WriteLine("test generic types");
            testGTyp(char.MinValue);
            testGTyp(char.MaxValue);
            testGTyp(byte.MinValue);
            testGTyp(byte.MaxValue);
            testGTyp(sbyte.MinValue);
            testGTyp(sbyte.MaxValue);
            testGTyp(ushort.MinValue);
            testGTyp(ushort.MaxValue);
            testGTyp(short.MinValue);
            testGTyp(short.MaxValue);
            testGTyp(uint.MinValue);
            testGTyp(uint.MaxValue);
            testGTyp(int.MinValue);
            testGTyp(int.MaxValue);
            testGTyp(ulong.MinValue);
            testGTyp(ulong.MaxValue);
            testGTyp(long.MinValue);
            testGTyp(long.MaxValue);
            testGTyp(float.MinValue);
            testGTyp(float.MaxValue);
            testGTyp(double.MinValue);
            testGTyp(double.MaxValue);
            testGTyp(decimal.MinValue);
            testGTyp(decimal.MaxValue);
            testGTyp(new Struct() { A = 42, B = 3.6f });
            Console.WriteLine();

            Console.WriteLine("test arrays");
            testArray(byteStream.WriteByteArray, byteStream.ReadByteArray, new byte[] { 0, 2, 4, 6 });
            testArray(byteStream.WriteStringArray, byteStream.ReadStringArray, new string[] { "ab", "cd", "ef", "gh" });
            Console.WriteLine();

            Console.WriteLine("test generic arrays");
            testGArray(new byte[] { 0, 2, 4, 6 });
            testGArray(new int[] { 0, -2, 4, -6 });
            testGArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
            testGArray(new Struct[] { new Struct() { A = 42, B = 3.6f }, new Struct() { A = 36, B = 1.666f } });
            Console.WriteLine();

            Console.WriteLine("test compression");
            testArray(byteStream.WriteByteArray, CompressMode.None, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            testArray(byteStream.WriteByteArray, CompressMode.RLE, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
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
                bool result = byteStream.Position == 8;
                if (result) printTest(0);
                else printTest(1, "" + byteStream.Position);
            });
            test("auto select (0..0,1..1) = RLE", () =>
            {
                byte[] array = new byte[] { 0, 0, 0, 1 ,1,1};
                byteStream.WriteByteArray(array, CompressMode.Auto);
                bool result = byteStream.Position == 6;
                if (result) printTest(0);
                else printTest(1, "" + byteStream.Position);
            });
            Console.WriteLine();

            Console.WriteLine("complex test");
            int size = 64;
            for (int it = 0; it < 4; it++)
            {
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

                    byteStream = new BinaryView();
                    byteStream.WriteString("map");
                    byteStream.WriteInt32(size);
                    byteStream.WriteSingle(0.45f);
                    byteStream.WriteByteArray(mapLayer1, CompressMode.None);
                    byteStream.WriteByteArray(mapLayer2, CompressMode.RLE);
                    byteStream.WriteByteArray(mapLayer3, CompressMode.RLE);

                    byte[] file = byteStream.GetBytes();

                    byteStream = new BinaryView(file);

                    bool result = true;
                    result &= byteStream.ReadString() == "map";
                    result &= byteStream.ReadInt32() == size;
                    result &= byteStream.ReadSingle() == 0.45f;
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
            byteStream = new BinaryView();
        }

        private static void testTyp<T>(Action<T> write, Func<T> read, T input)
        {
            string typ = typeof(T).Name;
            test("read/write " + typ+" ("+ input + ")", () =>
              {
                  byteStream.ResetIndex();
                  write(input);
                  byteStream.ResetIndex();
                  T result = read();
                  if (result.Equals(input)) printTest(0);
                  else printTest(1, "" + result);
              });
        }
        private static void testGTyp<T>(T input) where T : unmanaged
        {
            string typ = typeof(T).Name;
            test("read/write " + typ + " (" + input + ")", () =>
            {
                byteStream.ResetIndex();
                byteStream.Write<T>(input);
                byteStream.ResetIndex();
                T result = byteStream.Read<T>();
                if (result.Equals(input)) printTest(0);
                else printTest(1, "" + result);
            });
        }
        private static void testArray<T>(Action<T[]> write, Func<T[]> read, T[] input)
        {
            string typ = typeof(T).Name;
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
        private static void testGArray<T>(T[] input) where T : unmanaged
        {
            string typ = typeof(T).Name;
            test("read/write " + typ + "[] (" + arrayToString(input) + ")", () =>
            {
                byteStream.ResetIndex();
                byteStream.WriteArray(input);
                byteStream.ResetIndex();
                T[] result = byteStream.ReadArray<T>();
                if (input.Length != result.Length)
                    printTest(1, "length not equal" + input.Length + "!=" + result.Length);
                if (isArrayEqual(input, result)) printTest(0);
                else printTest(1, "array(" + arrayToString(result) + ")");
            });
        }

        private static void testArray<T>(Action<T[],CompressMode> write, CompressMode option, Func<T[]> read, T[] input)
        {
            string typ = typeof(T).Name;
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
