using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
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


        private StringBuilder _emulatorExeStandardOut = new StringBuilder();
        private StringBuilder _emulatorExeStandardErrorOut = new StringBuilder();


        public AndroidSdkEmulator(ILogger logger, IProcess androidEmulatorProcess, IAndroidDebugBridgeFactory adbFactory, Guid id, int? consolePort)
        {
            _logger = logger;
            _emulatorProcess = androidEmulatorProcess;
            _adbFactory = adbFactory;
            _id = id;
            _consolePort = consolePort;
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

            _logger.LogMessage(string.Format("Starting emulator: {0} {1}", _emulatorProcess.FileName, _emulatorProcess.Arguments));
            _emulatorProcess.Start();

            _emulatorProcess.ListenToStandardOut((s) => _emulatorExeStandardOut.AppendLine(s));
            _emulatorProcess.ListenToStandardError((s) => _emulatorExeStandardErrorOut.AppendLine(s));

            // Now get the devices..
            _logger.LogMessage(string.Format("Finding attached Adb Device: {0}", _id));

            // poll for attached device.
            var adb = _adbFactory.GetAndroidDebugBridge();          

            bool hasAttached = false;
            TimeSpan pollingFrequency = new TimeSpan(0, 0, 2);
            int attempts = 0;

            while (!hasAttached)
            {
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
            _logger.LogMessage(string.Format("Standard output from {0} was: ", _emulatorProcess.FileName));
            _logger.LogMessage(_emulatorExeStandardOut.ToString());
            _logger.LogMessage(string.Format("Standard error output from {0} was: ", _emulatorProcess.FileName));
            _logger.LogMessage(_emulatorExeStandardErrorOut.ToString());
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
                this.KillDevice();
                _emulatorProcess.Stop();
                _emulatorProcess = null;
            }
        }

        private void KillDevice()
        {
            var device = this._androidDevice;
            if (device != null)
            {
                _logger.LogMessage(string.Format("Killing device: {0}", device.FullName()));
                TcpClient client = new TcpClient("localhost", device.Port);
                using (var stream = client.GetStream())
                {

                    byte[] results = new byte[100];
                    var readCount = stream.Read(results, 0, 100);
                    var resultText = Encoding.ASCII.GetString(results, 0, readCount);

                    _logger.LogMessage("Connected to device console.");
                    _logger.LogMessage(resultText);

                    _logger.LogMessage("Sending kill command.");
                    var command = Encoding.ASCII.GetBytes("kill" + Environment.NewLine);
                    stream.Write(command, 0, command.Length);
                    stream.Flush();

                    readCount = stream.Read(results, 0, 100);
                    resultText = Encoding.ASCII.GetString(results, 0, readCount);
                    _logger.LogMessage(resultText);
                    if (string.IsNullOrWhiteSpace(resultText) || !resultText.Contains("OK"))
                    {
                        throw new Exception(string.Format("Unable to kill emulator. Expected OK Response from kill command, but was: {0}", resultText));
                    }
                    _logger.LogMessage("Emulator killed.");
                    TimeSpan timeout = new TimeSpan(0, 0, 30);
                    stream.Close((int)timeout.TotalMilliseconds);
                }
                client.Close();
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
