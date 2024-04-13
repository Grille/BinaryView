
namespace BinaryView_Tests;
partial class Section
{
    public static void Combined()
    {
        Section("test Combined");

        byte[] data0 = new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

        Test("Combined", () =>
        {
            var file = new MemoryStream();

            using (var bw = new BinaryViewWriter(file))
            {
                bw.WriteArray(data0, LengthPrefix.None);
            }

            file.Seek(0, SeekOrigin.Begin);
            using (var td = new TestData(file))
            {
                var bw = td.Writer;
                var br = td.Reader;

                var rdata = br.ReadArray<byte>(8);
                AssertIListIsEqual(rdata, data0);

                td.Seek(1);
                bw.Write<byte>(200);

                td.Seek(1);
                byte val = br.ReadByte();

                AssertIsEqual<byte>(val, 200);
            }

            Succes();
        });

        Test("Insert", () =>
        {
            var file = new MemoryStream();

            using (var bw = new BinaryViewWriter(file))
            {
                bw.WriteArray(data0, LengthPrefix.None);
            }

            file.Seek(0, SeekOrigin.Begin);
            using (var td = new TestData(file))
            {
                var bw = td.Writer;
                var br = td.Reader;

                td.Seek(4);
                bw.BeginInsert();

                bw.Write<byte>(100);
                bw.Write<byte>(101);

                bw.EndInsert();

                td.Seek(0);
                var vdata1 = new byte[10] { 0, 1, 2, 3, 100, 101, 4, 5, 6, 7 };
                var rdata1 = br.ReadArray<byte>(10);
                AssertIListIsEqual(rdata1, vdata1);
            }

            Succes();
        });
    }
}
