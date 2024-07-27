using System;
using System.IO.Pipes;
using Portal.Core.Interfaces;

namespace Portal.Core.NamedPipe
{
    public class NamedPipeServer
    {
        private readonly NamedPipeServerStream _server;
        private bool _isServerRunning;

        private bool _disposed;

        private readonly Action<Exception> _onError;
        private readonly Action<byte[]> _onMessageReceived;
        private readonly IReceivedBehaviour<NamedPipeServerStream> _receivedBehaviour;
        public NamedPipeServer(string pipeName, int bufferSize, 
            Action<Exception> onError, Action<byte[]> onMessageReceived, 
            IReceivedBehaviour<NamedPipeServerStream> receivedBehaviour = null)
        {
            _onError = onError;
            _onMessageReceived = onMessageReceived;
            _receivedBehaviour = receivedBehaviour ?? new DefaultNamedPipeServerReceivedBehaviour();
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
                _receivedBehaviour.ProcessData(_server, _onMessageReceived, _onError);
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
