
namespace BinaryView_Tests;
partial class Section
{
    public static void S08Speed()
    {
        TUtils.WriteTitle("test speed");

        var rnd = new Random();
        var watch = new Stopwatch();

        var data = new TestData();
        var stream = data.Stream;
        var bw = data.Writer;
        var br = data.Reader;

        TUtils.Test("WriteByte x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                bw.WriteByte((byte)rnd.NextDouble());
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });
        TUtils.Test("ReadByte x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                br.ReadByte();
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });

        TUtils.Test("Write<byte> x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                bw.Write<byte>((byte)rnd.NextDouble());
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });

        TUtils.Test("Read<byte> x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                br.Read<byte>();
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });

        TUtils.Test("WriteDouble x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                bw.WriteDouble(rnd.NextDouble());
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });
        TUtils.Test("ReadDouble x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                br.ReadDouble();
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });

        TUtils.Test("Write<double> x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                bw.Write<double>(rnd.NextDouble());
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });

        TUtils.Test("Read<double> x100000 time", () =>
        {
            stream.Seek(0, SeekOrigin.Begin);
            watch.Restart();
            for (int i = 0; i < 100000; i++)
            {
                br.Read<double>();
            }
            watch.Stop();

            TUtils.WriteSucces($"OK {watch.Elapsed.TotalMilliseconds}ms");
            return TestResult.Success;
        });

        data.Destroy();
    }
}
