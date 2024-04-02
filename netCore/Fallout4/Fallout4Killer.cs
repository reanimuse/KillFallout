using KillFallout4.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4.Fallout4
{
    internal class Fallout4Killer:IDisposable
    {
        public bool ActiveInstances => _FalloutInstances.Length > 0;


        private Fallout4Instance[] _FalloutInstances = new Fallout4Instance[0];
        private ConsoleWriter _writer;
        private IF4KLogger _logger;

        private string _pathToFalloutFolder;
        private bool _isSteamApp;

        public string PathToLauncher { get; set; }

        public Fallout4Killer(ConsoleWriter writer, IF4KLogger logger, string? pathToLastKnownLauncher)
        {
            _writer = writer;
            _logger = logger;
            PathToLauncher = pathToLastKnownLauncher ?? string.Empty;

            _isSteamApp = Fallout4Instance.CheckSteamPath(PathToLauncher);
        }


        public void FindInstances()
        {
            var allProcs = Process.GetProcesses();

            _FalloutInstances = allProcs.Where(x => x.ProcessName.StartsWith("Fallout", StringComparison.OrdinalIgnoreCase))
                .Select(x => new Fallout4Instance(_logger, x)).ToArray();

            _logger.LogVerbose($"Found {_FalloutInstances.Length} Fallout4 processes in {allProcs.Length} total running processes");

            foreach (var proc in _FalloutInstances)
            {
                if (proc.IsSteamApp) { _isSteamApp = true; }
                if (File.Exists(proc.PathToExe))
                {
                    var info = new FileInfo(proc.PathToExe);
                    _pathToFalloutFolder = info.Directory?.FullName ?? string.Empty;
                }
            }

            PathToLauncher = FindLauncherPath(false);
        }

        public void KillAll()
        {
            foreach (var proc in _FalloutInstances)
            {
                _writer.WriteLine(ConsoleColor.Yellow, $"Killing {proc.Name}...");
                proc.Kill();
            }
        }


        public void Restart(bool useLauncher = false)
        {
            Process launchedProcess;

            _writer.WriteLine(ConsoleColor.Green, $"Starting Fallout... (launcher: {useLauncher})");

            _logger.LogVerbose($"Starting fallout using: {PathToLauncher}");


            if (Fallout4Instance.CheckSteamPath(PathToLauncher))
            {
                launchedProcess = Process.Start(new ProcessStartInfo(PathToLauncher) { UseShellExecute = true });
            }
            else
            {
                launchedProcess = Process.Start(PathToLauncher);
            }

            var result = JsonUtils.Serialize(launchedProcess?.ToSafeWrapper(),false,false);
            _logger.LogVerbose($"Process Launched: {result}");
        }


        private string FindLauncherPath(bool useLauncher)
        {
            string launchPath;

            if (_isSteamApp)
            {
                launchPath = "steam://rungameid/377160";
                if (!useLauncher) launchPath += " -nolauncher";
            }
            else
            {
                if (!string.IsNullOrEmpty(_pathToFalloutFolder))
                {
                    var launchFile = useLauncher ? "Fallout4Launcher.exe" : "Fallout4.exe";
                    launchPath = Path.Join(_pathToFalloutFolder, launchFile);
                } else
                {
                    launchPath = PathToLauncher;
                }
            }

            return launchPath;
        }


        public void Dispose()
        {
            for (var i = 0; i < _FalloutInstances.Length; i++)
            {
                var proc = _FalloutInstances[i];
                if (proc != null) proc.Dispose();
                _FalloutInstances[i] = null;
            }
            _FalloutInstances = new Fallout4Instance[0];
        }
    }
}
