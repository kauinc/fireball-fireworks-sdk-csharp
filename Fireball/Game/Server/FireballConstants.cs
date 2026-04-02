namespace Fireball.Game.Server
{
    public static class FireballConstants
    {
        public const string URL_FIREBALL_SERVER = "https://cloud.fireballserver.com";
        public const string URL_FIREBALL_SERVER_API = "https://api.fireballserver.com/api/v2.0";

        public static class Environment
        {
            public const string ENV = "ENV";
            public const string DEVELOPMENT = "development";
            public const string STAGING = "staging";
            public const string PRODUCTION = "production";
        }

        public static class GameMode
        {
            public const string FUN = "fun";
            public const string COINS = "coins";
            public const string MONEY = "money";
        }

        public static class BetType
        {
            public const string SPIN = "SPIN";
            public const string FREESPIN = "FREESPIN";
            public const string BONUSGAME = "BONUSGAME";
            public const string JACKPOT = "JACKPOT";
            public const string BUYGAME = "BUYGAME";
        }

        public static class WinningType
        {
            public const string SPIN = "SPIN";
            public const string FREESPIN = "FREESPIN";
            public const string BONUSGAME = "BONUSGAME";
            public const string JACKPOT = "JACKPOT";
            public const string PAYGAME = "PAYGAME";
            public const string REFUND = "REFUND";
        }

        public class MessagesNames
        {
            public const string PING = "ping";
            public const string AUTHENTICATE = "authenticate";
            public const string AUTHENTICATE_REJECT = "authenticate-reject";
            public const string SESSION = "session";
            public const string DISCONNECTED = "player-disconnected";
            public const string ERROR = "error";
            public const string WARNING = "warning";

            public const string BALANCE_REQUEST = "balance";
            public const string BALANCE_UPDATED = "balance-updated";

            public const string BET_PLACE = "bet-place";
            public const string BET_PLACED = "bet-placed";
            public const string BET_PLACE_REJECTED = "bet-place-rejected";

            public const string WINNING_PAY = "winning-pay";
            public const string WINNING_PAID = "winning-paid";
            public const string WINNING_PAY_REJECTED = "winning-pay-rejected";

            public const string JACKPOT_DETAILS = "jackpot-details";
            public const string JACKPOT_DETAILS_RESULT = "jackpot-details-result";
            public const string JACKPOT_PAY = "jackpot-pay";
            public const string JACKPOT_PAID = "jackpot-paid";
            public const string JACKPOT_PAY_REJECTED = "jackpot-pay-rejected";

            public const string MULTIPLAYER_MATCH_START = "match-start";

            public const string RTP_START = "rtp-test-start";
            public const string RTP_RESULT = "rtp-test-result";
        }
    }
}
