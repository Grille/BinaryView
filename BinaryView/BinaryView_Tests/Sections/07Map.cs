
namespace BinaryView_Tests;
partial class Section
{
    public static void S07Map()
    {
        TUtils.WriteTitle("test map");

        Tests.testMap(512, false);
        Tests.testMap(512, true);
    }
}
