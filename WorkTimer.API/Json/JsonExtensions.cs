using System;
using System.Text.Json;

namespace WorkTimer4.API.Json
{
    public static class JsonExtensions
    {
        public static T ToObject<T>(this JsonElement element)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json);
        }
        public static T ToObject<T>(this JsonDocument document)
        {
            var json = document.RootElement.GetRawText();
            return JsonSerializer.Deserialize<T>(json);
        }

        public static object ToObject(this JsonElement jsonElement, Type type)
        {
            var json = jsonElement.GetRawText();
            return JsonSerializer.Deserialize(json, type);
        }
    }
}
