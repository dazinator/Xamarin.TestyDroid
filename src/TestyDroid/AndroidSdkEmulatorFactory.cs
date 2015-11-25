using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{

    public class AndroidSdkEmulatorFactory : IEmulatorFactory
    {
        private string _avdName;
        private int _port;
        private bool _noBootAnim;
        private bool _noWindow;
        private Guid _id;
        private string _EmulatorExePath;
        private IAndroidDebugBridgeFactory _adbFactory;
        private ILogger _Logger;
        private SingleInstanceMode _SingleInstanceMode;

        public AndroidSdkEmulatorFactory(ILogger logger, string emulatorExePath, IAndroidDebugBridgeFactory adbFactory, string avdName, int port, bool noBootAnim, bool noWindow, Guid id, SingleInstanceMode singleInstanceMode = SingleInstanceMode.Abort)
        {
            _Logger = logger;
            _EmulatorExePath = emulatorExePath;
            _adbFactory = adbFactory;
            _avdName = avdName;
            _port = port;
            _noBootAnim = noBootAnim;
            _noWindow = noWindow;
            _id = id;
            _SingleInstanceMode = singleInstanceMode;
        }

        public IEmulator GetEmulator()
        {
            return new AndroidSdkEmulator(_Logger, _EmulatorExePath, _avdName, _adbFactory, _id, _port, _SingleInstanceMode, _noBootAnim, _noWindow);
        }


    }
}
