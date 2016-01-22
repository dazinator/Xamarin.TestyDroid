using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private SingleInstanceMode _instanceMode;
        private string _avdName;
        private bool _noBootAnime;
        private bool _noWindow;
        private AndroidEmulatorInstanceResolver _instanceResolver;
        private AndroidEmulatorInstanceInfo _instanceInfo;

        public AndroidSdkEmulator(ILogger logger, string emulatorExePath, string avdName, IAndroidDebugBridgeFactory adbFactory, Guid id, int? consolePort, SingleInstanceMode instanceMode = SingleInstanceMode.KillExisting, bool noBootAnim = true, bool noWindow = true)
        {
            _logger = logger;
            _avdName = avdName;
            _adbFactory = adbFactory;
            _id = id;
            _consolePort = consolePort;
            _instanceMode = instanceMode;
            _noBootAnime = noBootAnim;
            _noWindow = noWindow;
            _instanceResolver = new AndroidEmulatorInstanceResolver(logger, adbFactory, emulatorExePath);
        }      

        public bool IsRunning
        {
            get
            {
                return _instanceInfo != null && _instanceInfo.IsRunning;
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
            StartInstance();

            // Now get the devices..
            _logger.LogMessage(string.Format("Finding attached Adb Device: {0}", _id));

            // poll for attached device.         


            var adb = _adbFactory.GetAndroidDebugBridge();
            TimeSpan pollingFrequency = new TimeSpan(0, 0, 2);
            int attempts = 0;

            while (this.Device == null)
            {
                if (_instanceInfo.DetectAborted())
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
                        restarted = true;
                        // Now try and start a new instance.
                        StartInstance();
                        // We may now see an existing device, so check.
                        if (Device != null)
                        {
                            break;
                        }
                    }
                }

                // Need to query the devices to see if ours is attached yet.
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
                        _instanceInfo.Device = device;
                        _logger.LogMessage(string.Format("Device Attached"));
                        break;
                    }
                }

                // if still not attached, check for timeout.
                if (this.Device == null)
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



        private void StartInstance()
        {
            _logger.LogMessage("Resolving emulator instance");
            _instanceInfo = _instanceResolver.EnsureInstance(_instanceMode, _id, _avdName, _consolePort, _noBootAnime, _noWindow);
            _instanceInfo.Start();
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
            if (_instanceInfo != null)
            {
                _instanceInfo.LogStandardOutput();
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
                _instanceInfo.Stop();
                _instanceInfo = null;
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
            _isBootComplete = true;

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

        public Device Device
        {
            get
            {
                if (_instanceInfo != null)
                {
                    return _instanceInfo.Device;
                }
                else
                {
                    return null;
                }

            }
        }

    }
}
