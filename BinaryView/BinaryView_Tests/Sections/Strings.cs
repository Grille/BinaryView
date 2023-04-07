
namespace BinaryView_Tests;
partial class Section
{
    public static void Strings()
    {
#pragma warning disable SYSLIB0001
        TestSys.WriteTitle("test string");

        Tests.WriteReadString("TestString123", LengthPrefix.Int32, Encoding.UTF8);
        Tests.WriteReadString("TestString123", LengthPrefix.Byte, Encoding.UTF8);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, Encoding.UTF32);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, Encoding.ASCII);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, Encoding.Unicode);
#if NETCOREAPP2_1_OR_GREATER
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, Encoding.Latin1);
#endif
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, Encoding.UTF8);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, Encoding.Unicode);

        Tests.WriteReadCString("TestString123", Encoding.UTF7);
        Tests.WriteReadCString("TestString123", Encoding.UTF8);
        Tests.WriteReadCString("TestString123", Encoding.ASCII);
#if NETCOREAPP2_1_OR_GREATER
        Tests.WriteReadCString("TestString123", Encoding.Latin1);
#endif
        Tests.WriteReadCString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", Encoding.UTF7);
        Tests.WriteReadCString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", Encoding.UTF8);
#pragma warning restore SYSLIB0001
    }
}
