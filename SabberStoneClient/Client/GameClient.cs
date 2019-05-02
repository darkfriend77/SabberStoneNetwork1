using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Networker.Client;
using Networker.Client.Abstractions;
using Networker.Extensions.ProtobufNet;
using Newtonsoft.Json;
using SabberStoneClient.Handler;
using SabberStoneCommon.Contract;

namespace SabberStoneClient.Client
{
    public enum ClientState
    {
        None,
        Connected,
        HandShake,
        Registred,
        Queued,
        InGame
    }

    public class GameClient
    {
        public static GameClient Instance => Lazy.Value;

        private static readonly Lazy<GameClient> Lazy = new Lazy<GameClient>(() => new GameClient());

        private ClientState _clientState;

        public ClientState ClientState
        {
            get => _clientState;
            set
            {
                Console.WriteLine($"changed ClientState[{_clientState}->{value}]");
                _clientState = value;
            }
        }

        public IClient Client { get; set; }

        private GameClient()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("clientSettings.json", false, true)
                .Build();
            IConfigurationSection networkerSettings = config.GetSection("Networker");

            // create logging service
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            Client = new ClientBuilder()
                .UseIp(networkerSettings.GetValue<string>("Address"))
                .UseTcp(networkerSettings.GetValue<int>("TcpPort"))
                .UseUdp(networkerSettings.GetValue<int>("UdpPort"))
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.Services.Add(serviceCollection);
                })
                .UseProtobufNet()
                .RegisterPacketHandler<SendDataPacket, ClientSendDataPacketHandler>()
                .Build();

            ClientState = ClientState.None;

            Client.Connected += Connected;
            Client.Disconnected += Disconnected;
        }

        private void Connected(object sender, Socket e)
        {
            ClientState = ClientState.Connected;
        }

        private void Disconnected(object sender, Socket e)
        {
            ClientState = ClientState.None;
        }

        public void Connect()
        {
            Client.Connect();

        }

        public void RequestHandShake(string accountName)
        {
            if (ClientState == ClientState.None)
            {
                Console.WriteLine("Currently not connected to the server!");
                return;
            }

            var sendDataPacket = new SendDataPacket()
            {
                Id = 0,
                SendData = JsonConvert.SerializeObject(
                    new SendData
                    {
                        MessageType = MessageType.HandShake,
                        MessageData = JsonConvert.SerializeObject(
                            new HandShake
                            {
                                AccountName = accountName
                            })
                    })
            };

            ClientState = ClientState.HandShake;
            Client.Send(sendDataPacket);
        }
    }
}
