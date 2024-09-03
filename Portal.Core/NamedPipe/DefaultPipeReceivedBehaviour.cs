using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Core.Interfaces;

namespace Portal.Core.NamedPipe
{
    internal class DefaultPipeReceivedBehaviour: IReceivedBehaviour<NamedPipeServerStream>
    {
        public void ProcessData(NamedPipeServerStream stream, Action<byte[]> onMessageReceived, Action<Exception> onError)
        {
            const int bufferSize = 4096; // Define a buffer size
            byte[] buffer = new byte[bufferSize];

            try
            {
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);
                    onMessageReceived?.Invoke(receivedData);
                }
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
            finally
            {
                stream.Disconnect();
            }
        }
    }
}
