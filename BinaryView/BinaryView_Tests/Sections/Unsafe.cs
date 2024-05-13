
using System.Numerics;

namespace BinaryView_Tests;

partial class Section
{
    public static void Unsafe()
    {
        Section("Unsafe");

        Tests.WriteRead(60);
        Tests.WriteRead(Vector2.One);
    }
}

file unsafe static class Tests
{
    public static void WriteRead<T>(T value) where T : unmanaged
    {
        WriteReadPtr(value);
        WriteReadRef(value);
    }

    public static void WriteReadPtr<T>(T value) where T : unmanaged
    {
        var name = $"read/write ptr<{typeof(T).Name}> ({value})";
        Test(name, () => TestWriteReadPtr(value));
    }

    public static void WriteReadRef<T>(T value) where T : unmanaged
    {
        var name = $"read/write ref<{typeof(T).Name}> ({value})";
        Test(name, () => TestWriteReadRef(value));
    }

    static void TestWriteReadPtr<T>(T value) where T : unmanaged
    {
        var data = new TestData();

        T src = value;
        T dst = default;

        AssertIsNotEqual(src, dst);

        using (var bw = data.Writer)
        {
            bw.WriteFromPtr(&src);
        }

        var pos = data.PopPos();
        AssertIsEqual(sizeof(T), pos);

        using (var br = data.Reader)
        {
            br.ReadToPtr(&dst);
        }

        AssertIsEqual(src, dst);
    }

    static void TestWriteReadRef<T>(T value) where T : unmanaged
    {
        var data = new TestData();

        T src = value;
        T dst = default;

        AssertIsNotEqual(src, dst);

        using (var bw = data.Writer)
        {
            bw.WriteFromRef(ref src);
        }

        var pos = data.PopPos();
        AssertIsEqual(sizeof(T), pos);

        using (var br = data.Reader)
        {
            br.ReadToRef(ref dst);
        }

        AssertIsEqual(src, dst);
    }
}
