using System;
using System.IO;
using GGL;

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
        const bool EnableExeptions = false;
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
            testTyp(binaryView.WriteString, binaryView.ReadString, "TestString123", "Ä'*Ü-.,><%§ÃoÜ╝ô○╝+");
            Console.WriteLine();

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
            testGArray(new byte[] { 0, 2, 4, 6 });
            testGArray(new int[] { 0, -2, 4, -6 });
            testGArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
            testGArray(new Struct[] { new Struct() { A = 42, B = 3.6f }, new Struct() { A = 36, B = 1.666f } });
            testArray(binaryView.WriteStringArray, binaryView.ReadStringArray, new string[] { "ab", "cd", "ef", "gh" });
            Console.WriteLine();

            Console.WriteLine("test compression");
            test("compression", () =>
            {
                byte[] array = new byte[] { 0, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1 };
                binaryView.WriteArray(array);
                long olength = binaryView.Length;
                binaryView.Compress();
                long clength = binaryView.Length;
                binaryView.Decompress();
                bool result = olength == binaryView.Length && olength != clength;
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
                    binaryView.WriteArray(mapLayer1);
                    binaryView.WriteArray(mapLayer2);
                    binaryView.WriteArray(mapLayer3);
                    binaryView.Compress();
                    byte[] file = binaryView.GetBytes();
                    binaryView = new BinaryView(file);

                    bool result = true;
                    binaryView.Decompress();
                    result &= binaryView.ReadString() == "map";
                    result &= binaryView.ReadInt32() == size;
                    result &= binaryView.ReadSingle() == 0.45f;
                    result &= isArrayEqual(mapLayer1, binaryView.ReadArray<byte>());
                    result &= isArrayEqual(mapLayer2, binaryView.ReadArray<byte>());
                    result &= isArrayEqual(mapLayer3, binaryView.ReadArray<byte>());

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
            if (EnableExeptions)
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
            binaryView.Position = 0;
            binaryView.Length = 0;
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
                binaryView.Position = 0;
                write(input);
                binaryView.Position = 0;
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
                binaryView.Position = 0;
                binaryView.Write<T>(input);
                binaryView.Position = 0;
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
                binaryView.Position = 0;
                write(input);
                binaryView.Position = 0;
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
                binaryView.Position = 0;
                binaryView.WriteArray(input);
                binaryView.Position = 0;
                T[] result = binaryView.ReadArray<T>();
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
