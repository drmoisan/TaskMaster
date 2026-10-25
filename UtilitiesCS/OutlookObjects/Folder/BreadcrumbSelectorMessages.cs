#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>Selector page presentation mode.</summary>
    public enum BreadcrumbSelectorViewMode
    {
        Collapsed,
        Expanded,
    }

    /// <summary>Keys handled by the selector rather than breadcrumb navigation.</summary>
    public enum BreadcrumbSelectorKey
    {
        Up,
        Down,
        Enter,
        Escape,

        /// <summary>Ctrl+Left: move to the parent folder (#1007).</summary>
        Parent,
        Home,
        End,
        PageUp,
        PageDown,
    }

    /// <summary>Base type for issue #400 selector messages.</summary>
    public abstract class BreadcrumbSelectorMessage
    {
        public abstract string Type { get; }
    }

    /// <summary>Host-to-page selector view state.</summary>
    public sealed class BreadcrumbSelectorViewMessage : BreadcrumbSelectorMessage
    {
        public BreadcrumbSelectorViewMessage(
            BreadcrumbSelectorViewMode mode,
            bool isOpen,
            string? committedIdentity,
            string? pendingIdentity
        )
            : this(mode, isOpen, committedIdentity, pendingIdentity, null) { }

        // An overload rather than an optional parameter, so reflective construction with the
        // original four arguments still binds (#1007).
        public BreadcrumbSelectorViewMessage(
            BreadcrumbSelectorViewMode mode,
            bool isOpen,
            string? committedIdentity,
            string? pendingIdentity,
            string? pendingNodeIdentity
        )
        {
            Mode = mode;
            IsOpen = isOpen;
            CommittedIdentity = OptionalIdentity(committedIdentity);
            PendingIdentity = OptionalIdentity(pendingIdentity);
            PendingNodeIdentity = OptionalIdentity(pendingNodeIdentity);
        }

        public override string Type => "selectorView";
        public BreadcrumbSelectorViewMode Mode { get; }
        public bool IsOpen { get; }
        public string? CommittedIdentity { get; }
        public string? PendingIdentity { get; }

        /// <summary>Pending descendant node identity on the pending row (#1007), or null.</summary>
        public string? PendingNodeIdentity { get; }

        private static string? OptionalIdentity(string? identity)
        {
            if (identity != null && string.IsNullOrWhiteSpace(identity))
            {
                throw new ArgumentException("A stable identity cannot be blank.", nameof(identity));
            }
            return identity;
        }
    }

    /// <summary>Page-to-host request to toggle the native drop-down.</summary>
    public sealed class BreadcrumbSelectorToggleMessage : BreadcrumbSelectorMessage
    {
        public override string Type => "selectorToggle";
    }

    /// <summary>Page-to-host selector key.</summary>
    public sealed class BreadcrumbSelectorKeyMessage : BreadcrumbSelectorMessage
    {
        public BreadcrumbSelectorKeyMessage(BreadcrumbSelectorKey key)
            : this(key, null) { }

        public BreadcrumbSelectorKeyMessage(BreadcrumbSelectorKey key, int? pageSize)
        {
            Key = key;
            PageSize = pageSize;
        }

        public override string Type => "selectorKey";
        public BreadcrumbSelectorKey Key { get; }

        /// <summary>Fully visible row count for PageUp and PageDown (#1007), or null.</summary>
        public int? PageSize { get; }
    }

    /// <summary>Page-to-host double-click on a breadcrumb segment of a row (#1007, S5).</summary>
    public sealed class BreadcrumbSelectorSegmentDoubleClickMessage : BreadcrumbSelectorMessage
    {
        /// <exception cref="ArgumentException"><paramref name="rowIdentity"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="segmentIndex"/> is negative.</exception>
        public BreadcrumbSelectorSegmentDoubleClickMessage(string rowIdentity, int segmentIndex)
        {
            if (string.IsNullOrWhiteSpace(rowIdentity))
            {
                throw new ArgumentException("A row identity is required.", nameof(rowIdentity));
            }
            if (segmentIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentIndex), segmentIndex, null);
            }
            RowIdentity = rowIdentity;
            SegmentIndex = segmentIndex;
        }

        public override string Type => "selectorSegmentDoubleClick";
        public string RowIdentity { get; }
        public int SegmentIndex { get; }
    }

    /// <summary>Page-to-host move of the open-list pending highlight to a row (#1007, AC9).</summary>
    public sealed class BreadcrumbSelectorHighlightMessage : BreadcrumbSelectorMessage
    {
        /// <exception cref="ArgumentException"><paramref name="identity"/> is blank.</exception>
        public BreadcrumbSelectorHighlightMessage(string identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
            {
                throw new ArgumentException("A row identity is required.", nameof(identity));
            }
            Identity = identity;
        }

        public override string Type => "selectorHighlight";
        public string Identity { get; }
    }

    /// <summary>Page-to-host activation of a row by stable identity.</summary>
    public sealed class BreadcrumbSelectorActivationMessage : BreadcrumbSelectorMessage
    {
        public BreadcrumbSelectorActivationMessage(string identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
            {
                throw new ArgumentException(
                    "A non-empty stable identity is required.",
                    nameof(identity)
                );
            }
            Identity = identity;
        }

        public override string Type => "selectorActivate";
        public string Identity { get; }
    }

    /// <summary>Page-to-host activation of an expanded subfolder by stable row identity.</summary>
    public sealed class BreadcrumbSelectorSubfolderActivationMessage : BreadcrumbSelectorMessage
    {
        /// <summary>Creates an explicit selector subfolder activation.</summary>
        /// <param name="rowIdentity">The unique stable identity of the containing row.</param>
        /// <param name="subfolderIndex">The zero-based index of the expanded subfolder.</param>
        /// <exception cref="ArgumentException"><paramref name="rowIdentity"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="subfolderIndex"/> is negative.
        /// </exception>
        public BreadcrumbSelectorSubfolderActivationMessage(string rowIdentity, int subfolderIndex)
        {
            if (string.IsNullOrWhiteSpace(rowIdentity))
            {
                throw new ArgumentException(
                    "A non-empty stable row identity is required.",
                    nameof(rowIdentity)
                );
            }
            if (subfolderIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(subfolderIndex),
                    subfolderIndex,
                    "A subfolder index cannot be negative."
                );
            }

            RowIdentity = rowIdentity;
            SubfolderIndex = subfolderIndex;
        }

        /// <summary>The selector message discriminator.</summary>
        public override string Type => "selectorSubfolderActivate";

        /// <summary>The unique stable identity of the containing row.</summary>
        public string RowIdentity { get; }

        /// <summary>The zero-based index of the activated expanded subfolder.</summary>
        public int SubfolderIndex { get; }
    }

    /// <summary>Page-to-host address of a known descendant occurrence.</summary>
    public sealed class BreadcrumbSelectorDescendantMessage : BreadcrumbSelectorMessage
    {
        public BreadcrumbSelectorDescendantMessage(
            string rowIdentity,
            string nodeIdentity,
            bool toggle = false
        )
        {
            if (string.IsNullOrWhiteSpace(rowIdentity))
            {
                throw new ArgumentException("A row identity is required.", nameof(rowIdentity));
            }
            if (string.IsNullOrWhiteSpace(nodeIdentity))
            {
                throw new ArgumentException("A node identity is required.", nameof(nodeIdentity));
            }
            RowIdentity = rowIdentity;
            NodeIdentity = nodeIdentity;
            Toggle = toggle;
        }

        public override string Type =>
            Toggle ? "selectorDescendantToggle" : "selectorDescendantActivate";
        public string RowIdentity { get; }
        public string NodeIdentity { get; }
        public bool Toggle { get; }
    }

    /// <summary>Strict JSON serializer for the focused selector message family.</summary>
    public static class BreadcrumbSelectorMessageSerializer
    {
        public static BreadcrumbSelectorMessage Parse(string json)
        {
            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (Exception ex) when (ex is JsonReaderException || ex is ArgumentNullException)
            {
                throw new FormatException($"Malformed selector message JSON: {ex.Message}");
            }

            string type = RequireString(root, "type");
            switch (type)
            {
                case "selectorView":
                    return new BreadcrumbSelectorViewMessage(
                        ParseMode(RequireString(root, "mode")),
                        root.Value<bool?>("isOpen")
                            ?? throw new FormatException(
                                "Selector message is missing the 'isOpen' field."
                            ),
                        OptionalString(root, "committedIdentity"),
                        OptionalString(root, "pendingIdentity"),
                        OptionalString(root, "pendingNodeIdentity")
                    );
                case "selectorToggle":
                    return new BreadcrumbSelectorToggleMessage();
                case "selectorKey":
                    return new BreadcrumbSelectorKeyMessage(
                        ParseKey(RequireString(root, "key")),
                        OptionalNonNegativeInt(root, "pageSize")
                    );
                case "selectorActivate":
                    return new BreadcrumbSelectorActivationMessage(
                        RequireIdentity(root, "identity")
                    );
                case "selectorSubfolderActivate":
                    return new BreadcrumbSelectorSubfolderActivationMessage(
                        RequireIdentity(root, "rowIdentity"),
                        RequireNonNegativeInt(root, "subfolderIndex")
                    );
                case "selectorSegmentDoubleClick":
                    return new BreadcrumbSelectorSegmentDoubleClickMessage(
                        RequireIdentity(root, "rowIdentity"),
                        RequireNonNegativeInt(root, "segmentIndex")
                    );
                case "selectorHighlight":
                    return new BreadcrumbSelectorHighlightMessage(
                        RequireIdentity(root, "identity")
                    );
                case "selectorDescendantActivate":
                case "selectorDescendantToggle":
                    return new BreadcrumbSelectorDescendantMessage(
                        RequireIdentity(root, "rowIdentity"),
                        RequireIdentity(root, "nodeIdentity"),
                        type == "selectorDescendantToggle"
                    );
                default:
                    throw new FormatException($"Unknown selector message type '{type}'.");
            }
        }

        public static string Serialize(BreadcrumbSelectorMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var root = new JObject { ["type"] = message.Type };
            switch (message)
            {
                case BreadcrumbSelectorViewMessage view:
                    root["mode"] =
                        view.Mode == BreadcrumbSelectorViewMode.Collapsed
                            ? "collapsed"
                            : "expanded";
                    root["isOpen"] = view.IsOpen;
                    AddOptional(root, "committedIdentity", view.CommittedIdentity);
                    AddOptional(root, "pendingIdentity", view.PendingIdentity);
                    AddOptional(root, "pendingNodeIdentity", view.PendingNodeIdentity);
                    break;
                case BreadcrumbSelectorToggleMessage _:
                    break;
                case BreadcrumbSelectorKeyMessage key:
                    root["key"] = key.Key.ToString().ToLowerInvariant();
                    if (key.PageSize.HasValue)
                    {
                        root["pageSize"] = key.PageSize.Value;
                    }
                    break;
                case BreadcrumbSelectorActivationMessage activation:
                    root["identity"] = activation.Identity;
                    break;
                case BreadcrumbSelectorSubfolderActivationMessage subfolderActivation:
                    root["rowIdentity"] = subfolderActivation.RowIdentity;
                    root["subfolderIndex"] = subfolderActivation.SubfolderIndex;
                    break;
                case BreadcrumbSelectorSegmentDoubleClickMessage doubleClick:
                    root["rowIdentity"] = doubleClick.RowIdentity;
                    root["segmentIndex"] = doubleClick.SegmentIndex;
                    break;
                case BreadcrumbSelectorHighlightMessage highlight:
                    root["identity"] = highlight.Identity;
                    break;
                case BreadcrumbSelectorDescendantMessage descendant:
                    root["rowIdentity"] = descendant.RowIdentity;
                    root["nodeIdentity"] = descendant.NodeIdentity;
                    break;
                default:
                    throw new FormatException(
                        $"No selector serialization is defined for '{message.Type}'."
                    );
            }
            return root.ToString(Formatting.None);
        }

        private static BreadcrumbSelectorViewMode ParseMode(string mode)
        {
            switch (mode)
            {
                case "collapsed":
                    return BreadcrumbSelectorViewMode.Collapsed;
                case "expanded":
                    return BreadcrumbSelectorViewMode.Expanded;
                default:
                    throw new FormatException($"Unknown selector view mode '{mode}'.");
            }
        }

        // The wire value of every key is its lowercase enum name ("pageup", "parent", ...), the
        // same spelling Serialize writes, so parsing and serialization cannot drift (#1007).
        private static BreadcrumbSelectorKey ParseKey(string key)
        {
            foreach (
                BreadcrumbSelectorKey candidate in Enum.GetValues(typeof(BreadcrumbSelectorKey))
            )
            {
                if (
                    string.Equals(
                        candidate.ToString().ToLowerInvariant(),
                        key,
                        StringComparison.Ordinal
                    )
                )
                {
                    return candidate;
                }
            }
            throw new FormatException($"Unknown selector key '{key}'.");
        }

        private static int? OptionalNonNegativeInt(JObject root, string name) =>
            root[name] == null || root[name]!.Type == JTokenType.Null
                ? (int?)null
                : RequireNonNegativeInt(root, name);

        private static string RequireIdentity(JObject root, string name)
        {
            string identity = RequireString(root, name);
            if (string.IsNullOrWhiteSpace(identity))
            {
                throw new FormatException($"Selector '{name}' identity must not be blank.");
            }
            return identity;
        }

        private static int RequireNonNegativeInt(JObject root, string name)
        {
            JToken? token = root[name];
            if (token == null || token.Type == JTokenType.Null)
            {
                throw new FormatException($"Selector message is missing the '{name}' field.");
            }
            if (token.Type != JTokenType.Integer)
            {
                throw new FormatException($"Selector '{name}' must be a JSON integer.");
            }

            int value;
            try
            {
                value = token.Value<int>();
            }
            catch (Exception ex)
                when (ex is OverflowException || ex is InvalidCastException || ex is FormatException
                )
            {
                throw new FormatException(
                    $"Selector '{name}' must be an in-range 32-bit integer.",
                    ex
                );
            }
            if (value < 0)
            {
                throw new FormatException($"Selector '{name}' must not be negative.");
            }
            return value;
        }

        private static string RequireString(JObject root, string name)
        {
            JToken? token = root[name];
            if (token == null || token.Type == JTokenType.Null)
            {
                throw new FormatException($"Selector message is missing the '{name}' field.");
            }
            if (token.Type != JTokenType.String)
            {
                throw new FormatException($"Selector '{name}' must be a JSON string.");
            }
            return token.Value<string>()!;
        }

        private static string? OptionalString(JObject root, string name)
        {
            string? value = root.Value<string>(name);
            if (value != null && string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException($"Selector '{name}' identity must not be blank.");
            }
            return value;
        }

        private static void AddOptional(JObject root, string name, string? value)
        {
            if (value != null)
            {
                root[name] = value;
            }
        }
    }
}
