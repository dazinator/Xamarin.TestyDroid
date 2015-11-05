using NUnit.Framework;
using System.Diagnostics;
using System;
using System.Threading;
using System.Linq;

namespace TestyDroid.Tests
{    
    [TestFixture(Category = "Unit")]
    public class TestResultsTests
    {

        [Test]
        public void Can_Union_Test_Results()
        {
            var resultSetOne = new TestResults();
            var testA = new TestResult("My Special Test A", TestResultKind.Passed);            
            resultSetOne.AddTest(testA);

            var testB = new TestResult("My Special Test B", TestResultKind.Failure);
            testB.StackTrace = "Special stack trace";
            resultSetOne.AddTest(testB);

            var testC = new TestResult("My Special Test C", TestResultKind.Skipped);         
            resultSetOne.AddTest(testC);
            
            var resultSetTwo = new TestResults();
            var dummyTestA = new TestResult("", TestResultKind.Passed);           
            resultSetTwo.AddTest(dummyTestA);

            var dummyTestB = new TestResult("My Special Test B", TestResultKind.Failure);            
            resultSetTwo.AddTest(dummyTestB);

            var dummyTestC = new TestResult("", TestResultKind.Inconclusive);          
            resultSetTwo.AddTest(dummyTestC);

            resultSetOne.Merge(resultSetTwo);

            Assert.That(resultSetOne != null);
            var tests = resultSetOne.GetTests().ToList();


            Assert.That(tests.Count == 4);

            var firstTest = tests[0];
            Assert.That(firstTest.Name == testA.Name);
            Assert.That(firstTest.Kind == testA.Kind);

            var secondTest = tests[1];
            Assert.That(secondTest.Name == testB.Name);
            Assert.That(secondTest.Kind == testB.Kind);
            Assert.That(secondTest.StackTrace == testB.StackTrace);

            var thirdTest = tests[2];
            Assert.That(thirdTest.Name == testC.Name);
            Assert.That(thirdTest.Kind == testC.Kind);

            var fourthTest = tests[3];
            Assert.That(fourthTest.Name == dummyTestC.Name);
            Assert.That(fourthTest.Kind == dummyTestC.Kind);

        }      

    }
}
