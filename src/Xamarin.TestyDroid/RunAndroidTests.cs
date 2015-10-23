using Microsoft.Build.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Xamarin.TestyDroid
{
    public class RunAndroidTests : AbstractTask
    {

        public RunAndroidTests()
        {
            this.StartEmulatorArgumentsFormatString = "-avd {0} -no-boot-anim";
            this.InstallApkArgumentsFormatString = "install -r \"{0}\"";
            this.RunTestsArgumentsFormatString = "shell am instrument -w {0}/{1}";
        }

        /// <summary>
        /// The full path to the emulator exe.
        /// </summary>
        [Required]
        public string EmulatorExePath { get; set; }

        /// <summary>
        /// The full path to adb.exe.
        /// </summary>
        [Required]
        public string AdbExePath { get; set; }

        /// <summary>
        /// The full path to your build APK file for your android tests.
        /// </summary>
        [Required]
        public string ApkPath { get; set; }

        /// <summary>
        /// The name of the AVD image to launch in the emulator.
        /// </summary>
        [Required]
        public string AvdName { get; set; }

        /// <summary>
        /// The name of your android package as per the manifest.
        /// </summary>
        [Required]
        public string ApkPackageName { get; set; }

        /// <summary>
        /// The class path for your android test instruemntation class.
        /// </summary>
        [Required]
        public string TestInstrumentationClassPath { get; set; }

        /// <summary>
        /// The number of seconds to wait for the emulator to startup.
        /// </summary>
        [Required]
        public int EmulatorStartupWaitTimeInSeconds { get; set; }

        /// <summary>
        /// The arguments passed to the emulator.
        /// </summary>     
        public string StartEmulatorArgumentsFormatString { get; set; }

        /// <summary>
        /// The arguments passed to adb to install the tests apk package..
        /// </summary>     
        public string InstallApkArgumentsFormatString { get; set; }

        /// <summary>
        /// The arguments passed to adb to run your tests using your test instrumentation class.
        /// </summary>     
        public string RunTestsArgumentsFormatString { get; private set; }

        /// <summary>
        /// The result output.
        /// </summary>
        [Output]
        public string Output { get; set; }

        public override bool ExecuteTask()
        {

            string name = Path.GetFileNameWithoutExtension(EmulatorExePath);
            this.LogMessage(string.Format("Starting emulator: {0}...", name));

            string emulatorArgs = GetEmulatorArgs();
            var emulatorProcess = new ProcessStartInfo(EmulatorExePath, emulatorArgs);
            emulatorProcess.UseShellExecute = false;
            emulatorProcess.CreateNoWindow = true;
            
            using (var process = Process.Start(emulatorProcess))
            {

                try
                {
                    // wait for emulator to load.
                    var sleepTime = new TimeSpan(0, 0, 0, EmulatorStartupWaitTimeInSeconds, 0);
                    LogMessage(string.Format("Waiting for emulator to load up. {0} (hh:mm:ss) ", sleepTime), MessageImportance.Normal);
                    System.Threading.Thread.Sleep(sleepTime);

                    var installApkArgs = GetInstallApkArgs();

                    var adbInstallApkProcessStartInfo = new ProcessStartInfo(AdbExePath, installApkArgs);
                    LogMessage(string.Format("AdbExePath is: {0} ", AdbExePath), MessageImportance.Normal);
                    LogMessage(string.Format("InstallApkArgs are: {0} ", installApkArgs), MessageImportance.Normal);

                    adbInstallApkProcessStartInfo.UseShellExecute = false;
                    adbInstallApkProcessStartInfo.RedirectStandardOutput = true;
                    adbInstallApkProcessStartInfo.RedirectStandardError = true;

                    using (var adbInstallApkProcess = Process.Start(adbInstallApkProcessStartInfo))
                    {
                        LogMessage(string.Format("Installing tests apk: {0}", ApkPath), MessageImportance.Normal);

                        var outputData = new StringBuilder();
                        var errorData = new StringBuilder();

                        adbInstallApkProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) => outputData.AppendLine(e.Data);
                        adbInstallApkProcess.BeginOutputReadLine();

                        adbInstallApkProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => errorData.AppendLine(e.Data);
                        adbInstallApkProcess.BeginErrorReadLine();

                        adbInstallApkProcess.WaitForExit();

                        LogMessage(string.Format("Adb install apk exited, Output Data was: {0}", outputData), MessageImportance.High);
                        LogMessage(string.Format("Adb install apk exited, Error Data: {0}", errorData), MessageImportance.High);

                        if (adbInstallApkProcess.ExitCode != 0)
                        {
                            LogMessage("Unable to install test apk", MessageImportance.High);
                            return false;
                        }
                    }



                    string runTestsArgs = GetRunTestsArgs();
                    LogMessage(string.Format("run tests args: {0}", runTestsArgs), MessageImportance.Normal);
                    LogMessage(string.Format("adb exe path: {0}", AdbExePath), MessageImportance.Normal);


                    var adbRunTestsProcessStartInfo = new ProcessStartInfo(AdbExePath, runTestsArgs);
                    //adbProcessStartInfo.Arguments = runTestsArgs;
                    adbRunTestsProcessStartInfo.UseShellExecute = false;
                    // adbProcessStartInfo.RedirectStandardInput = true;
                    adbRunTestsProcessStartInfo.RedirectStandardOutput = true;
                    adbRunTestsProcessStartInfo.RedirectStandardError = true;

                    LogMessage("Starting process..", MessageImportance.Normal);
                    using (var runTestsProcess = Process.Start(adbRunTestsProcessStartInfo))
                    {
                        LogMessage("Redirecting output and error..", MessageImportance.Normal);
                        var outputData = new StringBuilder();
                        var errorData = new StringBuilder();

                        runTestsProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) => outputData.AppendLine(e.Data);
                        runTestsProcess.BeginOutputReadLine();

                        runTestsProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => errorData.AppendLine(e.Data);
                        runTestsProcess.BeginErrorReadLine();

                        // StreamReader read = runTestsProcess.StandardOutput;
                        LogMessage("Waiting for exit..", MessageImportance.Normal);
                        runTestsProcess.WaitForExit();
                        LogMessage(string.Format("TESTS OUTPUT WAS: {0}", outputData), MessageImportance.High);

                        if(errorData.Length > 0)
                        {
                            LogMessage(string.Format("TESTS ERROR OUTPUT: {0}", errorData), MessageImportance.High);
                        }                      

                    
                        LogMessage("Checking exit code..", MessageImportance.Normal);

                        try
                        {
                            if (runTestsProcess.ExitCode != 0)
                            {
                                LogMessage(string.Format("Running tests errored: {0}", runTestsArgs), MessageImportance.High);
                                return false;
                            }

                            // need to detect if any tests failed.

                          
                           // runTestsProcess.Close();

                            return true;
                        }
                        catch (Exception e)
                        {
                            LogMessage(string.Format("Error checking exit code: {0}", e.ToString()), MessageImportance.High);
                            throw;
                        }


                    }
                }
                catch (Exception e)
                {
                    LogMessage(string.Format("Error: {0}", e.ToString()), MessageImportance.High);
                    throw;
                }
                finally
                {
                    var closed = process.CloseMainWindow();
                    process.Close();
                    process.Kill();
                }
            }

        }

        private string GetRunTestsArgs()
        {
            return string.Format(RunTestsArgumentsFormatString, ApkPackageName, TestInstrumentationClassPath);
        }

        protected virtual string GetEmulatorArgs()
        {
            return string.Format(StartEmulatorArgumentsFormatString, AvdName);
        }

        protected virtual string GetInstallApkArgs()
        {
            return string.Format(InstallApkArgumentsFormatString, ApkPath);
        }
    }

}
