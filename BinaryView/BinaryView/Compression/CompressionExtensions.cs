using System;
using System.IO.Compression;

namespace GGL.IO.Compression;



public static class CompressionExtensions
{
    // Read
    /// <summary>All Data after this will be read as compressed</summary>
    public static void CompressAll(this BinaryViewReader br, CompressionType type)
    {
        br.StreamStack.Push(new DecompressorStackEntry(br, type, br.Remaining));
    }

    /// <summary>Decompress data with CompressionStream, position will reset</summary>
    public static DecompressorStackEntry BeginCompressedSection(this BinaryViewReader br, CompressionType type, LengthPrefix lengthPrefix = LengthPrefix.Default)
    {
        long length = br.ReadLengthPrefix(lengthPrefix);
        return br.BeginCompressedSection(type, length);
    }

    public static DecompressorStackEntry BeginCompressedSection(this BinaryViewReader br, CompressionType type, long length)
    {
        var entry = new DecompressorStackEntry(br, type, length);
        br.StreamStack.Push(entry);
        return entry;
    }

    public static DecompressorStackEntry EndCompressedSection(this BinaryViewReader br)
    {
        var entry = (DecompressorStackEntry)br.StreamStack.Pop();
        entry.Dispose();
        return entry;
    }

    // Write
    /// <summary>All Data after this will be writen as compressed</summary>
    public static void CompressAll(this BinaryViewWriter bw, CompressionType type, CompressionLevel level = CompressionLevel.Optimal)
    {
        var entry = new CompressorStackEntry(bw, type, level);
        bw.StreamStack.Push(entry);
    }

    public static CompressorStackEntry BeginCompressedSection(this BinaryViewWriter bw, CompressionType type, LengthPrefix lengthPrefix = LengthPrefix.Default)
        => BeginCompressedSection(bw, type, CompressionLevel.Optimal, lengthPrefix);

    public static CompressorStackEntry BeginCompressedSection(this BinaryViewWriter bw, CompressionType type, CompressionLevel level, LengthPrefix lengthPrefix = LengthPrefix.Default)
    {
        var entry = new CompressorStackEntry(bw, type, level, lengthPrefix);
        bw.StreamStack.Push(entry);
        return entry;
    }

    public static CompressorStackEntry EndCompressedSection(this BinaryViewWriter bw)
    {
        var entry = (CompressorStackEntry)bw.StreamStack.Pop();
        entry.Dispose();
        return entry;
    }

    // view
    /// <summary>All Data after this will be compressed</summary>
    public static void CompressAll(this BinaryView view, CompressionType type, CompressionLevel level = CompressionLevel.Optimal)
    {
        if (view.Mode == ViewMode.Read)
            view.Reader.CompressAll(type);
        else
            view.Writer.CompressAll(type, level);
    }

    public static void BeginCompressedSection(this BinaryView view, CompressionType type, LengthPrefix prefix = LengthPrefix.Default)
        => BeginCompressedSection(view, type, CompressionLevel.Optimal, prefix);

    public static void BeginCompressedSection(this BinaryView view, CompressionType type, CompressionLevel level, LengthPrefix prefix = LengthPrefix.Default)
    {
        if (view.Mode == ViewMode.Read)
            view.Reader.BeginCompressedSection(type, prefix);
        else
            view.Writer.BeginCompressedSection(type, level, prefix);
    }

    public static void EndCompressedSection(this BinaryView view)
    {
        if (view.Mode == ViewMode.Read)
            view.Reader.EndCompressedSection();
        else
            view.Writer.EndCompressedSection();
    }
}
