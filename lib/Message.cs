using System;
using System.Collections.Generic;

namespace tcpserver_csharp_sample
{
    public enum MessageType
    {
        Invalid = 0,
        Handshake = 1,
        HandshakeResponse = 2,
        Ping = 3,
        PingResponse = 4,
        OtherPeer = 5
    };

    public class BaseMessage
    {

        public BaseMessage(MessageType type_in)
        {
            myType = type_in;
        }

        public MessageType getType() { return myType; }

        public virtual void visit(IMessageVisitor visitor_in) { }

        private MessageType myType;
    }

    public class HandshakeMessage : BaseMessage
    {
        public HandshakeMessage(string myVersion_in, string yourAddr_in, string myAddr_in) :
            base(MessageType.Handshake)
        {
            myMyVersion = myVersion_in;
            myYourAddr = yourAddr_in;
            myMyAddr = myAddr_in;
        }

        public void visit(IMessageVisitor visitor_in)
        {
            visitor_in.handshake(this);
        }

        public override string ToString()
        {
            return "HandSh " + myMyVersion + " " + myYourAddr + " " + myMyAddr;
        }

        public string getMyVersion() { return myMyVersion; }
        public string getYourAddr() { return myYourAddr; }
        public string getMyAddr() { return myMyAddr; }

        protected string myMyVersion;
        protected string myYourAddr;
        protected string myMyAddr;
    }

    public class HandshakeResponseMessage : BaseMessage
    {
        public HandshakeResponseMessage(string myVersion_in, string myAddr_in, string yourAddr_in) :
            base(MessageType.HandshakeResponse)
        {
            myMyVersion = myVersion_in;
            myMyAddr = myAddr_in;
            myYourAddr = yourAddr_in;
        }

        public void visit(IMessageVisitor visitor_in)
        {
            visitor_in.handshakeResponse(this);
        }

        public override string ToString()
        {
            return "HandShResp " + myMyVersion + " " + myMyAddr + " " + myYourAddr;
        }

        public string getMyVersion() { return myMyVersion; }
        public string getMyAddr() { return myMyAddr; }
        public string getYourAddr() { return myYourAddr; }

        protected string myMyVersion;
        protected string myYourAddr;
        protected string myMyAddr;
    }

    public class PingMessage: BaseMessage
    {
        public PingMessage(string text_in) : base(MessageType.Ping)
        {
            myText = text_in;
        }

        public void visit(IMessageVisitor visitor_in)
        {
            visitor_in.ping(this);
        }

        public override string ToString()
        {
            return "Ping " + myText;
        }

        public string getText() { return myText; }

        protected string myText;
    }

    public class PingResponseMessage : BaseMessage
    {
        public PingResponseMessage(string text_in) : base(MessageType.PingResponse)
        {
            myText = text_in;
        }

        public void visit(IMessageVisitor  visitor_in)
        {
            visitor_in.pingResponse(this);
        }

        public override string ToString()
        {
            return "PingResp " + myText;
        }

        public string getText() { return myText; }
        protected string myText;
    }

    public class OtherPeerMessage : BaseMessage
    { 
        public OtherPeerMessage(string host_in, int port_in) : base(MessageType.OtherPeer)
        {
            myHost = host_in;
            myPort = port_in;
        }

        public void visit(IMessageVisitor visitor_in)
        {
            visitor_in.otherPeer(this);
        }

        public override string ToString()
        {
            return "OtherPeer " + myHost + ":" + myPort.ToString();
        }

        public string getHost() { return myHost; }
        public int getPort() { return myPort; }

        private string myHost;
        private int myPort;
    }

    public interface IMessageVisitor
    {
	    void handshake(HandshakeMessage msg_in);
	    void handshakeResponse(HandshakeResponseMessage msg_in);
        void ping(PingMessage msg_in);
        void pingResponse(PingResponseMessage msg_in);
        void otherPeer(OtherPeerMessage msg_in);
    }

    public class SerializerMessageVisitor: IMessageVisitor
    {
        public void handshake(HandshakeMessage msg_in)
        {
            myMessage = "HANDSH " + msg_in.getMyVersion() + " " + msg_in.getYourAddr() + " " + msg_in.getMyAddr();
        }

        public void handshakeResponse(HandshakeResponseMessage msg_in)
        {
            myMessage = "HANDSHRESP " + msg_in.getMyVersion() + " " + msg_in.getMyAddr() + " " + msg_in.getYourAddr();
        }

        public void ping(PingMessage msg_in)
        {
            myMessage = "PING " + msg_in.getText();
        }

        public void pingResponse(PingResponseMessage msg_in)
        {
            myMessage = "PINGRESP " + msg_in.getText();
        }

        public void otherPeer(OtherPeerMessage msg_in)
        {
            myMessage = "OPEER " + msg_in.getHost() + " " + msg_in.getPort().ToString();
        }

        public string getMessage() { return myMessage; }

        private string myMessage;
    }

    public class MessageDeserializer
    {
        public static BaseMessage parseMessage(string[] tokens)
        {
            if (tokens.Length == 0)
            {
                return null;
            }
            if (tokens[0] == "HANDSH" && tokens.Length >= 4)
            {
                return new HandshakeMessage(tokens[1], tokens[2], tokens[3]);
            }
            else if (tokens[0] == "HANDSHRESP" && tokens.Length >= 4)
            {
                return new HandshakeResponseMessage(tokens[1], tokens[2], tokens[3]);
            }
            else if (tokens[0] == "PING" && tokens.Length >= 2)
            {
                return new PingMessage(tokens[1]);
            }
            else if (tokens[0] == "PINGRESP" && tokens.Length >= 2)
            {
                return new PingResponseMessage(tokens[1]);
            }
            else if (tokens[0] == "OPEER" && tokens.Length >= 3)
            {
                return new OtherPeerMessage(tokens[1], Int32.Parse(tokens[2]));
            }
            return null;
        }
    }
}
