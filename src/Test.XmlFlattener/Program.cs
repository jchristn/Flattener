namespace Test.XmlFlattener
{
    using System;
    using System.Collections.Specialized;
    using Flattener;

    class Program
    {
        private static int _passed = 0;
        private static int _failed = 0;

        static int Main(string[] args)
        {
            Console.WriteLine("XML Flattener Test Program");
            Console.WriteLine("==========================\n");

            // Positive cases
            SimpleXml_ProducesExpectedPairs();
            Attributes_UseAtNotation();
            NestedXml_UsesDotNotation();
            RepeatedElements_UseIndexNotation();
            IncludeNullItems_EmitsEmptyElements();

            // Negative cases
            InvalidXml_ReturnsEmptyCollection();
            EmptyString_ReturnsEmptyCollection();
            EmptyElements_ExcludedByDefault();
            NonExistentKey_ReturnsNull();

            Console.WriteLine();
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Results: {_passed} passed, {_failed} failed");

            return _failed == 0 ? 0 : 1;
        }

        // ---------- Positive test cases ----------

        static void SimpleXml_ProducesExpectedPairs()
        {
            string xml = @"
                <Person>
                    <Name>John Doe</Name>
                    <Age>30</Age>
                    <Active>true</Active>
                </Person>";
            NameValueCollection result = XmlFlattener.Flatten(xml, false);

            AssertEqual("SimpleXml count", 3, result.Count);
            AssertEqual("SimpleXml Name", "John Doe", result["Name"]);
            AssertEqual("SimpleXml Age", "30", result["Age"]);
            AssertEqual("SimpleXml Active", "true", result["Active"]);
        }

        static void Attributes_UseAtNotation()
        {
            string xml = @"
                <Employee id=""12345"" department=""IT"">
                    <Name>Jane Smith</Name>
                    <Position title=""Senior Developer"">Developer</Position>
                    <Salary currency=""USD"">85000</Salary>
                </Employee>";
            NameValueCollection result = XmlFlattener.Flatten(xml, false);

            AssertEqual("Attr Employee.@id", "12345", result["Employee.@id"]);
            AssertEqual("Attr Employee.@department", "IT", result["Employee.@department"]);
            AssertEqual("Attr Name", "Jane Smith", result["Name"]);
            AssertEqual("Attr Position.@title", "Senior Developer", result["Position.@title"]);
            AssertEqual("Attr Position value", "Developer", result["Position"]);
            AssertEqual("Attr Salary.@currency", "USD", result["Salary.@currency"]);
            AssertEqual("Attr Salary value", "85000", result["Salary"]);
        }

        static void NestedXml_UsesDotNotation()
        {
            string xml = @"
                <Company name=""Tech Solutions"">
                    <HeadOffice>
                        <Address>
                            <Street>123 Main St</Street>
                            <City>Anytown</City>
                        </Address>
                        <Phone>555-123-4567</Phone>
                    </HeadOffice>
                    <Founded>2005</Founded>
                </Company>";
            NameValueCollection result = XmlFlattener.Flatten(xml, false);

            AssertEqual("Nested Company.@name", "Tech Solutions", result["Company.@name"]);
            AssertEqual("Nested Street", "123 Main St", result["HeadOffice.Address.Street"]);
            AssertEqual("Nested City", "Anytown", result["HeadOffice.Address.City"]);
            AssertEqual("Nested Phone", "555-123-4567", result["HeadOffice.Phone"]);
            AssertEqual("Nested Founded", "2005", result["Founded"]);
        }

        static void RepeatedElements_UseIndexNotation()
        {
            string xml = @"
                <Department>
                    <Name>Engineering</Name>
                    <Employee>
                        <Name>Alice</Name>
                        <Skill>C#</Skill>
                        <Skill>JavaScript</Skill>
                        <Skill>SQL</Skill>
                    </Employee>
                    <Employee>
                        <Name>Bob</Name>
                        <Skill>Python</Skill>
                        <Skill>Java</Skill>
                    </Employee>
                </Department>";
            NameValueCollection result = XmlFlattener.Flatten(xml, false);

            AssertEqual("Repeated Name", "Engineering", result["Name"]);
            AssertEqual("Repeated Employee[0].Name", "Alice", result["Employee[0].Name"]);
            AssertEqual("Repeated Employee[0].Skill[0]", "C#", result["Employee[0].Skill[0]"]);
            AssertEqual("Repeated Employee[0].Skill[2]", "SQL", result["Employee[0].Skill[2]"]);
            AssertEqual("Repeated Employee[1].Name", "Bob", result["Employee[1].Name"]);
            AssertEqual("Repeated Employee[1].Skill[1]", "Java", result["Employee[1].Skill[1]"]);
        }

        static void IncludeNullItems_EmitsEmptyElements()
        {
            string xml = @"
                <TestCase>
                    <EmptyElement></EmptyElement>
                    <ValidItem>Content</ValidItem>
                </TestCase>";
            NameValueCollection result = XmlFlattener.Flatten(xml, true);

            AssertEqual("IncludeNulls ValidItem", "Content", result["ValidItem"]);
            AssertTrue("IncludeNulls EmptyElement present", ContainsKey(result, "EmptyElement"));
            AssertEqual("IncludeNulls EmptyElement value", "", result["EmptyElement"]);
        }

        // ---------- Negative test cases ----------

        static void InvalidXml_ReturnsEmptyCollection()
        {
            string xml = @"<InvalidXml><Unclosed>";
            NameValueCollection result = XmlFlattener.Flatten(xml, false);
            AssertEqual("Invalid XML count", 0, result.Count);
        }

        static void EmptyString_ReturnsEmptyCollection()
        {
            NameValueCollection result = XmlFlattener.Flatten("", false);
            AssertEqual("Empty string count", 0, result.Count);
        }

        static void EmptyElements_ExcludedByDefault()
        {
            string xml = @"
                <TestCase>
                    <EmptyElement></EmptyElement>
                    <ValidItem>Content</ValidItem>
                </TestCase>";
            NameValueCollection result = XmlFlattener.Flatten(xml, false);

            AssertEqual("EmptyExcluded count", 1, result.Count);
            AssertTrue("EmptyExcluded EmptyElement absent", !ContainsKey(result, "EmptyElement"));
            AssertEqual("EmptyExcluded ValidItem", "Content", result["ValidItem"]);
        }

        static void NonExistentKey_ReturnsNull()
        {
            string xml = @"<Root><A>1</A></Root>";
            NameValueCollection result = XmlFlattener.Flatten(xml, false);
            AssertTrue("NonExistent key returns null", result["Does.Not.Exist"] == null);
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
