using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using UtilitiesCS;


namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class CtfMapTests
    {
        [TestMethod]
        public void TopEntriesById_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var input = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "68109D5D0ED86B4B8384B64247D96451",
                                1),
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                1),
                new CtfMapEntry("Reference\\2nd Winner",
                                "719401DA247C4C479AD40FEF2873039F",
                                5),
                new CtfMapEntry("Random",
                                "68109D5D0ED86B4B8384B64247D96451",
                                1),
                new CtfMapEntry("Reference\\1st Winner",
                                "719401DA247C4C479AD40FEF2873039F",
                                10),
            };

            var expected = new CtfMapEntry[]
            {
                new CtfMapEntry("Reference\\1st Winner",
                                "719401DA247C4C479AD40FEF2873039F",
                                10),
                new CtfMapEntry("Reference\\2nd Winner",
                                "719401DA247C4C479AD40FEF2873039F",
                                5)
            };

            string id = "719401DA247C4C479AD40FEF2873039F";
            int topN = 2;

            // Act
            var test = input.TopEntriesById(id, topN);

            // Assert
            test.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_StateUnderTest_AddNew()
        {
            // Arrange
            var test = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "68109D5D0ED86B4B8384B64247D96451",
                                1),
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                1)
            };

            var expected = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "68109D5D0ED86B4B8384B64247D96451",
                                1),
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                1),
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039G",
                                3)
            };

            string emailFolder = "Reference\\Computer Information";
            string conversationID = "719401DA247C4C479AD40FEF2873039G";
            int emailCount = 3;

            // Act
            test.Add(emailFolder, conversationID, emailCount);

            // Assert
            test.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_StateUnderTest_AddToExisting()
        {
            // Arrange
            var test = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "68109D5D0ED86B4B8384B64247D96451",
                                1),
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                1)
            };

            var expected = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "68109D5D0ED86B4B8384B64247D96451",
                                1),
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                4)
            };

            string emailFolder = "Reference\\Computer Information";
            string conversationID = "719401DA247C4C479AD40FEF2873039F";
            int emailCount = 3;

            // Act
            test.Add(emailFolder, conversationID, emailCount);

            // Assert
            test.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void ProcessQueue_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "68109D5D0ED86B4B8384B64247D96451",
                "1",
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            var expected = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "68109D5D0ED86B4B8384B64247D96451",
                                1),
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                1)
            };

            // Act
            var test = CtfMap.ProcessQueue(lines);

            // Assert
            test.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void ProcessQueue_StateUnderTest_MalformedEntry1()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "68109D5D0ED86B4B8384B64247D96451",
                "error",
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            var expected = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                1)
            };

            // Act
            var test = CtfMap.ProcessQueue(lines);

            // Assert
            test.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void ProcessQueue_StateUnderTest_MalformedEntry2()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "68109D5D0ED86B4B8384B64247D96451",
                "1.5",
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            var expected = new CtfMap
            {
                new CtfMapEntry("Reference\\Computer Information",
                                "719401DA247C4C479AD40FEF2873039F",
                                1)
            };

            // Act
            var actual = CtfMap.ProcessQueue(lines);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TryDequeueEntry_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "68109D5D0ED86B4B8384B64247D96451",
                "1",
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            var expected = new CtfMapEntry(
                "Reference\\Computer Information", 
                "68109D5D0ED86B4B8384B64247D96451", 
                1);

            // Act
            var actual = CtfMap.TryDequeueEntry(ref lines);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TryDequeueEntry_StateUnderTest_IncompleteQueue()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "68109D5D0ED86B4B8384B64247D96451"
            });

            // Act
            var actual = CtfMap.TryDequeueEntry(ref lines);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void TryDequeueEntry_StateUnderTest_IncorrectType1()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "malformed"
            });

            // Act
            var actual = CtfMap.TryDequeueEntry(ref lines);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void TryDequeueEntry_StateUnderTest_IncorrectType2()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1.5"
            });

            // Act
            var actual = CtfMap.TryDequeueEntry(ref lines);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void DequeueToNextRecord_StateUnderTest_Remove0()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            Queue<string> expected = new(new List<string>
            {
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            // Act
            CtfMap.DequeueToNextRecord(ref lines);
            var actual = lines;

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void DequeueToNextRecord_StateUnderTest_Remove2()
        {
            // Arrange
            Queue<string> lines = new( new List<string>
            {
                "68109D5D0ED86B4B8384B64247D96451",
                "1",
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            Queue<string> expected = new(new List<string>
            {
                "Reference\\Computer Information",
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            // Act
            CtfMap.DequeueToNextRecord(ref lines);
            var actual = lines;

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void DequeueToNextRecord_StateUnderTest_RemoveAll()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "719401DA247C4C479AD40FEF2873039F",
                "1"
            });

            Queue<string> expected = new();

            // Act
            CtfMap.DequeueToNextRecord(ref lines);
            var test = lines;

            // Assert
            test.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void IsEntryID_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            string line = "68109D5D0ED86B4B8384B64247D96451";

            // Act
            var result = CtfMap.IsEntryID(line);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEntryID_StateUnderTest_Folder()
        {
            // Arrange
            string line = "Reference\\ComputerInformation\\ta";

            // Act
            var result = CtfMap.IsEntryID(line);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEntryID_StateUnderTest_TextWithSpaces()
        {
            // Arrange
            string line = "Reference ComputerInformation ta";

            // Act
            var result = CtfMap.IsEntryID(line);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEntryID_StateUnderTest_FewerCharacters()
        {
            // Arrange
            string line = "3";

            // Act
            var result = CtfMap.IsEntryID(line);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
