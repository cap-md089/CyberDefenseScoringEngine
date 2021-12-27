using System.Net.NetworkInformation;
using ScoringEngine.Models;
using ScoringEngine.Services;
using CompetitionSystem = ScoringEngine.Models.CompetitionSystem;
using ScoringItem = ScoringEngine.Models.ScoringItem;

namespace ScoringEngine.Client.Services
{
    public class ScoringService
    {
        private readonly ILogger<ScoringService> _logger;
        private readonly ConnectionService _connectionService;
        private readonly ConfigurationService _configService;

        public ScoringService(ILogger<ScoringService> logger, ConnectionService connectionService, ConfigurationService configService) =>
            (_logger, _connectionService, _configService) = (logger, connectionService, configService);

        public async Task<Team?> GetTeam(int id, CancellationToken cancellationToken = default)
        {
            var client = await _connectionService.GetScoringClient();

            if (client is null)
            {
                _logger.LogWarning("Unable to establish client connection");
                return null;
            }

            var response = await client.GetTeamAsync(new TeamRequest()
            {
                TeamId = id
            });

            return response is null ? null : Team.FromMessage(response);
        }

        public async Task<IEnumerable<Team>> GetTeams(CancellationToken cancellationToken = default)
        {
            var client = await _connectionService.GetScoringClient();

            if (client is null)
            {
                _logger.LogWarning("Unable to establish client connection");
                return new List<Team>();
            }

            var response = await client.GetTeamsAsync(new TeamsRequest());

            return response.Teams.Select(Team.FromMessage);
        }

        public async Task<CompetitionSystem?> GetSystem(int systemId, CancellationToken cancellationToken = default)
        {
            var client = await _connectionService.GetScoringClient();

            if (client is null)
            {
                _logger.LogWarning("Unable to establish client connection");
                return null;
            }

            var response = await client.GetSystemAsync(new SystemRequest
            {
                SystemIdentifier = systemId
            });

            return response?.System is null ? null : CompetitionSystem.FromMessage(response.System);
        }

        public async Task<IEnumerable<CompetitionSystem>> GetSystems(CancellationToken cancellationToken = default)
        {
            var client = await _connectionService.GetScoringClient();

            if (client is null)
            {
                _logger.LogWarning("Unable to establish client connection");
                return new List<CompetitionSystem>();
            }

            var response =
                await client.GetAvailableSystemsAsync(new SystemsRequest(), cancellationToken: cancellationToken);

            return response?.Systems is null
                ? new List<CompetitionSystem>() 
                : response.Systems.Select(CompetitionSystem.FromMessage);
        }

        public async Task UpdateScores(ScoringSession session, IEnumerable<ScoringItem> itemsToAdd, IEnumerable<ScoringItem> itemsToRemove,
            CancellationToken cancellationToken = default)
        {
            var client = await _connectionService.GetScoringClient();

            if (client is null)
            {
                _logger.LogWarning("Unable to establish client connection");
                return;
            }

            var request = new UpdateScoringItemRequest
            {
                Session = session.ToMessage(),
            };

            request.AddedItemIds.AddRange(itemsToAdd.Select(item => item.ID));
            request.RemovedItemIds.AddRange(itemsToRemove.Select(item => item.ID));

            await client.SetScoringItemCompletionStatusAsync(request, cancellationToken: cancellationToken);
        }
    }

    public class ScoringSession
    {
        public Guid Id { get; set; }

        public SessionInfo ToMessage() => new()
        {
            VmId = Id.ToString()
        };

        public static ScoringSession FromMessage(SessionInfo session) => new()
        {
            Id = Guid.Parse(session.VmId)
        };
    }

    public static class SessionExtension
    {
        public static ScoringSession GetSession(this RegisteredVirtualMachine vm) => new()
        {
            Id = vm.RegisteredVirtualMachineID
        };
    }
}
