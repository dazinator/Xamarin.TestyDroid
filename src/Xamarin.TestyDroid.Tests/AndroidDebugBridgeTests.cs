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
        public void Can_Get_Devices()
        {
            var logger = new ConsoleLogger();
            Guid emuId = Guid.NewGuid();

            // Start an emulator.
            var factory = new ProcessFactory(logger, TestConfig.PathToAndroidEmulatorExe, TestConfig.PathToAdbExe);
            int consolePort = 5554;

            using (IEmulator droidEmulator = factory.GetAndroidSdkEmulator(TestConfig.AvdName, consolePort, true, false, emuId))
            {
                droidEmulator.Start();

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
            }

               
        }     

    }
}
