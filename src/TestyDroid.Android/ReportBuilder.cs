using Android.OS;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Xamarin.Android.NUnitLite;

namespace TestyDroid.Android
{
    public class ReportBuilder
    {
        public ReportBuilder()
        {
            
        }

        public IDictionary<string, TestResult> GetTestResults()
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
              
        private string CreateReport()
        {
            var testResults = GetTestResults();

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
                }
            }

            var reportContents = testResultsDoc.OuterXml;
            return reportContents;

        }

        public void CreateReport(Bundle bundle)
        {
            var report = CreateReport();
            var reportFile = SaveReportToFile(report);
            bundle.PutString("##TestyDroidTestsReport", reportFile);
        }

        private string SaveReportToFile(string reportContents)
        {
            // save the results to tests file
            //var path = global::Android.OS.Environment.ExternalStorageDirectory;
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filename = Path.Combine(path, this.GetType().FullName + ".xml");

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            using (var streamWriter = new StreamWriter(filename))
            {
                streamWriter.Write(reportContents);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return filename;
        }

    }
}
