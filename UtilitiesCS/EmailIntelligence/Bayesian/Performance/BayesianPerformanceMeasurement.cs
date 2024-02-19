using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class BayesianPerformanceMeasurement
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Settings

        public BayesianPerformanceMeasurement(IApplicationGlobals globals)
        {
            _globals = globals;
            Serialization = new BayesianSerializationHelper(globals);
        }

        private IApplicationGlobals _globals;
        public IApplicationGlobals Globals { get => _globals; }

        internal BayesianSerializationHelper Serialization { get; set; }

        protected bool _saveWip = true;
        public bool SaveWip { get => _saveWip; set => _saveWip = value; }

        #endregion Constructors and Settings

        #region Classifier Performance Testing

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

            var (dataMiner, collection, folderPaths, ppkg) = await LoadIfNullAsync(null, null, null);
            
            ppkg.ProgressTrackerPane.Report(0, "Building Folder Classifier");
            var (train, test) = await SplitAndSave(collection, 0.75, ppkg.SpawnChild(10));

            var classifierGroup = await BuildClassifierAsync(dataMiner, ppkg.SpawnChild(20), train);

            VerboseTestOutcome[] verboseDetails = await RunVerboseClassifierTestAsync(test, classifierGroup, ppkg.SpawnChild(50));

            VerboseGroupedTestOutcome[] verboseResults = GroupOutcomes(verboseDetails);

            ppkg.ProgressTrackerPane.Increment(0, "Building Folder Classifier -> Building Confusion Matrix and Calculating Scores");

            var counts = CountHitsMisses(folderPaths, verboseResults);

            var scores = await CalculateTestScoresAsync(counts);

            await SaveScoresAsync(scores, ppkg.ProgressTrackerPane.SpawnChild(5));

            await BuildConfusionMatrixAsync(folderPaths, verboseResults);

            await DiagnosePoorPerformanceAsync(scores.ToArray(), ppkg.ProgressTrackerPane.SpawnChild(25));

            ppkg.ProgressTrackerPane.Report(100, "Operation Complete");
            _globals.AF.ProgressPane.Visible = progressState;
        }

        public async Task TestFolderClassifierAsync(EmailDataMiner dataMiner = null, MinedMailInfo[] collection = null)
        {
            var progressState = _globals.AF.ProgressPane.Visible;
            _globals.AF.ProgressPane.Visible = true;

            (dataMiner, collection, var folderPaths, var ppkg) = await LoadIfNullAsync(dataMiner, collection, null);

            ppkg.ProgressTrackerPane.Report(0, "Building Folder Classifier");
            var (train, test) = await SplitAndSave(collection, 0.75, ppkg.SpawnChild(10));

            var classifierGroup = await BuildClassifierAsync(dataMiner, ppkg.SpawnChild(20), train);
                        
            TestOutcome[] testOutcomes = await RunClassifierTestAsync(test, classifierGroup, ppkg.SpawnChild(50));
            GroupedTestOutcome[] testResults = GroupOutcomes(testOutcomes);

            ppkg.ProgressTrackerPane.Increment(0, "Building Folder Classifier -> Building Confusion Matrix and Calculating Scores");
            
            ClassCounts[] counts = CountHitsMisses(folderPaths, testResults);
            
            IEnumerable<TestScores> scores = await CalculateTestScoresAsync(counts);

            await SaveScoresAsync(scores, ppkg.ProgressTrackerPane.SpawnChild(5));

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

            (testOutcomes, testSource, classifierGroup, ppkg) = await LoadIfNullAsync(
                testOutcomes, testSource, classifierGroup, ppkg);

            var testScores = await Serialization.DeserializeAsync<TestScores[]>(ppkg.ProgressTrackerPane, "TestScores");
                        
            ppkg.ProgressTrackerPane.Report(0, "Getting Confusion Outcomes and Counts");
            TestOutcome[] confusedOutcomes = testOutcomes.Where(x => x.Actual != x.Predicted).ToArray();
            GroupedTestOutcome[] confusedCounts = GroupOutcomes(confusedOutcomes);

            ppkg.ProgressTrackerPane.Increment(10, "Extracting Confusion Drivers");
            ClassificationErrors[] errors = await DiagnosePoorPerformanceAsync(testSource, classifierGroup, 
                ppkg.SpawnChild(100 - (int)ppkg.ProgressTrackerPane.Progress), confusedOutcomes, testScores);
                        
            _globals.AF.ProgressPane.Visible = progressState;
            
        }

        #endregion Main Testing Methods

        #region Step 1: Build Classifier

        public async Task<BayesianClassifierGroup> BuildClassifierAsync(EmailDataMiner dataMiner, ProgressPackage ppkg, MinedMailInfo[] train)
        {
            ppkg.ProgressTrackerPane.Increment(0, "Building Folder Classifier -> Create Classifier Group");

            var classifierGroup = await dataMiner.CreateClassifierGroupAsync(train);

            ppkg.ProgressTrackerPane.Increment(30, "Building Folder Classifier -> Building Classifiers");

            await dataMiner.BuildFolderClassifiersAsync(classifierGroup, train, await new ProgressPackage().InitializeAsync(
                ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(70), ppkg.StopWatch));

            Serialization.SerializeAndSave(classifierGroup, "TestClassifierGroup");

            return classifierGroup;
        }

        #endregion Step 1: Build Classifier

        #region Step 2: Run The Test -> TestOutcome[]

        public async Task<TestOutcome[]> RunClassifierTestAsync(
            MinedMailInfo[] test, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg)
        {
            var paneState = _globals.AF.ProgressPane.Visible;
            _globals.AF.ProgressPane.Visible = true;

            var ppkg2 = ppkg?.SpawnChild(70);
            ppkg2?.ProgressTrackerPane.Report(0, "Testing Classifiers");
            TestOutcome[] testOutcomes = null;
            int completed = 0;
            int count = test.Count();
            double remainingSeconds = 0;
            double secondsPerItem = 0;
            double elapsedSeconds = 0;
            var sw = await Task.Run(() => new SegmentStopWatch().Start());
            var cores = Environment.ProcessorCount;
            
            var testTask = Task.Run(() => testOutcomes =
                [
                    .. test
                    .Select((MinedMail, Index) => (MinedMail, Index))
                    .AsParallel()
                    .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                    .WithDegreeOfParallelism(cores-2)
                    .Select(x => new TestOutcome
                    {
                        SourceIndex = x.Index,
                        Actual = x.MinedMail.FolderInfo.RelativePath,
                        Predicted = classifierGroup.Classify(x.MinedMail.Tokens.GroupAndCount()).First().Class
                    })
                    .WithAction(() => ReportAndCapture(ppkg2, ref completed, count, ref remainingSeconds, ref secondsPerItem, ref elapsedSeconds, sw)),
                ],
                ppkg2.Cancel);

            TimerWrapper timer = null;
            var timerTask = Task.Run(() =>
            {
                timer = new TimerWrapper(TimeSpan.FromSeconds(1));
                timer.Elapsed += (sender, e) =>
                {
                    if (count > 0)
                    {
                        var msg = AdjustProgressTimer(completed, count, sw, ref secondsPerItem, ref remainingSeconds, ref elapsedSeconds);
                        ppkg2.ProgressTrackerPane.Report(
                            ((int)(((double)completed / count) * 100),
                            $"Testing Classifiers -> {msg}"));
                    }
                };
                timer.AutoReset = true;
                timer.StartTimer();
            });

            try
            {
                await Task.WhenAll(timerTask, testTask);
                
                await Serialization.SerializeAndSaveAsync(testOutcomes, ppkg.ProgressTrackerPane.SpawnChild(30), testOutcomes.GetType().Name);
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

            var ppkg2 = ppkg?.SpawnChild(70);
            ppkg2?.ProgressTrackerPane.Report(0, "Testing Classifiers");
            VerboseTestOutcome[] verboseTestOutcomes = null;
            var cores = Environment.ProcessorCount;

            var (completed, count, remainingSeconds, secondsPerItem, elapsedSeconds) = (0, test.Count(), 0D, 0D, 0D);
            
            var sw = await Task.Run(() => new SegmentStopWatch().Start());

            var testTask = Task.Run(() => verboseTestOutcomes =
                [
                    .. test
                    .Select((MinedMail, Index) => (MinedMail, Index))
                    //.AsParallel()
                    //.WithDegreeOfParallelism(cores-2)
                    //.WithMergeOptions(ParallelMergeOptions.NotBuffered)
                    .Select(x => (Source: x.MinedMail, Outcome: new TestOutcome
                    {
                        SourceIndex = x.Index,
                        Actual = x.MinedMail.FolderInfo.RelativePath,
                        Predicted = classifierGroup.Classify(x.MinedMail.Tokens.GroupAndCount()).First().Class
                    })).WithProgressReporting(count, pgrs => Interlocked.Increment(ref completed))
                    //.WithAction(() => ReportAndCapture(ppkg2, ref completed, count, ref remainingSeconds, ref secondsPerItem, ref elapsedSeconds, sw))
                    .ToArray().AsParallel()
                    .Select(x =>
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
                    }),
                    //.WithProgressReporting(count, ppkg.ProgressTrackerPane, sw)
                    //.WithAction(() => ReportAndCapture(ppkg2, ref completed, count, ref remainingSeconds, ref secondsPerItem, ref elapsedSeconds, sw)),                    
                ],
                ppkg2.Cancel);

            TimerWrapper timer = null;
            var timerTask = Task.Run(() =>
            {
                timer = new TimerWrapper(TimeSpan.FromSeconds(1));
                timer.Elapsed += (sender, e) =>
                {
                    if (count > 0)
                    {
                        var msg = GetProgressMessage(completed, count, sw);
                        //var msg = AdjustProgressTimer(completed, count, sw, ref secondsPerItem, ref remainingSeconds, ref elapsedSeconds);
                        ppkg2.ProgressTrackerPane.Report(
                            (double)completed / count * 100,
                            $"Testing Classifiers -> {msg}");
                    }
                };
                timer.AutoReset = true;
                timer.StartTimer();
            });

            try
            {
                await Task.WhenAll(timerTask, testTask);
                
                await Serialization.SerializeAndSaveAsync(verboseTestOutcomes, ppkg.ProgressTrackerPane.SpawnChild(15), verboseTestOutcomes.GetType().Name);
                
                var testOutcomes = verboseTestOutcomes.Select(x => new TestOutcome
                {
                    Actual = x.Actual,
                    Predicted = x.Predicted,
                    SourceIndex = x.SourceIndex
                }).ToArray();
                await Serialization.SerializeAndSaveAsync(testOutcomes, ppkg.ProgressTrackerPane.SpawnChild(15), testOutcomes.GetType().Name);

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

        private void ReportAndCapture(ProgressPackage ppkg, ref int completed, int count, ref double remainingSeconds, ref double secondsPerItem, ref double elapsedSeconds, SegmentStopWatch sw)
        {
            //double elapsedSeconds;
            Interlocked.Increment(ref completed);

            var msg = GetProgressMessage(completed, count, sw, ref secondsPerItem, ref remainingSeconds);
            ppkg.ProgressTrackerPane.Report(
                ((int)(((double)completed / count) * 100),
                $"Testing Classifiers -> {msg}"));
            elapsedSeconds = sw.Elapsed.TotalSeconds;
            //return elapsedSeconds;
        }

        #endregion Step 2: Run The Test -> TestOutcome[]

        #region Step 3: Group Outcomes -> TestResult[]

        public GroupedTestOutcome[] GroupOutcomes(TestOutcome[] outcomes)
        {
            GroupedTestOutcome[] testResults =
            [
                .. outcomes
                    .GroupBy(x => (x.Actual, x.Predicted))
                    .Select(x => new GroupedTestOutcome
                    {
                        Actual = x.Key.Actual,
                        Predicted = x.Key.Predicted,
                        Count = x.Count()
                    })
                    .OrderByDescending(x => x.Count)
            ];
            Serialization.SerializeAndSave(testResults, testResults.GetType().Name);
            return testResults;
        }

        public VerboseGroupedTestOutcome[] GroupOutcomes(VerboseTestOutcome[] outcomes)
        {
            VerboseGroupedTestOutcome[] verboseTestResults =
            [
                .. outcomes
                    .GroupBy(x => (x.Actual, x.Predicted))
                    .Select(x => new VerboseGroupedTestOutcome
                    {
                        Actual = x.Key.Actual,
                        Predicted = x.Key.Predicted,
                        Count = x.Count(),
                        Details = [.. x]
                    })
                    .OrderByDescending(x => x.Count)
            ];
            Serialization.SerializeAndSave(verboseTestResults, verboseTestResults.GetType().Name);

            var testResults = verboseTestResults.Select(x => new GroupedTestOutcome
            {
                Actual = x.Actual,
                Predicted = x.Predicted,
                Count = x.Count
            }).ToArray();
            Serialization.SerializeAndSave(testResults, testResults.GetType().Name);

            return verboseTestResults;
        }

        #endregion Step 3: Group Outcomes -> TestResult[]

        #region Step 4: Count Hits and Misses -> ClassCounts[]

        public ClassCounts[] CountHitsMisses(List<string> folderPaths, GroupedTestOutcome[] testResults)
        {
            ClassCounts[] counts = folderPaths.Select(x => new ClassCounts
            {
                Class = x,
                TP = testResults.Count(y => y.Actual == x && y.Predicted == x),
                FP = testResults.Count(y => y.Actual != x && y.Predicted == x),
                FN = testResults.Count(y => y.Actual == x && y.Predicted != x),
                TN = testResults.Count(y => y.Actual != x && y.Predicted != x)
            }).ToArray();
            Serialization.SerializeAndSave(counts, counts.GetType().Name);
            return counts;
        }

        public VerboseClassCounts[] CountHitsMisses(List<string> folderPaths, VerboseGroupedTestOutcome[] testResults)
        {
            VerboseClassCounts[] verboseCounts = folderPaths
                .Select(x => new 
                {
                    Class = x,
                    TP = testResults.Count(y => y.Actual == x && y.Predicted == x),
                    FP = testResults.Count(y => y.Actual != x && y.Predicted == x),
                    FN = testResults.Count(y => y.Actual == x && y.Predicted != x),
                    TN = testResults.Count(y => y.Actual != x && y.Predicted != x),
                })
                .Where(x => x.TP + x.FP + x.FN > 0)
                .Select(x => new VerboseClassCounts
                {
                    Class = x.Class, 
                    TP = x.TP,
                    FP = x.FP,
                    FN = x.FN,
                    TN = x.TN,
                    Errors = x.FP + x.FN,
                
                    VerboseOutcomes = testResults
                        .Where(y => y.Actual == x.Class || y.Predicted == x.Class)
                        .SelectMany(y =>
                        {
                            var resultType = GetResultType(x.Class, y.Actual, y.Predicted);
                            return y.Details.Select(z => new KeyValuePair<VerboseTestOutcome, string>(z, resultType));
                        }).ToDictionary()

                }).ToArray();
            Serialization.SerializeAndSave(verboseCounts, verboseCounts.GetType().Name);

            var counts = verboseCounts.Select(x => new ClassCounts
            {
                Class = x.Class,
                TP = x.TP,
                FP = x.FP,
                FN = x.FN,
                TN = x.TN,
            }).ToArray();
            Serialization.SerializeAndSave(counts, counts.GetType().Name);

            return verboseCounts;
        }

        internal virtual string GetResultType(string @class, string actual, string predicted) 
        {
            var t = (@class, actual, predicted);
            return t switch
            {
                { } when (t.actual == t.@class) && (predicted == @class) => "TruePositive",
                { } when actual != @class && predicted == @class => "FalsePositive",
                { } when actual == @class && predicted != @class => "FalseNegative",
                { } when actual != @class && predicted != @class => "TrueNegative",
                _ => "Unknown"
            };

        }

        #endregion Step 4: Count Hits and Misses -> ClassCounts[]

        #region Step 5: Calculate Test Scores -> IEnumerable<TestScores>

        public IEnumerable<TestScores> CalculateTestScores(ClassCounts[] counts)
        {
            if (counts.IsNullOrEmpty())
            { counts = Serialization.Deserialize<ClassCounts[]>(typeof(ClassCounts[]).Name); }
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
            { counts = await Serialization.DeserializeAsync<ClassCounts[]>(typeof(ClassCounts[]).Name); }
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
            { details = await Serialization.DeserializeAsync<VerboseClassCounts[]>(typeof(VerboseClassCounts[]).Name); }
            var scores = details.Select(x => new
            {
                x.Class,
                x.TP,
                x.FP,
                x.FN,
                x.TN,
                x.Errors,
                x.VerboseOutcomes,
                Precision = (x.TP + x.FP) != 0 ? x.TP / (double)(x.TP + x.FP) : 0,
                Recall = (x.TP + x.FN) != 0 ? x.TP / (double)(x.TP + x.FN) : 0,
            }).Select(x => new VerboseTestScores
            {
                Class = x.Class,
                TP = x.TP,
                FP = x.FP,
                FN = x.FN,
                TN = x.TN,
                Errors = x.Errors,
                VerboseOutcomes = x.VerboseOutcomes,
                Precision = x.Precision,
                Recall = x.Recall,
                F1 = (x.Precision + x.Recall) != 0 ? 2 * (x.Precision * x.Recall) / (x.Precision + x.Recall) : 0
            }).ToList();

            scores.Add(new VerboseTestScores
            {
                Class = "TOTAL",
                TP = scores.Sum(x => x.TP),
                FP = scores.Sum(x => x.FP),
                FN = scores.Sum(x => x.FN),
                TN = scores.Sum(x => x.TN),
                Errors = scores.Sum(x => x.Errors),
                Precision = scores.Select(x => x.Precision).Average(),
                Recall = scores.Select(x => x.Recall).Average(),
                F1 = scores.Select(x => x.F1).Average()
            });

            return scores;
        }

        #endregion Step 5: Calculate Test Scores -> IEnumerable<TestScores>

        #region Step 6: Build Confusion Matrix -> serialized CSV and Text

        public async Task BuildConfusionMatrixAsync(List<string> folderPaths, GroupedTestOutcome[] testResults)
        {
            testResults ??= await Serialization.DeserializeAsync<GroupedTestOutcome[]>(typeof(GroupedTestOutcome[]).Name);
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

            await Serialization.SaveCsvAsync(jagged, "ConfusionMatrix");

            var headers = new List<string> { "Actual" };
            headers.AddRange(Enumerable.Range(0, folderPaths.Count()).Select(x => x.ToString().PadToCenter(3)));
            var justifications = Enumerable.Range(0, folderPaths.Count() + 1).Select(x => Enums.Justification.Center).ToArray();

            var confusionText = jagged.ToFormattedText(headers.ToArray(), justifications, "Confusion Matrix\nPredicted");
            var confusionArray = confusionText.Split("\n").ToArray();
            await Serialization.SaveTextsAsync(confusionArray, "ConfusionMatrixText");
            
        }

        public async Task BuildConfusionMatrixAsync(List<string> folderPaths, VerboseGroupedTestOutcome[] verboseTestResults)
        {
            verboseTestResults ??= await Serialization.DeserializeAsync<VerboseGroupedTestOutcome[]>(typeof(VerboseGroupedTestOutcome[]).Name);
            GroupedTestOutcome[] testResults = verboseTestResults.Select(x => new GroupedTestOutcome
            {
                Actual = x.Actual,
                Predicted = x.Predicted,
                Count = x.Count
            }).ToArray();
            await BuildConfusionMatrixAsync(folderPaths, testResults);
        }

        #endregion Step 6: Build Confusion Matrix -> serialized CSV and Text

        #region Step 7: Diagnose Poor Performers -> ClassificationErrors[]

        public async Task<VerboseGroupedTestOutcome[]> GetConfusionDetails(MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg, TestOutcome[] confusedOutcomes, GroupedTestOutcome[] confusedCounts)
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
                            Serialization.SerializeAndSave(source, "ErrorSource", $"{outcome.SourceIndex:00000}");
                            logger.Debug($"original prediction: {prediction}");
                            var predictions = classifierGroup.Classify(tokens).ToArray();
                            Serialization.SerializeAndSave(predictions, "Predictions", $"{outcome.SourceIndex:00000}");

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

                var results = new VerboseGroupedTestOutcome()
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
                    Serialization.SerializeAndSave(source, "ErrorSource", $"{outcome.SourceIndex:00000}");
                    logger.Debug($"original prediction: {prediction}");
                    var predictions = classifierGroup.Classify(tokens).ToArray();
                    Serialization.SerializeAndSave(predictions, "Predictions", $"{outcome.SourceIndex:00000}");

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

        public async Task<ClassificationErrors[]> DiagnosePoorPerformanceAsync(MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg, TestOutcome[] confusedOutcomes, TestScores[] testScores)
        {
            int complete = 0;
            var sw = await Task.Run(Stopwatch.StartNew);
            var misclassified = testScores
                .Select(x => new { x.Class, x.TP, x.FP, x.FN, x.TN, Errors = x.FP + x.FN })
                //new KeyValuePair<string, int>(x.Class, x.FN + x.FP))
                .Where(x => x.Class != "TOTAL" && x.Errors > 0)
                .OrderByDescending(x => x.Errors)
                .ToArray();

            var numberConfused = misclassified.Count();
            
            var ppkg2 = ppkg.SpawnChild(70);
            var classificationErrors = misclassified.Select(x =>
            {
                var fpDetails = GetVerboseTestDetails(confusedOutcomes.Where(
                    outcome => outcome.Predicted == x.Class), testSource, classifierGroup);

                var fnDetails = GetVerboseTestDetails(confusedOutcomes.Where(
                    outcome => outcome.Actual == x.Class), testSource, classifierGroup);

                var errors = new ClassificationErrors() 
                { 
                    Class = x.Class, 
                    TP = x.TP,
                    FP = x.FP, 
                    FN = x.FN,
                    TN = x.TN,
                    Errors = x.Errors,
                    VerboseOutcomes = fpDetails
                        .Select(x => new KeyValuePair<VerboseTestOutcome, string>(x, "False Positive"))
                        .Concat(fnDetails.Select(x => new KeyValuePair<VerboseTestOutcome, string>(x, "False Negative")))
                        .ToDictionary()
                };
                
                Interlocked.Increment(ref complete);

                ppkg2.ProgressTrackerPane.Report(
                    (int)(100 * complete / (double)numberConfused),
                    $"Extracting Confusion Drivers: {GetProgressMessage(complete, numberConfused, sw)}");

                return errors;
            })
            .ToArray();

            if (SaveWip) 
            { 
                await Serialization.SerializeAndSaveAsync(
                    classificationErrors, ppkg.ProgressTrackerPane.SpawnChild(30), "ClassificationErrors[]"); 
            }
            ppkg.ProgressTrackerPane.Report(100);
            
            return classificationErrors;
        }

        public async Task<ClassificationErrors[]> DiagnosePoorPerformanceAsync(VerboseTestScores[] verboseTestResults, ProgressTrackerPane progress)
        {
            var sw = await Task.Run(Stopwatch.StartNew);

            verboseTestResults.ForEach(x => 
            { 
                var verboseOutcomes = x.VerboseOutcomes?.Where(y => (new string[] { "False Positive", "False Negative" }).Contains(y.Value)) ?? [];
                x.VerboseOutcomes = verboseOutcomes.Count() > 0 ? verboseOutcomes.ToDictionary() : [];
            }); //.VerboseOutcomes.Where(y => (new string[] { "False Positive", "False Negative" }).Contains(y.Value))
            var classificationErrors = verboseTestResults
                .Where(x => x.Errors > 0)
                .Select(x => new ClassificationErrors()
                {
                    Class = x.Class,
                    TP = x.TP,
                    FP = x.FP,
                    FN = x.FN,
                    TN = x.TN,
                    Errors = x.FP + x.FN,
                    VerboseOutcomes = x.VerboseOutcomes,
                        //.Where(y => (new string[] { "False Positive", "False Negative" }).Contains(y.Value))
                        //?.ToDictionary(),

                })
                .OrderByDescending(x => x.Errors)
                .WithProgressReporting(verboseTestResults.Length, progress.SpawnChild(50), sw)
                .ToArray();

            if (SaveWip) { await Serialization.SerializeAndSaveAsync(classificationErrors, progress.SpawnChild(50), "ClassificationErrors[]"); }
            
            return classificationErrors;  
        }

        #endregion Step 7: Diagnose Poor Performers -> ClassificationErrors[]

        #region Step 8: Run Sensitivity Analysis -> ThresholdMetric[]

        public async Task<ThresholdMetric[]> RunSensitivityAsync(VerboseTestOutcome[] verboseTestOutcomes) 
        {
            verboseTestOutcomes ??= await Serialization.DeserializeAsync<VerboseTestOutcome[]>(typeof(VerboseTestOutcome[]).Name);
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
                GroupedTestOutcome[] testResults = GroupOutcomes(testOutcomes);
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

            Serialization.SerializeAndSave(thresholdMetrics, typeof(ThresholdMetric[]).Name);
            return thresholdMetrics;
        }

        public async Task ShowSensitivityChartAsync(ThresholdMetric[] thresholdMetrics) 
        { 
            thresholdMetrics ??= await Serialization.DeserializeAsync<ThresholdMetric[]>(typeof(ThresholdMetric[]).Name);
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

        #endregion Step 8: Run Sensitivity Analysis -> ThresholdMetric[]

        #endregion Classifier Performance Testing

        #region Progress Helpers

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

        private string AdjustProgressTimer(int complete, int count, Stopwatch sw, ref double secondsPerItem, 
            ref double remainingSeconds, ref double elapsedSeconds)
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

        #endregion Progress Helpers

        #region Data Loading and Saving

        public virtual async Task<(TestOutcome[], MinedMailInfo[], BayesianClassifierGroup, ProgressPackage)> LoadIfNullAsync(
            TestOutcome[] testOutcomes, MinedMailInfo[] testSource, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg)
        {
            ppkg ??= await new ProgressPackage().InitializeAsync(progressTrackerPane: _globals.AF.ProgressTracker);
            ppkg.ProgressTrackerPane.Report(0, "Reloading Data If Necessary");

            testOutcomes ??= await Serialization.DeserializeAsync<TestOutcome[]>(ppkg.ProgressTrackerPane, typeof(TestOutcome[]).Name);
            testSource ??= await Serialization.DeserializeAsync<MinedMailInfo[]>(ppkg.ProgressTrackerPane, "Test");
            classifierGroup ??= await Serialization.DeserializeAsync<BayesianClassifierGroup>(ppkg.ProgressTrackerPane, "TestClassifierGroup");

            if (testOutcomes.Length != testSource.Length) { throw new ArgumentException("Test Outcomes and Test Source Lengths Do Not Match"); }
            return (testOutcomes, testSource, classifierGroup, ppkg);
        }

        public virtual async Task<(EmailDataMiner, MinedMailInfo[], List<string>, ProgressPackage)> LoadIfNullAsync(
            EmailDataMiner dataMiner, MinedMailInfo[] collection, ProgressPackage ppkg)
        {
            ppkg ??= await new ProgressPackage().InitializeAsync(progressTrackerPane: _globals.AF.ProgressTracker);
            ppkg.ProgressTrackerPane.Report(0, "Reloading Data If Necessary");

            dataMiner ??= new EmailDataMiner(Globals);
            
            collection ??= await Serialization.DeserializeAsync<MinedMailInfo[]>(ppkg.ProgressTrackerPane, typeof(MinedMailInfo[]).Name);
            var folderPaths = collection.Select(x => x.FolderInfo.RelativePath).OrderBy(x => x).Distinct().ToList();

            return (dataMiner, collection, folderPaths, ppkg);
        }

        public virtual async Task SaveScoresAsync(IEnumerable<TestScores> scores, ProgressTrackerPane progress)
        {
            await Serialization.SerializeAndSaveAsync(scores, progress.SpawnChild(50), "TestScores");
            var scores2 = scores.Select(x => new string[]
                        {
                x.Class, x.TP.ToString(), x.FP.ToString(), x.FN.ToString(), x.TN.ToString(),x.Precision.ToString("0.00"), x.Recall.ToString("0.00"), x.F1.ToString("0.00")
                        }).ToArray();

            progress.Increment(0, "Scores as Text");
            var scoresText = scores2.ToFormattedText(
                ["Class", "TP", "FP", "FN", "TN", "Precision", "Recall", "F1"],
                Enumerable.Repeat(Enums.Justification.Center, 8).ToArray(), "Classifier Performance By Class");
            await Serialization.SaveTextsAsync([scoresText], "TestScores");
            
            progress.Report(100);
            logger.Debug($"\n{scoresText}");
        }

        public virtual async Task SaveScoresAsync(IEnumerable<VerboseTestScores> verboseScores, ProgressTrackerPane progress)
        {
            await Serialization.SerializeAndSaveAsync(verboseScores, progress.SpawnChild(30), "VerboseTestScores[]");
            
            var scores = verboseScores.Select(x => new TestScores
            {
                Class = x.Class,
                TP = x.TP,
                FP = x.FP,
                FN = x.FN,
                TN = x.TN,
                Precision = x.Precision,
                Recall = x.Recall,
                F1 = x.F1
            }).ToList();

            await Serialization.SerializeAndSaveAsync(scores, progress.SpawnChild(30), "TestScores");

            var scores2 = scores.Select(x => new string[]
            {
                x.Class, x.TP.ToString(), x.FP.ToString(), x.FN.ToString(), 
                x.TN.ToString(),x.Precision.ToString("0.00"), x.Recall.ToString("0.00"), x.F1.ToString("0.00")
            }).ToArray();

            var scoresText = scores2.ToFormattedText(
                ["Class", "TP", "FP", "FN", "TN", "Precision", "Recall", "F1"],
                Enumerable.Repeat(Enums.Justification.Center, 8).ToArray(), "Classifier Performance By Class");
            await Serialization.SaveTextsAsync([scoresText], "TestScores");
            logger.Debug($"\n{scoresText}");
            progress.Report(100);
        }

        public virtual async Task<(MinedMailInfo[] Train, MinedMailInfo[] Test)> SplitAndSave(MinedMailInfo[] collection, double trainPercent, ProgressPackage ppkg)
        {
            ppkg.ProgressTrackerPane.Report(0, "Building Folder Classifier -> Split Into Train / Test");
            var (train, test) = collection.SplitTestTrain(0.75);
            ppkg.ProgressTrackerPane.Increment(20);
            await Serialization.SerializeAndSaveAsync(train, ppkg.ProgressTrackerPane.SpawnChild(40), "Train", cancel: ppkg.Cancel);
            await Serialization.SerializeAndSaveAsync(test, ppkg.ProgressTrackerPane.SpawnChild(40), "Test", cancel: ppkg.Cancel);
            return (train, test);
        }

        #endregion Data Loading and Saving

    }

}
