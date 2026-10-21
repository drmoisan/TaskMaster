# [P1-T5] Scoring-Method Relocation and Injectable Scoring Seam

Timestamp: 2026-08-26T09-29

Task: [P1-T5]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

Per D3, `ScoreRemainingQueueMailItemAsync` was moved verbatim out of
`QuickFiler/Controllers/QfcDatamodel.cs` (it occupied `:363-377` at baseline) into
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`. Signature and behaviour are otherwise
unchanged: same accessibility, same parameters, same return type, same `Probability debug` log
line text.

Alongside it, the injectable factory seam was added:

```csharp
internal Func<IFolderScoringService> ScoringServiceFactory { get; set; } =
    () => new FolderScoringService();
```

and the hard-coded `new FolderScoringService()` inside the relocated method was replaced by a call
through that seam (`var scoringService = ScoringServiceFactory();`). The default preserves
production behaviour exactly.

This task precedes every high-confidence-path test task in this phase because the seam is what
keeps `[P1-T6]` and `[P1-T12]` off live Outlook COM, which `.claude/rules/general-unit-test.md`
UT4 prohibits.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `wc -l "QuickFiler/Controllers/QfcDatamodel.cs" "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs"`
EXIT_CODE: 0

| Path | Baseline (`[P0-T14]`) | Post-change | Condition | Result |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 496 | **480** | strictly less than 496 | satisfied (16 lines freed) |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 | **203** | at most 500 | satisfied (297 of headroom) |

## Output Summary

Scoring method relocated and seamed. `QfcDatamodel.cs` drops from 496 to 480 lines, buying 16
lines of budget under the 500-line cap before any widening. `QfcDatamodel.QueueProcessing.cs`
grows from 177 to 203, well inside the cap. Compile exit 0.
