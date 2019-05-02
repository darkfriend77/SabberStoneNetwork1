using System;
using System.Threading;
using SabberStoneClient.Client;
using SabberStoneServer.Server;

namespace SabberStoneNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread server = new Thread(new ThreadStart(ServerStart));
            server.Start();

            GameClient.Instance.Client.Connect();

            Thread.Sleep(5000);

            GameClient.Instance.RequestHandShake("Player1");

            Thread.Sleep(5000);

            GameClient.Instance.RequestGame();

            Thread.Sleep(5000);

            GameClient.Instance.RequestStats();

            Console.ReadKey();
        }

        private static void ServerStart()
        {
            GameServer.Instance.Start();
        }
    }
}
