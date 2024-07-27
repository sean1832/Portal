using System;
using System.IO.Pipes;

namespace Portal.Core.NamedPipe
{
    public class NamedPipeServer
    {
        private readonly NamedPipeServerStream _server;
        private bool _isServerRunning;

        private bool _disposed;

        private readonly Action<Exception> _onError;
        private readonly Action<byte[]> _onMessageReceived;
        public NamedPipeServer(string pipeName, int bufferSize, Action<Exception> onError, Action<byte[]> onMessageReceived)
        {
            _onError = onError;
            _onMessageReceived = onMessageReceived;

            try
            {
                _server = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    bufferSize,
                    bufferSize);
            }
            catch (Exception e)
            {
                _onError?.Invoke(e);
            }
        }

        public void Start()
        {
            try
            {
                _server.BeginWaitForConnection(new AsyncCallback(ConnectCallback), null);
                _isServerRunning = true;
            }
            catch (Exception e)
            {
                _onError?.Invoke(e);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            if (_disposed) return;

            try
            {
                _server.EndWaitForConnection(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                _onError?.Invoke(e);
                return;
            }

            if (!_server.IsConnected) return;

            try
            {
                byte[] lengthBuffer = new byte[4];
                int bytesRead = _server.Read(lengthBuffer, 0, lengthBuffer.Length);
                if (bytesRead == 0)
                {
                    _server.Disconnect();
                    return;
                }
                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] buffer = new byte[dataLength];
                while ((bytesRead = _server.Read(buffer, 0, dataLength)) > 0)
                {
                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);
                    _onMessageReceived?.Invoke(receivedData);
                }

                _server.Disconnect();
            }
            catch (Exception e)
            {
                _onError?.Invoke(e);
            }
            finally
            {
                if (_isServerRunning)
                {
                    _server.BeginWaitForConnection(new AsyncCallback(ConnectCallback), ar.AsyncState);
                }
            }
        }

        public void Close()
        {
            _server.Close();
            _isServerRunning = false;
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
                _server.Close();
                _server.Dispose();
            }
            _disposed = true;
        }

        ~NamedPipeServer()
        {
            Dispose(false);
        }
    }
}
