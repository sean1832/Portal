using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    // Read the length of the message
                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = server.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0)
                    {
                        break; // End of stream
                    }
                    int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

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

                    if (totalBytesRead == dataLength)
                    {
                        onMessageReceived?.Invoke(buffer);
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
