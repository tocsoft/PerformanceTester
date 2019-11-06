using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace Tocsoft.PerformanceTester
{
    public class AdapterSettings
    {
        private const string RANDOM_SEED_FILE = "tocsoft_perftestings_random_seed.tmp";
        private readonly TestLogger _logger;
        #region Constructor

        public AdapterSettings(TestLogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Properties - General

        public int MaxCpuCount { get; private set; }

        public string ResultsDirectory { get; private set; }

        public string TargetPlatform { get; private set; }

        public string TargetFrameworkVersion { get; private set; }

        public string TestAdapterPaths { get; private set; }

        /// <summary>
        /// If false, an adapter need not parse symbols to provide test case file, line number
        /// </summary>
        public bool CollectSourceInformation { get; private set; }

        /// <summary>
        /// If true, an adapter shouldn't create appdomains to run tests
        /// </summary>
        public bool DisableAppDomain { get; private set; }

        /// <summary>
        /// If true, an adapter should disable any test case parallelization
        /// </summary>
        public bool DisableParallelization { get; private set; }

        /// <summary>
        /// True if test run is triggered in an IDE/Editor context.
        /// </summary>
        public bool DesignMode { get; private set; }

        #endregion

        #region Properties - TestRunParameters

        public IReadOnlyDictionary<string, string> TestProperties { get; private set; }

        #endregion

        #region Properties - NUnit Specific

        public string InternalTraceLevel { get; private set; }

        public string WorkDirectory { get; private set; }
        public string TestOutputXml { get; private set; }
        public bool UseTestOutputXml => !string.IsNullOrEmpty(TestOutputXml);
        public int DefaultTimeout { get; private set; }

        public int NumberOfTestWorkers { get; private set; }

        public bool ShadowCopyFiles { get; private set; }

        public int Verbosity { get; private set; }

        public bool UseVsKeepEngineRunning { get; private set; }

        public string BasePath { get; private set; }

        public string PrivateBinPath { get; private set; }

        public int? RandomSeed { get; private set; }
        public bool RandomSeedSpecified { get; private set; }

        public bool CollectDataForEachTestSeparately { get; private set; }

        public bool InProcDataCollectorsAvailable { get; private set; }

        public bool SynchronousEvents { get; private set; }

        public string DomainUsage { get; private set; }

        public bool ShowInternalProperties { get; private set; }

        public bool DumpXmlTestDiscovery { get; private set; }

        public bool DumpXmlTestResults { get; private set; }

        /// <summary>
        ///  Syntax documentation <see cref="https://github.com/nunit/docs/wiki/Template-Based-Test-Naming"/>
        /// </summary>
        public string DefaultTestNamePattern { get; set; }

        public bool PreFilter { get; private set; }



        #endregion

        #region Public Methods

        public void Load(IDiscoveryContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Load called with null context");

            Load(context.RunSettings?.SettingsXml);
        }

        public void Load(string settingsXml)
        {
            if (string.IsNullOrEmpty(settingsXml))
                settingsXml = "<RunSettings />";

            // Visual Studio already gives a good error message if the .runsettings
            // file is poorly formed, so we don't need to do anything more.
            var doc = new XmlDocument();
            doc.LoadXml(settingsXml);

            var nunitNode = doc.SelectSingleNode("RunSettings/NUnit");
            Verbosity = GetInnerTextAsInt(nunitNode, nameof(Verbosity), 0);
            _logger.Verbosity = Verbosity;

            var runConfiguration = doc.SelectSingleNode("RunSettings/RunConfiguration");
            MaxCpuCount = GetInnerTextAsInt(runConfiguration, nameof(MaxCpuCount), -1);
            ResultsDirectory = GetInnerTextWithLog(runConfiguration, nameof(ResultsDirectory));
            TargetPlatform = GetInnerTextWithLog(runConfiguration, nameof(TargetPlatform));
            TargetFrameworkVersion = GetInnerTextWithLog(runConfiguration, nameof(TargetFrameworkVersion));
            TestAdapterPaths = GetInnerTextWithLog(runConfiguration, nameof(TestAdapterPaths));
            CollectSourceInformation = GetInnerTextAsBool(runConfiguration, nameof(CollectSourceInformation), true);
            DisableAppDomain = GetInnerTextAsBool(runConfiguration, nameof(DisableAppDomain), false);
            DisableParallelization = GetInnerTextAsBool(runConfiguration, nameof(DisableParallelization), false);
            DesignMode = GetInnerTextAsBool(runConfiguration, nameof(DesignMode), false);
            CollectDataForEachTestSeparately =
                GetInnerTextAsBool(runConfiguration, nameof(CollectDataForEachTestSeparately), false);

            var testProperties = new Dictionary<string, string>();
            foreach (XmlNode node in doc.SelectNodes("RunSettings/TestRunParameters/Parameter"))
            {
                var key = node.Attributes["name"]?.Value;
                var value = node.Attributes["value"]?.Value;
                if (key != null && value != null)
                    testProperties.Add(key, value);
            }

            TestProperties = new SafeReadonlyDictionary<string, string>(testProperties);

            // NUnit settings
            InternalTraceLevel = GetInnerTextWithLog(nunitNode, nameof(InternalTraceLevel), "Off", "Error", "Warning",
                "Info", "Verbose", "Debug");
            WorkDirectory = GetInnerTextWithLog(nunitNode, nameof(WorkDirectory));
            DefaultTimeout = GetInnerTextAsInt(nunitNode, nameof(DefaultTimeout), 0);
            NumberOfTestWorkers = GetInnerTextAsInt(nunitNode, nameof(NumberOfTestWorkers), -1);
            ShadowCopyFiles = GetInnerTextAsBool(nunitNode, nameof(ShadowCopyFiles), false);
            UseVsKeepEngineRunning = GetInnerTextAsBool(nunitNode, nameof(UseVsKeepEngineRunning), false);
            BasePath = GetInnerTextWithLog(nunitNode, nameof(BasePath));
            PrivateBinPath = GetInnerTextWithLog(nunitNode, nameof(PrivateBinPath));
            TestOutputXml = GetInnerTextWithLog(nunitNode, nameof(TestOutputXml));
            RandomSeed = GetInnerTextAsNullableInt(nunitNode, nameof(RandomSeed));
            RandomSeedSpecified = RandomSeed.HasValue;
            if (!RandomSeedSpecified)
                RandomSeed = new Random().Next();
            DefaultTestNamePattern = GetInnerTextWithLog(nunitNode, nameof(DefaultTestNamePattern));
            ShowInternalProperties = GetInnerTextAsBool(nunitNode, nameof(ShowInternalProperties), false);
            DumpXmlTestDiscovery = GetInnerTextAsBool(nunitNode, nameof(DumpXmlTestDiscovery), false);
            DumpXmlTestResults = GetInnerTextAsBool(nunitNode, nameof(DumpXmlTestResults), false);
            PreFilter = GetInnerTextAsBool(nunitNode, nameof(PreFilter), false);

#if DEBUG
            // Force Verbosity to 1 under Debug
            Verbosity = 1;
#endif

            var inProcDataCollectorNode =
                doc.SelectSingleNode("RunSettings/InProcDataCollectionRunSettings/InProcDataCollectors");
            InProcDataCollectorsAvailable = inProcDataCollectorNode != null &&
                                            inProcDataCollectorNode.SelectNodes("InProcDataCollector").Count > 0;

            // Older versions of VS do not pass the CollectDataForEachTestSeparately configuration together with the LiveUnitTesting collector.
            // However, the adapter is expected to run in CollectDataForEachTestSeparately mode.
            // As a result for backwards compatibility reasons enable CollectDataForEachTestSeparately mode whenever LiveUnitTesting collector is being used.
            var hasLiveUnitTestingDataCollector =
                inProcDataCollectorNode?.SelectSingleNode(
                    "InProcDataCollector[@uri='InProcDataCollector://Microsoft/LiveUnitTesting/1.0']") != null;

            // TestPlatform can opt-in to run tests one at a time so that the InProcDataCollectors can collect the data for each one of them separately.
            // In that case, we need to ensure that tests do not run in parallel and the test started/test ended events are sent synchronously.
            if (CollectDataForEachTestSeparately || hasLiveUnitTestingDataCollector)
            {
                NumberOfTestWorkers = 0;
                SynchronousEvents = true;
                if (Verbosity >= 4)
                {
                    if (!InProcDataCollectorsAvailable)
                    {
                        _logger.Info(
                            "CollectDataForEachTestSeparately is set, which is used to make InProcDataCollectors collect data for each test separately. No InProcDataCollectors can be found, thus the tests will run slower unnecessarily.");
                    }
                }
            }

            // If DisableAppDomain settings is passed from the testplatform, set the DomainUsage to None.
            if (DisableAppDomain)
            {
                DomainUsage = "None";
            }

            // Update NumberOfTestWorkers based on the DisableParallelization and NumberOfTestWorkers from runsettings.
            UpdateNumberOfTestWorkers();


            string ValidatedPath(string path, string purpose)
            {
                try
                {
                    if (string.IsNullOrEmpty(WorkDirectory))
                    {
                        return Path.GetFullPath(path);
                    }

                    if (Path.IsPathRooted(path))
                    {
                        return Path.GetFullPath(path);
                    }
                    return Path.GetFullPath(Path.Combine(WorkDirectory, path));
                }
                catch (Exception)
                {
                    _logger.Error($"   Invalid path for {purpose}: {path}");
                    throw;
                }
            }
        }

        public void SaveRandomSeed(string dirname)
        {
            try
            {
                var path = Path.Combine(dirname, RANDOM_SEED_FILE);
                File.WriteAllText(path, RandomSeed.Value.ToString());
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to save random seed.", ex);
            }
        }

        public void RestoreRandomSeed(string dirname)
        {
            var fullpath = Path.Combine(dirname, RANDOM_SEED_FILE);
            if (!File.Exists(fullpath))
                return;
            try
            {
                string value = File.ReadAllText(fullpath);
                RandomSeed = int.Parse(value);
            }
            catch (Exception ex)
            {
                _logger.Warning("Unable to restore random seed.", ex);
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateNumberOfTestWorkers()
        {
            // Overriding the NumberOfTestWorkers if DisableParallelization is true.
            if (DisableParallelization && NumberOfTestWorkers < 0)
            {
                NumberOfTestWorkers = 0;
            }
            else if (DisableParallelization && NumberOfTestWorkers > 0)
            {
                if (_logger.Verbosity > 0)
                {
                    _logger.Warning(string.Format("DisableParallelization:{0} & NumberOfTestWorkers:{1} are conflicting settings, hence not running in parallel", DisableParallelization, NumberOfTestWorkers));
                }
                NumberOfTestWorkers = 0;
            }
        }

        private string GetInnerTextWithLog(XmlNode startNode, string xpath, params string[] validValues)
        {
            return GetInnerText(startNode, xpath, true, validValues);
        }


        private string GetInnerText(XmlNode startNode, string xpath, bool log, params string[] validValues)
        {
            string val = null;
            var targetNode = startNode?.SelectSingleNode(xpath);
            if (targetNode != null)
            {
                val = targetNode.InnerText;

                if (validValues != null && validValues.Length > 0)
                {
                    foreach (string valid in validValues)
                        if (string.Compare(valid, val, StringComparison.OrdinalIgnoreCase) == 0)
                            return valid;

                    throw new ArgumentException($"Invalid value {val} passed for element {xpath}.");
                }


            }
            if (log)
                Log(xpath, val);

            return val;
        }

        private int GetInnerTextAsInt(XmlNode startNode, string xpath, int defaultValue)
        {
            var temp = GetInnerTextAsNullableInt(startNode, xpath, false);
            var res = defaultValue;
            if (temp != null)
                res = temp.Value;
            Log(xpath, res);
            return res;
        }

        private int? GetInnerTextAsNullableInt(XmlNode startNode, string xpath, bool log = true)
        {
            string temp = GetInnerText(startNode, xpath, log);
            int? res = null;
            if (!string.IsNullOrEmpty(temp))
                res = int.Parse(temp);
            if (log)
                Log(xpath, res);
            return res;
        }

        private bool GetInnerTextAsBool(XmlNode startNode, string xpath, bool defaultValue)
        {
            string temp = GetInnerText(startNode, xpath, false);
            bool res = defaultValue;
            if (!string.IsNullOrEmpty(temp))
                res = bool.Parse(temp);
            Log(xpath, res);
            return res;
        }

        private void Log<T>(string xpath, T res)
        {
            if (Verbosity >= 4)
            {
                _logger.Info($"Setting: {xpath} = {res}");
            }
        }
        #endregion

        private class SafeReadonlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        {
            private readonly IReadOnlyDictionary<TKey, TValue> dict;

            public SafeReadonlyDictionary(IDictionary<TKey, TValue> dict)
            {
                this.dict = new ReadOnlyDictionary<TKey, TValue>(dict);
            }

            public TValue this[TKey key]
            {
                get
                {
                    if (this.dict.TryGetValue(key, out var val))
                    {
                        return val;
                    }

                    return default(TValue);
                }
            }

            public IEnumerable<TKey> Keys => this.dict.Keys;

            public IEnumerable<TValue> Values => this.dict.Values;

            public int Count => this.dict.Count;

            public bool ContainsKey(TKey key)
            {
                return this.dict.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return this.dict.GetEnumerator();
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return this.dict.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.dict.GetEnumerator();
            }
        }
    }
}
