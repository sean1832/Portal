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
            byte[] connectionFlagBuffer = new byte[1];
            int connectionFlagBytesRead = server.Read(connectionFlagBuffer, 0, connectionFlagBuffer.Length);
            if (connectionFlagBytesRead == 0)
            {
                server.Disconnect();
                return;
            }
            bool isConnected = connectionFlagBuffer[0] == 1;
            if (!isConnected)
            {
                server.Disconnect();
                return;
            }

            byte[] lengthBuffer = new byte[4];
            int bytesRead = server.Read(lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead == 0)
            {
                server.Disconnect();
                return;
            }
            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
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
