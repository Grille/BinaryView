﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryView_Tests.Framework;

namespace BinaryView_Tests;

internal static class TestSys
{
    public static bool CatchExeptions = false;

    static int successCount = 0;
    static int failureCount = 0;
    static int errorCount = 0;

    public static Stopwatch Watch = new Stopwatch();

    public static double ElapsedMilliseconds
    {
        get => Watch.Elapsed.TotalMilliseconds;
    }

    public static void RunTest(string name, Func<TestResult> test)
    {
        Watch.Reset();

        Write($"{name}: ");
        TestResult result;

        try
        {
            Watch.Start();
            result = test();
        }
        catch (TestSuccessException e)
        {
            WriteSucces(e.Message);
            result = TestResult.Success;
        }
        catch (TestFailException e)
        {
            WriteFail(e.Message);
            result = TestResult.Failure;
        }
        catch (Exception e)
        {
            Write("\n");
            WriteError(e.ToString());
            result = TestResult.Error;

            if (!CatchExeptions)
                throw;
        }
        Watch.Stop();

        Write("\n");

        switch (result)
        {
            case TestResult.Success:
                successCount++;
                break;
            case TestResult.Failure:
                failureCount++;
                break;
            case TestResult.Error:
                errorCount++;
                break;
        }
    }

    public static void Write(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(msg);
    }
    public static void Write(string msg, ConsoleColor color)
    {
        var bcolor = Console.ForegroundColor;

        Console.ForegroundColor = color;
        Console.Write(msg);

        Console.ForegroundColor = bcolor;
    }
    public static void WriteTitle(string msg)
    {
        Write($"\n{msg}\n", ConsoleColor.Cyan);
    }
    public static void WriteSucces(string msg)
    {
        Write(msg, ConsoleColor.Green);
    }
    public static void WriteFail(string msg)
    {
        Write(msg, ConsoleColor.Magenta);
    }
    public static void WriteError(string msg)
    {
        Write(msg, ConsoleColor.Red);
    }
    public static void WriteResults()
    {
        WriteTitle("Results:");
        int testCount = successCount + errorCount + failureCount;
        Write($"Testcases: {testCount}\n");
        Write($"* Success: {successCount}\n");
        Write($"* failure: {failureCount + errorCount}\n");
    }
    public static bool IsIListEqual<T>(IList<T> array1, IList<T> array2)
    {
        if (array1.Count != array2.Count)
            return false;
        for (int i = 0; i < array2.Count; i++)
            if (!array2[i].Equals(array1[i]))
                return false;
        return true;
    }
    public static string IListToString<T>(IList<T> array)
    {
        var sb = new StringBuilder();
        sb.Append($"[{array.Count}]{{");
        int size = Math.Min(array.Count, 16);
        for (int i = 0; i < size; i++)
        {
            sb.Append(array[i]);
            if (i < size - 1)
                sb.Append(",");
            else if (size < array.Count)
                sb.Append("...");
        }
        sb.Append("}");
        return sb.ToString();
    }

    public unsafe static bool MatchBitsInStream<T>(T value, Stream stream, out string mask) where T : unmanaged
    {
        int size = sizeof(T);
        int bitSize = size * 8;

        byte* valueBufferPtr = (byte*)&value;
        byte[] streamBuffer = new byte[size];
        stream.Read(streamBuffer, 0, size);

        bool result = true;

        var sb = new StringBuilder();

        sb.Append("0b");

        for (int iByte = 0; iByte < size; iByte++)
        {
            for (int iBit = 0; iBit < 8; iBit++)
            {
                bool valueBit = ((*(valueBufferPtr + iByte) >> iBit) & 1) == 1;
                bool streamBit = ((streamBuffer[iByte] >> iBit) & 1) == 1;

                if (iBit == 0)
                    sb.Append("_");

                if (valueBit == streamBit)
                {
                    sb.Append(valueBit ? "1" : "0");
                }
                else
                {
                    sb.Append(valueBit ? "!" : "-");
                    result = false;
                }
            }
        }

        mask = sb.ToString();
        return result;
    }

    public static void ExpectException<T>(bool expect, Action action) where T : Exception
    {
        if (!expect)
        {
            action();
            return;
        }

        try
        {
            action();
        }
        catch (T err)
        {
            throw new TestSuccessException($"OK {err.Message}");
        }
        throw new TestFailException($"FAIL expected Exception not thrown");
    }

    public static void Succes(string msg = null)
    {
        if (msg == null)
            msg = "OK";

        throw new TestSuccessException(msg);
    }

    public static void AssertBitsMatchStream<T>(T mask, Stream stream, out string cmpmask) where T : unmanaged
    {
        if (!MatchBitsInStream(mask, stream, out cmpmask))
            throw new TestFailException($"FAIL w-bits:'{cmpmask}'");
    }

    public static void AssertValueIsNotEqual<T>(T value, T expected, string msg = "")
    {
        if (value.Equals(expected))
            throw new TestFailException($"FAIL value: {value} == expected: {expected} {msg}");
    }

    public static void AssertValueIsEqual<T>(T value, T expected, string msg = "")
    {
        if (!value.Equals(expected))
            throw new TestFailException($"FAIL value: {value} != expected: {expected} {msg}");
    }

    public static void AssertIListIsEqual<T>(IList<T> refarray0, IList<T> dataarray1, string msg = "") where T : unmanaged
    {
        if (!IsIListEqual(refarray0, dataarray1))
            throw new TestFailException($"FAIL expected: {IListToString(refarray0)} data: {IListToString(dataarray1)} {msg}");
    }

    public static void AssertEndOfStream(Stream stream, bool endExpected = true)
    {
        bool end = stream.Position >= stream.Length;
        if (endExpected && !end)
            throw new TestFailException($"FAIL stream not at end (len: {stream.Length} pos: {stream.Position})");
        if (!endExpected && end)
            throw new TestFailException($"FAIL stream reached end (len: {stream.Length} pos: {stream.Position})");
    }
}

