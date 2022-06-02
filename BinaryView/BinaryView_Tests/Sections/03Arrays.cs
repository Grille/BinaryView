
namespace BinaryView_Tests;
partial class Section
{
    public static void S03Arrays()
    {
        TUtils.WriteTitle("test arrays");

        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        Tests.testGArray(new byte[] { 0, 2, 4, 6 });
        Tests.testGArray(new byte[] { 0, 2, 4, 6 }, LengthPrefix.Int64);
        Tests.testGArray(new int[] { 0, -2, 4, -6 });
        Tests.testGArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
        Tests.testGArray(new TUtils.Struct[] { new TUtils.Struct() { A = 42, B = 3.6f }, new TUtils.Struct() { A = 36, B = 1.666f } });
        Tests.testArray("StringArray", new string[] { "ab", "cd", "ef", "gh" });

        data.Destroy();
    }
}
