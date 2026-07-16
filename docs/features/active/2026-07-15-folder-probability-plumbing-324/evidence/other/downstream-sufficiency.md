# Downstream Contract Sufficiency — 9002 and 9003

Timestamp: 2026-07-16T03-32

This feature (child 9001) is the prerequisite plumbing for two downstream sibling features in the
`folder-tree-percentage-ui` epic. This note records that the additive contract delivered here is
sufficient for both consumers to render a whole-number percentage and to skip non-suggestion rows,
from a single shared normalization point, with no second plumbing pass.

## Single normalization point

`Probability` is computed exactly once, in the scoring layer, by
`FolderScorer.ToScoredArray` / `ToScoredArray(int)` (max-normalization `Score / TopScore` with a
zero-guard, in `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`). Both consumers read the same
`FolderScore.Probability` value, so the EfcViewer `ListBox` and the QuickFiler `ComboBox` cannot
diverge on the percentage shown for the same suggestion set.

## 9002 — EfcViewer folder tree + percentage (via `FindFolderRows`)

- Consumer path: the 9002 renderer bound to `FindMatches` -> `FindFolder` switches to
  `FolderPredictor.FindFolderRows(...)` (same signature as `FindFolder`).
- For each `FolderRow`, 9002 inspects `Kind`:
  - skips `FolderRowKind.Separator`, `FolderRowKind.SearchResult`, and `FolderRowKind.Recent`
    (no `.StartsWith("====")` string-matching required);
  - for `FolderRowKind.Suggestion`, reads `row.Score.Value.Probability` and renders
    `Math.Round(Probability * 100)` right-aligned.
- `Text` for every row equals the legacy `FindFolder` string byte-for-byte, so folder text and
  ordering are unchanged from the current name-only list.

## 9003 — QuickFiler dropdown tree + percentage (via `FolderRowArray`)

- Consumer path: the 9003 renderer bound to `FolderArray` via `SetFolderItems` consumes
  `FolderPredictor.FolderRowArray` (or a future scored overload of `SetFolderItems`).
- 9003 uses the same `Kind` + `Probability` produced by the single normalization point in
  `FolderScorer.ToScoredArray`, applying the identical `Math.Round(Probability * 100)` mapping and
  the identical `Kind`-based skip of non-suggestion rows.
- Because both consumers read the same normalized value, the QuickFiler dropdown shows the same
  percentage for a given suggestion set as the EfcViewer list.

## Whole-number percentage mapping

Both consumers render `Math.Round(Probability * 100)` (a whole-number percentage in the range
0..100; the top suggestion is 1.0 -> 100). No consumer re-derives normalization; no second plumbing
pass is required. `IFolderSearchHandler` is intentionally left unchanged; any additive extension of
that seam is a consumer-driven decision deferred to 9003 (see plan Open Questions).

## Kind-based skip (no sentinel scraping)

Separator/section rows are tagged `FolderRowKind.Separator` with `Score == null`; search results
`FolderRowKind.SearchResult`; recents `FolderRowKind.Recent`. Only `FolderRowKind.Suggestion` rows
carry a non-null `Score`. Downstream renderers therefore distinguish rows by `Kind` rather than by
matching separator text such as `.StartsWith("====")`.
