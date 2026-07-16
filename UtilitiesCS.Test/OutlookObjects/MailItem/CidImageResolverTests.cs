using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.EmailParsing;

namespace UtilitiesCS.Test.OutlookObjects.MailItem
{
    /// <summary>
    /// Regression tests for <see cref="CidImageResolver"/>, the host-neutral logic that rewrites
    /// <c>cid:</c> image references in HTML email bodies to fetchable virtual-host URLs, and builds
    /// the supporting <c>Content-Id</c> -> <see cref="IAttachment"/> lookup map.
    /// </summary>
    [TestClass]
    public class CidImageResolverTests
    {
        [TestMethod]
        public void RewriteCidReferences_ShouldRewriteMatchedContentId()
        {
            // Arrange: HTML with a single cid: reference and an attachment whose ContentId matches it.
            const string html = "<html><body><img src=\"cid:logo1\"></body></html>";
            var attachment = new AttachmentSerializable
            {
                ContentId = "logo1",
                AttachmentData = new byte[] { 1, 2, 3 },
            };

            // Act
            var result = CidImageResolver.RewriteCidReferences(
                html,
                new IAttachment[] { attachment },
                "cid.quickfiler.local"
            );

            // Assert: the matched cid: reference is rewritten to the virtual-host URL.
            result.Should().Contain("src=\"https://cid.quickfiler.local/logo1\"");
            result.Should().NotContain("cid:logo1");
        }

        [TestMethod]
        public void RewriteCidReferences_ShouldLeaveUnmatchedContentIdUnchanged()
        {
            // Arrange: HTML references a cid: id that no attachment carries.
            const string html = "<html><body><img src=\"cid:unknown\"></body></html>";
            var attachment = new AttachmentSerializable
            {
                ContentId = "logo1",
                AttachmentData = new byte[] { 1, 2, 3 },
            };

            // Act
            var result = CidImageResolver.RewriteCidReferences(
                html,
                new IAttachment[] { attachment },
                "cid.quickfiler.local"
            );

            // Assert: the unmatched reference is left untouched by design.
            result.Should().Contain("cid:unknown");
        }

        [TestMethod]
        public void BuildContentIdMap_ShouldReturnCaseInsensitiveMapExcludingEmptyContentId()
        {
            // Arrange: a mix of populated (mixed-case), empty, and null ContentId values.
            var populated = new AttachmentSerializable { ContentId = "LOGO1" };
            var empty = new AttachmentSerializable { ContentId = "" };
            var nullContentId = new AttachmentSerializable { ContentId = null };

            // Act
            var map = CidImageResolver.BuildContentIdMap(
                new IAttachment[] { populated, empty, nullContentId }
            );

            // Assert: exactly one entry, keyed case-insensitively, excluding the empty/null entries.
            map.Should().HaveCount(1);
            map.Should().ContainKey("logo1");
            map["logo1"].Should().BeSameAs(populated);
        }
    }
}
