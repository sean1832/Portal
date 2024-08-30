using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.Encryption
{
    /// <summary>
    /// Computing Crc16 checksums for non-cryptographic purposes
    /// </summary>
    public class Crc16
    {
        private const ushort Polynomial = 0xA001; // A001 is the Crc16 polynomial
        private readonly ushort[] _table = new ushort[256];

        public Crc16()
        {
            for (ushort i = 0; i < _table.Length; ++i)
            {
                ushort value = 0;
                var temp = i;
                for (byte j = 0; j < 8; j++)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ Polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                _table[i] = value;
            }
        }

        /// <summary>
        /// Compute the Crc16 checksum of a byte array
        /// </summary>
        /// <param name="bytes">input bytes</param>
        /// <returns>16 bit checksum</returns>
        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ _table[index]);
            }
            return crc;
        }
    }
}
