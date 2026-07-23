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
        {
            Mode = mode;
            IsOpen = isOpen;
            CommittedIdentity = OptionalIdentity(committedIdentity);
            PendingIdentity = OptionalIdentity(pendingIdentity);
        }

        public override string Type => "selectorView";
        public BreadcrumbSelectorViewMode Mode { get; }
        public bool IsOpen { get; }
        public string? CommittedIdentity { get; }
        public string? PendingIdentity { get; }

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
        {
            Key = key;
        }

        public override string Type => "selectorKey";
        public BreadcrumbSelectorKey Key { get; }
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
                        OptionalString(root, "pendingIdentity")
                    );
                case "selectorToggle":
                    return new BreadcrumbSelectorToggleMessage();
                case "selectorKey":
                    return new BreadcrumbSelectorKeyMessage(ParseKey(RequireString(root, "key")));
                case "selectorActivate":
                    return new BreadcrumbSelectorActivationMessage(
                        RequireIdentity(root, "identity")
                    );
                case "selectorSubfolderActivate":
                    return new BreadcrumbSelectorSubfolderActivationMessage(
                        RequireIdentity(root, "rowIdentity"),
                        RequireNonNegativeInt(root, "subfolderIndex")
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
                    break;
                case BreadcrumbSelectorToggleMessage _:
                    break;
                case BreadcrumbSelectorKeyMessage key:
                    root["key"] = key.Key.ToString().ToLowerInvariant();
                    break;
                case BreadcrumbSelectorActivationMessage activation:
                    root["identity"] = activation.Identity;
                    break;
                case BreadcrumbSelectorSubfolderActivationMessage subfolderActivation:
                    root["rowIdentity"] = subfolderActivation.RowIdentity;
                    root["subfolderIndex"] = subfolderActivation.SubfolderIndex;
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

        private static BreadcrumbSelectorKey ParseKey(string key)
        {
            switch (key)
            {
                case "up":
                    return BreadcrumbSelectorKey.Up;
                case "down":
                    return BreadcrumbSelectorKey.Down;
                case "enter":
                    return BreadcrumbSelectorKey.Enter;
                case "escape":
                    return BreadcrumbSelectorKey.Escape;
                default:
                    throw new FormatException($"Unknown selector key '{key}'.");
            }
        }

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
