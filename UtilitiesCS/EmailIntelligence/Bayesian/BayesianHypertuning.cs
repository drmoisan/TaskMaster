using log4net.Repository.Hierarchy;
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

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class BayesianHypertuning
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public BayesianHypertuning(IApplicationGlobals globals)
        {
            _globals = globals;
        }

        #endregion Constructors

        #region Public Properties and Types

        private IApplicationGlobals _globals;
        public IApplicationGlobals Globals { get => _globals; }

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

        public record TestResult()
        {
            public string Actual { get; set; }
            public string Predicted { get; set; }
            public int Count { get; set; }
        }

        #endregion Public Properties and Types

        #region Public Methods

        public async Task TestFolderClassifierAsync()
        {
            var dataMiner = new EmailDataMiner(Globals);
            await TestFolderClassifierAsync(dataMiner);
        }

        public async Task TestFolderClassifierAsync(EmailDataMiner dataMiner)
        {
            dataMiner ??= new EmailDataMiner(Globals);

            var ppkg = await new ProgressPackage().InitializeAsync(progressTrackerPane: _globals.AF.ProgressTracker);
            _globals.AF.ProgressPane.Visible = true;
            ppkg.ProgressTrackerPane.Report(0, "Building Folder Classifier -> Load Mined Mail Info");
            var collection = await dataMiner.Load<MinedMailInfo[]>();
            var folderPaths = collection.Select(x => x.FolderInfo.RelativePath).Distinct().ToList();

            ppkg.ProgressTrackerPane.Report(10, "Building Folder Classifier -> Split Into Train / Test");
            var (train, test) = collection.SplitTestTrain(0.75);

            ppkg.ProgressTrackerPane.Report(20, "Building Folder Classifier -> Create Classifier Group");
            var classifierGroup = await dataMiner.CreateClassifierGroupAsync(train);

            ppkg.ProgressTrackerPane.Report(30, "Building Folder Classifier -> Building Classifiers");
            await dataMiner.BuildFolderClassifiersAsync(classifierGroup, train, await new ProgressPackage().InitializeAsync(
                ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(20), ppkg.StopWatch));

            ppkg.ProgressTrackerPane.Report(50, "Building Folder Classifier -> Testing Classifiers");
            var ppkg2 = await new ProgressPackage().InitializeAsync(
                ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(40), ppkg.StopWatch);
            TestResult[] testResults = await TestClassifierAsync(test, classifierGroup, ppkg2);

            ppkg.ProgressTrackerPane.Report(ppkg.ProgressTrackerPane.Progress, "Building Folder Classifier -> Building Confusion Matrix and Calculating Scores");
            ConfusionMatrixCounts[] counts = Count(folderPaths, testResults);
            IEnumerable<TestScores> scores = await CalculateTestScoresAsync(counts);
            LogScores(scores);
            await SaveConfusionMatrixAsync(folderPaths, testResults);

            ppkg.ProgressTrackerPane.Report(100, "Operation Complete");
        }

        #endregion Public Methods

        public async Task SaveConfusionMatrixAsync(List<string> folderPaths, TestResult[] testResults)
        {
            testResults ??= await DeserializeAsync<TestResult[]>("TestResults");
            folderPaths ??= testResults.Select(x => x.Actual).Concat(testResults.Select(x=>x.Predicted)).Distinct().OrderBy(x => x).ToList();

            string[][] jagged = new string[folderPaths.Count()][];
            jagged = jagged.Select(x => new string[folderPaths.Count() + 1]).ToArray();
            jagged.ForEach((x, i) => x[0] = $"{folderPaths[i]}");

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

        public void LogScores(IEnumerable<TestScores> scores)
        {
            var scores2 = scores.Select(x => new string[]
                        {
                x.Class, x.TP.ToString(), x.FP.ToString(), x.FN.ToString(), x.TN.ToString(),x.Precision.ToString("0.00"), x.Recall.ToString("0.00"), x.F1.ToString("0.00")
                        }).ToArray();

            var scoresText = scores2.ToFormattedText(
                ["Class", "TP", "FP", "FN", "TN", "Precision", "Recall", "F1"],
                Enumerable.Repeat(Enums.Justification.Center, 8).ToArray(), "Classifier Performance By Class");
            logger.Debug($"\n{scoresText}");
        }

        private async Task<TestResult[]> TestClassifierAsync(MinedMailInfo[] test, BayesianClassifierGroup classifierGroup, ProgressPackage ppkg)
        {
            TestResult[] testResults = null;
            int completed = 0;
            int count = test.Count();
            var sw = await Task.Run(() => new SegmentStopWatch().Start());
            var testTask = Task.Run(() => testResults =
                [
                    .. test.AsParallel()
                    .Select(x =>
                    (Actual: x.FolderInfo.RelativePath,
                    Predicted: classifierGroup.Classify(x.Tokens.GroupAndCount())
                        .First().Class))
                    .WithAction(() =>
                    {
                        Interlocked.Increment(ref completed);
                        ppkg.ProgressTrackerPane.Report(
                            ((int)(((double)completed / count) * 100),
                            $"Testing Classifier -> {GetProgressMessage(completed, count, sw)}"));
                    })
                    .GroupBy(x => x)
                    .Select(x => new TestResult
                    {
                        Actual = x.Key.Actual,
                        Predicted = x.Key.Predicted,
                        Count = x.Count()
                    }),
                ],
                ppkg.Cancel);

            //TimerWrapper timer = null;

            //await Task.Run(() => 
            //{
            //    var sw = Stopwatch.StartNew();
            //    timer = new TimerWrapper(TimeSpan.FromSeconds(1));
            //    timer.Elapsed += (sender, e) =>
            //    {
            //        if (count > 0)
            //        {
            //            ppkg.ProgressTrackerPane.Report(
            //                ((int)(((double)completed / count) * 100),
            //                $"Testing Classifier -> {GetReportMessage(completed, count, sw)}"));
            //        }
            //    };
            //    timer.AutoReset = true;
            //    timer.StartTimer();
            //}, ppkg.Cancel);


            try
            {
                await testTask;
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
            ppkg.ProgressTrackerPane.Report(100);

            SerializeAndSave(testResults, "TestResults");
            return testResults;
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

        internal ConfusionMatrixCounts[] Count(List<string> folderPaths, TestResult[] testResults)
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

        #region Serialization

        internal virtual T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
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
                Formatting = Formatting.Indented
            };
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
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

        internal virtual async Task SaveTextsAsync(IEnumerable<string> texts, string fileNameSeed, string fileNameSuffix = "")
        {
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.txt" : $"{fileNameSeed}_{fileNameSuffix}.txt";
            disk.FileName = fileName;
            if (File.Exists(disk.FilePath)) { File.Delete(disk.FilePath); }
            await WriteTextsAsync(disk.FilePath, texts);
        }

        internal virtual async Task SaveCsvAsync(string[][] jagged, string fileNameSeed, string fileNameSuffix = "")
        {
            var texts = jagged.Select(x => x.StringJoin(",")).ToArray();
            await SaveTextsAsync(texts, fileNameSeed, fileNameSuffix);
        }
        
        internal virtual void SerializeAndSave<T>(T obj, string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
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
