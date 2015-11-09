using System;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace TestyDroid
{
    public class ProcessWrapper : IProcess
    {
        private ProcessStartInfo _processStartInfo = null;
        private bool _isRunning;
        private Process _process = null;
        private string _args = null;
        private string _fileName = null;

        public string FileName
        {
            get
            {
                if (_fileName == null)
                {
                    if (_processStartInfo != null)
                    {
                        _fileName = _processStartInfo.FileName;
                    }
                    else
                    {
                        _fileName = _process.MainModule.FileName;
                    }
                }
                return _fileName;
            }
        }

        public string Arguments
        {
            get
            {
                if (_args == null)
                {
                    if (_processStartInfo != null)
                    {
                        _args = _processStartInfo.Arguments;
                    }
                    else
                    {
                        _args = GetCommandLine(this._process);
                    }
                }
                return _args;
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

        public ProcessWrapper(Process process)
        {
            _process = process;
            _isRunning = true;
            _processStartInfo = null;
        }

        public void Start(string args = null)
        {
            // The only time process start info would be null is if we are wrapping an existing running process.
            if (_processStartInfo == null)
            {
                // in that case create a new start info, using current filename and arguments.
                var fileName = this.FileName;
                var startArgs = args ?? this.Arguments;
                _processStartInfo = new ProcessStartInfo(fileName, startArgs);
            }
            else
            {
                // We allready have a start info, but allow args to be overriden.
                if (args != null)
                {
                    _processStartInfo.Arguments = args;
                }
            }

            // Starts the process.
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
                _isRunning = false;
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

        private string GetCommandLine(Process process)
        {
            var commandLine = new StringBuilder(process.MainModule.FileName);

            commandLine.Append(" ");
            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                    commandLine.Append(" ");
                }
            }

            return commandLine.ToString();
        }
    }

}
