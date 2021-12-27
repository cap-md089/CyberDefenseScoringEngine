using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ScoringEngine.Models
{
    public class CompetitionSystem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [DisplayName("README Text")] public string ReadmeText { get; set; }

        [DisplayName("System Name")] public string SystemIdentifier { get; set; }

        public ICollection<ScoringItem> ScoringItems { get; set; } = new List<ScoringItem>();

        public Services.CompetitionSystem ToMessage()
        {
            var system = new Services.CompetitionSystem()
            {
                Id = ID,
                ReadmeText = ReadmeText,
                SystemIdentifier = SystemIdentifier,
            };

            system.ScoringItems.AddRange(ScoringItems.Select(item => item.ToMessage()));

            return system;
        }

        public static CompetitionSystem FromMessage(Services.CompetitionSystem sys)
        {
            var system = new CompetitionSystem()
            {
                ID = sys.Id,
                SystemIdentifier = sys.SystemIdentifier,
                ReadmeText = sys.ReadmeText
            };

            system.ScoringItems = sys.ScoringItems
                .Select(item => ScoringItem.FromMessage(system, item))
                .ToList();

            return system;
        }
}
}
