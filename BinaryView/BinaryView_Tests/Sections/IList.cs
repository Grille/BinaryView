
namespace BinaryView_Tests;
partial class Section
{
    public static void IList()
    {
        TUtils.WriteTitle("test IList");

        int size = 8;

        Random rnd = new Random(1);

        byte[] data0 = new byte[size];
        for (int i = 0; i < size; i++)
            data0[i] = (byte)(rnd.NextDouble() * 255f);

        TUtils.RunTest("Read to new List", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0);
            test.ResetPtr();

            //read
            var list = new List<byte>();
            br.ReadToIList(list);

            if (TUtils.AssertIListIsEqual(data0, list))
                return TestResult.Failure;


            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.RunTest("Read no Prefix", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0, LengthPrefix.None);
            test.ResetPtr();

            //read
            var list = new List<byte>();
            br.ReadToIList(list, 0, size - 2);
            list.Add(br.ReadByte());
            list.Add(br.ReadByte());

            if (TUtils.AssertIListIsEqual(data0, list))
                return TestResult.Failure;


            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.RunTest("Read Remainder", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0, LengthPrefix.None);
            test.ResetPtr();

            //read
            var list = new List<byte>();
            br.ReadRemainderToIList(list, 0);
            br.Dispose();

            if (TUtils.AssertIListIsEqual(data0, list))
                return TestResult.Failure;


            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.RunTest("Read Remainder", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteIList(data0, LengthPrefix.None);
            test.ResetPtr();

            //read
            var list = new List<byte>();

            list.Add(br.ReadByte());
            list.Add(br.ReadByte());

            br.ReadRemainderToIList(list, 2);

            if (TUtils.AssertIListIsEqual(data0, list))
                return TestResult.Failure;


            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.RunTest("Read Remainder Mod", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            //setup
            bw.WriteByte(172);
            bw.WriteUInt16(5303);
            bw.WriteUInt16(12925);
            bw.WriteByte(211);
            test.ResetPtr();

            //read
            var rdata1 = new List<ushort>();
            byte rdata0 = br.ReadByte();
            br.ReadRemainderToIList(rdata1, 2);
            byte rdata2 = br.ReadByte();

            if (TUtils.AssertValueIsEqual(rdata0, 172))
                return TestResult.Failure;

            if (TUtils.AssertIListIsEqual(rdata1, new ushort[] { 5303, 12925 }))
                return TestResult.Failure;

            if (TUtils.AssertValueIsEqual(rdata2, 211))
                return TestResult.Failure;

            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });
    }
}
