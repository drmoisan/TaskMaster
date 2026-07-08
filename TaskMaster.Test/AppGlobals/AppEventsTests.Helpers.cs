using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using Moq;
using TaskMaster.Properties;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster.Test.AppGlobals
{
    public partial class AppEventsTests
    {
        private static Mock<IApplicationGlobals> CreateGlobalsWithNoEngines()
        {
            var engines = new Mock<IAppItemEngines>(MockBehavior.Strict);
            engines
                .SetupGet(x => x.InboxEngines)
                .Returns(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>());

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.Engines).Returns(engines.Object);
            return globals;
        }

        private static Mock<IApplicationGlobals> CreateGlobalsWithApplicableEngine()
        {
            var conditionalEngine = new Mock<IConditionalEngine<MailItemHelper>>(
                MockBehavior.Strict
            );
            conditionalEngine.SetupGet(x => x.Engine).Returns(new object());
            conditionalEngine.SetupGet(x => x.AsyncCondition).Returns(_ => Task.FromResult(true));
            conditionalEngine.SetupGet(x => x.AsyncAction).Returns(_ => Task.CompletedTask);

            var engines = new Mock<IAppItemEngines>(MockBehavior.Strict);
            engines
                .SetupGet(x => x.InboxEngines)
                .Returns(
                    new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>(
                        new[]
                        {
                            new System.Collections.Generic.KeyValuePair<
                                string,
                                IConditionalEngine<MailItemHelper>
                            >("engine-1", conditionalEngine.Object),
                        }
                    )
                );

            var olObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var archiveRoot = new Mock<Folder>(MockBehavior.Strict);
            var inbox = new Mock<Folder>(MockBehavior.Strict);
            archiveRoot.SetupGet(x => x.FolderPath).Returns("\\Archive");
            inbox.SetupGet(x => x.FolderPath).Returns("\\Inbox");
            olObjects.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot.Object);
            olObjects.SetupGet(x => x.Inbox).Returns(inbox.Object);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns("\\Archive");
            olObjects.SetupGet(x => x.EmailPrefixToStrip).Returns(string.Empty);

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.Engines).Returns(engines.Object);
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static Mock<IApplicationGlobals> CreateGlobalsWithHookableOutlookObjects()
        {
            var globals = CreateGlobalsWithNoEngines();
            var olObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var inboxItems = new Mock<Items>(MockBehavior.Loose);
            inboxItems
                .Setup(x => x.Restrict("[MessageClass] = 'IPM.Note'"))
                .Returns(BuildUnprocessedInboxItems(0));
            var toDoFolder = Mock.Of<Folder>(x => x.Items == Mock.Of<Items>());
            var inboxFolder = Mock.Of<Folder>(x => x.Items == inboxItems.Object);

            olObjects.SetupGet(x => x.App).Returns(Mock.Of<Application>());
            olObjects.SetupGet(x => x.ToDoFolder).Returns(toDoFolder);
            olObjects.SetupGet(x => x.OlReminders).Returns(Mock.Of<Reminders>());
            olObjects.SetupGet(x => x.Inboxes).Returns(new[] { inboxFolder });

            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static LockingLinkedList<Items> BuildInboxSubscriptions(params Items[] inboxes)
        {
            var list = new LockingLinkedList<Items>();
            foreach (var inbox in inboxes)
            {
                list.AddLast(inbox);
            }

            return list;
        }

        private static Items BuildUnprocessedInboxItems(int itemCount)
        {
            var mailItems = Enumerable.Range(0, itemCount).Select(_ => CreateMailItem()).ToArray();
            return BuildUnprocessedInboxItems(mailItems, itemCount);
        }

        private static Items BuildUnprocessedInboxItems(MailItem mailItem, int itemCount)
        {
            var mailItems = Enumerable.Range(0, itemCount).Select(_ => mailItem).ToArray();
            return BuildUnprocessedInboxItems(mailItems, itemCount);
        }

        private static Items BuildUnprocessedInboxItems(MailItem[] mailItems, int itemCount)
        {
            var restrictedItems = new Mock<Items>(MockBehavior.Strict);
            restrictedItems
                .Setup(x => x.GetEnumerator())
                .Returns(() => mailItems.Cast<object>().GetEnumerator());
            restrictedItems
                .Setup(x => x.Restrict(It.Is<string>(filter => filter.Contains("AutoProcessed"))))
                .Returns(restrictedItems.Object);

            var inboxItems = new Mock<Items>(MockBehavior.Strict);
            inboxItems
                .Setup(x => x.Restrict("[MessageClass] = 'IPM.Note'"))
                .Returns(restrictedItems.Object);
            return inboxItems.Object;
        }

        private static MailItem CreateMailItem()
        {
            var userProperties = new Mock<UserProperties>(MockBehavior.Strict);
            userProperties
                .Setup(x => x.Find("AutoProcessed", It.IsAny<object>()))
                .Returns((UserProperty)null!);

            var mailItem = new Mock<MailItem>(MockBehavior.Strict);
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            return mailItem.Object;
        }

        private static MailItem CreateProcessableMailItem()
        {
            var senderAccessor = new Mock<PropertyAccessor>(MockBehavior.Loose);
            var sender = new Mock<AddressEntry>(MockBehavior.Strict);
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            sender.SetupGet(x => x.Name).Returns("Ada Sender");
            sender.SetupGet(x => x.Address).Returns("ada@example.com");
            sender.SetupGet(x => x.PropertyAccessor).Returns(senderAccessor.Object);

            var toAccessor = new Mock<PropertyAccessor>(MockBehavior.Loose);
            var toAddress = new Mock<AddressEntry>(MockBehavior.Strict);
            toAddress
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            toAddress.SetupGet(x => x.Name).Returns("To User");
            toAddress.SetupGet(x => x.Address).Returns("to@example.com");
            toAddress.SetupGet(x => x.PropertyAccessor).Returns(toAccessor.Object);

            var toRecipient = new Mock<Recipient>(MockBehavior.Strict);
            toRecipient.SetupGet(x => x.Name).Returns("To User");
            toRecipient.SetupGet(x => x.Address).Returns("to@example.com");
            toRecipient.SetupGet(x => x.Type).Returns((int)OlMailRecipientType.olTo);
            toRecipient.SetupGet(x => x.AddressEntry).Returns(toAddress.Object);
            toRecipient.SetupGet(x => x.PropertyAccessor).Returns(toAccessor.Object);

            var recipients = new Mock<Recipients>(MockBehavior.Strict);
            recipients.SetupGet(x => x.Count).Returns(1);
            recipients
                .Setup(x => x.GetEnumerator())
                .Returns(() =>
                    new object[] { toRecipient.Object }
                        .Cast<object>()
                        .GetEnumerator()
                );

            var attachments = new Mock<Attachments>(MockBehavior.Strict);
            attachments.SetupGet(x => x.Count).Returns(0);
            attachments
                .Setup(x => x.GetEnumerator())
                .Returns(() => System.Array.Empty<object>().Cast<object>().GetEnumerator());

            var parentFolder = new Mock<Folder>(MockBehavior.Strict);
            parentFolder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Projects");

            var autoProcessed = new Mock<UserProperty>(MockBehavior.Strict);
            autoProcessed.SetupProperty(x => x.Value, false);
            var userProperties = new Mock<UserProperties>(MockBehavior.Strict);
            userProperties
                .Setup(x => x.Find("AutoProcessed", It.IsAny<object>()))
                .Returns((UserProperty)null!);
            userProperties
                .Setup(x =>
                    x.Find(It.Is<string>(name => name == "AutoProcessed"), It.IsAny<object>())
                )
                .Returns((UserProperty)null!);
            userProperties
                .Setup(x =>
                    x.Add(
                        It.Is<string>(name => name == "AutoProcessed"),
                        It.Is<OlUserPropertyType>(kind => kind == OlUserPropertyType.olYesNo),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(autoProcessed.Object);

            var mailItem = new Mock<MailItem>(MockBehavior.Strict);
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            mailItem.SetupGet(x => x.Subject).Returns("Subject");
            mailItem.SetupGet(x => x.Body).Returns("Body");
            mailItem.SetupGet(x => x.HTMLBody).Returns("<html><body>Body</body></html>");
            mailItem.SetupGet(x => x.InternetCodepage).Returns(65001);
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Sender");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.EntryID).Returns("entry-1");
            mailItem.SetupGet(x => x.Sender).Returns(sender.Object);
            mailItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            mailItem.SetupGet(x => x.Attachments).Returns(attachments.Object);
            mailItem.SetupGet(x => x.Parent).Returns(parentFolder.Object);
            mailItem.Setup(x => x.Save());

            return mailItem.Object;
        }

        private static MemoryAppender AttachMemoryAppender(System.Type targetType)
        {
            var appender = new MemoryAppender();
            appender.ActivateOptions();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = (Logger)hierarchy.GetLogger(targetType.FullName);
            logger.Level = log4net.Core.Level.Debug;
            logger.AddAppender(appender);
            logger.Repository.Configured = true;
            return appender;
        }

        private static void DetachMemoryAppender(System.Type targetType, MemoryAppender appender)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = (Logger)hierarchy.GetLogger(targetType.FullName);
            logger.RemoveAppender(appender);
        }

        private static int FindMessageIndex(string[] messages, string fragment)
        {
            var index = System.Array.FindIndex(messages, message => message.Contains(fragment));
            index.Should().BeGreaterThanOrEqualTo(0, $"the logs should contain '{fragment}'");
            return index;
        }
    }
}
