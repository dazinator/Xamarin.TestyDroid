using NUnit.Framework;
using System.Diagnostics;
using System;
using System.Threading;

namespace Xamarin.TestyDroid.Tests
{

    public class ConsoleLogger : ILogger
    {
        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    [TestFixture]
    public class EmulatorTests
    {

        [Test]
        public void Can_Create_Android_Emulator()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);
         
            IEmulator droidEmulator = emuFactory.GetEmulator();

        }

        [Test]
        public async void Can_Start_And_Stop_Android_Emulator()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            IEmulator droidEmulator = emuFactory.GetEmulator();
            await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
            {
                droidEmulator.Stop();
            });

        }


        //var task = new RunAndroidTests();
        //task.AdbExePath = PathToAdbExe;
        //    task.ApkPackageName = AndroidTestsPackageName;
        //    task.ApkPath = AndroidTestsPackageName;
        //    task.AvdName = AvdName;
        //    task.EmulatorExePath = PathToAndroidEmulatorExe;
        //    task.EmulatorStartupWaitTimeInSeconds = 120;
        //    task.TestInstrumentationClassPath = AndroidTestsInstrumentationClassPath;

        //    try
        //    {
        //        var result = task.Execute();
        //task.Output = AndroidTestsPackageName;
        //    }
        //    catch (System.Exception)
        //    {

        //        throw;
        //    }

    }
}
