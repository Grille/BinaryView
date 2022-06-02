
namespace BinaryView_Tests;
partial class Section
{
    public static void S05Serializble()
    {
        TUtils.WriteTitle("test serializable types");

        Tests.testSTyp(42);
        Tests.testSTyp("Hello World");
        Tests.testSTyp(new DateTime(2000, 10, 20));
        Tests.testSTyp(new DateTime(2020, 07, 20, 15, 54, 24));
        Tests.testSTyp(new Point(2000, 10));
        Tests.testSTyp(new RectangleF(10, 42, 25.5f, 23));
    }
}
