using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class EmailTokenizer_Tests
    {
        [TestMethod]
        public void TokenizeObject_WithStringArray_ReturnsSameArray()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();
            var tokens = new[] { "alpha", "beta" };

            // Act
            var result = tokenizer.Tokenize(tokens, globals: Mock.Of<IApplicationGlobals>());

            // Assert
            result.Should().BeSameAs(tokens);
        }

        [TestMethod]
        public async Task TokenizeAsync_WithStringArray_ReturnsTokens()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();
            var tokens = new[] { "alpha", "beta" };

            // Act
            var result = await tokenizer.TokenizeAsync(
                tokens,
                Mock.Of<IApplicationGlobals>(),
                CancellationToken.None
            );

            // Assert
            result.Should().Equal(tokens);
        }

        [TestMethod]
        public void TokenizeObject_WithNullObject_ThrowsArgumentNullException()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();

            // Act
            Action act = () =>
                tokenizer.Tokenize(obj: null, globals: Mock.Of<IApplicationGlobals>()).ToArray();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("obj");
        }

        [TestMethod]
        public void TokenizeObject_WithUnsupportedType_ThrowsArgumentException()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();

            // Act
            Action act = () =>
                tokenizer.Tokenize(obj: 42, globals: Mock.Of<IApplicationGlobals>()).ToArray();

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TokenizeMessage_WithTypicalMetadataAndBody_ReturnsHeaderAndBodyTokens()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();
            var item = CreateItemInfo(
                subject: "FREE money!!",
                body: "hello résumé world",
                htmlBody: string.Empty,
                sender: CreateRecipient("Alice", "alice@example.com"),
                toRecipients: new[] { CreateRecipient("Bob", "bob@example.com") },
                attachments: new[]
                {
                    CreateAttachment("reports/fy2026.csv", isImage: false, size: 10),
                }
            );

            // Act
            var tokens = tokenizer.Tokenize(item.Object).ToArray();

            // Assert
            tokens
                .Should()
                .Contain(
                    new[]
                    {
                        "charset:utf-8",
                        "subject:FREE",
                        "subject:money",
                        "subject:!!",
                        "from:name:alice",
                        "from:addr:alice",
                        "from:addr:example.com",
                        "to:name:bob",
                        "to:addr:bob",
                        "to:addr:example.com",
                        "filename:fname comp:reports",
                        "filename:fname piece:fy2026",
                        "filename:fname piece:csv",
                        "hello",
                        "re?sume",
                        "world",
                    }
                );
        }

        [TestMethod]
        public void TokenizeMessage_WhenBodyIsNull_YieldsControlToken()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();
            var item = CreateItemInfo(subject: "Status", body: null, htmlBody: string.Empty);

            // Act
            var tokens = tokenizer.Tokenize(item.Object).ToArray();

            // Assert
            tokens.Should().Contain("control: text payload is None");
        }

        [TestMethod]
        public void TokenizeMessage_WithHtmlBody_DoesNotPreventTokenization()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();
            var item = CreateItemInfo(
                subject: "Status",
                body: "hello",
                htmlBody: "<script src='cid:test'></script><iframe></iframe>"
            );

            // Act
            var tokens = tokenizer.Tokenize(item.Object).ToArray();

            // Assert
            tokens.Should().Contain("charset:utf-8");
            tokens.Should().Contain("subject:Status");
            tokens.Should().Contain("hello");
        }

        private static Mock<IItemInfo> CreateItemInfo(
            string subject,
            string body,
            string htmlBody,
            IRecipientInfo sender = null,
            IRecipientInfo[] toRecipients = null,
            IAttachment[] attachments = null
        )
        {
            var mock = new Mock<IItemInfo>(MockBehavior.Loose);
            mock.SetupGet(x => x.Subject).Returns(subject);
            mock.SetupGet(x => x.Body).Returns(body);
            mock.SetupGet(x => x.HTMLBody).Returns(htmlBody);
            mock.SetupGet(x => x.InternetCodepage).Returns(65001);
            mock.SetupGet(x => x.Sender).Returns(sender);
            mock.SetupGet(x => x.ToRecipients)
                .Returns(toRecipients ?? Array.Empty<IRecipientInfo>());
            mock.SetupGet(x => x.CcRecipients).Returns(Array.Empty<IRecipientInfo>());
            mock.SetupGet(x => x.AttachmentsInfo)
                .Returns(attachments ?? Array.Empty<IAttachment>());
            return mock;
        }

        private static IRecipientInfo CreateRecipient(string name, string address)
        {
            var mock = new Mock<IRecipientInfo>(MockBehavior.Loose);
            mock.SetupProperty(x => x.Name, name);
            mock.SetupProperty(x => x.Address, address);
            return mock.Object;
        }

        private static IAttachment CreateAttachment(string fileName, bool isImage, int size)
        {
            var mock = new Mock<IAttachment>(MockBehavior.Loose);
            mock.SetupGet(x => x.FileName).Returns(fileName);
            mock.SetupGet(x => x.IsImage).Returns(isImage);
            mock.SetupGet(x => x.Size).Returns(size);
            return mock.Object;
        }
    }
}
