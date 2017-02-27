using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    public interface IAndroidDebugBridge
    {
        string QueryProperty(AndroidDevice device, string propertyName);
        AndroidDevice[] GetDevices();
        void KillDevice(AndroidDevice device);
        void Install(AndroidDevice device, string apkFilePath, AdbInstallFlags installFlags = AdbInstallFlags.None);
        string StartInstrument(AndroidDevice device, string packageName, string runnerClass, Action<string> onStdOut);
        string ReadFileContents(AndroidDevice device, string path);
        void RestartServer();
    }
}
