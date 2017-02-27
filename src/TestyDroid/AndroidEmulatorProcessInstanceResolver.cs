using System;
using System.Linq;
using System.Text;

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

        public virtual AndroidDeviceInstanceInfo EnsureInstance(SingleInstanceMode mode, int? consolePort, Action<StringBuilder> appendExeArgs)
        {

            AndroidDeviceInstanceInfo result = new AndroidDeviceInstanceInfo(_logger);

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
            StringBuilder argsBuilder = new StringBuilder();
            appendExeArgs(argsBuilder);
            
            var process = ProcessHelper.GetProcess(_emulatorExePath, argsBuilder.ToString());

            result.Process = process;
            result.Device = null;
            result.LeaveDeviceOpen = false;

            return result;
        }       

    }
}
