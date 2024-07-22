using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkGh.Core.Udp
{
    internal class UdpServerManager : IDisposable
    {
        public event EventHandler<Exception> Error;

        private readonly UdpClient _udpServer;
        private bool _disposed;

        public UdpServerManager(int port)
        {
            _udpServer = new UdpClient(port);
        }

        public void StartListening(Action<byte[], IPEndPoint> onReceiveCallback)
        {

            try
            {
                _udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), onReceiveCallback);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (_disposed) return;
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receivedBytes;

                try
                {
                    receivedBytes = _udpServer.EndReceive(ar, ref clientEndPoint);
                }
                catch (ObjectDisposedException)
                {
                    // Ignore this exception as it is thrown when the UdpClient is closed
                    return;
                }

                // Invoke callback method provided by the user to handle the received data
                Action<byte[], IPEndPoint> receiveAction = (Action<byte[], IPEndPoint>)ar.AsyncState;
                receiveAction?.Invoke(receivedBytes, clientEndPoint);

                // Check again if disposed before continuing to listen.
                if (_disposed) return;

                // Continue listening for more data
                _udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), receiveAction);
            }

            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void Stop()
        {
            _udpServer.Close();
        }

        protected virtual void OnError(Exception ex)
        {
            Error?.Invoke(this, ex);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _udpServer.Close();
                _udpServer.Dispose();
            }
            _disposed = true;
        }

        ~UdpServerManager()
        {
            Dispose(false);
        }
    }
}
