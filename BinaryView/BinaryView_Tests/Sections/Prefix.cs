namespace BinaryView_Tests;
partial class Section
{
    public static void Prefix()
    {
        TestSys.WriteTitle("test prefix");

        Tests.WriteReadPrefix(LengthPrefix.SByte, sbyte.MinValue, 1);
        Tests.WriteReadPrefix(LengthPrefix.SByte, sbyte.MaxValue, 1);
        Tests.WriteReadPrefix(LengthPrefix.Byte, byte.MinValue, 1);
        Tests.WriteReadPrefix(LengthPrefix.Byte, byte.MaxValue, 1);
        Tests.WriteReadPrefix(LengthPrefix.Byte, short.MaxValue, 1, true);
        Tests.WriteReadPrefix(LengthPrefix.Int16, short.MinValue, 2);
        Tests.WriteReadPrefix(LengthPrefix.UInt16, ushort.MaxValue, 2);
        Tests.WriteReadPrefix(LengthPrefix.Int32, int.MinValue, 4);
        Tests.WriteReadPrefix(LengthPrefix.Int32, int.MaxValue, 4);
        Tests.WriteReadPrefix(LengthPrefix.Int32, uint.MaxValue, 4, true);
        Tests.WriteReadPrefix(LengthPrefix.UInt32, int.MinValue, 4, true);
        Tests.WriteReadPrefix(LengthPrefix.UInt32, uint.MaxValue, 4);
        Tests.WriteReadPrefix(LengthPrefix.UInt32, long.MaxValue, 4, true);
        Tests.WriteReadPrefix(LengthPrefix.Int64, long.MinValue, 8);
        Tests.WriteReadPrefix(LengthPrefix.Int64, long.MaxValue, 8);
        Tests.WriteReadPrefix(LengthPrefix.UInt64, sbyte.MinValue, 8, true);
        Tests.WriteReadPrefix(LengthPrefix.UInt64, long.MinValue, 8, true);
        Tests.WriteReadPrefix(LengthPrefix.UInt64, long.MaxValue, 8);

        Tests.WriteReadPrefix(LengthPrefix.Single, (1L << 24), 4);
        Tests.WriteReadPrefix(LengthPrefix.Double, (1L << 53), 8);

        Tests.WriteReadPrefix(LengthPrefix.UIntSmart15, 0, 1);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart15, UIntSmart15.MaxValue7Bit, 1);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart15, UIntSmart15.MaxValue15Bit, 2);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart15, UIntSmart15.MaxValue15Bit + 1, 0, true);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart15, -42, 0, true);

        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, 0, 1);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, UIntSmart62.MaxValue6Bit, 1);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, UIntSmart62.MaxValue14Bit, 2);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, UIntSmart62.MaxValue30Bit, 4);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, uint.MaxValue, 8);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, UIntSmart62.MaxValue62Bit, 8);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, UIntSmart62.MaxValue62Bit + 1, 0, true);
        Tests.WriteReadPrefix(LengthPrefix.UIntSmart62, -42, 0, true);

        Tests.WriteReadUnsafePrefix(LengthPrefix.Byte, 1000, 1000 % 256, 1);
        Tests.WriteReadUnsafePrefix(LengthPrefix.Byte, -50, -50 + 256, 1);

        Tests.WriteReadUnsafePrefix(LengthPrefix.UIntSmart15, short.MinValue, UIntSmart15.MinValue, 1);
        Tests.WriteReadUnsafePrefix(LengthPrefix.UIntSmart15, short.MaxValue, UIntSmart15.MaxValue, 2);

        Tests.WriteReadUnsafePrefix(LengthPrefix.UIntSmart62, long.MinValue, UIntSmart62.MinValue, 1);
        Tests.WriteReadUnsafePrefix(LengthPrefix.UIntSmart62, long.MaxValue, UIntSmart62.MaxValue, 8);
    }
}
