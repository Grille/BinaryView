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

        Test($"read/write {typeof(T).Name} ({input})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            var write = ReflectionUtils.CreateDelegate<BinaryViewWriter, Action<T>>(bw, $"Write{typeName}");
            var read = ReflectionUtils.CreateDelegate<BinaryViewReader, Func<T>>(br, $"Read{typeName}");

            data.Seek(0);
            write(input);
            data.Seek(0);
            T result = read();
            if (result.Equals(input))
            {
                Succes();
            }

            else
            {
                Fail($"{result}");
            }
        });
    }

    public static void WriteReadString(string str, LengthPrefix lengthPrefix, StringLengthMode lengthMode, Encoding encoding, bool expectException = false)
    {

        Test($"read/write string[{encoding.BodyName}].length:{lengthPrefix} {lengthMode} ({str})", () =>
        {
            using var data = new TestData();

            int postcheck = 42327856;
            using (var bw = data.Writer)
            {
                bw.ValidateEncoding = true;
                bw.Position = 0;
                bw.StringLengthMode = lengthMode;

                void Func()
                {
                    bw.WriteString(str, lengthPrefix, encoding);
                    bw.WriteInt32(postcheck);
                }

                if (expectException)
                {
                    AssertThrows<ArgumentException>(Func);
                }
                else
                {
                    Func();
                }

                //TestSys.Write($"l{str.Length} b{bw.Position} ");
            }

            using (var br = data.Reader)
            {
                br.Position = 0;
                br.StringLengthMode = lengthMode;
                string result = br.ReadString(lengthPrefix, encoding);

                AssertIsEqual(result, str);

                AssertIsEqual(br.ReadInt32(), postcheck);
            }

            Succes();
        });
    }

    public static void WriteReadCString(string str, Encoding encoding, bool expectException = false)
    {
        Test($"read/write c-string[{encoding.BodyName}] ({str})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            bw.ValidateEncoding = false;
            bw.ValidateTerminatedString = true;

            bw.Position = 0;

            void Func() => bw.WriteTerminatedString(str, encoding);
            if (expectException)
            {
                AssertThrows<ArgumentException>(Func);
            }
            else
            {
                Func();
            }

            var msg = $"l{str.Length} b{bw.Position}";
            bw.Position = 0;
            string result = br.ReadTerminatedString(encoding);

            AssertIsEqual(result, str);

            Succes(msg);
        });
    }

    public static void Endianness<T>(string pattern, T input, T mask) where T : unmanaged
    {
        Grille.ConsoleTestLib.GlobalTestSystem.Test($"endianness {typeof(T).Name} ({input}) {pattern}", (Action)(() =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            var endianness = pattern[0] == 'L' ? GGL.IO.Endianness.LittleEndian : GGL.IO.Endianness.BigEndian;
            bool reverseBits = pattern[1] == '1' ? true : false;

            bw.Endianness = br.Endianness = endianness;
            bw.ReverseBitsPerByte = br.ReverseBitsPerByte = reverseBits;


            bw.Write(input);
            int wSize = data.PopPos();

            AssertMatchBits(mask, data.Stream, out string cmpmask);

            data.ResetPos();

            var result = br.Read<T>();
            int rSize = data.PopPos();

            Grille.ConsoleTestLib.UsingSyntaxAsserts.AssertIsEqual(rSize, wSize);

            data.ResetPos();

            Grille.ConsoleTestLib.UsingSyntaxAsserts.AssertIsEqual(result, input);

            Grille.ConsoleTestLib.TestResult.Succes($"{wSize}b {cmpmask}");
        }));
    }

    public static void WriteReadGeneric<T>(T input) where T : unmanaged
    {
        Test($"read/write {typeof(T).Name} ({input})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            bw.Write(input);
            int wSize = data.PopPos();

            AssertMatchBits(input, data.Stream);
            data.ResetPos();

            T result = br.Read<T>();
            int rSize = data.PopPos();

            if (wSize != rSize)
            {
                Fail($"w:{wSize} != r:{rSize}");
            }
            AssertMatchBits(result, data.Stream);

            data.ResetPos();
            if (!result.Equals(input))
            { 
                Fail($"r-v:'{result}'");
            }

            Succes($"{wSize}b");
        });
    }
    public static void WriteReadGeneric<T>(T value1, T value2) where T : unmanaged
    {
        WriteReadGeneric(value1);
        WriteReadGeneric(value2);
    }

    public static void WriteReadSerializable<T>(T input)
    {
        Test($"read/write {typeof(T).Name} ({input})", () =>
        {
            Warn("Obsolete");
            /*
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;


            bw.Serialize(input);
            var size = data.PopPos();
            T result = br.Deserialize<T>();
            if (result.Equals(input))
            {
                Succes($"{size}b");
            }
            else
            {
                Fail($"{result}");
            }
            */
        });
    }
    public static void WriteReadStringArray(string typeName, string[] input, LengthPrefix lengthPrefix = LengthPrefix.Int32)
    {
        Test($"read/write {typeof(string).Name}[].length:{lengthPrefix} ({MessageUtils.IListToString(input)})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            bw.LengthPrefix = lengthPrefix;
            br.LengthPrefix = lengthPrefix;

            var write = ReflectionUtils.CreateDelegate<BinaryViewWriter, Action<string[]>>(bw, $"Write{typeName}");
            var read = ReflectionUtils.CreateDelegate<BinaryViewReader, Func<string[]>>(br, $"Read{typeName}");

            write(input);
            data.ResetPos();
            string[] result = read();
            if (input.Length != result.Length)
            {
                Fail($"length not equal{input.Length}!={result.Length}");
            }

            AssertIListIsEqual(input, result);  
        });
    }
    public static void WriteReadGenericArray<T>(T[] input, LengthPrefix lengthPrefix = LengthPrefix.Int32) where T : unmanaged
    {
        Test($"read/write {typeof(T).Name}[].length:{lengthPrefix} ({MessageUtils.IListToString(input)})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;


            bw.Position = 0;
            bw.WriteArray(input, lengthPrefix);
            bw.Position = 0;
            T[] result = br.ReadArray<T>(lengthPrefix);
            if (input.Length != result.Length)
            {
                Fail($"length not equal {input.Length}!={result.Length}");
            }
            AssertIListIsEqual(input, result);
        });
    }

    public static void WriteReadPrefix(LengthPrefix lengthPrefix, long value, int expectedSize, bool expectException = false)
    {
        Test($"read/write prefix:{lengthPrefix}({value})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            bw.ValidateLengthPrefix = true;

            data.ResetPos();

            void Func() => bw.WriteLengthPrefix(value, lengthPrefix);
            if (expectException)
            {
                AssertThrows<InvalidCastException>(Func);
            }
            else
            {
                Func();
            }

            AssertIsEqual(expectedSize, bw.Position, "Size");

            data.ResetPos();

            long result = br.ReadLengthPrefix(lengthPrefix);
            AssertMatchBits(value, result);

            Succes($"size:{expectedSize} value:{result}");
        });
    }

    public static void WriteReadUnsafePrefix(LengthPrefix lengthPrefix, long value, long expectedValue, int expectedSize)
    {
        Test($"read/write prefix:{lengthPrefix}({value})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            bw.ValidateLengthPrefix = false;

            data.ResetPos();
            bw.WriteLengthPrefix(value, lengthPrefix);

            AssertIsEqual(expectedSize, bw.Position, "Size");

            data.ResetPos();
            long result = br.ReadLengthPrefix(lengthPrefix);
            AssertMatchBits(expectedValue, result);

            Succes($"size:{expectedSize} original:{value} value:{result}");
        });
    }

    public static void WriteReadCustomPrefix(ILengthPrefix lengthPrefix, long value, long expectedValue, int expectedSize)
    {
        Test($"read/write prefix:{LengthPrefix.Custom}:{lengthPrefix.GetType().Name}({value})", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            bw.CustomLengthPrefixHandler = lengthPrefix;
            br.CustomLengthPrefixHandler = lengthPrefix;

            data.ResetPos();
            bw.WriteLengthPrefix(value, LengthPrefix.Custom);

            AssertIsEqual(expectedSize, bw.Position, "Size");

            data.ResetPos();
            long result = br.ReadLengthPrefix(LengthPrefix.Custom);
            AssertMatchBits(expectedValue, result);

            Succes($"size:{expectedSize} original:{value} value:{result}");
        });
    }

    public static void WriteReadMap(int size, bool compressed)
    {
        for (int it = 0; it < 6; it++)
        {
            var map = new Map(size);

            Test($"save map {(compressed ? "c" : "u")} {size}x{size}", (ctx) =>
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

                Succes($"OK {ctx.ElapsedMilliseconds}ms {new FileInfo("test.dat").Length}b");

            });
            Test($"load map {(compressed ? "c" : "u")} {size}x{size}", (ctx) =>
            {
                bool result = true;
                using (var binaryView = new BinaryViewReader("test.dat"))
                {
                    if (compressed)
                    {
                        binaryView.DecompressAll(CompressionType.Deflate);
                    }
                    result &= binaryView.ReadString() == map.Name;
                    result &= binaryView.ReadInt32() == map.Size;
                    result &= binaryView.ReadSingle() == map.Float;
                    result &= CompareUtils.IsIListEqual(map.Layer0, binaryView.ReadArray<byte>());
                    result &= CompareUtils.IsIListEqual(map.Layer1, binaryView.ReadArray<byte>());
                    result &= CompareUtils.IsIListEqual(map.Layer2, binaryView.ReadArray<byte>());
                }
                if (result)
                {
                    Succes($"OK {ctx.ElapsedMilliseconds}ms");
                }
                else
                {
                    Fail($"FAIL {ctx.ElapsedMilliseconds}ms");
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
                    view.DeCompressAll(CompressionType.Deflate);
                }
                view.String(ref map.Name);
                view.Int32(ref map.Size);
                view.Single(ref map.Float);
                view.Array(ref map.Layer0);
                view.Array(ref map.Layer1);
                view.Array(ref map.Layer2);
            }

            Test($"view-save map {(compressed ? "c" : "u")} {size}x{size}", (ctx) =>
            {
                using (var binaryView = new BinaryViewWriter("test.dat"))
                {
                    view(binaryView, map);
                }

                Succes($"OK {ctx.ElapsedMilliseconds}ms {new FileInfo("test.dat").Length}b");

            });
            Test($"view-load map {(compressed ? "c" : "u")} {size}x{size}", (ctx) =>
            {
                bool result = true;
                using (var binaryView = new BinaryViewReader("test.dat"))
                {
                    var rmap = new Map();
                    view(binaryView, rmap);

                    result &= rmap.Name == map.Name;
                    result &= rmap.Size== map.Size;
                    result &= rmap.Float == map.Float;
                    result &= CompareUtils.IsIListEqual(map.Layer0, map.Layer0);
                    result &= CompareUtils.IsIListEqual(map.Layer1, map.Layer1);
                    result &= CompareUtils.IsIListEqual(map.Layer2, map.Layer2);
                }
                if (result)
                {
                    Succes($"OK {ctx.ElapsedMilliseconds}ms");
                }
                else
                {
                    Fail($"FAIL {ctx.ElapsedMilliseconds}ms");
                }
            });
            size *= 2;
        }
    }

    public static void Benchmark(string msg, Action setup, Action bench)
    {
        Test($"load", () =>
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

            Succes($"OK {msg} {watch.Elapsed.TotalMilliseconds}ms");
        });
    }
}

