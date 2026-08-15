namespace Test.Automated
{
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Cli;

    /// <summary>
    /// Touchstone CLI runner. Executes the shared Flattener test catalog and prints a colored
    /// tabular report. Exit code is 0 when every case passes and 1 otherwise.
    ///
    /// Usage:
    ///   dotnet run --project Test.Automated
    ///   dotnet run --project Test.Automated -- results.json   (also writes a JSON report)
    /// </summary>
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            string resultsPath = (args != null && args.Length > 0) ? args[0] : null;
            return await ConsoleRunner.RunAsync(FlattenerSuites.All, resultsPath: resultsPath);
        }
    }
}
