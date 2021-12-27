using System.Diagnostics;

namespace ScoringEngine.Client.Scoring
{
    internal class ProcessManagement
    {
        public static async Task<ProcessResultInfo> RunProcessForOutput(string fileName, string arguments)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            int ExitCode = 0;
            List<string> StandardOutputLines = new();
            List<string> StandardErrorLines = new();
            List<string> OutputLines = new();

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = await process.StandardOutput.ReadLineAsync();
                        StandardOutputLines.Add(line);
                        OutputLines.Add(line);
                    }
                }),
                Task.Run(async () =>
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        string line = await process.StandardError.ReadLineAsync();
                        StandardErrorLines.Add(line);
                        OutputLines.Add(line);
                    }
                }),
                Task.Run(async () =>
                {
                    await process.WaitForExitAsync();

                    ExitCode = process.ExitCode;
                })
            );

            var results = new ProcessResultInfo
            {
                ExitCode = ExitCode,
                StandardOutputLines = StandardOutputLines,
                StandardErrorLines = StandardErrorLines,
                OutputLines = OutputLines
            };

            return results;
        }
    }

    internal class ProcessResultInfo
    {
        public int ExitCode { get; init; }

        public IEnumerable<string> StandardOutputLines { get; init; }

        public IEnumerable<string> StandardErrorLines { get; init; }

        public IEnumerable<string> OutputLines { get; init; }
    }

}
