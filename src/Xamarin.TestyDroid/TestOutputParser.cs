using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public class TestOutputParser
    {
        private TestResults _TestResults;

        public TestOutputParser()
        {
            _TestResults = new TestResults();
        }

        private StringBuilder _builder;

        private bool isReadingFailure;


        public void Append(string output)
        {
            if(output == null)
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
                var lastTest = this.TestResults.Tests.LastOrDefault();
                if (lastTest != null)
                {
                    lastTest.Detail = string.Format("{0}{1}", lastTest.Detail, output);
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

            // split on first =
            int indexOfFirstEquals = output.IndexOf('=');

            string resultKindSection = output.Substring(0, indexOfFirstEquals);
            string resultDetailSection = output.Substring(indexOfFirstEquals + 1).Trim();

            // The result kind section can also contain test name.
            string resultTestName = string.Empty;
            string resultKind = string.Empty;

            if (resultKindSection.Contains(":"))
            {
                var resultKindSectionSplit = resultKindSection.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                resultKind = resultKindSectionSplit[0].Trim();
                resultTestName = resultKindSectionSplit[1].Trim();
            }
            else
            {
                resultKind = resultKindSection.Trim();
            }

            switch (resultKind)
            {
                case "failure":
                    TestResults.AppendFailedTest(resultTestName, resultDetailSection);
                    break;

                case "passed":
                    AppendTestsFromCount(TestResultKind.Passed, resultDetailSection);
                    break;

                case "skipped":
                    AppendTestsFromCount(TestResultKind.Skipped, resultDetailSection);
                    break;

                case "inconclusive":
                    AppendTestsFromCount(TestResultKind.Inconclusive, resultDetailSection);
                    break;

                case "failed":
                    break;             
              
            }

        }

        private void AppendTestsFromCount(TestResultKind kind, string resultDetailSection)
        {
            int count;
            if(resultDetailSection.Length == 1)
            {
                if(int.TryParse(resultDetailSection, out count))
                {
                    for (int i = 0; i < count; i++)
                    {
                        TestResults.AppendTest(kind, string.Empty, string.Empty);
                    }
                }
            }           
        }

        public int? InstrumentationCode { get; set; }

        public TestResults TestResults { get { return _TestResults; } }

    }

    public class TestResults
    {
        public TestResults()
        {
            Tests = new List<TestResult>();
        }

        public void AppendFailedTest(string resultTestName, string resultDetailSection)
        {
            var testResult = new TestResult();
            testResult.Kind = TestResultKind.Failure;
            testResult.Name = resultTestName;
            testResult.Detail = resultDetailSection;
            this.Tests.Add(testResult);
        }

        public void AppendTest(TestResultKind kind, string testName, string details)
        {
            var testResult = new TestResult();
            testResult.Kind = kind;
            testResult.Name = testName;
            testResult.Detail = details;
            this.Tests.Add(testResult);
        }

        public List<TestResult> Tests { get; set; }
    }

    public class TestResult
    {
        public string Name { get; set; }
        public string Detail { get; set; }
        public TestResultKind Kind { get; set; }
    }

    public enum TestResultKind
    {
        Failure,
        Passed,
        Skipped,
        Inconclusive
    }

}
