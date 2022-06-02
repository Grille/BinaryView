
namespace BinaryView_Tests;
partial class Section
{
    public static void S02Strings()
    {
        TUtils.WriteTitle("test string");

        Tests.testString("TestString123", LengthPrefix.Default, CharSizePrefix.Default);
        Tests.testString("TestString123", LengthPrefix.Byte, CharSizePrefix.Byte);
        Tests.testString("TestString123", LengthPrefix.UInt32, CharSizePrefix.Char);
        Tests.testString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+");
    }
}
