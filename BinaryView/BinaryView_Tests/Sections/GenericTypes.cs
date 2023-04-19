
namespace BinaryView_Tests;
partial class Section
{
    public static void GenericTypes()
    {
        TestSys.WriteTitle("test unmanaged types");

        Tests.WriteReadGeneric(false, true);
        Tests.WriteReadGeneric(char.MinValue, char.MaxValue);
        Tests.WriteReadGeneric(byte.MinValue, byte.MaxValue);
        Tests.WriteReadGeneric(sbyte.MinValue, sbyte.MaxValue);
        Tests.WriteReadGeneric(ushort.MinValue, ushort.MaxValue);
        Tests.WriteReadGeneric(short.MinValue, short.MaxValue);
        Tests.WriteReadGeneric(uint.MinValue, uint.MaxValue);
        Tests.WriteReadGeneric(int.MinValue, int.MaxValue);
        Tests.WriteReadGeneric(ulong.MinValue, ulong.MaxValue);
        Tests.WriteReadGeneric(long.MinValue, long.MaxValue);
#if NET5_0_OR_GREATER
        Tests.WriteReadGeneric(Half.MinValue, Half.MaxValue);
#endif
        Tests.WriteReadGeneric(float.MinValue, float.MaxValue);
        Tests.WriteReadGeneric(double.MinValue, double.MaxValue);
        Tests.WriteReadGeneric(decimal.MinValue, decimal.MaxValue);
        Tests.WriteReadGeneric(new UIntSmart15(42));
        Tests.WriteReadGeneric(new Int32Single(42, 3.6f));
        Tests.WriteReadGeneric(new DateTime(2020, 07, 20, 15, 54, 24));
        Tests.WriteReadGeneric(new Point(10, 42));
        Tests.WriteReadGeneric(new RectangleF(10, 42, 25.5f, 23));
    }
}
