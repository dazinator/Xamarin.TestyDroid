using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public class DefaultProgressReporter : IProgressReporter
    {
        private Action<string> _Writer;

        public DefaultProgressReporter(Action<string> writeToOutput)
        {
            _Writer = writeToOutput;
        }

        public void ReportStatus(string message)
        {
            _Writer(message);
        }

        public void ReportTests(TestResults results)
        {
            if (results != null && results.Tests != null)
            {

                var passedTests = results.Tests.Where(t => t.Kind == TestResultKind.Passed).ToList();
                var skippedTests = results.Tests.Where(t => t.Kind == TestResultKind.Skipped).ToList();
                var inconclusiveTests = results.Tests.Where(t => t.Kind == TestResultKind.Inconclusive).ToList();
                var failedTests = results.Tests.Where(t => t.Kind == TestResultKind.Failure).ToList();

                _Writer(string.Format("Tests Summary", passedTests.Count));
                _Writer("---------------------");
                _Writer(string.Format("          Passed: {0}", passedTests.Count));
                _Writer(string.Format("         Skipped: {0}", skippedTests.Count));
                _Writer(string.Format("    Inconclusive: {0}", inconclusiveTests.Count));
                _Writer(string.Format("          Failed: {0}", failedTests.Count));

                if (failedTests.Any())
                {
                    _Writer(string.Format("Failed Tests", passedTests.Count));                  
                    foreach (var failedTest in failedTests)
                    {
                        _Writer("-----------------------------------------------");
                        _Writer(string.Format(" Name: {0}", failedTest.Name));
                        _Writer(string.Format(" Message: {0}", failedTest.Message));
                        _Writer(string.Format(" Output: {0}", failedTest.Detail));
                        _Writer("-----------------------------------------------");
                    }
                }               
            }
            // DO NOTHING.
        }

        public void ReportTestsFinished(string testSuiteName)
        {
            _Writer(string.Format(" Tests Finished: {0}", testSuiteName));
        }

        public void ReportTestsStarted(string testSuiteName)
        {
            _Writer(string.Format(" Starting Tests: {0}", testSuiteName));
        }
    }
}
