namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// NUnit adapter over the shared Touchstone catalog. Each shared descriptor becomes a single
    /// NUnit test case, so the entire Flattener test suite runs under <c>dotnet test</c> without
    /// duplicating any test definitions.
    /// </summary>
    [TestFixture]
    public sealed class FlattenerNunitTests
    {
        /// <summary>
        /// One NUnit test case per non-skipped shared test case.
        /// </summary>
        private static IEnumerable Cases()
        {
            return new TouchstoneTestCaseSource(FlattenerSuites.All);
        }

        /// <summary>
        /// Execute a single shared test case.
        /// </summary>
        /// <param name="testCase">The Touchstone descriptor to run.</param>
        [Test]
        [TestCaseSource(nameof(Cases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
