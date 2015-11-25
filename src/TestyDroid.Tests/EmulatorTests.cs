using NUnit.Framework;
using System.Diagnostics;
using System;
using System.Threading;

namespace TestyDroid.Tests
{
    [TestFixture(Category = "Integration")]
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

        [Test]
        public async void Can_Start_And_Stop_Android_Emulator_With_No_Window()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, true, emuId);

            IEmulator droidEmulator = emuFactory.GetEmulator();
            await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
            {
                droidEmulator.Stop();
            });

        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public async void Cannot_Start_Emulator_If_Existing_Device_Using_Port_And_Single_Instance_Mode_Abort_Specified()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, true, emuId, SingleInstanceMode.Abort);

            IEmulator droidEmulator = emuFactory.GetEmulator();
            Exception e = null;
                        
            var handles = new AutoResetEvent[] { new AutoResetEvent(false), new AutoResetEvent(false) };
            
            await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith(async (t) =>
            {
                // now try and start another one on same port.
                IEmulator secondEmulator = emuFactory.GetEmulator();
                await secondEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((a) =>
                {
                    if (a.IsFaulted)
                    {
                        e = a.Exception.InnerExceptions[0];
                    }
                  
                    secondEmulator.Stop();
                    handles[0].Set();

                });
                droidEmulator.Stop();
                handles[1].Set();
            });

            WaitHandle.WaitAll(handles, new TimeSpan(0, 2, 0));          
            if (e != null)
            {
                var rootEx = e.GetBaseException();
                throw rootEx;
            }

        }

        [Test]
        public async void Can_Start_Emulator_If_Existing_Device_Using_Port_Then_It_Is_Killed_When_Single_Instance_Mode_KillExisting_Specified()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, true, emuId, SingleInstanceMode.KillExisting);

            IEmulator droidEmulator = emuFactory.GetEmulator();
            await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
            {
                // now try and start another one on same port.
                IEmulator secondEmulator = emuFactory.GetEmulator();
                secondEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((a) =>
                {
                    // now try and start another one on same port.
                    secondEmulator.Stop();
                });

                droidEmulator.Stop();
            });

        }

        [Test]
        public async void Can_Start_Emulator_If_Existing_Device_Using_Port_Then_It_Is_Reused_When_Single_Instance_Mode_ReuseExisting_Specified()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, true, emuId, SingleInstanceMode.ReuseExisting);

            IEmulator droidEmulator = emuFactory.GetEmulator();
            await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
            {
                // now try and start another one on same port.
                Guid secondEmuId = Guid.NewGuid();
                var secondEmuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, true, secondEmuId, SingleInstanceMode.ReuseExisting);

                IEmulator secondEmulator = secondEmuFactory.GetEmulator();
                secondEmulator.Start(TestConfig.EmulatorStartupTimeout).Wait();

                // now try and start another one on same port.
                AndroidDevice device = (AndroidDevice)secondEmulator.Device;
                var adb = adbFactory.GetAndroidDebugBridge();

                // this second device should actually re-use the first device so should have device 1 emu id, not device 2.
                var secondId = device.QueryId(adb);
                Assert.That(Guid.Parse(secondId) != secondEmuId);

                secondEmulator.Stop();
                droidEmulator.Stop();
            });

        }


        [Test]
        public async void Can_Start_Emulator_If_Existing_Device_Using_Port_Then_It_Is_Reused_And_Killed_Afterwards_When_Single_Instance_Mode_ReuseExistingThenKill_Specified()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            var adbFactory = new AndroidDebugBridgeFactory(TestConfig.PathToAdbExe);
            int consolePort = 5554;
            var emuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, true, emuId, SingleInstanceMode.ReuseExistingThenKill);

            IEmulator droidEmulator = emuFactory.GetEmulator();
            await droidEmulator.Start(TestConfig.EmulatorStartupTimeout).ContinueWith((t) =>
            {

                var firstDevice = (AndroidDevice)droidEmulator.Device;

                // now try and start another one on same port.
                Guid secondEmuId = Guid.NewGuid();
                var secondEmuFactory = new AndroidSdkEmulatorFactory(logger, TestConfig.PathToAndroidEmulatorExe, adbFactory, TestConfig.AvdName, consolePort, true, true, secondEmuId, SingleInstanceMode.ReuseExistingThenKill);

                IEmulator secondEmulator = secondEmuFactory.GetEmulator();
                secondEmulator.Start(TestConfig.EmulatorStartupTimeout).Wait();

                // now try and start another one on same port.
                AndroidDevice device = (AndroidDevice)secondEmulator.Device;

                Assert.That(device.FullName() == firstDevice.FullName());

                var adb = adbFactory.GetAndroidDebugBridge();

                // this should result in the device being killed.
                secondEmulator.Stop();

                Thread.Sleep(new TimeSpan(0, 0, 3)); // give time for device to be killed.

                // device should no longer be listed.
                var devices = adb.GetDevices();
                foreach (var d in devices)
                {
                    Assert.That(d.FullName() != firstDevice.FullName(), "device was still listed in adb devices after it was killed");
                }

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
