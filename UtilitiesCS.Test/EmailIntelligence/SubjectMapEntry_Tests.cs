#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SubjectMapEntry_Tests
    {
        [TestMethod]
        public void Constructor_WithFolderSubjectCountAndCommonWords_InitializesNormalizedState()
        {
            // Arrange
            IList<string> commonWords = new List<string> { "fwd" };

            // Act
            var entry = new SubjectMapEntry("Inbox\\Reports", "Fwd project plan", 3, commonWords);

            // Assert
            entry.Folderpath.Should().Be("Inbox\\Reports");
            entry.Foldername.Should().Be("Reports");
            entry.EmailSubject.Should().Be("project plan");
            entry.EmailSubjectCount.Should().Be(3);
            entry.FolderWordLengths.Should().Equal("reports".Length);
            entry.SubjectWordLengths.Should().Equal("project".Length, "plan".Length);
        }

        [TestMethod]
        public void Folderpath_Setter_UpdatesFolderNameAndWordLengths()
        {
            // Arrange
            var entry = new SubjectMapEntry("status update", 1, new List<string>());

            // Act
            entry.Folderpath = "Inbox\\Action Items";

            // Assert
            entry.Foldername.Should().Be("Action Items");
            entry.FolderWordLengths.Should().Equal("action".Length, "items".Length);
        }

        [TestMethod]
        public void EmailSubject_Setter_WithNull_ClearsSubjectState()
        {
            // Arrange
            var entry = new SubjectMapEntry(
                "Inbox\\Reports",
                "Status update",
                2,
                new List<string>()
            );

            // Act
            entry.EmailSubject = null;

            // Assert
            entry.EmailSubject.Should().BeEmpty();
            entry.SubjectWordLengths.Should().BeEmpty();
            entry.SubjectEncoded.Should().BeNull();
        }

        [TestMethod]
        public void Equals_ReturnsTrueOnlyWhenFolderAndSubjectMatch()
        {
            // Arrange
            var left = new SubjectMapEntry(
                "Inbox\\Reports",
                "Status update",
                1,
                new List<string>()
            );
            var same = new SubjectMapEntry(
                "Inbox\\Reports",
                "Status update",
                5,
                new List<string>()
            );
            var different = new SubjectMapEntry(
                "Inbox\\Reports",
                "Different subject",
                1,
                new List<string>()
            );

            // Act
            var sameResult = left.Equals(same);
            var differentResult = left.Equals(different);

            // Assert
            sameResult.Should().BeTrue();
            differentResult.Should().BeFalse();
        }

        [TestMethod]
        public void ReadyToEncode_WithEncoder_AugmentsTokenDictionary()
        {
            // Arrange
            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            encoder.Setup(mock => mock.AugmentTokenDict(It.IsAny<string[]>())).Verifiable();
            var entry = new SubjectMapEntry(
                "Inbox\\Reports",
                "Status update",
                2,
                new List<string>()
            );

            // Act
            var ready = entry.ReadyToEncode(encoder.Object);

            // Assert
            ready.Should().BeTrue();
            encoder.Verify(
                mock =>
                    mock.AugmentTokenDict(
                        It.Is<string[]>(tokens =>
                            tokens.Length == 3
                            && Array.Exists(tokens, token => token == "reports")
                            && Array.Exists(tokens, token => token == "status")
                            && Array.Exists(tokens, token => token == "update")
                        )
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public void Encode_WithEncoderAndTokens_ReturnsEncodedValues()
        {
            // Arrange
            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            encoder.Setup(mock => mock.AugmentTokenDict(It.IsAny<string[]>())).Verifiable();
            encoder.Setup(mock => mock.Encode(It.IsAny<string[]>())).Returns(new[] { 10, 20 });
            var entry = new SubjectMapEntry();

            // Act
            var encoded = entry.Encode(encoder.Object, new[] { "alpha", "beta" });

            // Assert
            encoded.Should().Equal(10, 20);
            encoder.Verify(
                mock => mock.AugmentTokenDict(It.Is<string[]>(tokens => tokens.Length == 2)),
                Times.Once
            );
            encoder.Verify(
                mock => mock.Encode(It.Is<string[]>(tokens => tokens.Length == 2)),
                Times.Once
            );
        }

        [TestMethod]
        public void Constructor_WhenCommonWordsStripAllTokens_ThrowsInvalidOperationException()
        {
            // Arrange
            IList<string> commonWords = new List<string> { "fwd" };

            // Act
            Action act = () => new SubjectMapEntry("Fwd", 1, commonWords);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*has no valid tokens*");
        }

        [TestMethod]
        public void Constructors_WithRegexOverloads_InitializeExpectedState()
        {
            // Arrange
            var regex = new Regex("[A-Za-z]+", RegexOptions.Compiled);

            // Act
            var empty = new SubjectMapEntry(regex);
            var folderEntry = new SubjectMapEntry(
                "Inbox\\Action Items",
                "Re plan update",
                2,
                new List<string> { "re" },
                regex
            );
            Action subjectEntryAct = () => new SubjectMapEntry("Weekly summary", 4, regex);
            Action subjectOnlyAct = () => new SubjectMapEntry("Quarterly update", 3);

            // Assert
            empty.TokenizerRegex.Should().Be(regex);
            folderEntry.Foldername.Should().Be("Action Items");
            folderEntry.EmailSubject.Should().Be("plan update");
            subjectEntryAct.Should().Throw<NullReferenceException>();
            subjectOnlyAct.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void FolderpathAndEmailSubject_WithEncoder_EncodeValuesAndSupportPropertyRoundTrip()
        {
            // Arrange
            var encoder = CreateEncoderMock();
            var entry = new SubjectMapEntry("status update", 1, new List<string> { "re" });
            entry.CommonWords.Should().Contain("re");
            entry.EmailSubjectCount = 5;
            entry.Score = 7;
            entry.TokenizerRegex = new Regex("[A-Za-z]+", RegexOptions.Compiled);

            // Act
            entry.Encoder = encoder.Object;
            entry.Folderpath = "Inbox\\Reports";
            entry.EmailSubject = "status update";

            // Assert
            entry.Foldername.Should().Be("Reports");
            entry.FolderEncoded.Should().Equal(31);
            entry.EmailSubject.Should().Be("status update");
            entry.SubjectEncoded.Should().Equal(21, 22);
            entry.EmailSubjectCount.Should().Be(5);
            entry.Score.Should().Be(7);
            entry.FolderWordLengths.Should().Equal(7);
            entry.SubjectWordLengths.Should().Equal(6, 6);
        }

        [TestMethod]
        public void EncodeOverloads_AndScalarProperties_CoverAdditionalPublicBranches()
        {
            // Arrange
            var encoder = CreateEncoderMock();
            var regex = new Regex("[A-Za-z]+", RegexOptions.Compiled);
            var entry = new SubjectMapEntry(
                "Inbox\\Reports",
                "Status update",
                2,
                new List<string>()
            );
            entry.FolderEncoded = null;
            entry.SubjectEncoded = null;
            entry.FolderWordLengths = new[] { 7 };
            entry.SubjectWordLengths = new[] { 6, 6 };

            // Act
            entry.Encode(encoder.Object, regex);
            entry.Encode(encoder.Object);
            var encodedTokens = entry.Encode(encoder.Object, new[] { "alpha", "beta" });
            // null! deliberately passed to exercise the method's null-handling guard clause; the
            // null-forgiving operator documents the intentional null without changing the
            // runtime value (no behavior change per AC7).
            Action nullTokenEncodingAct = () => entry.Encode(encoder.Object, tokens: null!);

            // Assert
            entry.FolderEncoded.Should().Equal(31);
            entry.SubjectEncoded.Should().Equal(21, 22);
            encodedTokens.Should().Equal(41, 42);
            nullTokenEncodingAct.Should().Throw<ArgumentNullException>();
            entry.Folderpath.Should().Be("Inbox\\Reports");
            entry.Foldername.Should().Be("Reports");
        }

        [TestMethod]
        public void ReflectionHelpers_CoverInternalNullRepairAndValidationBranches()
        {
            // Arrange
            var encoder = CreateEncoderMock();
            var entry = new SubjectMapEntry(
                "Inbox\\Reports",
                "Status update",
                2,
                new List<string>()
            );
            entry.Encoder = encoder.Object;
            entry.FolderWordLengths = Array.Empty<int>();

            // Act
            var validateResult = entry.Validate();
            var readyWithoutEncoder = (bool)InvokeInstance(entry, "ReadyToEncode", false);
            var nullTokensReady = entry.ReadyToEncode((string[]?)null!, false);
            var nullStringEncoding = typeof(SubjectMapEntry)
                .GetMethod(
                    "Encode",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(ISubjectMapEncoder), typeof(string) },
                    modifiers: null
                )!
                .Invoke(entry, new object?[] { encoder.Object, null });
            entry.LogObjectState();

            SetPrivateField(entry, "_folderPath", null);
            var repairResult = (bool)InvokeInstance(entry, "TryRepair", false);

            // Assert
            validateResult.Should().BeTrue();
            readyWithoutEncoder.Should().BeTrue();
            nullTokensReady.Should().BeFalse();
            nullStringEncoding.Should().BeNull();
            repairResult.Should().BeFalse();
        }

        [TestMethod]
        public void ReflectionHelpers_WhenRequiredStateIsMissing_ThrowExpectedExceptions()
        {
            // Arrange
            var entry = new SubjectMapEntry();

            // Act
            Action readyAct = () => entry.ReadyToEncode(throwEx: true);
            Action tokensAct = () => InvokeInstance(entry, "TokensToEncode", true);
            Action isNullAct = () => InvokeInstance(entry, "IsNull", null, "value", true);

            // Assert
            readyAct.Should().Throw<ArgumentNullException>();
            tokensAct
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>();
            isNullAct
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>();
        }

        private static Mock<ISubjectMapEncoder> CreateEncoderMock()
        {
            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            encoder.Setup(mock => mock.AugmentTokenDict(It.IsAny<string[]>())).Verifiable();
            encoder
                .Setup(mock =>
                    mock.Encode(It.Is<string[]>(tokens => TokensEqual(tokens, "action", "items")))
                )
                .Returns(new[] { 11, 12 });
            encoder
                .Setup(mock =>
                    mock.Encode(It.Is<string[]>(tokens => TokensEqual(tokens, "status", "update")))
                )
                .Returns(new[] { 21, 22 });
            encoder
                .Setup(mock =>
                    mock.Encode(It.Is<string[]>(tokens => TokensEqual(tokens, "reports")))
                )
                .Returns(new[] { 31 });
            encoder
                .Setup(mock =>
                    mock.Encode(It.Is<string[]>(tokens => TokensEqual(tokens, "alpha", "beta")))
                )
                .Returns(new[] { 41, 42 });
            return encoder;
        }

        private static bool TokensEqual(string[] actual, params string[] expected)
        {
            return actual
                .Select(token => token.ToLowerInvariant())
                .SequenceEqual(expected.Select(token => token.ToLowerInvariant()));
        }

        private static object InvokeInstance(
            object instance,
            string methodName,
            params object?[] args
        )
        {
            var method = instance
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method =>
                    method.Name == methodName
                    && method.GetParameters().Length == args.Length
                    && method
                        .GetParameters()
                        .Select(parameter => parameter.ParameterType)
                        .Zip(
                            args,
                            (parameterType, argument) =>
                                argument is null || parameterType.IsInstanceOfType(argument)
                        )
                        .All(matches => matches)
                );
            return method.Invoke(instance, args);
        }

        private static void SetPrivateField(object instance, string fieldName, object? value)
        {
            instance
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(instance, value);
        }
    }
}
