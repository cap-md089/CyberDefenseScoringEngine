using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ScoringEngine.Models;
using ScoringEngine.Server.Data;
using ScoringEngine.Services;
using CompletedScoringItem = ScoringEngine.Models.CompletedScoringItem;

namespace ScoringEngine.Server.Services
{
    public class ScoringService : Scoring.ScoringBase
    {
        private readonly ILogger<ScoringService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private ServerDbContext DbContext => new (_serviceProvider.CreateScope());

        public ScoringService(ILogger<ScoringService> logger, IServiceProvider serviceProvider) =>
            (_logger, _serviceProvider) = (logger, serviceProvider);

        public override async Task<SystemsQueryResponse> GetAvailableSystems(SystemsRequest request, ServerCallContext context)
        {
            var response = new SystemsQueryResponse();

            using var scope = DbContext;
            var db = scope.Context;

            var systems = await db.CompetitionSystems
                .Select(sys => sys.ToMessage())
                .ToListAsync();

            response.Systems.AddRange(systems);

            return response;
        }

        public override async Task<SystemQueryResponse> GetSystem(SystemRequest request, ServerCallContext context)
        {
            using var scope = DbContext;
            var db = scope.Context;

            return new SystemQueryResponse
            {
                System = (await db.CompetitionSystems
                    .Include(sys => sys.ScoringItems)
                    .FirstOrDefaultAsync(sys => sys.ID == request.SystemIdentifier))?
                    .ToMessage()
            };
        }

        public override async Task<TeamsRequestResponse> GetTeams(TeamsRequest request, ServerCallContext context)
        {
            var response = new TeamsRequestResponse();

            using var scope = DbContext;
            var db = scope.Context;

            var teams = await db.Teams
                .Select(team => team.ToMessage())
                .ToListAsync();

            response.Teams.AddRange(teams);

            return response;
        }

        public override async Task<CompetitionTeam> GetTeam(TeamRequest request, ServerCallContext context)
        {
            using var scope = DbContext;
            var db = scope.Context;

            var completedItems = await db.CompletedScoringItems
                .Include(comp => comp.ScoringItem)
                .Where(comp => comp.AppliedVirtualMachine.TeamID == request.TeamId)
                .ToListAsync();

            var team = await db.Teams
                .Include(teams => teams.RegisteredVirtualMachines)
                .ThenInclude(vms => vms.ScoringHistory)
                .Include(team => team.RegisteredVirtualMachines)
                .ThenInclude(vms => vms.CompetitionSystem)
                .FirstOrDefaultAsync(sys => sys.ID == request.TeamId);

            if (team is null)
            {
                return null!;
            }

            foreach (var vm in team.RegisteredVirtualMachines)
            {
                vm.ScoringHistory = completedItems.Where(item => item.RegisteredVirtualMachineID == vm.RegisteredVirtualMachineID).ToList();
            }

            return team.ToMessage();
        }

        public override async Task<ScoringItemUpdateResponse> SetScoringItemCompletionStatus(UpdateScoringItemRequest request, ServerCallContext context)
        {
            using var scope = DbContext;
            var db = scope.Context;

            try
            {
                var vm = await db.RegisteredVirtualMachines
                    .FirstOrDefaultAsync(vm => vm.RegisteredVirtualMachineID == Guid.Parse(request.Session.VmId));

                if (vm is null)
                {
                    return new ScoringItemUpdateResponse();
                }

                var newAddedItems = request.AddedItemIds.Select(id => new Models.CompletedScoringItem()
                {
                    ApplicationTime = DateTime.UtcNow,
                    CompletionStatus = CompletionStatus.Added,
                    RegisteredVirtualMachineID = Guid.Parse(request.Session.VmId),
                    ScoringItemId = id
                });
                var newRemovedItems = request.RemovedItemIds.Select(id => new Models.CompletedScoringItem()
                {
                    ApplicationTime = DateTime.UtcNow,
                    CompletionStatus = CompletionStatus.Removed,
                    RegisteredVirtualMachineID = Guid.Parse(request.Session.VmId),
                    ScoringItemId = id
                });

                var allItems = newAddedItems.Union(newRemovedItems);

                foreach (var item in allItems)
                {
                    db.CompletedScoringItems.Add(item);
                }

                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Funky request going on");
            }

            return new ScoringItemUpdateResponse();
        }
    }

    class ServerDbContext : IDisposable
    {
        public readonly IServiceScope Scope;
        public readonly ScoringEngineDbContext Context;

        public ServerDbContext(IServiceScope scope)
        {
            Scope = scope;
            Context = scope.ServiceProvider.GetService<ScoringEngineDbContext>()!;
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
