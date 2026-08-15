namespace Test.Shared
{
    using System.Collections.Generic;
    using System.Linq;
    using Touchstone.Core;

    /// <summary>
    /// Aggregate entry point exposing every Flattener test suite.
    /// Runners (console CLI, xUnit, NUnit) reference <see cref="All"/> so that the full test
    /// catalog stays defined in exactly one place.
    /// </summary>
    public static class FlattenerSuites
    {
        /// <summary>
        /// Every Touchstone suite covering the Flattener library.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return JsonFlattenerSuites.All
                    .Concat(XmlFlattenerSuites.All)
                    .ToList();
            }
        }
    }
}
