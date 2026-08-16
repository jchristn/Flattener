namespace Test.Shared
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Threading.Tasks;
    using Flattener;
    using Touchstone.Core;

    /// <summary>
    /// Touchstone test suites exercising <see cref="JsonFlattener"/>.
    /// These descriptors are the single source of truth: they are consumed unchanged by the
    /// console runner (Test.Automated), the xUnit adapter (Test.Xunit), and the NUnit adapter (Test.Nunit).
    /// </summary>
    public static class JsonFlattenerSuites
    {
        /// <summary>
        /// All JSON flattener suites.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    Positive(),
                    IncludeNulls(),
                    Negative(),
                    EdgeCases()
                };
            }
        }

        // ---------------------------------------------------------------------
        // Positive cases
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor Positive()
        {
            const string suite = "Json.Positive";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "SimpleObject", "Flat object produces one pair per property", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(
                    @"{ ""name"": ""John Doe"", ""age"": 30, ""isActive"": true }");
                Check.Count(3, r, "SimpleObject");
                Check.ValueEqual(r, "name", "John Doe", "SimpleObject");
                Check.ValueEqual(r, "age", "30", "SimpleObject");
                Check.ValueEqual(r, "isActive", "True", "SimpleObject");
            }));

            cases.Add(Case(suite, "NestedObject", "Nested objects use dot notation", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(
                    @"{ ""person"": { ""name"": ""Jane Smith"",
                        ""address"": { ""street"": ""123 Main St"", ""zipCode"": 12345 } },
                        ""isEmployee"": true }");
                Check.ValueEqual(r, "person.name", "Jane Smith", "NestedObject");
                Check.ValueEqual(r, "person.address.street", "123 Main St", "NestedObject");
                Check.ValueEqual(r, "person.address.zipCode", "12345", "NestedObject");
                Check.ValueEqual(r, "isEmployee", "True", "NestedObject");
            }));

            cases.Add(Case(suite, "ObjectArray", "Arrays of objects use index notation", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(
                    @"{ ""employees"": [
                        { ""name"": ""Alice"", ""skills"": [""C#"", ""JavaScript"", ""SQL""] },
                        { ""name"": ""Bob"", ""skills"": [""Python"", ""Java""] } ],
                        ""departmentId"": 42 }");
                Check.ValueEqual(r, "employees[0].name", "Alice", "ObjectArray");
                Check.ValueEqual(r, "employees[0].skills[0]", "C#", "ObjectArray");
                Check.ValueEqual(r, "employees[0].skills[2]", "SQL", "ObjectArray");
                Check.ValueEqual(r, "employees[1].name", "Bob", "ObjectArray");
                Check.ValueEqual(r, "employees[1].skills[1]", "Java", "ObjectArray");
                Check.ValueEqual(r, "departmentId", "42", "ObjectArray");
            }));

            cases.Add(Case(suite, "ScalarArray", "Scalar arrays index each element", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""tags"": [""a"", ""b""] }");
                Check.Count(2, r, "ScalarArray");
                Check.ValueEqual(r, "tags[0]", "a", "ScalarArray");
                Check.ValueEqual(r, "tags[1]", "b", "ScalarArray");
            }));

            cases.Add(Case(suite, "Booleans", "Booleans surface as True/False", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""t"": true, ""f"": false }");
                Check.Count(2, r, "Booleans");
                Check.ValueEqual(r, "t", "True", "Booleans");
                Check.ValueEqual(r, "f", "False", "Booleans");
            }));

            cases.Add(Case(suite, "IntegerTypes", "Integer numbers round-trip as digits", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(
                    @"{ ""int"": 42, ""neg"": -7, ""zero"": 0 }");
                Check.Count(3, r, "IntegerTypes");
                Check.ValueEqual(r, "int", "42", "IntegerTypes");
                Check.ValueEqual(r, "neg", "-7", "IntegerTypes");
                Check.ValueEqual(r, "zero", "0", "IntegerTypes");
            }));

            cases.Add(Case(suite, "LargeInteger", "Int64 boundary value is preserved", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""big"": 9223372036854775807 }");
                Check.ValueEqual(r, "big", "9223372036854775807", "LargeInteger");
            }));

            cases.Add(Case(suite, "FloatingPoint", "Non-integer numbers surface as doubles", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""dbl"": 1.5 }");
                Check.ValueEqual(r, "dbl", "1.5", "FloatingPoint");
            }));

            cases.Add(Case(suite, "StringEscapes", "Escaped and Unicode characters are decoded", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(
                    @"{ ""quote"": ""a\""b"", ""unicode"": ""café"" }");
                Check.ValueEqual(r, "quote", "a\"b", "StringEscapes");
                Check.ValueEqual(r, "unicode", "café", "StringEscapes");
            }));

            cases.Add(Case(suite, "DeepNesting", "Deeply nested single-value object", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(
                    @"{ ""a"": { ""b"": { ""c"": { ""d"": ""deep"" } } } }");
                Check.Count(1, r, "DeepNesting");
                Check.ValueEqual(r, "a.b.c.d", "deep", "DeepNesting");
            }));

            cases.Add(Case(suite, "MixedTypeArray", "Mixed array keeps non-null elements, drops nulls by default", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""mixed"": [1, ""two"", true, null] }");
                Check.Count(3, r, "MixedTypeArray");
                Check.ValueEqual(r, "mixed[0]", "1", "MixedTypeArray");
                Check.ValueEqual(r, "mixed[1]", "two", "MixedTypeArray");
                Check.ValueEqual(r, "mixed[2]", "True", "MixedTypeArray");
                Check.NoKey(r, "mixed[3]", "MixedTypeArray");
            }));

            cases.Add(Case(suite, "NestedArrays", "Arrays nested in arrays chain index notation", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""matrix"": [[1, 2], [3, 4]] }");
                Check.Count(4, r, "NestedArrays");
                Check.ValueEqual(r, "matrix[0][0]", "1", "NestedArrays");
                Check.ValueEqual(r, "matrix[0][1]", "2", "NestedArrays");
                Check.ValueEqual(r, "matrix[1][0]", "3", "NestedArrays");
                Check.ValueEqual(r, "matrix[1][1]", "4", "NestedArrays");
            }));

            cases.Add(Case(suite, "EmptyStringRetained", "Empty string values are retained by default", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""empty"": """", ""present"": ""x"" }");
                Check.Count(2, r, "EmptyStringRetained");
                Check.ValueEqual(r, "empty", "", "EmptyStringRetained");
                Check.ValueEqual(r, "present", "x", "EmptyStringRetained");
            }));

            cases.Add(Case(suite, "DuplicateKeys", "Duplicate property names accumulate as multiple values under one key", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""a"": 1, ""a"": 2 }");
                Check.Count(1, r, "DuplicateKeys");
                Check.ValueCount(r, "a", 2, "DuplicateKeys");
                Check.HasValue(r, "a", "1", "DuplicateKeys");
                Check.HasValue(r, "a", "2", "DuplicateKeys");
                // Indexer joins multiple values with commas.
                Check.ValueEqual(r, "a", "1,2", "DuplicateKeys");
            }));

            cases.Add(Case(suite, "WholeNumberDouble", "A number with a fractional zero collapses to its integer text", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""n"": 2.0 }");
                Check.ValueEqual(r, "n", "2", "WholeNumberDouble");
            }));

            cases.Add(Case(suite, "ScientificNotation", "Exponential numbers surface as their expanded double text", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""n"": 1.5e3 }");
                Check.ValueEqual(r, "n", "1500", "ScientificNotation");
            }));

            cases.Add(Case(suite, "ControlCharEscapes", "Escaped control characters are decoded to their literal characters", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""nl"": ""a\nb"", ""tab"": ""x\ty"" }");
                Check.ValueEqual(r, "nl", "a\nb", "ControlCharEscapes");
                Check.ValueEqual(r, "tab", "x\ty", "ControlCharEscapes");
            }));

            return Build(suite, "JSON Flattener - Positive", cases);
        }

        // ---------------------------------------------------------------------
        // includeNullItems behavior
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor IncludeNulls()
        {
            const string suite = "Json.IncludeNulls";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "EmptyContainersEmitted", "Empty object/array/null emit markers when enabled", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(
                    @"{ ""emptyObject"": {}, ""emptyArray"": [], ""nullValue"": null, ""emptyString"": """" }",
                    true);
                Check.Count(4, r, "EmptyContainersEmitted");
                Check.ValueEqual(r, "emptyObject", "{}", "EmptyContainersEmitted");
                Check.ValueEqual(r, "emptyArray", "[]", "EmptyContainersEmitted");
                Check.ValueEqual(r, "emptyString", "", "EmptyContainersEmitted");
                Check.HasKey(r, "nullValue", "EmptyContainersEmitted");
                Check.Null(r["nullValue"], "EmptyContainersEmitted nullValue");
            }));

            cases.Add(Case(suite, "NullIncludedWhenEnabled", "Null value key appears when enabled", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""a"": null }", true);
                Check.Count(1, r, "NullIncludedWhenEnabled");
                Check.HasKey(r, "a", "NullIncludedWhenEnabled");
                Check.Null(r["a"], "NullIncludedWhenEnabled a");
            }));

            cases.Add(Case(suite, "EmptyObjectMarkerDefaultOff", "Empty object emits nothing when disabled", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""o"": {} }", false);
                Check.Count(0, r, "EmptyObjectMarkerDefaultOff");
            }));

            cases.Add(Case(suite, "NestedEmptyObjectMarker", "Nested empty object emits marker under its path", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""outer"": { ""inner"": {} } }", true);
                Check.Count(1, r, "NestedEmptyObjectMarker");
                Check.ValueEqual(r, "outer.inner", "{}", "NestedEmptyObjectMarker");
            }));

            cases.Add(Case(suite, "NullArrayElementIncluded", "Null array elements keep their indexed key when enabled", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""a"": [1, null, 3] }", true);
                Check.Count(3, r, "NullArrayElementIncluded");
                Check.ValueEqual(r, "a[0]", "1", "NullArrayElementIncluded");
                Check.HasKey(r, "a[1]", "NullArrayElementIncluded");
                Check.Null(r["a[1]"], "NullArrayElementIncluded a[1]");
                Check.ValueEqual(r, "a[2]", "3", "NullArrayElementIncluded");
            }));

            return Build(suite, "JSON Flattener - Include Nulls", cases);
        }

        // ---------------------------------------------------------------------
        // Negative cases
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor Negative()
        {
            const string suite = "Json.Negative";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "InvalidJson", "Malformed JSON yields empty collection", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""name"": ""Incomplete JSON");
                Check.Count(0, r, "InvalidJson");
            }));

            cases.Add(Case(suite, "EmptyString", "Empty string yields empty collection", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("");
                Check.Count(0, r, "EmptyString");
            }));

            cases.Add(Case(suite, "WhitespaceOnly", "Whitespace-only input yields empty collection", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("    ");
                Check.Count(0, r, "WhitespaceOnly");
            }));

            cases.Add(Case(suite, "GarbageText", "Non-JSON text yields empty collection", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("this is not json");
                Check.Count(0, r, "GarbageText");
            }));

            cases.Add(Case(suite, "TrailingComma", "Trailing comma is rejected by default", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""a"": 1, }");
                Check.Count(0, r, "TrailingComma");
            }));

            cases.Add(Case(suite, "CommentsRejected", "JSON comments are rejected by default", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""a"": 1 /* comment */ }");
                Check.Count(0, r, "CommentsRejected");
            }));

            cases.Add(Case(suite, "UnquotedKey", "Unquoted property names are rejected", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ a: 1 }");
                Check.Count(0, r, "UnquotedKey");
            }));

            cases.Add(Case(suite, "SingleQuotedString", "Single-quoted strings are rejected", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""a"": 'x' }");
                Check.Count(0, r, "SingleQuotedString");
            }));

            cases.Add(Case(suite, "NullExcludedByDefault", "Null values excluded when disabled", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""present"": ""yes"", ""missing"": null }");
                Check.Count(1, r, "NullExcludedByDefault");
                Check.NoKey(r, "missing", "NullExcludedByDefault");
                Check.ValueEqual(r, "present", "yes", "NullExcludedByDefault");
            }));

            cases.Add(Case(suite, "NonExistentKey", "Indexer returns null for absent key", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"{ ""a"": 1 }");
                Check.Null(r["does.not.exist"], "NonExistentKey");
            }));

            return Build(suite, "JSON Flattener - Negative", cases);
        }

        // ---------------------------------------------------------------------
        // Edge cases
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor EdgeCases()
        {
            const string suite = "Json.EdgeCases";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "RootArray", "Root-level array uses bare index keys", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"[10, 20, 30]");
                Check.Count(3, r, "RootArray");
                Check.ValueEqual(r, "[0]", "10", "RootArray");
                Check.ValueEqual(r, "[1]", "20", "RootArray");
                Check.ValueEqual(r, "[2]", "30", "RootArray");
            }));

            cases.Add(Case(suite, "RootArrayOfObjects", "Root array of objects prefixes with index", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"[{ ""id"": 1 }, { ""id"": 2 }]");
                Check.Count(2, r, "RootArrayOfObjects");
                Check.ValueEqual(r, "[0].id", "1", "RootArrayOfObjects");
                Check.ValueEqual(r, "[1].id", "2", "RootArrayOfObjects");
            }));

            cases.Add(Case(suite, "RootScalarNumber", "Root scalar number keyed by empty string", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("42");
                Check.Count(1, r, "RootScalarNumber");
                Check.ValueEqual(r, "", "42", "RootScalarNumber");
            }));

            cases.Add(Case(suite, "RootScalarString", "Root scalar string keyed by empty string", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten(@"""hello""");
                Check.Count(1, r, "RootScalarString");
                Check.ValueEqual(r, "", "hello", "RootScalarString");
            }));

            cases.Add(Case(suite, "RootBoolean", "Root boolean keyed by empty string", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("true");
                Check.Count(1, r, "RootBoolean");
                Check.ValueEqual(r, "", "True", "RootBoolean");
            }));

            cases.Add(Case(suite, "RootNullExcluded", "Root null excluded when disabled", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("null", false);
                Check.Count(0, r, "RootNullExcluded");
            }));

            cases.Add(Case(suite, "RootNullIncluded", "Root null included when enabled", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("null", true);
                Check.Count(1, r, "RootNullIncluded");
                Check.HasKey(r, "", "RootNullIncluded");
                Check.Null(r[""], "RootNullIncluded");
            }));

            cases.Add(Case(suite, "RootEmptyObject", "Root empty object yields empty collection by default", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("{}", false);
                Check.Count(0, r, "RootEmptyObject");
            }));

            cases.Add(Case(suite, "RootEmptyArray", "Root empty array yields empty collection by default", () =>
            {
                NameValueCollection r = JsonFlattener.Flatten("[]", false);
                Check.Count(0, r, "RootEmptyArray");
            }));

            return Build(suite, "JSON Flattener - Edge Cases", cases);
        }

        // ---------------------------------------------------------------------
        // Builders
        // ---------------------------------------------------------------------

        private static TestCaseDescriptor Case(string suiteId, string caseId, string displayName, System.Action body)
        {
            return new TestCaseDescriptor(
                suiteId: suiteId,
                caseId: caseId,
                displayName: displayName,
                executeAsync: _ =>
                {
                    body();
                    return Task.CompletedTask;
                });
        }

        private static TestSuiteDescriptor Build(string suiteId, string displayName, List<TestCaseDescriptor> cases)
        {
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: displayName,
                cases: cases);
        }
    }
}
