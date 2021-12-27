using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoringEngine.Client.Services;
using ScoringEngine.Models;

namespace ScoringEngine.Client.Pages
{
    public class ScoringReportModel : PageModel
    {
        private readonly ScoringService _scoringService;
        private readonly ConfigurationService _configurationService;

        public ScoringReportModel(ScoringService scoringService, ConfigurationService configService) =>
            (_scoringService, _configurationService) = (scoringService, configService);

        public string? RequestError;
        public RegisteredVirtualMachine? CurrentVm;
        public CompetitionSystem? CurrentSystem;

        public async Task<IActionResult> OnGet()
        {
            var config = await _configurationService.GetConfiguration();

            if (!config.IsFullyConfigured)
            {
                return RedirectToPage("/Configure");
            }

            var team = await _scoringService.GetTeam((int)config.TeamID!);

            if (team is null)
            {
                RequestError = "";
                return Page();
            }

            CurrentVm = team.RegisteredVirtualMachines.FirstOrDefault(vm =>
                vm.RegisteredVirtualMachineID == config.SystemGUID);

            CurrentSystem = await _scoringService.GetSystem((int)config.SystemIdentifier!);

            return Page();
        }

        public int PossiblePoints(CompetitionSystem sys) => sys.ScoringItems.Select(v => v.Points).Sum();
    }
}
