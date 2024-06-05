using KillFallout4.Fallout4;
using KillFallout4.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4
{
    internal class Program
    {
        private static F4KLogger logger = new F4KLogger();
        private static Config config = new Config(logger);

        static int Main(string[] args)
        {
            logger.LogInfo("Starting app");

            config.ProcessCommandArgs(args);
            var writer = new ConsoleWriter(logger, config.Verbose);

            writer.WriteLineVerbose($"Config path: {config.PathToConfigFile}");
            writer.WriteLineVerbose($"Log File: {logger.PathToCurrentLogFile}");

            using (var killer = new Fallout4Killer(writer, logger, config.PathToLastKnownLauncher))
            {
                killer.PathToLauncher = config.PathToLastKnownLauncher ?? string.Empty;
                killer.PathToFalloutFolder = config.Fallout4Folder ?? string.Empty;

                KillFalloutInstances(writer, killer);

                if (config.Restart)
                {
                    killer.Restart(false, config.UseScriptExtenderIfPresent);
                }

                config.PathToLastKnownLauncher = killer.PathToLauncher;
                config.Fallout4Folder = killer.PathToFalloutFolder;
                config.SaveConfig();
            }

            return 0;
        }


        private static void KillFalloutInstances(ConsoleWriter writer, Fallout4Killer killer)
        {
            killer.FindInstances();

            if (killer.ActiveInstances)
            {
                killer.KillAll();
            }
            else
            {
                writer.WriteLine(ConsoleColor.Green, "Fallout is not running");
            }
        }
    }
}
