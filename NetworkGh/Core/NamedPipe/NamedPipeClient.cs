using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace NetworkGh.Core.NamedPipe
{
    internal class NamedPipeClient: IDisposable
    {
        private readonly string _pipeName;
        private readonly string _serverName;
        private readonly Action<Exception> _onError;
        private NamedPipeClientStream _client;

        private bool _disposed;
        public NamedPipeClient(string serverName, string pipeName,Action<Exception> onError)
        {
            _serverName = serverName;
            _pipeName = pipeName;
            _onError = onError;
        }

        public void Connect()
        {
            try
            {
                _client = new NamedPipeClientStream(_serverName, _pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                _client.Connect(); // Timeout in milliseconds
            }
            catch (Exception e)
            {
                _onError?.Invoke(e);
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (_client == null || !_client.IsConnected)
            {
                _onError?.Invoke(new InvalidOperationException("Client is not connected"));
                return;
            }

            try
            {
                await _client.WriteAsync(data, 0, data.Length);
                await _client.FlushAsync();
            }
            catch (Exception e)
            {
                _onError?.Invoke(e);
            }
        }

        public void Disconnect()
        {
            if (_client == null) return;
            _client.Close();
        }

        // Implement IDisposable
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

        ~NamedPipeClient()
        {
            Dispose(false);
        }
    }
}
