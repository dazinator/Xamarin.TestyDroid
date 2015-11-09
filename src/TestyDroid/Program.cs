using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    class Program
    {
        public static int Main(string[] args)
        {

#if DEBUG
            if (Debugger.IsAttached)
            {
                args = new string[17];
                args[0] = "-e";
                args[1] = @"C:\Program Files (x86)\Android\android-sdk\tools\emulator.exe";
                args[2] = "-d";
                args[3] = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";
                args[4] = "-f";
                args[5] = @"..\..\..\TestyDroid.TestTests\bin\debug\TestyDroid.TestTests-Signed.apk";
                args[6] = "-i";
                args[7] = "AVD_GalaxyNexus_ToolsForApacheCordova";
                args[8] = "-n";
                args[9] = "TestyDroid.TestTests";
                args[10] = "-c";
                args[11] = "testydroid.testtests.TestInstrumentation";
                args[12] = "-w";
                args[13] = "120";
                args[14] = "-r";
                args[15] = "TeamCity";
                args[16] = "-v";
            }
#endif

            var parser = new CommandLine.Parser(with => with.IgnoreUnknownArguments = true);
            var options = new RunAndroidTestsOptions();
            if (parser.ParseArguments(args, options))
            {
                // parsing succeds
                // use appropriate emulator.
                var logger = new ConsoleLogger(options.Verbose);
                Guid emuId = Guid.NewGuid();

                IAndroidDebugBridgeFactory adbFactory = new AndroidDebugBridgeFactory(options.AdbExePath);
                IEmulatorFactory emulatorFactory;

                if (options.EmulatorType == "sdk")
                {
                    emulatorFactory = new AndroidSdkEmulatorFactory(logger, options.EmulatorExePath, adbFactory, options.ImageName, options.PortNumber, true, true, emuId, options.SingleInstanceMode);
                }
                else
                {
                    logger.LogMessage("Unsupported emulator type.");
                    return -1;
                }

                IEmulator droidEmulator = emulatorFactory.GetEmulator();
                IProgressReporter reporter = GetReporter(options.ReporterType);

                var testResults = StartEmulatorAndRunTests(reporter, adbFactory, logger, droidEmulator, options);
                return GetReturnCode(testResults);

            }
            else
            {
                Console.WriteLine("Failed to parse args, see usage.");
                Console.WriteLine(options.GetUsage());

                return -1;
            }

        }

        private static IProgressReporter GetReporter(ReporterType reporterType)
        {
            switch (reporterType)
            {
                case ReporterType.TeamCity:
                    return new TeamCityProgressReporter(Console.WriteLine);
                default:
                    return new DefaultProgressReporter(Console.WriteLine);
            }
        }

        private static int GetReturnCode(TestResults testResults)
        {
            // TODO: Use reporter to report on tests to STDOUT.
            if (testResults == null)
            {
                return -1; //error?
            }

            var tests = testResults.GetTests().ToList();
            if (tests.Any(a => a.Kind == TestResultKind.Failure))
            {
                return -2; // there are failed unit tests.
            }

            // no failed unit tests.
            return 0;
        }

        private static TestResults StartEmulatorAndRunTests(IProgressReporter progressReporter, IAndroidDebugBridgeFactory adbFactory, ILogger logger, IEmulator droidEmulator, RunAndroidTestsOptions options)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(options.EmulatorStartupWaitTimeInSeconds);
            using (droidEmulator)
            {
                progressReporter.ReportStatus("Waiting for emulator to boot.");
                droidEmulator.Start(timeout).Wait();

                var adb = adbFactory.GetAndroidDebugBridge();
               
                var apkPath = options.ApkPath;
                progressReporter.ReportStatus("Installing tests APK package.");
                adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);                 

                progressReporter.ReportTestsStarted(options.ApkPackageName);
                var testRunner = new AndroidTestRunner(logger, adbFactory, droidEmulator.Device, options.ApkPackageName, options.TestInstrumentationClassPath);
                var testResults = testRunner.RunTests();
                progressReporter.ReportTests(testResults);
                progressReporter.ReportTestsFinished(options.ApkPackageName);
                return testResults;
            }
        }
    }
}
