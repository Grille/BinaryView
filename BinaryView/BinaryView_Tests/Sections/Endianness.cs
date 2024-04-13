
namespace BinaryView_Tests;
partial class Section
{
    public static void Endianness()
    {
        Section("test endianness");

        //value, endian(byte,bit), mask
        Tests.Endianness<byte>("L0", 0b_00000001, 0b_00000001);
        Tests.Endianness<byte>("L1", 0b_00000001, 0b_10000000);
        Tests.Endianness<byte>("B0", 0b_00000001, 0b_00000001);
        Tests.Endianness<byte>("B1", 0b_00000001, 0b_10000000);

        Tests.Endianness<ushort>("L0", 0b_00000001_00000101, 0b_00000001_00000101);
        Tests.Endianness<ushort>("L1", 0b_00000001_00000101, 0b_10000000_10100000);
        Tests.Endianness<ushort>("B0", 0b_00000001_00000101, 0b_00000101_00000001);
        Tests.Endianness<ushort>("B1", 0b_00000001_00000101, 0b_10100000_10000000);

        Tests.Endianness<UInt24>("L0", 0b_00000001_00000101_10010011, 0b_00000001_00000101_10010011);
        Tests.Endianness<UInt24>("L1", 0b_00000001_00000101_10010011, 0b_10000000_10100000_11001001);
        Tests.Endianness<UInt24>("B0", 0b_00000001_00000101_10010011, 0b_10010011_00000101_00000001);
        Tests.Endianness<UInt24>("B1", 0b_00000001_00000101_10010011, 0b_11001001_10100000_10000000);

    }
}
