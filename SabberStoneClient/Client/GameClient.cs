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

        private int _id;

        private string _token;

        public ClientState ClientState
        {
            get => _clientState;
            set
            {
                Log(LogLevel.Information, $"changed ClientState[{_clientState}->{value}]");
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

            _id = -1;
            _token = string.Empty;
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

        public void RequestStats()
        {
            if (ClientState == ClientState.None)
            {
                Log(LogLevel.Warning, "Can't request stats without connection!");
                return;
            }

            var sendDataPacket = new SendDataPacket()
            {
                Id = _id,
                Token = _token,
                SendData = JsonConvert.SerializeObject(
                    new SendData
                    {
                        MessageType = MessageType.Stats,
                        MessageData = JsonConvert.SerializeObject(
                            new Stats())
                    })
            };

            Client.Send(sendDataPacket);
        }

        public void RequestHandShake(string accountName)
        {
            if (ClientState != ClientState.Connected)
            {
                Log(LogLevel.Warning, "Wrong client state to request a handshake!");
                return;
            }

            var sendDataPacket = new SendDataPacket()
            {
                Id = _id,
                Token = _token,
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

        public void RequestGame()
        {
            if (ClientState != ClientState.Registred)
            {
                Log(LogLevel.Warning, "Wrong client state to request a game!");
                return;
            }

            var sendDataPacket = new SendDataPacket()
            {
                Id = _id,
                Token = _token,
                SendData = JsonConvert.SerializeObject(
                    new SendData
                    {
                        MessageType = MessageType.Game,
                        MessageData = JsonConvert.SerializeObject(
                            new Game
                            {
                                GameType = GameType.Normal
                            })
                    })
            };

            Client.Send(sendDataPacket);
        }


        internal void ProcessResponse(Response response)
        {
            if (response.RequestState != RequestState.Success)
            {
                Log(LogLevel.Warning, $"Request[{response.ResponseType}]: failed! ");
                return;
            }

            switch (response.ResponseType)
            {
                case ResponseType.HandShake:
                        ClientState = ClientState.Registred;
                        var handShakeResponse = JsonConvert.DeserializeObject<HandShakeResponse>(response.ResponseData);
                        _id = handShakeResponse.Id;
                        _token = handShakeResponse.Token;
                    break;

                case ResponseType.Stats:
                        var statsResponse = JsonConvert.DeserializeObject<StatsResponse>(response.ResponseData);
                        statsResponse.UserInfos.ForEach(p => Log(LogLevel.Information, $" -> {p.AccountName}[{p.Id}]: {p.UserState}"));
                    break;

                case ResponseType.Game:
                    var gameResponse = JsonConvert.DeserializeObject<GameResponse>(response.ResponseData);
                    ClientState = ClientState.Queued;
                    break;

                case ResponseType.None:
                    break;

                default:
                    break;
            }
        }

        public void Log(LogLevel logLevel, string message)
        {
            Console.WriteLine($"CLIENT[{logLevel}]: {message}");
        }
    }
}
