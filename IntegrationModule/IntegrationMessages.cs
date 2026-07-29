using System;
using System.Collections.Generic;
using Fireball.Fireworks.JackpotsModule;
using Fireball.Fireworks.Models;
using Fireball.Fireworks.Validation;
using Newtonsoft.Json;

namespace Fireball.Fireworks.IntegrationModule
{
    public class IntegrationAuthMessage : BaseMessage
    {
        public string Token { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationAuthMessage(AuthMessage message, Guid? transactionId = null)
        {
            CopyBaseParams(message);

            Name = FireballConstants.MessagesNames.AUTHENTICATE;
            Token = message.Token;
            ConnectionId = message.ConnectionId;
            TransactionId = transactionId ?? Guid.NewGuid();
            server_side = null;
            client_side = null;
        }
    }
    public class IntegrationSessionMessage : BaseMessage
    {
        public string Token { get; set; }
        public long Balance { get; set; }
        public long? Multiplier { get; set; }
        public List<FreeBetCampaign> FreeBetCampaigns { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationSessionMessage()
        {
            Name = FireballConstants.MessagesNames.SESSION;
        }
    }

    public class IntegrationBalanceRequest : BaseMessage
    {
        [RequiredSet(IsRequired = false)]
        public new string GameSession { get; set; }
        [RequiredSet(IsRequired = false)]
        public new string PlayerId { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationBalanceRequest() { }

        public IntegrationBalanceRequest(BaseMessage message, Guid? transactionId = null)
        {
            CopyBaseParams(message);

            Name = FireballConstants.MessagesNames.BALANCE_REQUEST;
            TransactionId = transactionId ?? Guid.NewGuid();
            server_side = null;
            client_side = null;
        }
    }
    public class IntegrationBalanceUpdated : BaseMessage
    {
        public long Balance { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationBalanceUpdated()
        {
            Name = FireballConstants.MessagesNames.BALANCE_UPDATED;
        }
    }

    public class IntegrationBetPlace : BaseMessage
    {
        public string BetType { get; set; }
        public long Amount { get; set; }
        public Guid? RoundId { get; set; }
        public Guid? BetId { get; set; }
        public ParentBet ParentBet { get; set; }
        public FreeBetDetails FreeBetDetails { get; set; }
        public List<JackpotContribution> JackpotContributions { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationBetPlace(string betType, long amount, BaseMessage message, ParentBet parentBet = null, FreeBetDetails freeBetDetails = null, List<JackpotContribution> jackpotContributions = null, Guid? roundId = null, Guid? betId = null, Guid? transactionId = null)
        {
            CopyBaseParams(message);

            Name = FireballConstants.MessagesNames.BET_PLACE;
            Amount = amount;
            BetType = betType;
            ReplayId = FireballTools.GenerateGUID();
            RoundId = roundId ?? Guid.NewGuid();
            BetId = betId ?? Guid.NewGuid();
            ParentBet = parentBet;
            FreeBetDetails = freeBetDetails;
            JackpotContributions = jackpotContributions;
            TransactionId = transactionId ?? Guid.NewGuid();
            server_side = null;
            client_side = null;
        }
    }
    public class IntegrationBetPlaced : BaseMessage
    {
        public string BetType { get; set; }
        public long Balance { get; set; }
        public long Amount { get; set; }
        public string OperatorBetId { get; set; }
        public Guid? RoundId { get; set; }
        public Guid? BetId { get; set; }
        public ParentBet ParentBet { get; set; }
        public FreeBetDetails FreeBetDetails { get; set; }
        public List<FreeBetCampaign> FreeBetCampaigns { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationBetPlaced()
        {
            Name = FireballConstants.MessagesNames.BET_PLACED;
        }
    }

    public class IntegrationWinningPay : BaseMessage
    {
        public string WinningType { get; set; } // "BONUSGAME", "SPIN", "JACKPOT"
        public string OperatorBetId { get; set; }
        public long Amount { get; set; }
        public bool NoResponse { get; set; }
        public Guid? WinId { get; set; }
        public Guid? BetId { get; set; }
        public Guid? RoundId { get; set; }
        public bool RoundClosed { get; set; }
        public ParentBet ParentBet { get; set; }
        public DisplayDelay DisplayDelay { get; set; }
        public FreeBetDetails FreeBetDetails { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationWinningPay(string winningType, string operatorBetId, long amount, BaseMessage message, bool noResponse = false, ParentBet parentBet = null, DisplayDelay displayDelay = null, FreeBetDetails freeBetDetails = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false, Guid? winId = null, Guid? transactionId = null)
        {
            CopyBaseParams(message);

            Name = FireballConstants.MessagesNames.WINNING_PAY;
            WinningType = winningType;
            OperatorBetId = operatorBetId;
            Amount = amount;
            NoResponse = noResponse;
            WinId = winId ?? Guid.NewGuid();
            BetId = betId;
            RoundId = roundId;
            RoundClosed = roundClosed;
            ParentBet = parentBet;
            DisplayDelay = displayDelay;
            FreeBetDetails = freeBetDetails;
            TransactionId = transactionId ?? Guid.NewGuid();
            server_side = null;
            client_side = null;
        }
    }
    public class IntegrationWinningPaid : BaseMessage
    {
        public string WinningType { get; set; }
        public string OperatorBetId { get; set; }
        public long Balance { get; set; }
        public long Amount { get; set; }
        public Guid? WinId { get; set; }
        public Guid? BetId { get; set; }
        public Guid? RoundId { get; set; }
        public bool RoundClosed { get; set; }
        public ParentBet ParentBet { get; set; }
        public FreeBetDetails FreeBetDetails { get; set; }
        public List<FreeBetCampaign> FreeBetCampaigns { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationWinningPaid()
        {
            Name = FireballConstants.MessagesNames.WINNING_PAID;
        }
    }


    public class IntegrationJackpotPay : BaseMessage
    {
        public List<JackpotEntry> Jackpots { get; set; }
        public string OperatorBetId { get; set; }
        public Guid? WinId { get; set; }
        public Guid? BetId { get; set; }
        public Guid? RoundId { get; set; }
        public DisplayDelay DisplayDelay { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationJackpotPay(List<JackpotEntry> jackpots, string operatorBetId, BaseMessage message, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, Guid? winId = null, Guid? transactionId = null)
        {
            CopyBaseParams(message);

            Jackpots = jackpots;
            OperatorBetId = operatorBetId;
            WinId = winId ?? Guid.NewGuid();
            BetId = betId;
            RoundId = roundId;
            DisplayDelay = displayDelay;
            TransactionId = transactionId ?? Guid.NewGuid();
            server_side = null;
            client_side = null;
        }
    }
    public class IntegrationJackpotPaid : BaseMessage
    {
        public long TotalAmount { get; set; }
        public long Balance { get; set; }
        public List<JackpotEntry> Jackpots { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationJackpotPaid()
        {
            Name = FireballConstants.MessagesNames.WINNING_PAID;
        }
    }


    public class IntegrationPayDisplay : BaseMessage
    {
        public string DisplayId { get; set; }
        public Guid? TransactionId { get; set; }

        public IntegrationPayDisplay(string displayId, BaseMessage message, Guid? transactionId = null)
        {
            CopyBaseParams(message);

            DisplayId = displayId;
            TransactionId = transactionId ?? Guid.NewGuid();
            server_side = null;
            client_side = null;
        }
    }


    public class IntegrationDisconnectMessage : BaseMessage
    {
        public Guid? TransactionId { get; set; }

        public IntegrationDisconnectMessage()
        {
            Name = FireballConstants.MessagesNames.DISCONNECTED;
        }
    }

    public class IntegrationEndSessionMessage : BaseMessage
    {
        public Guid? TransactionId { get; set; }

        public IntegrationEndSessionMessage() { }

        public IntegrationEndSessionMessage(BaseMessage message, Guid? transactionId = null)
        {
            Name = FireballConstants.MessagesNames.DISCONNECTED;
            CopyBaseParams(message);
            TransactionId = transactionId ?? Guid.NewGuid();
        }
    }

    public class DisplayDelay
    {
        public string DisplayId { get; set; }
        public int DisplayTimeout { get; set; }
        public int Delay { get; set; }

        public DisplayDelay(string displayId = null, int displayTimeout = 300, int delay = 0)
        {
            DisplayId = displayId;
            DisplayTimeout = displayTimeout;
            Delay = delay;
        }
    }

    public class FreeBetCampaign
    {
        public string Id { get; set; }
        public long BetAmount { get; set; }
        public int NumberOfBets { get; set; }
        public Dictionary<string, object> Settings { get; set; }
    }

    public class FreeBetDetails
    {
        public string FreeBetCampaignId { get; set; }
        public string FreeBetId { get; set; }
        public int NumberOfBets { get; set; }
        public bool IsFreeBetCampaignOver { get; set; }
    }


    public class ParentBet
    {
        public string ActionId { get; set; }
        public string OperatorBetId { get; set; }
        public ParentBetDetails Details { get; set; }

        public ParentBet() { }

        public ParentBet(string actionId, string operatorBetId, long sum, bool finalWin)
        {
            ActionId = actionId;
            OperatorBetId = operatorBetId;
            Details = new ParentBetDetails()
            {
                Sum = sum,
                IsFinalWin = finalWin
            };
        }
    }

    public class ParentBetDetails
    {
        public long Sum { get; set; }
        public bool IsFinalWin { get; set; }
    }
}
