using Grpc.Net.Client;
using ScoringEngine.Services;

namespace ScoringEngine.Client.Services
{
    public class ConnectionService
    {
        private static readonly Dictionary<string, GrpcChannel> Channels = new();

        private readonly ILogger<ConnectionService> _logger;
        private readonly ConfigurationService _configService;

        public ConnectionService(ConfigurationService configService, ILogger<ConnectionService> logger) =>
            (_configService, _logger) = (configService, logger);

        public async Task<Session.SessionClient?> GetSessionClient()
        {
            var config = await _configService.GetConfiguration();

            if (config.ServerHost is null)
            {
                return null;
            }

            return GetSessionClient(config.ServerHost);
        }

        public Session.SessionClient GetSessionClient(string address)
        {
            var channel = Channels.ContainsKey(address)
                ? Channels[address]!
                : Channels[address] = GrpcChannel.ForAddress($"http://{address}:5001")!;

            return new Session.SessionClient(channel);
        }

        public async Task<ScoringEngine.Services.Scoring.ScoringClient?> GetScoringClient()
        {
            var config = await _configService.GetConfiguration();

            if (config.ServerHost is null)
            {
                return null;
            }

            return GetScoringClient(config.ServerHost);
        }

        public ScoringEngine.Services.Scoring.ScoringClient GetScoringClient(string address)
        {
            var channel = Channels.ContainsKey(address)
                ? Channels[address]!
                : Channels[address] = GrpcChannel.ForAddress($"http://{address}:5001")!;

            return new ScoringEngine.Services.Scoring.ScoringClient(channel);
        }

        public async Task<bool> Ping(string address)
        {
            try
            {
                var client = GetSessionClient(address);

                if (client is null)
                {
                    _logger.LogError("Could not create client");
                    return false;
                }

                await client.PingAsync(new PingRequest());

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not attempt to ping");
                return false;
            }
        }
    }
}
