using System;

namespace tcpserver_csharp_sample
{
    /// Application class interface.
    interface IApp
    {
        //void start(AppParams const appParams_in);
        /// Called when server is listening on a port already
        void ListenStarted(int port);
        /// Called when a new incoming connection is received
        //void inConnectionReceived(std::shared_ptr<NetClientBase>& client_in);
        /// Called when an incoming connection has finished
        void ConnectionClosed(NetClientBase client_in);
        /// Called when an incoming message is received
        //void messageReceived(NetClientBase & client_in, BaseMessage const & msg_in);
        //std::string getName();
    }
}
