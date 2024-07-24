using System;
using WebSocketSharp;

namespace Portal.Core.WebSocket
{
    public class WebSocketClientManager
    {
        private WebSocketSharp.WebSocket _ws;

        public event EventHandler<string> MessageReceived;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<ErrorEventArgs> Error;
        public bool IsConnected => _ws != null && _ws.ReadyState == WebSocketState.Open;
        public void Connect(string uri)
        {
            if (_ws != null)
            {
                if (IsConnected)
                    return; // Already connected

                Disconnect(); // Cleanup existing connection before reconnecting
            }

            // new instance of WebSocket
            _ws = new WebSocketSharp.WebSocket(uri);
            _ws.OnMessage += (sender, e) => MessageReceived?.Invoke(this, e.Data);
            _ws.OnOpen += (sender, e) => Connected?.Invoke(this, EventArgs.Empty);
            _ws.OnClose += (sender, e) => Disconnected?.Invoke(this, EventArgs.Empty);
            _ws.OnError += (sender, e) => Error?.Invoke(this, e);

            _ws.Connect();
        }
        public void Disconnect()
        {
            if (_ws == null) return;
            _ws.Close();
            _ws = null;
        }

        public void Send(string message)
        {
            if (_ws != null && _ws.IsAlive)
            {
                _ws.Send(message);
            }
        }
    }
}
