using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public string QueryProperty(Device device, string propertyName)
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

        public void KillDevice(Device device)
        {
            StringBuilder args = new StringBuilder();
            if (device != null)
            {
                args.AppendFormat("-s {0} ", device.FullName());
            }
            args.AppendFormat("emu kill");
            _adbProcess.Start(args.ToString());
        }

        public void Install(Device device, string apkFilePath, AdbInstallFlags installFlags = AdbInstallFlags.None)
        {
            StringBuilder args = new StringBuilder();
            if (device != null)
            {
                args.AppendFormat("-s {0} ", device.FullName());
            }
            args.Append("install ");

            if (installFlags != AdbInstallFlags.None)
            {
                var enums = Enum.GetValues(typeof(AdbInstallFlags));
                foreach (var installFlag in enums)
                {
                    // get description attribute
                    Enum flag = (AdbInstallFlags)installFlag;
                    if ((AdbInstallFlags)flag == AdbInstallFlags.None)
                    {
                        continue;
                    }
                    if (installFlags.HasFlag(flag))
                    {
                        string enumDesc = GetEnumDescription(flag);
                        args.AppendFormat("{0} ", enumDesc);
                    }
                }
            }

            if (!Path.IsPathRooted(apkFilePath))
            {
                var currentDir = Environment.CurrentDirectory;
                var apkPath = System.IO.Path.Combine(currentDir, apkFilePath);
            }

            args.AppendFormat("\"{0}\"", apkFilePath);

            StringBuilder output = new StringBuilder();
            _adbProcess.Start(args.ToString());

            _adbProcess.ListenToStandardOut((outMessage) =>
            {
                output.AppendLine(outMessage);
            });

            _adbProcess.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output.ToString()))
            {
                if (output.ToString().Contains("Success"))
                {
                    return;
                }
            }

            throw new Exception(string.Format("Unable to install APK file: {0}. Message: {1}", apkFilePath, output.ToString()));            
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public string StartInstrument(Device device, string packageName, string runnerClass, Action<string> onStdOut)
        {
            //   &quot;$(AndroidDebugBridgeExePath) & quot; shell am instrument - w $(AndroidTestsManifestPackageName) /$(AndroidTestsInstrumentationClassPath)
            StringBuilder args = new StringBuilder();
            if (device != null)
            {
                args.AppendFormat("-s {0} ", device.FullName());
            }
            args.Append("shell am instrument -w ");
            //if (waitForFinish)
            //{
            //    args.Append("-w ");
            //}

            args.AppendFormat("{0}/{1}", packageName, runnerClass);

            StringBuilder output = new StringBuilder();
            _adbProcess.Start(args.ToString());

            _adbProcess.ListenToStandardOut((outMessage) =>
            {
                output.AppendLine(outMessage);
                onStdOut(outMessage);
            });

            _adbProcess.WaitForExit();

            return output.ToString();

        }
    }
}
