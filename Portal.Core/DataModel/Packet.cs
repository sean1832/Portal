using System;
using System.Collections.Generic;
using Portal.Core.Encryption;
using Portal.Core.Utils;

namespace Portal.Core.DataModel
{

    public class PacketHeader
    {
        public bool IsCompressed { get; }
        public bool IsEncrypted { get; }
        public int Size { get; }
        public ushort Checksum { get; }
        public long Timestamp { get; private set; }

        public PacketHeader(bool isEncrypted, bool isCompressed, int size, ushort checksum, long timestamp)
        {
            IsEncrypted = isEncrypted;
            IsCompressed = isCompressed;
            Size = size;
            Checksum = checksum;
            Timestamp = timestamp;
        }

        public PacketHeader(bool isEncrypted, bool isCompressed, int size, ushort checksum)
        {
            IsEncrypted = isEncrypted;
            IsCompressed = isCompressed;
            Size = size;
            Checksum = checksum;
        }

        public void UpdateTimestamp(long timestamp)
        {
            Timestamp = timestamp;
        }
    }

    public class Packet
    {
        public byte[] Data { get; }
        public PacketHeader Header { get; }

        public Packet(byte[] data, int size, ushort checksum, long timestamp, bool isEncrypted, bool isCompressed)
        {
            Data = data;
            Header = new PacketHeader(isEncrypted, isCompressed, size, checksum, timestamp);
        }

        public Packet(byte[] data, bool isEncrypted, bool isCompressed, long timestamp, ushort checksum)
        {
            Data = data;
            Header = new PacketHeader(isEncrypted, isCompressed, data.Length, checksum, timestamp);
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

            // adding flags 
            headerBytes.Add((byte)(Header.IsCompressed ? 1 : 0));
            headerBytes.Add((byte)(Header.IsEncrypted ? 1 : 0));

            // adding checksum
            headerBytes.AddRange(BitConverter.GetBytes(Header.Checksum));

            // adding size
            headerBytes.AddRange(BitConverter.GetBytes(Header.Size));

            // adding timestamp
            headerBytes.AddRange(BitConverter.GetBytes(Header.Timestamp));

            // combine header and data
            byte[] result = new byte[headerBytes.Count + Data.Length];
            headerBytes.CopyTo(result, 0); // copy header bytes to result
            Array.Copy(Data, 0, result, headerBytes.Count, Data.Length); // copy data to result after header bytes
            
            return result;
        }

        public static Packet Deserialize(byte[] data)
        {
            PacketHeader header = DeserializeHeader(data, out var index);

            byte[] payloadData = new byte[header.Size];
            Array.Copy(data, index, payloadData, 0, header.Size);
            Packet packet = new Packet(payloadData, header);

            return packet;
        }

        public static PacketHeader DeserializeHeader(byte[] data, out int index)
        {
            
            index = 0;
            if (data.Length == 0)
            {
                return null;
            }

            // read flags
            bool isCompressed = data[index++] == 1;
            bool isEncrypted = data[index++] == 1;

            // read checksum
            ushort checksum = BitConverter.ToUInt16(data, index);
            index += 2;

            // read size
            int size = BitConverter.ToInt32(data, index);
            index += 4;

            // read timestamp
            long timestamp = BitConverter.ToInt64(data, index);
            index += 8;

            return new PacketHeader(isEncrypted, isCompressed, size, checksum, timestamp);
        }

        public static PacketHeader DeserializeHeader(byte[] data)
        {
            return DeserializeHeader(data, out _);
        }
    }
}
