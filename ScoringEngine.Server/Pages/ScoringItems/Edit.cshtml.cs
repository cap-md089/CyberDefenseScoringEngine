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
    public class EditModel : PageModel
    {
        private readonly ScoringEngine.Server.Data.ScoringEngineDbContext _context;

        public EditModel(ScoringEngine.Server.Data.ScoringEngineDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ScoringItem ScoringItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ScoringItem = await _context.ScoringItems.FirstOrDefaultAsync(m => m.ID == id);

            if (ScoringItem == null)
            {
                return NotFound();
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ScoringItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScoringItemExists(ScoringItem.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("../Systems/Details", new { id = ScoringItem.CompetitionSystemID });
        }

        private bool ScoringItemExists(int id)
        {
            return _context.ScoringItems.Any(e => e.ID == id);
        }
    }
}
