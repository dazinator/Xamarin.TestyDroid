using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public interface IAndroidDebugBridge
    {
        string QueryProperty(Device device, string propertyName);
        AndroidDevice[] GetDevices();
        void KillDevice(Device device);
        void Install(Device device, string apkFilePath, AdbInstallFlags installFlags = AdbInstallFlags.None);
        string StartInstrument(Device device, string packageName, string runnerClass, Action<string> onStdOut);
        string ReadFileContents(Device device, string path);
    }
}
