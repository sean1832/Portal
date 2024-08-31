using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Portal.Core.Binary;
using Portal.Core.DataModel;
using Portal.Core.Interfaces;

namespace Portal.Gh.Components.Local.Behaviour
{
    internal class NamedPipeServerReceivedBehaviour: IReceivedBehaviour<NamedPipeServerStream>
    {
        private bool TryReadFull(NamedPipeServerStream stream, byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int read = stream.Read(buffer, offset + bytesRead, count - bytesRead);
                if (read == 0)
                {
                    // No more data available at this moment
                    return false;
                }
                bytesRead += read;
            }
            return true;
        }


        public void ProcessData(NamedPipeServerStream server, Action<byte[]> onMessageReceived, Action<Exception> onError)
        {
            try
            {
                while (server.IsConnected)
                {
                    // Read the magic number
                    byte[] magicNumberBuffer = new byte[Packet.MagicNumber.Length];
                    if (!TryReadFull(server, magicNumberBuffer, 0, magicNumberBuffer.Length))
                        break;  // Exit stream if no more data

                    // Validate the magic number
                    for (int i = 0; i < magicNumberBuffer.Length; i++)
                    {
                        if (magicNumberBuffer[i] != Packet.MagicNumber[i])
                        {
                            throw new InvalidDataContractException("Invalid magic number");
                        }
                    }

                    // Read the header
                    int headerLength = PacketHeader.GetExpectedSize();
                    byte[] headerBuffer = new byte[headerLength];
                    if (!TryReadFull(server, headerBuffer, 0, headerLength))
                        break;  // Exit stream if no more data

                    // Deserialize the header
                    PacketHeader header = Packet.DeserializeHeader(headerBuffer);
                    if (header == null)
                    {
                        throw new InvalidDataContractException("Invalid header");
                    }
                    int dataLength = header.Size;

                    // Read the data
                    byte[] dataBuffer = new byte[dataLength];
                    if (!TryReadFull(server, dataBuffer, 0, dataLength))
                        break;  // Exit stream if no more data

                    // Combine magic number, header and data into one buffer for the message
                    byte[] totalBuffer = new byte[magicNumberBuffer.Length + headerBuffer.Length + dataBuffer.Length];
                    Array.Copy(magicNumberBuffer, 0, totalBuffer, 0, magicNumberBuffer.Length);
                    Array.Copy(headerBuffer, 0, totalBuffer, magicNumberBuffer.Length, headerBuffer.Length);
                    Array.Copy(dataBuffer, 0, totalBuffer, magicNumberBuffer.Length + headerBuffer.Length, dataBuffer.Length);
                    

                    // Invoke the message received callback
                    onMessageReceived?.Invoke(totalBuffer);
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
            finally
            {
                server.Disconnect();
            }
        }

    }
}
