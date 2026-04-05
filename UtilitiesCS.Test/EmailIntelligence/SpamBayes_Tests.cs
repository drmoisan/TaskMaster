using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class SpamBayes_Additional_Tests
    {
        [TestMethod]
        public async Task CreateAsync_WhenPathsAreInvalid_ReturnsNull()
        {
            var globals = CreateMockGlobals();

            var result = await SpamBayes.CreateAsync(globals.Object);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task CreateAsync_WhenTreatmentIsCreate_BuildsAndInitializesClassifier()
        {
            var globals = CreateMockGlobals();
            var manager = ConfigureManager(globals);
            ConfigureValidFolders(globals);

            var result = await SpamBayes.CreateAsync(
                globals.Object,
                treatment: Enums.NotFoundEnum.Create
            );

            result.Should().NotBeNull();
            result.ClassifierGroup.Should().NotBeNull();
            result.IsActivated.Should().BeTrue();
            result.Tokenize.Should().NotBeNull();
            result.TokenizeAsync.Should().NotBeNull();
            result.CalculateProbability.Should().NotBeNull();
            result.CalculateProbabilityAsync.Should().NotBeNull();
            result.CallbackAsync.Should().NotBeNull();
            result.Threshhold.MinimumTrue.Should().Be(0.8);
            result.Threshhold.MaximumFalse.Should().Be(0.2);
            manager.Should().ContainKey(SpamBayes.GroupName);
        }

        [TestMethod]
        public async Task InitAsync_WhenManagerMissingSpam_ReturnsNull()
        {
            var globals = CreateMockGlobals();
            ConfigureManager(globals);
            var spamBayes = new SpamBayes(globals.Object);

            var result = await spamBayes.InitAsync();

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task HasValidSpamClassifierAsync_WhenSpamTaskResolvesNull_ReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var manager = ConfigureManager(globals);
            manager[SpamBayes.GroupName] = ((BayesianClassifierGroup)null).ToAsyncLazy();
            var spamBayes = new SpamBayes(globals.Object);

            var (isValid, message) = await spamBayes.HasValidSpamClassifierAsync(default);

            isValid.Should().BeFalse();
            message.Should().Contain("Spam");
        }

        [TestMethod]
        public async Task HasValidSpamClassifierAsync_WhenClassifierIsMissing_ReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var manager = ConfigureManager(globals);
            var group = new BayesianClassifierGroup
            {
                TotalEmailCount = 0,
                SharedTokenBase = new Corpus(),
                Name = SpamBayes.GroupName,
            };
            group.Classifiers["Spam"] = new BayesianClassifierShared("Spam", group);
            manager[SpamBayes.GroupName] = group.ToAsyncLazy();
            var spamBayes = new SpamBayes(globals.Object);

            var (isValid, message) = await spamBayes.HasValidSpamClassifierAsync(default);

            isValid.Should().BeFalse();
            message.Should().Contain("classifier named Ham");
        }

        [TestMethod]
        public void Config_WhenClassifierGroupAssigned_ReturnsGroupConfig()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var group = new BayesianClassifierGroup();
            spamBayes.ClassifierGroup = group;

            spamBayes.Config.Should().BeSameAs(group.Config);
        }

        [TestMethod]
        public void TestAsync_Selection_WhenClassifierGroupIsNull_Completes()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);

            Func<Task> act = async () => await spamBayes.TestAsync((Selection)null);

            act.Should().NotThrowAsync();
        }

        [TestMethod]
        public void TrainAsync_Selection_WhenClassifierGroupIsNull_Completes()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);

            Func<Task> act = async () => await spamBayes.TrainAsync((Selection)null, isSpam: true);

            act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task TrainAsync_WithTokens_TrainsRequestedClassifier()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object)
            {
                ClassifierGroup = SpamBayes.CreateNewClassifier(),
            };

            await spamBayes.TrainAsync(new[] { "alpha", "beta" }, isSpam: false);
            spamBayes.ClassifierGroup.Classifiers.Should().ContainKey("Ham");
        }

        [TestMethod]
        public void TokenizeEmail_NullInput_ReturnsEmptyArray()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);

            spamBayes.TokenizeEmail(null).Should().BeEmpty();
        }

        [TestMethod]
        public async Task TokenizeEmailAsync_NullInput_ReturnsEmptyArray()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);

            var result = await spamBayes.TokenizeEmailAsync(null);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Train_WhenCalled_ThrowsNotImplementedException()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);

            System.Action act = () => spamBayes.Train(Array.Empty<string>(), true);

            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public async Task CreateEngineAsync_WithValidGlobals_ReturnsSpamBayes()
        {
            var globals = CreateMockGlobals();
            var manager = ConfigureManager(globals);
            ConfigureValidFolders(globals);
            manager[SpamBayes.GroupName] = SpamBayes.CreateNewClassifier().ToAsyncLazy();

            var result = await SpamBayes.CreateEngineAsync(globals.Object);

            result.Should().BeOfType<SpamBayes>();
        }

        [TestMethod]
        public async Task ConditionalEngine_SurfaceMembers_ReturnExpectedValues()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object)
            {
                ClassifierGroup = new BayesianClassifierGroup(),
                CalculateProbabilityAsync = _ => Task.FromResult(0.5),
                Threshhold = new TristateThreshhold(0.8, 0.2),
            };
            var helper = new MailItemHelper();

            ((IConditionalEngine<MailItemHelper>)spamBayes).Serialize();
            await spamBayes.AsyncAction(helper);

            spamBayes.Engine.Should().BeSameAs(spamBayes);
            spamBayes.EngineInitializer.Should().NotBeNull();
            spamBayes.EngineName.Should().Be("Spam");
            spamBayes.Message.Should().Contain("SpamBayes");
            spamBayes.TypedItem = helper;
            spamBayes.TypedItem.Should().BeSameAs(helper);
        }

        [TestMethod]
        public void Condition_WhenItemIsNotMailItem_ReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);

            InvokePrivate<bool>(spamBayes, "Condition", new object()).Should().BeFalse();
        }

        [TestMethod]
        public void Condition_WhenMessageClassIsNotNote_ReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var mailItem = CreateMailItem(messageClass: "IPM.Schedule");

            InvokePrivate<bool>(spamBayes, "Condition", mailItem.Object).Should().BeFalse();
        }

        [TestMethod]
        public void Condition_WhenSpamPropertyExists_SetsAutoProcessedAndReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var spamProperty = CreateUserProperty();
            var autoProcessed = CreateUserProperty();
            var userProperties = CreateUserProperties(
                spamProperty.Object,
                autoProcessed.Object,
                null
            );
            var mailItem = CreateMailItem(userProperties: userProperties);

            var result = InvokePrivate<bool>(spamBayes, "Condition", mailItem.Object);

            result.Should().BeFalse();
            autoProcessed.Object.Value.Should().Be(true);
            mailItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void Condition_WhenSpamPropertyMissing_ReturnsTrue()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var userProperties = CreateUserProperties(null, null, null);
            var mailItem = CreateMailItem(userProperties: userProperties);

            InvokePrivate<bool>(spamBayes, "Condition", mailItem.Object).Should().BeTrue();
        }

        [TestMethod]
        public async Task AsyncCondition_WhenItemIsNotMailItem_ReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var appointment = new Mock<AppointmentItem>(MockBehavior.Loose);

            var result = await spamBayes.AsyncCondition(appointment.Object);

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task AsyncCondition_WhenMessageClassIsNotNote_ReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var mailItem = CreateMailItem(messageClass: "IPM.Schedule");

            var result = await spamBayes.AsyncCondition(mailItem.Object);

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task AsyncCondition_WhenSpamPropertyMissing_ReturnsTrue()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var userProperties = CreateUserProperties(null, null, null);
            var mailItem = CreateMailItem(userProperties: userProperties);

            var result = await spamBayes.AsyncCondition(mailItem.Object);

            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task AsyncCondition_WhenSpamPropertyExistsWithoutAutoProcessed_AddsFlagAndReturnsFalse()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);
            var spamProperty = CreateUserProperty(0.9);
            var addedProperty = CreateUserProperty();
            var userProperties = CreateUserProperties(
                spamProperty.Object,
                null,
                addedProperty.Object
            );
            var mailItem = CreateMailItem(userProperties: userProperties);

            var result = await spamBayes.AsyncCondition(mailItem.Object);

            result.Should().BeFalse();
            addedProperty.Object.Value.Should().Be(true);
            mailItem.Verify(x => x.Save(), Times.Once);
            userProperties.Verify(
                x =>
                    x.Add(
                        "AutoProcessed",
                        OlUserPropertyType.olYesNo,
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public void GetDestinationFolder_WhenSpamTrue_ReturnsJunkCertain()
        {
            var globals = CreateMockGlobals();
            var folders = ConfigureValidFolders(globals);
            var spamBayes = new SpamBayes(globals.Object);
            var mailItem = CreateMailItem(folderPath: "\\Mailbox\\Inbox");

            var result = spamBayes.GetDestinationFolder(mailItem.Object, true);

            result.Should().BeSameAs(folders.JunkCertain.Object);
        }

        [TestMethod]
        public void GetDestinationFolder_WhenIndeterminateOutsidePotential_ReturnsJunkPotential()
        {
            var globals = CreateMockGlobals();
            var folders = ConfigureValidFolders(globals);
            var spamBayes = new SpamBayes(globals.Object);
            var mailItem = CreateMailItem(folderPath: "\\Mailbox\\Inbox");

            var result = spamBayes.GetDestinationFolder(mailItem.Object, null);

            result.Should().BeSameAs(folders.JunkPotential.Object);
        }

        [TestMethod]
        public void GetDestinationFolder_WhenIndeterminateAlreadyInPotential_ReturnsNull()
        {
            var globals = CreateMockGlobals();
            ConfigureValidFolders(globals);
            var spamBayes = new SpamBayes(globals.Object);
            var mailItem = CreateMailItem(folderPath: "\\Mailbox\\JunkPotential");

            var result = spamBayes.GetDestinationFolder(mailItem.Object, null);

            result.Should().BeNull();
        }

        [TestMethod]
        public void MoveSpamOrHam_WithHelperAndDestination_ReplacesHelperItem()
        {
            var globals = CreateMockGlobals();
            var folders = ConfigureValidFolders(globals);
            var spamBayes = new SpamBayes(globals.Object);
            var original = CreateMailItem(folderPath: "\\Mailbox\\Inbox");
            var moved = new Mock<MailItem>(MockBehavior.Loose);
            original.Setup(x => x.Move(folders.JunkPotential.Object)).Returns(moved.Object);
            var helper = new MailItemHelper { Item = original.Object };

            spamBayes.MoveSpamOrHam(helper, null);

            helper.Item.Should().BeSameAs(moved.Object);
        }

        [TestMethod]
        public void MoveSpamOrHam_WithMailItemAndDestination_MovesMail()
        {
            var globals = CreateMockGlobals();
            var folders = ConfigureValidFolders(globals);
            var spamBayes = new SpamBayes(globals.Object);
            var mailItem = CreateMailItem(folderPath: "\\Mailbox\\Inbox");

            spamBayes.MoveSpamOrHam(mailItem.Object, true);

            mailItem.Verify(x => x.Move(folders.JunkCertain.Object), Times.Once);
        }

        [TestMethod]
        public async Task TestAsync_Object_WhenInputIsUnknown_Completes()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object);

            Func<Task> act = async () => await spamBayes.TestAsync(new object());

            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task TestAsync_Selection_WhenInputContainsMailItem_ProcessesMessage()
        {
            var globals = CreateMockGlobals();
            var folders = ConfigureValidFolders(globals);
            var spamBayes = new SpamBayes(globals.Object)
            {
                ClassifierGroup = new BayesianClassifierGroup(),
                TokenizeAsync = _ => Task.FromResult(new[] { "token" }),
                CalculateProbabilityAsync = _ => Task.FromResult(0.9),
                Threshhold = new TristateThreshhold(0.8, 0.2),
            };
            var userProperties = CreateWritableUserProperties(CreateUserProperty().Object);
            var moved = new Mock<MailItem>(MockBehavior.Loose);
            var mailItem = CreateMailItem(
                folderPath: "\\Mailbox\\Inbox",
                userProperties: userProperties
            );
            mailItem.Setup(x => x.Move(folders.JunkCertain.Object)).Returns(moved.Object);
            var selection = new Mock<Selection>(MockBehavior.Loose);
            selection
                .Setup(x => x.GetEnumerator())
                .Returns(
                    (
                        (System.Collections.Generic.IEnumerable<object>)new[] { mailItem.Object }
                    ).GetEnumerator()
                );

            await spamBayes.TestAsync(selection.Object);

            mailItem.Verify(x => x.Move(folders.JunkCertain.Object), Times.Once);
        }

        [TestMethod]
        public async Task TestAsync_Object_WhenInputIsMailItem_ProcessesMessage()
        {
            var globals = CreateMockGlobals();
            var folders = ConfigureValidFolders(globals);
            var spamBayes = new SpamBayes(globals.Object)
            {
                ClassifierGroup = new BayesianClassifierGroup(),
                TokenizeAsync = _ => Task.FromResult(new[] { "token" }),
                CalculateProbabilityAsync = _ => Task.FromResult(0.9),
                Threshhold = new TristateThreshhold(0.8, 0.2),
            };
            var userProperties = CreateWritableUserProperties(CreateUserProperty().Object);
            var moved = new Mock<MailItem>(MockBehavior.Loose);
            var mailItem = CreateMailItem(
                folderPath: "\\Mailbox\\Inbox",
                userProperties: userProperties
            );
            mailItem.Setup(x => x.Move(folders.JunkCertain.Object)).Returns(moved.Object);

            await spamBayes.TestAsync(mailItem.Object);

            mailItem.Verify(x => x.Move(folders.JunkCertain.Object), Times.Once);
        }

        [TestMethod]
        public async Task TestAsync_IItemInfo_UsesProbabilityDelegateAndCompletes()
        {
            var globals = CreateMockGlobals();
            var spamBayes = new SpamBayes(globals.Object)
            {
                CalculateProbabilityAsync = _ => Task.FromResult(0.1),
                Threshhold = new TristateThreshhold(0.8, 0.2),
            };
            var itemInfo = new Mock<IItemInfo>();
            itemInfo.SetupGet(x => x.Tokens).Returns(new[] { "one", "two" });

            Func<Task> act = async () => await spamBayes.TestAsync(itemInfo.Object);

            await act.Should().NotThrowAsync();
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var globals = new Mock<IApplicationGlobals>();
            var folders = new Mock<IOlObjects>();
            var fileSystem = new Mock<IFileSystemFolderPaths>();
            var autoFile = new Mock<IAppAutoFileObjects>();
            globals.Setup(x => x.Ol).Returns(folders.Object);
            globals.Setup(x => x.FS).Returns(fileSystem.Object);
            globals.Setup(x => x.AF).Returns(autoFile.Object);
            return globals;
        }

        private static ManagerAsyncLazy ConfigureManager(Mock<IApplicationGlobals> globals)
        {
            var autoFile = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(globals.Object);
            autoFile.Setup(x => x.Manager).Returns(manager);
            globals.Setup(x => x.AF).Returns(autoFile.Object);
            return manager;
        }

        private static (
            Mock<Folder> JunkCertain,
            Mock<Folder> JunkPotential,
            Mock<Folder> Inbox
        ) ConfigureValidFolders(Mock<IApplicationGlobals> globals)
        {
            var ol = new Mock<IOlObjects>();
            var junkCertain = new Mock<Folder>(MockBehavior.Loose);
            var junkPotential = new Mock<Folder>(MockBehavior.Loose);
            var inbox = new Mock<Folder>(MockBehavior.Loose);
            junkCertain.Setup(x => x.FolderPath).Returns("\\Mailbox\\JunkCertain");
            junkPotential.Setup(x => x.FolderPath).Returns("\\Mailbox\\JunkPotential");
            inbox.Setup(x => x.FolderPath).Returns("\\Mailbox\\Inbox");
            ol.Setup(x => x.JunkCertain).Returns(junkCertain.Object);
            ol.Setup(x => x.JunkPotential).Returns(junkPotential.Object);
            ol.Setup(x => x.Inbox).Returns(inbox.Object);
            globals.Setup(x => x.Ol).Returns(ol.Object);
            return (junkCertain, junkPotential, inbox);
        }

        private static Mock<MailItem> CreateMailItem(
            string messageClass = "IPM.Note",
            string folderPath = "\\Mailbox\\Inbox",
            Mock<UserProperties> userProperties = null
        )
        {
            var parent = new Mock<Folder>(MockBehavior.Loose);
            parent.Setup(x => x.FolderPath).Returns(folderPath);

            var sender = new Mock<AddressEntry>(MockBehavior.Loose);
            sender.Setup(x => x.Name).Returns("Sender");

            var mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem.Setup(x => x.MessageClass).Returns(messageClass);
            mailItem.Setup(x => x.Parent).Returns(parent.Object);
            mailItem.Setup(x => x.CreationTime).Returns(new DateTime(2024, 1, 2, 3, 4, 5));
            mailItem.Setup(x => x.Subject).Returns("Subject");
            mailItem.Setup(x => x.Sender).Returns(sender.Object);
            mailItem.Setup(x => x.UserProperties).Returns(userProperties?.Object);
            return mailItem;
        }

        private static Mock<UserProperty> CreateUserProperty(object value = null)
        {
            var property = new Mock<UserProperty>(MockBehavior.Loose);
            property.SetupAllProperties();
            property.Object.Value = value;
            return property;
        }

        private static Mock<UserProperties> CreateUserProperties(
            UserProperty spamProperty,
            UserProperty autoProcessedProperty,
            UserProperty addedProperty
        )
        {
            var userProperties = new Mock<UserProperties>(MockBehavior.Loose);
            userProperties.Setup(x => x.Find("Spam", It.IsAny<object>())).Returns(spamProperty);
            userProperties
                .Setup(x => x.Find("AutoProcessed", It.IsAny<object>()))
                .Returns(autoProcessedProperty);
            userProperties
                .Setup(x =>
                    x.Add(
                        "AutoProcessed",
                        OlUserPropertyType.olYesNo,
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(addedProperty);
            return userProperties;
        }

        private static Mock<UserProperties> CreateWritableUserProperties(UserProperty addedProperty)
        {
            var userProperties = new Mock<UserProperties>(MockBehavior.Loose);
            userProperties
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            userProperties
                .Setup(x => x.Add(It.IsAny<string>(), It.IsAny<OlUserPropertyType>()))
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

        private static T InvokePrivate<T>(
            SpamBayes spamBayes,
            string methodName,
            params object[] args
        )
        {
            var method = typeof(SpamBayes).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            return (T)method.Invoke(spamBayes, args);
        }
    }
}
