
namespace BinaryView_Tests;
partial class Section
{
    public static void PrimitiveTypes()
    {
        TestSys.WriteTitle("test types");

        Tests.WriteReadPrimitive("Boolean", false, true);
        Tests.WriteReadPrimitive("Char", char.MinValue, char.MaxValue);
        Tests.WriteReadPrimitive("Byte", byte.MinValue, byte.MaxValue);
        Tests.WriteReadPrimitive("SByte", sbyte.MinValue, sbyte.MaxValue);
        Tests.WriteReadPrimitive("UInt16", ushort.MinValue, ushort.MaxValue);
        Tests.WriteReadPrimitive("Int16", short.MinValue, short.MaxValue);
        Tests.WriteReadPrimitive("UInt32", uint.MinValue, uint.MaxValue);
        Tests.WriteReadPrimitive("Int32", int.MinValue, int.MaxValue);
        Tests.WriteReadPrimitive("UInt64", ulong.MinValue, ulong.MaxValue);
        Tests.WriteReadPrimitive("Int64", long.MinValue, long.MaxValue);
#if NET5_0_OR_GREATER
        Tests.WriteReadPrimitive("Half", Half.MinValue, Half.MaxValue);
#endif
        Tests.WriteReadPrimitive("Single", float.MinValue, float.MaxValue);
        Tests.WriteReadPrimitive("Double", double.MinValue, double.MaxValue);
        Tests.WriteReadPrimitive("Decimal", decimal.MinValue, decimal.MaxValue);
    }
}
