# [P2-T3] Carry the Computed Folder Forward (Scope 427-A)

Timestamp: 2026-08-26T09-54

Task: [P2-T3]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` — the accepted-candidate construction
inside `DequeueAsync` now reads
`accepted.Add(new QfcPreScoredItem(mailItem, topFolder));`, where `topFolder` is the second element
of the tuple already returned by the widened `_scoreLoader`. This replaces the `string.Empty` stub
introduced by `[P1-T8]` under D-Plan-1.

No other line changed; the tuple deconstruction
`(long score, string topFolder) = await _scoreLoader(mailItem, token)` was already in place from
`[P1-T8]`.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult" "/Logger:trx;LogFileName=p2-t3.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t3"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t3/p2-t3.trx`

Counters: total 1, executed 1, **passed 1**, failed 0, error 0, timeout 0, aborted 0.

- `DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult` = **Passed**
  (was Failed at `[P1-T13]`).

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. A case-insensitive search for the
account name and the machine name across the feature folder returns no match.

## Output Summary

The Scope 427-A producer-side carrier is now populated at the gate boundary. The `[P1-T13]`
`[expect-fail]` test transitions Failed -> Passed. Format EXIT_CODE 0, check EXIT_CODE 0, compile
EXIT_CODE 0, scoped run EXIT_CODE 0 with 1 of 1 Passed and 0 Failed.
