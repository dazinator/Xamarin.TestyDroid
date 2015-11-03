using CommandLine;
using CommandLine.Text;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{

    public enum ReporterType
    {
        None = 0,
        TeamCity = 1
    }

    public class RunAndroidTestsOptions
    {

        [Option('r', "reporter-type", Required = false, DefaultValue = ReporterType.None, HelpText = "The type of reporter used to report progress. Specify teamcity if you would like to see progress in TeamCity builds.")]
        public ReporterType ReporterType { get; set; }     


        [Option('t', "emulatortype", Required = false, DefaultValue = "sdk", HelpText = "The type of emulator to run the tests on. In future may support Microsoft's Emulator.")]
        public string EmulatorType { get; set; }

        /// <summary>
        /// The full path to the emulator exe.
        /// </summary>
        [Option('e', "emulatorexepath", Required = true, HelpText = "The full path to the emulator exe.")]
        public string EmulatorExePath { get; set; }

        /// <summary>
        /// The full path to adb.exe.
        /// </summary>
        [Option('d', "debugbridgepath", Required = true, HelpText = "The full path to android debug bridge (adb.exe)")]
        public string AdbExePath { get; set; }

        /// <summary>
        /// The full path to your build APK file for your android tests.
        /// </summary>
        [Option('f', "apkfile", Required = true, HelpText = "The full path to the APK file containing your tests that should be run.")]
        public string ApkPath { get; set; }

        /// <summary>
        /// The name of the AVD image to launch in the emulator.
        /// </summary>
        [Option('i', "imagename", Required = true, HelpText = "The name of the avd image to launch in the emulator.")]
        public string ImageName { get; set; }

        /// <summary>
        /// The name of the AVD image to launch in the emulator.
        /// </summary>
        [Option('n', "name", Required = true, HelpText = "The package name of your APK package as per it's manifest.")]
        public string ApkPackageName { get; set; }

        /// <summary>
        /// The class path for your android test instruemntation class.
        /// </summary>
        [Option('c', "instrumentationclasspath", Required = true, HelpText = "The class path to the instrumentation class inside your tests APK. This should include the namespace (in lower case) and then the class name (case sensitive). E.g xamarin.testydroid.testtests.TestInstrumentation")]
        public string TestInstrumentationClassPath { get; set; }

        /// <summary>
        /// The number of seconds to wait for the emulator to startup.
        /// </summary>
        [Option('w', "emulatorwaittime", Required = false, DefaultValue = 120, HelpText = "The maximum number of seconds to wait for the emulator to start up before timing out.")]
        public int EmulatorStartupWaitTimeInSeconds { get; set; }

        [Option('v', "verbose", HelpText = "Enable verbose output to the console during execution.")]
        public bool Verbose { get; set; }

        [Option('p', "portnumber", Required = false, DefaultValue = 5554, HelpText = "The port number that the android console will be listening on, on localhost.")]
        public int PortNumber { get; set; }

        [HelpOption]
        public string GetUsage()
        {

            var helpText = HelpText.AutoBuild(this);
            var usage = new StringBuilder();
            usage.AppendLine(helpText.ToString());
            return usage.ToString();
        }

    }


}
