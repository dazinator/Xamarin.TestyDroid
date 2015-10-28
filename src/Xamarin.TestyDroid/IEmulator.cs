using System;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public interface IEmulator : IDisposable
    {
        Task Start(TimeSpan timeout);
        //void WaitForBootComplete(TimeSpan bootTimeOut);
        void Stop();
        bool IsRunning { get; }
        bool IsBootComplete { get; }
        
    }       
}



