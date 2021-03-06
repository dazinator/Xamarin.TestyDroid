# TestyDroid

TestyDroid makes automating your android tests (Xamarin Tests App) on an android device really easy.

# Getting Started

See here: https://github.com/dazinator/Xamarin.TestyDroid/wiki/Getting-Started

# What does it do?

TestyDroid handles all of the following for you:

1. Starting the android emulator. (Or can use an existing instance) 
2. Loading the desired AVD and detecting when it has successfully booted.
4. Installing the APK package containing your tests
5. Launching your tests and collating the report.
6. Ensuring the emulator is terminated appropriately afterwards.

Test Results are written to `STDOUT` (console out) by default, however there is the option to specify `TeamCity` as the reporter, in which case test results [will be reported in Team City format](https://confluence.jetbrains.com/display/TCD65/Build+Script+Interaction+with+TeamCity) and so will appear in the Team City user interface if invoked during a Team City build.

Open to supporting other build systems via custom Reporters.

# Usage

After adding the [Xamarin.TestyDroid](https://www.nuget.org/packages/Xamarin.TestyDroid/) NuGet package to your `Android Tests App` project, you can find `TestyDroid.exe` in the NuGet packages `tools` directory.

Call Xamarin.TestyDroid.exe from the command line, with the following arguments:

```

  -r, --reporter-type               (Default: None) The type of reporter used
                                    to report progress. Specify teamcity if you
                                    would like to see progress in TeamCity
                                    builds.

  -t, --emulatortype                (Default: sdk) The type of emulator to run
                                    the tests on. In future may support
                                    Microsoft's Emulator.

  -e, --emulatorexepath             Required. The full path to the emulator
                                    exe.

  -d, --debugbridgepath             Required. The full path to android debug
                                    bridge (adb.exe)

  -f, --apkfile                     Required. The full path to the APK file
                                    containing your tests that should be run.

  -i, --imagename                   Required. The name of the avd image to
                                    launch in the emulator.

  -n, --name                        Required. The package name of your APK
                                    package as per it's manifest.

  -c, --instrumentationclasspath    Required. The class path to the
                                    instrumentation class inside your tests
                                    APK. This should include the namespace (in
                                    lower case) and then the class name (case
                                    sensitive). E.g
                                    xamarin.testydroid.testtests.TestInstrumentation

  -w, --emulatorwaittime            (Default: 120) The maximum number of
                                    seconds to wait for the emulator to start
                                    up before timing out.

  -v, --verbose                     Enable verbose output to the console during
                                    execution.

  -p, --portnumber                  (Default: 5554) The port number that the
                                    android console will be listening on, on
                                    localhost.
                                    
  -s, --single-instance-mode        (Default: Abort) Controls what happens if there is 
                                    an existing device detected already running on
                                    the same console port. Options are: Abort,
                                    KillExisting, ReuseExisting and ReuseExistingThenKill

  --help                            Display this help screen.

```

Example TestyDroid.exe usage:

`Xamarin.TestyDroid.exe -e "C:\Program Files (x86)\Android\android-sdk\tools\emulator.exe" -d "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" -f "C:\Users\Darrell\Repo\Xamarin.TestyDroid\src\Xamarin.TestyDroid.TestTests\bin\Release\Xamarin.TestyDroid.TestTests-Signed.apk" -i "AVD_GalaxyNexus_ToolsForApacheCordova" -n "xamarin.testydroid.testtests" -c "xamarin.testydroid.testtests.TestInstrumentation" -w 120 -v`



