using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
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
            if (results != null && results.Tests != null)
            {
                // unfortunately we cant capture names or details of "non failed" tests at present, so need to generate names for all non failed tests!                
                foreach (var test in results.Tests)
                {
                    if (string.IsNullOrWhiteSpace(test.Name))
                    {
                        test.Name = "Test: " + testCounter;
                    }
                    testCounter = testCounter + 1;

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

                    if (test.Kind != TestResultKind.Inconclusive)
                    {
                        ReportTestFinished(test);
                    }
                }
            }
        }

        private void ReportTestIgnored(TestResult test)
        {
            string testIgnoredMessage = string.Format("##teamcity[testIgnored name='{0}' message='{1}']", test.Name.EscapeTeamCitySpecialCharacters(), "Ignored.");
            _Writer(testIgnoredMessage);
        }

        private void ReportTestFinished(TestResult failedTest)
        {
            string testFinishedMessage = string.Format("##teamcity[testFinished name='{0}']", failedTest.Name.EscapeTeamCitySpecialCharacters());
            _Writer(testFinishedMessage);
        }

        private void ReportTestFailed(TestResult failedTest)
        {
            string testFailedMessage = string.Format("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", failedTest.Name.EscapeTeamCitySpecialCharacters(), failedTest.Message.EscapeTeamCitySpecialCharacters(), failedTest.Detail.EscapeTeamCitySpecialCharacters());
            _Writer(testFailedMessage);
        }

        private void ReportTestStarted(TestResult failedTest)
        {
            string testStartedMessage = string.Format("##teamcity[testStarted name='{0}']", failedTest.Name.EscapeTeamCitySpecialCharacters());
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
