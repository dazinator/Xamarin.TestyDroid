using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    public class TestResults
    {
        public TestResults()
        {
            Tests = new List<TestResult>();
        }

        public void AddTest(TestResult test)
        {
            Tests.Add(test);
        }

        public int GetCount(TestResultKind kind)
        {
            return Tests.Count((t) => t.Kind == kind);
        }

        protected List<TestResult> Tests { get; set; }

        public IEnumerable<TestResult> GetTests()
        {
            return Tests.ToArray();
        }

        /// <summary>
        /// Merges the specified results into this result set.
        /// </summary>
        /// <param name="results"></param>
        public void Merge(TestResults results)
        {           
            var testEqualityComparer = new TestEqualityComparer();
            var unionTests = this.Tests.Union(results.Tests, testEqualityComparer).ToArray();
            this.Tests.Clear();
            this.Tests.AddRange(unionTests);

            // Now there might be dummy tests for particular result kinds. We should remove them if we have more detailed results for those result kinds.
            var dummyTests = this.Tests.Where(a => string.IsNullOrWhiteSpace(a.Name)).ToList();
            if (dummyTests.Any())
            {
                var dummyKinds = dummyTests.GroupBy(a => a.Kind).Distinct();
                var dummyKindsToRemove = new List<TestResult>();

                foreach (var item in dummyKinds)
                {
                    if (this.Tests.Any(t => !string.IsNullOrWhiteSpace(t.Name) && t.Kind == item.Key))
                    {
                        dummyKindsToRemove.AddRange(item.ToArray());
                    }
                }

                if (dummyKindsToRemove.Any())
                {
                    foreach (var dummy in dummyKindsToRemove)
                    {
                        this.Tests.Remove(dummy);
                    }
                }
            }          

        }

        //public static TestResults Union(TestResults testResults, TestResults otherTestResults)
        //{
        //    TestResults newTestResults = new TestResults();
        //    var testEqualityComparer = new TestEqualityComparer();
        //    var unionTests = testResults.Tests.Union(otherTestResults.Tests, testEqualityComparer);
        //    newTestResults.Tests.AddRange(unionTests);            
        //    return newTestResults;
        //}
    }

    public class TestEqualityComparer : IEqualityComparer<TestResult>
    {
        public bool Equals(TestResult x, TestResult y)
        {

            if (x.Kind != y.Kind)
            {
                return false;
            }

            if (x.Name == y.Name)
            {
                return true;
            }

            // we can assume that tests without a name are dummy generated and are therefore equal to one with a name of the same kind.
            if (string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(y.Name))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(y.Name) && !string.IsNullOrWhiteSpace(x.Name))
            {
                return true;
            }

            return false;

        }

        public int GetHashCode(TestResult obj)
        {
            return obj.Name.GetHashCode() * 17 + obj.Kind.GetHashCode();
        }
    }
}
