using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianPerformanceMeasurement_Tests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IFileSystemFolderPaths> _mockFileSystem;
        private Mock<IAppAutoFileObjects> _mockAutoFiles;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());

            _mockRepository = new MockRepository(MockBehavior.Loose);
            _mockGlobals = _mockRepository.Create<IApplicationGlobals>();
            _mockGlobals.SetupAllProperties();
            _mockFileSystem = _mockRepository.Create<IFileSystemFolderPaths>();
            _mockFileSystem
                .SetupGet(x => x.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string>());
            _mockAutoFiles = _mockRepository.Create<IAppAutoFileObjects>();
            _mockAutoFiles
                .SetupGet(x => x.ProgressTracker)
                .Returns(CreateFakeProgressTrackerPane());
            _mockGlobals.SetupGet(x => x.FS).Returns(_mockFileSystem.Object);
            _mockGlobals.SetupGet(x => x.AF).Returns(_mockAutoFiles.Object);
        }

        [TestMethod]
        public void Constructor_SetsGlobals()
        {
            // Act
            var sut = new BayesianPerformanceMeasurement(_mockGlobals.Object);

            // Assert
            sut.Globals.Should().BeSameAs(_mockGlobals.Object);
        }

        [TestMethod]
        public void SaveWip_DefaultsToTrue()
        {
            // Act
            var sut = new BayesianPerformanceMeasurement(_mockGlobals.Object);

            // Assert
            sut.SaveWip.Should().BeTrue();
        }

        [TestMethod]
        public void SaveWip_SetAndGet_Works()
        {
            // Arrange
            var sut = new BayesianPerformanceMeasurement(_mockGlobals.Object);

            // Act
            sut.SaveWip = false;

            // Assert
            sut.SaveWip.Should().BeFalse();
        }

        [TestMethod]
        public void Serialization_IsSetInConstructor()
        {
            // Act
            var sut = new BayesianPerformanceMeasurement(_mockGlobals.Object);

            // Assert
            sut.Serialization.Should().NotBeNull();
            sut.Serialization.Globals.Should().BeSameAs(_mockGlobals.Object);
        }

        [TestMethod]
        public void GroupOutcomes_WithTestOutcomes_GroupsByActualAndPredicted()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);

            // Act
            var results = sut.GroupOutcomes([
                new TestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Inbox",
                    SourceIndex = 0,
                },
                new TestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Archive",
                    SourceIndex = 1,
                },
                new TestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Inbox",
                    SourceIndex = 2,
                },
            ]);

            // Assert
            results.Should().HaveCount(2);
            results[0].Actual.Should().Be("Inbox");
            results[0].Predicted.Should().Be("Inbox");
            results[0].Count.Should().Be(2);
            results[1].Predicted.Should().Be("Archive");
            serialization.StoredObjects.Should().ContainKey("GroupedTestOutcome[].json");
        }

        [TestMethod]
        public void GroupOutcomes_WithVerboseTestOutcomes_PersistsVerboseAndSimpleResults()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);
            var source = CreateMinedMailInfo("Inbox", "alpha", "shared");

            // Act
            var results = sut.GroupOutcomes([
                new VerboseTestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Inbox",
                    Source = source,
                    SourceIndex = 0,
                    Drivers = [("alpha", 0.8)],
                    Probability = 0.8,
                },
                new VerboseTestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Archive",
                    Source = source,
                    SourceIndex = 1,
                    Drivers = [("shared", 0.4)],
                    Probability = 0.4,
                },
            ]);

            // Assert
            results.Should().HaveCount(2);
            serialization.StoredObjects.Should().ContainKey("VerboseGroupedTestOutcome[].json");
            serialization.StoredObjects.Should().ContainKey("GroupedTestOutcome[].json");
        }

        [TestMethod]
        public void CountHitsMisses_WithGroupedTestOutcomes_ComputesCountsPerFolder()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);

            // Act
            var counts = sut.CountHitsMisses(
                ["Archive", "Inbox"],
                [
                    new GroupedTestOutcome
                    {
                        Actual = "Inbox",
                        Predicted = "Inbox",
                        Count = 3,
                    },
                    new GroupedTestOutcome
                    {
                        Actual = "Inbox",
                        Predicted = "Archive",
                        Count = 1,
                    },
                    new GroupedTestOutcome
                    {
                        Actual = "Archive",
                        Predicted = "Inbox",
                        Count = 2,
                    },
                ]
            );

            // Assert
            counts.Should().HaveCount(2);
            counts
                .Single(x => x.Class == "Inbox")
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        Class = "Inbox",
                        TP = 1,
                        FP = 1,
                        FN = 1,
                        TN = 0,
                    }
                );
            counts
                .Single(x => x.Class == "Archive")
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        Class = "Archive",
                        TP = 0,
                        FP = 1,
                        FN = 1,
                        TN = 1,
                    }
                );
            serialization.StoredObjects.Should().ContainKey("ClassCounts[].json");
        }

        [TestMethod]
        public void CountHitsMisses_WithVerboseGroupedOutcomes_ComputesVerboseCounts()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);
            var inboxSource = CreateMinedMailInfo("Inbox", "alpha");
            var archiveSource = CreateMinedMailInfo("Archive", "beta");

            // Act
            var counts = sut.CountHitsMisses(
                ["Archive", "Inbox"],
                [
                    new VerboseGroupedTestOutcome
                    {
                        Actual = "Inbox",
                        Predicted = "Inbox",
                        Count = 2,
                        Details =
                        [
                            new VerboseTestOutcome
                            {
                                Actual = "Inbox",
                                Predicted = "Inbox",
                                Source = inboxSource,
                            },
                        ],
                    },
                    new VerboseGroupedTestOutcome
                    {
                        Actual = "Archive",
                        Predicted = "Inbox",
                        Count = 1,
                        Details =
                        [
                            new VerboseTestOutcome
                            {
                                Actual = "Archive",
                                Predicted = "Inbox",
                                Source = archiveSource,
                            },
                        ],
                    },
                ]
            );

            // Assert
            counts.Should().HaveCount(2);
            counts.Single(x => x.Class == "Inbox").Errors.Should().Be(1);
            counts
                .Single(x => x.Class == "Inbox")
                .VerboseOutcomes.Values.Should()
                .Contain(["TruePositive", "FalsePositive"]);
            counts.Single(x => x.Class == "Archive").Errors.Should().Be(1);
            counts
                .Single(x => x.Class == "Archive")
                .VerboseOutcomes.Values.Should()
                .ContainSingle()
                .Which.Should()
                .Be("FalseNegative");
            serialization.StoredObjects.Should().ContainKey("VerboseClassCounts[].json");
            serialization.StoredObjects.Should().ContainKey("ClassCounts[].json");
        }

        [TestMethod]
        public void GetResultType_ReturnsExpectedLabels()
        {
            // Arrange
            var sut = CreateMeasurement();

            // Act / Assert
            sut.GetResultType("Inbox", "Inbox", "Inbox").Should().Be("TruePositive");
            sut.GetResultType("Inbox", "Archive", "Inbox").Should().Be("FalsePositive");
            sut.GetResultType("Inbox", "Inbox", "Archive").Should().Be("FalseNegative");
            sut.GetResultType("Inbox", "Archive", "Drafts").Should().Be("TrueNegative");
        }

        [TestMethod]
        public void CalculateTestScores_WithNullCounts_UsesSerializedCountsAndAddsTotal()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            serialization.StoredObjects["ClassCounts[].json"] = new ClassCounts[]
            {
                new ClassCounts
                {
                    Class = "Inbox",
                    TP = 3,
                    FP = 1,
                    FN = 1,
                    TN = 5,
                },
                new ClassCounts
                {
                    Class = "Archive",
                    TP = 2,
                    FP = 0,
                    FN = 2,
                    TN = 6,
                },
            };
            var sut = CreateMeasurement(serialization);

            // Act
            var scores = sut.CalculateTestScores(null).ToArray();

            // Assert
            scores.Should().HaveCount(3);
            scores.Single(x => x.Class == "Inbox").Precision.Should().Be(0.75);
            scores.Single(x => x.Class == "Archive").Recall.Should().Be(0.5);
            scores.Last().Class.Should().Be("TOTAL");
            scores.Last().TP.Should().Be(5);
        }

        [TestMethod]
        public async Task CalculateTestScoresAsync_WithNullCounts_UsesSerializedCountsAndAddsTotal()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            serialization.StoredObjects["ClassCounts[].json"] = new ClassCounts[]
            {
                new ClassCounts
                {
                    Class = "Inbox",
                    TP = 1,
                    FP = 1,
                    FN = 0,
                    TN = 4,
                },
            };
            var sut = CreateMeasurement(serialization);

            // Act
            var scores = (await sut.CalculateTestScoresAsync((ClassCounts[])null)).ToArray();

            // Assert
            scores.Should().HaveCount(2);
            scores[0].F1.Should().BeApproximately(0.6666666667, 0.000001);
            scores[1].Class.Should().Be("TOTAL");
        }

        [TestMethod]
        public async Task CalculateVerboseTestScoresAsync_WithNullDetails_UsesSerializedDetailsAndAddsTotal()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            serialization.StoredObjects["VerboseClassCounts[].json"] = new VerboseClassCounts[]
            {
                new VerboseClassCounts
                {
                    Class = "Inbox",
                    TP = 2,
                    FP = 1,
                    FN = 1,
                    TN = 3,
                    Errors = 2,
                    VerboseOutcomes = new Dictionary<VerboseTestOutcome, string>(),
                },
            };
            var sut = CreateMeasurement(serialization);

            // Act
            var scores = (await sut.CalculateTestScoresAsync((VerboseClassCounts[])null)).ToArray();

            // Assert
            scores.Should().HaveCount(2);
            scores[0].Errors.Should().Be(2);
            scores[1].Class.Should().Be("TOTAL");
            scores[1].Errors.Should().Be(2);
        }

        [TestMethod]
        public async Task BuildConfusionMatrixAsync_WhenFolderPathsMissing_LoadsResultsAndSavesOutputs()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            serialization.StoredObjects["GroupedTestOutcome[].json"] = new GroupedTestOutcome[]
            {
                new GroupedTestOutcome
                {
                    Actual = "Archive",
                    Predicted = "Archive",
                    Count = 2,
                },
                new GroupedTestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Archive",
                    Count = 1,
                },
                new GroupedTestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Inbox",
                    Count = 3,
                },
            };
            var sut = CreateMeasurement(serialization);

            // Act
            await sut.BuildConfusionMatrixAsync((List<string>)null, (GroupedTestOutcome[])null);

            // Assert
            serialization.StoredCsv.Should().ContainKey("ConfusionMatrix.csv");
            serialization.StoredTexts.Should().ContainKey("ConfusionMatrixText.txt");
            serialization.StoredCsv["ConfusionMatrix.csv"].Should().HaveCount(3);
            serialization
                .StoredTexts["ConfusionMatrixText.txt"]
                .Should()
                .Contain(x => !string.IsNullOrWhiteSpace(x));
        }

        [TestMethod]
        public async Task BuildConfusionMatrixAsync_WithVerboseResults_ConvertsToSimpleResults()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);

            // Act
            await sut.BuildConfusionMatrixAsync(
                ["Archive", "Inbox"],
                [
                    new VerboseGroupedTestOutcome
                    {
                        Actual = "Inbox",
                        Predicted = "Archive",
                        Count = 2,
                        Details = Array.Empty<VerboseTestOutcome>(),
                    },
                ]
            );

            // Assert
            serialization.StoredCsv.Should().ContainKey("ConfusionMatrix.csv");
            serialization.StoredTexts.Should().ContainKey("ConfusionMatrixText.txt");
        }

        [TestMethod]
        public async Task SaveScoresAsync_WithTestScores_SerializesJsonAndText()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);

            // Act
            await sut.SaveScoresAsync(
                [
                    new TestScores
                    {
                        Class = "Inbox",
                        TP = 1,
                        FP = 2,
                        FN = 3,
                        TN = 4,
                        Precision = 0.25,
                        Recall = 0.5,
                        F1 = 0.33,
                    },
                ],
                CreateFakeProgressTrackerPane()
            );

            // Assert
            serialization.StoredObjects.Should().ContainKey("TestScores.json");
            serialization.StoredTexts.Should().ContainKey("TestScores.txt");
            serialization
                .StoredTexts["TestScores.txt"][0]
                .Should()
                .Contain("Classifier Performance By Class");
        }

        [TestMethod]
        public async Task SaveScoresAsync_WithVerboseScores_SerializesVerboseAndSimpleForms()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);

            // Act
            await sut.SaveScoresAsync(
                [
                    new VerboseTestScores
                    {
                        Class = "Inbox",
                        TP = 2,
                        FP = 1,
                        FN = 0,
                        TN = 3,
                        Errors = 1,
                        Precision = 0.67,
                        Recall = 1,
                        F1 = 0.8,
                        VerboseOutcomes = new Dictionary<VerboseTestOutcome, string>(),
                    },
                ],
                CreateFakeProgressTrackerPane()
            );

            // Assert
            serialization.StoredObjects.Should().ContainKey("VerboseTestScores[].json");
            serialization.StoredObjects.Should().ContainKey("TestScores.json");
            serialization.StoredTexts.Should().ContainKey("TestScores.txt");
        }

        [TestMethod]
        public async Task SplitAndSave_SerializesTrainAndTestPartitions()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);
            var collection = Enumerable
                .Range(0, 8)
                .Select(i => CreateMinedMailInfo($"Folder{i % 2}", $"token-{i}"))
                .ToArray();

            // Act
            var (train, test) = await sut.SplitAndSave(collection, 0.5, CreateProgressPackage());

            // Assert
            train.Length.Should().BeGreaterThan(0);
            test.Length.Should().BeGreaterThan(0);
            (train.Length + test.Length).Should().Be(collection.Length);
            serialization.StoredObjects.Should().ContainKey("Train.json");
            serialization.StoredObjects.Should().ContainKey("Test.json");
        }

        [TestMethod]
        public async Task LoadIfNullAsync_WithSerializedInputs_LoadsMissingValues()
        {
            // Arrange
            var group = CreateClassifierGroup();
            var testOutcomes = new[]
            {
                new TestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Inbox",
                    SourceIndex = 0,
                },
            };
            var testSource = new[] { CreateMinedMailInfo("Inbox", "alpha", "shared") };
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            serialization.StoredObjects["TestOutcome[].json"] = testOutcomes;
            serialization.StoredObjects["Test.json"] = testSource;
            serialization.StoredObjects["TestClassifierGroup.json"] = group;
            var sut = CreateMeasurement(serialization);

            // Act
            var (loadedOutcomes, loadedSource, loadedGroup, loadedPackage) =
                await sut.LoadIfNullAsync((TestOutcome[])null, null, null, CreateProgressPackage());

            // Assert
            loadedOutcomes.Should().BeEquivalentTo(testOutcomes);
            loadedSource.Should().BeEquivalentTo(testSource);
            loadedGroup.Should().BeSameAs(group);
            loadedPackage.Should().NotBeNull();
        }

        [TestMethod]
        public async Task LoadIfNullAsync_WhenLengthsMismatch_ThrowsArgumentException()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            serialization.StoredObjects["TestOutcome[].json"] = new TestOutcome[]
            {
                new TestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Inbox",
                    SourceIndex = 0,
                },
            };
            serialization.StoredObjects["Test.json"] = Array.Empty<MinedMailInfo>();
            serialization.StoredObjects["TestClassifierGroup.json"] = CreateClassifierGroup();
            var sut = CreateMeasurement(serialization);

            // Act
            Func<Task> act = async () =>
                await sut.LoadIfNullAsync((TestOutcome[])null, null, null, CreateProgressPackage());

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*Lengths Do Not Match*");
        }

        [TestMethod]
        public async Task LoadIfNullAsync_ForFolderClassifierInputs_ReturnsDistinctFolderPaths()
        {
            // Arrange
            var miner =
                new Mock<UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup>(
                    _mockGlobals.Object
                )
                {
                    CallBase = true,
                };
            var collection = new[]
            {
                CreateMinedMailInfo("Inbox", "alpha"),
                CreateMinedMailInfo("Archive", "beta"),
                CreateMinedMailInfo("Inbox", "gamma"),
            };
            var sut = CreateMeasurement();

            // Act
            var (dataMiner, loadedCollection, folderPaths, package) = await sut.LoadIfNullAsync(
                miner.Object,
                collection,
                CreateProgressPackage()
            );

            // Assert
            dataMiner.Should().BeSameAs(miner.Object);
            loadedCollection.Should().BeSameAs(collection);
            folderPaths.Should().Equal("Archive", "Inbox");
            package.Should().NotBeNull();
        }

        [TestMethod]
        public void GetVerboseTestDetails_ReturnsProbabilityDriversForPredictedClassifier()
        {
            // Arrange
            var sut = CreateMeasurement();
            var classifierGroup = CreateClassifierGroup();
            var testSource = new[] { CreateMinedMailInfo("Inbox", "alpha", "shared") };

            // Act
            var details = sut.GetVerboseTestDetails(
                [
                    new TestOutcome
                    {
                        Actual = "Inbox",
                        Predicted = "Inbox",
                        SourceIndex = 0,
                    },
                ],
                testSource,
                classifierGroup
            );

            // Assert
            details.Should().HaveCount(1);
            details[0].Actual.Should().Be("Inbox");
            details[0].Predicted.Should().Be("Inbox");
            details[0].Drivers.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task DiagnosePoorPerformanceAsync_WithConfusedOutcomes_ReturnsClassificationErrors()
        {
            // Arrange
            var sut = CreateMeasurement();
            sut.SaveWip = false;
            var classifierGroup = CreateClassifierGroup();
            var testSource = new[]
            {
                CreateMinedMailInfo("Archive", "alpha", "shared"),
                CreateMinedMailInfo("Inbox", "beta", "shared"),
            };
            var confusedOutcomes = new[]
            {
                new TestOutcome
                {
                    Actual = "Archive",
                    Predicted = "Inbox",
                    SourceIndex = 0,
                },
                new TestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Archive",
                    SourceIndex = 1,
                },
            };
            var testScores = new[]
            {
                new TestScores
                {
                    Class = "Inbox",
                    TP = 1,
                    FP = 1,
                    FN = 1,
                    TN = 0,
                    Precision = 0.5,
                    Recall = 0.5,
                    F1 = 0.5,
                },
                new TestScores
                {
                    Class = "TOTAL",
                    TP = 1,
                    FP = 1,
                    FN = 1,
                    TN = 0,
                },
            };

            // Act
            var errors = await sut.DiagnosePoorPerformanceAsync(
                testSource,
                classifierGroup,
                CreateProgressPackage(),
                confusedOutcomes,
                testScores
            );

            // Assert
            errors.Should().HaveCount(1);
            errors[0].Class.Should().Be("Inbox");
            errors[0].Errors.Should().Be(2);
            errors[0].VerboseOutcomes.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task DiagnosePoorPerformanceAsync_WithVerboseScores_FiltersOnlyFalseResults()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            var sut = CreateMeasurement(serialization);
            var verboseOutcome = new VerboseTestOutcome
            {
                Actual = "Inbox",
                Predicted = "Archive",
                Source = CreateMinedMailInfo("Inbox", "alpha"),
                Drivers = [("alpha", 0.9)],
                Probability = 0.9,
            };

            // Act
            var errors = await sut.DiagnosePoorPerformanceAsync(
                [
                    new VerboseTestScores
                    {
                        Class = "Inbox",
                        TP = 2,
                        FP = 1,
                        FN = 1,
                        TN = 0,
                        Errors = 2,
                        Precision = 0.5,
                        Recall = 0.5,
                        F1 = 0.5,
                        VerboseOutcomes = new Dictionary<VerboseTestOutcome, string>
                        {
                            [verboseOutcome] = "FalsePositive",
                            [new VerboseTestOutcome { Actual = "Inbox", Predicted = "Inbox" }] =
                                "TruePositive",
                        },
                    },
                    new VerboseTestScores
                    {
                        Class = "TOTAL",
                        Errors = 2,
                        VerboseOutcomes = new Dictionary<VerboseTestOutcome, string>(),
                    },
                ],
                CreateFakeProgressTrackerPane()
            );

            // Assert
            errors.Should().HaveCount(1);
            errors[0].Class.Should().Be("Inbox");
            errors[0].VerboseOutcomes.Should().ContainSingle();
            serialization.StoredObjects.Should().ContainKey("ClassificationErrors[].json");
        }

        [TestMethod]
        public async Task RunSensitivityAsync_WithNullInput_LoadsSerializedVerboseOutcomes()
        {
            // Arrange
            var serialization = new RecordingSerializationHelper(_mockGlobals.Object);
            serialization.StoredObjects["VerboseTestOutcome[].json"] = new VerboseTestOutcome[]
            {
                new VerboseTestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Inbox",
                    SourceIndex = 0,
                    Probability = 0.95,
                },
                new VerboseTestOutcome
                {
                    Actual = "Inbox",
                    Predicted = "Archive",
                    SourceIndex = 1,
                    Probability = 0.30,
                },
                new VerboseTestOutcome
                {
                    Actual = "Archive",
                    Predicted = "Archive",
                    SourceIndex = 2,
                    Probability = 0.85,
                },
            };
            var sut = CreateMeasurement(serialization);

            // Act
            var thresholds = await sut.RunSensitivityAsync(null);

            // Assert
            thresholds.Should().HaveCount(100);
            thresholds[0].Threshold.Should().Be(0);
            thresholds[^1].Threshold.Should().Be(0.99);
            serialization.StoredObjects.Should().ContainKey("ThresholdMetric[].json");
        }

        [TestMethod]
        public void PrivateProgressHelpers_FormatExpectedMessages()
        {
            // Arrange
            var sut = CreateMeasurement();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread.Sleep(20);
            double secondsPerItem = 0;
            double remainingSeconds = 10;
            double elapsedSeconds = 0;

            // Act
            var progressMessage = (string)InvokeNonPublic(
                sut,
                "GetProgressMessage",
                new[]
                {
                    typeof(int),
                    typeof(int),
                    typeof(Stopwatch),
                    typeof(double).MakeByRefType(),
                    typeof(double).MakeByRefType(),
                },
                2,
                4,
                stopwatch,
                secondsPerItem,
                remainingSeconds
            );
            var adjusted = (string)InvokeNonPublic(
                sut,
                "AdjustProgressTimer",
                new[]
                {
                    typeof(int),
                    typeof(int),
                    typeof(Stopwatch),
                    typeof(double).MakeByRefType(),
                    typeof(double).MakeByRefType(),
                    typeof(double).MakeByRefType(),
                },
                1,
                4,
                stopwatch,
                secondsPerItem,
                remainingSeconds,
                elapsedSeconds
            );

            // Assert
            progressMessage.Should().Contain("Completed 2 of 4");
            adjusted.Should().Contain("Completed 1 of 4");
        }

        private BayesianPerformanceMeasurement CreateMeasurement(
            RecordingSerializationHelper serialization = null
        )
        {
            var measurement = new BayesianPerformanceMeasurement(_mockGlobals.Object);
            measurement.Serialization =
                serialization ?? new RecordingSerializationHelper(_mockGlobals.Object);
            return measurement;
        }

        private static BayesianClassifierGroup CreateClassifierGroup()
        {
            var group = new BayesianClassifierGroup();
            group.Train("Inbox", new[] { "alpha", "shared", "alpha" }, 1);
            group.Train("Archive", new[] { "beta", "shared", "beta" }, 1);
            return group;
        }

        private static MinedMailInfo CreateMinedMailInfo(
            string relativePath,
            params string[] tokens
        )
        {
            var folder = new Mock<IFolderWrapper>(MockBehavior.Loose);
            folder.SetupGet(x => x.RelativePath).Returns(relativePath);

            return new MinedMailInfo
            {
                FolderInfo = folder.Object,
                Tokens = tokens,
                Subject = $"Subject-{relativePath}",
            };
        }

        private static ProgressPackage CreateProgressPackage()
        {
            var cancelSource = new CancellationTokenSource();
            return new ProgressPackage
            {
                CancelSource = cancelSource,
                Cancel = cancelSource.Token,
                ProgressTrackerPane = CreateFakeProgressTrackerPane(),
                StopWatch = new SegmentStopWatch().Start(),
            };
        }

        public static ProgressTrackerPane CreateFakeProgressTrackerPane()
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var parentField = typeof(ProgressTrackerPane).GetField(
                "_parent",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var parentType = parentField.FieldType;
            var rootProgress = new Progress<(int Value, string JobName)>(_ => { });
            var parent = Activator.CreateInstance(parentType, rootProgress, 100, 0);
            parentField.SetValue(pane, parent);
            typeof(ProgressTrackerPane)
                .GetField("_isRoot", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, false);
            typeof(ProgressTrackerPane)
                .GetField("_progress", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, 0d);
            typeof(ProgressTrackerPane)
                .GetField("_jobName", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, string.Empty);
            return pane;
        }

        private static object InvokeNonPublic(
            object target,
            string methodName,
            params object[] args
        )
        {
            var method = target
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Should().NotBeNull();
            return method.Invoke(target, args);
        }

        private static object InvokeNonPublic(
            object target,
            string methodName,
            Type[] parameterTypes,
            params object[] args
        )
        {
            var method = target
                .GetType()
                .GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: parameterTypes,
                    modifiers: null
                );
            method.Should().NotBeNull();
            return method.Invoke(target, args);
        }

        private sealed class RecordingSerializationHelper : BayesianSerializationHelper
        {
            public RecordingSerializationHelper(IApplicationGlobals globals)
                : base(globals) { }

            public Dictionary<string, object> StoredObjects { get; } = new();
            public Dictionary<string, string[][]> StoredCsv { get; } = new();
            public Dictionary<string, string[]> StoredTexts { get; } = new();

            public override T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
            {
                return StoredObjects.TryGetValue(
                    GetKey(fileNameSeed, fileNameSuffix),
                    out var value
                )
                    ? (T)value
                    : default;
            }

            public override Task<T> DeserializeAsync<T>(
                string fileNameSeed,
                string fileNameSuffix = ""
            )
            {
                return Task.FromResult(Deserialize<T>(fileNameSeed, fileNameSuffix));
            }

            public override Task<T> DeserializeAsync<T>(
                ProgressTrackerPane progress,
                string fileNameSeed,
                string fileNameSuffix = "",
                string fileExtension = ".json"
            )
            {
                return Task.FromResult(
                    StoredObjects.TryGetValue(
                        GetKey(fileNameSeed, fileNameSuffix, fileExtension),
                        out var value
                    )
                        ? (T)value
                        : default(T)
                );
            }

            public override void SerializeAndSave<T>(
                T obj,
                string fileNameSeed,
                string fileNameSuffix = ""
            )
            {
                StoredObjects[GetKey(fileNameSeed, fileNameSuffix)] = obj;
            }

            public override Task SerializeAndSaveAsync<T>(
                T obj,
                ProgressTrackerPane progress,
                string fileNameSeed,
                string fileNameSuffix = "",
                string fileExtension = ".json",
                string progressPrefix = "",
                CancellationToken cancel = default
            )
            {
                StoredObjects[GetKey(fileNameSeed, fileNameSuffix, fileExtension)] = obj;
                return Task.CompletedTask;
            }

            public override Task SaveTextsAsync(
                IEnumerable<string> texts,
                string fileNameSeed,
                string fileNameSuffix = "",
                string fileExtension = ".txt"
            )
            {
                StoredTexts[GetKey(fileNameSeed, fileNameSuffix, fileExtension)] = texts.ToArray();
                return Task.CompletedTask;
            }

            public override Task SaveCsvAsync(
                string[][] jagged,
                string fileNameSeed,
                string fileNameSuffix = ""
            )
            {
                StoredCsv[GetKey(fileNameSeed, fileNameSuffix, ".csv")] = jagged;
                return Task.CompletedTask;
            }

            private static string GetKey(
                string fileNameSeed,
                string fileNameSuffix = "",
                string extension = ".json"
            ) =>
                string.IsNullOrEmpty(fileNameSuffix)
                    ? $"{fileNameSeed}{extension}"
                    : $"{fileNameSeed}_{fileNameSuffix}{extension}";
        }
    }
}
