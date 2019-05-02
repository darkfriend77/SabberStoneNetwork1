using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Networker.Common;
using Networker.Common.Abstractions;
using Newtonsoft.Json;
using SabberStoneClient.Client;
using SabberStoneCommon.Contract;

namespace SabberStoneClient.Handler
{
    public class ClientSendDataPacketHandler : PacketHandlerBase<SendDataPacket>
    {
        private readonly ILogger<ClientSendDataPacketHandler> _logger;

        public ClientSendDataPacketHandler(ILogger<ClientSendDataPacketHandler> logger)
        {
            _logger = logger;
        }

        public override async Task Process(SendDataPacket packet, IPacketContext packetContext)
        {
            SendData sendData = JsonConvert.DeserializeObject<SendData>(packet.SendData);

            switch (sendData.MessageType)
            {
                case MessageType.Response:
                    var response = JsonConvert.DeserializeObject<Response>(sendData.MessageData);
                    GameClient.Instance.Log(LogLevel.Information, $"Message[{sendData.MessageType}]: {response.ResponseType} => {response.RequestState}");
                    GameClient.Instance.ProcessResponse(response);
                    break;

                default:
                    GameClient.Instance.Log(LogLevel.Error, $"Not implemented {sendData.MessageType}!");
                    break;
            }
        }
    }
}
