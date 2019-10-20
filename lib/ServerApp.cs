using System;
using System.Collections.Generic;

namespace tcpserver_csharp_sample
{
    public class ServerApp: BaseApp
    {
        public ServerApp()
        {
            myNetHandler = new NetHandler(this);
        }

        public void Start(AppParams appParams_in)
        {
            int actualPort = myNetHandler.StartWithListen(appParams_in.listenPort, appParams_in.listenPortRange);
            if (actualPort <= 0)
            {
                return;
            }
            myName = ":" + actualPort.ToString();
        }

        public void Stop()
        {
            myNetHandler.Stop();
        }

        /// Called when a new incoming connection is received
        public void InConnectionReceived(NetClientBase client_in)
        {
            System.Diagnostics.Debug.Assert(client_in != null);
            string cliaddr = client_in.GetNicePeerAddr();
            Console.WriteLine($"App: New incoming connection: {cliaddr}");
            myClients[cliaddr] = client_in;
        }

        /// Called when an incoming connection has finished
        public override void ConnectionClosed(NetClientBase client_in)
        {
            System.Diagnostics.Debug.Assert(client_in != null);
            string cliaddr = client_in.GetPeerAddr();
            Console.WriteLine($"App: Connection done: {cliaddr}");
            foreach(var c in myClients.Keys)
            {
                if (myClients[c] == client_in)
                {
                    myClients.Remove(c);
                    break;
                }
            }
        }

        /// Called when an incoming message is received
        public override void MessageReceived(NetClientBase client_in, BaseMessage msg_in)
        {
            Console.WriteLine($"App: Received: from {client_in.GetNicePeerAddr()} '{msg_in.ToString()}'");
           switch (msg_in.GetType())
            {
                case MessageType.Handshake:
                    {
                        HandshakeMessage hsMsg = msg_in as HandshakeMessage;
                        //cout << "Handshake message received, '" << hsMsg.getMyAddr() << "'" << endl;
                        if (hsMsg.GetMyVersion() != "V01")
                        {
                            Console.Error.WriteLine($"Wrong version '{hsMsg.GetMyVersion()}'");
                            client_in.Close();
                            return;
                        }
                        HandshakeResponseMessage resp = new HandshakeResponseMessage("V01", myName, client_in.GetPeerAddr());
                        client_in.SendMessage(resp);
                    }
                    break;

                case MessageType.Ping:
                    {
                        PingMessage pingMsg = msg_in as PingMessage;
                        //cout << "Ping message received, '" << pingMsg.GetText() << "'" << endl;
                        PingResponseMessage resp = new PingResponseMessage("Resp_from_" + myName + "_to_" + pingMsg.GetText());
                        client_in.SendMessage(resp);
                    }
                    break;

                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
        }

        protected NetHandler myNetHandler;
        protected string myName;
        protected Dictionary<string, NetClientBase> myClients;

    }
}
