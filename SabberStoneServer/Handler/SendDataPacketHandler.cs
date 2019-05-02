using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Networker.Common;
using Networker.Common.Abstractions;
using Newtonsoft.Json;
using SabberStoneCommon.Contract;
using SabberStoneServer.Server;

namespace SabberStoneServer.Handler
{
    public class SendDataPacketHandler : PacketHandlerBase<SendDataPacket>
    {
        private readonly ILogger<SendDataPacketHandler> _logger;

        public SendDataPacketHandler(ILogger<SendDataPacketHandler> logger)
        {
            _logger = logger;
        }

        public override async Task Process(SendDataPacket packet, IPacketContext packetContext)
        {
            SendData sendData = JsonConvert.DeserializeObject<SendData>(packet.SendData);
            _logger.LogDebug($"Id:{packet.Id} SendData:{packet.SendData}");

            switch (sendData.MessageType)
            {
                case MessageType.HandShake:
                    packetContext.Sender.Send(GameServer.Instance.ResponseHandShake(sendData, packetContext));
                    break;

                case MessageType.None:
                case MessageType.Response:
                default:
                    packetContext.Sender.Send(new SendDataPacket()
                    {
                        Id = 0,
                        SendData = JsonConvert.SerializeObject(DefaultAnswer)
                    });
                    break;
            }


        }



        //public override async Task Process(SendDataPacket packet, IPacketContext packetContext)
        //{
        //    SendData sendData = JsonConvert.DeserializeObject<SendData>(packet.SendData);
        //    _logger.LogDebug($"Message[Id:{packet.Id} SendData:{sendData.MessageType}]");

        //    var answer = DefaultAnswer;

        //    switch (sendData.MessageType)
        //    {
        //        case MessageType.HandShake:
        //        //packetContext.Sender.Send(GameServer.Instance.ResponseHandShake(sendData, packetContext));
        //        //break;

        //        case MessageType.None:
        //        case MessageType.Response:
        //        default:
        //            packetContext.Sender.Send(answer);
        //            break;
        //    }
        //}

        public SendData DefaultAnswer => new SendData
        {
            MessageType = MessageType.Response,
            MessageData = JsonConvert.SerializeObject(
                new Response
                {
                    RequestState = RequestState.None,
                    ResponseType = ResponseType.None,
                    ResponseData = "Not implemented yet!"
                })
        };

    }
}
