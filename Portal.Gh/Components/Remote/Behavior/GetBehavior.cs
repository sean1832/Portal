using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace Portal.Gh.Components.Remote.Behavior
{
    internal class GetBehavior : WebSocketBehavior
    {
        public static event Action<string> MessageReceived; 
        public static event Action<string> ErrorOccured;
        protected override void OnMessage(MessageEventArgs e)
        {
            Send("Data Received by server!");
            MessageReceived?.Invoke(e.Data);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Send("Server Error: " + e.Message);
            ErrorOccured?.Invoke(e.Message);
        }
    }
}
