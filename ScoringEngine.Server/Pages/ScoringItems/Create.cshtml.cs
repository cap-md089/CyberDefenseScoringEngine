#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScoringEngine.Models;
using ScoringEngine.Server.Data;

namespace ScoringEngine.Server.Pages.ScoringItems
{
    public class CreateModel : PageModel
    {
        private readonly ScoringEngine.Server.Data.ScoringEngineDbContext _context;

        public CreateModel(ScoringEngine.Server.Data.ScoringEngineDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet(int? SystemIdentifier)
        {
            if (SystemIdentifier is null)
            {
                return NotFound();
            }

            CompetitionSystem = await _context.CompetitionSystems.FirstOrDefaultAsync(s => s.ID == SystemIdentifier);

            if (CompetitionSystem is null)
            {
                return NotFound();
            }

            NewItemSystemIdentifier = SystemIdentifier;

            return Page();
        }

        [BindProperty]
        public CompetitionSystem CompetitionSystem { get; set; }

        [BindProperty]
        public ScoringItem ScoringItem { get; set; }

        [BindProperty]
        public int? NewItemSystemIdentifier { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || NewItemSystemIdentifier == null)
            {
                return Page();
            }

            ScoringItem.CompetitionSystemID = (int) NewItemSystemIdentifier;

            _context.ScoringItems.Add(ScoringItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("../Systems/Details", new { id = NewItemSystemIdentifier });
        }
    }
}
