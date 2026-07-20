# Debt 2 — Batch: OutlookObjects/Folder — Remediated (Final Phase 2 Batch)

Timestamp: 2026-07-20T01-20
Command: `MSBuild.exe UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

Output Summary: `Build succeeded. 0 Warning(s), 0 Error(s).` This is the FIRST isolated
`UtilitiesCS.csproj` rebuild to reach a fully clean state after all 7 Phase 2 batches. This
result is the direct predecessor to P2-T17's mandatory full-solution rebuild gate.

## Before/after (this batch's 4 files)

All 4 files' CS86xx diagnostics reduced to zero (31 diagnostics total: CS8604:17, CS8603:4,
CS8601:8).

## Remediation approach

- **CS8603 (possible null reference return)**: `FolderConverter.cs` and `FolderPredictor.cs` each
  assign a lambda to a `Func<...,string>`-typed delegate field whose implementation calls
  `InputBox.ShowDialog(...)`/`MyBox.ShowDialog(...)` (both nullable `string?` returns); the
  delegate's own declared return type is non-nullable `string`. Fixed with `!` at each lambda's
  call-site expression (4 occurrences across 2 files).
- **CS8604/CS8601 (`FolderScorer.cs`, the batch's largest file, 24 diagnostics)**: this file
  contains four near-duplicate `QuerySubject`/`QueryFolder` method pairs (parallel and
  non-parallel variants) that build a private `struct FolderScoring` (non-nullable `string
  FolderPath`/`FolderName`, `int[] FolderEncoding`/`FolderWordLengths`) from
  `SubjectMapEntry`'s nullable properties (`Folderpath`, `Foldername`, `FolderEncoded`,
  `FolderWordLengths`, `SubjectEncoded`, `SubjectWordLengths`, `EmailSubject` — all confirmed
  `?`-annotated in `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapEntry.cs`) and pass them
  to `SmithWaterman.CalculateScore(...)`'s non-nullable `int[]`/`string` parameters. Two of the
  six `new FolderScoring { ... }` construction sites (the `QueryCombined` overloads) source their
  fields from an ALREADY non-nullable `FolderScoring` struct (not `SubjectMapEntry`) and
  correctly required no fix — confirmed by their distinct casing (`x.FolderName`/
  `x.FolderEncoding`, not `x.Foldername`/`x.FolderEncoded`) not appearing in the diagnostic list.
  Fixed with `!` at each nullable-sourced construction/argument site (20 occurrences across the
  four flagged blocks, matching this remediation's established null-forgiving convention).
- **`FolderTreeCompatibilityView.cs`**: `treeNode.AddChild(child!)` — `child` comes from
  `snapshotNode.ChildKeys.Select(CreateNode).Where(node => node != null)`; the `!= null` LINQ
  filter does not narrow the compiler's nullable-flow state for the loop variable the same way
  an `is not null` pattern would, so the null-forgiving operator is required at the
  `AddChild(...)` call site despite the preceding filter.

## Behavior-preservation confirmation

`git diff --stat` for the 4 batch files shows 46 insertions / 38 deletions — all annotation/
null-forgiving additions and minor line-wrapping from `csharpier`-compatible formatting of the
multi-line `FolderWordLengths` assignment; no removed or altered method signatures, no altered
control flow.
