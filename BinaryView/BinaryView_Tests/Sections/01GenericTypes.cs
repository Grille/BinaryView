
namespace BinaryView_Tests;
partial class Section
{
    public static void S01GenericTypes()
    {
        TUtils.WriteTitle("test unmanaged types");

        Tests.testGTyp(false, true);
        Tests.testGTyp(char.MinValue, char.MaxValue);
        Tests.testGTyp(byte.MinValue, byte.MaxValue);
        Tests.testGTyp(sbyte.MinValue, sbyte.MaxValue);
        Tests.testGTyp(ushort.MinValue, ushort.MaxValue);
        Tests.testGTyp(short.MinValue, short.MaxValue);
        Tests.testGTyp(uint.MinValue, uint.MaxValue);
        Tests.testGTyp(int.MinValue, int.MaxValue);
        Tests.testGTyp(ulong.MinValue, ulong.MaxValue);
        Tests.testGTyp(long.MinValue, long.MaxValue);
        Tests.testGTyp(float.MinValue, float.MaxValue);
        Tests.testGTyp(double.MinValue, double.MaxValue);
        Tests.testGTyp(decimal.MinValue, decimal.MaxValue);
        Tests.testGTyp(new TUtils.Struct() { A = 42, B = 3.6f });
        Tests.testGTyp(new DateTime(2020, 07, 20, 15, 54, 24));
        Tests.testGTyp(new Point(10, 42));
        Tests.testGTyp(new RectangleF(10, 42, 25.5f, 23));
    }
}
