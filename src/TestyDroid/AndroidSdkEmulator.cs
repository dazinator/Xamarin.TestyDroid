using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestyDroid
{

    public class AndroidSdkEmulator : IEmulator
    {

        private ILogger _logger;

        private Guid _id;
        private bool _isBootComplete;
        private int? _consolePort;
        private IAndroidDebugBridgeFactory _adbFactory;
        private IProcess _emulatorProcess;
        private AndroidDevice _androidDevice;
        private bool _errorOnStartDetected;

        private StringBuilder _emulatorExeStandardOut = new StringBuilder();
        private StringBuilder _emulatorExeStandardErrorOut = new StringBuilder();
        private EmulatorAbortDetector _abortDetector = new EmulatorAbortDetector();

        private bool _LeaveDeviceOpen;

        public AndroidSdkEmulator(ILogger logger, IProcess androidEmulatorProcess, IAndroidDebugBridgeFactory adbFactory, Guid id, int? consolePort)
        {
            _logger = logger;
            _emulatorProcess = androidEmulatorProcess;
            _adbFactory = adbFactory;
            _id = id;
            _consolePort = consolePort;
        }

        public AndroidSdkEmulator(ILogger logger, IProcess androidEmulatorProcess, IAndroidDebugBridgeFactory adbFactory, AndroidDevice existingDevice, bool leaveDeviceOpen)
        {
            _logger = logger;
            _emulatorProcess = androidEmulatorProcess;
            _adbFactory = adbFactory;
            _consolePort = existingDevice.Port;
            _androidDevice = existingDevice;
            _LeaveDeviceOpen = leaveDeviceOpen;
        }

        public bool IsRunning
        {
            get
            {
                return _emulatorProcess != null && _emulatorProcess.IsRunning;
            }
        }

        public bool IsBootComplete
        {
            get
            {
                return _isBootComplete;
            }
        }

        public void Dispose()
        {
            if (IsRunning)
            {
                try
                {
                    Stop();
                }
                catch (Exception e)
                {
                    //throw;
                }
            }
        }

        public async Task Start(TimeSpan timeout)
        {
            var timeNow = DateTime.UtcNow;
            var endTime = timeNow.Add(timeout);

            // emulator process allready running, just ensure device has finished booting.
            if (IsRunning)
            {
                await Task.Factory.StartNew(() => WaitForBootComplete(endTime))
                .ConfigureAwait(false);
                return;
            }

            // otherwise start a new instance, and wait for boot complete.
            await Task.Factory.StartNew(() => StartEmulator(endTime))
                .ContinueWith((startTask) =>
                {
                    if (startTask.IsFaulted)
                    {
                        throw startTask.Exception;
                    }
                    WaitForBootComplete(endTime);
                })
                .ConfigureAwait(false);
        }

        private void StartEmulator(DateTime expiryTime)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Emulator is allready running..");
            }

            bool restarted = false;
            const bool allowAdbServerRestart = true;

            StartProcess();

            // Now get the devices..
            _logger.LogMessage(string.Format("Finding attached Adb Device: {0}", _id));

            // poll for attached device.
            var adb = _adbFactory.GetAndroidDebugBridge();

            bool hasAttached = false;
            TimeSpan pollingFrequency = new TimeSpan(0, 0, 2);
            int attempts = 0;

            while (!hasAttached)
            {
                if (_abortDetector.HasAborted(_emulatorExeStandardErrorOut))
                {
                    WriteEmulatorExeOutputToLog();

                    if (!allowAdbServerRestart | restarted)
                    {
                        throw new Exception("Emulator could not start.");
                    }
                    else
                    {
                        _logger.LogMessage("Attempting restart of adb server to resolve an issue.");
                        Stop();
                        adb.RestartServer();
                        StartProcess();
                    }
                }

                attempts = attempts + 1;
                var devices = adb.GetDevices();
                var matchingPortDevices = devices.Where(d => d.Port == _consolePort);

                _logger.LogMessage(string.Format("Polling attempt: {0}", attempts));
                // now for each device, get the uuid property
                foreach (var device in matchingPortDevices)
                {
                    _logger.LogMessage(string.Format("Querying Device: {0}", device.FullName()));

                    var id = adb.QueryProperty(device, "emu.uuid");

                    _logger.LogMessage(string.Format("Device: {0}, emu.uuid: {1}", device.FullName(), id));

                    if (id == _id.ToString())
                    {
                        _androidDevice = device;
                        hasAttached = true;
                        _logger.LogMessage(string.Format("Device Attached"));
                    }
                }

                if (!hasAttached)
                {
                    if (DateTime.UtcNow.Add(pollingFrequency) > expiryTime)
                    {
                        OnEmulatorStartupFailed(expiryTime);
                    }
                    Thread.Sleep(pollingFrequency);
                }
            }

            EnsureDeviceAttached();

        }

        private void StartProcess()
        {
            _logger.LogMessage(string.Format("Starting emulator: {0} {1}", _emulatorProcess.FileName, _emulatorProcess.Arguments));
            _emulatorProcess.Start();
            _emulatorProcess.ListenToStandardOut((s) =>
            {
                _emulatorExeStandardOut.AppendLine(s);
            });
            _emulatorProcess.ListenToStandardError((s) => _emulatorExeStandardErrorOut.AppendLine(s));
        }

        private void EnsureDeviceAttached()
        {
            if (this.Device == null)
            {
                // dump standard error out.
                _logger.LogMessage(string.Format("Could not find device."));
                WriteEmulatorExeOutputToLog();
                throw new InvalidOperationException("Could not find the attached device..");
            }
        }

        private void WriteEmulatorExeOutputToLog()
        {

            if (_emulatorExeStandardOut != null)
            {
                _logger.LogMessage(string.Format("Standard output from {0} was: ", _emulatorProcess.FileName));
                _logger.LogMessage(_emulatorExeStandardOut.ToString());
            }
            else
            {
                _logger.LogMessage(string.Format("Standard output for emulator.exe process not available."));
            }

            if (_emulatorExeStandardErrorOut != null)
            {
                _logger.LogMessage(string.Format("Standard error output from {0} was: ", _emulatorProcess.FileName));
                _logger.LogMessage(_emulatorExeStandardErrorOut.ToString());
            }
            else
            {
                _logger.LogMessage(string.Format("Standard error output for emulator.exe process not available."));
            }
        }

        private void OnEmulatorStartupFailed(DateTime expiryTime)
        {
            _logger.LogMessage(string.Format("Timeout expired."));
            WriteEmulatorExeOutputToLog();
            throw new TimeoutException(string.Format("The timeout was exceeded when starting the emulator. Timeout expired at: {0} (hh:mm:ss)", expiryTime));
        }

        public void Stop()
        {
            if (IsRunning)
            {
                if (!_LeaveDeviceOpen)
                {
                    this.KillDevice();
                }
                else
                {
                    _logger.LogMessage("Device will be left open.");
                }
                _emulatorProcess.Stop();
                _emulatorProcess = null;
            }
        }

        private void KillDevice()
        {
            var device = this._androidDevice;
            if (device != null)
            {
                try
                {
                    device.Kill(_logger);
                }
                catch (SocketException se)
                {
                    _logger.LogMessage("Socket exception caught when attempting to kill the device. This usually means the device has allready closed.");
                    throw;
                }
            }
            else
            {
                _logger.LogMessage("Cannot kill as no device attached.");
                throw new InvalidOperationException("Unable to kill device as device not yet attached.");
            }
        }

        public void WaitForBootComplete(DateTime expiryTime)
        {
            // block until finished booting.
            _logger.LogMessage(string.Format("Waiting until: {0} for device to complete boot up..", expiryTime));
            if (!IsRunning)
            {
                throw new InvalidOperationException("Emulator must be started first.");
            }

            WaitForProperty(expiryTime, "dev.bootcomplete", "1");
            WaitForProperty(expiryTime, "sys.boot_completed", "1");
            WaitForProperty(expiryTime, "init.svc.bootanim", "stopped");


        }

        public void WaitForProperty(DateTime expiryTime, string propertyName, string value)
        {
            // block until property value is set.
            _logger.LogMessage(string.Format("Waiting for property: {0} to be set to: {1} (Will wait until = {2})", propertyName, value, expiryTime));
            if (!IsRunning)
            {
                throw new InvalidOperationException("Emulator must be started first.");
            }

            bool hasBeenSet = false;
            TimeSpan pollingTime = new TimeSpan(0, 0, 3);

            while (!hasBeenSet)
            {
                var adb = _adbFactory.GetAndroidDebugBridge();
                string propertyResult = adb.QueryProperty(this.Device, propertyName);
                if (propertyResult == value)
                {
                    hasBeenSet = true;
                    _logger.LogMessage("Property has been set.");
                }
                else
                {
                    if (DateTime.UtcNow.Add(pollingTime) > expiryTime)
                    {
                        throw new TimeoutException("Emulator did not manage to boot within the allotted timeout.");
                    }
                    Thread.Sleep(pollingTime);
                }
            }
        }

        public Device Device { get { return _androidDevice; } }

    }
}
