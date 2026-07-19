# Batch F3c Nullable Gate — FolderScorer.cs (P4-T9)

Timestamp: 2026-07-19T12-55

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate (UtilitiesCS Rebuild, TreatWarningsAsErrors, BuildProjectReferences=false): **zero CS86xx** for
  FolderScorer.cs (AC1). This task precedes P4-T11 because FolderScorer must be clean before FolderPredictor
  (which holds a `FolderScorer Suggestions` field) is annotated.

## Key annotation decisions (FolderScorer.cs, 663 lines, not split)
- `Prediction<string>[]? predictions = null;` (declared null, reassigned in both branches).
- Cast-with-`as` locals nullable: `string[]? folders = foldersObject as string[];` and matching
  `AddArray(string[]? folders, int topN)`.
- The internal `struct FolderScoring` reference-type fields are set by every object-initializer construction
  site (QuerySubject/QueryFolder/QueryCombined), so no `= default!` was needed; struct fields require no CS8618.
- External oblivious types consumed at call sites only: `ScoDictionaryNew<string,long>` and #364's
  `Tokenizer.AsTokenPattern()`/`VerboseLogger<T>` — none edited. No post-condition attributes; no record/init.
