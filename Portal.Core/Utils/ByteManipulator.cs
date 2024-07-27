using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.Utils
{
    public static class ByteManipulator
    {
        public static byte[] PrependBytes(byte[] targetBytes, byte[] prefixBytes)
        {
            byte[] result = new byte[targetBytes.Length + prefixBytes.Length];
            prefixBytes.CopyTo(result, 0); // prefix bytes at the beginning
            targetBytes.CopyTo(result, prefixBytes.Length); // target bytes after prefix bytes
            return result;
        }

        public static byte[] PrependBytes(byte[] targetBytes, byte[] prefixBytes, int bufferSize)
        {
            byte[] result = new byte[targetBytes.Length + bufferSize];
            prefixBytes.CopyTo(result, 0); // prefix bytes at the beginning
            targetBytes.CopyTo(result, bufferSize); // target bytes after prefix bytes
            return result;
        }

        public static byte[] PrependBytes(byte[] targetBytes, byte prefixByte)
        {
            byte[] result = new byte[targetBytes.Length + 1];
            result[0] = prefixByte;
            targetBytes.CopyTo(result, 1);
            return result;
        }

        public static byte[] PrependBytes(byte[] targetBytes, bool prefixBool)
        {
            byte[] result = PrependBytes(targetBytes,  BooleanByte(prefixBool));
            return result;
        }

        public static byte BooleanByte(bool boolean)
        {
            return boolean ? (byte)1 : (byte)0;
        }

        public static bool ByteBoolean(byte byteValue)
        {
            return byteValue == 1;
        }
    }
}
