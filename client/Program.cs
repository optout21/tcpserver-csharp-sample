using System;

namespace tcpserver_csharp_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TCP CSharp Client");

            ClientApp app = new ClientApp();
            app.Start(new AppParams(5000, 5));

            Console.WriteLine("Done");
        }
    }
}
