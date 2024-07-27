using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Portal.Core.NamedPipe
{
    public class NamedPipeClient: IDisposable
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

        public byte[] EncodeConnectionFlagPrefix(byte[] data, bool isClose)
        {
            byte[] buffer = new byte[data.Length + 1];
            buffer[0] = isClose ? (byte)1 : (byte)0;
            data.CopyTo(buffer, 1);
            return buffer;
        }

        public byte[] EncodeConnectionFlagPrefix( bool isClose)
        {
            byte[] buffer = new byte[1];
            buffer[0] = isClose ? (byte)1 : (byte)0;
            return buffer;
        }

        public byte[] EncodeLengthPrefix(byte[] data)
        {
            int length = data.Length;
            byte[] lengthBytes = BitConverter.GetBytes(length);
            byte[] buffer = new byte[length + 4]; // 4 bytes for the length
            lengthBytes.CopyTo(buffer, 0); // 0-3 -> length
            data.CopyTo(buffer, lengthBytes.Length); // 4 -> data
            return buffer;
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
