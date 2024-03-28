using KillFallout4.Utils;

namespace KillFallout4
{
    internal class Config
    {
        public bool Restart { get; protected set; }

        public string? PathToLastKnownLauncher
        {
            get => _savedConfig.PathToLastLaunchFile; 
            set
            {
                if (value != null && value != _savedConfig.PathToLastLaunchFile )
                {
                    _savedConfig.PathToLastLaunchFile = value;
                }
            }
        }

        private SavedConfig _savedConfig;
        private IF4KLogger _logger;

        private string _localConfigFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KillFallout4.json");

        public Config(IF4KLogger logger, string[] args) 
        {
            _logger = logger;
            foreach (var arg in args)
            {
                var switchedArg = arg.TrimStart('-', '/', '\\');
                if (string.Compare(switchedArg, "restart", true) == 0) Restart = true;
            }

            LoadSavedConfig();
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
            JsonUtils.WriteFile(_localConfigFilePath, _savedConfig);
        }
    }


    internal class SavedConfig
    {
        public string? PathToLastLaunchFile { get; set; }
    }
}
