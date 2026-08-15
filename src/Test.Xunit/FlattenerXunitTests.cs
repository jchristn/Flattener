namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;
    using global::Xunit;

    /// <summary>
    /// xUnit adapter over the shared Touchstone catalog. Each shared descriptor becomes a single
    /// xUnit theory row, so the entire Flattener test suite runs under <c>dotnet test</c> without
    /// duplicating any test definitions.
    /// </summary>
    public sealed class FlattenerXunitTests
    {
        /// <summary>
        /// One theory row per non-skipped shared test case.
        /// </summary>
        public static TouchstoneTheoryData Cases
        {
            get { return new TouchstoneTheoryData(FlattenerSuites.All); }
        }

        /// <summary>
        /// Execute a single shared test case.
        /// </summary>
        /// <param name="testCase">The Touchstone descriptor to run.</param>
        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
