using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public class AndroidDebugBridgeFactory : BaseFactory, IAndroidDebugBridgeFactory
    {
        private string _adbExePath;

        public AndroidDebugBridgeFactory(string adbExePath)
        {
            _adbExePath = adbExePath;
        }

        public IAndroidDebugBridge GetAndroidDebugBridge()
        {
            var process = GetProcess(_adbExePath, null);
            return new AndroidDebugBridge(process);
        }
    }
}
