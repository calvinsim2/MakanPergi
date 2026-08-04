using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common
{
    public static class StringExtensions
    {
        public static string ToJsonString(this object jsonString)
        {
            if (jsonString is null)
            {
                return string.Empty;
            }

            return JsonSerializer.Serialize(jsonString,
                new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles, WriteIndented = true });
        }
    }
}
