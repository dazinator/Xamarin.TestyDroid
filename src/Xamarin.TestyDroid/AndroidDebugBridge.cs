using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public class AndroidDebugBridge : IAndroidDebugBridge
    {
        private IProcess _adbProcess;

        public AndroidDebugBridge(IProcess adbProcess)
        {
            _adbProcess = adbProcess;
        }

        public string QueryProperty(AndroidDevice device, string propertyName)
        {
            StringBuilder args = new StringBuilder();
            if (device != null)
            {
                args.AppendFormat("-s {0} ", device.FullName());
            }
            args.AppendFormat("shell getprop {0}", propertyName);

            StringBuilder output = new StringBuilder();
            _adbProcess.Start(args.ToString());

            _adbProcess.ListenToStandardOut((outMessage) =>
            {
                output.AppendLine(outMessage);
            });

            _adbProcess.WaitForExit();
            return output.ToString().Trim();
        }

        public AndroidDevice[] GetDevices()
        {

            StringBuilder output = new StringBuilder();
            _adbProcess.Start("devices");

            _adbProcess.ListenToStandardOut((outMessage) =>
            {
                output.AppendLine(outMessage);
            });

            _adbProcess.WaitForExit();
            _adbProcess.Stop();

            if (output.Length <= 0)
            {
                return new AndroidDevice[] { };
            }
            else
            {
                string[] lines = output.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                                                  .Skip(1).ToArray(); // skip over first line as that is just adb output that is not of interest.

                var devices = new List<AndroidDevice>();
                foreach (var line in lines)
                {
                    var androidDevice = AndroidDevice.Parse(line);
                    if (androidDevice != null)
                    {
                        devices.Add(androidDevice);
                    }
                }
                return devices.ToArray();
            }

        }
    }
}
