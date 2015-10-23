using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public interface IProcess
    {
        string FileName { get; }
        string Arguments { get; }
        void Start(string args = null);
        void Stop();
        bool IsRunning { get; }
        void ListenToStandardOut(Action<string> onStdOutReceived);
        void ListenToStandardError(Action<string> onStdErrorReceived);
        int WaitForExit();
    }
}
