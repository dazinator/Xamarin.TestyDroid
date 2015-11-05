using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid.Tests
{
    public class TestConfig
    {
        public static TimeSpan EmulatorStartupTimeout = new TimeSpan(0, 2, 0);

#if WORK
        public static string PathToAndroidSdk = @"D:\android-sdk";
        public static string AvdName = "Xamarin_Android_API_15";
#else
        public static string PathToAndroidSdk = @"C:\Program Files (x86)\Android\android-sdk";     
        public static string AvdName = "AVD_GalaxyNexus_ToolsForApacheCordova";  
#endif

        public static string PathToAndroidSdkTools = System.IO.Path.Combine(PathToAndroidSdk, "tools");
        public static string PathToAdbExe = System.IO.Path.Combine(PathToAndroidSdk, @"platform-tools\adb.exe");
        public static string PathToAndroidEmulatorExe = System.IO.Path.Combine(PathToAndroidSdkTools, @"emulator.exe");

      
        public static string AndroidTestsPackageName = "TestyDroid.TestTests";
        public static string PathToAndroidTestsApk = @"TestyDroid.TestTests\bin\debug\testydroid.testtests-Signed.apk";
        public static string AndroidTestsInstrumentationClassPath = @"testydroid.testtests.TestInstrumentation";


    }
}
