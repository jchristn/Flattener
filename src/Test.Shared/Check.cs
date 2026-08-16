namespace Test.Shared
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Lightweight assertion helpers used by Touchstone test descriptors.
    /// Every failed check throws a <see cref="TestAssertionException"/> so that Touchstone
    /// (and any adapter running the descriptor) records the case as failed with a clear message.
    /// </summary>
    internal static class Check
    {
        /// <summary>
        /// Assert that two strings are equal (ordinal).
        /// </summary>
        internal static void Equal(string expected, string actual, string context)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new TestAssertionException(
                    $"{context}: expected \"{Show(expected)}\" but got \"{Show(actual)}\".");
            }
        }

        /// <summary>
        /// Assert that two integers are equal.
        /// </summary>
        internal static void Equal(int expected, int actual, string context)
        {
            if (expected != actual)
            {
                throw new TestAssertionException(
                    $"{context}: expected {expected} but got {actual}.");
            }
        }

        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        internal static void True(bool condition, string context)
        {
            if (!condition)
            {
                throw new TestAssertionException($"{context}: expected condition to be true.");
            }
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        internal static void False(bool condition, string context)
        {
            if (condition)
            {
                throw new TestAssertionException($"{context}: expected condition to be false.");
            }
        }

        /// <summary>
        /// Assert that a reference is null.
        /// </summary>
        internal static void Null(object value, string context)
        {
            if (value != null)
            {
                throw new TestAssertionException(
                    $"{context}: expected null but got \"{Show(value.ToString())}\".");
            }
        }

        /// <summary>
        /// Assert the total key count of a collection.
        /// </summary>
        internal static void Count(int expected, NameValueCollection collection, string context)
        {
            if (collection.Count != expected)
            {
                throw new TestAssertionException(
                    $"{context}: expected {expected} keys but got {collection.Count} " +
                    $"(keys: {DescribeKeys(collection)}).");
            }
        }

        /// <summary>
        /// Assert that the collection contains a key.
        /// </summary>
        internal static void HasKey(NameValueCollection collection, string key, string context)
        {
            if (!ContainsKey(collection, key))
            {
                throw new TestAssertionException(
                    $"{context}: expected key \"{key}\" to be present " +
                    $"(keys: {DescribeKeys(collection)}).");
            }
        }

        /// <summary>
        /// Assert that the collection does not contain a key.
        /// </summary>
        internal static void NoKey(NameValueCollection collection, string key, string context)
        {
            if (ContainsKey(collection, key))
            {
                throw new TestAssertionException(
                    $"{context}: expected key \"{key}\" to be absent " +
                    $"(keys: {DescribeKeys(collection)}).");
            }
        }

        /// <summary>
        /// Assert that the value stored under a key equals an expected string.
        /// </summary>
        internal static void ValueEqual(NameValueCollection collection, string key, string expected, string context)
        {
            HasKey(collection, key, context);
            Equal(expected, collection[key], $"{context} [{key}]");
        }

        /// <summary>
        /// Assert the number of distinct values stored under a single key.
        /// A <see cref="NameValueCollection"/> can hold several values per key; this pins that behavior.
        /// </summary>
        internal static void ValueCount(NameValueCollection collection, string key, int expected, string context)
        {
            HasKey(collection, key, context);
            string[] values = collection.GetValues(key);
            int actual = values == null ? 0 : values.Length;
            if (actual != expected)
            {
                throw new TestAssertionException(
                    $"{context} [{key}]: expected {expected} value(s) but got {actual} " +
                    $"(values: {(values == null ? "(null)" : string.Join(" | ", values.Select(v => v ?? "(null)")))}).");
            }
        }

        /// <summary>
        /// Assert that one of the values stored under a key equals an expected string.
        /// </summary>
        internal static void HasValue(NameValueCollection collection, string key, string expected, string context)
        {
            HasKey(collection, key, context);
            string[] values = collection.GetValues(key);
            if (values == null || !values.Any(v => string.Equals(v, expected, StringComparison.Ordinal)))
            {
                throw new TestAssertionException(
                    $"{context} [{key}]: expected a value of \"{Show(expected)}\" " +
                    $"(values: {(values == null ? "(null)" : string.Join(" | ", values.Select(v => v ?? "(null)")))}).");
            }
        }

        /// <summary>
        /// Determine whether a key exists in the collection (null-safe, ordinal).
        /// </summary>
        internal static bool ContainsKey(NameValueCollection collection, string key)
        {
            foreach (string k in collection.AllKeys)
            {
                if (string.Equals(k, key, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        private static string DescribeKeys(NameValueCollection collection)
        {
            return string.Join(", ", collection.AllKeys.Select(k => k ?? "(null)"));
        }

        private static string Show(string value)
        {
            return value ?? "(null)";
        }
    }

    /// <summary>
    /// Exception thrown when a Touchstone test descriptor assertion fails.
    /// </summary>
    public sealed class TestAssertionException : Exception
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="message">Failure message.</param>
        public TestAssertionException(string message) : base(message)
        {
        }
    }
}
