using KillFallout4.Utils;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private string _pathToFalloutFolder = string.Empty;
        private bool _isSteamApp;
        public bool IsDisposed { get; protected set; }

        public string PathToLauncher { get; set; }
        public string PathToFalloutFolder { get; set; }


        public Fallout4Killer(ConsoleWriter writer, IF4KLogger logger, string? pathToLastKnownLauncher)
        {
            _writer = writer;
            _logger = logger;
            PathToLauncher = pathToLastKnownLauncher ?? string.Empty;

            _isSteamApp = Fallout4Instance.CheckSteamPath(PathToLauncher);
        }

        ~Fallout4Killer() {
            Dispose(false);
        }


        public void FindInstances()
        {
            var allProcs = Process.GetProcesses();

            _FalloutInstances = allProcs.Where(x => Fallout4Instance.IsFallout4Process(x))
                .Select(x => new Fallout4Instance(_logger, x)).ToArray();

            _logger.LogVerbose($"Found {_FalloutInstances.Length} Fallout4 processes in {allProcs.Length} total running processes");

            foreach (var proc in _FalloutInstances)
            {
                if (proc.IsSteamApp) { _isSteamApp = true; }
                if (File.Exists(proc.PathToExe))
                {
                    var info = new FileInfo(proc.PathToExe);
                    _pathToFalloutFolder = info.Directory?.FullName ?? string.Empty;

                    if (proc.IsFallout4Exe)
                    {
                        this.PathToFalloutFolder = info.Directory?.FullName ?? string.Empty;
                    }
                }
            }

            PathToLauncher = FindLauncherPath(false);
        }


        public void KillAll()
        {
            foreach (var proc in _FalloutInstances)
            {
                proc.Kill(_writer);
            }
        }


        public void Restart(bool useLauncher = false, bool useScriptExtender = false)
        {
            if (PathToLauncher == string.Empty)
            {
                _logger.LogVerbose($"Unable to restart as there is no PathToLauncher defined");
                return;
            }

            Process? launchedProcess;

            _writer.WriteLine(ConsoleColor.Green, $"Starting Fallout... (launcher: {useLauncher})");

            _logger.LogVerbose($"Starting fallout using: {PathToLauncher}");

            var pathToStartFallout = useScriptExtender ? UseScriptExtenderIfPresent(PathToLauncher) : PathToLauncher;
            var workingFolder = string.Empty;
            if (pathToStartFallout.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                var info = new FileInfo(pathToStartFallout);
                workingFolder = info.Directory?.FullName ?? string.Empty;
            }

            if (UseProcessStartInfo(pathToStartFallout, useScriptExtender))
            {
                var startArgs = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    WorkingDirectory = workingFolder,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = pathToStartFallout
                };

                if (useScriptExtender && IsScriptExtender(pathToStartFallout))
                {
                    startArgs.UseShellExecute = false;
                    _writer.WriteLine(ConsoleColor.Green, $" ... using ScriptExtender ");
                }

                launchedProcess = Process.Start(startArgs);
            }
            else
            {
                launchedProcess = Process.Start(pathToStartFallout);
            }

            var result = JsonUtils.Serialize(new SerializableProcessWrapper(launchedProcess),false,false);

            _logger.LogVerbose($"Process {launchedProcess?.Id} Launched: {result}");
        }

        private bool UseProcessStartInfo(string path, bool useScriptExtender)
        {
            var result = Fallout4Instance.CheckSteamPath(path);
            if (useScriptExtender && IsScriptExtender(path)) { result = true; }
            return result;
        }


        private string UseScriptExtenderIfPresent(string launcherPath)
        {
            if (PathToFalloutFolder == string.Empty) return launcherPath;

            var curFolder = new DirectoryInfo(PathToFalloutFolder);

            var f4sePath = Path.Combine(curFolder.FullName ?? "", "f4se_loader.exe");
            if (File.Exists(f4sePath))
            {
                _logger.LogVerbose($"Starting using F4SE");
                return f4sePath;
            }
            return launcherPath;
        }


        private bool IsScriptExtender(string path)
        {
            return path.Contains("f4se_loader.exe", StringComparison.InvariantCultureIgnoreCase);
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
                }
                else
                {
                    launchPath = PathToLauncher;
                }
            }

            return launchPath;
        }


        public void Dispose()
        {
            if (IsDisposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            // release any unmanaged instances
            for (var i = 0; i < _FalloutInstances.Length; i++)
            {
                var proc = _FalloutInstances[i];
                proc.Dispose();
            }
            _FalloutInstances = new Fallout4Instance[0];

            IsDisposed = true;
        }
    }
}
