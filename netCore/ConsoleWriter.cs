using KillFallout4.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KillFallout4
{
    internal class ConsoleWriter
    {
        IF4KLogger _logger;

        public ConsoleWriter(IF4KLogger logger)
        {
            _logger = logger;
        }
        public void WriteLine(string msg)
        {
            _logger.LogInfo(msg);
            Console.WriteLine(msg);
        }

        public void WriteLine(ConsoleColor foregroundColor, string msg)
        {
            WriteLine(foregroundColor, Console.BackgroundColor, msg);
        }

        public void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string msg)
        {
            Write(foregroundColor, backgroundColor, $"{msg}{Environment.NewLine}");
        }

        public void Write(string msg)
        {
            _logger.LogInfo(msg);
            Console.Write(msg);
        }

        public void Write(ConsoleColor foregroundColor, string msg)
        {
            Write(foregroundColor, Console.BackgroundColor, msg);
        }

        public void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string msg)
        {
            var currentColors = (
                ForegroundColor: Console.ForegroundColor,
                BackgroundColor: Console.BackgroundColor
            );

            SetColors(foregroundColor, backgroundColor);
            _logger.LogInfo(msg);
            Console.Write(msg);
            SetColors(currentColors.ForegroundColor, currentColors.BackgroundColor);
        }

        private void SetColors(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }
    }
}
