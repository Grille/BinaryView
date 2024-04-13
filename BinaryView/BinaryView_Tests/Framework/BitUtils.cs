using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests.Framework;
internal static class BitUtils
{
    public unsafe static bool[] GetBits<T>(T value) where T : unmanaged
    {
        int size = sizeof(T);
        var ptr = (byte*)&value;
        int bitSize = size * 8;

        var buffer = new bool[bitSize];

        for (int iByte = 0; iByte < size; iByte++)
        {
            for (int iBit = 0; iBit < 8; iBit++)
            {
                int index = iByte * 8 + iBit;

                buffer[index] = ((*(ptr + iByte) >> iBit) & 1) == 1;
            }
        }

        return buffer;
    }

    public unsafe static bool MatchBits<T>(T expected, Stream stream, out string mask) where T : unmanaged
    {
        int size = sizeof(T);

        byte[] buffer = new byte[size];
        stream.Read(buffer, 0, size);

        fixed (void* ptr = buffer)
        {
            var value = *(T*)ptr;
            return MatchBits(expected, value, out mask);
        }
    }

    public static bool MatchBits<T>(T expected, T value, out string mask) where T : unmanaged
    {
        var bits1 = GetBits(expected);
        var bits2 = GetBits(value);

        return MatchBits(bits1, bits2, out mask);
    }

    public static bool MatchBits(bool[] bits1, bool[] bits2, out string mask)
    {
        bool result = true;
        var sb = new StringBuilder();

        sb.Append("0b");

        for (int i = 0; i < bits1.Length; i++)
        {
            if (i % 8 == 0)
                sb.Append("_");

            var bit1 = bits1[i];
            var bit2 = bits2[i];

            if (bit1 == bit2)
            {
                sb.Append(bit1 ? "1" : "0");
            }
            else
            {
                sb.Append(bit1 ? "!" : "-");
                result = false;
            }
        }

        mask = sb.ToString();
        return result;
    }
}
