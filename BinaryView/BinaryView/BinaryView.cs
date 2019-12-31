using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace GGL
{
    public enum CompressMode
    {
        Auto = -1,
        None = 0,
        RLE = 1
    }
    public class BinaryView : IDisposable
    {
        private byte[] readBuffer = new byte[16];
        public Stream BaseStream { private set; get; }

        public int Position
        {
            get => (int)BaseStream.Position;
            set { BaseStream.Position = value; }
        }
        public int Length
        {
            get => (int)BaseStream.Length;
        }

        public BinaryView()
        {
            BaseStream = new MemoryStream(0);
        }
        public BinaryView(string path)
        {
            BaseStream = new FileStream(path, FileMode.Open);
        }
        public BinaryView(byte[] bytes)
        {
            BaseStream = new MemoryStream(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                WriteByte(bytes[i]);
            }
        }
        public BinaryView(Stream stream)
        {
            this.BaseStream = stream;
        }

        #region write

        /// <summary>Writes an primitive or unmanaged struct to the stream and increases the position by the size of the struct</summary>
        /// <typeparam name="T"></typeparam> Type of unmanaged struct
        /// <param name="obj">Struct to write</param>
        public unsafe void Write<T>(T obj) where T : unmanaged
        {
            int size = sizeof(T);
            var ptr = new IntPtr(&obj);
            if (size == 1) WriteByte(Marshal.ReadByte(ptr, 0));
            else for (int i = 0; i < size; i++) WriteByte(Marshal.ReadByte(ptr, i));
        }

        /// <summary>Writes an array of unmanaged structs into the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
        /// <typeparam name="T"></typeparam> Type of unmanaged struct
        /// <param name="array">Array of unmanaged structs to write</param>
        public void WriteArray<T>(T[] array) where T : unmanaged => WriteArray(array, 0, array.Length);

        /// <summary>Writes an array of unmanaged structs into the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
        /// <typeparam name="T"></typeparam> Type of unmanaged struct
        /// <param name="array">Array of unmanaged structs to write</param>
        /// <param name="offset">start offset in the array</param>
        /// <param name="count">number of elements to be written</param>
        public void WriteArray<T>(T[] array, int offset, int count) where T : unmanaged
        {
            WriteInt32(count);
            for (int i = 0; i < count; i++) Write(array[i + offset]);
        }

        /// <summary>Writes an char to the stream and increases the position by two byte</summary>
        public void WriteChar(char input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(char));

        /// <summary>Writes an byte to the stream and increases the position by one byte</summary>
        public void WriteByte(byte input) => BaseStream.WriteByte(input);

        /// <summary>Writes an sbyte to the stream and increases the position by one byte</summary>
        public void WriteSByte(sbyte input) => BaseStream.WriteByte((byte)input);

        /// <summary>Writes an ushort to the stream and increases the position by two byte</summary>
        public void WriteUInt16(ushort input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(ushort));

        /// <summary>Writes an short to the stream and increases the position by two byte</summary>
        public void WriteInt16(short input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(short));

        /// <summary>Writes an uint to the stream and increases the position by four byte</summary>
        public void WriteUInt32(uint input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(uint));

        /// <summary>Writes an int to the stream and increases the position by four byte</summary>
        public void WriteInt32(int input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(int));

        /// <summary>Writes an ulong to the stream and increases the position by eight byte</summary>
        public void WriteUInt64(ulong input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(ulong));

        /// <summary>Writes an long to the stream and increases the position by eight byte</summary>
        public void WriteInt64(long input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(long));

        /// <summary>Writes an float to the stream and increases the position by four byte</summary>
        public void WriteSingle(float input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(float));

        /// <summary>Writes an double to the stream and increases the position by eight byte</summary>
        public void WriteDouble(double input) => BaseStream.Write(BitConverter.GetBytes(input), 0, sizeof(double));

        /// <summary>Writes an decimal to the stream and increases the position by sixteen byte</summary>
        public void WriteDecimal(decimal input) => Write(input);

        public void WriteString(string input)
        {
            char[] stringData = input.ToCharArray();

            int max = 0;
            for (int i = 0;i< stringData.Length; i++)
                if (stringData[i] > max) max = stringData[i];

            byte lengthSizeBit = 0;
            byte charSizeBit = 0;

            if (input.Length > byte.MaxValue)
                lengthSizeBit = 1;
            if (max > byte.MaxValue)
                charSizeBit = 1;

            byte meta = (byte)(lengthSizeBit << 0 | charSizeBit << 1);
            WriteByte(meta);


            if (lengthSizeBit == 1)
                WriteInt32((int)input.Length);
            else
                WriteByte((byte)input.Length);

            if (charSizeBit == 1)
                for (int i = 0; i < stringData.Length; i++)
                    WriteChar((char)stringData[i]);
            else
                for (int i = 0; i < stringData.Length; i++)
                    WriteByte((byte)stringData[i]);
        }
        public void WriteStringArray(string[] input)
        {
            WriteInt32((int)input.Length);
            for (int i = 0; i < input.Length; i++) WriteString(input[i]);
        }
        #endregion

        #region read
        /// <summary>Reads an primitive or unmanaged struct from the stream and increases the position by the size of the struct</summary>
        /// <typeparam name="T"></typeparam> Type of unmanaged struct
        public unsafe T Read<T>() where T : unmanaged
        {
            int size = sizeof(T);
            var obj = new T();
            var ptr = new IntPtr(&obj);
            for (int i = 0; i < size; i++) Marshal.WriteByte(ptr, i, ReadByte());
            return obj;
        }

        /// <summary>Reads an array of unmanaged structs from the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
        /// <typeparam name="T"></typeparam> Type of unmanaged struct
        public unsafe T[] ReadArray<T>() where T : unmanaged
        {
            int length = ReadInt32();
            T[] array = new T[length];
            for (int i = 0; i < array.Length; i++) array[i] = Read<T>();
            return array;
        }

        /// <summary>Reads an array of unmanaged structs from the stream and increases the position by the size of the array elements, and 4 bytes for the length</summary>
        /// <typeparam name="T"></typeparam> Type of unmanaged struct
        /// <param name="array">Pointer to existing array to write in</param>
        /// <param name="offset">Offset in array</param>
        public unsafe void ReadArray<T>(T[] array, int offset = 0) where T : unmanaged
        {
            int length = ReadInt32();
            for (int i = 0; i < length; i++) array[i + offset] = Read<T>();
        }

        /// <summary>Reads an char from the stream and increases the position by two byte</summary>
        public char ReadChar()
        {
            BaseStream.Read(readBuffer, 0, sizeof(char));
            return BitConverter.ToChar(readBuffer, 0);
        }

        /// <summary>Reads an byte from the stream and increases the position by one byte</summary>
        public byte ReadByte() => (byte)BaseStream.ReadByte();

        /// <summary>Reads an sbyte from the stream and increases the position by one byte</summary>
        public sbyte ReadSByte() => (sbyte)BaseStream.ReadByte();

        /// <summary>Reads an ushort from the stream and increases the position by two byte</summary>
        public ushort ReadUInt16()
        {
            BaseStream.Read(readBuffer, 0, sizeof(ushort));
            return BitConverter.ToUInt16(readBuffer, 0);
        }

        /// <summary>Reads an short from the stream and increases the position by two byte</summary>
        public short ReadInt16()
        {
            BaseStream.Read(readBuffer, 0, sizeof(short));
            return BitConverter.ToInt16(readBuffer, 0);
        }

        /// <summary>Reads an uint from the stream and increases the position by four byte</summary>
        public uint ReadUInt32()
        {
            BaseStream.Read(readBuffer, 0, sizeof(uint));
            return BitConverter.ToUInt32(readBuffer, 0);
        }

        /// <summary>Reads an int from the stream and increases the position by four byte</summary>
        public int ReadInt32()
        {
            BaseStream.Read(readBuffer, 0, sizeof(int));
            return BitConverter.ToInt32(readBuffer, 0);
        }

        /// <summary>Reads an ulong from the stream and increases the position by eight byte</summary>
        public ulong ReadUInt64()
        {
            BaseStream.Read(readBuffer, 0, sizeof(ulong));
            return BitConverter.ToUInt64(readBuffer, 0);
        }

        /// <summary>Reads an long from the stream and increases the position by eight byte</summary>
        public long ReadInt64()
        {
            BaseStream.Read(readBuffer, 0, sizeof(long));
            return BitConverter.ToInt64(readBuffer, 0);
        }

        /// <summary>Reads an float from the stream and increases the position by four byte</summary>
        public float ReadSingle()
        {
            BaseStream.Read(readBuffer, 0, sizeof(float));
            return BitConverter.ToSingle(readBuffer, 0);
        }

        /// <summary>Reads an double from the stream and increases the position by eight byte</summary>
        public double ReadDouble()
        {
            BaseStream.Read(readBuffer, 0, sizeof(double));
            return BitConverter.ToDouble(readBuffer, 0);
        }

        /// <summary>Reads an decimal from the stream and increases the position by sixteen byte</summary>
        public decimal ReadDecimal()
        {
            return Read<decimal>();
        }

        public string ReadString()
        {
            byte meta = ReadByte();

            int lengthSizeBit = (meta >> 0) & 1;
            int charSizeBit = (meta >> 1) & 1;

            int length;
            if (lengthSizeBit == 1) length = ReadInt32();
            else length = ReadByte();

            char[] retData = new char[length];
            if (charSizeBit == 1)
                for (int i = 0; i < retData.Length; i++)
                    retData[i] = (char)ReadChar();
            else
                for (int i = 0; i < retData.Length; i++)
                    retData[i] = (char)ReadByte();

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
        public void Compress()
        {
            using (var resultStream = new MemoryStream())
            {
                BaseStream.Position = 0;
                using (var compressStream = new DeflateStream(resultStream, CompressionLevel.Optimal, true))
                {
                    BaseStream.CopyTo(compressStream);
                }
                BaseStream.SetLength(0);
                resultStream.Position = 0;
                resultStream.CopyTo(BaseStream);
                BaseStream.Position = 0;
            }
        }
        public void Decompress()
        {
            using (var resultStream = new MemoryStream())
            {
                BaseStream.Position = 0;
                using (var decompressStream = new DeflateStream(BaseStream, CompressionMode.Decompress, true))
                {
                    decompressStream.CopyTo(resultStream);
                }
                BaseStream.SetLength(0);
                resultStream.Position = 0;
                resultStream.CopyTo(BaseStream);
                BaseStream.Position = 0;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) { }

                BaseStream.Dispose();

                disposedValue = true;
            }
        }

         ~BinaryView()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
