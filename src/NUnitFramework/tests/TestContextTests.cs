﻿// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;
using System.Collections.Generic;
#if ASYNC
using System.Threading.Tasks;
#endif
using NUnit.Framework.Interfaces;
using NUnit.TestData.TestContextData;
using NUnit.TestUtilities;
using NUnit.Framework.Internal;

namespace NUnit.Framework.Tests
{
    [TestFixture]
    public class TestContextTests
    {
        private TestContext _setupContext;

        private string _name;

#if !PORTABLE
        private string _testDirectory;
#endif
        private string _workDirectory;

        public TestContextTests()
        {
            _name = TestContext.CurrentContext.Test.Name;

#if !PORTABLE
            _testDirectory = TestContext.CurrentContext.TestDirectory;
#endif
            _workDirectory = TestContext.CurrentContext.WorkDirectory;
        }

        [SetUp]
        public void SaveSetUpContext()
        {
            _setupContext = TestContext.CurrentContext;
        }

#if !PORTABLE
        [Test]
        public void ConstructorCanAccessTestDirectory()
        {
            Assert.That(_testDirectory, Is.Not.Null);
        }

        [TestCaseSource("MySource")]
        public void TestCaseSourceCanAccessTestDirectory(string testDirectory)
        {
            Assert.That(testDirectory, Is.EqualTo(_testDirectory));
        }

        static IEnumerable<string> MySource()
        {
            yield return TestContext.CurrentContext.TestDirectory;
        }
#endif

        [Test]
        public void ConstructorAccessWorkDirectory()
        {
            Assert.That(_workDirectory, Is.Not.Null);
        }

        [Test]
        public void TestCanAccessItsOwnName()
        {
            Assert.That(TestContext.CurrentContext.Test.Name, Is.EqualTo("TestCanAccessItsOwnName"));
        }

        [Test]
        public void ConstructorCanAccessFixtureName()
        {
            Assert.That(_name, Is.EqualTo("TestContextTests"));
        }

        [Test]
        public void SetUpCanAccessTestName()
        {
            Assert.That(_setupContext.Test.Name, Is.EqualTo(TestContext.CurrentContext.Test.Name));
        }

        [TestCase(5)]
        public void TestCaseCanAccessItsOwnName(int x)
        {
            Assert.That(TestContext.CurrentContext.Test.Name, Is.EqualTo("TestCaseCanAccessItsOwnName(5)"));
        }

        [Test]
        public void SetUpCanAccessTestFullName()
        {
            Assert.That(_setupContext.Test.FullName, Is.EqualTo(TestContext.CurrentContext.Test.FullName));
        }

        [Test]
        public void TestCanAccessItsOwnFullName()
        {
            Assert.That(TestContext.CurrentContext.Test.FullName,
                Is.EqualTo("NUnit.Framework.Tests.TestContextTests.TestCanAccessItsOwnFullName"));
        }

        [TestCase(42)]
        public void TestCaseCanAccessItsOwnFullName(int x)
        {
            Assert.That(TestContext.CurrentContext.Test.FullName,
                Is.EqualTo("NUnit.Framework.Tests.TestContextTests.TestCaseCanAccessItsOwnFullName(42)"));
        }

        [Test]
        public void SetUpCanAccessTestMethodName()
        {
            Assert.That(_setupContext.Test.MethodName, Is.EqualTo(TestContext.CurrentContext.Test.MethodName));
        }

        [Test]
        public void TestCanAccessItsOwnMethodName()
        {
            Assert.That(TestContext.CurrentContext.Test.MethodName, Is.EqualTo("TestCanAccessItsOwnMethodName"));
        }

        [TestCase(5)]
        public void TestCaseCanAccessItsOwnMethodName(int x)
        {
            Assert.That(TestContext.CurrentContext.Test.MethodName, Is.EqualTo("TestCaseCanAccessItsOwnMethodName"));
        }

        [Test]
        public void TestCanAccessItsOwnId()
        {
            Assert.That(TestContext.CurrentContext.Test.ID, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void SetUpCanAccessTestId()
        {
            Assert.That(_setupContext.Test.ID, Is.EqualTo(TestContext.CurrentContext.Test.ID));
        }

        [Test]
        [Property("Answer", 42)]
        public void TestCanAccessItsOwnProperties()
        {
            Assert.That(TestContext.CurrentContext.Test.Properties.Get("Answer"), Is.EqualTo(42));
        }

        [Test]
        public void TestCanAccessWorkDirectory()
        {
            string workDirectory = TestContext.CurrentContext.WorkDirectory;
            Assert.NotNull(workDirectory);
            // SL tests may be running on the desktop
#if !PORTABLE
            Assert.That(Directory.Exists(workDirectory), string.Format("Directory {0} does not exist", workDirectory));
#endif
        }

        [Test]
        public void TestCanAccessTestState_PassingTest()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>Inconclusive=>Passed"));
        }

        [Test]
        public void TestCanAccessTestState_FailureInSetUp()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            fixture.setUpFailure = true;
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>=>Failed"));
        }

        [Test]
        public void TestCanAccessTestState_FailingTest()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            fixture.testFailure = true;
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>Inconclusive=>Failed"));
        }

        [Test]
        public void TestCanAccessTestState_IgnoredInSetUp()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            fixture.setUpIgnore = true;
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>=>Skipped:Ignored"));
        }

        [Test]
        public void TestContextStoresFailureInfoForTearDown()
        {
            var fixture = new TestTestContextInTearDown();
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.FailCount, Is.EqualTo(1));
            Assert.That(fixture.Message, Is.EqualTo("Deliberate failure"));
            Assert.That(fixture.StackTrace, Does.Contain("NUnit.TestData.TestContextData.TestTestContextInTearDown.FailingTest"));
        }

        [Test]
        public void TestContextStoresFailureInfoForOneTimeTearDown()
        {
            var fixture = new TestTestContextInOneTimeTearDown();
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.PassCount, Is.EqualTo(2));
            Assert.That(fixture.FailCount, Is.EqualTo(1));
            Assert.That(fixture.WarningCount, Is.EqualTo(0));
            Assert.That(fixture.SkipCount, Is.EqualTo(3));
            Assert.That(fixture.InconclusiveCount, Is.EqualTo(4));
            Assert.That(fixture.Message, Is.EqualTo(TestResult.CHILD_ERRORS_MESSAGE));
            Assert.That(fixture.StackTrace, Is.Null);
        }
#if ASYNC

#if PORTABLE
        [Ignore("Not implemented yet as TestExecutionContext uses TLS in portable implementation")]
#endif
        [Test]
        public async Task TestContextOut_ShouldFlowWithAsyncExecution()
        {
            var expected = TestContext.Out;
            await YieldAsync();
            Assert.AreSame(expected, TestContext.Out);
        }

#if PORTABLE
        [Ignore("Not implemented yet as TestExecutionContext uses TLS in portable implementation")]
#endif
        [Test]
        public async Task TestExecutionContextCurrentContext_ShouldFlowWithAsyncExecution()
        {
            var expected = TestExecutionContext.CurrentContext;
            await YieldAsync();
            Assert.AreSame(expected, TestExecutionContext.CurrentContext);
        }

#if PORTABLE
        [Ignore("Not implemented yet as TestExecutionContext uses TLS in portable implementation")]
#endif
        [Test]
        public async Task TestExecutionContextCurrentContext_ShouldFlowWithParallelAsyncExecution()
        {
            var expected = TestExecutionContext.CurrentContext;
            var parallelResult = await WhenAllAsync(YieldAndReturnContext(), YieldAndReturnContext());

            Assert.AreSame(expected, TestExecutionContext.CurrentContext);
            Assert.AreSame(expected, parallelResult[0]);
            Assert.AreSame(expected, parallelResult[1]);
        }

#if PORTABLE
        [Ignore("Not implemented yet as TestExecutionContext uses TLS in portable implementation")]
#endif
        [Test]
        public async Task TestExecutionContext_ShouldFlowWithAsyncExecution_ExecutedInParallel()
        {
            Func<Task<TestExecutionContext>> functionWithSeparateContext = async () =>
            {
                await YieldAsync(); //to start both functions
                TestExecutionContext.ClearCurrentContext(); //establish new context
                var expected = TestExecutionContext.CurrentContext;
                await DelayAsync(300);
                Assert.AreSame(expected, TestExecutionContext.CurrentContext); //ensure that is still the same within given function call
                return expected;
            };

            var results = await WhenAllAsync(functionWithSeparateContext(), functionWithSeparateContext());
            // both calls have to be successful but have different context
            Assert.AreNotSame(results[0], results[1]);
        }

#if PORTABLE
        [Ignore("Not implemented yet as TestExecutionContext uses TLS in portable implementation")]
#endif
        [Test]
        public async Task TestContextWriteLine_ShouldNotThrow_WhenExecutedFromAsyncMethod()
        {
            Assert.DoesNotThrow(TestContext.WriteLine);
            await YieldAsync();
            Assert.DoesNotThrow(TestContext.WriteLine);
        }

#if PORTABLE
        [Ignore("Not implemented yet as TestExecutionContext uses TLS in portable implementation")]
#endif
        [Test]
        public void TestContextOut_ShouldBeAvailableFromOtherThreads()
        {
            var isTestContextOutAvailable = false;
            Task.Factory.StartNew(() =>
            {
                isTestContextOutAvailable = TestContext.Out != null;
            }).Wait();
            Assert.True(isTestContextOutAvailable);
        }

        private async Task YieldAsync()
        {
#if NET_4_0
            await TaskEx.Yield();
#else
            await Task.Yield();
#endif
        }

        private Task<T[]> WhenAllAsync<T>(params Task<T>[] tasks)
        {
#if NET_4_0
            return TaskEx.WhenAll(tasks);
#else
            return Task.WhenAll(tasks);
#endif
        }

        private Task DelayAsync(int milliseconds)
        {
#if NET_4_0
            return TaskEx.Delay(milliseconds);
#else
            return Task.Delay(milliseconds);
#endif
        }

        private async Task<TestExecutionContext> YieldAndReturnContext()
        {
            await YieldAsync();
            return TestExecutionContext.CurrentContext;
        }
#endif
    }

    [TestFixture]
    public class TestContextTearDownTests
    {
        private const int THE_MEANING_OF_LIFE = 42;

        [Test]
        public void TestTheMeaningOfLife()
        {
            Assert.That(THE_MEANING_OF_LIFE, Is.EqualTo(42));
        }

        [TearDown]
        public void TearDown()
        {
            TestContext context = TestContext.CurrentContext;
            Assert.That(context, Is.Not.Null);
            Assert.That(context.Test, Is.Not.Null);
            Assert.That(context.Test.Name, Is.EqualTo("TestTheMeaningOfLife"));
            Assert.That(context.Result, Is.Not.Null);
            Assert.That(context.Result.Outcome, Is.EqualTo(ResultState.Success));
            Assert.That(context.Result.PassCount, Is.EqualTo(1));
            Assert.That(context.Result.FailCount, Is.EqualTo(0));
#if !PORTABLE
            Assert.That(context.TestDirectory, Is.Not.Null);
            Assert.That(context.WorkDirectory, Is.Not.Null);
#endif
        }
    }

    [TestFixture]
    public class TestContextOneTimeTearDownTests
    {
        [Test]
        public void TestTruth()
        {
            Assert.That(true, Is.True);
        }

        [Test]
        public void TestFalsehood()
        {
            Assert.That(false, Is.False);
        }

        [Test, Explicit]
        public void TestExplicit()
        {
            Assert.Pass("Always passes if you run it!");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestContext context = TestContext.CurrentContext;
            Assert.That(context, Is.Not.Null);
            Assert.That(context.Test, Is.Not.Null);
            Assert.That(context.Test.Name, Is.EqualTo("TestContextOneTimeTearDownTests"));
            Assert.That(context.Result, Is.Not.Null);
            Assert.That(context.Result.Outcome, Is.EqualTo(ResultState.Success));
            Assert.That(context.Result.PassCount, Is.EqualTo(2));
            Assert.That(context.Result.FailCount, Is.EqualTo(0));
            Assert.That(context.Result.SkipCount, Is.EqualTo(1));
        }
    }
}
