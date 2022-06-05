using System.Text.Json;

namespace WorkTimer4.API.Json
{
    public sealed class JsonSerialisation
    {
        /// <summary>
        /// JSON serialiser options
        /// </summary>
        public static JsonSerializerOptions SerialiserOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
    }
}
