using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    public class BaseFactory
    {
        public IProcess GetProcess(string exePath, string args)
        {
            var process = new ProcessStartInfo(exePath, args);
            process.UseShellExecute = false;
            process.CreateNoWindow = true;
            process.RedirectStandardOutput = true;
            process.RedirectStandardError = true;
            return new ProcessWrapper(process);
        }

        public IProcess FindProcess(string exePath, ILogger logger)
        {
            return new ExistingEmulatorExeProcess(exePath);

            //Process[] processes = Process.GetProcesses();
            //foreach (Process process in processes)
            //{
            //    try
            //    {
            //        var fileName = process.MainModule.FileName;
            //        if (fileName.ToLowerInvariant() == exePath.ToLowerInvariant())
            //        {
            //            logger.LogMessage("Found existing running process: " + process.ProcessName + " for filename: " + fileName);
            //            return new ProcessWrapper(process);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        logger.LogMessage("Error occurred examining process filename for process: " + process.ProcessName);
            //    }
            //}

            //return null;

        }
        
        public class ExistingEmulatorExeProcess : IProcess
        {

            private bool _isRunning;
            private string _args = string.Empty;
            private string _fileName = string.Empty;

            public ExistingEmulatorExeProcess(string filePath)
            {
                _fileName = filePath;
                _isRunning = true;
            }

            public string Arguments
            {
                get
                {
                    return _args;
                }
            }

            public string FileName
            {
                get
                {
                    return _fileName;
                }
            }

            public bool IsRunning
            {
                get
                {
                    return _isRunning;
                }
            }

            public void ListenToStandardError(Action<string> onStdErrorReceived)
            {
                // no op;

            }

            public void ListenToStandardOut(Action<string> onStdOutReceived)
            {
                // no op;
            }

            public void Start(string args = null)
            {
                // no op;
            }

            public void Stop()
            {
                // no op;
            }

            public int WaitForExit()
            {
                // no op;
                return 0;
            }
        }

    }
}
