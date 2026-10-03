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
    /// XML flattener.
    /// </summary>
    public static class XmlFlattener
    {
        /// <summary>
        /// Flattens an XML string into a NameValueCollection with dot notation for nested elements.
        /// This can handle multiple values per key.
        /// Malformed XML returns an empty collection, which is indistinguishable from a valid document with no values;
        /// use <see cref="TryFlatten(string, out NameValueCollection, out Exception, bool)"/> or
        /// <see cref="Flatten(string, bool, bool)"/> when the caller needs to detect malformed input.
        /// </summary>
        /// <param name="xml">The XML string to flatten.</param>
        /// <param name="includeNullItems">When true, includes empty elements and null values in the result.</param>
        /// <returns>A NameValueCollection containing flattened key-value pairs.</returns>
        public static NameValueCollection Flatten(string xml, bool includeNullItems = false)
        {
            try
            {
                return FlattenInternal(xml, includeNullItems);
            }
            catch (System.Xml.XmlException)
            {
                // Return empty collection for malformed XML rather than throwing
                return new NameValueCollection();
            }
        }

        /// <summary>
        /// Flattens an XML string into a NameValueCollection with dot notation for nested elements,
        /// optionally throwing when the input is malformed.
        /// </summary>
        /// <param name="xml">The XML string to flatten.</param>
        /// <param name="includeNullItems">When true, includes empty elements and null values in the result.</param>
        /// <param name="throwOnError">When true, malformed XML throws <see cref="System.Xml.XmlException"/>; when false, an empty collection is returned.</param>
        /// <returns>A NameValueCollection containing flattened key-value pairs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xml"/> is null.</exception>
        /// <exception cref="System.Xml.XmlException">Thrown when <paramref name="xml"/> is malformed and <paramref name="throwOnError"/> is true.</exception>
        public static NameValueCollection Flatten(string xml, bool includeNullItems, bool throwOnError)
        {
            if (throwOnError) return FlattenInternal(xml, includeNullItems);
            return Flatten(xml, includeNullItems);
        }

        /// <summary>
        /// Attempts to flatten an XML string into a NameValueCollection with dot notation for nested elements.
        /// Does not throw for null or malformed input; distinguishes malformed input from a valid document with no values.
        /// </summary>
        /// <param name="xml">The XML string to flatten.</param>
        /// <param name="result">The flattened key-value pairs on success; an empty collection on failure.</param>
        /// <param name="error">Null on success; on failure, the exception describing why the input could not be flattened
        /// (<see cref="System.Xml.XmlException"/> for malformed input, <see cref="ArgumentNullException"/> for null input).</param>
        /// <param name="includeNullItems">When true, includes empty elements and null values in the result.</param>
        /// <returns>True if the input was parsed and flattened; false if it was null or malformed.</returns>
        public static bool TryFlatten(string xml, out NameValueCollection result, out Exception error, bool includeNullItems = false)
        {
            result = new NameValueCollection();

            if (xml == null)
            {
                error = new ArgumentNullException(nameof(xml));
                return false;
            }

            try
            {
                result = FlattenInternal(xml, includeNullItems);
                error = null;
                return true;
            }
            catch (System.Xml.XmlException e)
            {
                error = e;
                return false;
            }
        }

        private static NameValueCollection FlattenInternal(string xml, bool includeNullItems)
        {
            NameValueCollection result = new NameValueCollection();
            XDocument doc = XDocument.Parse(xml);
            FlattenXElement(doc.Root, "", result, includeNullItems);
            return result;
        }

        private static void FlattenXElement(
            XElement element,
            string prefix,
            NameValueCollection result,
            bool includeNullItems)
        {
            // Handle attributes
            foreach (var attr in element.Attributes())
            {
                string attrKey = string.IsNullOrEmpty(prefix)
                    ? $"{element.Name}.@{attr.Name}"
                    : $"{prefix}.@{attr.Name}";

                // Include attribute if it has a value or includeNullItems is true
                if (!string.IsNullOrEmpty(attr.Value) || includeNullItems)
                {
                    result.Add(attrKey, attr.Value);
                }
            }

            // Check if the element has child elements
            var childElements = element.Elements().ToList();
            if (childElements.Count > 0)
            {
                // Group child elements by name to handle arrays
                var groupedElements = childElements.GroupBy(e => e.Name.ToString());
                foreach (var group in groupedElements)
                {
                    string elementName = group.Key;
                    var elements = group.ToList();
                    // If there's only one element with this name, process it as a regular element
                    if (elements.Count == 1)
                    {
                        string newPrefix = string.IsNullOrEmpty(prefix)
                            ? elementName
                            : $"{prefix}.{elementName}";
                        FlattenXElement(elements[0], newPrefix, result, includeNullItems);
                    }
                    // If there are multiple elements with the same name, process them as an array
                    else
                    {
                        for (int i = 0; i < elements.Count; i++)
                        {
                            string newPrefix = string.IsNullOrEmpty(prefix)
                                ? $"{elementName}[{i}]"
                                : $"{prefix}.{elementName}[{i}]";
                            FlattenXElement(elements[i], newPrefix, result, includeNullItems);
                        }
                    }
                }
            }
            // If the element has no child elements, it's a leaf node
            else if (!element.HasElements)
            {
                string key = string.IsNullOrEmpty(prefix) ? element.Name.ToString() : prefix;

                // Include if value is not empty or includeNullItems is true
                if (!string.IsNullOrEmpty(element.Value) || includeNullItems)
                {
                    result.Add(key, element.Value);
                }
            }
            // Empty element with no content - include if configured
            else if (includeNullItems)
            {
                string key = string.IsNullOrEmpty(prefix) ? element.Name.ToString() : prefix;
                result.Add(key, "");
            }
        }
    }
}