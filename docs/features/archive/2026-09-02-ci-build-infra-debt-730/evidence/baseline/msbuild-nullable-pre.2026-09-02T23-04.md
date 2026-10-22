# Baseline — MSBuild Nullable / TreatWarningsAsErrors Pass (pre-change)

- Task: [P0-T4]
- Phase: Phase 0 — Policy Reads & Pre-Change Baseline Capture

Timestamp: 2026-09-02T23-04

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /flp:logfile=docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-nullable-pre.log;verbosity=normal`

EXIT_CODE: 0

Exit-code provenance: the command above was executed in the prior (stopped) execution session of this same plan, before any Phase 1 task had run, and its file-logger output is preserved unmodified on disk at `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-nullable-pre.log` (12030 lines). The log records exactly one `Build succeeded.` line (line 11998) and an end-of-build summary of `0 Error(s)`, which is MSBuild's success-exit condition; `EXIT_CODE: 0` is recorded on that basis. The log was not deleted or regenerated in this session; the three values below were re-derived from the preserved log using the same corrected de-duplicating method defined in [P0-T3] (option (a)).

## Measurement method (option (a), per [P0-T3])

- `RxCheck_nullable_pre`: count of lines matching the literal token `System.Reactive.PackagesConfigCheck.targets` found strictly before this log's single `Build succeeded.` line.
- `W_nullable_pre`: integer captured from this log's own once-emitted `^\s*(\d+) Warning\(s\)` end-of-build summary line.
- `E_nullable_pre`: integer captured the same way from the `^\s*(\d+) Error\(s\)` line.

## Output Summary

- Log total line count: 12030
- `Build succeeded.` line numbers found: 11998 (exactly one occurrence, as the method requires)
- `RxCheck_nullable_pre` = **5**
- `W_nullable_pre` = **5** (`WarningSummaryLineCount` = 1)
- `E_nullable_pre` = **0** (`ErrorSummaryLineCount` = 1)

De-duplication confirmation: a naive whole-log `-SimpleMatch` count of the `System.Reactive.PackagesConfigCheck.targets` token in this log returns **10**, exactly double the inline count of 5, matching the analyzers-pre log's behavior and confirming the Framing section's double-emission finding.

Consistency with `/p:TreatWarningsAsErrors=true`: the System.Reactive.PackagesConfigCheck diagnostic is an MSBuild target-emitted `<Warning>`, not a compiler `CSxxxx`/`BCxxxx` diagnostic, so `/p:TreatWarningsAsErrors=true` (consumed by the Csc/Vbc compiler tasks, not by arbitrary target `<Warning>` calls) does not promote it to an error. The observed `E_nullable_pre` = 0 with `W_nullable_pre` = 5 confirms this directly, and is consistent with `spec.md`'s Impact/Severity statement that neither finding causes a build failure today.

## Acceptance

- `RxCheck_nullable_pre` equal to exactly 5: PASS (5)
- `W_nullable_pre` equal to exactly 5: PASS (5)
- `E_nullable_pre` equal to 0: PASS (0)
