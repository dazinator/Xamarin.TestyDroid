using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    public class TeamCityProgressReporter : IProgressReporter
    {
        private Action<string> _Writer;
        private int testCounter = 1;

        public TeamCityProgressReporter(Action<string> writeToOutput)
        {
            _Writer = writeToOutput;
        }

        public void ReportStatus(string message)
        {
            string teamCityControlMessage = string.Format("##teamcity[progressMessage '{0}']", message.EscapeTeamCitySpecialCharacters());
            _Writer(teamCityControlMessage);
        }

        public void ReportTests(TestResults results)
        {
            if (results != null)
            {
                var tests = results.GetTests().ToList();
                // unfortunately we cant capture names or details of "non failed" tests at present, so need to generate names for all non failed tests!                
                foreach (var test in tests)
                {
                    if (test.Kind == TestResultKind.Skipped)
                    {
                        ReportTestIgnored(test);
                        continue;
                    }

                    ReportTestStarted(test);

                    if (test.Kind == TestResultKind.Failure)
                    {
                        ReportTestFailed(test);
                    }

                    ReportTestFinished(test);
                    testCounter = testCounter + 1;
                }
            }
        }

        private void ReportTestIgnored(TestResult test)
        {
            var name = EnsureTestName(test);
            string testIgnoredMessage = string.Format("##teamcity[testIgnored name='{0}' message='{1}']", name, "Ignored.");
            _Writer(testIgnoredMessage);
        }

        private string EnsureTestName(TestResult test)
        {
            string name = test.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Test" + testCounter;
            }
            return name.EscapeTeamCitySpecialCharacters();
        }

        private void ReportTestFinished(TestResult failedTest)
        {
            var name = EnsureTestName(failedTest);
            string testFinishedMessage = string.Format("##teamcity[testFinished name='{0}']", name);
            _Writer(testFinishedMessage);
        }

        private void ReportTestFailed(TestResult failedTest)
        {
            var name = EnsureTestName(failedTest);
            string testFailedMessage = string.Format("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", name, failedTest.Message.EscapeTeamCitySpecialCharacters(), failedTest.StackTrace.EscapeTeamCitySpecialCharacters());
            _Writer(testFailedMessage);
        }

        private void ReportTestStarted(TestResult failedTest)
        {
            var name = EnsureTestName(failedTest);
            string testStartedMessage = string.Format("##teamcity[testStarted name='{0}']", name);
            _Writer(testStartedMessage);
        }

        public void ReportTestsFinished(string testSuiteName)
        {
            string teamCityControlMessage = string.Format("##teamcity[testSuiteFinished name='{0}']", testSuiteName.EscapeTeamCitySpecialCharacters());
            _Writer(teamCityControlMessage);
        }

        public void ReportTestsStarted(string testSuiteName)
        {
            string teamCityControlMessage = string.Format("##teamcity[testSuiteStarted name='{0}']", testSuiteName.EscapeTeamCitySpecialCharacters());
            _Writer(teamCityControlMessage);
        }
    }
}
