using System;
using System.Collections.Generic;
using System.IO;
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
        private char PortSeperator { get; set; }

        public override string FullName()
        {
            return string.Format("{0}{1}{2}", Name, PortSeperator, Port);
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
                    char seperatorFound;
                    var portSeperator = new char[] { '-', ':' };
                    foreach (var sep in portSeperator)
                    {
                        if(devicePartsNamePart.Contains(sep))
                        {
                            seperatorFound = sep;
                            
                            var nameParts = devicePartsNamePart.Split(new char[] { sep }, StringSplitOptions.RemoveEmptyEntries);

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

                            device.PortSeperator = seperatorFound;
                            break;
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

                byte[] results = new byte[500];
                var readCount = stream.Read(results, 0, 500);
                var resultText = Encoding.ASCII.GetString(results, 0, readCount);

                logger.LogMessage("Connected to device console.");
                logger.LogMessage(resultText);

                if(resultText.Contains("Authentication required"))
                {
                    logger.LogMessage("Android console is requesting authentication token.");

                    readCount = stream.Read(results, 0, 500);
                    resultText = Encoding.ASCII.GetString(results, 0, readCount);

                    string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        path = Directory.GetParent(path).ToString();
                    }

                    var authTokenFilePath = Path.Combine(path, ".emulator_console_auth_token");
                    if(!File.Exists(authTokenFilePath))
                    {
                        logger.LogMessage("Could not find auth token file @ " + authTokenFilePath);
                        throw new Exception("The android console is asking for an authentication token, however the auth token file doesn't exist @ " + authTokenFilePath);
                    }

                    var authToken = File.ReadAllText(authTokenFilePath);
                    var authCommand = Encoding.ASCII.GetBytes("auth " + authToken + Environment.NewLine);
                    stream.Write(authCommand, 0, authCommand.Length);
                    stream.Flush();

                   // readCount = stream.Read(results, 0, 500);
                   // resultText = Encoding.ASCII.GetString(results, 0, readCount);

                    // client.Client.Send(authMessageBytes);

                    //resultText = Encoding.ASCII.GetString(results, 0, readCount);
                }

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
