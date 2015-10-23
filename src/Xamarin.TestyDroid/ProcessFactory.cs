using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public class ProcessFactory : IProcessFactory
    {
        private string _EmulatorExePath;
        private string _AdbExePath;
        private ILogger _Logger;

        public ProcessFactory(ILogger logger, string emulatorExePath, string adbExePath)
        {
            _EmulatorExePath = emulatorExePath;
            _AdbExePath = adbExePath;
            _Logger = logger;
        }        

        public virtual IEmulator GetAndroidSdkEmulator(string avdName, int? port, bool noBootAnim, bool noWindow, Guid id)
        {
            StringBuilder args = new StringBuilder();
            args.AppendFormat("-avd {0}", avdName);
            if (port.HasValue)
            {
                args.AppendFormat(" -port {0}", port.Value);
            }
            if (noBootAnim)
            {
                args.Append(" -no-boot-anim");
            }
            if (noWindow)
            {
                args.Append(" -no-window");
            }

            args.AppendFormat(" -prop emu.uuid={0}", id);

            var process = GetProcess(_EmulatorExePath, args.ToString());
            var adb = GetAndroidDebugBridge();
            return new AndroidSdkEmulator(_Logger, process, this.GetAndroidDebugBridge, id, port);
        }

        public IAndroidDebugBridge GetAndroidDebugBridge()
        {
            var process = GetProcess(_AdbExePath, null);
            return new AndroidDebugBridge(process);
        }

        public IProcess GetProcess(string exePath, string args)
        {
            var process = new ProcessStartInfo(exePath, args);
            process.UseShellExecute = false;
            process.CreateNoWindow = true;
            process.RedirectStandardOutput = true;
            process.RedirectStandardError = true;
            return new ProcessWrapper(process);
        }
    }
}
