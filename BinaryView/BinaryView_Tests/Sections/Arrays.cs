
namespace BinaryView_Tests;
partial class Section
{
    public static void Arrays()
    {
        Section("test arrays");

        Tests.WriteReadGenericArray(new byte[] { 0, 2, 4, 6 });
        Tests.WriteReadGenericArray(new byte[] { 0, 2, 4, 6 }, LengthPrefix.Int64);
        Tests.WriteReadGenericArray(new byte[] { 0, 2, 4, 6 }, LengthPrefix.UIntSmart15);
        Tests.WriteReadGenericArray(new byte[] { 0, 2, 4, 6 }, LengthPrefix.UIntSmart62);
        Tests.WriteReadGenericArray(new int[] { 0, -2, 4, -6 });
        Tests.WriteReadGenericArray(new float[] { 0, -2.5f, 4.25f, -6.66f });
        Tests.WriteReadGenericArray(new Int32Single[] { new(42, 3.6f), new(36, 1.666f) });
        Tests.WriteReadStringArray("StringArray", new string[] { "ab", "cd", "ef", "gh" });
    }
}
