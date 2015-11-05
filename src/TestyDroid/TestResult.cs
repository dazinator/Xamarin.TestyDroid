using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestyDroid
{
    [DebuggerDisplay("Name = {Name}, Kind = {Kind}")]
    public class TestResult
    {
        
        public TestResult(string name, TestResultKind kind)
        {
            this.Name = name;
            this.Kind = kind;
        }
        public string Name { get; private set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public TimeSpan Duration { get; set; }
        public TestResultKind Kind { get; private set; }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() * 17 + this.Kind.GetHashCode();
        }
    }
}
