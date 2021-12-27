using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ScoringEngine.Models
{
    public class ScoringItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("System Identifier")]
        public int CompetitionSystemID { get; set; }

        public int Points { get; set; }


        [DisplayName("Script Language")]
        public ScriptType ScriptType { get; set; }

        [DisplayName("Verification Script")]
        public string Script { get; set; }

        [JsonIgnore]
        public CompetitionSystem CompetitionSystem;

        [DisplayName("Task or Penalty")]
        public ScoringItemType ScoringItemType { get; set; }

        public static ScoringItem FromMessage(Services.ScoringItem message) => new ScoringItem
        {
            ID = message.Id,
            CompetitionSystemID = message.SystemIdentifier,
            Script = message.Script,
            ScoringItemType = (Models.ScoringItemType)message.ItemType,
            ScriptType = (Models.ScriptType)message.ScriptType,
            Points = message.Points,
            Name = message.Name
        };

        public static ScoringItem FromMessage(CompetitionSystem sys, Services.ScoringItem message) => new ScoringItem
        {
            ID = message.Id,
            CompetitionSystemID = message.SystemIdentifier,
            Script = message.Script,
            ScoringItemType = (Models.ScoringItemType)message.ItemType,
            ScriptType = (Models.ScriptType)message.ScriptType,
            Points = message.Points,
            Name = message.Name,
            CompetitionSystem = sys
        };

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object? obj)
        {
            return obj is ScoringItem item && item.ID == ID;
        }

        public Services.ScoringItem ToMessage() => new()
        {
            SystemIdentifier = CompetitionSystemID,
            Id = ID,
            Points = Points,
            ScriptType = (Services.ScoringItem.Types.ScriptType)ScriptType,
            Script = Script,
            ItemType = (Services.ScoringItem.Types.ItemType)ScoringItemType,
            Name = Name
        };
    }

    public enum ScoringItemType
    {
        Penalty = 0,
        Task = 1
    }

    public enum ScriptType
    {
        Python = 0,
        Lua = 1,
    }
}
