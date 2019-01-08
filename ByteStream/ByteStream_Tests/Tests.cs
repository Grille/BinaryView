using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGL.IO;
namespace ByteStream_Tests
{
    static class Tests
    {
        static int testOkCount = 0, testFailCount = 0, testErrorCount = 0;
        static ByteStream byteStream;
        private static string text;

        public static void Run()
        {
            byteStream = new ByteStream();

            int[] a1 = new int[] { 2, 3 };
            int[] a2 = new int[] { 2, 3 };
            Console.WriteLine(a1.Equals(a2));

            Console.WriteLine("Run tests...\n");


            test("read/write int", () =>
            {
                byteStream.WriteInt(8);
                byteStream.ResetIndex();
                int result = byteStream.ReadInt();
                if (result == 8) printTest(0);
                else printTest(1);
            });

            testTyp<byte>("byte", byteStream.WriteByte, byteStream.ReadByte, 4);
            testTyp<int>("int", byteStream.WriteInt, byteStream.ReadInt, 4);
            testTyp<int>("int", byteStream.WriteInt, byteStream.ReadInt, -4);
            testTyp<float>("float", byteStream.WriteFloat, byteStream.ReadFloat, 8.78f);
            testTyp<float>("float", byteStream.WriteFloat, byteStream.ReadFloat, -8.78f);
            testTyp<string>("string", byteStream.WriteString, byteStream.ReadString, "testString123");
            Console.WriteLine();

            testArray<byte>("byte", byteStream.WriteByteArray, byteStream.ReadByteArray, new byte[] { 0, 2, 4, 6 });
            testArray<int>("int", byteStream.WriteIntArray, byteStream.ReadIntArray, new int[] { 0, -2, 4, -6 });
            testArray<float>("float", byteStream.WriteFloatArray, byteStream.ReadFloatArray, new float[] { 0, -2.5f, 4.25f, -6.66f });
            testArray<string>("string", byteStream.WriteStringArray, byteStream.ReadStringArray, new string[] { "ab", "cd", "ef", "gh" });
            Console.WriteLine();

            testArray<byte>("byte", byteStream.WriteByteArray, 0, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0,0 });
            testArray<byte>("byte", byteStream.WriteByteArray, 1, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0, });
            /*
            testArray<byte>("byte", byteStream.WriteByteArray, 2, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0,0,0 });
            testArray<byte>("byte", byteStream.WriteByteArray, 3, byteStream.ReadByteArray, new byte[] { 0, 1, 1, 1, 2, 2, 2, 0,0 });
            */
            float count = testOkCount + testFailCount + testErrorCount;
            Console.WriteLine("\nExecuted tests: " + count);
            Console.WriteLine("ok: " + testOkCount + " | " + 100 * Math.Round((double)(testOkCount / count), 2) + "%");
            Console.WriteLine("fail: " + testFailCount + " | " + 100 * Math.Round((double)(testFailCount / count), 2) + "%");
            Console.WriteLine("error: " + testErrorCount + " | " + 100 * Math.Round((double)(testErrorCount / count), 2) + "%");
        }

        private static void test(string name, Action method)
        {
            
            text = name;
            try
            {
                method();
            }
            catch (Exception e) { printTest(2, e.Message); }
        }

        private static void testTyp<T>(string typ, Action<T> write, Func<T> read, T input)
        {
           
            test("read/write " + typ+" ("+ input + ")", () =>
              {
                  byteStream.ResetIndex();
                  write(input);
                  byteStream.ResetIndex();
                  T result = read();
                  if ("" + result == "" + input) printTest(0);
                  else printTest(1, "" + result);
              });
        }
        private static void testArray<T>(string typ, Action<T[]> write, Func<T[]> read, T[] input)
        {
            test("read/write " + typ + "[] (" + arrayToString(input)+")", () =>
            {
                byteStream.ResetIndex();
                write(input);
                byteStream.ResetIndex();
                T[] result = read();
                if (input.Length!= result.Length)
                {
                    printTest(1, "length not equal" + input.Length+"!="+ result.Length);
                }
                bool ok = true;
                int index = 0;
                for (int i = 0; i < result.Length; i++)
                {
                    if ("" + result[i] != "" + input[i]) {
                        ok = false;
                        index = i;
                        break;
                    }
                }
                if (ok) printTest(0);
                else printTest(1, "["+index+"] " + result[index] + "!=" + input[index]+" array("+ arrayToString(result) + ")");
            });
        }

        private static void testArray<T>(string typ, Action<T[],int> write,int option, Func<T[]> read, T[] input)
        {
            test("read/write " + typ + "[] -"+option+" (" + arrayToString(input) + ")", () =>
            {
                byteStream.ResetIndex();
                write(input,option);
                byteStream.ResetIndex();
                T[] result = read();
                if (input.Length != result.Length)
                {
                    printTest(1, "length not equal" + input.Length + "!=" + result.Length);
                }
                bool ok = true;
                int index = 0;
                for (int i = 0; i < result.Length; i++)
                {
                    if ("" + result[i] != "" + input[i])
                    {
                        ok = false;
                        index = i;
                        break;
                    }
                }
                if (ok) printTest(0);
                else printTest(1, "[" + index + "] " + result[index] + "!=" + input[index] + " array(" + arrayToString(result) + ")");
            });
        }

        private static void printTest(int state)
        {
            printTest(state, null);
        }
        private static void printTest(int state, string message)
        {
            switch (state)
            {
                case 0:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(text + " OK");
                    testOkCount++;
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(text + " FAIL");
                    testFailCount++;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(text + " ERROR");
                    testErrorCount++;
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            if (message != null) Console.Write(" -> " + message);
            Console.WriteLine();
        }
        private static string arrayToString<T>(T[] array)
        {
            string result = "";
            for (int i = 0; i < array.Length; i++)
            {
                result += "" + array[i];
                if (i < array.Length-1) result += ",";
            }
            return result;
        }
    }
}
