using System.Diagnostics;

namespace Domotica.Server.Execute
{
    public sealed class Bash
    {
        public static string Execute(string command)
        {
            var proc = Process(command);
            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }

        public static async Task<string> ExecuteAsync(string command, double secsTimeout = 10)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(secsTimeout));
            var proc = Process(command);
            await proc.WaitForExitAsync(cts.Token);

            return await proc.StandardOutput.ReadToEndAsync();
        }

        private static Process Process(string command)
        {
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            return proc;
        }
    }
}
