
namespace BinaryView_Tests;
partial class Section
{
    public unsafe static void Speed()
    {
        TUtils.WriteTitle("test speed");

        var rnd = new Random();
        var watch = new Stopwatch();

        var data = new TestData();
        var stream = data.Stream;
        var bw = data.Writer;
        var br = data.Reader;

        var setup = () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
        };



        Tests.Benchmark("WriteByte", setup, () =>
        {
            bw.WriteByte(10);
        });
        Tests.Benchmark("Write<byte>", setup, () =>
        {
            bw.Write<byte>(10);
        });
        Tests.Benchmark("ReadByte", setup, () =>
        {
            br.ReadByte();
        });
        Tests.Benchmark("Read<byte>", setup, () =>
        {
            br.Read<byte>();
        });

        Tests.Benchmark("WriteDouble", setup, () =>
        {
            bw.WriteDouble(10);
        });
        Tests.Benchmark("Write<double>", setup, () =>
        {
            bw.Write<double>(10);
        });
        Tests.Benchmark("ReadDouble", setup, () =>
        {
            br.ReadDouble();
        });
        Tests.Benchmark("Read<double>", setup, () =>
        {
            br.Read<double>();
        });


        data.Dispose();
    }
}
