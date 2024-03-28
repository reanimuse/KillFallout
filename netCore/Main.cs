using KillFallout4.Fallout4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var writer = new ConsoleWriter();
            var config = new Config(args);

            using (var killer = new Fallout4Killer(writer, config.PathToLastKnownLauncher))
            {
                killer.FindInstances();

                if (!killer.ActiveInstances)
                {
                    killer.PathToLauncher = config.PathToLastKnownLauncher;
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
