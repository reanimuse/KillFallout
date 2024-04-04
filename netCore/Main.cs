using KillFallout4.Fallout4;
using KillFallout4.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var logger = new Logger();
            logger.LogInfo("Starting app");
            var config = new Config(logger, args);
            var writer = new ConsoleWriter(logger);

            writer.WriteLineVerbose($"Config path: {config.PathToConfigFile}");
            writer.WriteLineVerbose($"Log File: {logger.PathToCurrentLogFile}");

            using (var killer = new Fallout4Killer(writer, logger, config.PathToLastKnownLauncher))
            {
                killer.FindInstances();

                if (!killer.ActiveInstances)
                {
                    killer.PathToLauncher = config.PathToLastKnownLauncher?? string.Empty;
                    writer.WriteLine(ConsoleColor.Green, "Fallout is not running");

                    if (config.Restart && config.PathToLastKnownLauncher != string.Empty)
                    {
                        killer.Restart();
                    }
                    return 1;
                }

                killer.KillAll();

                if (config.Restart)
                {
                    killer.Restart();
                }

                config.PathToLastKnownLauncher = killer.PathToLauncher;
                config.SaveConfig();
            }

            return 0;
        }
    }
}
