using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{  
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
