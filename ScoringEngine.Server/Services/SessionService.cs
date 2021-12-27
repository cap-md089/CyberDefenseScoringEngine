using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ScoringEngine.Models;
using ScoringEngine.Server.Data;
using ScoringEngine.Services;

namespace ScoringEngine.Server.Services
{
    public class SessionService : Session.SessionBase
    {
        private readonly ILogger<ScoringService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SessionService(ILogger<ScoringService> logger, IServiceProvider serviceProvider) =>
            (_logger, _serviceProvider) = (logger, serviceProvider);

        public override async Task SubscribeToCommands(RegisteredVM request, IServerStreamWriter<ClientCommands> responseStream, ServerCallContext serverCallContext)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ScoringEngineDbContext>()!;

                var id = Guid.Parse(request.Id);

                var registration = await context.RegisteredVirtualMachines
                    .FirstOrDefaultAsync(v => v.RegisteredVirtualMachineID == id);

                if (registration == null)
                {
                    await responseStream.WriteAsync(new ClientCommands()
                    {
                        Close = new CloseConnection()
                    });
                    return;
                }
            }

            while (true)
            {
                await SetOnlineStatus(request, true);

                await Task.Delay(5000);

                try
                {
                    await responseStream.WriteAsync(new ClientCommands()
                    {
                        Ping = new PingRequest()
                    });
                }
                catch
                {
                    await SetOnlineStatus(request, false);
                    return;
                }
            }
        }

        public override Task<Pong> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Pong());
        }

        public override async Task<VMRegistration> RegisterVM(RegisterVMRequest request, ServerCallContext serverCallContext)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<ScoringEngineDbContext>()!;

            var teamExists = await context.Teams.AnyAsync(team => team.ID == request.TeamId);
            var systemExists = await context.CompetitionSystems.AnyAsync(sys => sys.ID == request.SystemIdentifier);

            if (!teamExists || !systemExists)
            {
                return new VMRegistration()
                {
                    Error = new CommsError()
                    {
                        Message = "System or team does not exist"
                    }
                };
            }

            var newRegisteredVm = new RegisteredVirtualMachine()
            {
                SystemIdentifier = request.SystemIdentifier,
                IsConnectedNow = true,
                LastCheckIn = DateTime.UtcNow,
                RegisteredVirtualMachineID = Guid.Parse(request.VmId),
                TeamID = request.TeamId
            };

            await context.RegisteredVirtualMachines.AddAsync(newRegisteredVm);
            await context.SaveChangesAsync();

            return new VMRegistration()
            {
                Registration = new RegisteredVM()
                {
                    SystemIdentifier = newRegisteredVm.SystemIdentifier,
                    History = {  },
                    Id = request.VmId,
                    IsConnectedNow = true,
                    LastCheckIn = Timestamp.FromDateTime(DateTime.SpecifyKind(newRegisteredVm.LastCheckIn, DateTimeKind.Utc)),
                    TeamId = newRegisteredVm.TeamID
                }
            };
        }

        private async Task SetOnlineStatus(RegisteredVM vm, bool onlineStatus)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<ScoringEngineDbContext>()!;

            var id = Guid.Parse(vm.Id);

            var registration = await context.RegisteredVirtualMachines
                .FirstOrDefaultAsync(v => v.RegisteredVirtualMachineID == id);

            if (registration is null)
            {
                throw new NullReferenceException("Registration is null");
            }

            registration.IsConnectedNow = onlineStatus;
            if (onlineStatus)
            {
                registration.LastCheckIn = DateTime.UtcNow;
            }

            context.Attach(registration).State = EntityState.Modified;

            await context.SaveChangesAsync();
        }
    }
}
