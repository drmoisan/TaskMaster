# Baseline — Upstream Contract Dependency Verification (P0-T6)

Timestamp: 2026-07-16T09-22
Command: grep -nE "..." on UtilitiesCS/OutlookObjects/Folder/{FolderScore.cs,FolderRow.cs,FolderPredictor.cs}
EXIT_CODE: 0

Result: SATISFIED. The `folder-probability-plumbing` (#324) concrete contract resolves in namespace `UtilitiesCS` (merged onto this integration branch via PR #333). #325 CONSUMES — does not implement — this contract.

Per-type resolution:

- `FolderScore` — `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs`
  - `public readonly struct FolderScore` (line 11)
  - `public string FolderPath { get; }` (line 32) — scoring key
  - `public long Score { get; }` (line 39) — raw unbounded ranking score
  - `public double Probability { get; }` (line 51) — max-normalized `[0,1]` relative display value (`Score/TopScore`, `0` when top score is `0`)
  - Constructor `FolderScore(string folderPath, long score, double probability)` (line 21)

- `FolderRowKind` + `FolderRow` — `UtilitiesCS/OutlookObjects/Folder/FolderRow.cs`
  - `public enum FolderRowKind { Separator, SearchResult, Suggestion, Recent }` (line 9)
  - `public readonly struct FolderRow` (line 30)
  - `public string Text { get; }` (line 49) — exact legacy string at this position
  - `public FolderRowKind Kind { get; }` (line 52)
  - `public FolderScore? Score { get; }` (line 59) — non-null ONLY for `Kind == Suggestion`
  - Constructor `FolderRow(string text, FolderRowKind kind, FolderScore? score)` (line 41)

- `FolderPredictor` members — `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
  - `public FolderRow[] FolderRowArray` (line 237) — ordered SUGGESTIONS separator, top-5 Suggestion rows with FolderScore, RECENT SELECTIONS separator, Recent rows
  - `public FolderRow[] FindFolderRows(string searchString, object objItem, bool reloadCTFStagingFiles = true, List<string> emailSearchRoots = null, bool recalcSuggestions = false, IEnumerable<(string root, string excludedFolder, bool excludeChildren)> exclusions = null)` (line 354) — ordered SEARCH RESULTS separator + SearchResult rows, SUGGESTIONS separator + Suggestion rows with FolderScore, RECENT SELECTIONS separator + Recent rows

Additionally, the solution compiles clean with these types present (P0-T3 analyzer build EXIT_CODE 0, P0-T4 nullable build EXIT_CODE 0), which is a compile-level confirmation that the types resolve.

Consumption note: #325 reads `FolderScore.Probability` verbatim and formats it; it does not recompute scores or modify FolderScorer/FolderPredictor scoring math. Presence of all listed types SATISFIES the dependency.
