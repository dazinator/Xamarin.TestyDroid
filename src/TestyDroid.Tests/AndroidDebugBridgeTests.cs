using NUnit.Framework;
using System.Diagnostics;
using System;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace TestyDroid.Tests
{

    public enum AndroidEmulatorTypes
    {
        AndroidSdk = 0,
        Microsoft = 1
    }

    [TestFixture(Category = "Integration")]
    public class AndroidDebugBridgeTests
    {
        [TestCase(AndroidEmulatorTypes.Microsoft)]
        [TestCase(AndroidEmulatorTypes.AndroidSdk)]        
        [Test]
        public async Task Can_Get_Devices(AndroidEmulatorTypes emulatorType)
        {
            var logger = new ConsoleLogger();
           

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;

            IEmulatorFactory emuFactory = GetEmulatorFactory(emulatorType, logger, adbFactory, consolePort);  //GetEmulatorFactory(emulatorType); 

            using (IEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {

                    Assert.False(t.IsFaulted, "Exception occurred starting emulator: " + t.Exception?.ToString());
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

        private IEmulatorFactory GetEmulatorFactory(AndroidEmulatorTypes emulatorType, ILogger logger, IAndroidDebugBridgeFactory adbFactory, int consolePort, bool noBootAnim = true, bool noWindow = false, SingleInstanceMode instanceMode = SingleInstanceMode.Abort)
        {
            Guid emuId = Guid.NewGuid();
            switch (emulatorType)
            {
                case AndroidEmulatorTypes.AndroidSdk:
                  
                    return new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, noBootAnim, noWindow, emuId, instanceMode);
                   // break;
                case AndroidEmulatorTypes.Microsoft:
                    return new MicrosoftAndroidEmulatorFactory(logger, TestConfig.PathToMicrosoftAndroidEmulatorExe, adbFactory, TestConfig.MicrosoftAvdProfileId, consolePort, noBootAnim, noWindow, emuId, instanceMode);
                   // break;
            }

            throw new  NotImplementedException();
        }

        [TestCase(AndroidEmulatorTypes.Microsoft)]
        [TestCase(AndroidEmulatorTypes.AndroidSdk)]
        [Test]
        public async Task Can_Install_Apk(AndroidEmulatorTypes emulatorType)
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = GetEmulatorFactory(emulatorType, logger, adbFactory, consolePort, true, false);

            // new 

            using (IAndroidEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {
                    Assert.False(t.IsFaulted, "Exception occurred starting emulator: " + t.Exception?.ToString());
                  
                    // sut
                    var adb = adbFactory.GetAndroidDebugBridge();
                    var currentDir = TestContext.CurrentContext.TestDirectory; // Environment.CurrentDirectory;

                    var apkPath = System.IO.Path.Combine(currentDir, "..\\..\\..\\", TestConfig.PathToAndroidTestsApk);

                    adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);


                });
            }
        }

        [TestCase(AndroidEmulatorTypes.Microsoft)]
        [TestCase(AndroidEmulatorTypes.AndroidSdk)]
        [Test]
        public async Task Can_Install_Apk_From_Relative_Path(AndroidEmulatorTypes emulatorType)
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;

            var emuFactory = GetEmulatorFactory(emulatorType, logger, adbFactory, consolePort, true, false);
            //var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            using (IAndroidEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {
                    Assert.False(t.IsFaulted, "Exception occurred starting emulator: " + t.Exception?.ToString());

                    // sut
                    var adb = adbFactory.GetAndroidDebugBridge();
                    var currentDir = Environment.CurrentDirectory;
                    var apkPath = System.IO.Path.Combine("..\\..\\..\\", TestConfig.PathToAndroidTestsApk);
                    adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication);


                });
            }
        }

        [TestCase(AndroidEmulatorTypes.Microsoft)]
        [TestCase(AndroidEmulatorTypes.AndroidSdk)]
        [Test]
        //[ExpectedException(typeof(Exception), ExpectedMessage = "Unable to install", MatchType = MessageMatch.Contains)]
        public async Task Cannot_Install_Non_Existing_APK(AndroidEmulatorTypes emulatorType)
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = GetEmulatorFactory(emulatorType, logger, adbFactory, consolePort, true, false);
           // var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            using (IAndroidEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {
                    Assert.False(t.IsFaulted, "Exception occurred starting emulator: " + t.Exception?.ToString());

                    // sut
                    var adb = adbFactory.GetAndroidDebugBridge();
                    var currentDir = Environment.CurrentDirectory;
                    var apkPath = System.IO.Path.Combine("..\\..\\..\\", "SOMEOTHER.APK");

                    Assert.Throws<Exception>(() => adb.Install(droidEmulator.Device, apkPath, AdbInstallFlags.ReplaceExistingApplication));
                    
                });
            }

        }

        [TestCase(AndroidEmulatorTypes.Microsoft)]
        [TestCase(AndroidEmulatorTypes.AndroidSdk)]
        [Test]
        // [ExpectedException(typeof(TimeoutException))]
        public async Task Cannot_Proceed_Past_Timeout(AndroidEmulatorTypes emulatorType)
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = GetEmulatorFactory(emulatorType, logger, adbFactory, consolePort, true, false);
           // var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            try
            {
                using (IEmulator droidEmulator = emuFactory.GetEmulator())
                {
                    TaskContinuationOptions y = new TaskContinuationOptions();

                    await droidEmulator.Start(TimeSpan.FromSeconds(5)).ContinueWith((t) =>
                    {                     
                        if (!t.IsFaulted)
                        {
                            Assert.Fail();
                        }

                        var exception = t.Exception.InnerExceptions[0];

                        Assert.Throws<TimeoutException>(() => { throw exception; });


                    });

                }
            }
            catch (AggregateException e)
            {
                var ex = e.InnerExceptions[0];
                throw ex;
            }
        }


        [Test]
        [TestCase(SingleInstanceMode.Abort, AndroidEmulatorTypes.Microsoft)]
        [TestCase(SingleInstanceMode.KillExisting, AndroidEmulatorTypes.Microsoft)]
        [TestCase(SingleInstanceMode.ReuseExisting, AndroidEmulatorTypes.Microsoft)]
        [TestCase(SingleInstanceMode.ReuseExistingThenKill, AndroidEmulatorTypes.Microsoft)]
        [TestCase(SingleInstanceMode.Abort, AndroidEmulatorTypes.AndroidSdk)]
        [TestCase(SingleInstanceMode.KillExisting, AndroidEmulatorTypes.AndroidSdk)]
        [TestCase(SingleInstanceMode.ReuseExisting, AndroidEmulatorTypes.AndroidSdk)]
        [TestCase(SingleInstanceMode.ReuseExistingThenKill, AndroidEmulatorTypes.AndroidSdk)]
        public async Task Can_Detect_Existing_Device(SingleInstanceMode singleInstanceMode, AndroidEmulatorTypes emulatorType)
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = GetEmulatorFactory(emulatorType, logger, adbFactory, consolePort, true, false);
           // var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, false, emuId);

            using (IEmulator droidEmulator = emuFactory.GetEmulator())
            {
                await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
                {

                    Assert.False(t.IsFaulted, "Exception occurred starting emulator: " + t.Exception?.ToString());

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
        public void Can_Restart()
        {
            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            var adb = adbFactory.GetAndroidDebugBridge();
            adb.RestartServer();

        }


    }


   
}
