using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace SabberStoneCommon.Contract
{
    [ProtoContract]
    public class SendDataPacket
    {
        [ProtoMember(1)]
        public virtual int Id { get; set; }

        [ProtoMember(2)]
        public virtual string SendData { get; set; }
    }

    public enum MessageType
    {
        None,
        HandShake,
        Response
    }

    public enum ResponseType
    {
        None,
        HandShake
    }

    public enum RequestState
    {
        None,
        Fail,
        Success
    }

    public class SendData
    {
        public virtual MessageType MessageType{ get; set; }

        public virtual string MessageData { get; set; }
    }

    public class HandShake
    {
        public virtual string AccountName { get; set; }
    }

    public class Response
    {
        public RequestState RequestState { get; set; }

        public ResponseType ResponseType { get; set; }

        public virtual string ResponseData { get; set; }
    }

}
