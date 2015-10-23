using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public interface IProcessFactory
    {
        IEmulator GetAndroidSdkEmulator(string avdName, int? port, bool noBootAnim, bool noWindow, Guid id);
        IAndroidDebugBridge GetAndroidDebugBridge();
        IProcess GetProcess(string exePath, string args);
    }
}
