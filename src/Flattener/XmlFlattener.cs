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
        /// </summary>
        /// <param name="xml">The XML string to flatten.</param>
        /// <param name="includeNullItems">When true, includes empty elements and null values in the result.</param>
        /// <returns>A NameValueCollection containing flattened key-value pairs.</returns>
        public static NameValueCollection Flatten(string xml, bool includeNullItems = false)
        {
            var result = new NameValueCollection();
            try
            {
                XDocument doc = XDocument.Parse(xml);
                FlattenXElement(doc.Root, "", result, includeNullItems);
            }
            catch (System.Xml.XmlException)
            {
                // Return empty collection for malformed XML rather than throwing
            }
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