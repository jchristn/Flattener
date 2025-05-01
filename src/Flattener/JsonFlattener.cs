namespace Flattener
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// JSON flattener.
    /// </summary>
    public static class JsonFlattener
    {
        /// <summary>
        /// Flattens a JSON string into a NameValueCollection with dot notation for nested properties.
        /// This can handle multiple values per key.
        /// </summary>
        /// <param name="json">The JSON string to flatten.</param>
        /// <param name="includeNullItems">When true, includes null and empty items in the result.</param>
        /// <returns>A NameValueCollection containing flattened key-value pairs.</returns>
        public static NameValueCollection Flatten(string json, bool includeNullItems = false)
        {
            var result = new NameValueCollection();
            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    FlattenJsonElement(document.RootElement, "", result, includeNullItems);
                }
            }
            catch (JsonException)
            {
            }
            return result;
        }
        private static void FlattenJsonElement(
            JsonElement element,
            string prefix,
            NameValueCollection result,
            bool includeNullItems)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    // Add empty object indicator if configured
                    if (includeNullItems && !element.EnumerateObject().Any())
                    {
                        result.Add(prefix, "{}");
                    }

                    foreach (var property in element.EnumerateObject())
                    {
                        string newPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                        FlattenJsonElement(property.Value, newPrefix, result, includeNullItems);
                    }
                    break;
                case JsonValueKind.Array:
                    // Add empty array indicator if configured
                    if (includeNullItems && !element.EnumerateArray().Any())
                    {
                        result.Add(prefix, "[]");
                    }

                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        string newPrefix = $"{prefix}[{index}]";
                        FlattenJsonElement(item, newPrefix, result, includeNullItems);
                        index++;
                    }
                    break;
                default:
                    // Handle primitive types
                    object value = GetJsonElementValue(element);

                    // Only add null values if configured to do so
                    if (value != null || includeNullItems)
                    {
                        result.Add(prefix, value?.ToString());
                    }
                    break;
            }
        }
        private static object GetJsonElementValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long longValue))
                        return longValue;
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString();
            }
        }
    }
}