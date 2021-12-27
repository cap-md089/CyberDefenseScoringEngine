using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ScoringEngine.Client.Services;
using ScoringEngine.Models;
using ScoringEngine.Services;

namespace ScoringEngine.Client.Pages
{
    public class RegisterVMModel : PageModel
    {
        private readonly ConfigurationService _configurationService;
        private readonly ConnectionService _connectionService;
        private readonly ScoringService _scoringService;

        public RegisterVMModel(ConfigurationService configurationService, ConnectionService connectionService, ScoringService scoringService) =>
            (_configurationService, _connectionService, _scoringService) = (configurationService, connectionService, scoringService);

        [BindProperty]
        public string? ServerHost { get; set; }
        [BindProperty]
        public int? SystemIdentifier { get; set; }
        [BindProperty]
        public int? TeamID { get; set; }

        public IEnumerable<SelectListItem> SystemsSelection { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TeamsSelection { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync()
        {
            var config = await _configurationService.GetConfiguration();

            ServerHost = config.ServerHost;
            SystemIdentifier = config.SystemIdentifier;
            TeamID = config.TeamID;

            if (ServerHost is not null && SystemIdentifier is not null && TeamID is not null)
            {
                return RedirectToPage("/Index");
            }

            if (ServerHost is not null)
            {
                var competitionSystems = await _scoringService.GetSystems();
                var teams = await _scoringService.GetTeams();

                SystemsSelection = competitionSystems.Select(sys => new SelectListItem()
                {
                    Text = sys.SystemIdentifier,
                    Value = sys.ID.ToString()
                });
                TeamsSelection = teams.Select(team => new SelectListItem()
                {
                    Text = team.Name,
                    Value = team.ID.ToString()
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync();
            }

            var config = await _configurationService.GetConfiguration();
            var newConfig = new ServiceConfiguration
            {
                SystemIdentifier = config.SystemIdentifier,
                ServerHost = config.ServerHost,
                SystemGUID = config.SystemGUID,
                TeamID = config.TeamID
            };

            if (config.ServerHost is null && ServerHost is not null)
            {
                if (!await _connectionService.Ping(ServerHost))
                {
                    ServerHost = null;
                    ModelState.AddModelError("ServerHost",
                        "Could not establish connection with remote server. Check log file in order to debug");
                    return Page();
                }
                newConfig.ServerHost = ServerHost;
            }

            if (config.SystemIdentifier is null && SystemIdentifier is not null)
            {
                newConfig.SystemIdentifier = SystemIdentifier;
            }

            if (config.TeamID is null && TeamID is not null)
            {
                newConfig.TeamID = TeamID;
            }

            if (newConfig.IsFullyConfigured)
            {
                var client = _connectionService.GetSessionClient(config.ServerHost!);

                await client.RegisterVMAsync(new RegisterVMRequest()
                {
                    SystemIdentifier = (int)newConfig.SystemIdentifier!,
                    TeamId = (int)newConfig.TeamID!,
                    VmId = newConfig.SystemGUID.ToString()
                });
            }

            await _configurationService.SaveConfiguration(newConfig);

            return RedirectToPage("./Index");
        }
    }
}
