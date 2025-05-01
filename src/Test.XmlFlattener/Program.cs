namespace Test.XmlFlattener
{
    using System;
    using System.Collections.Specialized;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Flattener;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("XML Flattener Test Program");
            Console.WriteLine("==========================\n");

            // Test 1: Simple XML
            string simpleXml = @"
                <Person>
                    <Name>John Doe</Name>
                    <Age>30</Age>
                    <Active>true</Active>
                </Person>";
            TestXmlFlattening("Simple XML (Exclude Null Items)", simpleXml, false);
            TestXmlFlattening("Simple XML (Include Null Items)", simpleXml, true);

            // Test 2: XML with attributes
            string xmlWithAttributes = @"
                <Employee id=""12345"" department=""IT"">
                    <Name>Jane Smith</Name>
                    <Position title=""Senior Developer"">Developer</Position>
                    <Salary currency=""USD"">85000</Salary>
                </Employee>";
            TestXmlFlattening("XML with Attributes (Exclude Null Items)", xmlWithAttributes, false);
            TestXmlFlattening("XML with Attributes (Include Null Items)", xmlWithAttributes, true);

            // Test 3: Nested XML
            string nestedXml = @"
                <Company name=""Tech Solutions"">
                    <HeadOffice>
                        <Address>
                            <Street>123 Main St</Street>
                            <City>Anytown</City>
                            <ZipCode>12345</ZipCode>
                        </Address>
                        <Phone>555-123-4567</Phone>
                    </HeadOffice>
                    <Founded>2005</Founded>
                </Company>";
            TestXmlFlattening("Nested XML (Exclude Null Items)", nestedXml, false);
            TestXmlFlattening("Nested XML (Include Null Items)", nestedXml, true);

            // Test 4: Array-like XML (repeated elements)
            string arrayXml = @"
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
            TestXmlFlattening("Array-like XML (Exclude Null Items)", arrayXml, false);
            TestXmlFlattening("Array-like XML (Include Null Items)", arrayXml, true);

            // Test 5: XML with empty and null values
            string emptyValuesXml = @"
                <TestCase>
                    <EmptyElement></EmptyElement>
                    <NullAttribute attr=""""/>
                    <EmptyArray>
                    </EmptyArray>
                    <MixedContent>
                        <ValidItem>Content</ValidItem>
                        <EmptyItem></EmptyItem>
                        <SelfClosing />
                    </MixedContent>
                </TestCase>";
            TestXmlFlattening("XML with Empty Values (Exclude Null Items)", emptyValuesXml, false);
            TestXmlFlattening("XML with Empty Values (Include Null Items)", emptyValuesXml, true);

            // Test 6: Invalid XML (to test error handling)
            string invalidXml = @"<InvalidXml><Unclosed>";
            TestXmlFlattening("Invalid XML", invalidXml, false);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void TestXmlFlattening(string testName, string xml, bool includeNullItems)
        {
            Console.WriteLine($"Test: {testName}");
            Console.WriteLine("Input XML:");
            Console.WriteLine(FormatXml(xml));
            Console.WriteLine($"IncludeNullItems: {includeNullItems}");
            Console.WriteLine();

            NameValueCollection flattened = XmlFlattener.Flatten(xml, includeNullItems);
            Console.WriteLine("Flattened Result:");

            if (flattened.Count == 0)
            {
                Console.WriteLine("  (Empty result - possibly due to invalid XML)");
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

        static string FormatXml(string xml)
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch
            {
                // If formatting fails, return the original XML
                return xml;
            }
        }
    }
}