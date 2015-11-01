using NUnit.Framework;
using System.Diagnostics;
using System;
using System.Threading;
using System.Linq;

namespace Xamarin.TestyDroid.Tests
{

    [TestFixture]
    public class AndroidDebugBridgeTests
    {
        [Test]
        public async void Can_Get_Devices()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var factory = new ProcessFactory(logger, TestConfig.PathToAndroidEmulatorExe, TestConfig.PathToAdbExe);
            int consolePort = 5554;

            using (IEmulator droidEmulator = factory.GetAndroidSdkEmulator(TestConfig.AvdName, consolePort, true, false, emuId))
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {
                    // sut
                    var adb = factory.GetAndroidDebugBridge();
                    var devices = adb.GetDevices();

                    Assert.That(devices, Is.Not.Null);
                    Assert.That(devices.Length, Is.GreaterThanOrEqualTo(1));

                    var deviceFound = devices.Where(a => a.Name == "emulator" && a.Port == consolePort).FirstOrDefault();
                    Assert.That(deviceFound, Is.Not.Null);

                    foreach (var item in devices)
                    {
                        Console.WriteLine("Device Name: {0}, Status: {1}", item.Name, item.Status);
                    }

                });


            }


        }

        [Test]
        public async void Can_Install_Apk()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var factory = new ProcessFactory(logger, TestConfig.PathToAndroidEmulatorExe, TestConfig.PathToAdbExe);
            int consolePort = 5554;

            using (IEmulator droidEmulator = factory.GetAndroidSdkEmulator(TestConfig.AvdName, consolePort, true, false, emuId))
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {
                    // sut
                    var adb = factory.GetAndroidDebugBridge();
                    var currentDir = Environment.CurrentDirectory;

                    var apkPath = System.IO.Path.Combine(currentDir, "..\\..\\..\\", TestConfig.PathToAndroidTestsApk);

                    var installed = adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);
                    Assert.That(installed, Is.EqualTo(true));


                });


            }


        }

    }
}
