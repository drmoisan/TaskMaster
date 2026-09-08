# Phase 2 — FolderPredictor Read-Site and File-Size Invariants (P2-T6)

Timestamp: 2026-09-08T08-01

Command: a single `pwsh -NoProfile -Command` run against `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`, counting each fixed string with `[regex]::Matches($text, [regex]::Escape(<literal>))` over the file read with `-Raw`, measuring the line count with `(@(Get-Content -LiteralPath <path>)).Count`, and resolving the enclosing member of each surviving occurrence by scanning backwards to the nearest member declaration.

EXIT_CODE: 0

Output Summary:

The four required figures:

1. Occurrence count of the fixed string `_globals.Ol.ArchiveRootPath`: **5**.
2. Occurrence count of the fixed string `_globals?.Ol.ArchiveRootPath`: **0**.
3. Number of the five surviving occurrences that sit inside a `try` block or are routed through `GetArchiveRootForDisplayOrNull`: **0**.
4. Line count of `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`: **997**, which is less than or equal to 1002.

The five surviving occurrences and their enclosing members, which are exactly the five members AC3 pins:

| Line | Enclosing member | Expression |
| --- | --- | --- |
| 305 | `FindFolder` | `emailSearchRoots = new() { _globals.Ol.ArchiveRootPath };` |
| 376 | `FindFolderRows` | `emailSearchRoots = new() { _globals.Ol.ArchiveRootPath };` |
| 687 | `CreateFolder` | `olAncestor = _globals.Ol.ArchiveRootPath;` |
| 752 | `CreateFolderAsync` | `olAncestor = _globals.Ol.ArchiveRootPath;` |
| 909 | `LoopFolders` | `olAncestor = _globals.Ol.ArchiveRootPath;` |

The first four line numbers are unchanged from the pre-change figures recorded by P0-T13. The fifth moved from `:914` to `:909` because P2-T5 removed the seven-line `ProjectSuggestionPath` block and its adjacent blank line from above it while the two suggestion hoists added one line each; the expression on that line is unchanged.

Non-degradation evidence for figure 3: for each of the five lines, a scan from its enclosing member's declaration line down to the read line found zero `try` openings and zero `catch` clauses, so no read is inside a guarded region. Each is a direct `_globals.Ol.ArchiveRootPath` expression rather than a call to the guarded accessor, so none is routed through `GetArchiveRootForDisplayOrNull`. These five reads therefore still throw when the archive root cannot be resolved, which is what AC3 requires: they are functional search-root and ancestor resolutions rather than cosmetic display projections.

Discrimination against the pre-change state: P0-T13 recorded the pre-change counts as 7 and 1. Both of the first two conditions above are therefore false before this phase and true after it, so neither can pass vacuously.

The four display reads that were removed from the count are now routed through the guarded accessor, one hoisted call per member:

| Line | Enclosing member | Call |
| --- | --- | --- |
| 795 | `AddRecents` | `var r = GetArchiveRootForDisplayOrNull();` |
| 815 | `AddSuggestions` | `var r = GetArchiveRootForDisplayOrNull();` |
| 846 | `AddSuggestionRows` | `var root = GetArchiveRootForDisplayOrNull();` |
| 871 | `AddRecentRows` | `var root = GetArchiveRootForDisplayOrNull();` |

The total occurrence count of `GetArchiveRootForDisplayOrNull` in the file is 4, one per member, so no member reads the root more than once per projection.

File-size context for figure 4: the pre-change count was 1002. The net change is minus 5 lines, composed of plus 1 for each of the two suggestion-surface hoists and minus 7 for the relocation of `ProjectSuggestionPath` and its adjacent blank line into `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs`. That relocation is what keeps the file at or below its pre-change size, which is the AC6 condition; without it the two hoists would have taken the file to 1004.
