# [P5-T9] The Single Clean Toolchain Pass

Timestamp: 2026-08-26T11-11

Task: [P5-T9]
Feature: docs/features/active/quickfiler-bug-family-446

This artifact records the pass of the Phase 5 toolchain loop that the loop finally accepted. One
restart occurred; it is documented at the end.

## The five command strings and their exit codes

### [P5-T1] Formatting (mutating, scoped to the change set per D-Plan-4)

Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcDatamodelTests.cs QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs QuickFiler/Controllers/QfcDatamodel.cs QuickFiler/Controllers/QfcFormController.Actions.cs QuickFiler/Controllers/QfcHomeController.Iteration.cs QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs QuickFiler/Interfaces/IQfcDatamodel.cs'`

EXIT_CODE: 0
Evidence: `p5-t1-csharpier-format.2026-08-26T10-58.md`

### [P5-T2] Formatting gate (read-only, repository-wide)

Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier check .'`

EXIT_CODE: 0
Evidence: `p5-t2-csharpier-check.2026-08-26T10-59.md`

### [P5-T3] Analyzer gate

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0
Evidence: `p5-t3-analyzer-build.2026-08-26T10-59.md`

### [P5-T4] Type-check gate

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0
Evidence: `p5-t4-nullable-build.2026-08-26T11-00.md`

### [P5-T5] Test gate

Command: `& $vstest $asm /InIsolation /EnableCodeCoverage "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /Logger:trx "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\qa-gates\p5-t5"`

EXIT_CODE: 0
Evidence: `p5-t5-vstest.2026-08-26T11-01.md`

## Rewritten-file count from [P5-T1]

**0.** Derived by comparing SHA-256 digests of all 13 change-set `.cs` paths taken before and
after the mutating pass. All 13 digest pairs are byte-identical. The count is not read from
CSharpier's `Formatted 13 files` line, which reports files processed rather than files changed.

## Acceptance conditions of this task

| condition | value | satisfied |
| --- | --- | --- |
| recorded rewritten-file count is `0` | `0` | yes |
| `[P5-T1]` exit code is `0` | `0` | yes |
| `[P5-T3]` exit code is `0` | `0` | yes |
| `[P5-T4]` exit code is `0` | `0` | yes |
| `[P5-T2]` exit code is `0` | `0` | yes - primary branch, no reconciliation needed |
| `[P5-T5]` exit code is `0` | `0` | yes - primary branch, no reconciliation needed |

Neither `[P5-T2]` nor `[P5-T5]` completed on a pre-existing-baseline branch, so neither
reconciliation clause of this task applies. `[P0-T9]` recorded an empty unformatted path set and
`[P0-T12]` recorded an empty failed-test set, so no reconciliation set existed to fall back on in
the first place; both gates passed on their own merit.

Test counts in the accepted pass: total `6501`, passed `6501`, failed `0`, skipped `0`, across the
nine discovered test assemblies.

## Loop restarts

**One restart occurred.**

The first pass ran `[P5-T1]` through `[P5-T4]` green and then failed at `[P5-T5]` with
`EXIT_CODE: 1` and `No test source files were specified.` The cause was confined to the executor's
transcription of the plan's discovery prelude: the intermediate shell collapsed the doubled
backslashes in the regular expression `"\\bin\\Debug\\"`, which under .NET regular-expression
semantics turned `\b` into a word-boundary assertion, so the filter matched none of the nine
assemblies and `$asm` was empty. Nothing in the working tree was created, rewritten or deleted by
that failed invocation, but a failed toolchain step is a failed toolchain step, so per the Phase 5
preamble the loop restarted from `[P5-T1]`.

The artifacts of the aborted pass are retained alongside those of the accepted pass, as the
preamble requires:

- `p5-t1-csharpier-format.2026-08-26T10-52.md` (aborted pass)
- `p5-t2-csharpier-check.2026-08-26T10-54.md` (aborted pass)
- `p5-t3-analyzer-build.2026-08-26T10-55.md` (aborted pass)
- `p5-t4-nullable-build.2026-08-26T10-56.md` (aborted pass)
- `p5-t5-vstest.2026-08-26T10-56.md` (aborted pass, records `EXIT_CODE: 1` and the root cause)

No other restart occurred. No step of the accepted pass rewrote a file.

## Output Summary

Single accepted toolchain pass: format `0`, format-check `0`, analyzer `0`, type-check `0`, test
`0`, with a rewritten-file count of `0` and a failed-test count of `0`. Both conditional gates
completed on their primary branches. One loop restart occurred, caused by a collapsed-backslash
regular expression in the executor's transcription of the discovery prelude, and every artifact
from that aborted pass is retained.
