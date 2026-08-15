namespace Test.JsonFlattener
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Flattener;

    class Program
    {
        private static int _passed = 0;
        private static int _failed = 0;

        static int Main(string[] args)
        {
            Console.WriteLine("JSON Flattener Test Program");
            Console.WriteLine("==========================\n");

            SimpleObject_ExcludeNulls_ProducesExpectedPairs();
            NestedObject_UsesDotNotation();
            Arrays_UseIndexNotation();
            IncludeNullItems_EmitsNullEmptyMarkers();
            ScalarArray_IndexesEachElement();

            // Negative cases
            InvalidJson_ReturnsEmptyCollection();
            EmptyString_ReturnsEmptyCollection();
            NullValue_ExcludedByDefault();
            NonExistentKey_ReturnsNull();

            Console.WriteLine();
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Results: {_passed} passed, {_failed} failed");

            return _failed == 0 ? 0 : 1;
        }

        // ---------- Positive test cases ----------

        static void SimpleObject_ExcludeNulls_ProducesExpectedPairs()
        {
            string json = @"{ ""name"": ""John Doe"", ""age"": 30, ""isActive"": true }";
            NameValueCollection result = JsonFlattener.Flatten(json, false);

            AssertEqual("SimpleObject count", 3, result.Count);
            AssertEqual("SimpleObject name", "John Doe", result["name"]);
            AssertEqual("SimpleObject age", "30", result["age"]);
            // JSON booleans are surfaced via bool.ToString(), yielding "True"/"False".
            AssertEqual("SimpleObject isActive", "True", result["isActive"]);
        }

        static void NestedObject_UsesDotNotation()
        {
            string json = @"{
                ""person"": {
                    ""name"": ""Jane Smith"",
                    ""address"": { ""street"": ""123 Main St"", ""zipCode"": 12345 }
                },
                ""isEmployee"": true
            }";
            NameValueCollection result = JsonFlattener.Flatten(json, false);

            AssertEqual("Nested person.name", "Jane Smith", result["person.name"]);
            AssertEqual("Nested address.street", "123 Main St", result["person.address.street"]);
            AssertEqual("Nested address.zipCode", "12345", result["person.address.zipCode"]);
            AssertEqual("Nested isEmployee", "True", result["isEmployee"]);
        }

        static void Arrays_UseIndexNotation()
        {
            string json = @"{
                ""employees"": [
                    { ""name"": ""Alice"", ""skills"": [""C#"", ""JavaScript"", ""SQL""] },
                    { ""name"": ""Bob"", ""skills"": [""Python"", ""Java""] }
                ],
                ""departmentId"": 42
            }";
            NameValueCollection result = JsonFlattener.Flatten(json, false);

            AssertEqual("Array employees[0].name", "Alice", result["employees[0].name"]);
            AssertEqual("Array employees[0].skills[0]", "C#", result["employees[0].skills[0]"]);
            AssertEqual("Array employees[0].skills[2]", "SQL", result["employees[0].skills[2]"]);
            AssertEqual("Array employees[1].name", "Bob", result["employees[1].name"]);
            AssertEqual("Array employees[1].skills[1]", "Java", result["employees[1].skills[1]"]);
            AssertEqual("Array departmentId", "42", result["departmentId"]);
        }

        static void IncludeNullItems_EmitsNullEmptyMarkers()
        {
            string json = @"{
                ""emptyObject"": {},
                ""emptyArray"": [],
                ""nullValue"": null,
                ""emptyString"": """"
            }";
            NameValueCollection result = JsonFlattener.Flatten(json, true);

            AssertEqual("IncludeNulls emptyObject", "{}", result["emptyObject"]);
            AssertEqual("IncludeNulls emptyArray", "[]", result["emptyArray"]);
            AssertEqual("IncludeNulls emptyString", "", result["emptyString"]);
            AssertTrue("IncludeNulls nullValue key present", ContainsKey(result, "nullValue"));
        }

        static void ScalarArray_IndexesEachElement()
        {
            string json = @"{ ""tags"": [""a"", ""b""] }";
            NameValueCollection result = JsonFlattener.Flatten(json, false);

            AssertEqual("Scalar array count", 2, result.Count);
            AssertEqual("Scalar array tags[0]", "a", result["tags[0]"]);
            AssertEqual("Scalar array tags[1]", "b", result["tags[1]"]);
        }

        // ---------- Negative test cases ----------

        static void InvalidJson_ReturnsEmptyCollection()
        {
            string json = @"{""name"": ""Incomplete JSON";
            NameValueCollection result = JsonFlattener.Flatten(json, false);
            AssertEqual("Invalid JSON count", 0, result.Count);
        }

        static void EmptyString_ReturnsEmptyCollection()
        {
            NameValueCollection result = JsonFlattener.Flatten("", false);
            AssertEqual("Empty string count", 0, result.Count);
        }

        static void NullValue_ExcludedByDefault()
        {
            string json = @"{ ""present"": ""yes"", ""missing"": null }";
            NameValueCollection result = JsonFlattener.Flatten(json, false);

            AssertEqual("NullExcluded count", 1, result.Count);
            AssertTrue("NullExcluded missing absent", !ContainsKey(result, "missing"));
            AssertEqual("NullExcluded present", "yes", result["present"]);
        }

        static void NonExistentKey_ReturnsNull()
        {
            string json = @"{ ""a"": 1 }";
            NameValueCollection result = JsonFlattener.Flatten(json, false);
            AssertTrue("NonExistent key returns null", result["does.not.exist"] == null);
        }

        // ---------- Assertion helpers ----------

        static bool ContainsKey(NameValueCollection collection, string key)
        {
            foreach (string k in collection.AllKeys)
            {
                if (k == key) return true;
            }
            return false;
        }

        static void AssertEqual(string name, string expected, string actual)
        {
            if (expected == actual)
            {
                Pass(name);
            }
            else
            {
                Fail(name, $"expected \"{expected}\" but got \"{actual ?? "null"}\"");
            }
        }

        static void AssertEqual(string name, int expected, int actual)
        {
            if (expected == actual)
            {
                Pass(name);
            }
            else
            {
                Fail(name, $"expected {expected} but got {actual}");
            }
        }

        static void AssertTrue(string name, bool condition)
        {
            if (condition)
            {
                Pass(name);
            }
            else
            {
                Fail(name, "condition was false");
            }
        }

        static void Pass(string name)
        {
            _passed++;
            Console.WriteLine($"  [PASS] {name}");
        }

        static void Fail(string name, string detail)
        {
            _failed++;
            Console.WriteLine($"  [FAIL] {name}: {detail}");
        }
    }
}
