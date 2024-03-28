using System.Text.Json;

namespace KillFallout4.Utils
{
    internal static class JsonUtils
    {
        public static T? ReadFile<T>(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(text);
        }


        public static void WriteFile<T>(string filePath, T value)
        {
            var json = Serialize(value, true, false);
            File.WriteAllText(filePath, json);
        }


        public static string Serialize(object? obj, bool indented = true, bool includeFields = false)
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                WriteIndented = indented,
                IncludeFields = includeFields
            };
            var json = JsonSerializer.Serialize(obj, options);

            return json;
        }
    }
}
