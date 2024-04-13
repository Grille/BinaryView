using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;
partial class Section
{
    public static void CreateDispose()
    {
        Section("test create & dispose");

        Test("StreamStack", () =>
        {
            using var stream = new MemoryStream();
            using (var stack = new StreamStack(stream, false)) { }
            AssertThrows<ObjectDisposedException>(() => stream.WriteByte(0));
        });

        Test("StreamStack", () =>
        {
            using var stream = new MemoryStream();
            using (var stack = new StreamStack(stream, true)) { }
            stream.WriteByte(0);
        });

        Test("BinaryViewWriter", () =>
        {
            using (var stack = new BinaryViewWriter()) { }
        });

        Test("BinaryViewReader", () =>
        {
            using (var stack = new BinaryViewReader()) { }
        });

        Test("BinaryView ViewMode.Write", () =>
        {
            using var stream = new MemoryStream();
            using (var stack = new BinaryView(stream, ViewMode.Write)) { }
        });

        Test("BinaryView ViewMode.Read", () =>
        {
            using var stream = new MemoryStream();
            using (var stack = new BinaryView(stream, ViewMode.Read)) { }
        });
    }
}
