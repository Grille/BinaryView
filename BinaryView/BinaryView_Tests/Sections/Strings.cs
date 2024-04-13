
namespace BinaryView_Tests;
partial class Section
{
    public static void Strings()
    {
#pragma warning disable SYSLIB0001
        Section("test string");

        Tests.WriteReadString("TestString123", LengthPrefix.Int32, StringLengthMode.ByteCount, Encoding.UTF8);
        Tests.WriteReadString("TestString123", LengthPrefix.Byte, StringLengthMode.ByteCount, Encoding.UTF8);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, StringLengthMode.ByteCount, Encoding.UTF32);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, StringLengthMode.ByteCount, Encoding.ASCII);
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, StringLengthMode.ByteCount, Encoding.Unicode);
#if NETCOREAPP2_1_OR_GREATER
        Tests.WriteReadString("TestString123", LengthPrefix.UInt32, StringLengthMode.ByteCount, Encoding.Latin1);
#endif
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, StringLengthMode.ByteCount, Encoding.ASCII, true);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, StringLengthMode.ByteCount, Encoding.UTF8);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, StringLengthMode.ByteCount, Encoding.Unicode);

        Tests.WriteReadString("TestString123", LengthPrefix.Int32, StringLengthMode.CharCount, Encoding.ASCII);
        Tests.WriteReadString("TestString123", LengthPrefix.Int32, StringLengthMode.CharCount, Encoding.UTF8);

        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, StringLengthMode.CharCount, Encoding.ASCII, true);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, StringLengthMode.CharCount, Encoding.UTF8);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, StringLengthMode.CharCount, Encoding.Unicode);
        Tests.WriteReadString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", LengthPrefix.UInt32, StringLengthMode.CharCount, Encoding.UTF32);

        Tests.WriteReadCString("TestString123", Encoding.UTF7);
        Tests.WriteReadCString("TestString123", Encoding.UTF8);
        Tests.WriteReadCString("TestString123", Encoding.ASCII);
#if NETCOREAPP2_1_OR_GREATER
        Tests.WriteReadCString("TestString123", Encoding.Latin1);
#endif
        Tests.WriteReadCString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", Encoding.UTF7);
        Tests.WriteReadCString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", Encoding.UTF8);

        Tests.WriteReadCString("abcdef\0", Encoding.UTF8, true);
        Tests.WriteReadCString("abc\0def", Encoding.UTF8, true);
        Tests.WriteReadCString("Ä'*Ü-.,><%§ÃoÜ╝ô○╝+", Encoding.Unicode, true);
#pragma warning restore SYSLIB0001
    }
}
