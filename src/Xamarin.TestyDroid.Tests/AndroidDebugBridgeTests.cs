using NUnit.Framework;
using System.Diagnostics;
using System;
using System.Threading;
using System.Linq;

namespace Xamarin.TestyDroid.Tests
{

    [TestFixture(Category ="Integration")]
    public class AndroidDebugBridgeTests
    {
        [Test]
        public async void Can_Get_Devices()
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
                    // sut
                    var adb = adbFactory.GetAndroidDebugBridge();
                    var devices = adb.GetDevices();

                    Assert.That(devices, Is.Not.Null);
                    Assert.That(devices.Length, Is.GreaterThanOrEqualTo(1));

                    var deviceFound = devices.Where(a => a.Port == consolePort).FirstOrDefault();
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
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            using (IEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {
                    // sut
                    var adb = adbFactory.GetAndroidDebugBridge();
                    var currentDir = Environment.CurrentDirectory;

                    var apkPath = System.IO.Path.Combine(currentDir, "..\\..\\..\\", TestConfig.PathToAndroidTestsApk);

                    var installed = adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);
                    Assert.That(installed, Is.EqualTo(true));


                });


            }


        }


        [Test]
        public async void Can_Install_Apk_From_Relative_Path()
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
                    // sut
                    var adb = adbFactory.GetAndroidDebugBridge();
                    var currentDir = Environment.CurrentDirectory;                   
                    var apkPath = System.IO.Path.Combine("..\\..\\..\\", TestConfig.PathToAndroidTestsApk);
                    var installed = adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);
                    Assert.That(installed, Is.EqualTo(true));


                });


            }


        }

    }
}
