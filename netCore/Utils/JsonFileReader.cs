using System.Text.Json;

namespace KillFallout4.Utils
{
    internal static class JsonFileReader
    {
        public static T? Read<T>(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(text);
        }

        public static void Write<T>(string filePath, T value)
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(value, options);
            File.WriteAllText(filePath, json);
        }
    }
}
