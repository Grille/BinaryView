using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;

internal static class TUtils
{
    public struct Struct
    {
        public int A;
        public float B;
        public override string ToString() => "{A:" + A + ";B:" + B + "}";
        public override bool Equals(object obj) => A == ((Struct)obj).A && B == ((Struct)obj).B;
    }

    public static bool CatchExeptions = false;

    static int successCount = 0;
    static int failureCount = 0;
    static int errorCount = 0;

    public static void Test(string name, Func<TestResult> test)
    {
        Write($"{name}: ");
        TestResult result;
        if (CatchExeptions)
        {
            try
            {
                result = test();
            }
            catch (Exception e)
            {
                Write("\n");
                WriteError(e.ToString());
                result = TestResult.Error;

            }
        }
        else
        {
            result = test();
        }
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
    public static bool IsArrayEqual<T>(T[] array1, T[] array2)
    {
        if (array1.Length != array2.Length)
            return false;
        for (int i = 0; i < array2.Length; i++)
            if ("" + array2[i] != "" + array1[i])
                return false;
        return true;
    }
    public static string ArrayToString<T>(T[] array)
    {
        string result = "";
        for (int i = 0; i < array.Length; i++)
        {
            result += "" + array[i];
            if (i < array.Length - 1) result += ",";
        }
        return result;
    }
}

