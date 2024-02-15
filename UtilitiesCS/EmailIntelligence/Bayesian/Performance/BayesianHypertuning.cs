using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class BayesianHypertuning
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Settings

        public BayesianHypertuning(IApplicationGlobals globals)
        {
            _globals = globals;
        }

        private IApplicationGlobals _globals;
        public IApplicationGlobals Globals { get => _globals; }

        protected bool _saveWip;
        public bool SaveWip { get => _saveWip; set => _saveWip = value; }

        #endregion Constructors and Settings

        #region Performance Record Types

        public record ClassCounts()
        {
            public string Class { get; set; }
            public int TP { get; set; }
            public int FP { get; set; }
            public int FN { get; set; }
            public int TN { get; set; }
        }

        public record VerboseClassCounts() 
        {
            public string Class { get; set; }
            public int TPCount { get; set; }
            public int FPCount { get; set; }
            public int FNCount { get; set; }
            public int TNCount { get; set; }
            public VerboseTestOutcome[] TPDetails { get; set; }
            public VerboseTestOutcome[] FPDetails { get; set; }
            public VerboseTestOutcome[] FNDetails { get; set; }
            public VerboseTestOutcome[] TNDetails { get; set; }
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

        public record VerboseTestScores()
        {
            public string Class { get; set; }
            public int TPCount { get; set; }
            public int FPCount { get; set; }
            public int FNCount { get; set; }
            public int TNCount { get; set; }
            public VerboseTestOutcome[] TPDetails { get; set; }
            public VerboseTestOutcome[] FPDetails { get; set; }
            public VerboseTestOutcome[] FNDetails { get; set; }
            public VerboseTestOutcome[] TNDetails { get; set; }
            public double Precision { get; set; }
            public double Recall { get; set; }
            public double F1 { get; set; }
        }

        public record VerboseTestResult()
        {
            public string Actual { get; set; }
            public string Predicted { get; set; }
            public int Count { get; set; }
            public VerboseTestOutcome[] Details { get; set; }
        }

        public record ClassificationErrors()
        {
            public string Class { get; set; }
            public VerboseTestOutcome[] FalsePositives { get; set; }
            public VerboseTestOutcome[] FalseNegatives { get; set; }
        }

        public record VerboseTestOutcome() 
        { 
            public string Actual { get; set; }
            public string Predicted { get; set; }
            public MinedMailInfo Source { get; set; }
            public int SourceIndex { get; set; }
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

        public record ThresholdMetric()
        {
            public double Threshold { get; set; }
            public double Precision { get; set; }
            public int PrecisionCount { get; set; }
            public double Recall { get; set; }
            public int RecallCount { get; set; }
            public double F1 { get; set; }
            public int F1Count { get; set; }
        }

        public record ThresholdMetrics()
        {
            public Series Precision { get; set; }
            public Series Recall { get; set; }
            public Series F1 { get; set; }
        }

        #endregion Performance Record Types

        #region Main Testing Methods

        public async Task TestFolderClassifierAsync(bool verbose)
        {
            if (!verbose) 
            { 
                await TestFolderClassifierAsync();
                return;
            }
            
            var progressState = _globals.AF.ProgressPane.Visible;
            _globals.AF.ProgressPane.Visible = true;

            var (dataMiner, collection, folderPaths, ppkg) = await ReloadIfNullAsync(null, null, null);
            
            var (train, test) = SplitTestTrain(collection, 0.75, ppkg);

            var classifierGroup = await BuildClassifierAsync(dataMiner, ppkg, train);

            VerboseTestOutcome[] verboseDetails = await RunVerboseClassifierTestAsync(test, classifierGroup, ppkg);

            VerboseTestResult[] verboseResults = GroupOutcomes(verboseDetails);

            ppkg.ProgressTrackerPane.Increment(0, "Building Folder Classifier -> Building Confusion Matrix and Calculating Scores");

            var counts = CountHitsMisses(folderPaths, verboseResults);

            var scores = await CalculateTestScoresAsync(counts);

            await SaveScoresAsync(scores);

            await BuildConfusionMatrixAsync(folderPaths, verboseResults);

            DiagnosePoorPerformance(counts);

            ppkg.ProgressTrackerPane.Report(100, "Operation Complete");
            _globals.AF.ProgressPane.Visible = progressState;
        }

        public async Task TestFolderClassifierAsync(EmailDataMiner dataMiner = null, MinedMailInfo[] collection = null)
        {
            var progressState = _globals.AF.ProgressPane.Visible;
            _globals.AF.ProgressPane.Visible = true;

            (dataMiner, collection, var folderPaths, var ppkg) = await ReloadIfNullAsync(dataMiner, collection, null);

            var (train, test) = SplitTestTrain(collection, 0.75, ppkg);

            var classifierGroup = await BuildClassifierAsync(dataMiner, ppkg, train);
                        
            TestOutcome[] testOutcomes = await RunClassifierTestAsync(test, classifierGroup, ppkg.SpawnChild(40));
            TestResult[] testResults = GroupOutcomes(testOutcomes);

            ppkg.ProgressTrackerPane.Increment(0, "Building Folder Classifier -> Building Confusion Matrix and Calculating Scores");
            
            ClassCounts[] counts = CountHitsMisses(folderPaths, testResults);
            
            IEnumerable<TestScores> scores = await CalculateTestScoresAsync(counts);
            
            await SaveScoresAsync(scores);
            
            await BuildConfusionMatrixAsync(folderPaths, testResults);

            ppkg.ProgressTrackerPane.Report(100, "Operation Complete");

            _globals.AF.ProgressPane.Visible = progressState;

        }
        
        public async Task GetConfusionDriversAsync(
            MinedMailInfo[] testSource = null, 
            TestOutcome[] testOutcomes = null,
            BayesianClassifierGroup classifierGroup = null,
            ProgressPackage ppkg = null)
        {
            var progressState = _globals.AF.ProgressPane.Visible;
            _globals.AF.ProgressPane.Visible = true;

            (testOutcomes, testSource, classifierGroup, ppkg) = await ReloadIfNullAsync(
                testOutcomes, testSource, classifierGroup, ppkg);

            var testScores = await DeserializeAsync<TestScores[]>("TestScores");
                        
            ppkg.ProgressTrackerPane.Increment(10, "Getting Confusion Outcomes and Counts");
            TestOutcome[] confusedOutcomes = testOutcomes.Where(x => x.Actual != x.Predicted).ToArray();
            TestResult[] confusedCounts = GroupOutcomes(confusedOutcomes);

            ppkg.ProgressTrackerPane.Increment(10, "Extracting Confusion Drivers");
            ClassificationErrors[] errors = await DiagnosePoorPerformance(testSource, classifierGroup, 
                ppkg.SpawnChild(100 - (int)ppkg.ProgressTrackerPane.Progress), confusedOutcomes, testScores);
                        
            _globals.AF.ProgressPane.Visible = progressState;
            
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
                SerializeAndSave(testOutcomes, testOutcomes.GetType().Name);
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

        public async Task<VerboseTestOutcome[]> RunVerboseClassifierTestAsync(
            MinedMailInfo[] test, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg)
        {
            var paneState = _globals.AF.ProgressPane.Visible;
            _globals.AF.ProgressPane.Visible = true;

            ppkg?.ProgressTrackerPane.Report(0, "Testing Classifiers");
            VerboseTestOutcome[] verboseTestOutcomes = null;
            int completed = 0;
            int count = test.Count();
            double remainingSeconds = 0;
            double secondsPerItem = 0;
            double elapsedSeconds = 0;
            var sw = await Task.Run(() => new SegmentStopWatch().Start());

            var testTask = Task.Run(() => verboseTestOutcomes =
                [
                    .. test
                    .Select((MinedMail, Index) => (MinedMail, Index))
                    .AsParallel()
                    .Select(x => (Source: x.MinedMail, Outcome: new TestOutcome
                    {
                        SourceIndex = x.Index,
                        Actual = x.MinedMail.FolderInfo.RelativePath,
                        Predicted = classifierGroup.Classify(x.MinedMail.Tokens.GroupAndCount()).First().Class
                    })).Select(x =>
                    {
                        var classifier = classifierGroup.Classifiers[x.Outcome.Predicted];
                        var tokens = x.Source.Tokens.GroupAndCount();
                        var drivers = classifier.GetProbabilityDrivers(tokens);
                        var detail = new VerboseTestOutcome()
                        {
                            Actual = x.Outcome.Actual,
                            Predicted = x.Outcome.Predicted,
                            Probability = drivers.Probability,
                            Drivers = drivers.Item2,
                            Source = x.Source,
                            SourceIndex = x.Outcome.SourceIndex
                        };
                        return detail;
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

                SerializeAndSave(verboseTestOutcomes, verboseTestOutcomes.GetType().Name);
                
                var testOutcomes = verboseTestOutcomes.Select(x => new TestOutcome
                {
                    Actual = x.Actual,
                    Predicted = x.Predicted,
                    SourceIndex = x.SourceIndex
                }).ToArray();
                SerializeAndSave(testOutcomes, testOutcomes.GetType().Name);

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

            return verboseTestOutcomes;
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
            SerializeAndSave(testResults, testResults.GetType().Name);
            return testResults;
        }

        public VerboseTestResult[] GroupOutcomes(VerboseTestOutcome[] outcomes)
        {
            VerboseTestResult[] verboseTestResults =
            [
                .. outcomes
                    .GroupBy(x => (x.Actual, x.Predicted))
                    .Select(x => new VerboseTestResult
                    {
                        Actual = x.Key.Actual,
                        Predicted = x.Key.Predicted,
                        Count = x.Count(),
                        Details = [.. x]
                    })
                    .OrderByDescending(x => x.Count)
            ];
            SerializeAndSave(verboseTestResults, verboseTestResults.GetType().Name);

            var testResults = verboseTestResults.Select(x => new TestResult
            {
                Actual = x.Actual,
                Predicted = x.Predicted,
                Count = x.Count
            }).ToArray();
            SerializeAndSave(testResults, testResults.GetType().Name);

            return verboseTestResults;
        }
                
        public ClassCounts[] CountHitsMisses(List<string> folderPaths, TestResult[] testResults)
        {
            ClassCounts[] counts = folderPaths.Select(x => new ClassCounts
            {
                Class = x,
                TP = testResults.Count(y => y.Actual == x && y.Predicted == x),
                FP = testResults.Count(y => y.Actual != x && y.Predicted == x),
                FN = testResults.Count(y => y.Actual == x && y.Predicted != x),
                TN = testResults.Count(y => y.Actual != x && y.Predicted != x)
            }).ToArray();
            SerializeAndSave(counts, counts.GetType().Name);
            return counts;
        }

        public VerboseClassCounts[] CountHitsMisses(List<string> folderPaths, VerboseTestResult[] testResults)
        {
            VerboseClassCounts[] verboseCounts = folderPaths.Select(x => new VerboseClassCounts
            {
                Class = x,
                TPCount = testResults.Count(y => y.Actual == x && y.Predicted == x),
                FPCount = testResults.Count(y => y.Actual != x && y.Predicted == x),
                FNCount = testResults.Count(y => y.Actual == x && y.Predicted != x),
                TNCount = testResults.Count(y => y.Actual != x && y.Predicted != x),
                TPDetails = testResults.Where(y => y.Actual == x && y.Predicted == x).SelectMany(y => y.Details).ToArray(),
                FPDetails = testResults.Where(y => y.Actual != x && y.Predicted == x).SelectMany(y => y.Details).ToArray(),
                FNDetails = testResults.Where(y => y.Actual == x && y.Predicted != x).SelectMany(y => y.Details).ToArray(),
                //TNDetails = testResults.Where(y => y.Actual != x && y.Predicted != x).SelectMany(y => y.Details).ToArray()
            }).ToArray();
            SerializeAndSave(verboseCounts, verboseCounts.GetType().Name);

            var counts = verboseCounts.Select(x => new ClassCounts
            {
                Class = x.Class,
                TP = x.TPCount,
                FP = x.FPCount,
                FN = x.FNCount,
                TN = x.TNCount
            }).ToArray();
            SerializeAndSave(counts, counts.GetType().Name);

            return verboseCounts;
        }

        public IEnumerable<TestScores> CalculateTestScores(ClassCounts[] counts)
        {
            if (counts.IsNullOrEmpty())
            { counts = Deserialize<ClassCounts[]>(typeof(ClassCounts[]).Name); }
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

        public async Task<IEnumerable<TestScores>> CalculateTestScoresAsync(ClassCounts[] counts)
        {
            if (counts.IsNullOrEmpty())
            { counts = await DeserializeAsync<ClassCounts[]>(typeof(ClassCounts[]).Name); }
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

        public async Task<IEnumerable<VerboseTestScores>> CalculateTestScoresAsync(VerboseClassCounts[] details)
        {
            if (details.IsNullOrEmpty())
            { details = await DeserializeAsync<VerboseClassCounts[]>(typeof(VerboseClassCounts[]).Name); }
            var scores = details.Select(x => new
            {
                x.Class,
                x.TPCount,
                x.FPCount,
                x.FNCount,
                x.TNCount,
                x.TPDetails,
                x.FPDetails,
                x.FNDetails,
                x.TNDetails,
                Precision = (x.TPCount + x.FPCount) != 0 ? x.TPCount / (double)(x.TPCount + x.FPCount) : 0,
                Recall = (x.TPCount + x.FNCount) != 0 ? x.TPCount / (double)(x.TPCount + x.FNCount) : 0,
            }).Select(x => new VerboseTestScores
            {
                Class = x.Class,
                TPCount = x.TPCount,
                FPCount = x.FPCount,
                FNCount = x.FNCount,
                TNCount = x.TNCount,
                TPDetails = x.TPDetails,
                FPDetails = x.FPDetails,
                FNDetails = x.FNDetails,
                TNDetails = x.TNDetails,
                Precision = x.Precision,
                Recall = x.Recall,
                F1 = (x.Precision + x.Recall) != 0 ? 2 * (x.Precision * x.Recall) / (x.Precision + x.Recall) : 0
            }).ToList();

            scores.Add(new VerboseTestScores
            {
                Class = "TOTAL",
                TPCount = scores.Sum(x => x.TPCount),
                FPCount = scores.Sum(x => x.FPCount),
                FNCount = scores.Sum(x => x.FNCount),
                TNCount = scores.Sum(x => x.TNCount),
                Precision = scores.Select(x => x.Precision).Average(),
                Recall = scores.Select(x => x.Recall).Average(),
                F1 = scores.Select(x => x.F1).Average()
            });

            return scores;
        }

        public async Task BuildConfusionMatrixAsync(List<string> folderPaths, TestResult[] testResults)
        {
            testResults ??= await DeserializeAsync<TestResult[]>(typeof(TestResult[]).Name);
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

            await SaveCsvAsync(jagged, "ConfusionMatrix");

            var headers = new List<string> { "Actual" };
            headers.AddRange(Enumerable.Range(0, folderPaths.Count()).Select(x => x.ToString().PadToCenter(3)));
            var justifications = Enumerable.Range(0, folderPaths.Count() + 1).Select(x => Enums.Justification.Center).ToArray();

            var confusionText = jagged.ToFormattedText(headers.ToArray(), justifications, "Confusion Matrix\nPredicted");
            var confusionArray = confusionText.Split("\n").ToArray();
            await SaveTextsAsync(confusionArray, "ConfusionMatrixText");
            
        }

        public async Task BuildConfusionMatrixAsync(List<string> folderPaths, VerboseTestResult[] verboseTestResults)
        {
            verboseTestResults ??= await DeserializeAsync<VerboseTestResult[]>(typeof(VerboseTestResult[]).Name);
            TestResult[] testResults = verboseTestResults.Select(x => new TestResult
            {
                Actual = x.Actual,
                Predicted = x.Predicted,
                Count = x.Count
            }).ToArray();
            await BuildConfusionMatrixAsync(folderPaths, testResults);
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
                        var detail = new VerboseTestOutcome()
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
                    Count = details.Count(),
                };
                return results;
            })
            .ToArray();
            return confusedResults;
        }

        public VerboseTestOutcome[] GetVerboseTestDetails(IEnumerable<TestOutcome> outcomes, MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup)
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
                var detail = new VerboseTestOutcome()
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
            var classificationErrors = misclassified.Select(x =>
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

            if (SaveWip) { SerializeAndSave(classificationErrors, "ClassificationErrors"); }
            ppkg.ProgressTrackerPane.Report(100);
            
            return classificationErrors;
        }

        public ClassificationErrors[] DiagnosePoorPerformance(VerboseClassCounts[] classCounts)
        {
            var classificationErrors = classCounts
                .Select(x => new ClassificationErrors()
                {
                    Class = x.Class,
                    FalsePositives = x.FPDetails,
                    FalseNegatives = x.FNDetails
                }).OrderByDescending(x => x.FalsePositives.Count() + x.FalseNegatives.Count())
                .ToArray();

            if (SaveWip) { SerializeAndSave(classificationErrors, "ClassificationErrors"); }
            
            return classificationErrors;  
        }

        public async Task<ThresholdMetric[]> RunSensitivityAsync(VerboseTestOutcome[] verboseTestOutcomes) 
        {
            verboseTestOutcomes ??= await DeserializeAsync<VerboseTestOutcome[]>(typeof(VerboseTestOutcome[]).Name);
            var folderPaths = verboseTestOutcomes.SelectMany(x => new string[] { x.Actual, x.Predicted }).Distinct().OrderBy(x => x).ToList();

            var thresholdMetrics = Enumerable.Range(0, 100).Select(i =>
            {
                var testOutcomes = verboseTestOutcomes
                .AsParallel()
                .Where(x => x.Probability >= (i / (double)100))
                .Select(x => new TestOutcome
                {
                    Actual = x.Actual,
                    Predicted = x.Predicted,
                    SourceIndex = x.SourceIndex,
                }).ToArray();
                TestResult[] testResults = GroupOutcomes(testOutcomes);
                ClassCounts[] counts = CountHitsMisses(folderPaths, testResults);
                var metrics = CalculateTestScores(counts).Last();
                var observations = metrics.TP + metrics.FN + metrics.FP + metrics.TN; 
                return new ThresholdMetric 
                { 
                    Threshold = i / (double)100, 
                    Precision = metrics.Precision, 
                    PrecisionCount = (int)(metrics.Precision * observations),
                    Recall = metrics.Recall, 
                    RecallCount = (int)(metrics.Recall * observations),
                    F1 = metrics.F1,
                    F1Count = (int)(metrics.F1 * observations),
                };
            }).ToArray();

            SerializeAndSave(thresholdMetrics, typeof(ThresholdMetric[]).Name);
            return thresholdMetrics;
        }

        public async Task ShowSensitivityChartAsync(ThresholdMetric[] thresholdMetrics) 
        { 
            thresholdMetrics ??= await DeserializeAsync<ThresholdMetric[]>(typeof(ThresholdMetric[]).Name);
            thresholdMetrics ??= await RunSensitivityAsync(null);

            var viewer = new MetricChartViewer();
            viewer.MetricChart.Series["F1"].Points.DataBind(thresholdMetrics, "Threshold", "F1", "");
            viewer.MetricChart.Series["Precision"].Points.DataBind(thresholdMetrics, "Threshold", "Precision", "");
            viewer.MetricChart.Series["Recall"].Points.DataBind(thresholdMetrics, "Threshold", "Recall", "");
            viewer.MetricChart.Series["F1 Count"].Points.DataBind(thresholdMetrics, "Threshold", "F1Count", "");
            viewer.MetricChart.Series["Precision Count"].Points.DataBind(thresholdMetrics, "Threshold", "PrecisionCount", "");
            viewer.MetricChart.Series["Recall Count"].Points.DataBind(thresholdMetrics, "Threshold", "RecallCount", "");
            viewer.Show();
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

        public async Task SaveScoresAsync(IEnumerable<VerboseTestScores> verboseScores)
        {
            SerializeAndSave(verboseScores, "VerboseTestScores[]");
            
            var scores = verboseScores.Select(x => new TestScores
            {
                Class = x.Class,
                TP = x.TPCount,
                FP = x.FPCount,
                FN = x.FNCount,
                TN = x.TNCount,
                Precision = x.Precision,
                Recall = x.Recall,
                F1 = x.F1
            }).ToList();
            
            SerializeAndSave(scores, "TestScores");

            var scores2 = scores.Select(x => new string[]
            {
                x.Class, x.TP.ToString(), x.FP.ToString(), x.FN.ToString(), 
                x.TN.ToString(),x.Precision.ToString("0.00"), x.Recall.ToString("0.00"), x.F1.ToString("0.00")
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
