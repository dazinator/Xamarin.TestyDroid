using System;

using Android.App;
using Android.Runtime;
using Xamarin.Android.NUnitLite;
using System.Reflection;
using Android.OS;
using System.Collections.Generic;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using Android.Util;
using System.Xml;

namespace Xamarin.TestyDroid.TestTests
{
    [Instrumentation(Name = "xamarin.testydroid.testtests.TestInstrumentation")]
    public class TestInstrumentation : TestSuiteInstrumentation
    {

        public TestInstrumentation(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        protected override void AddTests()
        {
            AddTest(Assembly.GetExecutingAssembly());
        }

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
                IDictionary<string, NUnit.Framework.Internal.TestResult> testResults = EnsureTestResults();
                AddResultsToBundle(testResults, results);
                // results.PutBoolean("##TestyDroidResultFormat", true);
            }
            catch (Exception e)
            {
                Log.Error("error getting results", e.ToString());
                throw;
            }


            base.Finish(resultCode, results);
        }

        private void AddResultsToBundle(IDictionary<string, TestResult> testResults, Bundle results)
        {

            var testResultsDoc = new XmlDocument();
            var testResultsElement = testResultsDoc.CreateElement("TestResults");
            testResultsDoc.AppendChild(testResultsElement);

            // now output desired format in bundle.
            foreach (var testResult in testResults.Values)
            {
                if (!testResult.HasChildren)
                {
                    var testElement = testResultsDoc.CreateElement("TestResult");
                    testResultsElement.AppendChild(testElement);

                    var nameAtt = testResultsDoc.CreateAttribute("Name");
                    nameAtt.Value = testResult.FullName;
                    testElement.Attributes.Append(nameAtt);

                    var statusAtt = testResultsDoc.CreateAttribute("Status");
                    statusAtt.Value = testResult.ResultState.Status.ToString();
                    testElement.Attributes.Append(statusAtt);

                    var durationAtt = testResultsDoc.CreateAttribute("Duration");
                    durationAtt.Value = testResult.Duration.ToString();
                    testElement.Attributes.Append(durationAtt);

                    var messageAtt = testResultsDoc.CreateAttribute("Message");
                    messageAtt.Value = testResult.Message;
                    testElement.Attributes.Append(messageAtt);

                    var labelAtt = testResultsDoc.CreateAttribute("Label");
                    labelAtt.Value = testResult.ResultState.Label;
                    testElement.Attributes.Append(labelAtt);

                    testElement.InnerText = testResult.StackTrace;

                    //  testResultsElement.AppendChild(new XmlElement())
                    // string testResultBundleKey = string.Format("##Test:{0}##", testResult.FullName);
                    //  string testResultBundleValue = string.Format("<TestResult Name='{0}' Status:{1} Duration:{1}##Message:{2}##StackTrace:{3}##Label:{4}",testResult.FullName, testResult.ResultState.Status,  testResult.Duration testResult.Message, testResult.StackTrace);

                }
            }

            var reportContents = testResultsDoc.OuterXml;
            results.PutString("##TestyDroidTestsReport", reportContents);

            //// now output desired format in bundle.
            //foreach (var testResult in testResults.Values)
            //{
            //    if (!testResult.HasChildren)
            //    {
            //        string testResultBundleKey = string.Format("##UnitTest##{0}##{1}##{2}##", testResult.FullName, testResult.ResultState.Status,  testResult.Duration);


            //        string testResultBundleValue = string.Format("##Message##{0}StackTrace##{1}##", testResult.Message, testResult.StackTrace);
            //        results.PutString(testResultBundleKey, testResultBundleValue);
            //    }
            //}

        }

        private IDictionary<string, TestResult> EnsureTestResults()
        {
            var aType = typeof(TestSuiteInstrumentation);
            if (aType == null)
            {
                throw new InvalidOperationException("could not get aType.");
            }
            var assembly = aType.Assembly;
            if (assembly == null)
            {
                throw new InvalidOperationException("could not get assembly of atype.");
            }

            var androidRunnerType = assembly.GetType("Xamarin.Android.NUnitLite.AndroidRunner");
            if (androidRunnerType == null)
            {
                throw new InvalidOperationException("could not get Runner type.");
            }

            var prop = androidRunnerType.GetProperty("Runner", BindingFlags.Public | BindingFlags.Static);
            if (prop == null)
            {
                throw new InvalidOperationException("could not get Runner property.");
            }

            var objRunner = prop.GetValue(null, null);
            if (objRunner == null)
            {
                throw new InvalidOperationException("could not get Runner instance.");
            }

            var resultsProperty = objRunner.GetType().GetProperty("Results", BindingFlags.Public | BindingFlags.Static);
            if (resultsProperty == null)
            {
                throw new InvalidOperationException("could not get Results property from Runner instance.");
            }

            var testResults = (IDictionary<string, TestResult>)resultsProperty.GetValue(objRunner);
            return testResults;


        }
    }

}