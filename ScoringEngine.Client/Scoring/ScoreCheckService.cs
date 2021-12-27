using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using IronPython.Hosting;
using MoonSharp.Interpreter;

using ScoringEngine.Client.Scoring.ScriptUtilities;
using ScoringEngine.Client.Services;
using ScoringEngine.Models;
using ScoringEngine.Services;
using CompletedScoringItem = ScoringEngine.Models.CompletedScoringItem;
using ScoringItem = ScoringEngine.Models.ScoringItem;

namespace ScoringEngine.Client.Scoring
{
    public class ScoreCheckService : BackgroundService
    {
        private readonly ConfigurationService _config;
        private readonly ILogger<ScoreCheckService> _logger;
        private readonly IScriptBackend _scriptBackend;
        private readonly ScoringService _scoreCheck;

        public ScoreCheckService(ConfigurationService config, ILogger<ScoreCheckService> logger, IScriptBackend scriptBackend, ScoringService scoreCheck) =>
            (_config, _logger, _scriptBackend, _scoreCheck) = (config, logger, scriptBackend, scoreCheck);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            UserData.RegisterType(_scriptBackend.GetType());
            UserData.RegisterType<LuaPenalty>();
            UserData.RegisterType<LuaTask>();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(10000, cancellationToken);

                await VerifySystem(cancellationToken);
            }
        }

        public async Task VerifySystem(CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _config.GetConfiguration();
                var teamInfo = config.TeamID is not null 
                    ? await _scoreCheck.GetTeam((int)config.TeamID, cancellationToken) 
                    : null;
                var system = config.SystemIdentifier is not null 
                    ? await _scoreCheck.GetSystem((int)config.SystemIdentifier, cancellationToken) 
                    : null;

                if (system is null || teamInfo is null)
                {
                    _logger.LogWarning("System is not yet set up");
                    return;
                }

                HashSet<ScoringItem> completedTasks = new();
                HashSet<ScoringItem> appliedPenalties = new();

                await Task.WhenAll(
                    Task.Run(async () =>
                    {
                        foreach (var task in system.ScoringItems.Where(item => item.ScoringItemType == ScoringItemType.Task))
                        {
                            if (await IsTaskCompleted(task, cancellationToken))
                            {
                                completedTasks.Add(task);
                            }
                        }
                    }, cancellationToken),

                    Task.Run(async () =>
                    {
                        foreach (var penalty in system.ScoringItems.Where(item => item.ScoringItemType == ScoringItemType.Penalty))
                        {
                            if (await DoesPenaltyApply(penalty, cancellationToken))
                            {
                                appliedPenalties.Add(penalty);
                            }
                        }
                    }, cancellationToken)
                );

                var vm = teamInfo.RegisteredVirtualMachines
                    .FirstOrDefault(m => m.RegisteredVirtualMachineID == config.SystemGUID);

                if (vm is null)
                {
                    _logger.LogWarning("Unable to find VM registration");
                    return;
                }

                var currentState = completedTasks
                    .Union(appliedPenalties)
                    .ToHashSet();
                var serverState = CompletedScoringItem.CalculateCurrentStatus(vm.ScoringHistory)!
                    .ToHashSet();

                var itemsToRemove = serverState.Except(currentState);
                var itemsToAdd = currentState.Except(serverState);

                await _scoreCheck.UpdateScores(vm.GetSession(), itemsToAdd, itemsToRemove, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not verify system");
            }
        }

        private Task<bool> DoesPenaltyApply(ScoringItem penalty, CancellationToken cancellationToken = default)
        {
            bool DoesLuaPenaltyApply(string scriptCode)
            {
                var script = new Script();
                var Penalty = new LuaPenalty();

                script.Globals.Set("Penalty", UserData.Create(Penalty));
                script.Globals.Set("Env", UserData.Create(_scriptBackend));

                try
                {
                    DynValue result = script.DoString(scriptCode);
                }
                catch (FinishExecutionException)
                {
                }

                return Penalty.DoesApply;
            }

            bool DoesPythonPenaltyApply(string script)
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(script);
                var penalty = new PythonPenalty();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    engine.Runtime.LoadAssembly(assembly);
                }

                scope.SetVariable("Penalty", penalty);
                scope.SetVariable("Env", _scriptBackend);

                try
                {
                    var result = source.Execute(scope);
                }
                catch (FinishExecutionException)
                {
                }

                return penalty.DoesApply;
            }

            return penalty.ScriptType switch
            {
                ScriptType.Lua => Task.Run(() => DoesLuaPenaltyApply(penalty.Script), cancellationToken),
                ScriptType.Python => Task.Run(() => DoesPythonPenaltyApply(penalty.Script), cancellationToken),
                _ => throw new Exception("Invalid script type")
            };
        }

        private Task<bool> IsTaskCompleted(ScoringItem task, CancellationToken cancellationToken = default)
        {
            bool IsLuaTaskCompleted(string scriptCode)
            {
                var script = new Script();
                var Task = new LuaTask();

                script.Globals.Set("Task", UserData.Create(Task));
                script.Globals.Set("Env", UserData.Create(_scriptBackend));

                try
                {
                    DynValue result = script.DoString(scriptCode);
                }
                catch (FinishExecutionException)
                {
                }

                return Task.IsCompleted;
            }

            bool IsPythonTaskCompleted(string scriptCode)
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(scriptCode);
                var task = new PythonTask();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    engine.Runtime.LoadAssembly(assembly);
                }

                scope.SetVariable("Task", task);
                scope.SetVariable("Env", _scriptBackend);

                try
                {
                    var result = source.Execute(scope);
                }
                catch (FinishExecutionException)
                {
                }

                return task.IsCompleted;
            }

            return task.ScriptType switch
            {
                ScriptType.Lua => Task.Run(() => IsLuaTaskCompleted(task.Script), cancellationToken),
                ScriptType.Python => Task.Run(() => IsPythonTaskCompleted(task.Script), cancellationToken),
                _ => throw new Exception("Unknown script type")
            };
        }
    }
}
