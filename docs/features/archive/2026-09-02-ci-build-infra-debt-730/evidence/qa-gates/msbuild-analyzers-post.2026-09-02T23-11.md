# QA Gate — MSBuild Analyzer Pass (post-change)

- Task: [P2-T6]
- Phase: Phase 2 — Verification & Final QC

Timestamp: 2026-09-02T23-11

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-analyzers-post.log;verbosity=normal`

EXIT_CODE: 0

Executed after Phase 1 was complete (all four Phase-1 files in place, including the new root `Directory.Build.props`). MSBuild resolved via `vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"`. Console tail: `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:18.28`; `$LASTEXITCODE` = 0.

## Measurement method (option (a), per [P0-T3])

`RxCheck_*` counted strictly before this log's `Build succeeded.` line; `W_*` / `E_*` parsed from this log's own `^\s*(\d+) Warning\(s\)` / `^\s*(\d+) Error\(s\)` summary line, not a whole-log token count.

## Output Summary

- Log total line count: 11906
- `Build succeeded.` line numbers found: 11900 (exactly one occurrence)
- `RxCheck_analyzers_post` = **0**
- `W_analyzers_post` = **0**
- `E_analyzers_post` = **0**

Recompilation confirmation (guards against a vacuous result from a skipped compile): the log records 67 `CoreCompile:` target entries and 36 `csc.exe` invocations, confirming `/t:Rebuild` genuinely recompiled the solution rather than short-circuiting on an up-to-date check. The whole-log naive token count for `System.Reactive.PackagesConfigCheck.targets` is also 0, so the warning is absent from both the inline and the end-of-build summary regions of the log — not merely excluded by the de-duplicating slice.

## Delta against the [P0-T3] baseline

| Value | Baseline (P0-T3) | Post-change (P2-T6) | Delta |
|---|---|---|---|
| `RxCheck_analyzers_*` | 5 | 0 | -5 |
| `W_analyzers_*` | 5 | 0 | -5 |
| `E_analyzers_*` | 0 | 0 | 0 |

The five System.Reactive.PackagesConfigCheck "unsupported scenario" warnings (one per affected project: QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, UtilitiesCS.Test) are eliminated, and no new warning or error is introduced: the total warning count fell by exactly 5 and the error count is unchanged at 0.

## Acceptance

- `RxCheck_analyzers_post == 0` (down from `RxCheck_analyzers_pre == 5`): PASS.
- `W_analyzers_post == W_analyzers_pre - 5`: PASS (0 == 5 - 5).
- `E_analyzers_post == E_analyzers_pre`: PASS (0 == 0).
