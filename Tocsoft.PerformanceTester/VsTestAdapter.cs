using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Tocsoft.PerformanceTester
{


    [FileExtension(".dll")]
    [DefaultExecutorUri(ExecutorUri)]
    [ExtensionUri(ExecutorUri)]
    public class VsTestAdapter : ITestExecutor, ITestDiscoverer
    {
        public static TestProperty MethodNameProperty = TestProperty.Register("Tocsoft.PerformanceTesting.MethodName", "Performance Test - Method Name", typeof(string), typeof(VsTestAdapter));
        public static TestProperty TypeNameProperty = TestProperty.Register("Tocsoft.PerformanceTesting.TypeName", "Performance Test - Type Name", typeof(string), typeof(VsTestAdapter));

        internal const string ExecutorUri = "executor://Tocsoft.PerformanceTester/1";

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            foreach (var t in DiscoverTests(sources))
            {
                discoverySink.SendTestCase(t);
            }
        }

        private IEnumerable<TestCase> DiscoverTests(IEnumerable<string> sources)
        {
            foreach (var s in sources)
            {
                var assembly = Assembly.LoadFrom(s);

                var types = assembly
                                .GetTypes();
                foreach (var t in types)
                {
                    if (t.IsAbstract)
                    {
                        continue;
                    }

                    var methods = t.GetRuntimeMethods();

                    foreach (var m in methods)
                    {
                        var testCaseFactories = m.GetCustomAttributes(true)
                                            .OfType<IPerformanceTestCaseFactory>();
                        foreach (var f in testCaseFactories)
                        {
                            foreach (var c in f.Build(m))
                            {
                                var vase = new TestCase(c.Name, new Uri(ExecutorUri), s)
                                {
                                    Id = c.Id,
                                    DisplayName = c.DisplayName,
                                    Traits = {
                                        new Trait("Performance Test", "true")
                                    }
                                };
                                vase.SetPropertyValue(MethodNameProperty, m.Name);
                                vase.SetPropertyValue(TypeNameProperty, t.FullName);

                                yield return vase;
                            }
                        }
                    }
                }
            }
        }

        public void Cancel()
        {
            // TODO track executing test runs and enable cancelation tokens thru out 
            throw new NotImplementedException();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var logger = new TestLogger(frameworkHandle);
            var settings = new AdapterSettings(logger);
            settings.Load(runContext.RunSettings.SettingsXml);
            logger.InitSettings(settings);

            frameworkHandle.EnableShutdownAfterTestRun = true;
            var toRun = Convert(tests);

            var missing = tests.Except(toRun.Select(x => x.testCase));
            foreach (var m in missing)
            {
                frameworkHandle.RecordEnd(m, TestOutcome.NotFound);
            }
            // generate report details for all runs etc of all calls

            // parallel etc in here

            foreach (var t in toRun)
            {
                var testResult = new TestResult(t.testCase);
                if (t.perfTest.Skipped)
                {
                    testResult.Outcome = TestOutcome.Skipped;
                    frameworkHandle.RecordResult(testResult);
                    //frameworkHandle.RecordEnd(t.testCase, testResult.Outcome);
                    continue;
                }
                frameworkHandle.RecordStart(t.testCase);
                using (var context = TestContext.Start(settings))
                {
                    var sw = Stopwatch.StartNew();
                    var task = t.perfTest.ExecuteAsync(context);

                    Task.WaitAll(task);
                    sw.Stop();
                    var result = task.Result;

                    // process the results here
                    testResult.Duration = sw.Elapsed;


                    var runs = result.Where(x => x.IsWarmup == false).Select(x => x.Duration);
                    var warmups = result.Where(x => x.IsWarmup == true).Select(x => x.Duration);

                    var mean = TimeSpanStatistics.Mean(runs);
                    var standardDeviation = TimeSpanStatistics.StandardDeviation(runs);

                    // format a table of output results here
                    var msg = $@"Warm up Count : {warmups.Count()}
Warm up Duration : {new TimeSpan(warmups.Sum(x => x.Ticks))}
Executed : {runs.Count()}
Mean Duration: {mean}
Standard Deviation Duration: {standardDeviation}";

                    testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, msg));
                    testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, context.Output));

                    foreach (var r in result.Where(x => !string.IsNullOrWhiteSpace(x.Output)))
                    {
                        testResult.Messages.Add(new TestResultMessage(TestResultMessage.AdditionalInfoCategory, r.Output));
                    }

                    var errors = result.Select(x => x.Error).Where(x => x != null).ToList();
                    if (errors.Any())
                    {
                        testResult.ErrorStackTrace = string.Join("\n\n-------\n\n", errors.Select(x => x.StackTrace));
                        testResult.ErrorMessage = string.Join("\n\n-------\n\n", errors.Select(x => x.Message));

                        testResult.Outcome = TestOutcome.Failed;
                        frameworkHandle.RecordResult(testResult);
                        //frameworkHandle.RecordEnd(t.testCase, testResult.Outcome);
                    }
                    else
                    {
                        testResult.Outcome = TestOutcome.Passed;

                        frameworkHandle.RecordResult(testResult);
                        //frameworkHandle.RecordEnd(t.testCase, testResult.Outcome);
                    }
                }
            }
        }

        private IEnumerable<(PerformanceTestCase perfTest, TestCase testCase)> Convert(IEnumerable<TestCase> tests)
        {
            // convert testcase back to PerformanceTestCase
            foreach (var assemblyGroup in tests.GroupBy(x => x.Source))
            {
                var assembly = Assembly.LoadFrom(assemblyGroup.Key);

                //vase.SetPropertyValue(MethodNameProperty, m.Name);
                //vase.SetPropertyValue(TypeNameProperty, t.FullName);
                var typeGroups = assemblyGroup.GroupBy(x => x.GetPropertyValue<string>(TypeNameProperty, null));

                foreach (var typeGroup in typeGroups)
                {
                    var type = assembly.GetType(typeGroup.Key);
                    foreach (var methodGroup in typeGroup.GroupBy(m => m.GetPropertyValue(MethodNameProperty, "")))
                    {
                        var testsToRun = methodGroup.ToDictionary(x => x.Id, x => x);
                        var method = type.GetMethod(methodGroup.Key);

                        var testCaseFactories = method.GetCustomAttributes(true)
                                          .OfType<IPerformanceTestCaseFactory>();

                        foreach (var f in testCaseFactories)
                        {
                            foreach (var c in f.Build(method))
                            {
                                if (testsToRun.TryGetValue(c.Id, out var tc))
                                {

                                    yield return (c, tc);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var tests = this.DiscoverTests(sources);
            this.RunTests(tests, runContext, frameworkHandle);
        }
    }
}
