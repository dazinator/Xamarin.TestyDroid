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
        private Func<IAndroidDebugBridge> _adbFactory;
        private IProcess _emulatorProcess;

        public AndroidSdkEmulator(ILogger logger, IProcess androidEmulatorProcess, Func<IAndroidDebugBridge> adbFactory, Guid id, int? consolePort)
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
            await Task.Factory.StartNew(StartEmulator)
                .ContinueWith((startTask) =>
                {
                    WaitForBootComplete(timeout);
                })
                .ConfigureAwait(false);
        }

        private void StartEmulator()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Emulator is allready running..");
            }

            _logger.LogMessage(string.Format("Starting emulator: {0} {1}", _emulatorProcess.FileName, _emulatorProcess.Arguments));
            _emulatorProcess.Start();

            // Now get the devices..
            _logger.LogMessage(string.Format("Finding attached Adb Device: {0}", _id));

            // poll for attached device.
            var adb = _adbFactory();

            var timeNow = DateTime.UtcNow;
            var pollTimeout = new TimeSpan(0, 0, 30);
            var endTime = timeNow.Add(pollTimeout);

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
                        this.Device = device;
                        hasAttached = true;
                        _logger.LogMessage(string.Format("Device Attached"));
                    }
                }

                if (!hasAttached)
                {
                    if (DateTime.UtcNow.Add(pollingFrequency) > endTime)
                    {
                        throw new TimeoutException(string.Format("Emulator started but could not detect the attached adb device after polling within the timeout of: {0} (hh:mm:ss)", pollTimeout));
                    }
                    Thread.Sleep(pollingFrequency);
                }
            }

            if (this.Device == null)
            {
                throw new InvalidOperationException("Could not find attached device after emulator started..");
            }
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
            var device = this.Device;
            if (device != null)
            {
                TcpClient client = new TcpClient("localhost", device.Port);
                using (var stream = client.GetStream())
                {

                    byte[] results = new byte[100];
                    var readCount = stream.Read(results, 0, 100);
                    var resultText = Encoding.ASCII.GetString(results, 0, readCount);

                    var command = Encoding.ASCII.GetBytes("kill" + Environment.NewLine);
                    stream.Write(command, 0, command.Length);
                    stream.Flush();

                    //  readCount = stream.Read(results, 0, 100);
                    //  resultText = Encoding.ASCII.GetString(results, 0, readCount);

                    TimeSpan timeout = new TimeSpan(0, 0, 30);
                    stream.Close((int)timeout.TotalMilliseconds);
                }
                client.Close();
            }
            else
            {
                throw new InvalidOperationException("Unable to kill device as device not yet attached.");
            }

        }

        public void WaitForBootComplete(TimeSpan bootTimeOut)
        {
            // block until finished booting.
            if (!IsRunning)
            {
                throw new InvalidOperationException("Emulator must be started first.");
            }

            var timeNow = DateTime.UtcNow;
            var endTime = timeNow.Add(bootTimeOut);

            bool hasBooted = false;
            TimeSpan pollingTime = new TimeSpan(0, 0, 3);

            while (!hasBooted)
            {
                var adb = _adbFactory();
                string propertyResult = adb.QueryProperty(this.Device, "dev.bootcomplete");
                if (propertyResult == "1")
                {
                    hasBooted = true;
                }
                else
                {
                    if (DateTime.UtcNow.Add(pollingTime) > endTime)
                    {
                        throw new TimeoutException(string.Format("Emulator did not manage to completely boot within the allotted timeout of: {0} (hh:mm:ss)", bootTimeOut));
                    }
                    Thread.Sleep(pollingTime);
                }
            }
        }

        public AndroidDevice Device { get; protected set; }

    }
}
