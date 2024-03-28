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

        private string _localConfigFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KillFallout4.json");

        public Config(string[] args) 
        {
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
                _savedConfig = JsonFileReader.Read<SavedConfig>(_localConfigFilePath) ?? new SavedConfig();

            } else
            {
                _savedConfig = new SavedConfig();
            }
        }

        public void SaveConfig()
        {
            JsonFileReader.Write(_localConfigFilePath, _savedConfig);
        }
    }


    internal class SavedConfig
    {
        public string? PathToLastLaunchFile { get; set; }
    }
}
