
namespace BinaryView_Tests;
partial class Section
{
    public static void IViewObject()
    {
        const int val0 = 354;
        const int val1 = 144445;

        Section("test IViewObject");

        Test("read-to/write", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            var obj = new InterfaceImplementation() { A = val0, B = val1 };

            bw.WriteIView(obj);
            test.ResetPos();
            obj.A = obj.B = 0;

            br.ReadToIView(obj);

            AssertIsEqual(obj.A, val0);
            AssertIsEqual(obj.B, val1);

            Succes();
        });

        Test("read-new/write", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            var obj = new InterfaceImplementation() { A = val0, B = val1 };

            bw.WriteIView(obj);
            test.ResetPos();
            obj.A = obj.B = 0;

            obj = br.ReadIView<InterfaceImplementation>();

            AssertIsEqual(obj.A, val0);
            AssertIsEqual(obj.B, val1);

            Succes();
        });
    }
}
