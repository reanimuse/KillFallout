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
        public string PathToExe { get; protected set; }
        public string Name { get; protected set; }

        public bool HasExited => _process?.HasExited ?? true;

        private Process? _process;

        public Fallout4Instance(Process falloutProcess)
        {
            IsLauncher = falloutProcess.ProcessName.Contains("Launcher",StringComparison.InvariantCultureIgnoreCase);
            var pathToExe = falloutProcess?.MainModule?.FileName;
            PathToExe = pathToExe ?? string.Empty;

            Name = falloutProcess?.MainModule?.ModuleName ?? string.Empty;

            IsSteamApp = CheckSteamPath(PathToExe);
            _process = falloutProcess;
        }

        public void Kill()
        {
            _process?.Kill(true);
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
    }
}
