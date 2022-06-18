
namespace BinaryView_Tests;
partial class Section
{
    public static void IViewObject()
    {
        const int val0 = 354;
        const int val1 = 144445;

        TUtils.WriteTitle("test IViewObject");

        TUtils.RunTest("read-to/write", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            var obj = new TUtils.InterfaceImplementation() { A = val0, B = val1 };

            bw.WriteIView(obj);
            test.ResetPtr();
            obj.A = obj.B = 0;

            br.ReadToIView(obj);

            if (TUtils.AssertValueIsEqual(obj.A, val0))
                return TestResult.Failure;

            if (TUtils.AssertValueIsEqual(obj.B, val1))
                return TestResult.Failure;


            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

        TUtils.RunTest("read-new/write", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            var obj = new TUtils.InterfaceImplementation() { A = val0, B = val1 };

            bw.WriteIView(obj);
            test.ResetPtr();
            obj.A = obj.B = 0;

            obj = br.ReadIView<TUtils.InterfaceImplementation>();

            if (TUtils.AssertValueIsEqual(obj.A, val0))
                return TestResult.Failure;

            if (TUtils.AssertValueIsEqual(obj.B, val1))
                return TestResult.Failure;


            TUtils.WriteSucces($"OK");
            return TestResult.Success;
        });

    }
}
