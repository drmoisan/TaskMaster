# [P2-T4] Return the Real Folder from the Datamodel Scorer (Scope 427-A)

Timestamp: 2026-08-26T09-56

Task: [P2-T4]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` — the relocated
`ScoreRemainingQueueMailItemAsync` now returns `(score.Score, score.TopFolder)` in place of the
`(score.Score, string.Empty)` stub landed by `[P1-T11]` under D-Plan-1. This is the only changed
line in the method; its `Probability debug` log line text is byte-for-byte unchanged.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder" "/Logger:trx;LogFileName=p2-t4.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t4"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t4/p2-t4.trx`

Counters: total 1, executed 1, **passed 1**, failed 0, error 0, timeout 0, aborted 0.

- `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` = **Passed**
  (was Failed at `[P1-T12]`).

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. No `danmoisan` or `megalodon4` match
anywhere under the feature folder.

## Output Summary

The producer end of the Scope 427-A folder path is complete: the scorer surfaces the folder the
classifier already ranked highest, and the gate (`[P2-T3]`) carries it into the accepted carrier.
The `[P1-T12]` `[expect-fail]` test transitions Failed -> Passed. Format EXIT_CODE 0, check
EXIT_CODE 0, compile EXIT_CODE 0, scoped run EXIT_CODE 0 with 1 of 1 Passed and 0 Failed.
