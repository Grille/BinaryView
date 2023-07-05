using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GGL.IO;
using System.Diagnostics;
using System.Threading;
using System.Text;
using BinaryView_Tests.Framework;

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

    public static void WriteReadString(string str, LengthPrefix lengthPrefix, StringLengthMode lengthMode, Encoding encoding, bool expectException = false)
    {
        var data = new TestData();
        var bw = data.Writer;

        TestSys.RunTest($"read/write string[{encoding.BodyName}].length:{lengthPrefix} {lengthMode} ({str})", () =>
        {
            int postcheck = 42327856;
            using (var bw = data.Writer)
            {
                bw.ValidateEncoding = true;
                bw.Position = 0;
                bw.StringLengthMode = lengthMode;

                TestSys.ExpectException<ArgumentException>(expectException, () =>
                {
                    bw.WriteString(str, lengthPrefix, encoding);

                    bw.WriteInt32(postcheck);
                });

                TestSys.Write($"l{str.Length} b{bw.Position} ");
            }

            using (var br = data.Reader)
            {
                br.Position = 0;
                br.StringLengthMode = lengthMode;
                string result = br.ReadString(lengthPrefix, encoding);

                TestSys.AssertValueIsEqual(result, str);

                TestSys.AssertValueIsEqual(br.ReadInt32(), postcheck);
            }

            TestSys.Succes();
            return TestResult.Success;
        });

        data.Dispose();
    }

    public static void WriteReadCString(string str, Encoding encoding, bool expectException = false)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        bw.ValidateEncoding = false;
        bw.ValidateTerminatedString = true;

        TestSys.RunTest($"read/write c-string[{encoding.BodyName}] ({str})", () =>
        {
            bw.Position = 0;

            TestSys.ExpectException<ArgumentException>(expectException, () =>
            {
                bw.WriteTerminatedString(str, encoding);
            });

            TestSys.Write($"l{str.Length} b{bw.Position} ");
            bw.Position = 0;
            string result = br.ReadTerminatedString(encoding);

            TestSys.AssertValueIsEqual(result, str);

            TestSys.Succes();
            return TestResult.Success;
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

            TestSys.AssertBitsMatchStream(mask, data.Stream, out string cmpmask);

            data.ResetPos();

            T result = br.Read<T>();
            int rSize = data.PopPos();

            TestSys.AssertValueIsEqual(rSize, wSize);

            data.ResetPos();

            TestSys.AssertValueIsEqual(result, input);

            TestSys.Succes($"OK {wSize}b {cmpmask}");
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
    public static void WriteReadStringArray(string typeName, string[] input, LengthPrefix lengthPrefix = LengthPrefix.Default)
    {
        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        var writeInfo = typeof(BinaryViewWriter).GetMethod($"Write{typeName}");
        var write = (Action<string[], LengthPrefix, LengthPrefix>)writeInfo.CreateDelegate(typeof(Action<string[], LengthPrefix, LengthPrefix>), bw);

        var readInfo = typeof(BinaryViewReader).GetMethod($"Read{typeName}");
        var read = (Func<LengthPrefix, LengthPrefix, string[]>)readInfo.CreateDelegate(typeof(Func<LengthPrefix, LengthPrefix, string[]>), br);

        string typ = typeof(string).Name;
        TestSys.RunTest($"read/write {typ}[].length:{lengthPrefix} ({TestSys.IListToString(input)})", () =>
        {
            write(input, lengthPrefix, lengthPrefix);
            data.ResetPos();
            string[] result = read(lengthPrefix, lengthPrefix);
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
        using var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        bw.ValidateLengthPrefix = true;

        TestSys.RunTest($"read/write prefix:{lengthPrefix}({value})", () =>
        {
            bw.Position = 0;

            TestSys.ExpectException<InvalidCastException>(expectException, () =>
            {
                bw.WriteLengthPrefix(lengthPrefix, value);
            });

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
    }

    public static void WriteReadUnsafePrefix(LengthPrefix lengthPrefix, long value, long expectedValue, int expectedSize)
    {
        using var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        bw.ValidateLengthPrefix = false;

        TestSys.RunTest($"read/write prefix:{lengthPrefix}({value})", () =>
        {
            bw.Position = 0;

            bw.WriteLengthPrefix(lengthPrefix, value);

            if (bw.Position != expectedSize)
            {
                TestSys.WriteFail($"FAIL size not equal {bw.Position}!={expectedSize}");
                return TestResult.Failure;
            }

            bw.Position = 0;
            long result = br.ReadLengthPrefix(lengthPrefix);
            if (expectedValue != result)
            {
                TestSys.WriteFail($"FAIL length not equal {expectedValue}!={result}");
                return TestResult.Failure;
            }
            TestSys.WriteSucces($"OK size:{expectedSize} original:{value} value:{result}");
            return TestResult.Success;
        });
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

