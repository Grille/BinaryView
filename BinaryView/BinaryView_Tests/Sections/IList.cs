
namespace BinaryView_Tests;
partial class Section
{
    public static void IList()
    {
        Section("test IList");

        int size = 8;

        Random rnd = new Random(1);

        byte[] data0 = new byte[size];
        for (int i = 0; i < size; i++)
            data0[i] = (byte)(rnd.NextDouble() * 255f);

        Test("Read new List", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0);
            test.ResetPos();

            //read

            var list = new byte[data0.Length];
            br.ReadToIList(list);

            AssertIListIsEqual(data0, list);


            Succes();
        });

        Test("Read to new List", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0);
            test.ResetPos();

            //read
            var list = new List<byte>();
            br.ReadToIList(list);

            AssertIListIsEqual(data0, list);


            Succes();
        });

        Test("Read no Prefix", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0, LengthPrefix.None);
            test.ResetPos();

            //read
            var list = new List<byte>();
            br.ReadToIList(list, size - 2);
            list.Add(br.ReadByte());
            list.Add(br.ReadByte());

            AssertIListIsEqual(data0, list);


            Succes();
        });

        Test("Read Remainder", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0, LengthPrefix.None);
            test.ResetPos();

            //read
            var list = new List<byte>();
            br.ReadRemainderToIList(list, 0);
            br.Dispose();

            AssertIListIsEqual(data0, list);


            Succes();
        });

        Test("Read Remainder", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0, LengthPrefix.None);
            test.ResetPos();

            //read
            var list = new List<byte>();

            list.Add(br.ReadByte());
            list.Add(br.ReadByte());

            br.ReadRemainderToIList(list, 2);

            AssertIListIsEqual(data0, list);


            Succes();
        });

        Test("Read Remainder Mod", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteByte(172);
            bw.WriteUInt16(5303);
            bw.WriteUInt16(12925);
            bw.WriteByte(211);
            test.ResetPos();

            //read
            var rdata1 = new List<ushort>();
            byte rdata0 = br.ReadByte();
            br.ReadRemainderToIList(rdata1, 2);
            byte rdata2 = br.ReadByte();

            AssertIsEqual(rdata0, 172);
            AssertIListIsEqual(rdata1, new ushort[] { 5303, 12925 });
            AssertIsEqual(rdata2, 211);

            Succes();
        });

        Test("Read out of bounds", () => {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            var list = new List<int>();

            AssertThrows<EndOfStreamException>(() =>
            {
                br.ReadToIList(list, 1000000);
            });
        });
    }
}
