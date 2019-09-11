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
        private byte[] readBuffer = new byte[256];
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
        public unsafe void Write<T>(T obj) where T : unmanaged
        {
            int size = sizeof(T);
            var ptr = new IntPtr(&obj);
            if (size == 1) WriteByte(Marshal.ReadByte(ptr, 0));
            else for (int i = 0; i < size; i++) WriteByte(Marshal.ReadByte(ptr, i));
        }
        public void WriteArray<T>(T[] array) where T : unmanaged => WriteArray(array, 0, array.Length);
        public void WriteArray<T>(T[] array, int offset, int length) where T : unmanaged
        {
            WriteInt32(length);
            for (int i = 0; i < length; i++) Write(array[i + offset]);
        }

        public void WriteChar(char input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(char));
        public void WriteByte(byte input) => BaseStream.WriteByte(input);
        public void WriteSByte(sbyte input) => BaseStream.WriteByte((byte)input);
        public void WriteUInt16(ushort input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(ushort));
        public void WriteInt16(short input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(short));
        public void WriteUInt32(uint input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(uint));
        public void WriteInt32(int input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(int));
        public void WriteUInt64(ulong input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(ulong));
        public void WriteInt64(long input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(long));
        public void WriteSingle(float input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(float));
        public void WriteDouble(double input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(double));
        public void WriteDouble(decimal input) => Write(input);

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

        public char ReadChar()
        {
            BaseStream.Read(readBuffer, 0, sizeof(char));
            return BitConverter.ToChar(readBuffer, 0);
        }
        public byte ReadByte() => (byte)BaseStream.ReadByte();
        public sbyte ReadSByte() => (sbyte)BaseStream.ReadByte();
        public ushort ReadUInt16()
        {
            BaseStream.Read(readBuffer, 0, sizeof(ushort));
            return BitConverter.ToUInt16(readBuffer, 0);
        }
        public short ReadInt16()
        {
            BaseStream.Read(readBuffer, 0, sizeof(short));
            return BitConverter.ToInt16(readBuffer, 0);
        }
        public uint ReadUInt32()
        {
            BaseStream.Read(readBuffer, 0, sizeof(uint));
            return BitConverter.ToUInt32(readBuffer, 0);
        }
        public int ReadInt32()
        {
            BaseStream.Read(readBuffer, 0, sizeof(int));
            return BitConverter.ToInt32(readBuffer, 0);
        }
        public ulong ReadUInt64()
        {
            BaseStream.Read(readBuffer, 0, sizeof(ulong));
            return BitConverter.ToUInt64(readBuffer, 0);
        }
        public long ReadInt64()
        {
            BaseStream.Read(readBuffer, 0, sizeof(long));
            return BitConverter.ToInt64(readBuffer, 0);
        }
        public float ReadSingle()
        {
            BaseStream.Read(readBuffer, 0, sizeof(float));
            return BitConverter.ToSingle(readBuffer, 0);
        }
        public double ReadDouble()
        {
            BaseStream.Read(readBuffer, 0, sizeof(double));
            return BitConverter.ToDouble(readBuffer, 0);
        }
        public decimal ReadDecimal()
        {
            return Read<decimal>();
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
