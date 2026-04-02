using System;
using System.Collections.Generic;
using Fireball.Game.Server.Models;
using Newtonsoft.Json;

namespace Fireball.Game.Server.SessionModule
{
    public class GameSession
    {
        public string Id { get; set; }
        public string GameId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? AutoCompleteAt { get; set; }
        public string Environment { get; set; }
        public string GameState { get; set; }
        public string GameMode { get; set; }
        public List<Player> Players { get; set; }

        public Player GetPlayer(string operatorPlayerId)
        {
            return Players?.Find(p => p?.OperatorPlayerId == operatorPlayerId);
        }

        public Dictionary<string, object> ParseGameState()
        {
            Dictionary<string, object> result = null;
            try
            {
                if (GameState != null)
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, object>>(GameState);
                }
            }
            catch (Exception)
            {

            }
            return result;
        }

        public T ParseGameState<T>() where T : class
        {
            T result = null;
            try
            {
                if (GameState != null)
                {
                    result = JsonConvert.DeserializeObject<T>(GameState);
                }
            }
            catch (Exception)
            {

            }
            return result;
        }
    }
}
