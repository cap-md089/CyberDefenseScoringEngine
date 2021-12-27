using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace ScoringEngine.Client.Services
{
    public class ConfigurationService
    {
        private string FilePath => OperatingSystem.IsLinux() ? "/opt/CDSE/config.json" : @"C:\CDSE\config.json";
            
        public async Task<ServiceConfiguration> GetConfiguration()
        {
            try
            {
                await using var fileStream = File.OpenRead(FilePath);
                return await JsonSerializer.DeserializeAsync<ServiceConfiguration>(fileStream) ?? new ServiceConfiguration();
            }
            catch
            {
                return new ServiceConfiguration();
            }
        }

        public async Task SaveConfiguration(ServiceConfiguration config)
        {
            await using var fileStream = File.Create(FilePath);
            await JsonSerializer.SerializeAsync(fileStream, config, new JsonSerializerOptions()
            {
                WriteIndented = true,
                IgnoreReadOnlyProperties = true
            });
        }
    }

    public class ServiceConfiguration
    {
        public int? TeamID { get; set; }

        public int? SystemIdentifier { get; set; }

        public string? ServerHost { get; set; }

        public Guid SystemGUID { get; set; } = Guid.NewGuid();

        public bool IsFullyConfigured =>
            TeamID is not null && SystemIdentifier is not null && ServerHost is not null;
    }
}
