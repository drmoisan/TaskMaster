#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>Arrow-key direction carried by <see cref="ArrowKeyMessage"/>/<see cref="UnhandledArrowMessage"/>.</summary>
    public enum BreadcrumbArrowDirection
    {
        /// <summary>The Left arrow key.</summary>
        Left,

        /// <summary>The Right arrow key.</summary>
        Right,
    }

    /// <summary>
    /// Base of the typed JS&lt;-&gt;.NET breadcrumb bridge protocol (#351 FR-6). Each concrete message
    /// maps 1:1 to one JSON message family handled by <see cref="BreadcrumbBridgeSerializer"/>.
    /// </summary>
    public abstract class BreadcrumbBridgeMessage
    {
        /// <summary>The wire `type` discriminator of this message family.</summary>
        public abstract string Type { get; }
    }

    /// <summary>JS -&gt; .NET: a non-leaf segment was double-clicked (collapse-after, FR-3).</summary>
    public sealed class SegmentDoubleClickMessage : BreadcrumbBridgeMessage
    {
        public SegmentDoubleClickMessage(int rowIndex, int segmentIndex)
        {
            RowIndex = rowIndex;
            SegmentIndex = segmentIndex;
        }

        public override string Type => "segmentDoubleClick";
        public int RowIndex { get; }
        public int SegmentIndex { get; }
    }

    /// <summary>JS -&gt; .NET: the plus/minus affordance of a row was activated (FR-2/FR-3).</summary>
    public sealed class AffordanceToggleMessage : BreadcrumbBridgeMessage
    {
        public AffordanceToggleMessage(int rowIndex)
        {
            RowIndex = rowIndex;
        }

        public override string Type => "affordanceToggle";
        public int RowIndex { get; }
    }

    /// <summary>JS -&gt; .NET: a Left/Right arrow key was pressed inside the breadcrumb (FR-6).</summary>
    public sealed class ArrowKeyMessage : BreadcrumbBridgeMessage
    {
        public ArrowKeyMessage(BreadcrumbArrowDirection direction)
        {
            Direction = direction;
        }

        public override string Type => "arrowKey";
        public BreadcrumbArrowDirection Direction { get; }
    }

    /// <summary>
    /// Bidirectional selection message: inbound row/subfolder selection from JS, and the outbound
    /// acknowledgement carrying the mapped <see cref="SelectedFolder"/> output string (FR-7).
    /// <see cref="SubfolderIndex"/> is -1 when the row itself is selected.
    /// </summary>
    public sealed class SelectionChangeMessage : BreadcrumbBridgeMessage
    {
        public SelectionChangeMessage(int rowIndex, int subfolderIndex, string? selectedFolder)
        {
            RowIndex = rowIndex;
            SubfolderIndex = subfolderIndex;
            SelectedFolder = selectedFolder;
        }

        public override string Type => "selectionChange";
        public int RowIndex { get; }
        public int SubfolderIndex { get; }
        public string? SelectedFolder { get; }
    }

    /// <summary>JS -&gt; .NET: the immediate subfolders of a row's leaf are requested (FR-4).</summary>
    public sealed class SubfolderRequestMessage : BreadcrumbBridgeMessage
    {
        public SubfolderRequestMessage(int rowIndex)
        {
            RowIndex = rowIndex;
        }

        public override string Type => "subfolderRequest";
        public int RowIndex { get; }
    }

    /// <summary>.NET -&gt; JS: the fetched immediate subfolders of a row's leaf (FR-4).</summary>
    public sealed class SubfolderResponseMessage : BreadcrumbBridgeMessage
    {
        public SubfolderResponseMessage(
            int rowIndex,
            IReadOnlyList<BreadcrumbSubfolderRender> subfolders
        )
        {
            RowIndex = rowIndex;
            Subfolders = subfolders ?? throw new ArgumentNullException(nameof(subfolders));
        }

        public override string Type => "subfolderResponse";
        public int RowIndex { get; }
        public IReadOnlyList<BreadcrumbSubfolderRender> Subfolders { get; }
    }

    /// <summary>Theme switch: the page swaps its CSS custom-property set ("dark"/"light").</summary>
    public sealed class ThemeChangeMessage : BreadcrumbBridgeMessage
    {
        public ThemeChangeMessage(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
            {
                throw new ArgumentException("A non-empty theme name is required.", nameof(theme));
            }
            Theme = theme;
        }

        public override string Type => "themeChange";
        public string Theme { get; }
    }

    /// <summary>.NET -&gt; JS: the full render payload projected by <see cref="BreadcrumbRenderProjection"/>.</summary>
    public sealed class RenderMessage : BreadcrumbBridgeMessage
    {
        public RenderMessage(IReadOnlyList<BreadcrumbRowRender> rows)
        {
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        }

        public override string Type => "render";
        public IReadOnlyList<BreadcrumbRowRender> Rows { get; }
    }

    /// <summary>An arrow the breadcrumb could not consume; triggers the legacy fall-through (FR-6).</summary>
    public sealed class UnhandledArrowMessage : BreadcrumbBridgeMessage
    {
        public UnhandledArrowMessage(BreadcrumbArrowDirection direction)
        {
            Direction = direction;
        }

        public override string Type => "unhandledArrow";
        public BreadcrumbArrowDirection Direction { get; }
    }

    /// <summary>.NET -&gt; JS: an explicit routing/provider error surfaced to the page.</summary>
    public sealed class BridgeErrorMessage : BreadcrumbBridgeMessage
    {
        public BridgeErrorMessage(string message)
        {
            Message = message ?? string.Empty;
        }

        public override string Type => "error";
        public string Message { get; }
    }

    /// <summary>
    /// Newtonsoft.Json-based serializer/parser for the breadcrumb bridge protocol. Parsing fails
    /// explicitly (<see cref="FormatException"/>) on malformed JSON, an unknown message type, or a
    /// missing required field — never a silent null (#351 P3-T5).
    /// </summary>
    public static class BreadcrumbBridgeSerializer
    {
        /// <summary>Parses one wire message into its typed form.</summary>
        /// <exception cref="FormatException">Malformed JSON, unknown type, or missing/invalid field.</exception>
        public static BreadcrumbBridgeMessage Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new FormatException("Bridge message JSON must be a non-empty object.");
            }

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                throw new FormatException($"Malformed bridge message JSON: {ex.Message}");
            }

            string type =
                root.Value<string>("type")
                ?? throw new FormatException("Bridge message is missing the 'type' field.");

            switch (type)
            {
                case "segmentDoubleClick":
                    return new SegmentDoubleClickMessage(
                        RequireInt(root, "rowIndex"),
                        RequireInt(root, "segmentIndex")
                    );
                case "affordanceToggle":
                    return new AffordanceToggleMessage(RequireInt(root, "rowIndex"));
                case "arrowKey":
                    return new ArrowKeyMessage(RequireDirection(root));
                case "selectionChange":
                    return new SelectionChangeMessage(
                        RequireInt(root, "rowIndex"),
                        root.Value<int?>("subfolderIndex") ?? -1,
                        root.Value<string>("selectedFolder")
                    );
                case "subfolderRequest":
                    return new SubfolderRequestMessage(RequireInt(root, "rowIndex"));
                case "subfolderResponse":
                    return new SubfolderResponseMessage(
                        RequireInt(root, "rowIndex"),
                        ParseSubfolders(RequireArray(root, "subfolders"))
                    );
                case "themeChange":
                    return new ThemeChangeMessage(RequireString(root, "theme"));
                case "render":
                    return new RenderMessage(ParseRows(RequireArray(root, "rows")));
                case "unhandledArrow":
                    return new UnhandledArrowMessage(RequireDirection(root));
                case "error":
                    return new BridgeErrorMessage(RequireString(root, "message"));
                default:
                    throw new FormatException($"Unknown bridge message type '{type}'.");
            }
        }

        /// <summary>Serializes one typed message to its wire JSON.</summary>
        public static string Serialize(BreadcrumbBridgeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var root = new JObject { ["type"] = message.Type };
            switch (message)
            {
                case SegmentDoubleClickMessage m:
                    root["rowIndex"] = m.RowIndex;
                    root["segmentIndex"] = m.SegmentIndex;
                    break;
                case AffordanceToggleMessage m:
                    root["rowIndex"] = m.RowIndex;
                    break;
                case ArrowKeyMessage m:
                    root["direction"] = DirectionName(m.Direction);
                    break;
                case SelectionChangeMessage m:
                    root["rowIndex"] = m.RowIndex;
                    root["subfolderIndex"] = m.SubfolderIndex;
                    if (m.SelectedFolder != null)
                    {
                        root["selectedFolder"] = m.SelectedFolder;
                    }
                    break;
                case SubfolderRequestMessage m:
                    root["rowIndex"] = m.RowIndex;
                    break;
                case SubfolderResponseMessage m:
                    root["rowIndex"] = m.RowIndex;
                    root["subfolders"] = SubfoldersToJson(m.Subfolders);
                    break;
                case ThemeChangeMessage m:
                    root["theme"] = m.Theme;
                    break;
                case RenderMessage m:
                    root["rows"] = new JArray(m.Rows.Select(RowToJson));
                    break;
                case UnhandledArrowMessage m:
                    root["direction"] = DirectionName(m.Direction);
                    break;
                case BridgeErrorMessage m:
                    root["message"] = m.Message;
                    break;
                default:
                    throw new FormatException(
                        $"No serialization is defined for message type '{message.Type}'."
                    );
            }
            return root.ToString(Formatting.None);
        }

        private static string DirectionName(BreadcrumbArrowDirection direction) =>
            direction == BreadcrumbArrowDirection.Left ? "left" : "right";

        private static JObject RowToJson(BreadcrumbRowRender row)
        {
            return new JObject
            {
                ["rowIndex"] = row.RowIndex,
                ["isSuggestion"] = row.IsSuggestion,
                ["selected"] = row.Selected,
                ["collapsed"] = row.Collapsed,
                ["leafExpanded"] = row.LeafExpanded,
                ["percentText"] = row.PercentText,
                ["cells"] = new JArray(
                    row.Cells.Select(cell => new JObject
                    {
                        ["kind"] = cell.Kind.ToString().ToLowerInvariant(),
                        ["text"] = cell.Text,
                        ["segmentIndex"] = cell.SegmentIndex,
                        ["truncationEligible"] = cell.TruncationEligible,
                    })
                ),
                ["subfolders"] = SubfoldersToJson(row.Subfolders),
            };
        }

        private static JArray SubfoldersToJson(IReadOnlyList<BreadcrumbSubfolderRender> subfolders)
        {
            return new JArray(
                subfolders.Select(s => new JObject
                {
                    ["displayName"] = s.DisplayName,
                    ["folderPath"] = s.FolderPath,
                    ["hasChildren"] = s.HasChildren,
                })
            );
        }

        private static IReadOnlyList<BreadcrumbRowRender> ParseRows(JArray rows)
        {
            return rows.Select(token =>
                {
                    var row = AsObject(token, "rows[]");
                    return new BreadcrumbRowRender(
                        RequireInt(row, "rowIndex"),
                        RequireBool(row, "isSuggestion"),
                        RequireBool(row, "selected"),
                        RequireBool(row, "collapsed"),
                        RequireBool(row, "leafExpanded"),
                        RequireString(row, "percentText"),
                        ParseCells(RequireArray(row, "cells")),
                        ParseSubfolders(RequireArray(row, "subfolders"))
                    );
                })
                .ToArray();
        }

        private static IReadOnlyList<BreadcrumbCellRender> ParseCells(JArray cells)
        {
            return cells
                .Select(token =>
                {
                    var cell = AsObject(token, "cells[]");
                    string kindName = RequireString(cell, "kind");
                    BreadcrumbCellKind kind;
                    switch (kindName)
                    {
                        case "segment":
                            kind = BreadcrumbCellKind.Segment;
                            break;
                        case "arrow":
                            kind = BreadcrumbCellKind.Arrow;
                            break;
                        case "plus":
                            kind = BreadcrumbCellKind.Plus;
                            break;
                        case "minus":
                            kind = BreadcrumbCellKind.Minus;
                            break;
                        default:
                            throw new FormatException($"Unknown cell kind '{kindName}'.");
                    }
                    return new BreadcrumbCellRender(
                        kind,
                        RequireString(cell, "text"),
                        RequireInt(cell, "segmentIndex"),
                        RequireBool(cell, "truncationEligible")
                    );
                })
                .ToArray();
        }

        private static IReadOnlyList<BreadcrumbSubfolderRender> ParseSubfolders(JArray subfolders)
        {
            return subfolders
                .Select(token =>
                {
                    var subfolder = AsObject(token, "subfolders[]");
                    return new BreadcrumbSubfolderRender(
                        RequireString(subfolder, "displayName"),
                        RequireString(subfolder, "folderPath"),
                        RequireBool(subfolder, "hasChildren")
                    );
                })
                .ToArray();
        }

        private static BreadcrumbArrowDirection RequireDirection(JObject root)
        {
            string direction = RequireString(root, "direction");
            switch (direction)
            {
                case "left":
                    return BreadcrumbArrowDirection.Left;
                case "right":
                    return BreadcrumbArrowDirection.Right;
                default:
                    throw new FormatException($"Unknown arrow direction '{direction}'.");
            }
        }

        private static JObject AsObject(JToken token, string context)
        {
            return token as JObject
                ?? throw new FormatException($"Expected a JSON object in '{context}'.");
        }

        private static int RequireInt(JObject source, string name)
        {
            return source.Value<int?>(name)
                ?? throw new FormatException($"Bridge message is missing the '{name}' field.");
        }

        private static bool RequireBool(JObject source, string name)
        {
            return source.Value<bool?>(name)
                ?? throw new FormatException($"Bridge message is missing the '{name}' field.");
        }

        private static string RequireString(JObject source, string name)
        {
            return source.Value<string>(name)
                ?? throw new FormatException($"Bridge message is missing the '{name}' field.");
        }

        private static JArray RequireArray(JObject source, string name)
        {
            return source[name] as JArray
                ?? throw new FormatException($"Bridge message is missing the '{name}' array.");
        }
    }
}
