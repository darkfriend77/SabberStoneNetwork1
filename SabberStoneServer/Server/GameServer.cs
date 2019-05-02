using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    public class UserInfoData : UserInfo
    {
        public ISender Sender { get; set; }
    }

    public class GameServer
    {
        public static GameServer Instance => Lazy.Value;

        private static readonly Lazy<GameServer> Lazy = new Lazy<GameServer>(() => new GameServer());

        private int index = 10000;
        public int Index => index++;

        private readonly IServer _server;

        private ConcurrentDictionary<string, UserInfoData> registredUsers;

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
                .ConfigureLogging(loggingBuilder => { loggingBuilder.Services.Add(serviceCollection); })
                .UseProtobufNet()
                .RegisterPacketHandler<SendDataPacket, SendDataPacketHandler>()
                .Build();

            registredUsers = new ConcurrentDictionary<string, UserInfoData>();

        }

        public void Start()
        {
            _server.Start();

            while (_server.Information.IsRunning)
            {
                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendData"></param>
        /// <param name="packetContext"></param>
        /// <returns></returns>
        public SendDataPacket ResponseHandShake(SendData sendData, IPacketContext packetContext)
        {

            var handShake = JsonConvert.DeserializeObject<HandShake>(sendData.MessageData);
            var requestState = RequestState.Success;

            // create token here
            int userIndex = Index;
            var salted = (handShake.AccountName + userIndex).GetHashCode();
            var token = salted.ToString();
            //Console.WriteLine($"Hash: {salted} -> {token}");

            var user = registredUsers.Values.ToList().Find(p => p.AccountName == handShake.AccountName);
            if (user != null)
            {
                Log(LogLevel.Warning, $"Account {user.AccountName} already registred! EndPoint: {user.Sender.EndPoint.Address} {user.Sender.EndPoint.Equals(packetContext.Sender.EndPoint)}");
                requestState = RequestState.Fail;
            }
            else if (!registredUsers.TryAdd(token, new UserInfoData { Id = userIndex, AccountName = handShake.AccountName, Sender = packetContext.Sender, UserState = UserState.None }))
            {
                Log(LogLevel.Warning, $"Account {user.AccountName} couldn't be registred!");
                requestState = RequestState.Fail;
            }

            var responseData = new HandShakeResponse() { Id = userIndex, Token = token };

            var answerData = new SendData
            {
                MessageType = MessageType.Response,
                MessageData = JsonConvert.SerializeObject(
                    new Response()
                    {
                        RequestState = requestState,
                        ResponseType = ResponseType.HandShake,
                        ResponseData = requestState == RequestState.Success ? JsonConvert.SerializeObject(responseData) : ""
                    })
            };

            return CreatePacket(answerData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendData"></param>
        /// <param name="packetContext"></param>
        /// <returns></returns>
        public SendDataPacket ResponseStats(SendData sendData, IPacketContext packetContext)
        {
            var list = new List<UserInfo>();
            registredUsers.Values.ToList().ForEach(p =>
                list.Add(new UserInfo()
                {
                    Id = p.Id,
                    AccountName = p.AccountName,
                    UserState = p.UserState
                }));

            var answerData = new SendData
            {
                MessageType = MessageType.Response,
                MessageData = JsonConvert.SerializeObject(
                    new Response()
                    {
                        RequestState = RequestState.Success,
                        ResponseType = ResponseType.Stats,
                        ResponseData = JsonConvert.SerializeObject(new StatsResponse { UserInfos = list })
                    })
            };

            return CreatePacket(answerData);
        }

        public SendDataPacket ResponseGame(int id, string token, SendData sendData, IPacketContext packetContext)
        {
            var requestState = RequestState.Success;

            if (registredUsers.TryGetValue(token, out UserInfoData userInfoData) && userInfoData.UserState == UserState.None)
            {
                userInfoData.UserState = UserState.Queued;
            }
            else
            {
                requestState = RequestState.Fail;
            }

            var answerData = new SendData
            {
                MessageType = MessageType.Response,
                MessageData = JsonConvert.SerializeObject(
                    new Response()
                    {
                        RequestState = requestState,
                        ResponseType = ResponseType.Game,
                        ResponseData = JsonConvert.SerializeObject(new GameResponse() { QueueSize = 0 })
                    })
            };

            return CreatePacket(answerData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="answerData"></param>
        /// <returns></returns>
        private SendDataPacket CreatePacket(SendData answerData)
        {
            return new SendDataPacket()
            {
                Id = 0,
                Token = "",
                SendData = JsonConvert.SerializeObject(answerData)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        public void Log(LogLevel logLevel, string message)
        {
            Console.WriteLine($"SERVER[{logLevel}]: {message}");
        }
    }
}
