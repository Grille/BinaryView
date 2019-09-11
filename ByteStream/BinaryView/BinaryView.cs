using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace GGL.IO
{
    public enum CompressMode
    {
        Auto = -1,
        None = 0,
        RLE = 1
    }
    public class BinaryView
    {
        public Stream BaseStream;

        public int Position
        {
            get => (int)BaseStream.Position;
            set { BaseStream.Position = value; }
        }
        public BinaryView()
        {
            BaseStream = new MemoryStream(10);
        }
        public BinaryView(string path)
        {
            BaseStream = new FileStream(path, FileMode.Open);
        }
        public BinaryView(byte[] bytes)
        {
            BaseStream = new MemoryStream(bytes);
        }
        public BinaryView(Stream stream)
        {
            this.BaseStream = stream;
        }

        #region write
        public void WriteByte(byte input)
        {
            BaseStream.WriteByte(input);
        }
        public void WriteInt16(short input)
        {
            byte[] value = BitConverter.GetBytes(input);
            BaseStream.Write(value, 0, sizeof(short));
        }
        public void WriteInt32(int input)
        {
            byte[] value = BitConverter.GetBytes(input);
            BaseStream.Write(value, 0, sizeof(int));
        }
        public void WriteInt64(long input)
        {
            byte[] value = BitConverter.GetBytes(input);
            BaseStream.Write(value, 0, sizeof(long));
        }
        public void WriteSingle(float input)
        {
            byte[] value = BitConverter.GetBytes(input);
            BaseStream.Write(value, 0, sizeof(float));
        }
        public void WriteDouble(double input)
        {
            byte[] value = BitConverter.GetBytes(input);
            BaseStream.Write(value, 0, sizeof(double));
        }
        public void WriteDouble(decimal input)
        {
            Write(input);
        }

        public void WriteByteArray(int[] input)
        {
            byte[] data = new byte[input.Length];
            for (int i = 0; i < input.Length; i++) data[i] = (byte)input[i];
            WriteByteArray(data, 0);
        }
        public void WriteByteArray(byte[] input)
        {
            WriteByteArray(input, CompressMode.None);
        }
        public void WriteByteArray(byte[] input, CompressMode compressionMode)
        {
            if (compressionMode == CompressMode.Auto)
            {
                byte curValue = input[0];
                int changes = 1;
                for (int i = 1; i < input.Length; i++)
                {
                    if (input[i] != curValue)
                    {
                        changes++;
                        curValue = input[i];
                    }
                }
                float clutter = changes / (float)input.Length;
                if (clutter >= 0.5) compressionMode = CompressMode.None;
                else compressionMode = CompressMode.RLE;

            }
            if (input.Length < 256)
            {
                WriteByte((byte)(compressionMode + 4));
                WriteByte((byte)input.Length);
            }
            else
            {
                WriteByte((byte)compressionMode);
                WriteInt32((int)input.Length);
            }

            switch (compressionMode)
            {
                case CompressMode.None:
                    for (int i = 0; i < input.Length; i++)
                    {
                        WriteByte(input[i]);
                    }

                    break;
                case CompressMode.RLE:
                    byte curValue = input[0], curLength = 0;
                    for (int i = 1; i < input.Length; i++)
                    {
                        if (input[i] != curValue || curLength >= 255)
                        {
                            WriteByte(curLength);
                            WriteByte(curValue);
                            curValue = input[i]; curLength = 0;
                        }
                        else curLength++;
                    }
                    WriteByte(curLength);
                    WriteByte(curValue);
                    break;
            }
        }
        public unsafe void WriteIntArray(int[] input)
        {
            WriteInt32((int)input.Length);
            for (int i = 0; i < input.Length; i++) WriteInt32(input[i]);
        }

        public unsafe void Write<T>(T obj) where T : unmanaged
        {
            int size = sizeof(T);
            var ptr = new IntPtr(&obj);
            if (size == 1) WriteByte(Marshal.ReadByte(ptr, 0));
            else for (int i = 0; i < size; i++) WriteByte(Marshal.ReadByte(ptr, i));
        }
        public void WriteArray<T>(T[] array, int offset = 0, int length = 0) where T : unmanaged
        {
            writeArray(array, offset, length, Write);
        }
        private void writeArray<T>(T[] array, int offset = 0, int length = 0, Action<T> writer = null) where T : unmanaged
        {
            WriteInt32(array.Length);
            for (int i = 0; i < array.Length; i++) writer(array[i]);
        }

        public unsafe T Read<T>() where T : unmanaged
        {
            int size = sizeof(T);
            var obj = new T();
            var ptr = new IntPtr(&obj);
            for (int i = 0; i < size; i++) Marshal.WriteByte(ptr, i, ReadByte());
            return obj;
        }
        public unsafe T[] ReadArray<T>() where T : unmanaged
        {
            int length = ReadInt32();
            T[] array = new T[length];
            for (int i = 0; i < array.Length; i++) array[i] = Read<T>();
            return array;
        }
        public unsafe void ReadArray<T>(ref T[] array, int offset = 0) where T : unmanaged
        {
            int length = ReadInt32();
            for (int i = 0; i < array.Length; i++) array[i + offset] = Read<T>();
        }

        public void WriteIntArray2D(int[,] input)
        {
            int width = input.GetLength(0);
            int height = input.GetLength(1);
            WriteInt32(width);
            WriteInt32(height);
            for (int ix = 0; ix < width; ix++)
            {
                for (int iy = 0; iy < height; iy++)
                {
                    WriteInt32(input[ix, iy]);
                }
            }
        }
        public void WriteFloatArray(float[] input)
        {
            WriteInt32((int)input.Length);
            for (int i = 0; i < input.Length; i++) WriteSingle(input[i]);
        }
        public void WriteString(string input)
        {
            char[] stringData = input.ToCharArray();

            if (input.Length < 256)
            {
                WriteByte(0);
                WriteByte((byte)input.Length);
            }
            else
            {
                WriteByte(1);
                WriteInt32((int)input.Length);
            }
            for (int i = 0; i < stringData.Length; i++)
            {
                WriteByte((byte)stringData[i]);
            }
        }
        public void WriteStringArray(string[] input)
        {
            WriteInt32((int)input.Length);
            for (int i = 0; i < input.Length; i++) WriteString(input[i]);
        }
        #endregion

        #region read
        public byte ReadByte()
        {
            byte value = (byte)BaseStream.ReadByte();
            return value;
        }
        public int ReadInt32()
        {
            return (ReadByte() << 0 | ReadByte() << 8 | ReadByte() << 16 | ReadByte() << 24);
        }
        public float ReadSingle()
        {
            byte[] value = new byte[4];
            BaseStream.Read(value, 0, 4);
            return BitConverter.ToSingle(value, 0);
        }

        public byte[] ReadByteArray()
        {
            byte mode = ReadByte();
            int length;
            if (mode > 3)
            {
                length = ReadByte();
                mode -= 4;
            }
            else length = ReadInt32();

            byte[] retData = new byte[length];

            if (mode == 0)
            {
                for (int i = 0; i < retData.Length; i++)
                {
                    retData[i] = ReadByte();
                }
            }
            else if (mode == 1)
            {
                int curLength = 0;
                while (curLength < length)
                {
                    byte len = ReadByte();
                    byte value = ReadByte();
                    for (int i = 0; i < len + 1; i++)
                        retData[curLength + i] = value;
                    curLength += len + 1;
                }
            }
            else if (mode == 2)
            {
                for (int i = 0; i < retData.Length; i += 2)
                {
                    retData[i] = (byte)(ReadByte() >> 4);
                    retData[i + 1] = (byte)(ReadByte() & 15);
                }
            }

            return retData;
        }
        public int[] ReadIntArray()
        {
            int length = ReadInt32();
            int[] retData = new int[length];
            for (int i = 0; i < retData.Length; i++) retData[i] = ReadInt32();
            return retData;
        }
        public int[,] ReadIntArray2D()
        {
            int width = ReadInt32();
            int height = ReadInt32();
            int[,] retData = new int[width, height];

            for (int ix = 0; ix < width; ix++)
            {
                for (int iy = 0; iy < height; iy++)
                {
                    retData[ix, iy] = ReadInt32();
                }
            }
            return retData;
        }
        public float[] ReadFloatArray()
        {
            int length = ReadInt32();
            float[] retData = new float[length];
            for (int i = 0; i < retData.Length; i++) retData[i] = ReadSingle();
            return retData;
        }

        public string ReadString()
        {
            byte mode = ReadByte();
            int length;
            if (mode == 0) length = ReadByte();
            else length = ReadInt32();

            char[] retData = new char[length];

            for (int i = 0; i < retData.Length; i++)
            {
                retData[i] = (char)ReadByte();
            }

            return new string(retData);
        }
        public string[] ReadStringArray()
        {
            int length = ReadInt32();
            string[] retData = new string[length];
            for (int i = 0; i < retData.Length; i++) retData[i] = ReadString();
            return retData;
        }
        #endregion

        public void Flush()
        {
            BaseStream.Flush();
            /*
            byte[] newData = new byte[size];
            for (int i = 0; i < Math.Min(data.Length, size); i++)
            {
                newData[i] = data[i];
            }
            data = newData;
            */
        }
        public void ResetIndex()
        {
            BaseStream.Seek(0, SeekOrigin.Begin);
        }
        public byte[] GetBytes()
        {
            BaseStream.Flush();
            byte[] result = new byte[BaseStream.Length];
            int backup = Position;
            Position = 0;
            BaseStream.Read(result, 0, (int)BaseStream.Length);
            Position = backup;
            return result;
        }
        public string GetString()
        {
            byte[] data = GetBytes();
            char[] saveData = new char[data.Length];
            for (int i = 0; i < saveData.Length; i++)
            {
                saveData[i] = (char)data[i];
            }
            return new string(saveData);

        }
        public void Save(string path)
        {
            File.WriteAllBytes(path, GetBytes());
        }


    }
}
