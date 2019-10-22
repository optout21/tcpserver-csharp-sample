using System;
using System.Collections.Generic;

namespace tcpserver_csharp_sample
{
    public class ClientApp: BaseApp
    {
        /// Start the clients, connect, process events
        public virtual void Start(AppParams appParams_in)
        {
            // create and connect clients, before starting the loop
            int n = 3;
            var clis = new NetClientOut[n];
            for (int i = 0; i < n; ++i)
            {
                var nc = new NetClientOut(this, "localhost", appParams_in.listenPort + i, 3 + i);
                clis[i] = nc;
                nc.Start();
            }

            // wait for completions
            for (int i = 0; i < n; ++i)
            {
                clis[i].Join();
            }
            // delete clients
            for (int i = 0; i < n; ++i)
            {
                clis[i] = null;
            }
        }

        public void InConnectionReceived(NetClientBase client_in) { }
        public override void ConnectionClosed(NetClientBase client_in) { }
        public override void MessageReceived(NetClientBase client_in, BaseMessage msg_in)
        {
            Console.WriteLine($"App: Received: from {client_in.GetPeerAddr()} '{msg_in.ToString()}'");
        }
    }
}
