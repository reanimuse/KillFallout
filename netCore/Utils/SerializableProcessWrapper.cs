using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4.Utils
{
    internal class SerializableProcessWrapper
    {
        public int BasePriority { get; private set; }
        public int ExitCode { get; private set; } = -1;
        public DateTime ExitTime { get; private set; } = DateTime.MaxValue;
        public int HandleCount { get; private set; }
        public bool HasExited { get; private set; } = true;
        public int Id { get; private set; }
        public string MachineName { get; private set; } = string.Empty; 
        public string ProcessName { get; private set; } = string.Empty;

        public SerializableProcessWrapper(Process? proc)
        {
            if (proc == null) {  return; }

            Id = proc.Id;
            BasePriority = proc.BasePriority;
            HandleCount = proc.HandleCount;
            MachineName = proc.MachineName;
            ProcessName = proc.ProcessName;

            HasExited = proc.HasExited;
            if (HasExited)
            {
                ExitCode = proc.ExitCode;
                ExitTime = proc.ExitTime;
            }
        }
    }


    internal static class ProcessExtensions
    {
        public static SerializableProcessWrapper ToSafeWrapper(this Process proc)
        {
            return new SerializableProcessWrapper(proc);
        }
    }
}
