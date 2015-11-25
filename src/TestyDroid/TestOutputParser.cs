using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TestyDroid
{
    public class TestOutputParser
    {
        private TestResults _TestResults;


        private TestResults _TestReportTestResults;

        private List<Func<string, string, bool>> _bundleResultParsers;
        private StringBuilder _builder;
        private IAndroidDebugBridgeFactory _adbFactory;
        private Device _device;

        public TestOutputParser(IAndroidDebugBridgeFactory adbFactory, Device device)
        {
            _adbFactory = adbFactory;
            _device = device;
            _TestResults = new TestResults();
            _TestReportTestResults = new TestResults();
            _bundleResultParsers = LoadParsers();
            //_testSuiteFailedTests = new List<TestResult>();
        }

        private List<Func<string, string, bool>> LoadParsers()
        {
            var parsers = new List<Func<string, string, bool>>();
            parsers.Add(HandleXamarinTestSuiteResult);
            parsers.Add(HandleTestyDroidXmlReportResult);
            return parsers;
        }

        public void Append(string output)
        {
            if (output == null)
            {
                return;
            }

            if (output.StartsWith("INSTRUMENTATION_RESULT"))
            {
                AppendResult(output);
                return;
            }
            if (output.StartsWith("INSTRUMENTATION_CODE"))
            {
                AppendCode(output);
            }
            else
            {
                var lastTest = this.TestResults.GetTests().LastOrDefault();
                if (lastTest != null)
                {
                    lastTest.StackTrace = string.Format("{0}{1}", lastTest.StackTrace, output);
                }
            }
        }

        private void AppendCode(string output)
        {
            var result = output.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            var code = result[1];
            InstrumentationCode = int.Parse(code.Trim());
        }

        private void AppendResult(string output)
        {
            //  var reader = new StringReader(output);
            int skipCount = "INSTRUMENTATION_RESULT:".Length;
            output = output.Substring(skipCount);

            // split on first = to get the key from bundle and value.
            int indexOfFirstEquals = output.IndexOf('=');

            string bundleKey = output.Substring(0, indexOfFirstEquals);
            string bundleValue = output.Substring(indexOfFirstEquals + 1).Trim();

            foreach (var bundleResultParser in _bundleResultParsers)
            {
                var handled = bundleResultParser(bundleKey, bundleValue);
                if (handled)
                {
                    break;
                }
            }

        }

        private bool HandleTestyDroidXmlReportResult(string key, string value)
        {
            // The result kind section can also contain test name.
            if (key.Trim() != "##TestyDroidTestsReport")
            {
                return false;
            }
           
            var reportPath = value;
            var adb = _adbFactory.GetAndroidDebugBridge();
            var fileContents = adb.ReadFileContents(_device, reportPath);

            XmlDocument report = new XmlDocument();
            report.LoadXml(fileContents);

            var testResults = report.GetElementsByTagName("TestResult");
            foreach (XmlElement testResult in testResults)
            {
                var testName = testResult.GetAttribute("Name");
                var testStatus = testResult.GetAttribute("Status");
                var testDuration = testResult.GetAttribute("Duration");
                var testMessage = testResult.GetAttribute("Message");
                var stackTrace = testResult.InnerText;

                TestResultKind resultKind;
                switch (testStatus)
                {
                    case "Failed":
                        resultKind = TestResultKind.Failure;
                        break;
                    case "Skipped":
                        resultKind = TestResultKind.Skipped;
                        break;
                    case "Inconclusive":
                        resultKind = TestResultKind.Inconclusive;
                        break;
                    case "Passed":
                        resultKind = TestResultKind.Passed;
                        break;
                    default:
                        throw new FormatException("Could not parse the result type in the test report: " + testStatus);
                }

                var test = new TestResult(testName, resultKind);
                test.Message = testMessage;
                test.StackTrace = stackTrace;
                test.Duration = TimeSpan.Parse(testDuration);

                this._TestReportTestResults.AddTest(test);

            }

            return true;

        }

        private bool HandleXamarinTestSuiteResult(string key, string value)
        {
            // The result kind section can also contain test name.

            string resultTestName = string.Empty;
            string resultKind = string.Empty;

            if (key.Contains(":"))
            {
                var resultKindSectionSplit = key.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                resultKind = resultKindSectionSplit[0].Trim();
                resultTestName = resultKindSectionSplit[1].Trim();
            }
            else
            {
                resultKind = key.Trim();
            }
          
            switch (resultKind)
            {
                case "failure":
                    var testResult = new TestResult(resultTestName, TestResultKind.Failure);
                    testResult.Message = value;
                    this.TestResults.AddTest(testResult);
                    break;

                case "passed":
                    AppendTestsFromCount(TestResultKind.Passed, value);
                    break;

                case "skipped":
                    AppendTestsFromCount(TestResultKind.Skipped, value);
                    break;

                case "inconclusive":
                    AppendTestsFromCount(TestResultKind.Inconclusive, value);
                    break;

                case "failed":
                    break;

                default:
                    return false;

            }

            return true;
        }

        private void AppendTestsFromCount(TestResultKind kind, string resultDetailSection)
        {
            int count;
            if (resultDetailSection.Length == 1)
            {
                if (int.TryParse(resultDetailSection, out count))
                {
                    for (int i = 0; i < count; i++)
                    {
                        var dummyTestResult = new TestResult(string.Empty, kind);
                        TestResults.AddTest(dummyTestResult);
                    }
                }
            }
        }

        public int? InstrumentationCode { get; set; }

        public TestResults GetResults()
        {
            // merge the xamarin results, into our detailed report results (if were present) keeping our detailed report results as priority (master).
            _TestReportTestResults.Merge(_TestResults);
            return _TestReportTestResults;
        }

        protected TestResults TestResults { get { return _TestResults; } }

    }

}
