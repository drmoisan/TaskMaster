# Baseline — MSBuild Analyzer Pass (pre-change)

- Task: [P0-T3]
- Phase: Phase 0 — Policy Reads & Pre-Change Baseline Capture

Timestamp: 2026-09-02T23-03

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-analyzers-pre.log;verbosity=normal`

EXIT_CODE: 0

Exit-code provenance: the command above was executed in the prior (stopped) execution session of this same plan, before any Phase 1 task had run, and its file-logger output is preserved unmodified on disk at `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-analyzers-pre.log` (11878 lines). The log records exactly one `Build succeeded.` line (line 11846) and an end-of-build summary of `0 Error(s)`, which is MSBuild's success-exit condition; `EXIT_CODE: 0` is recorded on that basis. The log was not deleted or regenerated in this session; the three values below were re-derived from the preserved log using the corrected de-duplicating method fixed by the plan's Framing section "Warning-count measurement method" note (option (a)).

## Measurement method (option (a), per [P0-T3])

- `RxCheck_analyzers_pre`: count of lines matching the literal token `System.Reactive.PackagesConfigCheck.targets` found strictly before the log's single `Build succeeded.` line.
  - `$content = Get-Content <logfile>; $cut = ($content | Select-String -Pattern 'Build succeeded.' -SimpleMatch).LineNumber[0]; ($content[0..($cut-2)] | Select-String -Pattern 'System.Reactive.PackagesConfigCheck.targets' -SimpleMatch).Count`
- `W_analyzers_pre`: integer captured from MSBuild's own once-emitted end-of-build summary line.
  - `(Select-String -Path <logfile> -Pattern '^\s*(\d+) Warning\(s\)').Matches.Groups[1].Value`
- `E_analyzers_pre`: integer captured the same way from the `^\s*(\d+) Error\(s\)` line.

## Output Summary

- Log total line count: 11878
- `Build succeeded.` line numbers found: 11846 (exactly one occurrence, as the method requires)
- `RxCheck_analyzers_pre` = **5** (one inline occurrence per affected project: QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, UtilitiesCS.Test)
- `W_analyzers_pre` = **5** (parsed from the single `N Warning(s)` summary line; `WarningSummaryLineCount` = 1)
- `E_analyzers_pre` = **0** (parsed from the single `N Error(s)` summary line; `ErrorSummaryLineCount` = 1)

De-duplication confirmation: a naive whole-log `-SimpleMatch` count of the `System.Reactive.PackagesConfigCheck.targets` token in this log returns **10**, exactly double the inline count of 5, confirming the Framing section's finding that MSBuild's file logger at `verbosity=normal` prints each such warning once inline and again in the end-of-build per-target summary that follows `Build succeeded.`. The option (a) method used above excludes the summary duplicates and yields the correct value of 5.

## Acceptance

- `RxCheck_analyzers_pre` equal to exactly 5: PASS (5)
- `W_analyzers_pre` equal to exactly 5: PASS (5)
- `E_analyzers_pre` equal to 0: PASS (0)
