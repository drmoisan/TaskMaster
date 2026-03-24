#nullable enable

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
    }
}
