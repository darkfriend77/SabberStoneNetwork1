using System;
using SabberStoneServer.Server;

namespace SabberStoneServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer.Instance.Start();
        }
    }
}
