using System;
using System.Threading.Tasks;

namespace TestyDroid
{
    public interface IEmulator : IDisposable
    {
        Task Start(TimeSpan timeout);
        void Stop();
        bool IsRunning { get; }
        bool IsBootComplete { get; }
        //Device Device { get; }
    }

    public interface IAndroidEmulator: IEmulator //: IEmulator
    {
        new AndroidDevice Device { get; }

        //Task Start(TimeSpan timeout);
        //void Stop();
        //bool IsRunning { get; }
        //bool IsBootComplete { get; }
      //  Device Device { get; }
    }
}



