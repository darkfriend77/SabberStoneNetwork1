using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Networker.Common;
using Networker.Common.Abstractions;
using Newtonsoft.Json;
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
            _logger.LogDebug($"Message[Id:{packet.Id} SendData:{sendData.MessageType}]");

        }
    }
}
