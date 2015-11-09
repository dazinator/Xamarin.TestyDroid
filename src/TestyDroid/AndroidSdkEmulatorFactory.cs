using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{

    public class AndroidSdkEmulatorFactory : BaseFactory, IEmulatorFactory
    {
        private string _avdName;
        private int _port;
        private bool _noBootAnim;
        private bool _noWindow;
        private Guid _id;
        private string _EmulatorExePath;
        private IAndroidDebugBridgeFactory _adbFactory;
        private ILogger _Logger;
        private SingleInstanceMode _SingleInstanceMode;

        public AndroidSdkEmulatorFactory(ILogger logger, string emulatorExePath, IAndroidDebugBridgeFactory adbFactory, string avdName, int port, bool noBootAnim, bool noWindow, Guid id, SingleInstanceMode singleInstanceMode = SingleInstanceMode.Abort)
        {
            _Logger = logger;
            _EmulatorExePath = emulatorExePath;
            _adbFactory = adbFactory;
            _avdName = avdName;
            _port = port;
            _noBootAnim = noBootAnim;
            _noWindow = noWindow;
            _id = id;
            _SingleInstanceMode = singleInstanceMode;
        }

        public IEmulator GetEmulator()
        {

            // look for an existing emulator
            var adb = _adbFactory.GetAndroidDebugBridge();
            var devices = adb.GetDevices();
            var deviceOnSamePort = devices.FirstOrDefault(a => a.Port == _port);
            AndroidSdkEmulator emulator = null;
            IProcess emulatorExeProcess = null;

            if (deviceOnSamePort != null)
            {

                switch (_SingleInstanceMode)
                {
                    case SingleInstanceMode.Abort:
                        throw new InvalidOperationException("Cannot start emulator as there is already an existing emulator running on the same port. Use argument -s to specify single instance re-use options.");

                    case SingleInstanceMode.KillExisting:
                        // TODO: issue kill command for existing emulator before proceeding.
                        _Logger.LogMessage("Found existing android device listening on same console port. Will kill.");
                        deviceOnSamePort.Kill(_Logger);
                        break;

                    case SingleInstanceMode.ReuseExisting:
                        // TODO: don't start new emulator, use existing one - and' don't terminate afterwards.
                        _Logger.LogMessage("Found existing android device listening on same console port. Will re-use this device and will leave open afterwards.");
                        // Get exisring process
                        emulatorExeProcess = FindProcess(_EmulatorExePath, _Logger);
                        if (emulatorExeProcess == null)
                        {
                            // todo: if we don't find the process, it could be the device is being hosted by a different emulator. We don't handle this as of yet.
                            throw new NotSupportedException("Detected a device running on the same port, however could not find existing emulator.exe process. Are you hosting the device using a different emulator?");
                        }
                        emulator = new AndroidSdkEmulator(_Logger, emulatorExeProcess, _adbFactory, deviceOnSamePort, true);
                        break;

                    case SingleInstanceMode.ReuseExistingThenKill:
                        // TODO: don't start new emulator, use existing one - but still terminate afterwards.
                        _Logger.LogMessage("Found existing android device listening on same console port. Will re-use this device and will kill afterwards.");
                        // Get exisring process
                        emulatorExeProcess = FindProcess(_EmulatorExePath, _Logger);
                        if (emulatorExeProcess == null)
                        {
                            // todo: if we don't find the process, it could be the device is being hosted by a different emulator. We don't handle this as of yet.
                            throw new NotSupportedException("Detected a device running on the same port, however could not find existing emulator.exe process. Are you hosting the device using a different emulator?");
                        }
                        emulator = new AndroidSdkEmulator(_Logger, emulatorExeProcess, _adbFactory, deviceOnSamePort, false);
                        break;
                }

            }

            if (emulator == null)
            {
                // did not detect existing emulator on port, so proceed to create new one.
                StringBuilder args = new StringBuilder();
                args.AppendFormat("-avd {0}", _avdName);
                if (_port != 0)
                {
                    args.AppendFormat(" -port {0}", _port);
                }
                if (_noBootAnim)
                {
                    args.Append(" -no-boot-anim");
                }
                if (_noWindow)
                {
                    args.Append(" -no-window");
                }

                args.AppendFormat(" -prop emu.uuid={0}", _id);
                var process = GetProcess(_EmulatorExePath, args.ToString());
                emulator = new AndroidSdkEmulator(_Logger, process, _adbFactory, _id, _port);
            }

            return emulator;

        }


    }
}
