using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public interface IProgressReporter
    {
        void ReportStatus(string message);
        void ReportTestsStarted(string testSuiteName);
        void ReportTestsFinished(string testSuiteName);
        void ReportTests(TestResults results);
        
    }
}
