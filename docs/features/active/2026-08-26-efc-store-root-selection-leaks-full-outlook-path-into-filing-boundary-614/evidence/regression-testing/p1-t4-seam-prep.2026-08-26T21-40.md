# P1-T4 — Interim Build and Behavior-Preservation Proof (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T21-40

Command (1 of 2):
`& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`

Command (2 of 2):
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests" "/Logger:trx;LogFileName=p1-t4.trx" "/ResultsDirectory:coverage\trx\p1-t4"`

(`$vstest` resolved through `vswhere -latest -property installationPath` to the VS 18 Community
Test Platform. This is an interim, non-gate build, so `/t:Build` is used as the plan permits; both
`/t:Rebuild` gates run in Phase 5.)

EXIT_CODE: 0 (both commands)

## Output Summary

| Command | Exit code | Result |
| --- | ---: | --- |
| MSBuild `/t:Build` | 0 | Solution builds with the two-predicate split and the new resolver in place. |
| vstest `EfcSelectionGuardTests` | 0 | `Test Run Successful.` — `Total tests: 19`, `Passed: 19`, `Failed: 0`, `Skipped: 0`. |

**Behaviour preserved.** The pre-change file carried 9 tests, all passing. After P1-T1 through
P1-T3 the class carries 19 tests, all passing:

- 8 filing-predicate tests, mechanically retargeted to the two-argument call. Every assertion is
  unchanged in outcome: `null`, `string.Empty`, whitespace, the banner sentinel, the store-rooted
  value, the single-separator-leading value, and the drive-rooted value are still rejected, and
  `Clients\North` is still accepted.
- 9 folder-creation-predicate tests, of which one
  (`IsValidCreationSelection_TwoCharacterSelection_IsRejected`) is the renamed and retargeted form
  of the former `IsValidFilingSelection_TwoCharacterSelection_IsRejected`; its expectation
  (`"AB"` rejected) is unchanged, only the predicate it addresses moved.
- 2 archive-root resolver tests covering the success path and the degrade path.

Phase 1 therefore changed no observable validation outcome. The two defects CR-1 and CR-2 remain
present and unpinned in the filing predicate; Phase 2 and Phase 3 prove them fail-before and fix
them.

Raw TRX was written to the gitignored `coverage\trx\p1-t4\` tree, not under `evidence/`.
