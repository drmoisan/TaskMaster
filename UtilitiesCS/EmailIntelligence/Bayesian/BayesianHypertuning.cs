using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Properties;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class BayesianHypertuning
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Globals

        public BayesianHypertuning(IApplicationGlobals globals)
        {
            _globals = globals;
        }

        private IApplicationGlobals _globals;
        public IApplicationGlobals Globals { get => _globals; }

        #endregion Constructors and Globals

        #region Performance Record Types

        public record ConfusionMatrixCounts()
        {
            public string Class { get; set; }
            public int TP { get; set; }
            public int FP { get; set; }
            public int FN { get; set; }
            public int TN { get; set; }
        }

        public record TestScores()
        {
            public string Class { get; set; }
            public int TP { get; set; }
            public int FP { get; set; }
            public int FN { get; set; }
            public int TN { get; set; }
            public double Precision { get; set; }
            public double Recall { get; set; }
            public double F1 { get; set; }
        }

        public record VerboseTestResult()
        {
            public string Actual { get; set; }
            public string Predicted { get; set; }
            public VerboseTestDetail[] Details { get; set; }
        }

        public record ClassificationErrors()
        {
            public string Class { get; set; }
            public VerboseTestDetail[] FalsePositives { get; set; }
            public VerboseTestDetail[] FalseNegatives { get; set; }
        }

        public record VerboseTestDetail() 
        { 
            public string Actual { get; set; }
            public string Predicted { get; set; }
            public MinedMailInfo Source { get; set; }
            public (string Token, double TokenProbability)[] Drivers { get; set; }
            public double Probability { get; set; }
        }
        
        public record TestResult()
        {
            public string Actual { get; set; }
            public string Predicted { get; set; }
            public int Count { get; set; }
        }

        public record TestOutcome()
        {
            public string Actual { get; set; }
            public string Predicted { get; set; }
            public int SourceIndex { get; set; }
        }

        #endregion Performance Record Types

        #region Main Testing Methods

        public async Task TestFolderClassifierAsync()
        {
            var dataMiner = new EmailDataMiner(Globals);
            await TestFolderClassifierAsync(dataMiner);
        }

        public async Task TestFolderClassifierAsync(EmailDataMiner dataMiner = null, MinedMailInfo[] collection = null)
        {
            (dataMiner, collection, var folderPaths, var ppkg) = await ReloadIfNullAsync(dataMiner, collection, null);

            var (train, test) = SplitTestTrain(collection, 0.75, ppkg);

            var classifierGroup = await BuildClassifierAsync(dataMiner, ppkg, train);
                        
            TestOutcome[] testOutcomes = await RunClassifierTestAsync(test, classifierGroup, ppkg.SpawnChild(40));
            TestResult[] testResults = GroupOutcomes(testOutcomes);

            ppkg.ProgressTrackerPane.Increment(0, "Building Folder Classifier -> Building Confusion Matrix and Calculating Scores");
            
            ConfusionMatrixCounts[] counts = CountHitsMisses(folderPaths, testResults);
            
            IEnumerable<TestScores> scores = await CalculateTestScoresAsync(counts);
            
            await SaveScoresAsync(scores);
            
            await BuildConfusionMatrixAsync(folderPaths, testResults);

            ppkg.ProgressTrackerPane.Report(100, "Operation Complete");

        }
        
        //public async Task DiagnosePoorPerformance()
        //{
        //    var dataMiner = new EmailDataMiner(Globals);
            
        //}

        public async Task GetConfusionDriversAsync(
            MinedMailInfo[] testSource = null, 
            TestOutcome[] testOutcomes = null,
            BayesianClassifierGroup classifierGroup = null,
            ProgressPackage ppkg = null)
        {
            (testOutcomes, testSource, classifierGroup, ppkg) = await ReloadIfNullAsync(
                testOutcomes, testSource, classifierGroup, ppkg);

            var testScores = await DeserializeAsync<TestScores[]>("TestScores");
                        
            ppkg.ProgressTrackerPane.Increment(10, "Getting Confusion Outcomes and Counts");
            TestOutcome[] confusedOutcomes = testOutcomes.Where(x => x.Actual != x.Predicted).ToArray();
            TestResult[] confusedCounts = GroupOutcomes(confusedOutcomes);

            ppkg.ProgressTrackerPane.Increment(10, "Extracting Confusion Drivers");
            ClassificationErrors[] errors = await DiagnosePoorPerformance(testSource, classifierGroup, 
                ppkg.SpawnChild(100 - (int)ppkg.ProgressTrackerPane.Progress), confusedOutcomes, testScores);

            SerializeAndSave(errors, "ClassificationErrors");
            _globals.AF.ProgressPane.Visible = false;
            
        }

        #endregion Main Testing Methods

        #region Classifier Performance Testing

        public async Task<BayesianClassifierGroup> BuildClassifierAsync(EmailDataMiner dataMiner, ProgressPackage ppkg, MinedMailInfo[] train)
        {
            ppkg.ProgressTrackerPane.Increment(10, "Building Folder Classifier -> Create Classifier Group");

            var classifierGroup = await dataMiner.CreateClassifierGroupAsync(train);

            ppkg.ProgressTrackerPane.Increment(10, "Building Folder Classifier -> Building Classifiers");

            await dataMiner.BuildFolderClassifiersAsync(classifierGroup, train, await new ProgressPackage().InitializeAsync(
                ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(20), ppkg.StopWatch));

            SerializeAndSave(classifierGroup, "TestClassifierGroup");

            return classifierGroup;
        }

        public async Task<TestOutcome[]> RunClassifierTestAsync(
            MinedMailInfo[] test, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg)
        {
            var paneState = _globals.AF.ProgressPane.Visible;
            _globals.AF.ProgressPane.Visible = true;

            ppkg?.ProgressTrackerPane.Report(0, "Testing Classifiers");
            TestOutcome[] testOutcomes = null;
            int completed = 0;
            int count = test.Count();
            double remainingSeconds = 0;
            double secondsPerItem = 0;
            double elapsedSeconds = 0;
            var sw = await Task.Run(() => new SegmentStopWatch().Start());
            
            var testTask = Task.Run(() => testOutcomes =
                [
                    .. test
                    .Select((MinedMail, Index) => (MinedMail, Index))
                    .AsParallel()
                    .Select(x => new TestOutcome
                    {
                        SourceIndex = x.Index,
                        Actual = x.MinedMail.FolderInfo.RelativePath,
                        Predicted = classifierGroup.Classify(x.MinedMail.Tokens.GroupAndCount()).First().Class
                    })
                    .WithAction(() =>
                    {
                        Interlocked.Increment(ref completed);
                        
                        var msg = GetProgressMessage(completed, count, sw, ref secondsPerItem, ref remainingSeconds);
                        ppkg.ProgressTrackerPane.Report(
                            ((int)(((double)completed / count) * 100),
                            $"Testing Classifiers -> {msg}"));
                        elapsedSeconds = sw.Elapsed.TotalSeconds;
                    }),
                ],
                ppkg.Cancel);

            TimerWrapper timer = null;
            var timerTask = Task.Run(() =>
            {
                timer = new TimerWrapper(TimeSpan.FromSeconds(1));
                timer.Elapsed += (sender, e) =>
                {
                    if (count > 0)
                    {
                        var msg = AdjustProgressTimer(completed, count, sw, ref secondsPerItem, ref remainingSeconds, ref elapsedSeconds);
                        ppkg.ProgressTrackerPane.Report(
                            ((int)(((double)completed / count) * 100),
                            $"Testing Classifiers -> {msg}"));
                    }
                };
                timer.AutoReset = true;
                timer.StartTimer();
            });

            try
            {
                await timerTask;
                await testTask;
                SerializeAndSave(testOutcomes, "TestOutcomes");
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            finally
            {
                timer.StopTimer();
                timer.Dispose();
                ppkg.ProgressTrackerPane.Report(100);
                _globals.AF.ProgressPane.Visible = paneState;
            }

            return testOutcomes;
        }

        public TestResult[] GroupOutcomes(TestOutcome[] outcomes)
        {
            TestResult[] testResults =
            [
                .. outcomes
                    .GroupBy(x => (x.Actual, x.Predicted))
                    .Select(x => new TestResult
                    {
                        Actual = x.Key.Actual,
                        Predicted = x.Key.Predicted,
                        Count = x.Count()
                    })
                    .OrderByDescending(x => x.Count)
            ];
            SerializeAndSave(testResults, "TestResults");
            return testResults;
        }

        public ConfusionMatrixCounts[] CountHitsMisses(List<string> folderPaths, TestResult[] testResults)
        {
            ConfusionMatrixCounts[] counts = folderPaths.Select(x => new ConfusionMatrixCounts
            {
                Class = x,
                TP = testResults.Count(y => y.Actual == x && y.Predicted == x),
                FP = testResults.Count(y => y.Actual != x && y.Predicted == x),
                FN = testResults.Count(y => y.Actual == x && y.Predicted != x),
                TN = testResults.Count(y => y.Actual != x && y.Predicted != x)
            }).ToArray();
            SerializeAndSave(counts, "ConfusionMatrixCounts");
            return counts;
        }

        public async Task<IEnumerable<TestScores>> CalculateTestScoresAsync(ConfusionMatrixCounts[] counts)
        {
            if (counts.IsNullOrEmpty())
            { counts = await DeserializeAsync<ConfusionMatrixCounts[]>("ConfusionMatrixCounts"); }
            var scores = counts.Select(x => new
            {
                x.Class,
                x.TP,
                x.FP,
                x.FN,
                x.TN,
                Precision = (x.TP + x.FP) != 0 ? x.TP / (double)(x.TP + x.FP) : 0,
                Recall = (x.TP + x.FN) != 0 ? x.TP / (double)(x.TP + x.FN) : 0,
            }).Select(x => new TestScores
            {
                Class = x.Class,
                TP = x.TP,
                FP = x.FP,
                FN = x.FN,
                TN = x.TN,
                Precision = x.Precision,
                Recall = x.Recall,
                F1 = (x.Precision + x.Recall) != 0 ? 2 * (x.Precision * x.Recall) / (x.Precision + x.Recall) : 0
            }).ToList();

            scores.Add(new TestScores
            {
                Class = "TOTAL",
                TP = scores.Sum(x => x.TP),
                FP = scores.Sum(x => x.FP),
                FN = scores.Sum(x => x.FN),
                TN = scores.Sum(x => x.TN),
                Precision = scores.Select(x => x.Precision).Average(),
                Recall = scores.Select(x => x.Recall).Average(),
                F1 = scores.Select(x => x.F1).Average()
            });

            return scores;
        }
                        
        public async Task BuildConfusionMatrixAsync(List<string> folderPaths, TestResult[] testResults)
        {
            testResults ??= await DeserializeAsync<TestResult[]>("TestResults");
            folderPaths ??= [.. testResults.Select(x => x.Actual).Concat(testResults.Select(x=>x.Predicted)).Distinct().OrderBy(x => x)];

            string[][] jagged = new string[folderPaths.Count()+1][];
            jagged = jagged.Select(x => new string[folderPaths.Count() + 1]).ToArray();
            jagged[0].ForEach((x, i) => x = i > 0 ? $"{folderPaths[i - 1]}" : "");
            jagged.ForEach((x, i) => x[0] = i>0 ? $"{folderPaths[i-1]}": "");

            foreach (var result in testResults)
            {
                var i = folderPaths.IndexOf(result.Actual);
                var j = folderPaths.IndexOf(result.Predicted) + 1;
                jagged[i][j] = result.Count.ToString();
            }

            await SaveCsvAsync(jagged, "ConfusionMatrixCsv");

            var headers = new List<string> { "Actual" };
            headers.AddRange(Enumerable.Range(0, folderPaths.Count()).Select(x => x.ToString().PadToCenter(3)));
            var justifications = Enumerable.Range(0, folderPaths.Count() + 1).Select(x => Enums.Justification.Center).ToArray();

            var confusionText = jagged.ToFormattedText(headers.ToArray(), justifications, "Confusion Matrix\nPredicted");
            var confusionArray = confusionText.Split("\n").ToArray();
            await SaveTextsAsync(confusionArray, "ConfusionMatrixText");
            
        }

        public async Task<VerboseTestResult[]> GetConfusionDetails(MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg, TestOutcome[] confusedOutcomes, TestResult[] confusedCounts)
        {
            var numberConfused = confusedOutcomes.Count();
            int complete = 0;
            double remainingSeconds = 0;
            double secondsPerItem = 0;
            var sw = await Task.Run(Stopwatch.StartNew);
            var confusedResults = confusedCounts.Select(x =>
            {
                var details = confusedOutcomes
                    .Where(outcome =>
                        outcome.Predicted == x.Predicted &&
                        outcome.Actual == x.Actual)

                    .Select((outcome) =>
                    {
                        var source = testSource[outcome.SourceIndex];
                        var tokens = source.Tokens.GroupAndCount();
                        var prediction = outcome.Predicted;
                        BayesianClassifierShared classifier = null;

                        try
                        {
                            classifier = classifierGroup.Classifiers[prediction];
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.Message, e);
                            SerializeAndSave(source, "ErrorSource", $"{outcome.SourceIndex:00000}");
                            logger.Debug($"original prediction: {prediction}");
                            var predictions = classifierGroup.Classify(tokens).ToArray();
                            SerializeAndSave(predictions, "Predictions", $"{outcome.SourceIndex:00000}");

                            throw;
                        }

                        var drivers = classifier.GetProbabilityDrivers(tokens);
                        var detail = new VerboseTestDetail()
                        {
                            Actual = source.FolderInfo.RelativePath,
                            Predicted = prediction,
                            Probability = drivers.Probability,
                            Drivers = drivers.Item2,
                            Source = source,
                        };


                        Interlocked.Increment(ref complete);

                        var msg = GetProgressMessage(complete, numberConfused, sw, ref secondsPerItem, ref remainingSeconds);

                        ppkg.ProgressTrackerPane.Report(
                            (int)(100 * complete / (double)numberConfused),
                            $"Extracting Confusion Drivers: {msg}");

                        return detail;
                    }).ToArray();

                var results = new VerboseTestResult()
                {
                    Actual = x.Actual,
                    Predicted = x.Predicted,
                    Details = details,
                };
                return results;
            })
            .ToArray();
            return confusedResults;
        }

        public VerboseTestDetail[] GetVerboseTestDetails(IEnumerable<TestOutcome> outcomes, MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup)
        {
            return outcomes.Select((outcome) =>
            {
                var source = testSource[outcome.SourceIndex];
                var tokens = source.Tokens.GroupAndCount();
                var prediction = outcome.Predicted;
                BayesianClassifierShared classifier = null;

                try
                {
                    classifier = classifierGroup.Classifiers[prediction];
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    SerializeAndSave(source, "ErrorSource", $"{outcome.SourceIndex:00000}");
                    logger.Debug($"original prediction: {prediction}");
                    var predictions = classifierGroup.Classify(tokens).ToArray();
                    SerializeAndSave(predictions, "Predictions", $"{outcome.SourceIndex:00000}");

                    throw;
                }

                var drivers = classifier.GetProbabilityDrivers(tokens);
                var detail = new VerboseTestDetail()
                {
                    Actual = source.FolderInfo.RelativePath,
                    Predicted = prediction,
                    Probability = drivers.Probability,
                    Drivers = drivers.Item2,
                    Source = source,
                };
                return detail;
            }).ToArray();
        }

        public async Task<ClassificationErrors[]> DiagnosePoorPerformance(MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg, TestOutcome[] confusedOutcomes, TestScores[] testScores)
        {
            int complete = 0;
            var sw = await Task.Run(Stopwatch.StartNew);
            var misclassified = testScores
                .Select(x => new KeyValuePair<string, int>(x.Class, x.FN + x.FP))
                .Where(x => x.Key != "TOTAL" && x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ToArray();

            var numberConfused = misclassified.Count();
            var confusedResults = misclassified.Select(x =>
            {
                var fpDetails = GetVerboseTestDetails(confusedOutcomes.Where(
                    outcome => outcome.Predicted == x.Key), testSource, classifierGroup);

                var fnDetails = GetVerboseTestDetails(confusedOutcomes.Where(
                    outcome => outcome.Actual == x.Key), testSource, classifierGroup);

                Interlocked.Increment(ref complete);

                ppkg.ProgressTrackerPane.Report(
                    (int)(100 * complete / (double)numberConfused),
                    $"Extracting Confusion Drivers: {GetProgressMessage(complete, numberConfused, sw)}");

                var errors = new ClassificationErrors() 
                { 
                    Class = x.Key, 
                    FalsePositives = fpDetails, 
                    FalseNegatives = fnDetails 
                };
                return errors;
            })
            .ToArray();
            ppkg.ProgressTrackerPane.Report(100);
            return confusedResults;
        }

        #endregion Classifier Performance Testing

        #region Data Progress, Loading, Saving, and Logging Methods

        private string GetProgressMessage(int complete, int count, Stopwatch sw)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"Completed {complete} of {count} ({seconds:N2} spm) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        private string GetProgressMessage(int complete, int count, Stopwatch sw, ref double secondsPerItem, ref double remainingSeconds)
        {
            secondsPerItem = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            remainingSeconds = remaining * secondsPerItem;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"Completed {complete} of {count} ({secondsPerItem:N2} spm) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        private string AdjustProgressTimer(int complete, int count, Stopwatch sw, ref double secondsPerItem, ref double remainingSeconds, ref double elapsedSeconds)
        {
            int attempts = 0;
            double exchangeValue = 0;
            double startingValue = -1;
            int maxAttempts = 100;

            while (startingValue != exchangeValue)
            {
                if (++attempts > maxAttempts)
                    throw new InvalidOperationException($"Attempted to add {attempts - 1} times without success");
                startingValue = remainingSeconds;
                var temp = Math.Max(0, startingValue + elapsedSeconds - sw.Elapsed.TotalSeconds);
                exchangeValue = Interlocked.CompareExchange(ref remainingSeconds, temp, startingValue);
            }
            elapsedSeconds = sw.Elapsed.TotalSeconds;
            
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"Completed {complete} of {count} ({secondsPerItem:N2} spm) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        public async Task SaveScoresAsync(IEnumerable<TestScores> scores)
        {
            SerializeAndSave(scores, "TestScores");
            var scores2 = scores.Select(x => new string[]
                        {
                x.Class, x.TP.ToString(), x.FP.ToString(), x.FN.ToString(), x.TN.ToString(),x.Precision.ToString("0.00"), x.Recall.ToString("0.00"), x.F1.ToString("0.00")
                        }).ToArray();

            var scoresText = scores2.ToFormattedText(
                ["Class", "TP", "FP", "FN", "TN", "Precision", "Recall", "F1"],
                Enumerable.Repeat(Enums.Justification.Center, 8).ToArray(), "Classifier Performance By Class");
            await SaveTextsAsync([scoresText], "TestScores");
            logger.Debug($"\n{scoresText}");
        }

        public virtual (MinedMailInfo[] Train, MinedMailInfo[] Test) SplitTestTrain(MinedMailInfo[] collection, double trainPercent, ProgressPackage ppkg)
        {
            ppkg.ProgressTrackerPane.Increment(10, "Building Folder Classifier -> Split Into Train / Test");
            var (train, test) = collection.SplitTestTrain(0.75);
            SerializeAndSave(train, "Train");
            SerializeAndSave(test, "Test");
            return (train, test);
        }

        public virtual async Task LoadForDiagnosisAsync() 
        {
            var (testOutcomes, testSource, classifierGroup, ppkg) = await ReloadIfNullAsync(null, null, null, null);

        }
        
        public virtual async Task<(TestOutcome[], MinedMailInfo[], BayesianClassifierGroup, ProgressPackage)> ReloadIfNullAsync(
            TestOutcome[] testOutcomes, MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg)
        {
            ppkg ??= await new ProgressPackage().InitializeAsync(progressTrackerPane: _globals.AF.ProgressTracker);
            _globals.AF.ProgressPane.Visible = true;
            ppkg.ProgressTrackerPane.Report(0, "Reloading Data If Necessary");

            testOutcomes ??= await DeserializeAsync<TestOutcome[]>("TestOutcomes");
            testSource ??= await DeserializeAsync<MinedMailInfo[]>("Test");
            classifierGroup ??= await DeserializeAsync<BayesianClassifierGroup>("TestClassifierGroup");

            if (testOutcomes.Length != testSource.Length) { throw new ArgumentException("Test Outcomes and Test Source Lengths Do Not Match"); }
            return (testOutcomes, testSource, classifierGroup, ppkg);
        }

        public virtual async Task<(EmailDataMiner, MinedMailInfo[], List<string>, ProgressPackage)> ReloadIfNullAsync(
            EmailDataMiner dataMiner, MinedMailInfo[] collection, ProgressPackage ppkg)
        {
            ppkg ??= await new ProgressPackage().InitializeAsync(progressTrackerPane: _globals.AF.ProgressTracker);
            _globals.AF.ProgressPane.Visible = true;
            ppkg.ProgressTrackerPane.Report(0, "Reloading Data If Necessary");

            dataMiner ??= new EmailDataMiner(Globals);
            collection ??= await dataMiner.Load<MinedMailInfo[]>();
            var folderPaths = collection.Select(x => x.FolderInfo.RelativePath).OrderBy(x => x).Distinct().ToList();

            return (dataMiner, collection, folderPaths, ppkg);
        }

        #endregion Data Progress, Loading, Saving, and Logging Methods

        #region Serialization

        internal virtual T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            if (File.Exists(disk.FilePath))
            {
                var item = JsonConvert.DeserializeObject<T>(
                    File.ReadAllText(disk.FilePath), jsonSettings);
                return item;
            }
            else { return default(T); }
        }

        internal async virtual Task<T> DeserializeAsync<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            if (File.Exists(disk.FilePath))
            {
                string fileText = null;
                using (var reader = File.OpenText(disk.FilePath))
                {
                    fileText = await reader.ReadToEndAsync();
                }

                var item = JsonConvert.DeserializeObject<T>(fileText, jsonSettings);
                return item;
            }
            else { return default(T); }
        }

        internal virtual async Task SaveTextsAsync(IEnumerable<string> texts, string fileNameSeed, string fileNameSuffix = "", string fileExtension = ".txt")
        {
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty() ? 
                $"{fileNameSeed}{fileExtension}" : 
                $"{fileNameSeed}_{fileNameSuffix}{fileExtension}";

            disk.FileName = fileName;
            if (File.Exists(disk.FilePath)) { File.Delete(disk.FilePath); }
            await WriteTextsAsync(disk.FilePath, texts);
        }

        internal virtual async Task SaveCsvAsync(string[][] jagged, string fileNameSeed, string fileNameSuffix = "")
        {
            var texts = jagged.Select(x => x.StringJoin(",")).ToArray();
            await SaveTextsAsync(texts, fileNameSeed, fileNameSuffix, ".csv");
        }
        
        internal virtual void SerializeAndSave<T>(T obj, string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,                
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            var serializer = JsonSerializer.Create(jsonSettings);
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            SerializeAndSave(obj, serializer, disk);
        }

        static async Task WriteTextsAsync(string filePath, IEnumerable<string> texts)
        {

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await texts.ToAsyncEnumerable().ForEachAwaitAsync(async text =>
                {
                    byte[] encodedText = Encoding.Unicode.GetBytes(text + Environment.NewLine);
                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                });
            };
        }

        internal virtual void SerializeAndSave<T>(T obj, JsonSerializer serializer, FilePathHelper disk)
        {
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                disk.FileName = null;
            }
        }

        #endregion Serialization

    }

}
