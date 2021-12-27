using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using ScoringEngine.Services;

namespace ScoringEngine.Models
{
    public class RegisteredVirtualMachine
    {
        public Guid RegisteredVirtualMachineID { get; set; }

        [JsonIgnore]
        public int TeamID { get; set; }
        public Team Team { get; set; }

        [JsonIgnore]
        public int SystemIdentifier { get; set; }
        public CompetitionSystem CompetitionSystem { get; set; }

        public DateTime LastCheckIn { get; set; }

        public bool IsConnectedNow { get; set; }

        [JsonIgnore]
        public ICollection<CompletedScoringItem> ScoringHistory { get; set; }

        [JsonIgnore]
        public int Points
        {
            get
            {
                var currentState = CompletedScoringItem.CalculateCurrentStatus(ScoringHistory);

                var items = currentState as ScoringItem[] ?? currentState.ToArray();

                var penaltyPoints = (
                    from penalty
                    in items
                    where penalty.ScoringItemType == ScoringItemType.Penalty
                    select penalty.Points
                ).Sum();
                var taskPoints = (
                    from task
                    in items
                    where task.ScoringItemType == ScoringItemType.Task
                    select task.Points
                ).Sum();

                return taskPoints - penaltyPoints;
            }
        }

        public RegisteredVM ToMessage()
        {
            Services.RegisteredVM message = new()
            {
                LastCheckIn = Timestamp.FromDateTime(DateTime.SpecifyKind(LastCheckIn, DateTimeKind.Utc)),
                SystemIdentifier = SystemIdentifier,
                Id = RegisteredVirtualMachineID.ToString(),
                IsConnectedNow = IsConnectedNow,
                TeamId = TeamID
            };
            
            message.History.AddRange(from item in ScoringHistory
                    select item.ToMessage());

            return message;
        }

        public static RegisteredVirtualMachine FromMessage(RegisteredVM message)
        {
            var vm = new RegisteredVirtualMachine
            {
                SystemIdentifier = message.SystemIdentifier,
                IsConnectedNow = message.IsConnectedNow,
                LastCheckIn = message.LastCheckIn.ToDateTime(),
                RegisteredVirtualMachineID = Guid.Parse(message.Id),
                TeamID = message.TeamId
            };

            vm.ScoringHistory = message.History.Select(item => CompletedScoringItem.FromMessage(vm, item)).ToList();

            return vm;
        }
    }
}
