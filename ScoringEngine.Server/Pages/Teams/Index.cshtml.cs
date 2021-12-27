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

namespace ScoringEngine.Server.Pages.Teams
{
    public class IndexModel : PageModel
    {
        private readonly ScoringEngineDbContext _context;

        public IndexModel(ScoringEngineDbContext context)
        {
            _context = context;
        }

        public IList<Team> Team { get;set; }

        public async Task OnGetAsync()
        {
            Team = await _context.Teams
                .Include(t => t.RegisteredVirtualMachines)
                .ThenInclude(vm => vm.ScoringHistory)
                .ThenInclude(hist => hist.ScoringItem)
                .ToListAsync();
        }
    }
}
