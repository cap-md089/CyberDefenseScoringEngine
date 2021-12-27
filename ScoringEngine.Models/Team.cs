using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ScoringEngine.Services;

namespace ScoringEngine.Models
{
    public class Team
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [EditableAttribute(true)]
        public string Name { get; set; }

        public int Points =>
        (
            from vm
                in RegisteredVirtualMachines
            select vm.Points
        ).Sum();

        public ICollection<RegisteredVirtualMachine> RegisteredVirtualMachines { get; set; }

        public static Team FromMessage(CompetitionTeam team)
        {
            return new Team
            {
                ID = team.Id,
                Name = team.Name,
                RegisteredVirtualMachines = team.Vms.Select(RegisteredVirtualMachine.FromMessage).ToList(),
            };
        }

        public CompetitionTeam ToMessage()
        {
            var msg = new CompetitionTeam()
            {
                Id = ID,
                Name = Name
            };

            if (RegisteredVirtualMachines != null)
            {
                msg.Vms.AddRange(RegisteredVirtualMachines.Select(vm => vm.ToMessage()));
            }

            return msg;
        }
    }
}
