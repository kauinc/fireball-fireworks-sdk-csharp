using System;
using System.Collections.Generic;
using Fireball.Fireworks.Models;

namespace Fireball.Fireworks.SessionModule
{
    public class GetSessionMessage : JsonMessage
    {
        public string Environment { get; set; }
        public string OperatorId { get; set; }
        public string GameId { get; set; }
        public string OperatorPlayerId { get; set; }
        public string OperatorPlayerSession { get; set; }
        public string Type { get; set; }
        public string GameState { get; set; }
    }

    public class CreateSessionsMessage : JsonMessage
    {
        public string GameId { get; set; }
        public string GameMode { get; set; }
        public string Environment { get; set; }
        public string GameState { get; set; }
        public List<Player> Players { get; set; }
        public string ReplayId { get; set; }
        public Lock Lock { get; set; }
        public AutoComplete AutoComplete { get; set; }
    }

    public class FindSessionsMessage : JsonMessage
    {
        public string GameId { get; set; }
        public string GameMode { get; set; }
        public string Environment { get; set; }
        public string OperatorId { get; set; }
        public string OperatorPlayerId { get; set; }
        public string OperatorSessionId { get; set; }
    }

    public class FindSessionsResult : JsonMessage
    {
        public List<GameSession> Sessions { get; set; }
    }

    public class GameSessionResult : JsonMessage
    {
        public GameSession Session { get; set; }
    }

    public class UpdateSessionStateMessage : JsonMessage
    {
        public string SessionId { get; set; }
        public string GameStateFieldPath { get; set; }
        public object GameStateFieldObject { get; set; }
        public string LockId { get; set; }
    }

    public class UpdateOperatorPlayerSessionMessage : JsonMessage
    {
        public string SessionId { get; set; }
        public string PlayerId { get; set; }
        public string OperatorSessionId { get; set; }
    }

    public class SaveSessionMessage : JsonMessage
    {
        public string SessionId { get; set; }
        public string GameState { get; set; }
        public string ReplayId { get; set; }
        public string LockId { get; set; }
    }

    public class SaveSessionResult : JsonMessage
    {
        public GameSession Session { get; set; }
    }

    public class EndSessionMessage : JsonMessage
    {
        public string SessionId { get; set; }
    }

    //public class Replay
    //{
    //    public string OperatorBetId { get; set; }

    //    public Replay(string operatorBetId)
    //    {
    //        OperatorBetId = operatorBetId;
    //    }
    //}

    public class Lock
    {
        public string Id { get; set; }
        public DateTime Timeout { get; set; }

        public Lock(string id, DateTime timeout)
        {
            Id = id;
            Timeout = timeout;
        }
    }

    public class AutoComplete
    {
        public DateTime Time { get; set; }
        public string Extras { get; set; }

        public AutoComplete(DateTime time, string extras = null)
        {
            Time = time;
            Extras = extras;
        }
    }
}
