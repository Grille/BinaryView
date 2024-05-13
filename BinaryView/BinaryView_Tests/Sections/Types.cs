
namespace BinaryView_Tests;
partial class Section
{
    public static void PrimitiveTypes()
    {
        Section("test types");

        Tests.WriteReadPrimitive(false, true);
        Tests.WriteReadPrimitive(char.MinValue, char.MaxValue);
        Tests.WriteReadPrimitive(byte.MinValue, byte.MaxValue);
        Tests.WriteReadPrimitive(sbyte.MinValue, sbyte.MaxValue);
        Tests.WriteReadPrimitive(ushort.MinValue, ushort.MaxValue);
        Tests.WriteReadPrimitive(short.MinValue, short.MaxValue);
        Tests.WriteReadPrimitive(uint.MinValue, uint.MaxValue);
        Tests.WriteReadPrimitive(int.MinValue, int.MaxValue);
        Tests.WriteReadPrimitive(ulong.MinValue, ulong.MaxValue);
        Tests.WriteReadPrimitive(long.MinValue, long.MaxValue);
#if NET5_0_OR_GREATER
        Tests.WriteReadPrimitive(Half.MinValue, Half.MaxValue);
#endif
        Tests.WriteReadPrimitive(float.MinValue, float.MaxValue);
        Tests.WriteReadPrimitive(double.MinValue, double.MaxValue);
        Tests.WriteReadPrimitive(decimal.MinValue, decimal.MaxValue);
    }
}
