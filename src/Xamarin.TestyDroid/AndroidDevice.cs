using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{

    public abstract class Device
    {
        public abstract string FullName();       
    }

    public class AndroidDevice : Device
    {
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

                    var portSeperator = new string[] { "-" , ":"};
                    var nameParts = devicePartsNamePart.Split(portSeperator, StringSplitOptions.RemoveEmptyEntries);

                    if (nameParts.Length > 0)
                    {
                        device.Name = nameParts[0];
                    }

                    if (deviceParts.Length > 1)
                    {
                        int port;
                        if(int.TryParse(nameParts[1], out port))
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

    }
}
