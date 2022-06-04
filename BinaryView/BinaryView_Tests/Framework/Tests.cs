using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GGL.IO;
using System.Diagnostics;

namespace BinaryView_Tests;

static class Tests
{
    public static void WriteReadPrimitive<T>(string typeName, T value1, T value2)
    {
        testTyp(typeName, value1);
        testTyp(typeName, value2);
    }
    public static void testTyp<T>(string typeName, T input)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        var writeInfo = typeof(BinaryViewWriter).GetMethod($"Write{typeName}");
        var write = (Action<T>)writeInfo.CreateDelegate(typeof(Action<T>),bw);

        var readInfo = typeof(BinaryViewReader).GetMethod($"Read{typeName}");
        var read = (Func<T>)readInfo.CreateDelegate(typeof(Func<T>), br);

        string typ = typeof(T).Name;
        TUtils.RunTest("read/write " + typ + " (" + input + ")", () =>
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

        data.Dispose();
    }

    public static void WriteReadString(string str, LengthPrefix lengthPrefix = LengthPrefix.Default, CharSizePrefix charSizePrefix = CharSizePrefix.Default)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        TUtils.RunTest($"read/write string[{charSizePrefix}].length:{lengthPrefix} ({str})", () =>
        {
            bw.Position = 0;
            bw.WriteString(str, lengthPrefix, charSizePrefix);
            bw.Position = 0;
            string result = br.ReadString(lengthPrefix, charSizePrefix);
            if (result.Equals(str))
            {
                TUtils.WriteSucces("OK");
                return TestResult.Success;
            }

            else
            {
                TUtils.WriteFail($"FAIL \"{result}\"");
                return TestResult.Failure;
            }
        });

        data.Dispose();
    }

    public static void Endianness<T>(string endianness, T input, T mask) where T : unmanaged
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        Endianness byteOrder = endianness[0] == 'L' ? GGL.IO.Endianness.LittleEndian : GGL.IO.Endianness.BigEndian;
        Endianness bitOrder = endianness[1] == 'L' ? GGL.IO.Endianness.LittleEndian : GGL.IO.Endianness.BigEndian;

        bw.ByteOrder = br.ByteOrder = byteOrder;
        bw.BitOrder = br.BitOrder = bitOrder;

        string typ = typeof(T).Name;
        TUtils.RunTest($"endianness {typ} ({input}) {endianness}", () =>
        {
            bw.Write(input);
            int wSize = data.PopPtr();

            if (!TUtils.MatchBitsInStream(mask, data.Stream, out string cmpmask))
            {
                TUtils.WriteFail($"FAIL w-bits:'{cmpmask}'");
                return TestResult.Failure;
            }
            data.ResetPtr();

            T result = br.Read<T>();
            int rSize = data.PopPtr();

            if (wSize != rSize)
            {
                TUtils.WriteFail($"FAIL w:{wSize} != r:{rSize}");
                return TestResult.Failure;
            }
            data.ResetPtr();
            if (!result.Equals(input))
            {
                TUtils.WriteFail($"FAIL r-v:'{result}'");
                return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK {wSize}b {cmpmask}");
            return TestResult.Success;
        });

        data.Dispose();
    }

    public static void WriteReadGeneric<T>(T input) where T : unmanaged
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        string typ = typeof(T).Name;
        TUtils.RunTest("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Write(input);
            int wSize = data.PopPtr();

            if (!TUtils.MatchBitsInStream(input, data.Stream, out string mask))
            {
                TUtils.WriteFail($"FAIL w-bits:'{mask}'");
                return TestResult.Failure;
            }
            data.ResetPtr();

            T result = br.Read<T>();
            int rSize = data.PopPtr();

            if (wSize != rSize)
            {
                TUtils.WriteFail($"FAIL w:{wSize} != r:{rSize}");
                return TestResult.Failure;
            }
            if (!TUtils.MatchBitsInStream(result, data.Stream, out mask))
            {
                TUtils.WriteFail($"FAIL r-bits:'{mask}'");
                return TestResult.Failure;
            }
            data.ResetPtr();
            if (!result.Equals(input))
            { 
                TUtils.WriteFail($"FAIL r-v:'{result}'");
                return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK {wSize}b");
            return TestResult.Success;
        });

        data.Dispose();
    }
    public static void WriteReadGeneric<T>(T value1, T value2) where T : unmanaged
    {
        WriteReadGeneric(value1);
        WriteReadGeneric(value2);
    }

    public static void WriteReadSerializable<T>(T input)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        string typ = typeof(T).Name;
        TUtils.RunTest("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Serialize(input);
            var size = data.PopPtr();
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

        data.Dispose();
    }
    public static void WriteReadStringArray<T>(string typeName, T[] input, LengthPrefix lengthPrefix = LengthPrefix.Default)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        var writeInfo = typeof(BinaryViewWriter).GetMethod($"Write{typeName}");
        var write = (Action<T[]>)writeInfo.CreateDelegate(typeof(Action<T[]>), bw);

        var readInfo = typeof(BinaryViewReader).GetMethod($"Read{typeName}");
        var read = (Func<T[]>)readInfo.CreateDelegate(typeof(Func<T[]>), br);

        string typ = typeof(T).Name;
        TUtils.RunTest("read/write " + typ + "[] (" + TUtils.IListToString(input) + ")", () =>
        {
            write(input);
            data.ResetPtr();
            T[] result = read();
            if (input.Length != result.Length)
            {
                TUtils.WriteFail($"FAIL length not equal{input.Length}!={result.Length}");
                return TestResult.Failure;
            }
            if (TUtils.IsIListEqual(input, result))
            {
                TUtils.WriteSucces($"OK");
                return TestResult.Success;
            }
            else
            {
                TUtils.WriteFail($"FAIL array({TUtils.IListToString(result)})");
                return TestResult.Failure;
            }
        });

        data.Dispose();
    }
    public static void WriteReadGenericArray<T>(T[] input, LengthPrefix lengthPrefix = LengthPrefix.Int32) where T : unmanaged
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        string typ = typeof(T).Name;
        TUtils.RunTest($"read/write {typ}[].length:{lengthPrefix} ({TUtils.IListToString(input)})", () =>
        {
            bw.Position = 0;
            bw.WriteArray(input, lengthPrefix);
            bw.Position = 0;
            T[] result = br.ReadArray<T>(lengthPrefix);
            if (input.Length != result.Length)
            {
                TUtils.WriteFail($"FAIL length not equal{input.Length}!={result.Length}");
                return TestResult.Failure;
            }
            if (TUtils.IsIListEqual(input, result))
            {
                TUtils.WriteSucces($"OK");
                return TestResult.Success;
            }
            else
            {
                TUtils.WriteFail($"FAIL array({TUtils.IListToString(result)})");
                return TestResult.Failure;
            }
        });

        data.Dispose();
    }

    public static void WriteReadMap(int size, bool compressed)
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

            TUtils.RunTest($"save {(compressed ? "c" : "u")}map {size}x{size}", () =>
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

                TUtils.WriteSucces($"OK {TUtils.ElapsedMilliseconds}ms {new FileInfo("test.dat").Length}b");
                return TestResult.Success;

            });
            TUtils.RunTest($"load {(compressed ? "c" : "u")}map {size}x{size}", () =>
            {
                bool result = true;
                using (var binaryView = new BinaryViewReader("test.dat"))
                {
                    if (compressed)
                        binaryView.DecompressAll();
                    result &= binaryView.ReadString() == "map";
                    result &= binaryView.ReadInt32() == size;
                    result &= binaryView.ReadSingle() == 0.45f;
                    result &= TUtils.IsIListEqual(mapLayer1, binaryView.ReadArray<byte>());
                    result &= TUtils.IsIListEqual(mapLayer2, binaryView.ReadArray<byte>());
                    result &= TUtils.IsIListEqual(mapLayer3, binaryView.ReadArray<byte>());
                }
                if (result)
                {
                    TUtils.WriteSucces($"OK {TUtils.ElapsedMilliseconds}ms");
                    return TestResult.Success;
                }
                else
                {
                    TUtils.WriteFail($"FAIL {TUtils.ElapsedMilliseconds}ms");
                    return TestResult.Failure;
                }
            });
            size *= 2;
        }
    }
}

