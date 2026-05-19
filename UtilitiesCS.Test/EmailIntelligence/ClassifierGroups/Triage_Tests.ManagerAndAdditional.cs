using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;
using TriageClass = UtilitiesCS.EmailIntelligence.Triage;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    public partial class Triage_Tests
    {
        private static ManagerAsyncLazy ConfigureManager(Mock<IApplicationGlobals> mockGlobals)
        {
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return manager;
        }

        private static Mock<Selection> CreateSelection(params object[] items)
        {
            var selection = new Mock<Selection>(MockBehavior.Loose);
            selection
                .As<IEnumerable>()
                .Setup(s => s.GetEnumerator())
                .Returns(() => items.GetEnumerator());
            return selection;
        }

        private static Mock<MailItem> CreateMailItem(
            string messageClass = "IPM.Note",
            UserProperties userProperties = null
        )
        {
            var attachments = new Mock<Attachments>(MockBehavior.Loose);
            attachments
                .As<IEnumerable>()
                .Setup(a => a.GetEnumerator())
                .Returns(() => new List<Attachment>().GetEnumerator());

            var recipients = new Mock<Recipients>(MockBehavior.Loose);
            recipients
                .As<IEnumerable>()
                .Setup(r => r.GetEnumerator())
                .Returns(() => new List<Recipient>().GetEnumerator());

            var folder = new Mock<Folder>(MockBehavior.Loose);
            folder.SetupGet(f => f.Name).Returns("Inbox");
            folder.SetupGet(f => f.FolderPath).Returns("\\Mailbox\\Inbox");
            folder.SetupGet(f => f.StoreID).Returns("store-1");

            var sender = new Mock<AddressEntry>(MockBehavior.Loose);
            sender.SetupGet(s => s.Name).Returns("Sender");

            var mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem.SetupGet(m => m.MessageClass).Returns(messageClass);
            mailItem.SetupGet(m => m.UserProperties).Returns(userProperties);
            mailItem.SetupGet(m => m.Attachments).Returns(attachments.Object);
            mailItem.SetupGet(m => m.Recipients).Returns(recipients.Object);
            mailItem.SetupGet(m => m.Parent).Returns(folder.Object);
            mailItem.SetupGet(m => m.Subject).Returns("Workflow");
            mailItem.SetupGet(m => m.Body).Returns("workflow coverage");
            mailItem
                .SetupGet(m => m.HTMLBody)
                .Returns("<html><body>workflow coverage</body></html>");
            mailItem.SetupGet(m => m.InternetCodepage).Returns(65001);
            mailItem.SetupGet(m => m.SentOn).Returns(new DateTime(2024, 1, 2, 3, 4, 5));
            mailItem.SetupGet(m => m.Sender).Returns(sender.Object);
            mailItem.SetupGet(m => m.ConversationID).Returns("conversation-1");
            mailItem.SetupGet(m => m.Categories).Returns(string.Empty);
            mailItem.SetupGet(m => m.UnRead).Returns(true);
            mailItem.SetupGet(m => m.Size).Returns(42);
            return mailItem;
        }

        private static Mock<UserProperty> CreateUserProperty(object value = null)
        {
            var property = new Mock<UserProperty>(MockBehavior.Loose);
            property.SetupAllProperties();
            property.Object.Value = value;
            return property;
        }

        private static Mock<UserProperties> CreateWritableUserProperties(
            UserProperty addedProperty,
            UserProperty triageProperty = null
        )
        {
            var userProperties = new Mock<UserProperties>(MockBehavior.Loose);
            userProperties
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(triageProperty);
            userProperties
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(addedProperty);
            userProperties
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(addedProperty);
            return userProperties;
        }

        #region Properties

        [TestMethod]
        public void ClassifierGroup_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            triage.ClassifierGroup = group;

            triage.ClassifierGroup.Should().BeSameAs(group);
        }

        [TestMethod]
        public void TokenizeAsync_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> tokenizer = (
                _,
                __,
                ___
            ) => Task.FromResult(new[] { "token" });
            triage.TokenizeAsync = tokenizer;

            triage.TokenizeAsync.Should().BeSameAs(tokenizer);
        }

        [TestMethod]
        public void CallbackAsync_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            Func<object, string, Task> callback = (_, __) => Task.CompletedTask;
            triage.CallbackAsync = callback;

            triage.CallbackAsync.Should().BeSameAs(callback);
        }

        #endregion

        #region ManagerAsyncLazy

        [TestMethod]
        public void ManagerAsyncLazy_Constructor_SetsGlobals()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            manager.Should().NotBeNull();
            manager.Configuration.Should().NotBeNull();
        }

        [TestMethod]
        public void ManagerAsyncLazy_ResetConfigAsyncLazy_ResetsConfiguration()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var original = manager.Configuration;

            manager.ResetConfigAsyncLazy();

            manager.Configuration.Should().NotBeNull();
            manager.Configuration.Should().NotBeSameAs(original);
        }

        [TestMethod]
        public void ManagerAsyncLazy_ResetConfigAsyncLazy_NewReferenceIsDifferentFromOriginal()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var originalConfig = manager.Configuration;

            manager.ResetConfigAsyncLazy();

            manager
                .Configuration.Should()
                .NotBeSameAs(originalConfig, "ResetConfigAsyncLazy must create a fresh lazy task");
        }

        [TestMethod]
        public void ManagerAsyncLazy_ResetLoadClassifierAsyncLazy_InactiveLoader_RemovesEntry()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            manager["InactiveKey"] = new BayesianClassifierGroup().ToAsyncLazy();

            var loader = new SmartSerializableLoader(mockGlobals.Object);
            loader.Name = "InactiveKey";
            loader.Config.ClassifierActivated = false;

            manager.ResetLoadClassifierAsyncLazy("InactiveKey", loader);

            manager.ContainsKey("InactiveKey").Should().BeFalse();
        }

        [TestMethod]
        public void ManagerAsyncLazy_ResetLoadClassifierAsyncLazy_ActiveLoader_AddsEntry()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var loader = new SmartSerializableLoader(mockGlobals.Object);
            loader.Name = "ActiveKey";
            loader.Config.ClassifierActivated = true;

            manager.ResetLoadClassifierAsyncLazy("ActiveKey", loader);

            manager.ContainsKey("ActiveKey").Should().BeTrue();
        }

        [TestMethod]
        public async Task ManagerAsyncLazy_InitAsync_DoesNotThrow()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            Func<Task> act = async () => await manager.InitAsync();

            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public void ManagerAsyncLazy_TryGetValue_MissingKey_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            manager.TryGetValue("NonExistent", out _).Should().BeFalse();
        }

        [TestMethod]
        public async Task ManagerAsyncLazy_AddAndRetrieve_Works()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            manager["TestKey"] = group.ToAsyncLazy();

            manager.TryGetValue("TestKey", out var task).Should().BeTrue();
            (await task).Should().BeSameAs(group);
        }

        #endregion

        #region Triage Additional Methods

        [TestMethod]
        public void Triage_CreateClassifier_SetsMinimumProbability()
        {
            var group = TriageClass.CreateClassifier();
            group.MinimumProbability.Should().Be(0.9);
            group.TotalEmailCount.Should().Be(0);
        }

        [TestMethod]
        public void Triage_Serialize_WithClassifierGroup_DoesNotThrow()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            triage.ClassifierGroup = new BayesianClassifierGroup();

            System.Action act = () => triage.Serialize();

            act.Should().NotThrow();
        }

        [TestMethod]
        public void Triage_Config_ReturnsClassifierGroupConfig()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            triage.ClassifierGroup = group;

            triage.Config.Should().BeSameAs(group.Config);
        }

        [TestMethod]
        public async Task Triage_TrainAsync_WithTokens_TrainsClassifier()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            triage.ClassifierGroup = TriageClass.CreateClassifier();

            await triage.TrainAsync(new[] { "hello", "world" }, "A");

            triage.ClassifierGroup.Should().NotBeNull();
        }

        [TestMethod]
        public void Triage_TypedItem_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            triage.TypedItem = null;

            triage.TypedItem.Should().BeNull();
        }

        [TestMethod]
        public void Triage_EngineInitializer_Throws()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            System.Action act = () =>
            {
                var _ = triage.EngineInitializer;
            };

            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public async Task TrainAsync_ObjectOverload_InvokesTokenizeAndCallback()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            triage.ClassifierGroup = TriageClass.CreateClassifier();
            bool tokenizeInvoked = false;
            bool callbackInvoked = false;
            string callbackTriageId = null;

            triage.TokenizeAsync = (_, _, _) =>
            {
                tokenizeInvoked = true;
                return Task.FromResult(new[] { "urgent", "deadline" });
            };
            triage.CallbackAsync = (_, triageId) =>
            {
                callbackInvoked = true;
                callbackTriageId = triageId;
                return Task.CompletedTask;
            };

            await triage.TrainAsync((object)"emailItem", "A");

            tokenizeInvoked.Should().BeTrue();
            callbackInvoked.Should().BeTrue();
            callbackTriageId.Should().Be("A");
        }

        #endregion
    }
}
