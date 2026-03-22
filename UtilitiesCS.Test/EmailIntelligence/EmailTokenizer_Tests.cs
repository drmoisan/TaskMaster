#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
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

        [TestMethod]
        public void TokenizeObject_WithDerivedMailItemHelper_UsesMailItemHelperBranch()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();
            var item = CreateItemInfo(
                subject: "Quarterly update",
                body: "alpha beta",
                htmlBody: string.Empty,
                sender: CreateRecipient("Alice", "alice@example.com")
            );
            var helper = new TestMailItemHelper(item.Object);

            // Act
            var tokens = tokenizer.Tokenize(helper, Mock.Of<IApplicationGlobals>()).ToArray();

            // Assert
            tokens.Should().Contain("charset:utf-8");
            tokens.Should().Contain("subject:Quarterly");
            tokens.Should().Contain("subject:update");
            tokens.Should().Contain("alpha");
            tokens.Should().Contain("beta");
        }

        [TestMethod]
        public void InternalHelpers_CoverFilenameWordAndEntityBranches()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();

            // Act
            var filenameTokens = InvokeEnumerable<string>(
                    tokenizer,
                    "crack_filename",
                    "reports/archive:fy2026.csv"
                )
                .ToArray();
            var emailWordTokens = InvokeEnumerable<string>(
                    tokenizer,
                    "tokenize_word",
                    "person@example.com",
                    (Func<string, int>?)null,
                    SpamBayesOptions.skip_max_word_size
                )
                .ToArray();
            var longWordTokens = InvokeEnumerable<string>(
                    tokenizer,
                    "tokenize_word",
                    "résumérésumérésumé",
                    (Func<string, int>?)null,
                    SpamBayesOptions.skip_max_word_size
                )
                .ToArray();
            var decodedEntity = (string)
                InvokeMethod(
                    tokenizer,
                    "NumericEntityReplacer",
                    Regex.Match("&#97;", @"&#(\d+);")
                )!;
            var invalidEntity = (string)
                InvokeMethod(
                    tokenizer,
                    "NumericEntityReplacer",
                    Regex.Match("&#999999999999999999999;", @"&#(\d+);")
                )!;

            // Assert
            filenameTokens
                .Should()
                .Contain(
                    new[]
                    {
                        "fname:reports/archive:fy2026.csv",
                        "fname comp:reports",
                        "fname comp:archive",
                        "fname piece:fy2026",
                        "fname piece:csv",
                    }
                );
            emailWordTokens
                .Should()
                .Contain(new[] { "email name:person", "email domain:example.com" });
            longWordTokens.Should().Contain(token => token.StartsWith("skip:r "));
            longWordTokens.Should().Contain(token => token.StartsWith("8bit%%:"));
            InvokeMethod(tokenizer, "has_highbit_char", "résumé").Should().Be(true);
            decodedEntity.Should().Be("a");
            invalidEntity.Should().Be("?");
        }

        [TestMethod]
        public void InternalHelpers_CoverTextHtmlAndImageBranches()
        {
            // Arrange
            var tokenizer = new EmailTokenizer();
            var nullAttachmentItem = CreateItemInfo(
                subject: "Status",
                body: "x y z important",
                htmlBody: "<script src='cid:test'></script><iframe></iframe>"
            );
            nullAttachmentItem.SetupGet(x => x.AttachmentsInfo).Returns((IAttachment[]?)null!);

            var mixedAttachmentItem = CreateItemInfo(
                subject: "Status",
                body: "hello",
                htmlBody: string.Empty,
                attachments: new[]
                {
                    CreateAttachment("image.png", isImage: true, size: 42),
                    CreateAttachment(string.Empty, isImage: false, size: 5),
                }
            );

            // Act
            var textTokens = InvokeEnumerable<string>(tokenizer, "tokenize_text", "x y z important")
                .ToArray();
            var htmlTokens = InvokeEnumerable<string>(
                    tokenizer,
                    "find_html_virus_clues",
                    "<script src='cid:test'></script><iframe></iframe>"
                )
                .ToArray();
            Action nullImagePartsAct = () =>
                InvokeMethod(tokenizer, "imageparts", nullAttachmentItem.Object);
            var mixedImageParts =
                (List<object>)InvokeMethod(tokenizer, "imageparts", mixedAttachmentItem.Object)!;
            var mixedContentTokens = InvokeEnumerable<string>(
                    tokenizer,
                    "crack_content_xyz",
                    mixedAttachmentItem.Object
                )
                .ToArray();

            // Assert
            textTokens.Should().Contain("important");
            textTokens.Should().Contain(token => token.StartsWith("short:"));
            htmlTokens.Should().BeEmpty();
            nullImagePartsAct
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>();
            mixedImageParts.Should().HaveCount(1);
            mixedContentTokens.Should().Contain("charset:utf-8");
            mixedContentTokens.Should().Contain("filename:<bogus>");
        }

        private static Mock<IItemInfo> CreateItemInfo(
            string subject,
            string? body,
            string? htmlBody,
            IRecipientInfo? sender = null,
            IRecipientInfo[]? toRecipients = null,
            IAttachment[]? attachments = null
        )
        {
            var mock = new Mock<IItemInfo>(MockBehavior.Loose);
            mock.SetupGet(x => x.Subject).Returns(subject);
            mock.SetupGet(x => x.Body).Returns(() => body!);
            mock.SetupGet(x => x.HTMLBody).Returns(() => htmlBody!);
            mock.SetupGet(x => x.InternetCodepage).Returns(65001);
            mock.SetupGet(x => x.Sender).Returns(() => sender!);
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

        private static object? InvokeMethod(
            object instance,
            string methodName,
            params object?[] args
        )
        {
            var method = instance
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Should().NotBeNull();
            return method!.Invoke(instance, args);
        }

        private static IEnumerable<T> InvokeEnumerable<T>(
            object instance,
            string methodName,
            params object?[] args
        )
        {
            return ((IEnumerable)InvokeMethod(instance, methodName, args)!).Cast<T>();
        }

        private sealed class TestMailItemHelper : MailItemHelper
        {
            public TestMailItemHelper(IItemInfo itemInfo)
                : base(itemInfo)
            {
                InternetCodepage = itemInfo.InternetCodepage;
            }
        }
    }
}
