using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
   
    public class ConsoleLogger : ILogger
    {
        private bool _verboseEnabled;
        
        public ConsoleLogger(bool verboseEnabled)
        {
            _verboseEnabled = verboseEnabled;
        }

        public void LogMessage(string message)
        {
            if(_verboseEnabled)
            {
                Console.WriteLine(message);
            }          
        }
    }
}
