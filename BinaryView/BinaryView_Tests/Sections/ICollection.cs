using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;

partial class Section
{
    public static void ICollection()
    {
        Section("test ICollection");

        Test("collection", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            var collection1 = new Collection<int>() { 1, 2, 3, 4 };


            test.ResetPos();
            bw.WriteICollection(collection1);

            AssertIsEqual(sizeof(int) * 5, test.PopPos());

            var collection2 = new Collection<int>();
            br.ReadToICollection(collection2);

            AssertIEnumerableIsEqual(collection1, collection2);
        });

        Test("collection", () =>
        {
            using var test = new TestData();
            var bw = test.Writer;
            var br = test.Reader;

            var collection1 = new int[4] { 1, 2, 3, 4 };


            test.ResetPos();
            bw.WriteICollection(collection1);

            test.ResetPos();

            var collection2 = new int[4];

            AssertThrows<NotSupportedException>(() =>
            {
                br.ReadToICollection(collection2);
            });
        });
    }
}
