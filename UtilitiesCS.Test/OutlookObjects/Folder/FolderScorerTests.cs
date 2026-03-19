using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outlook = Microsoft.Office.Interop.Outlook;

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

        [TestMethod]
        public void QueryCombined_WithParallelQueries_MergesScores()
        {
            var scorer = new FolderScorer();
            var querySubject = new[]
            {
                new FolderScorer.FolderScoring
                {
                    FolderPath = "Inbox\\Alpha",
                    FolderName = "Alpha",
                    Score = 7,
                },
                new FolderScorer.FolderScoring
                {
                    FolderPath = "Inbox\\Beta",
                    FolderName = "Beta",
                    Score = 3,
                },
            }.AsParallel();
            var queryFolder = new[]
            {
                new FolderScorer.FolderScoring
                {
                    FolderPath = "Inbox\\Alpha",
                    FolderName = "Alpha",
                    Score = 2,
                },
                new FolderScorer.FolderScoring
                {
                    FolderPath = "Inbox\\Gamma",
                    FolderName = "Gamma",
                    Score = 4,
                },
            }.AsParallel();

            var result = scorer.QueryCombined(querySubject, queryFolder).ToList();

            result.Should().ContainSingle(x => x.FolderPath == "Inbox\\Alpha" && x.Score == 9);
            result
                .Select(x => x.FolderPath)
                .Should()
                .Contain(new[] { "Inbox\\Beta", "Inbox\\Gamma" });
        }

        [TestMethod]
        public void QueryAddOlFolderKeys_WhenFolderKeyArrayExists_UsesObjectArrayBranch()
        {
            var property = new Mock<Outlook.UserProperty>();
            property.SetupGet(x => x.Value).Returns(new[] { "Archive\\Finance", "Archive\\Ops" });
            var userProperties = new Mock<Outlook.UserProperties>();
            userProperties.Setup(x => x.Find("FolderKey")).Returns(property.Object);
            var mailItem = new Mock<Outlook.MailItem>();
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            var scorer = new FolderScorer();

            var result = scorer.AddOlFolderKeys(
                mailItem.Object,
                new Mock<IApplicationGlobals>().Object,
                topN: 1
            );

            result.Should().BeTrue();
            scorer.ToArray().Should().Equal("Archive\\Finance");
        }

        [TestMethod]
        public void QueryAddOlFolderKeys_WhenFolderKeyIsMissing_ReturnsFalse()
        {
            var userProperties = new Mock<Outlook.UserProperties>();
            userProperties.Setup(x => x.Find("FolderKey")).Returns((Outlook.UserProperty)null);
            var mailItem = new Mock<Outlook.MailItem>();
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            var scorer = new FolderScorer();

            var result = scorer.AddOlFolderKeys(
                mailItem.Object,
                new Mock<IApplicationGlobals>().Object
            );

            result.Should().BeFalse();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void QueryFromArray_ShouldReplaceExistingSuggestionsBeforeAddingArrayValues()
        {
            var scorer = new FolderScorer();
            scorer.AddSuggestion("Existing", 10);

            scorer.FromArray(new[] { "NewA", "NewB" });

            scorer.ToArray().Should().Equal("NewA", "NewB");
        }

        [TestMethod]
        public void AddConversationBasedSuggestions_WhenConversationExists_AddsWeightedMatches()
        {
            var scorer = new FolderScorer();
            var mailItem = new Mock<Outlook.MailItem>();
            mailItem.SetupGet(x => x.ConversationID).Returns("conv-1");
            var globals = CreateGlobalsWithAutoFiles(
                ctfMap: new CtfMap
                {
                    new CtfMapEntry("Inbox\\Projects", "conv-1", 4),
                    new CtfMapEntry("Archive\\Plans", "conv-1", 2),
                }
            );

            scorer.AddConversationBasedSuggestions(mailItem.Object, globals.Object, topN: 5);

            scorer.ToArray().Should().Equal("Inbox\\Projects", "Archive\\Plans");
        }

        [TestMethod]
        public void LoadFromField_WhenFolderKeyIsSingleString_ReturnsTrueAndAddsSuggestion()
        {
            var property = new Mock<Outlook.UserProperty>();
            property.Setup(x => x.Value).Returns("Archive\\Finance");
            var userProperties = new Mock<Outlook.UserProperties>();
            userProperties.Setup(x => x.Find("FolderKey")).Returns(property.Object);
            var mailItem = new Mock<Outlook.MailItem>();
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            mailItem.SetupGet(x => x.ConversationID).Returns("conv-2");
            var globals = CreateGlobalsWithAutoFiles(ctfMap: new CtfMap());
            var scorer = new FolderScorer();

            Action act = () => scorer.LoadFromField(mailItem.Object, globals.Object);

            act.Should().Throw<RuntimeBinderException>();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void QuerySubject_WithSubjectMapSco_GroupsScoresByFolderPath()
        {
            var commonWords = new SerializableList<string>();
            var target = new SubjectMapEntry("Inbox\\Reports", "Project Plan", 1, commonWords);
            var map = new SubjectMapSco(commonWords)
            {
                new SubjectMapEntry("Inbox\\Reports", "Project Plan", 2, commonWords),
                new SubjectMapEntry("Inbox\\Reports", "Project Plan", 1, commonWords),
                new SubjectMapEntry("Inbox\\Other", "Budget Plan", 1, commonWords),
            };
            target.SubjectEncoded = new[] { 1, 2 };
            foreach (var entry in map)
            {
                entry.SubjectEncoded =
                    entry.Folderpath == "Inbox\\Other" ? new[] { 4, 5 } : new[] { 1, 2 };
            }
            var scorer = new FolderScorer();

            var result = scorer.QuerySubject(map, target, 2, -1, -1, 1).ToList();

            result.Should().HaveCount(2);
            result.Single(x => x.FolderPath == "Inbox\\Reports").Score.Should().BePositive();
        }

        [TestMethod]
        public void QueryFolder_WithSubjectMapSco_GroupsFolderScoresByFolderPath()
        {
            var commonWords = new SerializableList<string>();
            var target = new SubjectMapEntry("Inbox\\Reports", "Project Plan", 1, commonWords);
            var map = new SubjectMapSco(commonWords)
            {
                new SubjectMapEntry("Inbox\\Reports", "Project Plan", 2, commonWords),
                new SubjectMapEntry("Inbox\\Reports", "Project Update", 1, commonWords),
                new SubjectMapEntry("Inbox\\Other", "Budget Plan", 1, commonWords),
            };
            target.SubjectEncoded = new[] { 1, 2 };
            foreach (var entry in map)
            {
                entry.FolderEncoded =
                    entry.Folderpath == "Inbox\\Other" ? new[] { 4 } : new[] { 1 };
            }
            var scorer = new FolderScorer();

            var result = scorer.QueryFolder(map, target, 2, -1, -1).ToList();

            result.Should().HaveCount(2);
            result.Single(x => x.FolderPath == "Inbox\\Reports").Score.Should().BePositive();
        }

        [TestMethod]
        public void QueryFolder_WithParallelQuery_GroupsScoresByFolderPath()
        {
            var encoder = CreateEncoder();
            var commonWords = new SerializableList<string>();
            var target = new SubjectMapEntry("Inbox\\Reports", "Project Plan", 1, commonWords);
            target.Encode(encoder.Object);
            var map = new SubjectMapSco(commonWords)
            {
                CreateEncodedSubjectMapEntry(
                    "Inbox\\Reports",
                    "Project Plan",
                    2,
                    commonWords,
                    encoder.Object
                ),
                CreateEncodedSubjectMapEntry(
                    "Inbox\\Review",
                    "Plan Review",
                    1,
                    commonWords,
                    encoder.Object
                ),
            };
            var scorer = new FolderScorer();

            var result = scorer.QueryFolder(map.AsParallel(), target, 2, -1, -1).ToList();

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(entry => !string.IsNullOrEmpty(entry.FolderPath));
        }

        [TestMethod]
        public void QuerySubject_WithParallelQuery_GroupsAndScoresEntries()
        {
            var commonWords = new SerializableList<string>();
            var target = new SubjectMapEntry("Inbox\\Reports", "Project Plan", 1, commonWords);
            var map = new SubjectMapSco(commonWords)
            {
                new SubjectMapEntry("Inbox\\Reports", "Project Plan", 2, commonWords),
                new SubjectMapEntry("Inbox\\Review", "Plan Review", 1, commonWords),
            };
            target.SubjectEncoded = new[] { 1, 2 };
            map[0].SubjectEncoded = new[] { 1, 2 };
            map[1].SubjectEncoded = new[] { 2, 3 };
            var scorer = new FolderScorer();

            var result = scorer
                .QuerySubject(map.Cast<ISubjectMapEntry>().AsParallel(), target, 2, -1, -1, 1)
                .ToList();

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(entry => !string.IsNullOrEmpty(entry.FolderPath));
        }

        [TestMethod]
        public void RefreshSuggestions_WhenSubjectIsNull_UsesFolderKeysOnlyAndClearsScores()
        {
            var property = new Mock<Outlook.UserProperty>();
            property.SetupGet(x => x.Value).Returns(new[] { "Archive\\Finance", "Archive\\Ops" });
            var userProperties = new Mock<Outlook.UserProperties>();
            userProperties.Setup(x => x.Find("FolderKey")).Returns(property.Object);
            var mailItem = new Mock<Outlook.MailItem>();
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            mailItem.SetupGet(x => x.Subject).Returns((string)null);
            mailItem.SetupGet(x => x.ConversationID).Returns("conv-3");
            var globals = CreateGlobalsWithAutoFiles(ctfMap: new CtfMap());
            var scorer = new FolderScorer();
            scorer.AddSuggestion("Old", 10);

            scorer.RefreshSuggestions(
                mailItem.Object,
                globals.Object,
                topNfolderKeys: 1,
                parallel: false
            );

            scorer.ToArray().Should().Equal("Archive\\Finance");
        }

        [TestMethod]
        public void LoadFromField_WithMailItemAndFolderKeyArray_ReturnsTrueAndAddsSuggestion()
        {
            var mailItem = CreateMailItemWithFolderKey(
                new[] { "Archive\\Finance", "Archive\\Ops" }
            );
            var globals = CreateGlobalsWithAutoFiles(ctfMap: new CtfMap());
            var scorer = new FolderScorer();

            var result = scorer.LoadFromField(mailItem.Object, globals.Object);

            result.Should().BeTrue();
            scorer.ToArray().Should().Equal("Archive\\Finance", "Archive\\Ops");
        }

        [TestMethod]
        public void LoadFromField_WithMailItemAndMissingFolderKey_ReturnsFalse()
        {
            var mailItem = CreateMailItemWithFolderKey(value: null, conversationId: "conv-0");
            var globals = CreateGlobalsWithAutoFiles(ctfMap: new CtfMap());
            var scorer = new FolderScorer();

            var result = scorer.LoadFromField(mailItem.Object, globals.Object);

            result.Should().BeFalse();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void LoadFromField_WithMailItemHelperAndFolderKeyArray_ReturnsTrueAndAddsSuggestion()
        {
            var mailItem = CreateMailItemWithFolderKey(
                new[] { "Archive\\Finance", "Archive\\Ops" }
            );
            var mailInfo = new MailItemHelper { Item = mailItem.Object };
            var globals = CreateGlobalsWithAutoFiles(ctfMap: new CtfMap());
            var scorer = new FolderScorer();

            var result = scorer.LoadFromField(mailInfo, globals.Object);

            result.Should().BeTrue();
            scorer.ToArray().Should().Equal("Archive\\Finance", "Archive\\Ops");
        }

        [TestMethod]
        public void LoadFromField_WithMailItemHelperAndMissingFolderKey_ReturnsFalse()
        {
            var userProperties = new Mock<Outlook.UserProperties>();
            userProperties.Setup(x => x.Find("FolderKey")).Returns((Outlook.UserProperty)null);
            var mailItem = new Mock<Outlook.MailItem>();
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            mailItem.SetupGet(x => x.ConversationID).Returns("conv-4");
            var mailInfo = new MailItemHelper { Item = mailItem.Object };
            var globals = CreateGlobalsWithAutoFiles(ctfMap: new CtfMap());
            var scorer = new FolderScorer();

            var result = scorer.LoadFromField(mailInfo, globals.Object);

            result.Should().BeFalse();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void RefreshSuggestions_WhenSubjectMapAndEncoderExist_AddsWordSequenceMatches()
        {
            var encoder = CreateEncoder();
            var commonWords = new SerializableList<string>();
            var subjectMap = new SubjectMapSco(commonWords)
            {
                CreateEncodedSubjectMapEntry(
                    "Inbox\\Projects",
                    "Project Plan",
                    2,
                    commonWords,
                    encoder.Object
                ),
                CreateEncodedSubjectMapEntry(
                    "Inbox\\Projects",
                    "Project Update",
                    1,
                    commonWords,
                    encoder.Object
                ),
                CreateEncodedSubjectMapEntry(
                    "Inbox\\Review",
                    "Plan Review",
                    1,
                    commonWords,
                    encoder.Object
                ),
            };
            var mailItem = CreateMailItemWithFolderKey(
                value: null,
                subject: "Project Plan",
                conversationId: "conv-5"
            );
            var globals = CreateGlobalsWithAutoFiles(
                ctfMap: new CtfMap(),
                subjectMap: subjectMap,
                encoder: encoder.Object,
                commonWords: commonWords
            );
            var scorer = new FolderScorer();

            scorer.RefreshSuggestions(mailItem.Object, globals.Object, parallel: false);

            scorer.ToArray().Should().Contain("Inbox\\Projects");
        }

        [TestMethod]
        public void AddWordSequenceSuggestions_WithParallelQuery_AddsMatches()
        {
            var encoder = CreateEncoder();
            var commonWords = new SerializableList<string>();
            var target = new SubjectMapEntry(
                "Project Plan",
                1,
                commonWords,
                Tokenizer.GetRegex(),
                encoder.Object
            );
            var subjectMap = new SubjectMapSco(commonWords)
            {
                CreateEncodedSubjectMapEntry(
                    "Inbox\\Projects",
                    "Project Plan",
                    2,
                    commonWords,
                    encoder.Object
                ),
                CreateEncodedSubjectMapEntry(
                    "Inbox\\Review",
                    "Plan Review",
                    1,
                    commonWords,
                    encoder.Object
                ),
            };
            var globals = CreateGlobalsWithAutoFiles(
                ctfMap: new CtfMap(),
                subjectMap: subjectMap,
                encoder: encoder.Object,
                commonWords: commonWords
            );
            var scorer = new FolderScorer();

            Action act = () =>
                scorer.AddWordSequenceSuggestions(target, globals.Object, parallel: true);

            act.Should().Throw<RuntimeBinderException>();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void AddWordSequenceSuggestions_WhenEncoderIsMissing_SwallowsExceptionAndLeavesSuggestionsEmpty()
        {
            var mailItem = CreateMailItemWithFolderKey(
                value: null,
                subject: "Project Plan",
                conversationId: "conv-6"
            );
            var globals = CreateGlobalsWithAutoFiles(ctfMap: new CtfMap());
            var scorer = new FolderScorer();

            Action act = () =>
                scorer.AddWordSequenceSuggestions(mailItem.Object, globals.Object, parallel: false);

            act.Should().NotThrow();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void AddSuggestion_WithStringObject_ReturnsTrueAndStoresFolder()
        {
            var scorer = new FolderScorer();

            var result = scorer.AddSuggestion((object)"Archive\\Manual", 5);

            result.Should().BeTrue();
            scorer.ToArray().Should().Equal("Archive\\Manual");
        }

        [TestMethod]
        public void AddArray_WithObjectOverload_ReturnsExpectedValues()
        {
            var scorer = new FolderScorer();

            var result = scorer.AddArray((object)new[] { "Archive\\Finance", "Archive\\Ops" }, 1);

            result.Should().BeTrue();
            scorer.ToArray().Should().Equal("Archive\\Finance");
        }

        [TestMethod]
        public void AddArray_WithNullObject_ReturnsFalse()
        {
            var scorer = new FolderScorer();

            var result = scorer.AddArray((object)null, -1);

            result.Should().BeFalse();
            scorer.Count.Should().Be(0);
        }

        private static Mock<IApplicationGlobals> CreateGlobalsWithAutoFiles(
            CtfMap ctfMap,
            SubjectMapSco subjectMap = null,
            ISubjectMapEncoder encoder = null,
            SerializableList<string> commonWords = null
        )
        {
            commonWords ??= new SerializableList<string>();
            subjectMap ??= new SubjectMapSco(commonWords);
            var autoFiles = new Mock<IAppAutoFileObjects>();
            autoFiles.SetupGet(x => x.CtfMap).Returns(ctfMap);
            autoFiles.SetupGet(x => x.LngConvCtPwr).Returns(1);
            autoFiles.SetupGet(x => x.Conversation_Weight).Returns(1);
            autoFiles.SetupGet(x => x.CommonWords).Returns(commonWords);
            autoFiles.SetupGet(x => x.SmithWatterman_MatchScore).Returns(2);
            autoFiles.SetupGet(x => x.SmithWatterman_MismatchScore).Returns(-1);
            autoFiles.SetupGet(x => x.SmithWatterman_GapPenalty).Returns(-1);
            autoFiles.SetupGet(x => x.SubjectMap).Returns(subjectMap);
            autoFiles.SetupGet(x => x.Encoder).Returns(encoder);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.AF).Returns(autoFiles.Object);
            return globals;
        }

        private static Mock<Outlook.MailItem> CreateMailItemWithFolderKey(
            object value,
            string subject = null,
            string conversationId = "conv"
        )
        {
            var userProperties = new Mock<Outlook.UserProperties>();
            var mailItem = new Mock<Outlook.MailItem>();

            if (value is null)
            {
                userProperties.Setup(x => x.Find("FolderKey")).Returns((Outlook.UserProperty)null);
            }
            else
            {
                var property = new Mock<Outlook.UserProperty>();
                property.SetupGet(x => x.Value).Returns(value);
                userProperties.Setup(x => x.Find("FolderKey")).Returns(property.Object);
            }

            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            mailItem.SetupGet(x => x.Subject).Returns(subject);
            mailItem.SetupGet(x => x.ConversationID).Returns(conversationId);
            return mailItem;
        }

        private static Mock<ISubjectMapEncoder> CreateEncoder()
        {
            var tokenMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var nextCode = 1;
            var encoder = new Mock<ISubjectMapEncoder>();

            encoder
                .Setup(x => x.AugmentTokenDict(It.IsAny<string[]>()))
                .Callback<string[]>(tokens =>
                {
                    foreach (var token in tokens.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        if (!tokenMap.ContainsKey(token))
                        {
                            tokenMap[token] = nextCode++;
                        }
                    }
                });
            encoder
                .Setup(x => x.Encode(It.IsAny<string[]>()))
                .Returns<string[]>(tokens => tokens.Select(token => tokenMap[token]).ToArray());

            return encoder;
        }

        private static SubjectMapEntry CreateEncodedSubjectMapEntry(
            string folderPath,
            string subject,
            int emailCount,
            SerializableList<string> commonWords,
            ISubjectMapEncoder encoder
        )
        {
            var entry = new SubjectMapEntry(folderPath, subject, emailCount, commonWords);
            entry.Encode(encoder);
            return entry;
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
