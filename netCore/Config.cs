using KillFallout4.Utils;

namespace KillFallout4
{
    internal class Config
    {
        public bool Restart { get; protected set; }
        public bool Verbose { get; protected set; }

        public string PathToConfigFile => _localConfigFilePath;

        public string? PathToLastKnownLauncher
        {
            get => _savedConfig.PathToLastLaunchFile; 
            set
            {
                if (value != null && value != _savedConfig.PathToLastLaunchFile )
                {
                    _isDirty = true;
                    _savedConfig.PathToLastLaunchFile = value;
                }
            }
        }

        private SavedConfig _savedConfig;
        private IF4KLogger _logger;
        private bool _isDirty = false;

        private string _localConfigFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KillFallout4.json");

        public Config(IF4KLogger logger)
        {
            _logger = logger;

            if (Verbose) logger.LogLevel = LogLevel.Verbose;

            LoadSavedConfig();
        }


        public void ProcessCommandArgs(string[] args)
        {
            foreach (var arg in args)
            {
                var switchChar = arg.StartsWithAny('-', '/', '\\');
                if (switchChar != null)
                {
                    var switchedArg = arg.TrimStart(switchChar.Value);
                    if (string.Compare(switchedArg, "restart", true) == 0) Restart = true;
                    if (string.Compare(switchedArg, "verbose", true) == 0) Verbose = true;
                }
            }

            if (Verbose) _logger.LogLevel = LogLevel.Verbose;
        }


        private void LoadSavedConfig()
        {
            if (File.Exists(_localConfigFilePath))
            {
                _logger.LogVerbose($"Loading config from: {_localConfigFilePath}");
                _savedConfig = JsonUtils.ReadFile<SavedConfig>(_localConfigFilePath) ?? new SavedConfig();

            } else
            {
                _savedConfig = new SavedConfig();
            }
        }


        public void SaveConfig()
        {
            _logger.LogVerbose($"Saving config to: {_localConfigFilePath}");

            if (!_isDirty)
            {
                _logger.LogVerbose("Skipping save as config has not changed");
            }
            else
            {
                JsonUtils.WriteFile(_localConfigFilePath, _savedConfig);
                _isDirty = false;
            }
        }
    }


    internal class SavedConfig
    {
        public string? PathToLastLaunchFile { get; set; }
    }
}
