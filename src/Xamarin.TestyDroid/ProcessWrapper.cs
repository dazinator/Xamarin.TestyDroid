using System;
using System.Diagnostics;

namespace Xamarin.TestyDroid
{
    public class ProcessWrapper : IProcess
    {
        private ProcessStartInfo _processStartInfo;
        private bool _isRunning;
        private Process _process;

        public string FileName
        {
            get
            {
                return _processStartInfo.FileName;
            }
        }

        public string Arguments
        {
            get
            {
                return _processStartInfo.Arguments;
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public ProcessWrapper(ProcessStartInfo processStartInfo)
        {
            _processStartInfo = processStartInfo;
        }

        public void Start(string args = null)
        {
            if (args != null)
            {
                _processStartInfo.Arguments = args;
            }
            _process = Process.Start(_processStartInfo);
            _isRunning = true;
        }

        public void ListenToStandardOut(Action<string> onStdOutReceived)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Process must be started first.");
            }
            _process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => onStdOutReceived(e.Data);
            _process.BeginOutputReadLine();
        }

        public void ListenToStandardError(Action<string> onStdErrorReceived)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Process must be started first.");
            }
            _process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => onStdErrorReceived(e.Data);
            _process.BeginErrorReadLine();
        }

        public void Stop()
        {
            if (_process != null && _isRunning)
            {
                _process.Close();
                _process = null;
            }
        }

        public int WaitForExit()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Process must be started first.");
            }

            _process.WaitForExit();
            return _process.ExitCode;
        }
    }
}
