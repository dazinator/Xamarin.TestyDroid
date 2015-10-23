using System;

namespace Xamarin.TestyDroid
{
    public interface IEmulator : IDisposable
    {
        void Start();
        void WaitForBootComplete(TimeSpan bootTimeOut);
        void Stop();
        bool IsRunning { get; }
        bool IsBootComplete { get; }
        
    }       
}



