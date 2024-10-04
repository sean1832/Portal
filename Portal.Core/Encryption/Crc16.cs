using System;

namespace Portal.Core.Encryption
{
    /// <summary>
    /// Computing CRC16-CCITT (XModem) checksums for non-cryptographic purposes
    /// </summary>
    public class Crc16
    {
        // Polynomial (CRC-CCITT XModem variant)
        private const ushort Polynomial = 0x1021;
        private readonly ushort[] _table = new ushort[256];

        public Crc16()
        {
            // Precompute the CRC table
            for (ushort i = 0; i < _table.Length; ++i)
            {
                ushort value = 0;
                ushort temp = (ushort)(i << 8);  // Shift left to align with 16 bits
                for (byte j = 0; j < 8; j++)
                {
                    if ((value ^ temp) >= 0x8000)
                    {
                        value = (ushort)((value << 1) ^ Polynomial);
                    }
                    else
                    {
                        value <<= 1;
                    }
                    temp <<= 1;
                }
                _table[i] = value;
            }
        }

        /// <summary>
        /// Compute the CRC16-CCITT checksum of a byte array
        /// </summary>
        /// <param name="bytes">input bytes</param>
        /// <returns>16-bit checksum</returns>
        public ushort ComputeChecksum(byte[] bytes)
        {
            // Initialize CRC value to 0x1D0F
            ushort crc = 0x1d0f;

            foreach (byte b in bytes)
            {
                byte index = (byte)((crc >> 8) ^ b);  // Align the high byte of CRC with input byte
                crc = (ushort)((crc << 8) ^ _table[index]);
            }

            // Finalize and return the CRC (no additional final XOR is required)
            return crc;
        }
    }
}