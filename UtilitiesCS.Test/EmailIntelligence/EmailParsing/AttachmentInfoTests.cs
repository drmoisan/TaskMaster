using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence;
using Microsoft.Office.Interop.Outlook;

namespace Z.Unfinished.UtilitiesCS.Test.EmailIntelligence.EmailParsing
{
    [TestClass]
    public class AttachmentInfoTests
    {
        //private MockRepository mockRepository;
        //private Mock<Attachment> mockAttachment;

        //[TestInitialize]
        //public void TestInitialize()
        //{
        //    this.mockRepository = new MockRepository(MockBehavior.Loose);
        //    this.mockAttachment = this.mockRepository.Create<Attachment>();
        //    this.mockAttachment.SetupAllProperties();
        //}

        #region How To Test Static Methods

        //public interface IFactory
        //{
        //    Task<AttachmentInfo> CreateAsync(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath);
        //}
        
        //public class FactoryWrapper(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath) : IFactory
        //{
        //    Attachment _attachment = attachment;
        //    DateTime _sentOn = sentOn;
        //    string _saveFolderPath = saveFolderPath;
        //    string _deleteFolderPath = deleteFolderPath;
        //    public async Task<AttachmentInfo> CreateAsync(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        //    {
        //        return await AttachmentInfo.CreateAsync(_attachment, _sentOn, _saveFolderPath, _deleteFolderPath);
        //    }
        //}

        #endregion How To Test Static Methods

        private AttachmentInfo CreateAttachmentInfo()
        {
            return new AttachmentInfo();
        }

        [TestMethod]
        public async Task CreateAsync_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            //// Arrange
            //DateTime sentOn = default;
            //string saveFolderPath = null;
            //string deleteFolderPath = null;

            //// Act
            //var result = await AttachmentInfo.CreateAsync(
            //    this.mockAttachment.Object,
            //    sentOn,
            //    saveFolderPath,
            //    deleteFolderPath);

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }

        //[TestMethod]
        //public void Init_StateUnderTest_ExpectedBehavior()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();
        //    //Attachment attachment = null;
        //    //DateTime sentOn = default(global::System.DateTime);
        //    //string saveFolderPath = null;
        //    //string deleteFolderPath = null;

        //    //// Act
        //    //attachmentInfo.Init(
        //    //    attachment,
        //    //    sentOn,
        //    //    saveFolderPath,
        //    //    deleteFolderPath);

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task InitAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();
        //    //Attachment attachment = null;
        //    //DateTime sentOn = default(global::System.DateTime);
        //    //string saveFolderPath = null;
        //    //string deleteFolderPath = null;

        //    //// Act
        //    //await attachmentInfo.InitAsync(
        //    //    attachment,
        //    //    sentOn,
        //    //    saveFolderPath,
        //    //    deleteFolderPath);

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void AdjustForMaxPath_StateUnderTest_ExpectedBehavior()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();
        //    //string folderPath = null;
        //    //string filenameSeed = null;
        //    //string fileExtension = null;
        //    //string filenameSuffix = null;

        //    //// Act
        //    //var result = AttachmentInfo.AdjustForMaxPath(
        //    //    folderPath,
        //    //    filenameSeed,
        //    //    fileExtension,
        //    //    filenameSuffix);

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void CheckParameters_StateUnderTest_ExpectedBehavior()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();
        //    //Attachment attachment = null;
        //    //DateTime sentOn = default(global::System.DateTime);
        //    //string saveFolderPath = null;

        //    //// Act
        //    //var result = attachmentInfo.CheckParameters(
        //    //    attachment,
        //    //    sentOn,
        //    //    saveFolderPath);

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void CheckParameters_StateUnderTest_ExpectedBehavior1()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();
        //    //Attachment attachment = null;
        //    //DateTime sentOn = default(global::System.DateTime);
        //    //string saveFolderPath = null;
        //    //string deleteFolderPath = null;

        //    //// Act
        //    //var result = attachmentInfo.CheckParameters(
        //    //    attachment,
        //    //    sentOn,
        //    //    saveFolderPath,
        //    //    deleteFolderPath);

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void GetAttachmentFilename_StateUnderTest_ExpectedBehavior()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();
        //    //Attachment attachment = null;

        //    //// Act
        //    //var result = attachmentInfo.GetAttachmentFilename(
        //    //    attachment);

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void GetNameSuffix_StateUnderTest_ExpectedBehavior()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();

        //    //// Act
        //    //var result = attachmentInfo.GetNameSuffix();

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void PrependDatePrefix_StateUnderTest_ExpectedBehavior()
        //{
        //    //// Arrange
        //    //var attachmentInfo = this.CreateAttachmentInfo();
        //    //string seed = null;
        //    //DateTime date = default(global::System.DateTime);

        //    //// Act
        //    //var result = attachmentInfo.PrependDatePrefix(
        //    //    seed,
        //    //    date);

        //    //// Assert
        //    //Assert.Fail();
        //    //this.mockRepository.VerifyAll();
        //}
    }
}
