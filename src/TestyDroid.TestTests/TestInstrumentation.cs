using System;

using Android.App;
using Android.Runtime;
using System.Reflection;
using TestyDroid.Android;

namespace TestyDroid.TestTests
{
    [Instrumentation(Name = "testydroid.testtests.TestInstrumentation")]
    public class TestInstrumentation : TestyDroidTestSuiteInstrumentation
    {
        public TestInstrumentation(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        protected override void AddTests()
        {
            AddTest(Assembly.GetExecutingAssembly());
        }
     
    }

}