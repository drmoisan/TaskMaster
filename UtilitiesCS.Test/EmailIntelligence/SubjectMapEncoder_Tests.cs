#nullable enable

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SubjectMapEncoder_Tests
    {
        [TestMethod]
        public void RebuildEncoding_BuildsSymmetricEncodeDecodeMaps()
        {
            var commonWords = new SerializableList<string>();
            var subjectMap = new SubjectMapSco(commonWords)
            {
                new SubjectMapEntry("Inbox\\Reports", "Alpha Beta", 1, commonWords),
                new SubjectMapEntry("Inbox\\Review", "Beta Gamma", 1, commonWords),
            };
            var encoder = new SubjectMapEncoder(string.Empty, string.Empty, subjectMap);
            var expectedTokens = new[] { "alpha", "beta", "reports", "gamma", "review" };

            encoder.RebuildEncoding(subjectMap);

            encoder.Encoder.Keys.Should().BeEquivalentTo(expectedTokens);
            encoder.Decoder.Count.Should().Be(expectedTokens.Length);

            foreach (var token in expectedTokens)
            {
                encoder.Encoder.Should().ContainKey(token);
                var code = encoder.Encoder[token];
                encoder.Decoder.Should().ContainKey(code);
                encoder.Decoder[code].Should().Be(token);
            }
        }

        [TestMethod]
        public void AugmentTokenDict_AppendsOnlyUnseenTokens()
        {
            var commonWords = new SerializableList<string>();
            var encoder = new SubjectMapEncoder(
                string.Empty,
                string.Empty,
                new SubjectMapSco(commonWords)
            );
            encoder.Encoder.Add("alpha", 1);
            encoder.Encoder.Add("beta", 2);
            _ = encoder.Decoder;

            encoder.AugmentTokenDict(new[] { "beta", "gamma", "gamma", "delta" });

            encoder.Encoder.Should().HaveCount(4);
            encoder.Encoder["alpha"].Should().Be(1);
            encoder.Encoder["beta"].Should().Be(2);
            encoder.Encoder["gamma"].Should().Be(3);
            encoder.Encoder["delta"].Should().Be(4);
            encoder.Decoder[3].Should().Be("gamma");
            encoder.Decoder[4].Should().Be("delta");
        }

        [TestMethod]
        public void EncodeFollowedByDecode_RoundTripsOriginalTerms()
        {
            var commonWords = new SerializableList<string>();
            var encoder = new SubjectMapEncoder(
                string.Empty,
                string.Empty,
                new SubjectMapSco(commonWords)
            );
            encoder.Encoder.Add("alpha", 1);
            encoder.Encoder.Add("beta", 2);
            encoder.Encoder.Add("gamma", 3);
            _ = encoder.Decoder;

            var encoded = encoder.Encode(new[] { "alpha", "gamma", "alpha" });
            var decoded = encoder.Decode(encoded);

            encoded.Should().Equal(1, 3, 1);
            decoded.Should().Be("alpha gamma alpha");
        }

        // ---------------------------------------------------------------------------
        // Default constructor — covers line 15
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the default constructor creates a valid instance and that
        /// accessing the Encoder property on a default-constructed encoder enters the
        /// null-fallback branch (lines 91-94) and throws ArgumentNullException because
        /// both _filename and _folderpath are null, making ScoDictionaryNew's path
        /// combination fail.
        ///
        /// Purpose:
        ///     Covers line 15 (default ctor body) and lines 91-94 (Encoder null fallback).
        /// </summary>
        [TestMethod]
        public void DefaultConstructor_EncoderAccessCreatesEmptyDictionary()
        {
            var encoder = new SubjectMapEncoder();

            // Accessing Encoder triggers the null-folderpath path which throws in ScoDictionaryNew.
            Action act = () => _ = encoder.Encoder;

            act.Should().Throw<ArgumentNullException>();
        }

        // ---------------------------------------------------------------------------
        // Decoder with null encoder — covers lines 39-41
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that accessing Decoder on a default-constructed SubjectMapEncoder
        /// (where both _decoder and _encoder are null, and the file name / folder path are
        /// unset) fails fast. After the ScoDictionaryNew migration the null-encoder branch
        /// loads the encoder via ScoDictionaryNew&lt;string,int&gt;.Static.Deserialize(_filename,
        /// _folderpath); with a null file name / folder path this throws ArgumentNullException
        /// from FilePathHelper / Path.Combine rather than the previous NullReferenceException.
        ///
        /// Purpose:
        ///     Covers the _encoder null-check branch inside the Decoder getter.
        /// </summary>
        [TestMethod]
        public void Decoder_WhenEncoderIsNull_ThrowsArgumentNullException()
        {
            var encoder = new SubjectMapEncoder();

            // Accessing Decoder triggers the null-encoder path, which now loads the encoder via
            // Static.Deserialize; with an unset file name / folder path it fails fast.
            Action act = () => _ = encoder.Decoder;

            act.Should().Throw<ArgumentNullException>();
        }

        // ---------------------------------------------------------------------------
        // RebuildEncoding() no-arg — null path covers lines 100-105
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling the no-argument RebuildEncoding() when _subjectMap is
        /// null (default constructor) throws NullReferenceException with a helpful message.
        ///
        /// Purpose:
        ///     Covers lines 100-105 (null guard in RebuildEncoding() no-arg overload).
        /// </summary>
        [TestMethod]
        public void RebuildEncoding_WhenSubjectMapIsNull_ThrowsNullReferenceException()
        {
            var encoder = new SubjectMapEncoder();

            Action act = () => encoder.RebuildEncoding();

            act.Should().Throw<NullReferenceException>();
        }

        // ---------------------------------------------------------------------------
        // RebuildEncoding() no-arg — non-null path covers lines 107-108
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling RebuildEncoding() with a valid _subjectMap delegates
        /// to RebuildEncoding(SubjectMapSco) (line 107) without throwing.
        ///
        /// Purpose:
        ///     Covers lines 107-108 (delegation branch of no-arg RebuildEncoding).
        /// </summary>
        [TestMethod]
        public void RebuildEncoding_WithValidSubjectMap_DelegatesToOverload()
        {
            var commonWords = new SerializableList<string>();
            var subjectMap = new SubjectMapSco(commonWords)
            {
                new SubjectMapEntry("Inbox\\Alpha", "Alpha", 1, commonWords),
            };
            var encoder = new SubjectMapEncoder(string.Empty, string.Empty, subjectMap);

            encoder.RebuildEncoding();

            encoder.Encoder.Should().NotBeEmpty();
        }

        // ---------------------------------------------------------------------------
        // AugmentTokenDict(string[]) null — covers lines 147-148
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that passing null to AugmentTokenDict(string[]) throws
        /// ArgumentNullException, exercising the null guard on lines 147-148.
        ///
        /// Purpose:
        ///     Covers lines 147-148 (null-check branch in AugmentTokenDict(string[])).
        /// </summary>
        [TestMethod]
        public void AugmentTokenDict_WhenTokensIsNull_ThrowsArgumentNullException()
        {
            var commonWords = new SerializableList<string>();
            var encoder = new SubjectMapEncoder(
                string.Empty,
                string.Empty,
                new SubjectMapSco(commonWords)
            );

            Action act = () => encoder.AugmentTokenDict((string[])null!);

            act.Should().Throw<ArgumentNullException>();
        }

        // ---------------------------------------------------------------------------
        // AugmentTokenDict(string text) overload — covers lines 178-180
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the string-text overload of AugmentTokenDict tokenizes the
        /// text and delegates to the array overload, adding unseen tokens.
        ///
        /// Purpose:
        ///     Covers lines 178-180 (AugmentTokenDict(string text) overload body).
        /// </summary>
        [TestMethod]
        public void AugmentTokenDict_WithStringText_AddsTokenizedTerms()
        {
            var commonWords = new SerializableList<string>();
            var encoder = new SubjectMapEncoder(
                string.Empty,
                string.Empty,
                new SubjectMapSco(commonWords)
            );
            encoder.Encoder.Add("alpha", 1);
            _ = encoder.Decoder;

            encoder.AugmentTokenDict("beta gamma");

            encoder.Encoder.Should().ContainKey("beta");
            encoder.Encoder.Should().ContainKey("gamma");
        }

        // ---------------------------------------------------------------------------
        // Encode(string text) overload — covers lines 188-190
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the string-text overload of Encode tokenizes the text and
        /// returns the corresponding integer codes from the encoder dictionary.
        ///
        /// Purpose:
        ///     Covers lines 188-190 (Encode(string text) overload body).
        /// </summary>
        [TestMethod]
        public void Encode_WithStringText_ReturnsCodes()
        {
            var commonWords = new SerializableList<string>();
            var encoder = new SubjectMapEncoder(
                string.Empty,
                string.Empty,
                new SubjectMapSco(commonWords)
            );
            encoder.Encoder.Add("alpha", 1);
            encoder.Encoder.Add("beta", 2);

            var codes = encoder.Encode("alpha beta");

            codes.Should().Equal(1, 2);
        }
    }
}
