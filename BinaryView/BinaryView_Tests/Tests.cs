using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GGL.IO;
namespace ByteStream_Tests
{
    static class Tests
    {
        private struct Struct
        {
            public int A;
            public float B;
            public override string ToString() => "{A:" + A + ";B:" + B + "}";
            public override bool Equals(object obj) => A == ((Struct)obj).A && B == ((Struct)obj).B;
        }
        static int testOkCount = 0, testFailCount = 0, testErrorCount = 0;
        static BinaryView binaryView;
        private static string text;
        const bool enableExeptions = false;
        public static void Run()
        {
            binaryView = new BinaryView();
            Console.WriteLine("Run tests...\n");

            Console.WriteLine("test types");
            testTyp(binaryView.WriteChar, binaryView.ReadChar, char.MinValue, char.MaxValue);
            testTyp(binaryView.WriteByte, binaryView.ReadByte, byte.MinValue, byte.MaxValue);
            testTyp(binaryView.WriteSByte, binaryView.ReadSByte, sbyte.MinValue, sbyte.MaxValue);
            testTyp(binaryView.WriteUInt16, binaryView.ReadUInt16, ushort.MinValue, ushort.MaxValue);
            testTyp(binaryView.WriteInt16, binaryView.ReadInt16, short.MinValue, short.MaxValue);
            testTyp(binaryView.WriteUInt32, binaryView.ReadUInt32, uint.MinValue, uint.MaxValue);
            testTyp(binaryView.WriteInt32, binaryView.ReadInt32, int.MinValue, int.MaxValue);
            testTyp(binaryView.WriteUInt64, binaryView.ReadUInt64, ulong.MinValue, ulong.MaxValue);
            testTyp(binaryView.WriteInt64, binaryView.ReadInt64, long.MinValue, long.MaxValue);
            testTyp(binaryView.WriteSingle, binaryView.ReadSingle, float.MinValue, float.MaxValue);
            testTyp(binaryView.WriteDouble, binaryView.ReadDouble, double.MinValue, double.MaxValue);
            testTyp(binaryView.WriteDecimal, binaryView.ReadDecimal, decimal.MinValue, decimal.MaxValue);
            Console.WriteLine();
            testTyp(binaryView.WriteString, binaryView.ReadString, "TestString123", "Ä'*Ü-.,><%§ÃoÜ╝ô○╝+");

            Console.WriteLine("test generic types");
            testGTyp(char.MinValue, char.MaxValue);
            testGTyp(byte.MinValue, byte.MaxValue);
            testGTyp(sbyte.MinValue, sbyte.MaxValue);
            testGTyp(ushort.MinValue, ushort.MaxValue);
            testGTyp(short.MinValue, short.MaxValue);
            testGTyp(uint.MinValue, uint.MaxValue);
            testGTyp(int.MinValue, int.MaxValue);
            testGTyp(ulong.MinValue, ulong.MaxValue);
            testGTyp(long.MinValue, long.MaxValue);
            testGTyp(float.MinValue, float.MaxValue);
            testGTyp(double.MinValue, double.MaxValue);
            testGTyp(decimal.MinValue, decimal.MaxValue);
            testGTyp(new Struct() { A = 42, B = 3.6f });
            Console.WriteLine();

            Console.WriteLine("test arrays");
            testArray(binaryView.WriteByteArray, binaryView.ReadByteArray, new byte[] { 0, 2, 4, 6 });
            testArray(binaryView.WriteStringArray, binaryView.ReadStringArray, new string[] { "ab", "cd", "ef", "gh" });
            Console.WriteLine();

            Console.WriteLine("test generic arrays");
            testGArray(new byte[] { 0, 2, 4, 6 });
            testGArray(new int[] { 0, -2, 4, -6 });
            testGArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
            testGArray(new Struct[] { new Struct() { A = 42, B = 3.6f }, new Struct() { A = 36, B = 1.666f } });
            Console.WriteLine();

            Console.WriteLine("test compression");
            testArray(binaryView.WriteByteArray, CompressMode.None, binaryView.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            testArray(binaryView.WriteByteArray, CompressMode.RLE, binaryView.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            test("read/write byte[] -RLE (size > 256)", () =>
            {
                byte[] array = new byte[1000];
                for (int i = 0; i < array.Length; i++)
                    array[i] = 1;
                binaryView.WriteByteArray(array, CompressMode.RLE);
                binaryView.ResetIndex();
                bool result = isArrayEqual(binaryView.ReadByteArray(), array);
                if (result) printTest(0);
                else printTest(1);
            });
            test("auto select (0,1,0,1..) = None", () =>
            {
                byte[] array = new byte[] { 0, 1, 0, 1, 0, 1 };
                binaryView.WriteByteArray(array, CompressMode.Auto);
                bool result = binaryView.Position == 8;
                if (result) printTest(0);
                else printTest(1, "" + binaryView.Position);
            });
            test("auto select (0..0,1..1) = RLE", () =>
            {
                byte[] array = new byte[] { 0, 0, 0, 1, 1, 1 };
                binaryView.WriteByteArray(array, CompressMode.Auto);
                bool result = binaryView.Position == 6;
                if (result) printTest(0);
                else printTest(1, "" + binaryView.Position);
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

                    binaryView = new BinaryView();
                    binaryView.WriteString("map");
                    binaryView.WriteInt32(size);
                    binaryView.WriteSingle(0.45f);
                    binaryView.WriteByteArray(mapLayer1, CompressMode.None);
                    binaryView.WriteByteArray(mapLayer2, CompressMode.RLE);
                    binaryView.WriteByteArray(mapLayer3, CompressMode.RLE);

                    byte[] file = binaryView.GetBytes();

                    binaryView = new BinaryView(file);

                    bool result = true;
                    result &= binaryView.ReadString() == "map";
                    result &= binaryView.ReadInt32() == size;
                    result &= binaryView.ReadSingle() == 0.45f;
                    result &= isArrayEqual(mapLayer1, binaryView.ReadByteArray());
                    result &= isArrayEqual(mapLayer2, binaryView.ReadByteArray());
                    result &= isArrayEqual(mapLayer3, binaryView.ReadByteArray());

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
            {
                method();
            }
            else
            {
                try
                {
                    method();
                }
                catch (Exception e) { printTest(2, e.Message); }
            }
            binaryView.ResetIndex();// = new BinaryView();
        }

        private static void testTyp<T>(Action<T> write, Func<T> read, T value1,T value2)
        {
            testTyp(write, read, value1);
            testTyp(write, read, value2);
        }
        private static void testTyp<T>(Action<T> write, Func<T> read, T input)
        {
            string typ = typeof(T).Name;
            test("read/write " + typ + " (" + input + ")", () =>
            {
                binaryView.ResetIndex();
                write(input);
                binaryView.ResetIndex();
                T result = read();
                if (result.Equals(input)) printTest(0);
                else printTest(1, "" + result);
            });
        }
        private static void testGTyp<T>(T value1, T value2) where T : unmanaged
        {
            testGTyp(value1);
            testGTyp(value2);
        }
        private static void testGTyp<T>(T input) where T : unmanaged
        {
            string typ = typeof(T).Name;
            test("read/write " + typ + " (" + input + ")", () =>
            {
                binaryView.ResetIndex();
                binaryView.Write<T>(input);
                binaryView.ResetIndex();
                T result = binaryView.Read<T>();
                if (result.Equals(input)) printTest(0);
                else printTest(1, "" + result);
            });
        }
        private static void testArray<T>(Action<T[]> write, Func<T[]> read, T[] input)
        {
            string typ = typeof(T).Name;
            test("read/write " + typ + "[] (" + arrayToString(input) + ")", () =>
            {
                binaryView.ResetIndex();
                write(input);
                binaryView.ResetIndex();
                T[] result = read();
                if (input.Length != result.Length)
                    printTest(1, "length not equal" + input.Length + "!=" + result.Length);
                if (isArrayEqual(input, result)) printTest(0);
                else printTest(1, "array(" + arrayToString(result) + ")");
            });
        }
        private static void testGArray<T>(T[] input) where T : unmanaged
        {
            string typ = typeof(T).Name;
            test("read/write " + typ + "[] (" + arrayToString(input) + ")", () =>
            {
                binaryView.ResetIndex();
                binaryView.WriteArray(input);
                binaryView.ResetIndex();
                T[] result = binaryView.ReadArray<T>();
                if (input.Length != result.Length)
                    printTest(1, "length not equal" + input.Length + "!=" + result.Length);
                if (isArrayEqual(input, result)) printTest(0);
                else printTest(1, "array(" + arrayToString(result) + ")");
            });
        }

        private static void testArray<T>(Action<T[], CompressMode> write, CompressMode option, Func<T[]> read, T[] input)
        {
            string typ = typeof(T).Name;
            test("read/write " + typ + "[] -" + option + " (" + arrayToString(input) + ")", () =>
            {
                binaryView.ResetIndex();
                write(input, option);
                binaryView.ResetIndex();
                T[] result = read();
                if (input.Length != result.Length)
                    printTest(1, "length not equal" + input.Length + "!=" + result.Length);
                if (isArrayEqual(input, result)) printTest(0);
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
        private static bool isArrayEqual<T>(T[] array1, T[] array2)
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
                if (i < array.Length - 1) result += ",";
            }
            return result;
        }
    }
}
