using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid.Tests
{
    public class TestConfig
    {
        public static TimeSpan EmulatorStartupTimeout = new TimeSpan(0, 2, 0);

        public static string PathToAndroidSdk = @"C:\Program Files (x86)\Android\android-sdk";
        public static string PathToAndroidSdkTools = System.IO.Path.Combine(PathToAndroidSdk, "tools");

        public static string PathToAdbExe = System.IO.Path.Combine(PathToAndroidSdk, @"platform-tools\adb.exe");
        public static string PathToAndroidEmulatorExe = System.IO.Path.Combine(PathToAndroidSdkTools, @"emulator.exe");

        public static string AvdName = "AVD_GalaxyNexus_ToolsForApacheCordova";
        public static string AndroidTestsPackageName = "xamarin.testydroid.testtests";
        public static string PathToAndroidTestsApk = @"Xamarin.TestyDroid.TestTests\bin\release\xamarin.testydroid.testtests-Signed.apk";
        public static string AndroidTestsInstrumentationClassPath = @"xamarin.testydroid.testtests.TestInstrumentation";


    }
}
