
namespace BinaryView_Tests;
partial class Section
{
    public static void View()
    {
        TUtils.WriteTitle("test View");

        byte[] data0 = new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

        TUtils.RunTest("View", () =>
        {
            var file = new MemoryStream();

            using (var bw = new BinaryViewWriter(file))
            {
                bw.WriteArray(data0, LengthPrefix.None);
            }

            file.Seek(0, SeekOrigin.Begin);
            using (var view = new BinaryView(file))
            {
                var bw = view.Writer;
                var br = view.Reader;

                var rdata = br.ReadArray<byte>(8);
                if (TUtils.AssertIListIsEqual(rdata, data0))
                    return TestResult.Failure;

                view.Seek(1);
                bw.Write<byte>(200);

                view.Seek(1);
                byte val = br.ReadByte();

                if (TUtils.AssertValueIsEqual<byte>(val, 200))
                    return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.RunTest("Insert", () =>
        {
            var file = new MemoryStream();

            using (var bw = new BinaryViewWriter(file))
            {
                bw.WriteArray(data0, LengthPrefix.None);
            }

            file.Seek(0, SeekOrigin.Begin);
            using (var view = new BinaryView(file))
            {
                var bw = view.Writer;
                var br = view.Reader;

                view.Seek(4);
                bw.BeginInsert();

                bw.Write<byte>(100);
                bw.Write<byte>(101);

                bw.EndInsert();

                view.Seek(0);
                var vdata1 = new byte[10] { 0, 1, 2, 3, 100, 101, 4, 5, 6, 7 };
                var rdata1 = br.ReadArray<byte>(10);
                if (TUtils.AssertIListIsEqual(rdata1, vdata1))
                    return TestResult.Failure;
            }

            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });
    }
}
