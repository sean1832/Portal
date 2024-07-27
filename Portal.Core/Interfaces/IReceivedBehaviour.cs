using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.Interfaces
{
    public interface IReceivedBehaviour<in TStream>
    {
        /// <summary>
        /// Define handling of incoming data
        /// </summary>
        void ProcessData(TStream stream, Action<byte[]> onMessageReceived, Action<Exception> onError);
    }
}
