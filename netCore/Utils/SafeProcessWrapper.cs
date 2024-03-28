using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4.Utils
{
    internal class SafeProcessWrapper
    {
        public int BasePriority { get; private set; }
        public int ExitCode { get; private set; }
        public DateTime ExitTime { get; private set; }
        public int HandleCount { get; private set; }
        public bool HasExited { get; private set; }
        public int Id { get; private set; }
        public string MachineName { get; private set; }
        public string ProcessName { get; private set; }

        public SafeProcessWrapper(Process proc)
        {
            BasePriority = proc.BasePriority;
            HasExited = proc.HasExited;
            if (HasExited)
            {
                ExitCode = proc.ExitCode;
                ExitTime = proc.ExitTime;
            } else
            {
                ExitCode = -1;
                ExitTime = DateTime.MaxValue;
            }
            HandleCount = proc.HandleCount;
            Id = proc.Id;
            MachineName = proc.MachineName;
            ProcessName = proc.ProcessName;
        }
    }

    internal static class ProcessExtensions
    {
        public static SafeProcessWrapper ToSafeWrapper(this Process proc)
        {
            return new SafeProcessWrapper(proc);
        }
    }
}
