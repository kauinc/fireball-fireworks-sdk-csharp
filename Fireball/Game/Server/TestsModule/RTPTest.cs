using Fireball.Game.Server.Models;
using System.Collections.Generic;

namespace Fireball.Game.Server.TestsModule
{
    public class RTPTest : JsonMessage
    {
        public string Name { get; set; } // "rtp-test";
        public string TestId { get; set; } // UUID - "5055018a-0878-445d-af17-3029b6daff06"
        public Dictionary<string, object> GameState { get; set; }
    }

    public class RTPStart : JsonMessage
    {
        public string Name { get; set; } // "rtp-test-start";
        public string TestId { get; set; }
        public string CycleId { get; set; }
        public int AttemptNumber { get; set; }
        public long BetAmount { get; set; }
        public string Currency { get; set; }
        public Dictionary<string, object> GameState { get; set; }
        public Dictionary<string, object> CustomSettings { get; set; }
        public int Variant { get; set; }
    }

    public class RTPStart<T> : RTPStart where T : class
    {
        public new T GameState { get; set; }
    }

    public class RTPResult : JsonMessage
    {
        public string Name { get; set; } // "rtp-test-result";
        public string TestId { get; set; }
        public string CycleId { get; set; }
        public List<RTPWinResult> Results { get; set; }
        public Dictionary<string, object> GameState { get; set; }

        public RTPResult()
        {
            Name = FireballConstants.MessagesNames.RTP_RESULT;
        }
    }

    public class RTPResult<T> : RTPResult where T : class
    {
        public new T GameState { get; set; }
    }

    public class RTPWinResult
    {
        public string WinType { get; set; }
        public string WinSymbol { get; set; }
        public long WinAmount { get; set; }
        public long BetAmount { get; set; }

        public override string ToString()
        {
            return $"WinType = {WinType}, WinSymbol = {WinSymbol}, WinAmount = {WinAmount}, BetAmount = {BetAmount}";
        }
    }
}
