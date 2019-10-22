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

        public MessageType GetType() { return myType; }

        public virtual void Visit(IMessageVisitor visitor_in) { }

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

        public override void Visit(IMessageVisitor visitor_in)
        {
            visitor_in.Handshake(this);
        }

        public override string ToString()
        {
            return "HandSh " + myMyVersion + " " + myYourAddr + " " + myMyAddr;
        }

        public string GetMyVersion() { return myMyVersion; }
        public string GetYourAddr() { return myYourAddr; }
        public string GetMyAddr() { return myMyAddr; }

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

        public override void Visit(IMessageVisitor visitor_in)
        {
            visitor_in.HandshakeResponse(this);
        }

        public override string ToString()
        {
            return "HandShResp " + myMyVersion + " " + myMyAddr + " " + myYourAddr;
        }

        public string GetMyVersion() { return myMyVersion; }
        public string GetMyAddr() { return myMyAddr; }
        public string GetYourAddr() { return myYourAddr; }

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

        public override void Visit(IMessageVisitor visitor_in)
        {
            visitor_in.Ping(this);
        }

        public override string ToString()
        {
            return "Ping " + myText;
        }

        public string GetText() { return myText; }

        protected string myText;
    }

    public class PingResponseMessage : BaseMessage
    {
        public PingResponseMessage(string text_in) : base(MessageType.PingResponse)
        {
            myText = text_in;
        }

        public override void Visit(IMessageVisitor  visitor_in)
        {
            visitor_in.PingResponse(this);
        }

        public override string ToString()
        {
            return "PingResp " + myText;
        }

        public string GetText() { return myText; }
        protected string myText;
    }

    public class OtherPeerMessage : BaseMessage
    { 
        public OtherPeerMessage(string host_in, int port_in) : base(MessageType.OtherPeer)
        {
            myHost = host_in;
            myPort = port_in;
        }

        public override void Visit(IMessageVisitor visitor_in)
        {
            visitor_in.OtherPeer(this);
        }

        public override string ToString()
        {
            return "OtherPeer " + myHost + ":" + myPort.ToString();
        }

        public string GetHost() { return myHost; }
        public int GetPort() { return myPort; }

        private string myHost;
        private int myPort;
    }

    public interface IMessageVisitor
    {
	    void Handshake(HandshakeMessage msg_in);
	    void HandshakeResponse(HandshakeResponseMessage msg_in);
        void Ping(PingMessage msg_in);
        void PingResponse(PingResponseMessage msg_in);
        void OtherPeer(OtherPeerMessage msg_in);
    }

    public class SerializerMessageVisitor: IMessageVisitor
    {
        public void Handshake(HandshakeMessage msg_in)
        {
            myMessage = "HANDSH " + msg_in.GetMyVersion() + " " + msg_in.GetYourAddr() + " " + msg_in.GetMyAddr();
        }

        public void HandshakeResponse(HandshakeResponseMessage msg_in)
        {
            myMessage = "HANDSHRESP " + msg_in.GetMyVersion() + " " + msg_in.GetMyAddr() + " " + msg_in.GetYourAddr();
        }

        public void Ping(PingMessage msg_in)
        {
            myMessage = "PING " + msg_in.GetText();
        }

        public void PingResponse(PingResponseMessage msg_in)
        {
            myMessage = "PINGRESP " + msg_in.GetText();
        }

        public void OtherPeer(OtherPeerMessage msg_in)
        {
            myMessage = "OPEER " + msg_in.GetHost() + " " + msg_in.GetPort().ToString();
        }

        public string GetMessage() { return myMessage; }

        private string myMessage;
    }

    public class MessageDeserializer
    {
        public static BaseMessage ParseMessage(string[] tokens)
        {
            //Console.Error.WriteLine($"ParseMessage {tokens.Length} '{tokens[0]}'");
            if (tokens.Length == 0)
            {
                return null;
            }
            if (tokens[0].Equals("HANDSH") && tokens.Length >= 4)
            {
                return new HandshakeMessage(tokens[1], tokens[2], tokens[3]);
            }
            else if (tokens[0].Equals("HANDSHRESP") && tokens.Length >= 4)
            {
                return new HandshakeResponseMessage(tokens[1], tokens[2], tokens[3]);
            }
            else if (tokens[0].Equals("PING") && tokens.Length >= 2)
            {
                return new PingMessage(tokens[1]);
            }
            else if (tokens[0].Equals("PINGRESP") && tokens.Length >= 2)
            {
                return new PingResponseMessage(tokens[1]);
            }
            else if (tokens[0].Equals("OPEER") && tokens.Length >= 3)
            {
                return new OtherPeerMessage(tokens[1], Int32.Parse(tokens[2]));
            }
            return null;
        }
    }
}
