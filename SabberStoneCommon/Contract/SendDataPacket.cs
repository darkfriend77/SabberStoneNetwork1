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
        public virtual string Token { get; set; }

        [ProtoMember(3)]
        public virtual string SendData { get; set; }
    }

    public enum MessageType
    {
        None,
        HandShake,
        Stats,
        Game,
        Response
    }

    public enum ResponseType
    {
        None,
        HandShake,
        Stats,
        Game
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

    public class HandShakeResponse
    {
        public virtual int Id { get; set; }
        public virtual string Token { get; set; }
    }

    public class Stats { }
    public class StatsResponse
    {
        public List<UserInfo> UserInfos { get; set; }
    }
    public class UserInfo
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public UserState UserState { get; set; }
    }
    public enum UserState
    {
        None,
        Queued,
        Playing
    }

    public class Game
    {
        public GameType GameType { get; set; }
    }

    public class GameResponse
    {
        public int QueueSize { get; set; }
    }
    public enum GameType
    {
        Normal
    }

    public class Response
    {
        public RequestState RequestState { get; set; }

        public ResponseType ResponseType { get; set; }

        public virtual string ResponseData { get; set; }
    }

}
