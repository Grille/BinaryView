
namespace BinaryView_Tests;
partial class Section
{
    public static void Strings()
    {
        TUtils.WriteTitle("test string");

        Tests.WriteReadString("TestString123", LengthPrefix.Default, CharSizePrefix.Default);
        Tests.WriteReadString("TestString123", LengthPrefix.Byte, CharSizePrefix.Byte);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, CharSizePrefix.Char);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+");
    }
}
