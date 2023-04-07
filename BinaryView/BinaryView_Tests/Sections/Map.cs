
namespace BinaryView_Tests;
partial class Section
{
    public static void Map()
    {
        TestSys.WriteTitle("test map");

        Tests.WriteReadMap(2048, false);
        Tests.WriteReadMap(2048, true);

        Tests.ViewMap(2048, false);
        Tests.ViewMap(2048, true);
    }
}
