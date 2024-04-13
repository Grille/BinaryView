using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryView_Tests.Framework;

namespace BinaryView_Tests;

internal static class Asserts
{
    public static void AssertMatchBits<T>(T expected, T value) where T : unmanaged
    {
        if (!BitUtils.MatchBits(expected, value, out string cmpmask))
            Fail($"bits:'{cmpmask}'");
    }

    public static void AssertMatchBits<T>(T mask, Stream stream) where T : unmanaged
    {
        if (!BitUtils.MatchBits(mask, stream, out string cmpmask))
            Fail($"bits:'{cmpmask}'");
    }

    public static void AssertMatchBits<T>(T mask, Stream stream, out string cmpmask) where T : unmanaged
    {
        if (!BitUtils.MatchBits(mask, stream, out cmpmask))
            Fail($"bits:'{cmpmask}'");
    }

    public static void AssertIsEndOfStream(Stream stream, bool endExpected = true)
    {
        bool end = stream.Position >= stream.Length;
        if (!end)
        {
            Fail($"stream not at end (len: {stream.Length} pos: {stream.Position})");
        }
    }

    public static void AssertIsNotEndOfStream(Stream stream, bool endExpected = true)
    {
        bool end = stream.Position >= stream.Length;
        if (end)
        {
            Fail($"stream reached end (len: {stream.Length} pos: {stream.Position})");
        }
    }

    public static void AssertVirtualFileWasWriten(TestData data)
    {
        AssertIsNotEqual(0, data.PopPos(), $"file length is 0");
    }
}

