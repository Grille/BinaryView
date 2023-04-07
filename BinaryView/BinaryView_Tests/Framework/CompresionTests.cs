using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryView_Tests;

namespace BinaryView_Tests.Framework;
internal class CompresionTests
{
    public readonly string Name;
    public readonly CompressionType Type;

    public CompresionTests(string name, CompressionType type)
    {
        Name = name;
        Type = type;
    }

    public void Run()
    {
        int size = 32;

        Random rnd = new Random(1);

        byte[] data0 = new byte[size];
        byte[] data1 = new byte[size];
        byte[] data2 = new byte[size];

        for (int i = 0; i < size; i++)
            data0[i] = (byte)(rnd.NextDouble() * 16f);
        for (int i = 0; i < size; i++)
            data1[i] = (byte)(rnd.NextDouble() * 16f);
        for (int i = 0; i < size; i++)
            data2[i] = (byte)(rnd.NextDouble() * 16f);

        byte[] rdata0, rdata1, rdata2;

        TestSys.RunTest($"{Name} Section", () =>
            {
                using var data = new TestData();
                var bw = data.Writer;
                var br = data.Reader;

                using (bw.BeginCompressedSection(Type))
                {
                    bw.WriteArray(data0);
                }


                TestSys.AssertValueIsNotEqual(data.PopPos(), 0, $"FAIL file length is 0");


                using (br.BeginCompressedSection(Type))
                {
                    rdata0 = br.ReadArray<byte>();
                    TestSys.AssertIListIsEqual(data0, rdata0);
                }


                TestSys.Succes($"{data.Position}b");
                return TestResult.Success;
            });

        TestSys.RunTest($"{Name} Empty Section", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;


            using (bw.BeginCompressedSection(Type)) { }
            bw.WriteArray(data0);


            TestSys.AssertValueIsNotEqual(data.PopPos(), 0, $"FAIL file length is 0");


            using (br.BeginCompressedSection(Type)) { }
            rdata0 = br.ReadArray<byte>();
            TestSys.AssertIListIsEqual(data0, rdata0);


            TestSys.Succes($"{data.Position}b");
            return TestResult.Success;
        });

        TestSys.RunTest($"{Name} 2 Sections Sequential", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;


            using (bw.BeginCompressedSection(Type))
            {
                bw.WriteArray(data0);
            }

            bw.WriteArray(data1);

            using (bw.BeginCompressedSection(Type))
            {
                bw.WriteArray(data2);
            }


            TestSys.AssertValueIsNotEqual(data.PopPos(), 0, $"FAIL file length is 0");


            using (br.BeginCompressedSection(Type))
            {
                rdata0 = br.ReadArray<byte>();
                TestSys.AssertIListIsEqual(data0, rdata0);
            }
            TestSys.AssertEndOfStream(data.Stream, false);

            rdata1 = br.ReadArray<byte>();
            TestSys.AssertIListIsEqual(data1, rdata1);

            using (br.BeginCompressedSection(Type))
            {
                rdata2 = br.ReadArray<byte>();
                TestSys.AssertIListIsEqual(data2, rdata2);
            }


            TestSys.Succes($"{data.Position}b");
            return TestResult.Success;
        });

        TestSys.RunTest($"{Name} 2 Sections Nested", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;


            bw.WriteArray(data0);
            using (bw.BeginCompressedSection(Type))
            {
                bw.WriteArray(data1);

                using (bw.BeginCompressedSection(Type))
                {
                    bw.WriteArray(data2);
                }
            }


            TestSys.AssertValueIsNotEqual(data.PopPos(), 0, $"FAIL file length is 0");


            rdata0 = br.ReadArray<byte>();
            TestSys.AssertIListIsEqual(data0, rdata0);
            using (br.BeginCompressedSection(Type))
            {
                rdata1 = br.ReadArray<byte>();
                TestSys.AssertIListIsEqual(data1, rdata1);
                using (br.BeginCompressedSection(Type))
                {
                    rdata2 = br.ReadArray<byte>();
                    TestSys.AssertIListIsEqual(data2, rdata2);
                }
            }


            TestSys.Succes($"{data.Position}b");
            return TestResult.Success;
        });

        TestSys.RunTest($"{Name} All", () =>
        {
            using var data = new TestData();

            using (var bw = data.Writer)
            {
                bw.CompressAll(Type);
                bw.WriteArray(data0);
            }

            TestSys.AssertValueIsNotEqual(data.PopPos(), 0, $"FAIL file length is 0");

            using (var br = data.Reader)
            {
                br.CompressAll(Type);
                rdata0 = br.ReadArray<byte>();
                TestSys.AssertIListIsEqual(data0, rdata0);
            }


            TestSys.Succes($"{data.Position}b");
            return TestResult.Success;
        });

        TestSys.RunTest($"{Name} All after Head", () =>
        {
            using var data = new TestData();

            using (var bw = data.Writer)
            {
                bw.WriteArray(data0);
                bw.CompressAll(Type);
                bw.WriteArray(data1);
            }

            TestSys.AssertValueIsNotEqual(data.PopPos(), 0, $"FAIL file length is 0");

            using (var br = data.Reader)
            {
                rdata0 = br.ReadArray<byte>();
                TestSys.AssertIListIsEqual(data0, rdata0);
                br.CompressAll(Type);
                rdata1 = br.ReadArray<byte>();
                TestSys.AssertIListIsEqual(data1, rdata1);
            }


            TestSys.Succes($"{data.Position}b");
            return TestResult.Success;
        });
    }
}
