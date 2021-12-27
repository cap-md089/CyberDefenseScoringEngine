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

namespace ScoringEngine.Server.Pages.ScoringItems
{
    public class DetailsModel : PageModel
    {
        private readonly ScoringEngine.Server.Data.ScoringEngineDbContext _context;

        public DetailsModel(ScoringEngine.Server.Data.ScoringEngineDbContext context)
        {
            _context = context;
        }

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
    }
}
