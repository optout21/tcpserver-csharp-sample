using System;
using System.Net.Sockets;

namespace tcpserver_csharp_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TCP CSharp Server");

            ClientApp app = new ClientApp();
            app.Start(new AppParams(5000, 5));

            /*ServerApp app = new ServerApp();
            app.Start(new AppParams(5000, 5));

            Console.Write("Press Enter to exit ...");
            Console.ReadLine();
            Console.WriteLine();

            app.Stop();
            Console.WriteLine("Done");
            */

            /*
            string host = "localhost";
            int port = 5000;
            string myAddr = host + ":" + port.ToString();
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(host, port);

            // TODO: obtain actual remote EP of the socket
            string remoteEP = socket.LocalEndPoint.ToString();
            string handShakeMsg = "HANDSH " + "V01" + " " + remoteEP + " " + myAddr + "\n";
            byte[] msgSer = System.Text.Encoding.ASCII.GetBytes(handShakeMsg);
            int sendRes = socket.Send(msgSer);
            Console.WriteLine($"sendRes {sendRes}");
            byte[] recBuffer = new byte[256];
            int recRes = socket.Receive(recBuffer);
            Console.WriteLine($"recRes {recRes}");
            Console.WriteLine($" {System.Text.Encoding.ASCII.GetString(recBuffer)}");
            */
        }
    }
}
