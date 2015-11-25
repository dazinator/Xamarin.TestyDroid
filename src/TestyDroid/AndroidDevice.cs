using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{

    public abstract class Device
    {
        public abstract string FullName();
    }

    public class AndroidDevice : Device
    {

        private const string IdPropertyName = "emu.uuid";

        public string Name { get; set; }
        public string Status { get; set; }
        public int Port { get; set; }

        public override string FullName()
        {
            return string.Format("{0}-{1}", Name, Port);
        }

        public static AndroidDevice Parse(string deviceMessage)
        {
            if (!string.IsNullOrWhiteSpace(deviceMessage))
            {
                var seperator = new char[] { '\t' };
                var deviceParts = deviceMessage.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

                var device = new AndroidDevice();

                if (deviceParts.Length > 0)
                {

                    var devicePartsNamePart = deviceParts[0];

                    var portSeperator = new string[] { "-", ":" };
                    var nameParts = devicePartsNamePart.Split(portSeperator, StringSplitOptions.RemoveEmptyEntries);

                    if (nameParts.Length > 0)
                    {
                        device.Name = nameParts[0];
                    }

                    if (deviceParts.Length > 1)
                    {
                        int port;
                        if (int.TryParse(nameParts[1], out port))
                        {
                            device.Port = port;
                        }
                    }
                }
                if (deviceParts.Length > 1)
                {
                    device.Status = deviceParts[1];
                }



                return device;

            }

            return null;
        }

        public void Kill(ILogger logger)
        {

            logger.LogMessage(string.Format("Killing device: {0}", FullName()));
            TcpClient client = new TcpClient("localhost", Port);
            using (var stream = client.GetStream())
            {

                byte[] results = new byte[100];
                var readCount = stream.Read(results, 0, 100);
                var resultText = Encoding.ASCII.GetString(results, 0, readCount);

                logger.LogMessage("Connected to device console.");
                logger.LogMessage(resultText);

                logger.LogMessage("Sending kill command.");
                var command = Encoding.ASCII.GetBytes("kill" + Environment.NewLine);
                stream.Write(command, 0, command.Length);
                stream.Flush();

                readCount = stream.Read(results, 0, 100);
                resultText = Encoding.ASCII.GetString(results, 0, readCount);
                logger.LogMessage(resultText);
                if (string.IsNullOrWhiteSpace(resultText) || !resultText.Contains("OK"))
                {
                    throw new Exception(string.Format("Unable to kill emulator. Expected OK Response from kill command, but was: {0}", resultText));
                }
                logger.LogMessage("Emulated device killed.");
                TimeSpan timeout = new TimeSpan(0, 0, 30);
                stream.Close((int)timeout.TotalMilliseconds);
            }
            client.Close();
        }

        public string QueryId(IAndroidDebugBridge adb)
        {
            var id = adb.QueryProperty(this, IdPropertyName);
            return id;
        }

    }
}
