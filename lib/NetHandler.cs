using System;
using System.Net;
using System.Net.Sockets;

namespace tcpserver_csharp_sample
{
    class NetHandler
    {
        public NetHandler(IApp app_in)
        {
            myApp = app_in;
        }

        public int startWithListen(int port_in, int tryNextPorts_in)
        {
            int actualPort = doListen(port_in, tryNextPorts_in);
            if (actualPort <= 0)
            {
                return actualPort;
            }
            return actualPort;
        }

        public void doBindAndListen(int port_in)
        {
            Console.Error.WriteLine($"doBindAndListen trying port {port_in}");
            IPAddress localAddr = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEp = new IPEndPoint(localAddr, port_in);
            Socket listeningSocket = new Socket(localEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(localEp);
            listeningSocket.Listen(10);
        }

        public int doListen(int port_in, int tryNextPorts_in)
        {
            int nextPorts = Math.Max(Math.Min(tryNextPorts_in, 10), -1);
            int actualPort = -1;

            for (int i = 0; i < nextPorts; ++i)
            {
                int port = port_in + i;
                doBindAndListen(port);
                // we are bound and listening
                //myListenSocket = listenSoc;
                actualPort = port;
                myApp.ListenStarted(actualPort);
                return actualPort;
            }
            // could not bind anywhere
            return -1;
        }

        private IApp myApp;
    }
}
