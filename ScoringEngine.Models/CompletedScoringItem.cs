using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Transactions;
using Google.Protobuf.WellKnownTypes;
using ScoringEngine.Common.Utilities;

namespace ScoringEngine.Models
{
    public class CompletedScoringItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int ScoringItemId { get; set; }

        public Guid RegisteredVirtualMachineID { get; set; }

        [JsonIgnore]
        public ScoringItem ScoringItem { get; set; }
        [JsonIgnore]
        public RegisteredVirtualMachine AppliedVirtualMachine { get; set; }

        public CompletionStatus CompletionStatus { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime ApplicationTime { get; set; }

        public static IEnumerable<ScoringItem> CalculateCurrentStatus(IEnumerable<CompletedScoringItem> scoringItems)
        {
            var result = new HashSet<ScoringItem>();

            foreach(var item in scoringItems)
            {
                if (item.ScoringItem is null) continue;

                if (item.CompletionStatus == CompletionStatus.Added)
                {
                    result.Add(item.ScoringItem);
                }
                else
                {
                    result.Remove(item.ScoringItem);
                }
            }

            return result;
        }

        public static IEnumerable<ScoringItem> CalculateStatusAtTime(IEnumerable<CompletedScoringItem> scoringItems,
            DateTime time)
        {
            var usedItems = from item
                in scoringItems
                where item.ApplicationTime < time
                select item;

            return CalculateCurrentStatus(usedItems);
        }

        public Services.CompletedScoringItem ToMessage() => new()
        {
            ScoringItem = Optional.HandleOptional<ScoringItem, Services.ScoringItem>(item => item.ToMessage())(ScoringItem),
            ApplicationTime = Timestamp.FromDateTime(DateTime.SpecifyKind(ApplicationTime, DateTimeKind.Utc)),
            ScoringItemId = ScoringItemId,
            Status = (Services.CompletedScoringItem.Types.ScoringItemStatus)CompletionStatus,
            Vm = null,
            VmId = RegisteredVirtualMachineID.ToString(),
        };

        public static CompletedScoringItem FromMessage(RegisteredVirtualMachine vm, Services.CompletedScoringItem message) => new()
        {
            ApplicationTime = message.ApplicationTime.ToDateTime(),
            RegisteredVirtualMachineID = Guid.Parse(message.VmId),
            CompletionStatus = (Models.CompletionStatus)message.Status,
            ScoringItemId = message.ScoringItemId,
            ScoringItem = message.ScoringItem is null ? null : ScoringItem.FromMessage(message.ScoringItem),
            AppliedVirtualMachine = vm,
        };
    }

    public enum CompletionStatus
    {
        Added = 0,
        Removed = 1
    }
}
