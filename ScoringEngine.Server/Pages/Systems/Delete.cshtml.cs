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
    public class DeleteModel : PageModel
    {
        private readonly ScoringEngine.Server.Data.ScoringEngineDbContext _context;

        public DeleteModel(ScoringEngine.Server.Data.ScoringEngineDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CompetitionSystem CompetitionSystem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CompetitionSystem = await _context.CompetitionSystems.FirstOrDefaultAsync(m => m.ID == id);

            if (CompetitionSystem == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CompetitionSystem = await _context.CompetitionSystems.FindAsync(id);

            if (CompetitionSystem != null)
            {
                _context.CompetitionSystems.Remove(CompetitionSystem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
