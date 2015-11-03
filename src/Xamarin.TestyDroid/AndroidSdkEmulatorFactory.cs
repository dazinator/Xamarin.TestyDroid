using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public class AndroidSdkEmulatorFactory : BaseFactory, IEmulatorFactory
    {
        private string _avdName;
        private int? _port;
        private bool _noBootAnim;
        private bool _noWindow;
        private Guid _id;
        private string _EmulatorExePath;
        private IAndroidDebugBridgeFactory _adbFactory;
        private ILogger _Logger;

        public AndroidSdkEmulatorFactory(ILogger logger, string emulatorExePath, IAndroidDebugBridgeFactory adbFactory, string avdName, int? port, bool noBootAnim, bool noWindow, Guid id)
        {
            _Logger = logger;
            _EmulatorExePath = emulatorExePath;
            _adbFactory = adbFactory;
            _avdName = avdName;
            _port = port;
            _noBootAnim = noBootAnim;
            _noWindow = noWindow;
            _id = id;
        }

        public IEmulator GetEmulator()
        {
            StringBuilder args = new StringBuilder();
            args.AppendFormat("-avd {0}", _avdName);
            if (_port.HasValue)
            {
                args.AppendFormat(" -port {0}", _port.Value);
            }
            if (_noBootAnim)
            {
                args.Append(" -no-boot-anim");
            }
            if (_noWindow)
            {
                args.Append(" -no-window");
            }

            args.AppendFormat(" -prop emu.uuid={0}", _id);

            var process = GetProcess(_EmulatorExePath, args.ToString());

            return new AndroidSdkEmulator(_Logger, process, _adbFactory, _id, _port);
        }


    }
}
