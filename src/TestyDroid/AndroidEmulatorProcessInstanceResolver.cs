using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    public class AndroidEmulatorInstanceResolver
    {

        private ILogger _logger;
        private IAndroidDebugBridgeFactory _adbFactory;
        private string _emulatorExePath;
        public AndroidEmulatorInstanceResolver(ILogger logger, IAndroidDebugBridgeFactory adbFactory, string emulatorExePath)
        {
            _logger = logger;
            _adbFactory = adbFactory;
            _emulatorExePath = emulatorExePath;
        }

        public virtual AndroidEmulatorInstanceInfo EnsureInstance(SingleInstanceMode mode, Guid id, string avdName, int? consolePort, bool noBootAnim, bool noWindow)
        {

            AndroidEmulatorInstanceInfo result = new AndroidEmulatorInstanceInfo(_logger);

            var adb = _adbFactory.GetAndroidDebugBridge();
            var devices = adb.GetDevices();
            var deviceOnSamePort = devices.FirstOrDefault(a => a.Port == consolePort);

            if (deviceOnSamePort != null)
            {

                switch (mode)
                {
                    case SingleInstanceMode.Abort:
                        throw new InvalidOperationException("Cannot start emulator as there is already an existing emulator running on the same port. Use argument -s to specify single instance re-use options.");

                    case SingleInstanceMode.KillExisting:
                        // TODO: issue kill command for existing emulator before proceeding.
                        _logger.LogMessage("Found existing android device listening on same console port. Will kill.");
                        deviceOnSamePort.Kill(_logger);
                        break;

                    case SingleInstanceMode.ReuseExisting:
                    case SingleInstanceMode.ReuseExistingThenKill:
                        // TODO: don't start new emulator, use existing one - and' don't terminate afterwards.
                        _logger.LogMessage("Found existing android device listening on same console port. Will re-use this device.");
                        // Get existing process
                        result.Process = new ExistingEmulatorExeProcess(_emulatorExePath);
                        result.Device = deviceOnSamePort;
                        result.LeaveDeviceOpen = mode == SingleInstanceMode.ReuseExisting;
                        return result;
                }
            }

            // did not detect existing emulator instance / process on port, so proceed to create a new one.
            StringBuilder args = new StringBuilder();
            args.AppendFormat("-avd {0}", avdName);
            if (consolePort.HasValue && consolePort != 0)
            {
                args.AppendFormat(" -port {0}", consolePort);
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
            var process = ProcessHelper.GetProcess(_emulatorExePath, args.ToString());

            result.Process = process;
            result.Device = null;
            result.LeaveDeviceOpen = false;

            return result;
        }

       

    }
}
