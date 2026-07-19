#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UtilitiesCS
{
    /// <summary>
    /// Host-neutral, pure logic that resolves inline <c>cid:</c> (Content-Id) image references in an
    /// HTML email body against a mail item's attachments, rewriting matched references to a fetchable
    /// virtual-host URL so a <c>CoreWebView2.WebResourceRequested</c> handler can serve the attachment
    /// bytes at render time. This class performs no I/O and has no COM/WebView2 dependency.
    /// </summary>
    public static class CidImageResolver
    {
        /// <summary>
        /// The fixed virtual host used to make otherwise-unfetchable <c>cid:</c> references
        /// resolvable by Chromium's WebView2 <c>WebResourceRequested</c> interception.
        /// </summary>
        public const string DefaultVirtualHost = "cid.quickfiler.local";

        private static readonly Regex CidImageSourcePattern = new(
            @"src=(['""])cid:([^'""]+)\1",
            RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Builds a case-insensitive <c>Content-Id</c> -> <see cref="IAttachment"/> lookup map from the
        /// supplied attachments, excluding any attachment whose <see cref="IAttachment.ContentId"/> is
        /// null or empty.
        /// </summary>
        /// <param name="attachments">The mail item's attachments to index.</param>
        /// <returns>A case-insensitive, read-only map keyed by <c>Content-Id</c>.</returns>
        public static IReadOnlyDictionary<string, IAttachment> BuildContentIdMap(
            IReadOnlyCollection<IAttachment> attachments
        )
        {
            var map = new Dictionary<string, IAttachment>(StringComparer.OrdinalIgnoreCase);
            if (attachments is null)
            {
                return map;
            }

            foreach (var attachment in attachments)
            {
                if (!string.IsNullOrEmpty(attachment?.ContentId))
                {
                    // The IsNullOrEmpty guard above returns false only when attachment and its
                    // ContentId are both non-null, so the dereference here is provably safe.
                    map[attachment!.ContentId] = attachment;
                }
            }

            return map;
        }

        /// <summary>
        /// Rewrites <c>src="cid:&lt;id&gt;"</c> references in <paramref name="html"/> to
        /// <c>src="https://&lt;virtualHost&gt;/&lt;url-encoded id&gt;"</c> whenever a supplied
        /// attachment's <see cref="IAttachment.ContentId"/> matches (case-insensitive). Unmatched
        /// <c>cid:</c> references are left unchanged in the output.
        /// </summary>
        /// <param name="html">The raw HTML body to rewrite.</param>
        /// <param name="attachments">The mail item's attachments to resolve references against.</param>
        /// <param name="virtualHost">The virtual host to rewrite matched references to.</param>
        /// <returns>The HTML body with matched <c>cid:</c> references rewritten.</returns>
        public static string RewriteCidReferences(
            string html,
            IReadOnlyCollection<IAttachment> attachments,
            string virtualHost
        )
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var contentIdMap = BuildContentIdMap(attachments);

            return CidImageSourcePattern.Replace(
                html,
                match =>
                {
                    var quote = match.Groups[1].Value;
                    var id = match.Groups[2].Value;
                    if (!contentIdMap.TryGetValue(id, out _))
                    {
                        return match.Value;
                    }

                    return $"src={quote}https://{virtualHost}/{Uri.EscapeDataString(id)}{quote}";
                }
            );
        }
    }
}
