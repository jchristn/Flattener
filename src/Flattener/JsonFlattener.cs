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
        /// Malformed JSON returns an empty collection, which is indistinguishable from a valid document with no values;
        /// use <see cref="TryFlatten(string, out NameValueCollection, out Exception, bool)"/> or
        /// <see cref="Flatten(string, bool, bool)"/> when the caller needs to detect malformed input.
        /// </summary>
        /// <param name="json">The JSON string to flatten.</param>
        /// <param name="includeNullItems">When true, includes null and empty items in the result.</param>
        /// <returns>A NameValueCollection containing flattened key-value pairs.</returns>
        public static NameValueCollection Flatten(string json, bool includeNullItems = false)
        {
            try
            {
                return FlattenInternal(json, includeNullItems);
            }
            catch (JsonException)
            {
                // Return empty collection for malformed JSON rather than throwing
                return new NameValueCollection();
            }
        }

        /// <summary>
        /// Flattens a JSON string into a NameValueCollection with dot notation for nested properties,
        /// optionally throwing when the input is malformed.
        /// </summary>
        /// <param name="json">The JSON string to flatten.</param>
        /// <param name="includeNullItems">When true, includes null and empty items in the result.</param>
        /// <param name="throwOnError">When true, malformed JSON throws <see cref="System.Text.Json.JsonException"/>; when false, an empty collection is returned.</param>
        /// <returns>A NameValueCollection containing flattened key-value pairs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="System.Text.Json.JsonException">Thrown when <paramref name="json"/> is malformed and <paramref name="throwOnError"/> is true.</exception>
        public static NameValueCollection Flatten(string json, bool includeNullItems, bool throwOnError)
        {
            if (throwOnError) return FlattenInternal(json, includeNullItems);
            return Flatten(json, includeNullItems);
        }

        /// <summary>
        /// Attempts to flatten a JSON string into a NameValueCollection with dot notation for nested properties.
        /// Does not throw for null or malformed input; distinguishes malformed input from a valid document with no values.
        /// </summary>
        /// <param name="json">The JSON string to flatten.</param>
        /// <param name="result">The flattened key-value pairs on success; an empty collection on failure.</param>
        /// <param name="error">Null on success; on failure, the exception describing why the input could not be flattened
        /// (<see cref="System.Text.Json.JsonException"/> for malformed input, <see cref="ArgumentNullException"/> for null input).</param>
        /// <param name="includeNullItems">When true, includes null and empty items in the result.</param>
        /// <returns>True if the input was parsed and flattened; false if it was null or malformed.</returns>
        public static bool TryFlatten(string json, out NameValueCollection result, out Exception error, bool includeNullItems = false)
        {
            result = new NameValueCollection();

            if (json == null)
            {
                error = new ArgumentNullException(nameof(json));
                return false;
            }

            try
            {
                result = FlattenInternal(json, includeNullItems);
                error = null;
                return true;
            }
            catch (JsonException e)
            {
                error = e;
                return false;
            }
        }

        private static NameValueCollection FlattenInternal(string json, bool includeNullItems)
        {
            NameValueCollection result = new NameValueCollection();
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                FlattenJsonElement(document.RootElement, "", result, includeNullItems);
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