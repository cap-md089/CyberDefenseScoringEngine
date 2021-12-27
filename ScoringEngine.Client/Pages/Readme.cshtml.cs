using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoringEngine.Client.Services;
using ScoringEngine.Models;

namespace ScoringEngine.Client.Pages
{
    public class ReadmeModel : PageModel
    {
        private readonly ScoringService _scoringService;
        private readonly ConfigurationService _configurationService;

        public ReadmeModel(ScoringService scoringService, ConfigurationService configService) =>
            (_scoringService, _configurationService) = (scoringService, configService);

        public string? RequestError;
        public string? ReadmeHtml;

        public async Task<IActionResult> OnGetAsync()
        {
            var config = await _configurationService.GetConfiguration();

            if (!config.IsFullyConfigured)
            {
                return RedirectToPage("/Configure");
            }


            var currentSystem = await _scoringService.GetSystem((int)config.SystemIdentifier!);

            if (currentSystem == null)
            {
                RequestError = "Could not load README information";
                return Page();
            }

            var convertor = new MarkdownSharp.Markdown();
            ReadmeHtml = convertor.Transform(currentSystem.ReadmeText);

            return Page();
        }
    }
}
