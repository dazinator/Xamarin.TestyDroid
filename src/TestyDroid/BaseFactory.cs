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

    }
}
