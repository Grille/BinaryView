using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dia2Lib;

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

        Test("StreamStack", () =>
        {
            var stream1 = new MemoryStream();
            var stream2 = new MemoryStream();

            var stack = new StreamStack(stream1, false);
            AssertIsEqual(1, stack.Count);
            stack.Push(stream2, false);
            AssertIsEqual(2, stack.Count);
            var entry = stack.Pop();
            AssertIsEqual(1, stack.Count);

            AssertThrowsAndReturn<InvalidOperationException>(()=>stack.Pop());

            stack.Dispose();

            stream2.WriteByte(0);

            entry.Dispose();

            AssertThrowsAndReturn<ObjectDisposedException>(() => stream1.WriteByte(0));
            AssertThrowsAndReturn<ObjectDisposedException>(() => stream2.WriteByte(0));
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
