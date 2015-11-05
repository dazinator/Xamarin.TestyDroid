using System;

using Android.App;
using Android.Runtime;
using Xamarin.Android.NUnitLite;
using Android.OS;
using Android.Util;

namespace TestyDroid.Android
{
    /// <summary>
    /// An enhanced TestSuiteInstrumentation that provides better reporting of test results. 
    /// </summary>
    public abstract class TestyDroidTestSuiteInstrumentation : TestSuiteInstrumentation
    {
        public TestyDroidTestSuiteInstrumentation(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        protected override abstract void AddTests();

        public override void Finish(Result resultCode, Bundle results)
        {
            if (results == null)
            {
                throw new InvalidOperationException("could not get results.");
            }
            // There is an internal class "AndroidRunner" with a public static method that returns it's instance.
            // Get this using reflection to get at test results.
            try
            {
                var ReportBuilder = new ReportBuilder();
                ReportBuilder.CreateReport(results);
            }
            catch (Exception e)
            {
                Log.Error("error getting results", e.ToString());
                throw;
            }

            base.Finish(resultCode, results);
        }   

    }

}