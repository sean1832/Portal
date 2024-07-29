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
            // read the length of the message
            byte[] lengthBuffer = new byte[4];
            int bytesRead = server.Read(lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead == 0)
            {
                server.Disconnect();
                return;
            }
            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

            // read the data
            byte[] buffer = new byte[dataLength];
            while ((bytesRead = server.Read(buffer, 0, dataLength)) > 0)
            {
                byte[] receivedData = new byte[bytesRead];
                Array.Copy(buffer, receivedData, bytesRead);
                onMessageReceived?.Invoke(receivedData);
            }
            server.Disconnect();
        }
    }
}
