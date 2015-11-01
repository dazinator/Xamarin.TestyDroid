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
    public class RunAndroidTestsOptions
    {

        [Option('t', "emulatortype", Required = false, DefaultValue = "sdk", HelpText = "The type of emulator to run the tests on.  Default is 'sdk'")]
        public string EmulatorType { get; set; }

        /// <summary>
        /// The full path to the emulator exe.
        /// </summary>
        [Option('e', "emulatorexepath", Required = true, HelpText = "The full path to the emulator exe.")]
        public string EmulatorExePath { get; set; }

        /// <summary>
        /// The full path to adb.exe.
        /// </summary>
        [Option('d', "debugbridgepath", Required = true, HelpText = "The full path to android debug bride (adb.exe)")]
        public string AdbExePath { get; set; }

        /// <summary>
        /// The full path to your build APK file for your android tests.
        /// </summary>
        [Option('f', "apkfile", Required = true, HelpText = "The full path to the .apk file containing your tests that should be run.")]
        public string ApkPath { get; set; }

        /// <summary>
        /// The name of the AVD image to launch in the emulator.
        /// </summary>
        [Option('i', "imagename", Required = true, HelpText = "The name of the avd image to launch in the emulator.")]
        public string ImageName { get; set; }

        /// <summary>
        /// The name of the AVD image to launch in the emulator.
        /// </summary>
        [Option('p', "packagename", Required = true, HelpText = "The package name of your apk package as per it's manifest..")]
        public string ApkPackageName { get; set; }

        /// <summary>
        /// The class path for your android test instruemntation class.
        /// </summary>
        [Option('c', "instrumentationclasspath", Required = true, HelpText = "The class path to the instrumentation class inside your tests apk.")]
        public string TestInstrumentationClassPath { get; set; }

        /// <summary>
        /// The number of seconds to wait for the emulator to startup.
        /// </summary>
        [Option('w', "emulatorwaittime", Required = false, DefaultValue = 120, HelpText = "The number of seconds to wait for the emulator to start up.")]
        public int EmulatorStartupWaitTimeInSeconds { get; set; }

        [Option('v', "verbose", HelpText = "Enable verbose output to the console during execution.")]
        public bool Verbose { get; set; }



        [HelpOption]
        public string GetUsage()
        {

            var helpText = HelpText.AutoBuild(this);
            var usage = new StringBuilder();
            usage.AppendLine(helpText.ToString());
            return usage.ToString();
        }

    }


    public class AndroidSdkEmulatorOptions
    {
        [Option('p', "emulatorpath", Required = true, HelpText = "The full path to emulator.exe")]
        public string EmulatorExePath { get; set; }



        [Option('p', "password", Required = false, DefaultValue = "x-oauth-basic", HelpText = "Your password. Defaults to x-oauth-basic if not specified - which allows an access token to be used as the username.")]
        public string Password { get; set; }

        [Option('o', "owner", Required = true, HelpText = "The username of the repo owner.")]
        public string Owner { get; set; }

        [Option('r', "repo", Required = true, HelpText = "The repository name.")]
        public string RepoName { get; set; }

        [Option('t', "tag", Required = true, HelpText = "The tag name. This tag must already exist.")]
        public string TagName { get; set; }

        [Option('n', "name", Required = true, HelpText = "The name given to the release.")]
        public string ReleaseName { get; set; }

        [Option('d', "desc", Required = false, HelpText = "The description for the release.")]
        public string Description { get; set; }

        [Option('f', "filedesc", Required = false, HelpText = "The path to a file containing the description for this release.")]
        public string DescriptionFile { get; set; }

        [Option('l', "draft", Required = false, HelpText = "creates the release as a draft release.")]
        public bool Draft { get; set; }

        [Option('e', "pre", Required = false, HelpText = "creates the release as a pre-release.")]
        public bool PreRelease { get; set; }

        [Option('v', "verbose", HelpText = "Enable verbose output to the console during execution.")]
        public bool Verbose { get; set; }

        [OptionList('a', "assetfiles", Separator = ',', HelpText = "Upload one or more files as assets against the release. To specify more than one file, seperate the filenames with a comma. For example: c:/myfile.txt,c:/myotherfile.txt")]
        public IList<string> ReleaseAssetFiles { get; set; }
    }

}
