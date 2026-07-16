# Upstream folder-probability-plumbing (9001) Contract Re-confirmation (P3-T4)

Timestamp: 2026-07-16T01-15

## Merged-vs-unmerged status

MERGED. `folder-probability-plumbing` (real issue #324, epic placeholder 9001) is merged into this
branch via merge commit a2af16c4 ("Merge pull request #333 from
drmoisan/feature/folder-probability-plumbing-324"), which is an ancestor of the current
feature branch head. The real merged surface lives in `UtilitiesCS/OutlookObjects/Folder/`.

## Observed merged contract shape (verified by reading source on this branch)

- `public readonly struct UtilitiesCS.FolderScore` (`FolderScore.cs`): ctor `(string folderPath, long score, double probability)`; properties `string FolderPath`, `long Score`, `double Probability`. `Probability` is documented as a max-normalized value in `[0,1]` (`Score / TopScore`) intended as a relative display value; when the top score is 0, every row's Probability is 0 (no divide-by-zero).
- `public readonly struct UtilitiesCS.FolderRow` (`FolderRow.cs`): ctor `(string text, FolderRowKind kind, FolderScore? score)`; properties `string Text`, `FolderRowKind Kind`, `FolderScore? Score`. `Score` is non-null only for `FolderRowKind.Suggestion` rows.
- `public enum UtilitiesCS.FolderRowKind { Separator, SearchResult, Suggestion, Recent }`.
- Producing surface: `FolderPredictor.FolderRowArray` (`FolderRow[]`, FolderPredictor.cs:237), `FolderPredictor.FindFolderRows(...)` (`FolderRow[]`, :354), `FolderScorer.ToScoredArray()` / `ToScoredArray(int topN)` (`FolderScore[]`, FolderScorer.cs:261/272).

## Assumed seam shape (research §5.2 / plan P3-T2)

`IFolderProbabilitySource.TryGetProbability(string fullFolderPath, out double probability)` — a mapping
of full folder-path string to prediction probability, `double` in `[0,1]`.

## Reconciliation outcome

NO CHANGE REQUIRED to the seam or the adapter.

- The assumed shape (full folder-path string -> `double` in `[0,1]`) is confirmed by the merged surface:
  `FolderScore.Probability` is a `double` in `[0,1]` keyed by `FolderScore.FolderPath` (a full/relative folder-path string), exactly the mapping the seam abstracts. `FolderRow.Text` carries the same path string the presentation layer joins by (byte-for-byte parity with the legacy `FolderArray`/`FindFolder` string output, per FolderRowTests).
- Because the merged probability is already a `[0,1]` value (not a pre-scaled percent), `PercentageFormatter` keeps its `* 100` step; no formatter change is needed either.
- The seam remains the single coupling point. A production `IFolderProbabilitySource` is built in the coverage-exempt controller (Phase 4) by projecting `FolderRowArray`/`ToScoredArray` into a `FolderPath -> Probability` lookup over `Suggestion`-kind rows; separators/search-results/recents carry a null `Score` and therefore contribute no probability, which the adapter already leaves blank.

Adapter delta: none. The host-neutral `FolderProbabilityAdapter` (`Apply(FolderSuggestionTree)`) is unchanged from the assumed design; it joins by `FolderSuggestionNode.FullPath` string equality and leaves banners/unmatched rows null.
