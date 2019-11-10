using System;
using System.IO;

namespace UnityHelpers
{
    public static class DataParser
    {
        #region 1 byte structures
        public static bool ReadBool(Stream stream)
        {
            byte[] boolArray = new byte[1];
            stream.Read(boolArray, 0, 1);
            return BitConverter.ToBoolean(boolArray, 0);
        }
        public static sbyte ReadSByte(Stream stream)
        {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            return Convert.ToSByte(buffer[0]);
        }
        public static byte ReadByte(Stream stream)
        {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            return buffer[0];
        }
        public static char ReadChar(Stream stream)
        {
            byte[] buffer = new byte[2];
            stream.Read(buffer, 0, 1);
            return BitConverter.ToChar(buffer, 0);
        }
        #endregion

        #region 2 byte structures
        public static short ReadShort(Stream stream)
        {
            byte[] shortBytes = new byte[2];
            stream.Read(shortBytes, 0, 2);
            return BitConverter.ToInt16(shortBytes, 0);
        }
        public static ushort ReadUShort(Stream stream)
        {
            byte[] ushortBytes = new byte[2];
            stream.Read(ushortBytes, 0, 2);
            return BitConverter.ToUInt16(ushortBytes, 0);
        }
        #endregion

        #region 4 byte structures
        public static float ReadFloat(Stream stream)
        {
            byte[] floatBytes = new byte[4];
            stream.Read(floatBytes, 0, 4);
            return BitConverter.ToSingle(floatBytes, 0);
        }
        public static int ReadInt(Stream stream)
        {
            byte[] intBytes = new byte[4];
            stream.Read(intBytes, 0, 4);
            return BitConverter.ToInt32(intBytes, 0);
        }
        public static uint ReadUInt(Stream stream)
        {
            byte[] uintBytes = new byte[4];
            stream.Read(uintBytes, 0, 4);
            return BitConverter.ToUInt32(uintBytes, 0);
        }
        #endregion

        #region 8 byte structures
        public static double ReadDouble(Stream stream)
        {
            byte[] doubleBytes = new byte[8];
            stream.Read(doubleBytes, 0, 8);
            return BitConverter.ToDouble(doubleBytes, 0);
        }
        public static long ReadLong(Stream stream)
        {
            byte[] longBytes = new byte[8];
            stream.Read(longBytes, 0, 8);
            return BitConverter.ToInt64(longBytes, 0);
        }
        public static ulong ReadULong(Stream stream)
        {
            byte[] ulongBytes = new byte[8];
            stream.Read(ulongBytes, 0, 8);
            return BitConverter.ToUInt64(ulongBytes, 0);
        }
        #endregion

        #region 16 byte structures
        public static decimal ReadDecimal(Stream stream)
        {
            return new decimal(new int[] { ReadInt(stream), ReadInt(stream), ReadInt(stream), ReadInt(stream) }); //Big endian probably doesn't work, each individual int is flipped but their ordering is probably wrong
        }
        #endregion

        #region Other
        public static void CopyTo(this Stream from, Stream to, long amount, int bufferSize = 81920)
        {
            long totalCopied = 0;
            byte[] buffer = new byte[bufferSize];
            int actualAmountRead;
            do
            {
                int readLength = (int)Math.Min(amount - totalCopied, bufferSize);
                actualAmountRead = from.Read(buffer, 0, readLength);
                if (actualAmountRead > 0)
                    to.Write(buffer, 0, actualAmountRead);
                totalCopied += actualAmountRead;
            }
            while (actualAmountRead > 0);
        }
        #endregion
    }
}