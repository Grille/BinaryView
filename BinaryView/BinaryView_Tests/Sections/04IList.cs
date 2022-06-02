
namespace BinaryView_Tests;
partial class Section
{
    public static void S04IList()
    {
        TUtils.WriteTitle("test IList");

        int size = 8;

        Random rnd = new Random(1);

        byte[] data0 = new byte[size];
        for (int i = 0; i < size; i++)
            data0[i] = (byte)(rnd.NextDouble() * 255f);

        TUtils.Test("Read to new List", () =>
        {
            var bw = new BinaryViewWriter();
            bw.WriteIList(data0);
            bw.Dispose();
            var file = bw.ToArray();

            var br = new BinaryViewReader(file);
            var list = new List<byte>();
            br.ReadToIList(list);
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, list))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.IListToString(list)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Read no Prefix", () =>
        {
            var bw = new BinaryViewWriter();
            bw.WriteIList(data0, LengthPrefix.None);
            bw.Dispose();
            var file = bw.ToArray();

            var br = new BinaryViewReader(file);
            var list = new List<byte>();
            br.ReadToIList(list, 0, size - 2);
            list.Add(br.ReadByte());
            list.Add(br.ReadByte());
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, list))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.IListToString(list)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Read Remainder", () =>
        {
            var bw = new BinaryViewWriter();
            bw.WriteIList(data0, LengthPrefix.None);
            bw.Dispose();
            var file = bw.ToArray();

            var br = new BinaryViewReader(file);
            var list = new List<byte>();
            br.ReadRemainderToIList(list, 0);
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, list))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.IListToString(list)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Read Remainder", () =>
        {
            var bw = new BinaryViewWriter();
            bw.WriteIList(data0, LengthPrefix.None);
            bw.Dispose();
            var file = bw.ToArray();

            var br = new BinaryViewReader(file);
            var list = new List<byte>();


            list.Add(br.ReadByte());
            list.Add(br.ReadByte());
            br.ReadRemainderToIList(list, 2);
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, list))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.IListToString(list)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });
    }
}
