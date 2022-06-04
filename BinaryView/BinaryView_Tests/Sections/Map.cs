
namespace BinaryView_Tests;
partial class Section
{
    public static void Map()
    {
        TUtils.WriteTitle("test map");

        Tests.WriteReadMap(2048, false);
        Tests.WriteReadMap(2048, true);
    }
}
