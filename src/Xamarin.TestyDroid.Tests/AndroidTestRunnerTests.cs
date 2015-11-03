using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid.Tests
{

    [TestFixture(Category = "Integration")]
    public class AndroidTestRunnerTests
    {
        [Test]
        public async void Can_Run_Tests()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            using (IEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {

                    if (t.IsFaulted)
                    {
                        throw t.Exception;
                    }

                    var adb = adbFactory.GetAndroidDebugBridge();

                    // install tests apk
                    var currentDir = Environment.CurrentDirectory;
                    var apkPath = System.IO.Path.Combine(currentDir, "..\\..\\..\\", TestConfig.PathToAndroidTestsApk);
                    var installed = adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);

                    // sut
                    var testRunner = new AndroidTestRunner(logger, adbFactory, droidEmulator.Device, TestConfig.AndroidTestsPackageName, TestConfig.AndroidTestsInstrumentationClassPath);
                    var testResults = testRunner.RunTests();

                    Assert.That(testResults, Is.Not.Null);
                    var failedTests = testResults.Tests.Where(a => a.Kind == TestResultKind.Failure);

                    foreach (var a in failedTests)
                    {
                        logger.LogMessage(string.Format("{0} - ", a.Name, a.Detail));
                    }

                });

            }

        }

        [Test]
        public async void Can_Report_Tests_To_Default_Reporter()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            using (IEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {

                    if (t.IsFaulted)
                    {
                        throw t.Exception;
                    }

                    var adb = adbFactory.GetAndroidDebugBridge();

                    // install tests apk
                    var currentDir = Environment.CurrentDirectory;
                    var apkPath = System.IO.Path.Combine(currentDir, "..\\..\\..\\", TestConfig.PathToAndroidTestsApk);
                    var installed = adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);

                    // sut
                    IProgressReporter progressReporter = new DefaultProgressReporter(Console.WriteLine);
                    progressReporter.ReportTestsStarted(TestConfig.AndroidTestsPackageName);
                    var testRunner = new AndroidTestRunner(logger, adbFactory, droidEmulator.Device, TestConfig.AndroidTestsPackageName, TestConfig.AndroidTestsInstrumentationClassPath);
                    var testResults = testRunner.RunTests();
                    Assert.That(testResults, Is.Not.Null);

                    progressReporter.ReportTests(testResults);
                    progressReporter.ReportTestsFinished(TestConfig.AndroidTestsPackageName);
                    return testResults;

                });

            }

        }

        [Test]
        public async void Can_Report_Tests_To_TeamCity_Reporter()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            using (IEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {

                    if (t.IsFaulted)
                    {
                        throw t.Exception;
                    }

                    var adb = adbFactory.GetAndroidDebugBridge();

                    // install tests apk
                    var currentDir = Environment.CurrentDirectory;
                    var apkPath = System.IO.Path.Combine(currentDir, "..\\..\\..\\", TestConfig.PathToAndroidTestsApk);
                    var installed = adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);

                    // sut
                    IProgressReporter progressReporter = new TeamCityProgressReporter(Console.WriteLine);
                    progressReporter.ReportTestsStarted(TestConfig.AndroidTestsPackageName);
                    var testRunner = new AndroidTestRunner(logger, adbFactory, droidEmulator.Device, TestConfig.AndroidTestsPackageName, TestConfig.AndroidTestsInstrumentationClassPath);
                    var testResults = testRunner.RunTests();
                    Assert.That(testResults, Is.Not.Null);

                    progressReporter.ReportTests(testResults);
                    progressReporter.ReportTestsFinished(TestConfig.AndroidTestsPackageName);
                    return testResults;




                });

            }

        }

    }
}
