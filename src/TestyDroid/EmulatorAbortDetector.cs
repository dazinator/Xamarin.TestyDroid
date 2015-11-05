using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    public class EmulatorAbortDetector
    {
        private List<string> _matchErrors = new List<string>();

        public EmulatorAbortDetector()
        {
            _matchErrors = new List<string>();
            _matchErrors.Add("PANIC: Could not open:");
            _matchErrors.Add("it seems too many emulator instances are running on this machine. Aborting");            
        }

        public bool HasAborted(StringBuilder errorOut)
        {
            foreach (var item in _matchErrors)
            {
                if (errorOut.ToString().Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
