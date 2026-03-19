using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderScorerTests
    {
        [TestMethod]
        public void AddSuggestion_ShouldAggregateScoresForExistingFolder()
        {
            var scorer = new FolderScorer();

            scorer.AddSuggestion("Archive\\Finance", 3);
            scorer.AddSuggestion("Archive\\Finance", 7);

            scorer.Count.Should().Be(1);
            scorer.ToArray().Should().Equal("Archive\\Finance");
        }

        [TestMethod]
        public void AddSuggestion_WithNullOrErrorObject_ShouldReturnFalse()
        {
            var scorer = new FolderScorer();

            var nullResult = scorer.AddSuggestion((object)null, 1);
            var errorResult = scorer.AddSuggestion((object)"Error", 1);

            nullResult.Should().BeFalse();
            errorResult.Should().BeFalse();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void ToArray_WithTopN_ShouldTrimToHighestScores()
        {
            var scorer = new FolderScorer();
            scorer.AddSuggestion("One", 1);
            scorer.AddSuggestion("Two", 5);
            scorer.AddSuggestion("Three", 3);

            var result = scorer.ToArray(2);

            result.Should().Equal("Two", "Three");
        }

        [TestMethod]
        public void AddArray_ShouldAddEachFolderAndRespectTopN()
        {
            var scorer = new FolderScorer();

            var result = scorer.AddArray(new[] { "NewA", "NewB", "NewC" }, 2);

            result.Should().BeTrue();
            scorer.Count.Should().Be(2);
            scorer.ToArray().Should().Equal("NewA", "NewB");
        }

        [TestMethod]
        public void Indexer_WhenIndexIsOutOfRange_ShouldThrow()
        {
            var scorer = new FolderScorer();
            scorer.AddSuggestion("Only", 1);

            Action act = () => _ = scorer[1];

            act.Should().Throw<IndexOutOfRangeException>();
        }

        [TestMethod]
        public void QueryCombined_ShouldMergeDuplicateFolderScoresAndKeepTopResults()
        {
            var scorer = new FolderScorer();
            var nestedType =
                typeof(FolderScorer).GetNestedType("FolderScoring", BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("FolderScoring type not found.");
            var create =
                typeof(FolderScorerTests).GetMethod(
                    nameof(CreateFolderScoring),
                    BindingFlags.Static | BindingFlags.NonPublic
                ) ?? throw new InvalidOperationException("Factory method not found.");
            var queryCombined =
                typeof(FolderScorer)
                    .GetMethods(
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    )
                    .SingleOrDefault(m =>
                        m.Name == "QueryCombined"
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[0].ParameterType.IsGenericType
                        && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
                            == typeof(System.Collections.Generic.IEnumerable<>)
                        && m.GetParameters()[1].ParameterType.IsGenericType
                        && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition()
                            == typeof(System.Collections.Generic.IEnumerable<>)
                    )
                ?? throw new InvalidOperationException("QueryCombined overload not found.");

            var subjectEntries = Array.CreateInstance(nestedType, 2);
            subjectEntries.SetValue(
                create.Invoke(null, new object[] { nestedType, "Inbox\\Alpha", "Alpha", 8 }),
                0
            );
            subjectEntries.SetValue(
                create.Invoke(null, new object[] { nestedType, "Inbox\\Beta", "Beta", 5 }),
                1
            );

            var folderEntries = Array.CreateInstance(nestedType, 2);
            folderEntries.SetValue(
                create.Invoke(null, new object[] { nestedType, "Inbox\\Alpha", "Alpha", 4 }),
                0
            );
            folderEntries.SetValue(
                create.Invoke(null, new object[] { nestedType, "Inbox\\Gamma", "Gamma", 6 }),
                1
            );

            var result = (
                (IEnumerable)
                    queryCombined.Invoke(scorer, new object[] { subjectEntries, folderEntries })
            )
                .Cast<object>()
                .Select(entry => new
                {
                    FolderPath = (string)nestedType.GetField("FolderPath")!.GetValue(entry),
                    Score = (int)nestedType.GetField("Score")!.GetValue(entry),
                })
                .ToArray();

            result.Should().HaveCount(3);
            result[0].FolderPath.Should().Be("Inbox\\Alpha");
            result[0].Score.Should().Be(12);
            result
                .Select(x => x.FolderPath)
                .Should()
                .Contain(new[] { "Inbox\\Beta", "Inbox\\Gamma" });
        }

        private static object CreateFolderScoring(
            Type nestedType,
            string folderPath,
            string folderName,
            int score
        )
        {
            var instance = Activator.CreateInstance(nestedType);
            nestedType.GetField("FolderPath")!.SetValue(instance, folderPath);
            nestedType.GetField("FolderName")!.SetValue(instance, folderName);
            nestedType.GetField("FolderEncoding")!.SetValue(instance, Array.Empty<int>());
            nestedType.GetField("FolderWordLengths")!.SetValue(instance, Array.Empty<int>());
            nestedType.GetField("Score")!.SetValue(instance, score);
            return instance!;
        }
    }
}
