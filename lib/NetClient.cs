using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace tcpserver_csharp_sample
{
    public class NetClientBase
    {
        public enum State
        {
            Undefined = 0,
            NotConnected,
            Connecting,
            Connected,
            Accepted,
            Sending,
            Sent,
            Receiving,
            Received,
            Closing,
            Closed
        }

        public NetClientBase(IApp app_in, string peerAddr_in)
        {
            myApp = app_in;
            myPeerAddr = peerAddr_in;
            myState = State.NotConnected;
        }

        public string GetPeerAddr() { return myPeerAddr; }
        public string GetCanonPeerAddr() { return myCanonPeerAddr; }
        public string GetNicePeerAddr() { return myCanonPeerAddr.Length > 0 ? myCanonPeerAddr : myPeerAddr; }
        public void SetCanonPeerAddr(string peerAddr_in) { myCanonPeerAddr = peerAddr_in; }

        public void SendMessage(BaseMessage msg_in)
        {
            //Console.Error.WriteLine($"NetClientBase.SendMessage '{msg_in.ToString()}' {myState}");
            if (myState == State.Closing || myState == State.Closed)
            {
                throw new ApplicationException($"Invalid state {myState}");
            }
            myState = State.Sending;
            SerializerMessageVisitor visitor = new SerializerMessageVisitor();
            msg_in.Visit(visitor);
            string msg = visitor.GetMessage();
            //Console.Error.WriteLine($"SendMessage {msg.Length} '{msg}'");
            msg += '\n'; // terminator
            // convert to byte array
            byte[] msgByte = System.Text.Encoding.ASCII.GetBytes(msg);
            try
            {
                /*int sendRes = */mySocket.Send(msgByte);
                //Console.Error.WriteLine($"sendRes {sendRes}");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Error when writing socket {ex.Message}");
                Close();
            }
            return;
        }

        public void Close()
        {
            //cout << "NetClientBase::close " << getPeerAddr() << endl;
            myState = State.Closing;
            if (!mySocket.Connected)
            {
                // already closing
                Console.Error.WriteLine($"Warning: Socket is already closing {GetPeerAddr()}");
            }
            else
            {
                mySocket.Close();
            }
            if (myApp != null)
            {
                myApp.ConnectionClosed(this);
            }
            myState = State.Closed;
        }

        protected void DoRead()
        {
            // communicate with client, process one message
            //Console.Error.WriteLine($"DoRead {myState}");
            System.Diagnostics.Debug.Assert(myState == State.Accepted || myState == State.Sent || myState == State.Sending || myState == State.Receiving);
            myState = State.Receiving;
            //cout << "doRead " << endl; //(long)((IUvSocket*)this) << " " << (long)((NetClientBase*)this) << " " << (long)((NetClientIn*)this) << endl;
            //myReceiveBuffer.clear();
            byte[] buf = new byte[1024];
            int readres = mySocket.Receive(buf);
            //Console.Error.WriteLine($"DoRead readres {readres}");
            if (readres > 0)
            {
                myReceiveBuffer += System.Text.Encoding.ASCII.GetString(buf, 0, readres);
                DoProcessReceivedBuffer();
            }
        }

        protected void DoProcessReceivedBuffer()
        {
            if (myReceiveBuffer.Length == 0)
            {
                return;
            }
            //Console.Error.WriteLine($"DoProcessReceivedBuffer {myReceiveBuffer.Length}");
            int terminatorIdx;
            while ((myReceiveBuffer.Length > 0) && ((terminatorIdx = myReceiveBuffer.IndexOf('\n')) >= 0))
            {
                string msg1 = myReceiveBuffer.Substring(0, terminatorIdx); // without the terminator
                myReceiveBuffer = myReceiveBuffer.Substring(terminatorIdx + 1);
                //cout << "Incoming message: from " << myPeerAddr << " '" << msg1 << "' " << myReceiveBuffer.length() << endl;
                // split into tokens
                string[] tokens = msg1.Split(' ');
                BaseMessage msg = MessageDeserializer.ParseMessage(tokens);
                if (msg == null)
                {
                    Console.Error.WriteLine($"Error: Unparseable message '{msg1}' {tokens.Length}");
                    continue;
                }
                myState = State.Received;
                System.Diagnostics.Debug.Assert(myApp != null);
                myApp.MessageReceived(this, msg);
            }
        }

        protected IApp myApp;
        protected State myState;
        private string myPeerAddr;
        private string myCanonPeerAddr;
        private string myReceiveBuffer;
        protected Socket mySocket;
    }

/*
void NetClientBase::on_write(uv_write_t* req, int status) 
{
    //cout << "on_write " << status << endl;
    UvWriteRequest* wrreq = (UvWriteRequest*)req->data;
    if (wrreq == nullptr)
    {
        cerr << "Fatal error: uv_write_t->data is nullptr " << endl;
        //uv_close((uv_handle_t*)req->handle, NULL);
        return;
    }
    IUvSocket* uvSocket = wrreq->uvSocket;
    if (uvSocket == nullptr)
    {
        cerr << "Fatal error: uvSocket is nullptr " << endl;
        //uv_close((uv_handle_t*)req->handle, NULL);
        return;
    }
    uvSocket->onWrite(req, status);
    delete wrreq;
    delete req;
}

void NetClientBase::onWrite(uv_write_t* req, int status) 
{
    //cout << "NetClientBase::onWrite " << status << " "  << myState << endl;
    assert(myState == State::Sending || myState == State::Receiving || myState == State::Received);
    if (status != 0) 
    {
        cerr << "write error " << status << " " << ::uv_strerror(status) << endl;
        //uv_close((uv_handle_t*) req->handle, NULL);
        close();
        return;
    }
    process();
}

void NetClientBase::on_read(uv_stream_t* stream, ssize_t nread, const uv_buf_t* buf)
{
    //cout << "on_read " << nread << endl; // << " " << (long)buf << " " << (long)buf->base << endl;
    assert(stream != NULL);
    IUvSocket* uvSocket = (IUvSocket*)(stream->data);
    if (uvSocket == nullptr)
    {
        cerr << "Fatal error: uvSocket is nullptr " << endl;
        //uv_close((uv_handle_t*)stream, NULL);
        //delete stream;
        return;
    }
    //cerr << (long)uvSocket << " " << buf->base[0] << endl;
    uvSocket->onRead(stream, nread, buf);
}

void NetClientBase::alloc_buffer(uv_handle_t* handle, size_t suggested_size, uv_buf_t* buf)
{
    //cerr << "alloc_buffer " << suggested_size << endl;
    size_t s = std::min(suggested_size, (size_t)16384);
    buf->base = new char[s];
    buf->len = s;
}

void NetClientBase::onRead(uv_stream_t* stream, ssize_t nread, const uv_buf_t* buf)
{
    //cout << "onRead " << myPeerAddr << " " << nread << endl;
    if (nread < 0)
    {
        string errtxt = ::uv_strerror(nread);
        if (errtxt == "end of file")
        {
            // ...
        }
        else
        {
            cerr << "Read error " << errtxt << " " << nread << " pending " << myReceiveBuffer.length() << endl;
        }
        // close socket
        close();
        //delete stream;
        return;
    }
    if (nread == 0)
    {
        cerr << "Socket closed while reading " << ::uv_strerror(nread) << "  pending " << myReceiveBuffer.length() << endl;
        close();
        //delete stream;
        return;
    }
    if (buf != nullptr && buf->base != nullptr)
    {
        myReceiveBuffer.append(string(buf->base, nread));
        //cerr << "ReceiveBuffer increased to " << myReceiveBuffer.length() << endl;
        doProcessReceivedBuffer();
    }
    //delete stream;

    process();
}

int NetClientBase::doRead()
{
    // communicate with client, process one message
    //cout << "doRead " << myState << endl;
    assert(myState == State::Accepted || myState == State::Sent || myState == State::Sending || myState == State::Receiving);
    myState = State::Receiving;
    //cout << "doRead " << endl; //(long)((IUvSocket*)this) << " " << (long)((NetClientBase*)this) << " " << (long)((NetClientIn*)this) << endl;
    //myReceiveBuffer.clear();
    //static const int buflen = 256;
    //char buffer[buflen];
    ((uv_stream_t*)myUvStream)->data = (void*)dynamic_cast<IUvSocket*>(this);
    int res = ::uv_read_start((uv_stream_t*)myUvStream, NetClientBase::alloc_buffer, NetClientBase::on_read);
    if (res < 0)
    {
        cerr << "Error from uv_read_start() " << res << " "<< ::uv_err_name(res) << endl;
        close();
        return res;
    }
    return 0;
}

bool NetClientBase::isConnected() const
{
    if (myUvStream == nullptr) return false;
    if (myState == State::Undefined || myState == State::NotConnected || myState == State::Connecting || myState == State::Closing || myState == State::Closed) return false;
    uv_os_fd_t fd;
    if (::uv_fileno((uv_handle_t*)myUvStream, &fd)) return false;
    if (fd <= 0) return false;
    return true;
}

NetClientIn::NetClientIn(ServerApp* app_in, uv_tcp_t* socket_in, string const & peerAddr_in) :
NetClientBase(app_in, peerAddr_in)
{
    setUvStream(socket_in);
    myState = State::Accepted;
}
*/

    public class NetClientOut: NetClientBase
    {
        private Thread myThread;
        private string myHost;
        private int myPort;
        private int myPingToSend;
        private int mySendCounter;

        public NetClientOut(IApp app_in, string host_in, int port_in, int pingToSend_in)
            : base(app_in, host_in + ":" + port_in.ToString())
        {
            myHost = host_in;
            myPort = port_in;
            myPingToSend = pingToSend_in;
            mySendCounter = 0;
        }

        public void DoConnect()
        {
            try
            {
                if (myState >= State.Connected && myState < State.Closed)
                {
                    var msg = $"Fatal error: Connect on connected connection {myState}";
                    Console.WriteLine(msg);
                    throw new ApplicationException(msg);
                }
                myState = State.Connecting;
                mySendCounter = 0;
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mySocket.Connect(myHost, myPort);
                // obtain connected remote IP
                //string remoteHost = socket.RemoteEndPoint
                //int remotePort;
                string canonEp = String.Empty;
                string remoteHost = mySocket.RemoteEndPoint.ToString().Split(':')[0];
                string remotePort = mySocket.RemoteEndPoint.ToString().Split(':')[1];
                if (remoteHost != myHost)
                {
                    canonEp = remoteHost + ":" + myPort.ToString();
                    Console.WriteLine($"Canonical endpoint of {myHost}:{myPort} is {canonEp}");
                    SetCanonPeerAddr(canonEp);
                }
                //// obtain canonical endpoint: IP is connected remote IP, port is original port
                //string canonEp;
                //if (remoteHost != myHost)
                //{
                //    canonEp = remoteHost + ":" + to_string(myPort);
                //    cout << "Canonical endpoint of " << myHost << ":" << myPort << " is " << canonEp << endl;
                //    setCanonPeerAddr(canonEp);
                //}

                myState = State.Connected;
                Console.WriteLine($"Connected to {myHost}:{myPort} ({canonEp} {remoteHost}:{remotePort})");

                while (true)
                {
                    Process();
                    if (myState == State.Closed)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error on out socket {ex.Message}");
            }
        }

        public void Start()
        {
            myThread = new Thread(new ThreadStart(DoConnect));
            myThread.Start();
        }

        public void Join()
        {
            myThread.Join();
        }

        // Perform state-dependent next action in the client state diagram
        public virtual void Process()
        {
            //Console.Error.WriteLine($"Process {myState}");
            switch (myState)
            {
                case State.Connected:
                    {
                        mySendCounter = 0;
                        var msg = new HandshakeMessage("V01", GetPeerAddr(), myApp.GetName());
                        SendMessage(msg);
                    }
                    break;

                case State.Sending:
                    ++mySendCounter;
                    DoRead();
                    break;

                case State.Sent:
                    DoRead();
                    break;

                case State.Receiving:
                    DoRead();
                    break;

                case State.Received:
                    if (mySendCounter >= 1 + myPingToSend)
                    {
                        Close();
                    }
                    else if (mySendCounter >= 1)
                    {
                        var msg = new PingMessage("Ping_from_" + myApp.GetName() + "_to_" + GetPeerAddr() + "_" + mySendCounter.ToString());
                        SendMessage(msg);
                    }
                    break;

                default:
                    Console.Error.WriteLine($"Fatal error: unhandled state {myState}");
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
            return;
        }
    }
}
