
namespace BinaryView_Tests;
partial class Section
{
    public static void Strings()
    {
        TUtils.WriteTitle("test string");

        Tests.WriteReadString("TestString123", LengthPrefix.Default, CharSize.Default);
        Tests.WriteReadString("TestString123", LengthPrefix.Byte, CharSize.Byte);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, CharSize.Char);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+");
    }
}
