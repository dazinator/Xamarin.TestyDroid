# TestyDroid

TestyDroid is a small command line tool, to handle running your unit tests on an android device during CI builds.

1. It will start the android emulator
2. It will load the desired AVD and detect when it has successfully booted.
4. It will install your APK package containing your tests
5. It will then run your tests using the Instrumentation class (see docs for info)
6. It will ensure the emulator is terminated afterwards.


Test Results are reported to `STDOUT` via the default reported.
However there is the option to use the `TeamCity` reported, in which case results will be reported using Team City Contol messages and will appear in Team City.

Open to supporting other build systems via custom Reporters.


