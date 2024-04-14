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

        Test($"{Name} Section", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            using (bw.BeginCompressedSection(Type))
            {
                bw.WriteArray(data0);
            }


            AssertVirtualFileWasWriten(data);


            using (br.BeginCompressedSection(Type))
            {
                rdata0 = br.ReadArray<byte>();
                AssertIListIsEqual(data0, rdata0);
            }


            Succes($"{data.Position}b");
        });

        Test($"{Name} Section (non using)", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;

            bw.BeginCompressedSection(Type);
            {
                bw.WriteArray(data0);
            }
            var c = bw.EndCompressedSection();


            AssertVirtualFileWasWriten(data);


            br.BeginCompressedSection(Type);
            {
                rdata0 = br.ReadArray<byte>();
                AssertIListIsEqual(data0, rdata0);
            }
            br.EndCompressedSection();


            Succes($"{data.Position}b");
        });

        Test($"{Name} Empty Section", () =>
        {
            using var data = new TestData();
            var bw = data.Writer;
            var br = data.Reader;


            using (bw.BeginCompressedSection(Type)) { }
            bw.WriteArray(data0);


            AssertVirtualFileWasWriten(data);


            using (br.BeginCompressedSection(Type)) { }
            rdata0 = br.ReadArray<byte>();
            AssertIListIsEqual(data0, rdata0);


            Succes($"{data.Position}b");
        });

        Test($"{Name} 2 Sections Sequential", () =>
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


            AssertVirtualFileWasWriten(data);


            using (br.BeginCompressedSection(Type))
            {
                rdata0 = br.ReadArray<byte>();
                AssertIListIsEqual(data0, rdata0);
            }
            AssertIsNotEndOfStream(data.Stream);

            rdata1 = br.ReadArray<byte>();
            AssertIListIsEqual(data1, rdata1);

            using (br.BeginCompressedSection(Type))
            {
                rdata2 = br.ReadArray<byte>();
                AssertIListIsEqual(data2, rdata2);
            }


            Succes($"{data.Position}b");
        });

        Test($"{Name} 2 Sections Nested", () =>
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


            AssertVirtualFileWasWriten(data);


            rdata0 = br.ReadArray<byte>();
            AssertIListIsEqual(data0, rdata0);
            using (br.BeginCompressedSection(Type))
            {
                rdata1 = br.ReadArray<byte>();
                AssertIListIsEqual(data1, rdata1);
                using (br.BeginCompressedSection(Type))
                {
                    rdata2 = br.ReadArray<byte>();
                    AssertIListIsEqual(data2, rdata2);
                }
            }


            Succes($"{data.Position}b");
        });

        Test($"{Name} All", () =>
        {
            using var data = new TestData();

            using (var bw = data.Writer)
            {
                bw.CompressAll(Type);
                bw.WriteArray(data0);
            }

            AssertVirtualFileWasWriten(data);

            using (var br = data.Reader)
            {
                br.DecompressAll(Type);
                rdata0 = br.ReadArray<byte>();
                AssertIListIsEqual(data0, rdata0);
            }


            Succes($"{data.Position}b");
        });

        Test($"{Name} All after Head", () =>
        {
            using var data = new TestData();

            using (var bw = data.Writer)
            {
                bw.WriteArray(data0);
                bw.CompressAll(Type);
                bw.WriteArray(data1);
            }

            AssertVirtualFileWasWriten(data);

            using (var br = data.Reader)
            {
                rdata0 = br.ReadArray<byte>();
                AssertIListIsEqual(data0, rdata0);
                br.DecompressAll(Type);
                rdata1 = br.ReadArray<byte>();
                AssertIListIsEqual(data1, rdata1);
            }

            Succes($"{data.Position}b");
        });
    }
}
