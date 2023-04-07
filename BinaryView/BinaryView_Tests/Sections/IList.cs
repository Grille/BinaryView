
namespace BinaryView_Tests;
partial class Section
{
    public static void IList()
    {
        TestSys.WriteTitle("test IList");

        int size = 8;

        Random rnd = new Random(1);

        byte[] data0 = new byte[size];
        for (int i = 0; i < size; i++)
            data0[i] = (byte)(rnd.NextDouble() * 255f);

        TestSys.RunTest("Read to new List", () =>
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

            TestSys.AssertIListIsEqual(data0, list);


            TestSys.WriteSucces($"OK");
            return TestResult.Success;
        });

        TestSys.RunTest("Read no Prefix", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0, LengthPrefix.None);
            test.ResetPos();

            //read
            var list = new List<byte>();
            br.ReadToIList(list, 0, size - 2);
            list.Add(br.ReadByte());
            list.Add(br.ReadByte());

            TestSys.AssertIListIsEqual(data0, list);


            TestSys.WriteSucces($"OK");
            return TestResult.Success;
        });

        TestSys.RunTest("Read Remainder", () =>
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

            TestSys.AssertIListIsEqual(data0, list);


            TestSys.WriteSucces($"OK");
            return TestResult.Success;
        });

        TestSys.RunTest("Read Remainder", () =>
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

            TestSys.AssertIListIsEqual(data0, list);


            TestSys.WriteSucces($"OK");
            return TestResult.Success;
        });

        TestSys.RunTest("Read Remainder Mod", () =>
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

            TestSys.AssertValueIsEqual(rdata0, 172);
            TestSys.AssertIListIsEqual(rdata1, new ushort[] { 5303, 12925 });
            TestSys.AssertValueIsEqual(rdata2, 211);

            TestSys.WriteSucces($"OK");
            return TestResult.Success;
        });
    }
}
