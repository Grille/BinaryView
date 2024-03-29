﻿
namespace BinaryView_Tests;
partial class Section
{
    public static void Endianness()
    {
        TestSys.WriteTitle("test endianness");

        //value, endian(byte,bit), mask
        Tests.Endianness<byte>("LL", 0b_00000001, 0b_00000001);
        Tests.Endianness<byte>("LB", 0b_00000001, 0b_10000000);
        Tests.Endianness<byte>("BL", 0b_00000001, 0b_00000001);
        Tests.Endianness<byte>("BB", 0b_00000001, 0b_10000000);

        Tests.Endianness<ushort>("LL", 0b_00000001_00000101, 0b_00000001_00000101);
        Tests.Endianness<ushort>("LB", 0b_00000001_00000101, 0b_10000000_10100000);
        Tests.Endianness<ushort>("BL", 0b_00000001_00000101, 0b_00000101_00000001);
        Tests.Endianness<ushort>("BB", 0b_00000001_00000101, 0b_10100000_10000000);

        Tests.Endianness<UInt24>("LL", 0b_00000001_00000101_10010011, 0b_00000001_00000101_10010011);
        Tests.Endianness<UInt24>("LB", 0b_00000001_00000101_10010011, 0b_10000000_10100000_11001001);
        Tests.Endianness<UInt24>("BL", 0b_00000001_00000101_10010011, 0b_10010011_00000101_00000001);
        Tests.Endianness<UInt24>("BB", 0b_00000001_00000101_10010011, 0b_11001001_10100000_10000000);

    }
}
