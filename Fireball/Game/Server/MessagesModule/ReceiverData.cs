using System.Collections.Generic;

namespace Fireball.Game.Server.MessagesModule
{
    public class ReceiverTypes
    {
        public const string Connection = "connection";
        public const string Game = "game";
        public const string Operator = "operator";
        public const string GameSession = "gamesession";
        public const string Player = "player";
        public const string Jackpot = "jackpot";
    }

    public class ReceiverData
    {
        public string Type { get; set; }
        public List<string> Ids { get; set; }
        public List<ReceiverData> Filters { get; set; }

        public ReceiverData() { }

        public ReceiverData(string type, string id)
        {
            Type = type;
            Ids = new List<string>() { id };
        }

        public ReceiverData(string type, List<string> ids)
        {
            Type = type;
            Ids = ids;
        }
    }
}
