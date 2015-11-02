using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
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
}
