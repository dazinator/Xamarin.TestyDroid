# TestyDroid

TestyDroid is a small command line tool, to handle running your unit tests on an android device during CI builds.

1. It will start the android emulator
2. It will load the desired AVD and detect when it has successfully booted.
4. It will install your APK package containing your tests
5. It will then run your tests using the Instrumentation class (see docs for info)
6. It will ensure the emulator is terminated afterwards.


Test Results are reported to `STDOUT` via the default reporter.
However there is the option to use the `TeamCity` reporter, in which case results will be reported using Team City Control messages and so will appear in Team City if invoked during the Team City build.

Open to supporting other build systems via custom Reporters.

# Getting Started

See here: https://github.com/dazinator/Xamarin.TestyDroid/wiki/Getting-Started

# Usage

Call Xamarin.TestyDroid.exe from the command line, with the following arguments:

```

  -t, --emulatortype                (Default: sdk) The type of emulator to run
                                    the tests on.

  -e, --emulatorexepath             Required. The full path to the emulator
                                    exe.

  -d, --debugbridgepath             Required. The full path to android debug
                                    bride (adb.exe)

  -f, --apkfile                     Required. The full path to the .apk file
                                    containing your tests that should be run.

  -i, --imagename                   Required. The name of the avd image to
                                    launch in the emulator.

  -n, --name                        Required. The package name of your apk
                                    package as per it's manifest..

  -c, --instrumentationclasspath    Required. The class path to the
                                    instrumentation class inside your tests
                                    apk. This should include the namespace (in
                                    lower case) and then the class name (case
                                    sensitive). E.g
                                    xamarin.testydroid.testtests.TestInstrumentation

  -w, --emulatorwaittime            (Default: 120) The maximum number of seconds to
                                    wait for the emulator to start up before timing out.

  -v, --verbose                     Enable verbose output to the console during
                                    execution.

  -p, --portnumber                  (Default: 5554) The port number that the
                                    android console will be listening on, on
                                    localhost.

  --help                            Display this help screen.

```

Example:

`Xamarin.TestyDroid.exe -e "C:\Program Files (x86)\Android\android-sdk\tools\emulator.exe" -d "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" -f "C:\Users\Darrell\Repo\Xamarin.TestyDroid\src\Xamarin.TestyDroid.TestTests\bin\Release\Xamarin.TestyDroid.TestTests-Signed.apk" -i "AVD_GalaxyNexus_ToolsForApacheCordova" -n "xamarin.testydroid.testtests" -c "xamarin.testydroid.testtests.TestInstrumentation" -w 120 -v`



