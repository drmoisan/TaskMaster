using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using UtilitiesCS;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    [Obsolete]
    public class CtfIncidenceListTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private CtfIncidenceList CreateCtfIncidenceList()
        {
            return new CtfIncidenceList();
        }

        [TestMethod]
        public void CTF_Inc_Position_ADD_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var ctfIncidenceList = this.CreateCtfIncidenceList();
            int Inc_Num = 0;
            CtfMapEntry CTF_Map = null;

            // Act
            ctfIncidenceList.CtfIncidencePositionAdd(
                Inc_Num,
                CTF_Map);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CTF_Incidence_FIND_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var ctfIncidenceList = this.CreateCtfIncidenceList();
            string ConvID = null;

            // Act
            var result = ctfIncidenceList.FindID(
                ConvID);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CTF_Incidence_INIT_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var ctfIncidenceList = this.CreateCtfIncidenceList();
            int Inc_Num = 0;

            // Act
            ctfIncidenceList.CTF_Incidence_INIT(
                Inc_Num);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CTF_Incidence_SET_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var ctfIncidenceList = this.CreateCtfIncidenceList();
            int Inc_Num = 0;
            int Inc_Position = 0;
            int Folder_Count = 0;
            CtfMapEntry Map = null;

            // Act
            ctfIncidenceList.CTF_Incidence_SET(
                Inc_Num,
                Inc_Position,
                Folder_Count,
                Map);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CTF_Incidence_Text_File_WRITE_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var ctfIncidenceList = this.CreateCtfIncidenceList();
            string folderpath = null;
            string filename = null;

            // Act
            ctfIncidenceList.CTF_Incidence_Text_File_WRITE(
                folderpath,
                filename);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ReadTextFile_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var ctfIncidenceList = this.CreateCtfIncidenceList();

            // Act
            //var result = ctfIncidenceList.ReadTextFile(
            //    filepath);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ProcessQueue_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            Queue<string> lines = new(new List<string> 
            {
                "68109D5D0ED86B4B8384B64247D96451", "1",
                "Reference\\Computer Information", "1",
                "D5A990D48B6B2B40ADC28F23CE8D6FAC", "2",
                "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "4",
                "Reference\\Computer Information", "3" 
            });

            var expected = new CtfIncidenceList();
            expected.Add(
                new CtfIncidence(emailConversationID: "68109D5D0ED86B4B8384B64247D96451",
                                 folderCount: 1,
                                 emailFolder: new List<string> 
                                 { 
                                     "Reference\\Computer Information" 
                                 },
                                 emailConversationCount: new List<int> { 1 }
                ));
            expected.Add(
                new CtfIncidence(emailConversationID: "D5A990D48B6B2B40ADC28F23CE8D6FAC",
                                 folderCount: 2,
                                 emailFolder: new List<string>
                                 {
                                     "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT",
                                     "Reference\\Computer Information"
                                 },
                                 emailConversationCount: new List<int> { 4, 3 }
                ));

            // Act
            var actual = CtfIncidenceList.ProcessQueue(lines);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void ProcessQueue_StateUnderTest_MalformedEntry1()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "68109D5D0ED86B4B8384B64247D96451", "malformed",
                "Reference\\Computer Information", "1",
                "D5A990D48B6B2B40ADC28F23CE8D6FAC", "2",
                "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "4",
                "Reference\\Computer Information", "3"
            });

            var expected = new CtfIncidenceList();
            expected.Add(
                new CtfIncidence(emailConversationID: "D5A990D48B6B2B40ADC28F23CE8D6FAC",
                                 folderCount: 2,
                                 emailFolder: new List<string>
                                 {
                                     "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT",
                                     "Reference\\Computer Information"
                                 },
                                 emailConversationCount: new List<int> { 4, 3 }
                ));

            // Act
            var actual = CtfIncidenceList.ProcessQueue(lines);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void ProcessQueue_StateUnderTest_MalformedEntry2()
        {
            // Arrange
            Queue<string> lines = new(new List<string>
            {
                "68109D5D0ED86B4B8384B64247D96451", "1",
                "Reference\\Computer Information", "1",
                "D5A990D48B6B2B40ADC28F23CE8D6FAC", "2",
                "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "4"
            });

            var expected = new CtfIncidenceList();
            expected.Add(
                new CtfIncidence(emailConversationID: "68109D5D0ED86B4B8384B64247D96451",
                                 folderCount: 1,
                                 emailFolder: new List<string>
                                 {
                                     "Reference\\Computer Information"
                                 },
                                 emailConversationCount: new List<int> { 1 }
                ));

            // Act
            var actual = CtfIncidenceList.ProcessQueue(lines);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TryDequeueIncidence_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            Queue<string> lines = new( new List<string> { "D5A990D48B6B2B40ADC28F23CE8D6FAC", "2",
                "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "4",
                "Reference\\Computer Information", "3" });
            var expected = new CtfIncidence(
                emailConversationID: "D5A990D48B6B2B40ADC28F23CE8D6FAC",
                folderCount: 2,
                emailFolder: new List<string> { "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "Reference\\Computer Information" },
                emailConversationCount: new List<int> { 4, 3 }
                );
            
            // Act
            var actual = CtfIncidenceList.TryDequeueIncidence(ref lines);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TryDequeueIncidence_HandlesIncompleteQueue()
        {
            // Arrange
            Queue<string> lines = new(new List<string> { "D5A990D48B6B2B40ADC28F23CE8D6FAC", "2",
                "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "4" });

            // Act
            var actual = CtfIncidenceList.TryDequeueIncidence(ref lines);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void TryDequeueIncidence_HandlesIncorrectType1()
        {
            // Arrange
            Queue<string> lines = new(new List<string> { "D5A990D48B6B2B40ADC28F23CE8D6FAC", "malformed",
                "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "4",
                "Reference\\Computer Information", "3" });

            // Act
            var actual = CtfIncidenceList.TryDequeueIncidence(ref lines);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void TryDequeueIncidence_HandlesIncorrectType2()
        {
            // Arrange
            Queue<string> lines = new(new List<string> { "D5A990D48B6B2B40ADC28F23CE8D6FAC", "2",
                "Completed Jobs - 02 PLANET\\_ Active Projects\\02 SETUP\\02 SETUP - IT", "4.3",
                "Reference\\Computer Information", "3" });

            // Act
            var actual = CtfIncidenceList.TryDequeueIncidence(ref lines);

            // Assert
            Assert.IsNull(actual);
        }
    }
}
