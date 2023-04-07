using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GGL.IO;
using System.Diagnostics;
using System.Threading;

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
        TestSys.RunTest("read/write " + typ + " (" + input + ")", () =>
        {
            data.Seek(0);
            write(input);
            data.Seek(0);
            T result = read();
            if (result.Equals(input))
            {
                TestSys.WriteSucces("OK");
                return TestResult.Success;
            }

            else
            {
                TestSys.WriteFail($"{result}");
                return TestResult.Failure;
            }
        });

        data.Dispose();
    }

    public static void WriteReadString(string str, LengthPrefix lengthPrefix, Encoding encoding)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        TestSys.RunTest($"read/write string[{encoding.BodyName}].length:{lengthPrefix} ({str})", () =>
        {
            bw.Position = 0;
            bw.WriteString(str, lengthPrefix, encoding);
            TestSys.Write($"l{str.Length} b{bw.Position} ");
            bw.Position = 0;
            string result = br.ReadString(lengthPrefix, encoding);
            if (result.Equals(str))
            {
                TestSys.WriteSucces("OK");
                return TestResult.Success;
            }

            else
            {
                TestSys.WriteFail($"FAIL \"{result}\"");
                return TestResult.Failure;
            }
        });

        data.Dispose();
    }

    public static void WriteReadCString(string str, Encoding encoding)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        TestSys.RunTest($"read/write c-string[{encoding.BodyName}] ({str})", () =>
        {
            bw.Position = 0;
            bw.WriteTerminatedString(str, encoding);
            TestSys.Write($"l{str.Length} b{bw.Position} ");
            bw.Position = 0;
            string result = br.ReadTerminatedString(encoding);
            if (result.Equals(str))
            {
                TestSys.WriteSucces("OK");
                return TestResult.Success;
            }

            else
            {
                TestSys.WriteFail($"FAIL \"{result}\"");
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
        TestSys.RunTest($"endianness {typ} ({input}) {endianness}", () =>
        {
            bw.Write(input);
            int wSize = data.PopPos();

            if (!TestSys.MatchBitsInStream(mask, data.Stream, out string cmpmask))
            {
                TestSys.WriteFail($"FAIL w-bits:'{cmpmask}'");
                return TestResult.Failure;
            }
            data.ResetPos();

            T result = br.Read<T>();
            int rSize = data.PopPos();

            if (wSize != rSize)
            {
                TestSys.WriteFail($"FAIL w:{wSize} != r:{rSize}");
                return TestResult.Failure;
            }
            data.ResetPos();
            if (!result.Equals(input))
            {
                TestSys.WriteFail($"FAIL r-v:'{result}'");
                return TestResult.Failure;
            }

            TestSys.WriteSucces($"OK {wSize}b {cmpmask}");
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
        TestSys.RunTest("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Write(input);
            int wSize = data.PopPos();

            if (!TestSys.MatchBitsInStream(input, data.Stream, out string mask))
            {
                TestSys.WriteFail($"FAIL w-bits:'{mask}'");
                return TestResult.Failure;
            }
            data.ResetPos();

            T result = br.Read<T>();
            int rSize = data.PopPos();

            if (wSize != rSize)
            {
                TestSys.WriteFail($"FAIL w:{wSize} != r:{rSize}");
                return TestResult.Failure;
            }
            if (!TestSys.MatchBitsInStream(result, data.Stream, out mask))
            {
                TestSys.WriteFail($"FAIL r-bits:'{mask}'");
                return TestResult.Failure;
            }
            data.ResetPos();
            if (!result.Equals(input))
            { 
                TestSys.WriteFail($"FAIL r-v:'{result}'");
                return TestResult.Failure;
            }

            TestSys.WriteSucces($"OK {wSize}b");
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
        TestSys.RunTest("read/write " + typ + " (" + input + ")", () =>
        {
            bw.Serialize(input);
            var size = data.PopPos();
            T result = br.Deserialize<T>();
            if (result.Equals(input))
            {
                TestSys.WriteSucces($"OK {size}b");
                return TestResult.Success;
            }
            else
            {
                TestSys.WriteFail($"{result}");
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
        TestSys.RunTest("read/write " + typ + "[] (" + TestSys.IListToString(input) + ")", () =>
        {
            write(input);
            data.ResetPos();
            T[] result = read();
            if (input.Length != result.Length)
            {
                TestSys.WriteFail($"FAIL length not equal{input.Length}!={result.Length}");
                return TestResult.Failure;
            }
            if (TestSys.IsIListEqual(input, result))
            {
                TestSys.WriteSucces($"OK");
                return TestResult.Success;
            }
            else
            {
                TestSys.WriteFail($"FAIL array({TestSys.IListToString(result)})");
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
        TestSys.RunTest($"read/write {typ}[].length:{lengthPrefix} ({TestSys.IListToString(input)})", () =>
        {
            bw.Position = 0;
            bw.WriteArray(input, lengthPrefix);
            bw.Position = 0;
            T[] result = br.ReadArray<T>(lengthPrefix);
            if (input.Length != result.Length)
            {
                TestSys.WriteFail($"FAIL length not equal {input.Length}!={result.Length}");
                return TestResult.Failure;
            }
            if (TestSys.IsIListEqual(input, result))
            {
                TestSys.WriteSucces($"OK");
                return TestResult.Success;
            }
            else
            {
                TestSys.WriteFail($"FAIL array({TestSys.IListToString(result)})");
                return TestResult.Failure;
            }
        });

        data.Dispose();
    }

    public static void WriteReadPrefix(LengthPrefix lengthPrefix, long value, int expectedSize, bool expectException = false)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        TestSys.RunTest($"read/write prefix:{lengthPrefix}({value})", () =>
        {
            bw.Position = 0;
            try
            {
                bw.WriteLengthPrefix(lengthPrefix, value);
            }
            catch (Exception err)
            {
                if (expectException)
                {
                    TestSys.WriteSucces($"OK {err.Message}");
                    return TestResult.Success;
                }
                throw;
            }
            if (expectException)
            {
                TestSys.WriteFail($"FAIL expected Exception not thrown");
                return TestResult.Failure;
            }
            if (bw.Position != expectedSize)
            {
                TestSys.WriteFail($"FAIL size not equal {bw.Position}!={expectedSize}");
                return TestResult.Failure;
            }
            bw.Position = 0;
            long result = br.ReadLengthPrefix(lengthPrefix);
            if (value != result)
            {
                TestSys.WriteFail($"FAIL length not equal {value}!={result}");
                return TestResult.Failure;
            }
            TestSys.WriteSucces($"OK size:{expectedSize} value:{result}");
            return TestResult.Success;
        });

        data.Dispose();
    }

    public static void WriteReadMap(int size, bool compressed)
    {
        for (int it = 0; it < 6; it++)
        {
            var map = new Map(size);

            TestSys.RunTest($"save map {(compressed ? "c" : "u")} {size}x{size}", () =>
            {

                using (var binaryView = new BinaryViewWriter("test.dat"))
                {
                    if (compressed)
                    {
                        binaryView.CompressAll(CompressionType.Deflate);
                    }
                    binaryView.WriteString(map.Name);
                    binaryView.WriteInt32(map.Size);
                    binaryView.WriteSingle(map.Float);
                    binaryView.WriteArray(map.Layer0);
                    binaryView.WriteArray(map.Layer1);
                    binaryView.WriteArray(map.Layer2);
                }

                TestSys.WriteSucces($"OK {TestSys.ElapsedMilliseconds}ms {new FileInfo("test.dat").Length}b");
                return TestResult.Success;

            });
            TestSys.RunTest($"load map {(compressed ? "c" : "u")} {size}x{size}", () =>
            {
                bool result = true;
                using (var binaryView = new BinaryViewReader("test.dat"))
                {
                    if (compressed)
                    {
                        binaryView.CompressAll(CompressionType.Deflate);
                    }
                    result &= binaryView.ReadString() == map.Name;
                    result &= binaryView.ReadInt32() == map.Size;
                    result &= binaryView.ReadSingle() == map.Float;
                    result &= TestSys.IsIListEqual(map.Layer0, binaryView.ReadArray<byte>());
                    result &= TestSys.IsIListEqual(map.Layer1, binaryView.ReadArray<byte>());
                    result &= TestSys.IsIListEqual(map.Layer2, binaryView.ReadArray<byte>());
                }
                if (result)
                {
                    TestSys.WriteSucces($"OK {TestSys.ElapsedMilliseconds}ms");
                    return TestResult.Success;
                }
                else
                {
                    TestSys.WriteFail($"FAIL {TestSys.ElapsedMilliseconds}ms");
                    return TestResult.Failure;
                }
            });
            size *= 2;
        }
    }

    public static void ViewMap(int size, bool compressed)
    {
        for (int it = 0; it < 6; it++)
        {
            var map = new Map(size);

            void view(BinaryView view, Map map)
            {
                if (compressed)
                {
                    view.CompressAll(CompressionType.Deflate);
                }
                view.String(ref map.Name);
                view.Int32(ref map.Size);
                view.Single(ref map.Float);
                view.Array(ref map.Layer0);
                view.Array(ref map.Layer1);
                view.Array(ref map.Layer2);
            }

            TestSys.RunTest($"view-save map {(compressed ? "c" : "u")} {size}x{size}", () =>
            {
                using (var binaryView = new BinaryViewWriter("test.dat"))
                {
                    view(binaryView, map);
                }

                TestSys.WriteSucces($"OK {TestSys.ElapsedMilliseconds}ms {new FileInfo("test.dat").Length}b");
                return TestResult.Success;

            });
            TestSys.RunTest($"view-load map {(compressed ? "c" : "u")} {size}x{size}", () =>
            {
                bool result = true;
                using (var binaryView = new BinaryViewReader("test.dat"))
                {
                    var rmap = new Map();
                    view(binaryView, rmap);

                    result &= rmap.Name == map.Name;
                    result &= rmap.Size== map.Size;
                    result &= rmap.Float == map.Float;
                    result &= TestSys.IsIListEqual(map.Layer0, map.Layer0);
                    result &= TestSys.IsIListEqual(map.Layer1, map.Layer1);
                    result &= TestSys.IsIListEqual(map.Layer2, map.Layer2);
                }
                if (result)
                {
                    TestSys.WriteSucces($"OK {TestSys.ElapsedMilliseconds}ms");
                    return TestResult.Success;
                }
                else
                {
                    TestSys.WriteFail($"FAIL {TestSys.ElapsedMilliseconds}ms");
                    return TestResult.Failure;
                }
            });
            size *= 2;
        }
    }

    public static void Benchmark(string msg, Action setup, Action bench)
    {
        TestSys.RunTest($"load", () =>
        {
            var watch = new Stopwatch();

            setup();

            for (int i = 0; i < 10_000_000; i++)
            {
                bench();
            }
            Thread.Sleep(500);

            watch.Start();
            for (int i = 0; i < 10_000_000; i++)
            {
                bench();
            }
            watch.Stop();

            TestSys.WriteSucces($"OK {msg} {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });
    }
}

