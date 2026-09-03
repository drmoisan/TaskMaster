# QA Gate — MSBuild Nullable / TreatWarningsAsErrors Pass (post-change)

- Task: [P2-T7]
- Phase: Phase 2 — Verification & Final QC

Timestamp: 2026-09-02T23-12

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-nullable-post.log;verbosity=normal`

EXIT_CODE: 0

Executed after Phase 1 was complete. Console tail: `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:15.76`; `$LASTEXITCODE` = 0.

## Measurement method (option (a), per [P0-T3])

`RxCheck_*` counted strictly before this log's `Build succeeded.` line; `W_*` / `E_*` parsed from this log's own `^\s*(\d+) Warning\(s\)` / `^\s*(\d+) Error\(s\)` summary line, not a whole-log token count.

## Output Summary

- Log total line count: 11742
- `Build succeeded.` line numbers found: 11736 (exactly one occurrence)
- `RxCheck_nullable_post` = **0**
- `W_nullable_post` = **0**
- `E_nullable_post` = **0**

Recompilation confirmation: the log records 56 `CoreCompile:` target entries and 36 `csc.exe` invocations, confirming `/t:Rebuild` genuinely recompiled the solution. The whole-log naive token count for `System.Reactive.PackagesConfigCheck.targets` is also 0.

## Delta against the [P0-T4] baseline

| Value | Baseline (P0-T4) | Post-change (P2-T7) | Delta |
|---|---|---|---|
| `RxCheck_nullable_*` | 5 | 0 | -5 |
| `W_nullable_*` | 5 | 0 | -5 |
| `E_nullable_*` | 0 | 0 | 0 |

The five System.Reactive.PackagesConfigCheck warnings are eliminated under the `/p:TreatWarningsAsErrors=true` pass as well, with no new warning or error introduced.

## Acceptance

- `RxCheck_nullable_post == 0` (down from `RxCheck_nullable_pre == 5`): PASS.
- `W_nullable_post == W_nullable_pre - 5`: PASS (0 == 5 - 5).
- `E_nullable_post == E_nullable_pre`: PASS (0 == 0).
