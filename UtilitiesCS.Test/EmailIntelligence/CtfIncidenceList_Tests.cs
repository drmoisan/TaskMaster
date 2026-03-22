using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class CtfIncidenceList_Tests
    {
#pragma warning disable CS0618 // Deprecated types are intentionally covered by this plan.
        [TestMethod]
        public void Constructor_WithNoArguments_StartsEmpty()
        {
            // Arrange

            // Act
            var list = new CtfIncidenceList();

            // Assert
            list.Should().BeEmpty();
            list.CTF_Inc_Ct.Should().Be(0);
        }

        [TestMethod]
        public void FindID_WhenConversationExists_ReturnsMatchingIndex()
        {
            // Arrange
            var list = new CtfIncidenceList
            {
                CreateIncidence("conv-1", folderCount: 1, firstFolder: "Inbox", firstCount: 2),
                CreateIncidence("conv-2", folderCount: 1, firstFolder: "Archive", firstCount: 4),
            };

            // Act
            var index = list.FindID("conv-2");

            // Assert
            index.Should().Be(1);
        }

        [TestMethod]
        public void FindID_WhenConversationIsMissing_ReturnsMinusOne()
        {
            // Arrange
            var list = new CtfIncidenceList();

            // Act
            var index = list.FindID("missing");

            // Assert
            index.Should().Be(-1);
        }

        [TestMethod]
        public void CTF_Incidence_SET_PopulatesRequestedPosition()
        {
            // Arrange
            var list = new CtfIncidenceList { CreateIncidence("seed", folderCount: 0) };
            var entry = new CtfMapEntry("Projects", "conv-3", 9);

            // Act
            list.CTF_Incidence_SET(Inc_Num: 0, Inc_Position: 1, Folder_Count: 1, Map: entry);

            // Assert
            list[0].FolderCount.Should().Be(1);
            list[0].EmailConversationID.Should().Be("conv-3");
            list[0].EmailFolders[1].Should().Be("Projects");
            list[0].EmailCounts[1].Should().Be(9);
        }

        [TestMethod]
        public void CTF_Incidence_INIT_ResetsTrackedSlotsToSentinels()
        {
            // Arrange
            var list = new CtfIncidenceList
            {
                CreateIncidence(
                    "conv-4",
                    folderCount: 3,
                    firstFolder: "Inbox",
                    firstCount: 5,
                    secondFolder: "Archive",
                    secondCount: 4,
                    thirdFolder: "Reference",
                    thirdCount: 1
                ),
            };

            // Act
            list.CTF_Incidence_INIT(Inc_Num: 0);

            // Assert
            list[0].FolderCount.Should().Be(0);
            list[0].EmailCounts.Should().Equal(0, 0, 0, 0);
            list[0]
                .EmailFolders.Should()
                .Equal(
                    string.Empty,
                    "===============================",
                    "===============================",
                    "==============================="
                );
        }

        [TestMethod]
        public void CtfIncidencePositionAdd_WhenCountBelongsInMiddle_InsertsInDescendingOrder()
        {
            // Arrange
            var list = new CtfIncidenceList
            {
                CreateIncidence(
                    "conv-5",
                    folderCount: 2,
                    firstFolder: "Inbox",
                    firstCount: 10,
                    secondFolder: "Archive",
                    secondCount: 5,
                    thirdFolder: "Reference",
                    thirdCount: 1
                ),
            };
            var entry = new CtfMapEntry("Projects", "conv-5", 7);

            // Act
            list.CtfIncidencePositionAdd(idx: 0, CtfMap: entry);

            // Assert
            list[0].FolderCount.Should().Be(3);
            list[0].EmailFolders.Should().Equal(string.Empty, "Inbox", "Projects", "Archive");
            list[0].EmailCounts.Should().Equal(0, 10, 7, 5);
        }

        [TestMethod]
        public void CtfIncidencePositionAdd_WhenEntryOnlyBeatsLastSlot_ReplacesLastSlot()
        {
            // Arrange
            var list = new CtfIncidenceList
            {
                CreateIncidence(
                    "conv-6",
                    folderCount: 3,
                    firstFolder: "Inbox",
                    firstCount: 10,
                    secondFolder: "Archive",
                    secondCount: 7,
                    thirdFolder: "Reference",
                    thirdCount: 3
                ),
            };
            var entry = new CtfMapEntry("Projects", "conv-6", 5);

            // Act
            list.CtfIncidencePositionAdd(idx: 0, CtfMap: entry);

            // Assert
            list[0].FolderCount.Should().Be(3);
            list[0].EmailFolders.Should().Equal(string.Empty, "Inbox", "Archive", "Projects");
            list[0].EmailCounts.Should().Equal(0, 10, 7, 5);
        }

        [TestMethod]
        public void CtfIncidencePositionAdd_WhenMaxFoldersIsOne_UpdatesSecondSlotWhenEntryWins()
        {
            // Arrange
            var list = new CtfIncidenceList
            {
                CreateIncidence(
                    "conv-7",
                    folderCount: 1,
                    firstFolder: "Inbox",
                    firstCount: 2,
                    secondFolder: "Archive",
                    secondCount: 1,
                    thirdFolder: "Reference",
                    thirdCount: 0
                ),
            };
            typeof(CtfIncidenceList)
                .GetField("_maxFoldersPerConv", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(list, 1);
            var entry = new CtfMapEntry("Projects", "conv-7", 8);

            // Act
            list.CtfIncidencePositionAdd(idx: 0, CtfMap: entry);

            // Assert
            list[0].FolderCount.Should().Be(1);
            list[0].EmailFolders[1].Should().Be("Projects");
            list[0].EmailCounts[1].Should().Be(8);
        }

        [TestMethod]
        public void InheritedListOperations_AllowAddRemoveAndDuplicateEntries()
        {
            // Arrange
            var list = new CtfIncidenceList();
            var first = CreateIncidence("dup", folderCount: 1, firstFolder: "Inbox", firstCount: 1);
            var second = CreateIncidence(
                "dup",
                folderCount: 1,
                firstFolder: "Archive",
                firstCount: 2
            );

            // Act
            list.Add(first);
            list.Add(second);
            var removed = list.Remove(first);

            // Assert
            removed.Should().BeTrue();
            list.Should().ContainSingle();
            list[0].Should().BeSameAs(second);
        }

        [TestMethod]
        public void TryDequeueIncidence_WithOverflowCount_ReturnsNull()
        {
            // Arrange
            var lines = new Queue<string>(
                new[]
                {
                    "AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH",
                    "1",
                    "Inbox",
                    ((long)int.MaxValue + 1).ToString(),
                }
            );

            // Act
            var result = CtfIncidenceList.TryDequeueIncidence(ref lines);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void ArrayToQueue_SkipsHeaderLine()
        {
            // Arrange
            var method = typeof(CtfIncidenceList).GetMethod(
                "ArrayToQueue",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            // Act
            var queue =
                (Queue<string>)
                    method.Invoke(null, new object[] { new[] { "header", "one", "two" } });

            // Assert
            queue.Should().ContainInOrder("one", "two");
        }

        [TestMethod]
        public void ReadFileToArray_WhenFileIsMissing_ThrowsFileNotFoundException()
        {
            // Arrange
            var method = typeof(CtfIncidenceList).GetMethod(
                "ReadFileToArray",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            // Act
            Action act = () =>
                method.Invoke(null, new object[] { GetMissingFilePath("ctf-incidence-list") });

            // Assert
            act.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<FileNotFoundException>();
        }

        [TestMethod]
        public void ReadTextFile_WhenFileIsMissing_ThrowsFileNotFoundException()
        {
            // Arrange

            // Act
            Action act = () =>
                CtfIncidenceList.ReadTextFile(GetMissingFilePath("ctf-incidence-list-read"));

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [TestMethod]
        public void CTF_Incidence_Text_File_WRITE_WithInvalidFolderPath_ThrowsArgumentException()
        {
            // Arrange
            var list = new CtfIncidenceList
            {
                CreateIncidence("conv-8", folderCount: 1, firstFolder: "Inbox", firstCount: 3),
            };
            list.CTF_Inc_Ct = 1;

            // Act
            Action act = () =>
                list.CTF_Incidence_Text_File_WRITE("\0invalid-folder", "ctf-incidence.txt");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ReadFileToArray_WhenPathIsInvalid_ThrowsArgumentException()
        {
            // Arrange
            var method = typeof(CtfIncidenceList).GetMethod(
                "ReadFileToArray",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            // Act
            Action act = () => method.Invoke(null, new object[] { "\0invalid-path" });

            // Assert
            act.Should()
                .Throw<TargetInvocationException>()
                .Where(exception => exception.InnerException is ArgumentException);
        }

        private static CtfIncidence CreateIncidence(
            string conversationId,
            int folderCount,
            string firstFolder = "",
            int firstCount = 0,
            string secondFolder = "",
            int secondCount = 0,
            string thirdFolder = "",
            int thirdCount = 0
        )
        {
            return new CtfIncidence(
                emailConversationID: conversationId,
                folderCount: folderCount,
                emailFolder: new List<string>
                {
                    string.Empty,
                    firstFolder,
                    secondFolder,
                    thirdFolder,
                },
                emailConversationCount: new List<int> { 0, firstCount, secondCount, thirdCount }
            );
        }

        private static string GetMissingFilePath(string name)
        {
            return Path.Combine(Environment.CurrentDirectory, $"{name}-{Guid.NewGuid():N}.txt");
        }
#pragma warning restore CS0618
    }
}
