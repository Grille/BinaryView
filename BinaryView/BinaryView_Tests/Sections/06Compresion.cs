
namespace BinaryView_Tests;
partial class Section
{
    public static void S06Compresion()
    {
        TUtils.WriteTitle("test compresion");

        int size = 8;

        Random rnd = new Random(1);

        byte[] data0 = new byte[size];
        for (int i = 0; i < size; i++)
            data0[i] = (byte)(rnd.NextDouble() * 255f);
        byte[] data1 = new byte[size];
        for (int i = 0; i < size; i++)
            data1[i] = (byte)(rnd.NextDouble() * 255f);
        byte[] data2 = new byte[size];
        for (int i = 0; i < size; i++)
            data2[i] = (byte)(rnd.NextDouble() * 255f);

        TUtils.Test("Compress All", () =>
        {
            var bw = new BinaryViewWriter();
            bw.CompressAll();
            bw.WriteArray(data0);
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);
            br.DecompressAll();
            var rdata0 = br.ReadArray<byte>();
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, rdata0))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.IListToString(rdata0)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }
            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Compress Section", () =>
        {
            var bw = new BinaryViewWriter();
            bw.BeginDeflateSection();
            bw.WriteArray(data0);
            bw.EndDeflateSection();
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);
            br.BeginDeflateSection();
            var rdata0 = br.ReadArray<byte>();
            br.EndDeflateSection();
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, rdata0))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.IListToString(rdata0)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }
            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Compress Empty Section", () =>
        {
            var bw = new BinaryViewWriter();
            bw.BeginDeflateSection();
            bw.EndDeflateSection();
            bw.WriteArray(data0);
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);
            br.BeginDeflateSection();
            br.EndDeflateSection();
            var rdata0 = br.ReadArray<byte>();
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, rdata0))
            {
                TUtils.WriteFail($"FAIL data: {TUtils.IListToString(rdata0)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }
            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Compress 2 Sections Sequential", () =>
        {
            var bw = new BinaryViewWriter();
            bw.BeginDeflateSection();
            bw.WriteArray(data0);
            bw.EndDeflateSection();
            bw.WriteArray(data1);
            bw.BeginDeflateSection();
            bw.WriteArray(data2);
            bw.EndDeflateSection();
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);
            br.BeginDeflateSection();
            var rdata0 = br.ReadArray<byte>();
            br.EndDeflateSection();
            var rdata1 = br.ReadArray<byte>();
            br.BeginDeflateSection();
            var rdata2 = br.ReadArray<byte>();
            br.EndDeflateSection();
            br.Dispose();

            if (!TUtils.IsIListEqual(data0, rdata0))
            {
                TUtils.WriteFail($"FAIL data0: {TUtils.IListToString(rdata0)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }
            if (!TUtils.IsIListEqual(data1, rdata1))
            {
                TUtils.WriteFail($"FAIL data1: {TUtils.IListToString(rdata1)}, expected: {TUtils.IListToString(data1)}");
                return TestResult.Failure;
            }
            if (!TUtils.IsIListEqual(data2, rdata2))
            {
                TUtils.WriteFail($"FAIL data2: {TUtils.IListToString(rdata2)}, expected: {TUtils.IListToString(data2)}");
                return TestResult.Failure;
            }
            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.Test("Compress 2 Sections Nested", () =>
        {
            var bw = new BinaryViewWriter();

            bw.WriteArray(data0);

            bw.BeginDeflateSection();

            bw.WriteArray(data1);

            bw.BeginDeflateSection();

            bw.WriteArray(data2);

            bw.EndDeflateSection();
            bw.EndDeflateSection();
            bw.Dispose();
            var file = bw.ToArray();

            if (file.Length == 0)
            {
                TUtils.WriteFail($"FAIL file length is 0");
                return TestResult.Failure;
            }

            var br = new BinaryViewReader(file);

            var rdata0 = br.ReadArray<byte>();
            if (!TUtils.IsIListEqual(data0, rdata0))
            {
                TUtils.WriteFail($"FAIL data0: {TUtils.IListToString(rdata0)}, expected: {TUtils.IListToString(data0)}");
                return TestResult.Failure;
            }

            br.BeginDeflateSection();

            var rdata1 = br.ReadArray<byte>();
            if (!TUtils.IsIListEqual(data1, rdata1))
            {
                TUtils.WriteFail($"FAIL data1: {TUtils.IListToString(rdata1)}, expected: {TUtils.IListToString(data1)}");
                return TestResult.Failure;
            }

            br.BeginDeflateSection();

            var rdata2 = br.ReadArray<byte>();
            if (!TUtils.IsIListEqual(data2, rdata2))
            {
                TUtils.WriteFail($"FAIL data2: {TUtils.IListToString(rdata2)}, expected: {TUtils.IListToString(data2)}");
                return TestResult.Failure;
            }

            br.EndDeflateSection();
            br.EndDeflateSection();
            br.Dispose();




            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });
    }
}
