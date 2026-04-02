using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fireball.Fireworks.Models;
using Newtonsoft.Json;

namespace Fireball.Fireworks.JackpotsModule
{
    public class JackpotBaseMessage : JsonMessage
    {
        public string Environment { get; set; }
        public string OperatorId { get; set; }
        public string GameId { get; set; }
        public string PlayerId { get; set; }
        public string GameSession { get; set; }
        public string OperatorPlayerSession { get; set; }

        public string PlayerCurrency { get; set; }
        public long ContributionAmount { get; set; }
        public string TemplateId { get; set; }
    }



    public class JackpotContributeMessage : JackpotBaseMessage
    {

    }
    public class JackpotContributeResult : JsonMessage
    {
        public string TemplateId { get; set; }
        public string PlayerCurrency { get; set; }
        public long ContributionAmount { get; set; }
        public long NewJackpotAmount { get; set; }
    }



    public class JackpotOutcomeMessage : JackpotBaseMessage
    {
        public bool ForceWin { get; set; }
    }
    public class JackpotOutcomeResult : JsonMessage
    {
        public string TemplateId { get; set; }
        public string JackpotId { get; set; }
        public string PlayerCurrency { get; set; }
        public bool Won { get; set; }
        public long AmountWon { get; set; }
        public long NewJackpotAmount { get; set; }
        public long Savings { get; set; }
    }



    // Used for Pay Jackpot Win
    public class JackpotEntry
    {
        public string TemplateId { get; set; }
        public string JackpotId { get; set; }
        public long Savings { get; set; }
        public long Amount { get; set; }

        public JackpotEntry() { }

        public JackpotEntry(JackpotOutcomeResult outcomeResult)
        {
            TemplateId = outcomeResult.TemplateId;
            JackpotId = outcomeResult.JackpotId;
            Savings = outcomeResult.Savings;
            Amount = outcomeResult.AmountWon;
        }
    }

    public class JackpotContribution
    {
        public string Id { get; set; }
        public long Contribution { get; set; }

        public JackpotContribution() { }

        public JackpotContribution(string templateId, long contribution)
        {
            Id = templateId;
            Contribution = contribution;
        }
    }

    public class JackpotDetail
    {
        public string TemplateId { get; set; }
        public long Value { get; set; }
        public bool OperatorControlled { get; set; }
    }
    public class JackpotsDetailsMessage : JsonMessage
    {
        public List<string> JackpotTemplateIds { get; set; }
        public string PlayerCurrency { get; set; }
        public string OperatorId { get; set; }
        public string Environment { get; set; }
    }
    public class JackpotDetailsResult : JsonMessage
    {
        public List<JackpotDetail> Jackpots { get; set; }
    }



    public class JackpotDetailsRequest : BaseMessage
    {
        [Required]
        public List<string> JackpotTemplateIds { get; set; }
    }
    public class JackpotDetailsResponse : BaseMessage
    {
        public List<JackpotDetail> Jackpots { get; set; }

        public JackpotDetailsResponse(List<JackpotDetail> jackpots, BaseMessage incomeMessage)
        {
            Name = FireballConstants.MessagesNames.JACKPOT_DETAILS_RESULT;
            Jackpots = jackpots;

            CopyBaseParams(incomeMessage);
        }
    }
}
