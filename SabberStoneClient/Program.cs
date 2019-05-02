using System;
using System.Threading;
using SabberStoneClient.Client;
using SabberStoneCommon.Contract;

namespace SabberStoneClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connecting");
            GameClient.Instance.Client.Connect();

            Thread.Sleep(5000);

            GameClient.Instance.RequestHandShake("Player1");

            Thread.Sleep(5000);

            GameClient.Instance.RequestStats();

            Thread.Sleep(5000);

            Console.ReadKey();

        }
    }
}
