using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public class AndroidTestRunner : ITestRunner
    {
        private ILogger _logger;
        private IAndroidDebugBridgeFactory _adbFactory;
        private Device _androidDevice;

        private string _packageName;
        private string _runnerClass;

        public AndroidTestRunner(ILogger logger, IAndroidDebugBridgeFactory adbFactory, Device device, string packageName, string runnerClass)
        {
            _logger = logger;
            _androidDevice = device;
            _adbFactory = adbFactory;
            _packageName = packageName;
            _runnerClass = runnerClass;
        }

        public TestResults RunTests()
        {
            var adb = _adbFactory.GetAndroidDebugBridge();
            var testOutputParser = new TestOutputParser();

            var testOutput = adb.StartInstrument(_androidDevice, _packageName, _runnerClass, (outPut) =>
            {
                testOutputParser.Append(outPut);
            });

            var results = testOutputParser.TestResults;
            _logger.LogMessage(testOutput);

            return results;
        }


    }
}
