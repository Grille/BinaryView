
namespace BinaryView_Tests;
partial class Section
{
    public static void Serializble()
    {
        TUtils.WriteTitle("test serializable types");

        Tests.WriteReadSerializable(42);
        Tests.WriteReadSerializable("Hello World");
        Tests.WriteReadSerializable(new DateTime(2000, 10, 20));
        Tests.WriteReadSerializable(new DateTime(2020, 07, 20, 15, 54, 24));
        Tests.WriteReadSerializable(new Point(2000, 10));
        Tests.WriteReadSerializable(new RectangleF(10, 42, 25.5f, 23));
    }
}
