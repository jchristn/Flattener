namespace Test.JsonFlattener
{
    using System;
    using System.Collections.Specialized;
    using System.Text;
    using System.Text.Json;
    using Flattener;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("JSON Flattener Test Program");
            Console.WriteLine("==========================\n");

            // Test 1: Simple JSON object
            string simpleJson = @"{
                ""name"": ""John Doe"",
                ""age"": 30,
                ""isActive"": true
            }";
            TestJsonFlattening("Simple JSON Object (Exclude Null Items)", simpleJson, false);
            TestJsonFlattening("Simple JSON Object (Include Null Items)", simpleJson, true);

            // Test 2: Nested JSON object
            string nestedJson = @"{
                ""person"": {
                    ""name"": ""Jane Smith"",
                    ""age"": 28,
                    ""address"": {
                        ""street"": ""123 Main St"",
                        ""city"": ""Anytown"",
                        ""zipCode"": 12345
                    }
                },
                ""isEmployee"": true
            }";
            TestJsonFlattening("Nested JSON Object (Exclude Null Items)", nestedJson, false);
            TestJsonFlattening("Nested JSON Object (Include Null Items)", nestedJson, true);

            // Test 3: JSON with arrays
            string arrayJson = @"{
                ""employees"": [
                    {
                        ""name"": ""Alice"",
                        ""skills"": [""C#"", ""JavaScript"", ""SQL""]
                    },
                    {
                        ""name"": ""Bob"",
                        ""skills"": [""Python"", ""Java""]
                    }
                ],
                ""departmentId"": 42
            }";
            TestJsonFlattening("JSON with Arrays (Exclude Null Items)", arrayJson, false);
            TestJsonFlattening("JSON with Arrays (Include Null Items)", arrayJson, true);

            // Test 4: Edge cases with null and empty values
            string edgeCasesJson = @"{
                ""emptyObject"": {},
                ""emptyArray"": [],
                ""nullValue"": null,
                ""emptyString"": """",
                ""mixedArray"": [42, ""text"", true, null, {""nested"": ""value""}]
            }";
            TestJsonFlattening("Edge Cases (Exclude Null Items)", edgeCasesJson, false);
            TestJsonFlattening("Edge Cases (Include Null Items)", edgeCasesJson, true);

            // Test 5: Invalid JSON (to test error handling)
            string invalidJson = @"{""name"": ""Incomplete JSON";
            TestJsonFlattening("Invalid JSON", invalidJson, false);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void TestJsonFlattening(string testName, string json, bool includeNullItems)
        {
            Console.WriteLine($"Test: {testName}");
            Console.WriteLine("Input JSON:");
            Console.WriteLine(FormatJson(json));
            Console.WriteLine($"IncludeNullItems: {includeNullItems}");
            Console.WriteLine();

            NameValueCollection flattened = JsonFlattener.Flatten(json, includeNullItems);
            Console.WriteLine("Flattened Result:");

            if (flattened.Count == 0)
            {
                Console.WriteLine("  (Empty result - possibly due to invalid JSON)");
            }
            else
            {
                foreach (string key in flattened.AllKeys)
                {
                    string[] values = flattened.GetValues(key);
                    if (values != null)
                    {
                        foreach (string value in values)
                        {
                            Console.WriteLine($"  {key} = {(value ?? "null")}");
                        }
                    }
                }
            }

            Console.WriteLine("\n" + new string('-', 50) + "\n");
        }

        static string FormatJson(string json)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, options);
            }
            catch
            {
                // If formatting fails, return the original JSON
                return json;
            }
        }
    }
}