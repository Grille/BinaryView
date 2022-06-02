
namespace BinaryView_Tests;
partial class Section
{
    public static void S00Types()
    {
        TUtils.WriteTitle("test types");

        Tests.testTyp("Boolean", false, true);
        Tests.testTyp("Char", char.MinValue, char.MaxValue);
        Tests.testTyp("Byte", byte.MinValue, byte.MaxValue);
        Tests.testTyp("SByte", sbyte.MinValue, sbyte.MaxValue);
        Tests.testTyp("UInt16", ushort.MinValue, ushort.MaxValue);
        Tests.testTyp("Int16", short.MinValue, short.MaxValue);
        Tests.testTyp("UInt32", uint.MinValue, uint.MaxValue);
        Tests.testTyp("Int32", int.MinValue, int.MaxValue);
        Tests.testTyp("UInt64", ulong.MinValue, ulong.MaxValue);
        Tests.testTyp("Int64", long.MinValue, long.MaxValue);
        Tests.testTyp("Single", float.MinValue, float.MaxValue);
        Tests.testTyp("Double", double.MinValue, double.MaxValue);
        Tests.testTyp("Decimal", decimal.MinValue, decimal.MaxValue);
    }
}
