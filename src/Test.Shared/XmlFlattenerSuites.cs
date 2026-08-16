namespace Test.Shared
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Threading.Tasks;
    using Flattener;
    using Touchstone.Core;

    /// <summary>
    /// Touchstone test suites exercising <see cref="XmlFlattener"/>.
    /// These descriptors are the single source of truth: they are consumed unchanged by the
    /// console runner (Test.Automated), the xUnit adapter (Test.Xunit), and the NUnit adapter (Test.Nunit).
    /// </summary>
    public static class XmlFlattenerSuites
    {
        /// <summary>
        /// All XML flattener suites.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    Positive(),
                    Attributes(),
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
            const string suite = "Xml.Positive";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "SimpleElements", "Leaf elements produce one pair each (root name dropped)", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<Person><Name>John Doe</Name><Age>30</Age><Active>true</Active></Person>");
                Check.Count(3, r, "SimpleElements");
                Check.ValueEqual(r, "Name", "John Doe", "SimpleElements");
                Check.ValueEqual(r, "Age", "30", "SimpleElements");
                Check.ValueEqual(r, "Active", "true", "SimpleElements");
            }));

            cases.Add(Case(suite, "NestedElements", "Nested elements use dot notation", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<Company name=""Tech Solutions"">
                        <HeadOffice>
                          <Address><Street>123 Main St</Street><City>Anytown</City></Address>
                          <Phone>555-123-4567</Phone>
                        </HeadOffice>
                        <Founded>2005</Founded>
                      </Company>");
                Check.ValueEqual(r, "Company.@name", "Tech Solutions", "NestedElements");
                Check.ValueEqual(r, "HeadOffice.Address.Street", "123 Main St", "NestedElements");
                Check.ValueEqual(r, "HeadOffice.Address.City", "Anytown", "NestedElements");
                Check.ValueEqual(r, "HeadOffice.Phone", "555-123-4567", "NestedElements");
                Check.ValueEqual(r, "Founded", "2005", "NestedElements");
            }));

            cases.Add(Case(suite, "RepeatedElements", "Repeated sibling elements use index notation", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<Department>
                        <Name>Engineering</Name>
                        <Employee><Name>Alice</Name><Skill>C#</Skill><Skill>JavaScript</Skill><Skill>SQL</Skill></Employee>
                        <Employee><Name>Bob</Name><Skill>Python</Skill><Skill>Java</Skill></Employee>
                      </Department>");
                Check.ValueEqual(r, "Name", "Engineering", "RepeatedElements");
                Check.ValueEqual(r, "Employee[0].Name", "Alice", "RepeatedElements");
                Check.ValueEqual(r, "Employee[0].Skill[0]", "C#", "RepeatedElements");
                Check.ValueEqual(r, "Employee[0].Skill[2]", "SQL", "RepeatedElements");
                Check.ValueEqual(r, "Employee[1].Name", "Bob", "RepeatedElements");
                Check.ValueEqual(r, "Employee[1].Skill[1]", "Java", "RepeatedElements");
            }));

            cases.Add(Case(suite, "DeepNesting", "Deeply nested single-value element", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<a><b><c><d>deep</d></c></b></a>");
                Check.Count(1, r, "DeepNesting");
                Check.ValueEqual(r, "b.c.d", "deep", "DeepNesting");
            }));

            cases.Add(Case(suite, "CDataContent", "CDATA sections surface as text", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r><v><![CDATA[hello world]]></v></r>");
                Check.Count(1, r, "CDataContent");
                Check.ValueEqual(r, "v", "hello world", "CDataContent");
            }));

            cases.Add(Case(suite, "EntityDecoding", "XML entities are decoded", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r><v>a &amp; b &lt; c</v></r>");
                Check.ValueEqual(r, "v", "a & b < c", "EntityDecoding");
            }));

            cases.Add(Case(suite, "NumericValuesRemainStrings", "XML values are untyped text, so leading zeros survive", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r><Zip>01234</Zip><Flag>true</Flag></r>");
                Check.ValueEqual(r, "Zip", "01234", "NumericValuesRemainStrings");
                Check.ValueEqual(r, "Flag", "true", "NumericValuesRemainStrings");
            }));

            return Build(suite, "XML Flattener - Positive", cases);
        }

        // ---------------------------------------------------------------------
        // Attribute handling
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor Attributes()
        {
            const string suite = "Xml.Attributes";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "RootAttributes", "Root attributes use element-name prefix with @", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<Employee id=""12345"" department=""IT""><Name>Jane Smith</Name></Employee>");
                Check.ValueEqual(r, "Employee.@id", "12345", "RootAttributes");
                Check.ValueEqual(r, "Employee.@department", "IT", "RootAttributes");
                Check.ValueEqual(r, "Name", "Jane Smith", "RootAttributes");
            }));

            cases.Add(Case(suite, "ElementWithAttributeAndValue", "Element carries both @attr and its text value", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<Employee><Position title=""Senior Developer"">Developer</Position></Employee>");
                Check.ValueEqual(r, "Position.@title", "Senior Developer", "ElementWithAttributeAndValue");
                Check.ValueEqual(r, "Position", "Developer", "ElementWithAttributeAndValue");
            }));

            cases.Add(Case(suite, "NestedAttributes", "Attributes on nested elements use their path", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<Company name=""X""><HeadOffice zone=""Z""><Phone>555</Phone></HeadOffice></Company>");
                Check.ValueEqual(r, "Company.@name", "X", "NestedAttributes");
                Check.ValueEqual(r, "HeadOffice.@zone", "Z", "NestedAttributes");
                Check.ValueEqual(r, "HeadOffice.Phone", "555", "NestedAttributes");
            }));

            cases.Add(Case(suite, "EmptyAttributeExcluded", "Empty attribute value excluded by default", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r a=""""><b>x</b></r>", false);
                Check.Count(1, r, "EmptyAttributeExcluded");
                Check.NoKey(r, "r.@a", "EmptyAttributeExcluded");
                Check.ValueEqual(r, "b", "x", "EmptyAttributeExcluded");
            }));

            cases.Add(Case(suite, "EmptyAttributeIncluded", "Empty attribute value included when enabled", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r a=""""><b>x</b></r>", true);
                Check.HasKey(r, "r.@a", "EmptyAttributeIncluded");
                Check.ValueEqual(r, "r.@a", "", "EmptyAttributeIncluded");
                Check.ValueEqual(r, "b", "x", "EmptyAttributeIncluded");
            }));

            cases.Add(Case(suite, "AttributeOnlyElement", "Element with only an attribute keeps the attribute", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r><item code=""A""></item></r>", false);
                Check.Count(1, r, "AttributeOnlyElement");
                Check.ValueEqual(r, "item.@code", "A", "AttributeOnlyElement");
            }));

            return Build(suite, "XML Flattener - Attributes", cases);
        }

        // ---------------------------------------------------------------------
        // includeNullItems behavior
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor IncludeNulls()
        {
            const string suite = "Xml.IncludeNulls";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "EmptyElementExcludedByDefault", "Empty element excluded when disabled", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<r><Empty></Empty><Valid>Content</Valid></r>", false);
                Check.Count(1, r, "EmptyElementExcludedByDefault");
                Check.NoKey(r, "Empty", "EmptyElementExcludedByDefault");
                Check.ValueEqual(r, "Valid", "Content", "EmptyElementExcludedByDefault");
            }));

            cases.Add(Case(suite, "EmptyElementIncludedWhenEnabled", "Empty element included when enabled", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<r><Empty></Empty><Valid>Content</Valid></r>", true);
                Check.Count(2, r, "EmptyElementIncludedWhenEnabled");
                Check.HasKey(r, "Empty", "EmptyElementIncludedWhenEnabled");
                Check.ValueEqual(r, "Empty", "", "EmptyElementIncludedWhenEnabled");
                Check.ValueEqual(r, "Valid", "Content", "EmptyElementIncludedWhenEnabled");
            }));

            cases.Add(Case(suite, "SelfClosingElement", "Self-closing element treated as empty leaf", () =>
            {
                NameValueCollection off = XmlFlattener.Flatten(@"<r><x/><y>v</y></r>", false);
                Check.Count(1, off, "SelfClosingElement off");
                Check.NoKey(off, "x", "SelfClosingElement off");

                NameValueCollection on = XmlFlattener.Flatten(@"<r><x/><y>v</y></r>", true);
                Check.Count(2, on, "SelfClosingElement on");
                Check.ValueEqual(on, "x", "", "SelfClosingElement on");
                Check.ValueEqual(on, "y", "v", "SelfClosingElement on");
            }));

            return Build(suite, "XML Flattener - Include Nulls", cases);
        }

        // ---------------------------------------------------------------------
        // Negative cases
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor Negative()
        {
            const string suite = "Xml.Negative";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "InvalidXml", "Unclosed tags yield empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<InvalidXml><Unclosed>");
                Check.Count(0, r, "InvalidXml");
            }));

            cases.Add(Case(suite, "EmptyString", "Empty string yields empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten("");
                Check.Count(0, r, "EmptyString");
            }));

            cases.Add(Case(suite, "WhitespaceOnly", "Whitespace-only input yields empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten("    ");
                Check.Count(0, r, "WhitespaceOnly");
            }));

            cases.Add(Case(suite, "GarbageText", "Non-XML text yields empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten("this is not xml");
                Check.Count(0, r, "GarbageText");
            }));

            cases.Add(Case(suite, "MismatchedTags", "Mismatched tags yield empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<a>content</b>");
                Check.Count(0, r, "MismatchedTags");
            }));

            cases.Add(Case(suite, "UnescapedAmpersand", "A bare ampersand is malformed and yields empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r><a>x & y</a></r>");
                Check.Count(0, r, "UnescapedAmpersand");
            }));

            cases.Add(Case(suite, "InvalidAttributeSyntax", "An attribute without a value is malformed and yields empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<r attr></r>");
                Check.Count(0, r, "InvalidAttributeSyntax");
            }));

            cases.Add(Case(suite, "MultipleRoots", "Multiple root elements yield empty collection", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<a/><b/>");
                Check.Count(0, r, "MultipleRoots");
            }));

            cases.Add(Case(suite, "NonExistentKey", "Indexer returns null for absent key", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<Root><A>1</A></Root>");
                Check.Null(r["Does.Not.Exist"], "NonExistentKey");
            }));

            return Build(suite, "XML Flattener - Negative", cases);
        }

        // ---------------------------------------------------------------------
        // Edge cases
        // ---------------------------------------------------------------------

        private static TestSuiteDescriptor EdgeCases()
        {
            const string suite = "Xml.EdgeCases";
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            cases.Add(Case(suite, "RootLeafWithValue", "Root leaf element is keyed by its own name", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<Root>value</Root>");
                Check.Count(1, r, "RootLeafWithValue");
                Check.ValueEqual(r, "Root", "value", "RootLeafWithValue");
            }));

            cases.Add(Case(suite, "RootLeafEmptyExcluded", "Empty root leaf excluded by default", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<Root></Root>", false);
                Check.Count(0, r, "RootLeafEmptyExcluded");
            }));

            cases.Add(Case(suite, "RootLeafEmptyIncluded", "Empty root leaf included when enabled", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<Root></Root>", true);
                Check.Count(1, r, "RootLeafEmptyIncluded");
                Check.ValueEqual(r, "Root", "", "RootLeafEmptyIncluded");
            }));

            cases.Add(Case(suite, "RootWithOnlyAttribute", "Root with only an attribute keeps the attribute", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<Root id=""5""/>", false);
                Check.Count(1, r, "RootWithOnlyAttribute");
                Check.ValueEqual(r, "Root.@id", "5", "RootWithOnlyAttribute");
            }));

            cases.Add(Case(suite, "XmlDeclarationIgnored", "XML declaration is ignored", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(
                    @"<?xml version=""1.0"" encoding=""utf-8""?><Root><A>1</A></Root>");
                Check.Count(1, r, "XmlDeclarationIgnored");
                Check.ValueEqual(r, "A", "1", "XmlDeclarationIgnored");
            }));

            cases.Add(Case(suite, "CommentsIgnored", "Comment nodes are ignored", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<Root><!-- note --><A>1</A></Root>");
                Check.Count(1, r, "CommentsIgnored");
                Check.ValueEqual(r, "A", "1", "CommentsIgnored");
            }));

            cases.Add(Case(suite, "MixedContentTextIgnored", "Text alongside child elements is ignored", () =>
            {
                NameValueCollection r = XmlFlattener.Flatten(@"<Root>text<A>1</A></Root>");
                Check.Count(1, r, "MixedContentTextIgnored");
                Check.ValueEqual(r, "A", "1", "MixedContentTextIgnored");
            }));

            return Build(suite, "XML Flattener - Edge Cases", cases);
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
