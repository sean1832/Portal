using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Portal.Core.DataModel;
using Portal.Core.Interfaces;

namespace Portal.Gh.Components.Local.Behaviour
{
    internal class NamedPipeServerReceivedBehaviour: IReceivedBehaviour<NamedPipeServerStream>
    {
        public void ProcessData(NamedPipeServerStream server, Action<byte[]> onMessageReceived, Action<Exception> onError)
        {
            try
            {
                while (server.IsConnected)
                {
                    //// Read the length of the message
                    //byte[] lengthBuffer = new byte[4];
                    //int bytesRead = server.Read(lengthBuffer, 0, lengthBuffer.Length);
                    //if (bytesRead == 0)
                    //{
                    //    break; // End of stream
                    //}
                    //int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                    // read the header
                    int headerLength = 8;
                    byte[] headerBuffer = new byte[headerLength];
                    int bytesRead = server.Read(headerBuffer, 0, headerBuffer.Length);
                    if (bytesRead == 0)
                    {
                        break; // End of stream
                    }
                    PacketHeader header = Packet.DeserializeHeader(headerBuffer);
                    if (header == null)
                    {
                        throw new InvalidDataContractException("Invalid header");
                    }
                    int dataLength = header.Size;

                    // Read the data
                    byte[] buffer = new byte[dataLength];
                    int totalBytesRead = 0;
                    while (totalBytesRead < dataLength)
                    {
                        bytesRead = server.Read(buffer, totalBytesRead, dataLength - totalBytesRead);
                        if (bytesRead == 0)
                        {
                            break; // End of stream
                        }
                        totalBytesRead += bytesRead;
                    }

                    byte[] totalBuffer = new byte[dataLength + headerLength];
                    Array.Copy(headerBuffer, totalBuffer, headerLength);
                    Array.Copy(buffer, 0, totalBuffer, headerLength, dataLength);

                    if (totalBytesRead == dataLength)
                    {
                        onMessageReceived?.Invoke(totalBuffer);
                    }
                    else
                    {
                        throw new Exception("Incomplete message received");
                    }
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
