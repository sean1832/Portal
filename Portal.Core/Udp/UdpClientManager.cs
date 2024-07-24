using System;
using System.Net.Sockets;

namespace Portal.Core.Udp
{
    public class UdpClientManager : IDisposable
    {
        public event EventHandler<Exception> Error;
        private readonly UdpClient _client;
        private readonly string _ip;
        private readonly int _port;
        private bool _disposed;

        public UdpClientManager(string ip, int port)
        {
            _ip = ip;
            _port = port;
            _client = new UdpClient();
        }

        public void Send(byte[] data)
        {
            try
            {
                _client.Send(data, data.Length, _ip, _port);
            }
            catch (Exception e)
            {
                OnError(e);
            }

        }

        public void Close()
        {
            _client.Close();
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
                _client.Close();
                _client.Dispose();
            }
            _disposed = true;
        }
        ~UdpClientManager()
        {
            Dispose(false);
        }
    }
}
