using System;
//using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Networker.Common.Abstractions;
using Networker.Extensions.ProtobufNet;
using Networker.Server;
using Networker.Server.Abstractions;
using Newtonsoft.Json;
using SabberStoneCommon.Contract;
using SabberStoneServer.Handler;
using SabberStoneCommon.Helper;

namespace SabberStoneServer.Server
{
    public class GameServer
    {
        public static GameServer Instance => Lazy.Value;

        private static readonly Lazy<GameServer> Lazy = new Lazy<GameServer>(() => new GameServer());

        private int index = 10000;
        public int Index => index++;

        private readonly IServer _server;

        //private HashAlgorithm hashAlgorithm;

        private GameServer()
        {
            //hashAlgorithm = new MD5CryptoServiceProvider();

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("serverSettings.json", false, true)
                .Build();

            IConfigurationSection networkerSettings = config.GetSection("Networker");

            // create logging service
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            _server = new ServerBuilder()
                .UseTcp(networkerSettings.GetValue<int>("TcpPort"))
                .UseUdp(networkerSettings.GetValue<int>("UdpPort"))
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.Services.Add(serviceCollection);
                })
                .UseProtobufNet()
                .RegisterPacketHandler<SendDataPacket, SendDataPacketHandler>()
                .Build();

        }

        public void Start()
        {
            _server.Start();

            while (_server.Information.IsRunning)
            {
                Thread.Sleep(10000);
            }
        }


        public SendDataPacket ResponseHandShake(SendData sendData, IPacketContext packetContext)
        {
            var handShake = JsonConvert.DeserializeObject<HandShake>(sendData.MessageData);
            var salted = (handShake.AccountName + Index).GetHashCode();
            var token = salted.ToString();

            Console.WriteLine($"Hash: {salted} -> {token}");

            var answerData = new SendData
            {
                MessageType = MessageType.Response,
                MessageData = JsonConvert.SerializeObject(
                    new Response()
                    {
                        RequestState = RequestState.Success,
                        ResponseType = ResponseType.HandShake,
                        ResponseData = token
                    })
            };

            return new SendDataPacket()
            {
                Id = 0,
                SendData = JsonConvert.SerializeObject(answerData)
            };
        }
    }
}
