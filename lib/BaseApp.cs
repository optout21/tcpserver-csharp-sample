using System;
using System.Collections.Generic;

namespace tcpserver_csharp_sample
{
    /// Params for the app.
    public class AppParams
    {
        public AppParams(int listenPort_in, int listenPortRange_in)
        {
            listenPort = listenPort_in;
            listenPortRange = listenPortRange_in;
        }

        public AppParams(string[] extraPeers_in, int listenPort_in, int listenPortRange_in)
        {
            extraPeers = extraPeers_in;
            listenPort = listenPort_in;
            listenPortRange = listenPortRange_in;
        }

        public string[] extraPeers;
        public int listenPort;
        public int listenPortRange;

        //public void print() { }
    };

    /// Application class base
    public abstract class BaseApp: IApp
    {
        //void start(AppParams const appParams_in);
        /// Called when server is listening on a port already
        public void ListenStarted(int port)
        {
            Console.WriteLine($"App: Listening on port {port}");
        }
        /// Called when a new incoming connection is received
        //void inConnectionReceived(std::shared_ptr<NetClientBase>& client_in);
        /// Called when an incoming connection has finished
        public abstract void ConnectionClosed(NetClientBase client_in);
        /// Called when an incoming message is received
        public virtual void MessageReceived(NetClientBase client_in, BaseMessage msg_in) { }
        public virtual string GetName() { return "_NONE_"; }
    }
}
