#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public static partial class BreadcrumbBridgeSerializer
    {
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
                    ["identity"] = s.Identity,
                    ["expanded"] = s.Expanded,
                    ["selected"] = s.Selected,
                    ["childrenLoaded"] = s.ChildrenLoaded,
                    ["children"] = SubfoldersToJson(s.Children),
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
                        RequireBool(subfolder, "hasChildren"),
                        OptionalNodeIdentity(subfolder),
                        subfolder["expanded"] != null && RequireBool(subfolder, "expanded"),
                        subfolder["selected"] != null && RequireBool(subfolder, "selected"),
                        subfolder["childrenLoaded"] != null
                            && RequireBool(subfolder, "childrenLoaded"),
                        subfolder["children"] == null
                            ? null
                            : ParseSubfolders(RequireArray(subfolder, "children"))
                    );
                })
                .ToArray();
        }

        private static string? OptionalNodeIdentity(JObject subfolder) =>
            subfolder.TryGetValue("identity", out JToken? token) && token.Type != JTokenType.Null
                ? RequireString(subfolder, "identity")
                : null;
    }
}
