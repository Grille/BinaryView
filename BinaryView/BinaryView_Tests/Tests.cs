using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using GGL.IO;

namespace ByteStream_Tests;

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
    static Stream stream;
    static BinaryViewWriter bw;
    static BinaryViewReader br;
    private static string text;
    const bool EnableExeptions = true;
    public static void Run()
    {
        stream = new MemoryStream();
        bw = new BinaryViewWriter(stream);
        br = new BinaryViewReader(stream);

        Console.WriteLine("Run tests...\n");

        Console.WriteLine("test types");
        testTyp(bw.WriteChar, br.ReadChar, char.MinValue, char.MaxValue);
        testTyp(bw.WriteByte, br.ReadByte, byte.MinValue, byte.MaxValue);
        testTyp(bw.WriteSByte, br.ReadSByte, sbyte.MinValue, sbyte.MaxValue);
        testTyp(bw.WriteUInt16, br.ReadUInt16, ushort.MinValue, ushort.MaxValue);
        testTyp(bw.WriteInt16, br.ReadInt16, short.MinValue, short.MaxValue);
        testTyp(bw.WriteUInt32, br.ReadUInt32, uint.MinValue, uint.MaxValue);
        testTyp(bw.WriteInt32, br.ReadInt32, int.MinValue, int.MaxValue);
        testTyp(bw.WriteUInt64, br.ReadUInt64, ulong.MinValue, ulong.MaxValue);
        testTyp(bw.WriteInt64, br.ReadInt64, long.MinValue, long.MaxValue);
        testTyp(bw.WriteSingle, br.ReadSingle, float.MinValue, float.MaxValue);
        testTyp(bw.WriteDouble, br.ReadDouble, double.MinValue, double.MaxValue);
        testTyp(bw.WriteDecimal, br.ReadDecimal, decimal.MinValue, decimal.MaxValue);
        testTyp(bw.WriteString, br.ReadString, "TestString123", "Ä'*Ü-.,><%§ÃoÜ╝ô○╝+");
        Console.WriteLine();

        Console.WriteLine("test unmanaged types");
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
        testGTyp(new DateTime(2020, 07, 20, 15, 54, 24));
        testGTyp(new Point(10, 42));
        testGTyp(new RectangleF(10, 42, 25.5f, 23));
        Console.WriteLine();

        Console.WriteLine("test serializable types");
        testSTyp(42);
        testSTyp("Hello World");
        testSTyp(new DateTime(2000, 10, 20));
        testSTyp(new DateTime(2020, 07, 20, 15, 54, 24));
        testSTyp(new Point(2000, 10));
        testSTyp(new RectangleF(10, 42, 25.5f, 23));
        Console.WriteLine();

        Console.WriteLine("test arrays");
        testGArray(new byte[] { 0, 2, 4, 6 });
        testGArray(new int[] { 0, -2, 4, -6 });
        testGArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
        testGArray(new Struct[] { new Struct() { A = 42, B = 3.6f }, new Struct() { A = 36, B = 1.666f } });
        testArray(bw.WriteStringArray, br.ReadStringArray, new string[] { "ab", "cd", "ef", "gh" });
        Console.WriteLine();

        Console.WriteLine("test map");
        testMap(64, false);
        Console.WriteLine();

        Console.WriteLine("test map compressed");
        testMap(64, true);

        float count = testOkCount + testFailCount + testErrorCount;
        Console.WriteLine("\nExecuted tests: " + count);
        Console.WriteLine("ok: " + testOkCount + " | " + 100 * Math.Round((double)(testOkCount / count), 2) + "%");
        Console.WriteLine("fail: " + testFailCount + " | " + 100 * Math.Round((double)(testFailCount / count), 2) + "%");
        Console.WriteLine("error: " + testErrorCount + " | " + 100 * Math.Round((double)(testErrorCount / count), 2) + "%");
    }

    private static void test(string name, Action test)
    {
        text = name;
        if (EnableExeptions)
        {
            test();
        }
        else
        {
            try
            {
                test();
            }
            catch (Exception e) { printTest(2, $"{e.Message}"); }
        }
        bw.Position = 0;
        br.Position = 0;
        bw.Length = 0;
        br.Length = 0;
    }

    private static void testTyp<T>(Action<T> write, Func<T> read, T value1, T value2)
    {
        testTyp(write, read, value1);
        testTyp(write, read, value2);
    }
    private static void testTyp<T>(Action<T> write, Func<T> read, T input)
    {
        string typ = typeof(T).Name;
        test("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Position = 0;
            write(input);
            bw.Position = 0;
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
            bw.Position = 0;
            bw.Write(input);
            var size = bw.Position;
            bw.Position = 0;
            T result = br.Read<T>();
            if (result.Equals(input)) printTest(0, size + "b");
            else printTest(1, "" + result);
        });
    }
    private static void testSTyp<T>(T input)
    {
        string typ = typeof(T).Name;
        test("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Position = 0;
            bw.Serialize(input);
            var size = bw.Position;
            bw.Position = 0;
            T result = br.Deserialize<T>();
            if (result.Equals(input)) printTest(0, size + "b");
            else printTest(1, "" + result);
        });
    }
    private static void testArray<T>(Action<T[]> write, Func<T[]> read, T[] input)
    {
        string typ = typeof(T).Name;
        test("read/write " + typ + "[] (" + arrayToString(input) + ")", () =>
        {
            bw.Position = 0;
            write(input);
            bw.Position = 0;
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
            bw.Position = 0;
            bw.WriteArray(input);
            bw.Position = 0;
            T[] result = br.ReadArray<T>();
            if (input.Length != result.Length)
                printTest(1, "length not equal" + input.Length + "!=" + result.Length);
            if (isArrayEqual(input, result)) printTest(0);
            else printTest(1, "array(" + arrayToString(result) + ")");
        });
    }

    private static void testMap(int size, bool compressed)
    {
        for (int it = 0; it < 6; it++)
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

            test($"save map {size}x{size}", () =>
            {

                using (var binaryView = new BinaryViewWriter("test.dat"))
                {
                    if (compressed)
                        binaryView.CompressAll();
                    binaryView.WriteString("map");
                    binaryView.WriteInt32(size);
                    binaryView.WriteSingle(0.45f);
                    binaryView.WriteArray(mapLayer1);
                    binaryView.WriteArray(mapLayer2);
                    binaryView.WriteArray(mapLayer3);
                }
                
                printTest(0, new FileInfo("test.dat").Length + "b");

            });
            test($"load map {size}x{size}", () =>
            {
                bool result = true;
                using (var binaryView = new BinaryViewReader("test.dat"))
                {
                    if (compressed)
                        binaryView.DecompressAll();
                    result &= binaryView.ReadString() == "map";
                    result &= binaryView.ReadInt32() == size;
                    result &= binaryView.ReadSingle() == 0.45f;
                    result &= isArrayEqual(mapLayer1, binaryView.ReadArray<byte>());
                    result &= isArrayEqual(mapLayer2, binaryView.ReadArray<byte>());
                    result &= isArrayEqual(mapLayer3, binaryView.ReadArray<byte>());
                }
                if (result) printTest(0);
                else printTest(1);
            });
            size *= 2;
        }
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

