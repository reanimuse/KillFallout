using KillFallout4.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4.Fallout4
{
    internal class Fallout4Instance:IDisposable
    {
        public bool IsLauncher { get; protected set; }
        public bool IsSteamApp { get; protected set; }
        public bool IsScriptExtender { get; protected set; }
        public bool IsFallout4Exe { get; protected set; }
        public string PathToExe { get; protected set; }
        public string Name { get; protected set; }
        public string ProcessName { get; protected set; }

        public bool HasExited => _process?.HasExited ?? true;
        public int ExitCode => (_process != null && _process.HasExited) ? _process.ExitCode : -1;
        public DateTime ExitTime => (_process != null && _process.HasExited) ? _process.ExitTime : DateTime.MaxValue;

        private Process? _process;
        private IF4KLogger _logger;

        public static string[] Fallout4ProcessNames = ["fallout4", "fallout4launcher", "f4se_loader"];

        public Fallout4Instance(IF4KLogger logger, Process falloutProcess)
        {
            _logger = logger;
            IsLauncher = falloutProcess.ProcessName.Contains("fallout4launcher", StringComparison.InvariantCultureIgnoreCase);
            IsScriptExtender = falloutProcess.ProcessName.Contains("f4se_loader", StringComparison.InvariantCultureIgnoreCase);

            var pathToExe = falloutProcess?.MainModule?.FileName;
            PathToExe = pathToExe ?? string.Empty;

            Name = falloutProcess?.MainModule?.ModuleName ?? string.Empty;
            ProcessName = falloutProcess?.ProcessName ?? string.Empty;
            IsFallout4Exe = Name.Contains("fallout4.exe", StringComparison.InvariantCultureIgnoreCase);

            IsSteamApp = CheckSteamPath(PathToExe);
            _process = falloutProcess;
        }


        public void Kill(ConsoleWriter writer)
        {
            writer.Write(ConsoleColor.Yellow, $"Killing {Name}");
            if (HasExited)
            {
                _logger.LogVerbose($"Process {Name} already exited with: {ExitCode} {DateTime.Now.Subtract(ExitTime).TotalMilliseconds} ms ago");
                writer.WriteLine(ConsoleColor.Green, $" no longer running");
                return;
            }

            _logger.LogVerbose($"Killing Process {Name}...");
            System.Threading.Thread.Sleep(10);
            _process?.Kill(true);
            System.Threading.Thread.Sleep(250);

            var stillRunning = Process.GetProcesses().Any(x => x.ProcessName == ProcessName);

            // looping because the .HasExited property on the Process object does not reliably show if the process has actually exited
            while (stillRunning)
            {
                _logger.LogVerbose($"Waiting for {Name} to exit... (HasExited: {HasExited})");
                writer.Write(ConsoleColor.Yellow, $".");
                _process?.Kill(true);
                System.Threading.Thread.Sleep(1000);
                stillRunning = Process.GetProcesses().Any(x => x.ProcessName == ProcessName);
            }
            writer.WriteLine(ConsoleColor.Green, $" done");
        }


        public void Dispose()
        {
            if (_process != null) { _process.Dispose(); }
            _process = null;
        }


        public static bool CheckSteamPath(string path)
        {
            var isSteam = path.Contains("steamapps", StringComparison.InvariantCultureIgnoreCase)
                || path.Contains("steam://", StringComparison.InvariantCultureIgnoreCase);

            return isSteam;
        }

        public static bool IsFallout4Process(Process process)
        {
            return Fallout4ProcessNames.Any(x => string.Compare(x, process.ProcessName, true) == 0);
        }
    }
}
