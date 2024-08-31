using System;
using System.Collections.Generic;
using System.IO;

namespace Portal.Core.Binary
{

    public class PacketHeader
    {
        public bool IsCompressed { get; } // 1 byte
        public bool IsEncrypted { get; } // 1 byte
        public int Size { get; } // 4 bytes
        public ushort Checksum { get; } // 2 bytes

        public PacketHeader(bool isEncrypted, bool isCompressed, int size, ushort checksum)
        {
            IsEncrypted = isEncrypted;
            IsCompressed = isCompressed;
            Size = size;
            Checksum = checksum;
        }

        public static int GetExpectedSize()
        {
            int result = 0;
            result += sizeof(bool); // isCompressed
            result += sizeof(bool); // isEncrypted
            result += sizeof(int); // size
            result += sizeof(ushort); // checksum
            return result;
        }
    }

    public class Packet
    {
        public byte[] Data { get; }
        public PacketHeader Header { get; }
        public static readonly byte[] MagicNumber = { 0x70, 0x6b }; // pk

        public Packet(byte[] data, bool isEncrypted, bool isCompressed, ushort checksum, int size)
        {
            Data = data;
            Header = new PacketHeader(isEncrypted, isCompressed, size, checksum);
        }

        public Packet(byte[] data, bool isEncrypted, bool isCompressed, ushort checksum)
        {
            Data = data;
            Header = new PacketHeader(isEncrypted, isCompressed, data.Length, checksum);
        }

        public Packet(byte[] data, PacketHeader header)
        {
            Data = data;
            Header = header;
        }

        public byte[] Serialize()
        {
            List<byte> headerBytes = new List<byte>();

            // adding magic number
            headerBytes.AddRange(MagicNumber);

            // adding flags 
            headerBytes.Add((byte)(Header.IsCompressed ? 1 : 0));
            headerBytes.Add((byte)(Header.IsEncrypted ? 1 : 0));

            // adding checksum
            headerBytes.AddRange(BitConverter.GetBytes(Header.Checksum));

            // adding size
            headerBytes.AddRange(BitConverter.GetBytes(Header.Size));

            // combine header and data
            byte[] result = new byte[headerBytes.Count + Data.Length];
            headerBytes.CopyTo(result, 0); // copy header bytes to result
            Array.Copy(Data, 0, result, headerBytes.Count, Data.Length); // copy data to result after header bytes
            
            return result;
        }

        public static void ValidateMagicNumber(byte[] data)
        {
            // minimum size of a packet is the magic number and the header
            if (data.Length < MagicNumber.Length)
            {
                throw new InvalidDataException("Data is too short to be a valid packet");
            }

            // check magic number
            for (int i = 0; i < MagicNumber.Length; i++)
            {
                if (data[i] != MagicNumber[i])
                {
                    throw new InvalidDataException("Data does not contain the magic number");
                }
            }
        }

        public static Packet Deserialize(byte[] data)
        {
            ValidateMagicNumber(data);
            int index = MagicNumber.Length; // start after magic number
            PacketHeader header = DeserializeHeader(data, ref index);

            byte[] payloadData = new byte[header.Size];
            Array.Copy(data, index, payloadData, 0, header.Size);
            Packet packet = new Packet(payloadData, header);

            return packet;
        }

        public static PacketHeader DeserializeHeader(byte[] data, ref int index)
        {
            // read flags
            bool isCompressed = data[index++] == 1;
            bool isEncrypted = data[index++] == 1;

            // read checksum
            ushort checksum = BitConverter.ToUInt16(data, index);
            index += 2;

            // read size
            int size = BitConverter.ToInt32(data, index);
            index += 4;

            return new PacketHeader(isEncrypted, isCompressed, size, checksum);
        }

        public static PacketHeader DeserializeHeader(byte[] data, int startIndex = 0)
        {
            return DeserializeHeader(data, ref startIndex);
        }
    }
}
