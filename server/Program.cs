using System;

namespace tcpserver_csharp_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TCP CSharp Server");

            ServerApp app = new ServerApp();
            app.Start(new AppParams(5000, 5));

            Console.Write("Press Enter to exit ...");
            Console.ReadLine();
            Console.WriteLine();

            app.Stop();
            Console.WriteLine("Done");
        }
    }
}
