
namespace BinaryView_Tests;
partial class Section
{
    public static void Arrays()
    {
        TUtils.WriteTitle("test arrays");

        var data = new TestData();
        var bw = data.Writer;
        var br = data.Reader;

        Tests.WriteReadGenericArray(new byte[] { 0, 2, 4, 6 });
        Tests.WriteReadGenericArray(new byte[] { 0, 2, 4, 6 }, LengthPrefix.Int64);
        Tests.WriteReadGenericArray(new int[] { 0, -2, 4, -6 });
        Tests.WriteReadGenericArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
        Tests.WriteReadGenericArray(new TUtils.Struct[] { new TUtils.Struct() { A = 42, B = 3.6f }, new TUtils.Struct() { A = 36, B = 1.666f } });
        Tests.WriteReadStringArray("StringArray", new string[] { "ab", "cd", "ef", "gh" });

        data.Destroy();
    }
}
