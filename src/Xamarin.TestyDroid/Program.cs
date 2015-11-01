using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    class Program
    {
        public static int Main(string[] args)
        {

#if DEBUG
            if (Debugger.IsAttached)
            {
                args = new string[15];
                args[0] = "-e";
                args[1] = @"C:\Program Files (x86)\Android\android-sdk\tools\emulator.exe";
                args[2] = "-d";
                args[3] = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";
                args[4] = "-f";
                args[5] = @"C:\Users\Darrell\Repo\Xamarin.TestyDroid\src\Xamarin.TestyDroid.TestTests\bin\Release\Xamarin.TestyDroid.TestTests-Signed.apk";
                args[6] = "-i";
                args[7] = "AVD_GalaxyNexus_ToolsForApacheCordova";
                args[8] = "-p";
                args[9] = "xamarin.testydroid.testtests";
                args[10] = "-c";
                args[11] = "xamarin.testydroid.testtests.TestInstrumentation";
                args[12] = "-w";
                args[13] = "120";
                args[14] = "-v";
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

                var factory = new ProcessFactory(logger, options.EmulatorExePath, options.AdbExePath);

                IEmulator droidEmulator;

                if (options.EmulatorType == "sdk")
                {
                    droidEmulator = factory.GetAndroidSdkEmulator(options.ImageName, 5554, true, false, emuId);
                }
                else
                {
                    logger.LogMessage("Unsupported emulator type.");
                    return -1;
                }

                StartEmulatorAndRunTests(droidEmulator, options);
                return 0;

            }
            else
            {
                Console.WriteLine("Failed to parse args, see usage.");
                return -1;
            }

        }

        private static void StartEmulatorAndRunTests(IEmulator droidEmulator, RunAndroidTestsOptions options)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(options.EmulatorStartupWaitTimeInSeconds);
            using (droidEmulator)
            {
                droidEmulator.Start(timeout).Wait();
                droidEmulator.Stop();
            }


        }
    }
}
