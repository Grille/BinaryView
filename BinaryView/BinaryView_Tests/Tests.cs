using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using GGL.IO;

namespace BinaryView_Tests;

static class Tests
{

    static Stream stream;
    static BinaryViewWriter bw;
    static BinaryViewReader br;

    public static void Run()
    {
        stream = new MemoryStream();
        bw = new BinaryViewWriter(stream);
        br = new BinaryViewReader(stream);

        TUtils.WriteTitle("Run tests...\n");

        TUtils.WriteTitle("test types");
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

        TUtils.WriteTitle("test unmanaged types");
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
        testGTyp(new TUtils.Struct() { A = 42, B = 3.6f });
        testGTyp(new DateTime(2020, 07, 20, 15, 54, 24));
        testGTyp(new Point(10, 42));
        testGTyp(new RectangleF(10, 42, 25.5f, 23));

        TUtils.WriteTitle("test serializable types");
        testSTyp(42);
        testSTyp("Hello World");
        testSTyp(new DateTime(2000, 10, 20));
        testSTyp(new DateTime(2020, 07, 20, 15, 54, 24));
        testSTyp(new Point(2000, 10));
        testSTyp(new RectangleF(10, 42, 25.5f, 23));

        TUtils.WriteTitle("test arrays");
        testGArray(new byte[] { 0, 2, 4, 6 });
        testGArray(new int[] { 0, -2, 4, -6 });
        testGArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
        testGArray(new TUtils.Struct[] { new TUtils.Struct() { A = 42, B = 3.6f }, new TUtils.Struct() { A = 36, B = 1.666f } });
        testArray(bw.WriteStringArray, br.ReadStringArray, new string[] { "ab", "cd", "ef", "gh" });

        TUtils.WriteTitle("test compresion");
        testCompression();

        TUtils.WriteTitle("test map");
        testMap(64, false);

        TUtils.WriteTitle("test map compressed");
        testMap(64, true);

        TUtils.WriteResults();
    }


    private static void testTyp<T>(Action<T> write, Func<T> read, T value1, T value2)
    {
        testTyp(write, read, value1);
        testTyp(write, read, value2);
    }
    private static void testTyp<T>(Action<T> write, Func<T> read, T input)
    {
        string typ = typeof(T).Name;
        TUtils.Test("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Position = 0;
            write(input);
            bw.Position = 0;
            T result = read();
            if (result.Equals(input))
            {
                TUtils.WriteSucces("OK");
                return TestResult.Success;
            }

            else
            {
                TUtils.WriteFail($"{result}");
                return TestResult.Failure;
            }
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
        TUtils.Test("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Position = 0;
            bw.Write(input);
            var size = bw.Position;
            bw.Position = 0;
            T result = br.Read<T>();
            if (result.Equals(input))
            {
                TUtils.WriteSucces($"OK {size}b");
                return TestResult.Success;
            }
            else
            {
                TUtils.WriteFail($"{result}");
                return TestResult.Failure;
            }
        });
    }
    private static void testSTyp<T>(T input)
    {
        string typ = typeof(T).Name;
        TUtils.Test("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Position = 0;
            bw.Serialize(input);
            var size = bw.Position;
            bw.Position = 0;
            T result = br.Deserialize<T>();
            if (result.Equals(input))
            {
                TUtils.WriteSucces($"OK {size}b");
                return TestResult.Success;
            }
            else
            {
                TUtils.WriteFail($"{result}");
                return TestResult.Failure;
            }
        });
    }
    private static void testArray<T>(Action<T[]> write, Func<T[]> read, T[] input)
    {
        string typ = typeof(T).Name;
        TUtils.Test("read/write " + typ + "[] (" + TUtils.ArrayToString(input) + ")", () =>
        {
            bw.Position = 0;
            write(input);
            bw.Position = 0;
            T[] result = read();
            if (input.Length != result.Length)
            {
                TUtils.WriteFail($"FAIL length not equal{input.Length}!={result.Length}");
                return TestResult.Failure;
            }
            if (TUtils.IsArrayEqual(input, result))
            {
                TUtils.WriteSucces($"OK");
                return TestResult.Success;
            }
            else
            {
                TUtils.WriteFail($"FAIL array({TUtils.ArrayToString(result)})");
                return TestResult.Failure;
            }
        });
    }
    private static void testGArray<T>(T[] input) where T : unmanaged
    {
        string typ = typeof(T).Name;
        TUtils.Test("read/write " + typ + "[] (" + TUtils.ArrayToString(input) + ")", () =>
        {
            bw.Position = 0;
            bw.WriteArray(input);
            bw.Position = 0;
            T[] result = br.ReadArray<T>();
            if (input.Length != result.Length)
            {
                TUtils.WriteFail($"FAIL length not equal{input.Length}!={result.Length}");
                return TestResult.Failure;
            }
            if (TUtils.IsArrayEqual(input, result))
            {
                TUtils.WriteSucces($"OK");
                return TestResult.Success;
            }
            else
            {
                TUtils.WriteFail($"FAIL array({TUtils.ArrayToString(result)})");
                return TestResult.Failure;
            }
        });
    }

    private static void testCompression()
    {
        int size = 8;

        Random rnd = new Random(1);

        byte[] data0 = new byte[size];
        for (int i = 0; i < size; i++)
            data0[i] = (byte)(rnd.NextDouble() * 255f);
        byte[] data1 = new byte[size];
        for (int i = 0; i < size; i++)
            data1[i] = (byte)(rnd.NextDouble() * 255f);
        byte[] data2 = new byte[size];
        for (int i = 0; i < size; i++)
            data2[i] = (byte)(rnd.NextDouble() * 255f);

        TUtils.Test("Compress All", () =>
        {
            var bw = new BinaryViewWriter();
            bw.CompressAll();
            bw.WriteArray(data0);
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);
            br.DecompressAll();
            var rdata0 = br.ReadArray<byte>();
            br.Dispose();

            if (!TUtils.IsArrayEqual(data0, rdata0))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.ArrayToString(rdata0)}, expected: {TUtils.ArrayToString(data0)}");
                return TestResult.Failure;
            }
            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Compress Section", () =>
        {
            var bw = new BinaryViewWriter();
            bw.BeginDeflateSection();
            bw.WriteArray(data0);
            bw.EndDeflateSection();
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);
            br.BeginDeflateSection();
            var rdata0 = br.ReadArray<byte>();
            br.EndDeflateSection();
            br.Dispose();

            if (!TUtils.IsArrayEqual(data0, rdata0)) 
            { 
                TUtils.WriteFail($"FAIL data: {TUtils.ArrayToString(rdata0)}, expected: {TUtils.ArrayToString(data0)}");
                return TestResult.Failure;
            }
            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Compress 2 Sections", () =>
        {
            var bw = new BinaryViewWriter();
            bw.BeginDeflateSection();
            bw.WriteArray(data0);
            bw.EndDeflateSection();
            bw.WriteArray(data1);
            bw.BeginDeflateSection();
            bw.WriteArray(data2);
            bw.EndDeflateSection();
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);
            br.BeginDeflateSection();
            var rdata0 = br.ReadArray<byte>();
            br.EndDeflateSection();
            var rdata1 = br.ReadArray<byte>();
            br.BeginDeflateSection();
            var rdata2 = br.ReadArray<byte>();
            br.EndDeflateSection();
            br.Dispose();

            if (!TUtils.IsArrayEqual(data0, rdata0))
            {
                TUtils.WriteFail($"FAIL data0: {TUtils.ArrayToString(rdata0)}, expected: {TUtils.ArrayToString(data0)}");
                return TestResult.Failure;
            }
            if (!TUtils.IsArrayEqual(data1, rdata1))
            {
                TUtils.WriteFail($"FAIL data1: {TUtils.ArrayToString(rdata1)}, expected: {TUtils.ArrayToString(data1)}");
                return TestResult.Failure;
            }
            if (!TUtils.IsArrayEqual(data2, rdata2))
            {
                TUtils.WriteFail($"FAIL data2: {TUtils.ArrayToString(rdata2)}, expected: {TUtils.ArrayToString(data2)}");
                return TestResult.Failure;
            }
            TUtils.WriteSucces($"OK");
            return TestResult.Success;
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

            TUtils.Test($"save map {size}x{size}", () =>
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
                
                TUtils.WriteSucces($"OK {new FileInfo("test.dat").Length}b");
                return TestResult.Success;

            });
            TUtils.Test($"load map {size}x{size}", () =>
            {
                bool result = true;
                using (var binaryView = new BinaryViewReader("test.dat"))
                {
                    if (compressed)
                        binaryView.DecompressAll();
                    result &= binaryView.ReadString() == "map";
                    result &= binaryView.ReadInt32() == size;
                    result &= binaryView.ReadSingle() == 0.45f;
                    result &= TUtils.IsArrayEqual(mapLayer1, binaryView.ReadArray<byte>());
                    result &= TUtils.IsArrayEqual(mapLayer2, binaryView.ReadArray<byte>());
                    result &= TUtils.IsArrayEqual(mapLayer3, binaryView.ReadArray<byte>());
                }
                if (result)
                {
                    TUtils.WriteSucces("OK");
                    return TestResult.Success;
                }
                else
                {
                    TUtils.WriteFail("FAIL");
                    return TestResult.Failure;
                }
            });
            size *= 2;
        }
    }


}

