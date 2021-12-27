#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ScoringEngine.Models;
using ScoringEngine.Server.Data;

namespace ScoringEngine.Server.Pages.Systems
{
    public class DetailsModel : PageModel
    {
        private readonly ScoringEngineDbContext _context;

        public DetailsModel(ScoringEngineDbContext context)
        {
            _context = context;
        }

        public CompetitionSystem CompetitionSystem { get; set; }

        public string ReadmeHtml { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CompetitionSystem = await _context.CompetitionSystems
                .Include(sys => sys.ScoringItems)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (CompetitionSystem == null)
            {
                return NotFound();
            }

            var markdownConverter = new MarkdownSharp.Markdown();
            ReadmeHtml = markdownConverter.Transform(CompetitionSystem.ReadmeText);

            return Page();
        }
    }
}
