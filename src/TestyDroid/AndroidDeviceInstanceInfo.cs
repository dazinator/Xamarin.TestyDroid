using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    public class AndroidDeviceInstanceInfo
    {
        private ILogger _logger;
        private StringBuilder _emulatorExeStandardOut = new StringBuilder();
        private StringBuilder _emulatorExeStandardErrorOut = new StringBuilder();
        private EmulatorAbortDetector _abortDetector = new EmulatorAbortDetector();

        public AndroidDeviceInstanceInfo(ILogger logger)
        {
            _logger = logger;
        }

        public AndroidDevice Device { get; set; }

        public IProcess Process { get; set; }

        public bool LeaveDeviceOpen { get; set; }

        public bool IsRunning
        {
            get
            {
                return Process != null && Process.IsRunning;
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                if (!LeaveDeviceOpen)
                {
                    this.KillDevice();
                }
                else
                {
                    _logger.LogMessage("Device will be left open.");
                }
                var process = Process;
                process.Stop();
                process = null;
            }
        }

        private void KillDevice()
        {
            var device = this.Device;
            if (device != null)
            {
                try
                {
                    device.Kill(_logger);
                }
                catch (SocketException se)
                {
                    _logger.LogMessage("Socket exception caught when attempting to kill the device. This usually means the device has allready closed so ignoring.");
                }
            }
            else
            {
                _logger.LogMessage("No device attached, so nothing to kill.");
                //  throw new InvalidOperationException("Unable to kill device as device not yet attached.");
            }
        }

        public void Start()
        {
            _logger.LogMessage(string.Format("Starting emulator: {0} {1}", Process.FileName, Process.Arguments));
            Process.Start();
            Process.ListenToStandardOut((s) =>
            {
                _emulatorExeStandardOut.AppendLine(s);
            });
            Process.ListenToStandardError((s) => _emulatorExeStandardErrorOut.AppendLine(s));
        }

        public bool DetectAborted()
        {
            return _abortDetector.HasAborted(_emulatorExeStandardErrorOut);
        }

        public void LogStandardOutput()
        {

            if (_emulatorExeStandardOut != null)
            {
                var process = Process;
                _logger.LogMessage(string.Format("Standard output from {0} was: ", process.FileName));
                _logger.LogMessage(_emulatorExeStandardOut.ToString());
            }
            else
            {
                _logger.LogMessage(string.Format("Standard output for emulator.exe process not available."));
            }

            if (_emulatorExeStandardErrorOut != null)
            {
                var process = Process;
                _logger.LogMessage(string.Format("Standard error output from {0} was: ", process.FileName));
                _logger.LogMessage(_emulatorExeStandardErrorOut.ToString());
            }
            else
            {
                _logger.LogMessage(string.Format("Standard error output for emulator.exe process not available."));
            }
        }

    }
}
