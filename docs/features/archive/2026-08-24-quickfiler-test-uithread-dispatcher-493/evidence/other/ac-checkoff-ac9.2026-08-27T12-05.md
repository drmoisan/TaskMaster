# AC-9 Check-Off (P5-T9)

Timestamp: 2026-08-27T12-05
Task: [P5-T9]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-9 ("Full C# toolchain passes in a single final pass, in order") is verified against
all six Phase 3 artifacts and is checked off in `spec.md`. `P3-T1` through `P3-T4` each record
`EXIT_CODE: 0`; `P3-T5` and `P3-T6` record their observed exit codes with satisfied subset
comparisons; `P3-T6` records a satisfied line-rate condition. No Phase 3 artifact records `SKIPPED`.
`PairsN: 9`, `PairsNMinus1: 8`, so exactly one further checkbox changed state.

PairsN: 9
PairsNMinus1: 8

`pairs(9) - pairs(8) == 1`. `pairs(8)` is the value recorded by `P5-T8` in
`<FEATURE>/evidence/other/ac-checkoff-ac8.2026-08-27T12-03.md`.

## The six Phase 3 artifacts, resolved per § Conventions

| Task | Stem | Resolved filename | Exists |
| --- | --- | --- | --- |
| `P3-T1` | `csharpier-format` | `<FEATURE>/evidence/qa-gates/csharpier-format.2026-08-27T11-08.md` | yes |
| `P3-T2` | `csharpier-check` | `<FEATURE>/evidence/qa-gates/csharpier-check.2026-08-27T11-10.md` | yes |
| `P3-T3` | `msbuild-analyzers` | `<FEATURE>/evidence/qa-gates/msbuild-analyzers.2026-08-27T11-13.md` | yes |
| `P3-T4` | `msbuild-nullable` | `<FEATURE>/evidence/qa-gates/msbuild-nullable.2026-08-27T11-16.md` | yes |
| `P3-T5` | `quickfiler-test-run` | `<FEATURE>/evidence/qa-gates/quickfiler-test-run.2026-08-27T11-19.md` | yes |
| `P3-T6` | `quickfiler-test-coverage` | `<FEATURE>/evidence/qa-gates/quickfiler-test-coverage.2026-08-27T11-23.md` | yes |

## Recorded exit codes

| Task | Recorded `EXIT_CODE:` | Required |
| --- | --- | --- |
| `P3-T1` | `0` | `0` |
| `P3-T2` | `0` | `0` |
| `P3-T3` | `0` | `0` |
| `P3-T4` | `0` | `0` |
| `P3-T5` | `0` | observed, plus satisfied subset comparison |
| `P3-T6` | `1` | observed, plus satisfied subset comparison and line-rate condition |

`EXIT_CODE: 0` is **not** required from `P3-T5` or `P3-T6`, per this task's own text. Both run over a
test assembly that also contains sibling-owned files, so an absolute all-green exit code over that
assembly would be unsatisfiable whenever any test outside this feature's owned set is already red.
`P3-T5` happened to exit `0` regardless. `P3-T6` exited `1` solely because
`Assert-CoberturaLineCoverageThreshold` throws below an 80% line-coverage floor that § Decisions
Record D5 explicitly declines to assert, ratified by spec § Test Strategy on the grounds that this is
a test-only change with no production line in the diff. No test failed in that run.

## Subset and rate conditions

| Condition | Result |
| --- | --- |
| `P3-T5` failed-test set subset of `BaselineFailedTests` (`P0-T12`) | satisfied — both sets empty |
| `P3-T6` failed-test set subset of `CoverageBaselineFailedTests` (`P0-T13`) | satisfied — both sets empty |
| `P3-T6` line-rate condition | satisfied — `CoberturaPostProcessed` matched (`false` both sides), `abs(lines-valid delta)` 0 at or under `AddedLineCount: 624`, line-rate delta `0.00` pp which is at least `-0.50` |

No test failed that was not already failing at the Phase 0 baseline, so the
`BLOCKED: post-change test regression blocks AC-9` branch is not taken.

## No SKIPPED

A recursive search of the entire `<FEATURE>/evidence/` tree for the literal `SKIPPED` returns no
file. No Phase 3 command task was skipped; every one executed its stated command.

## The four commands run, verbatim, in the order AC-9 states

1. `dotnet tool run csharpier check .`
2. `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=quickfiler-test-final.trx" /ResultsDirectory:TestResults\plan-logs\p3-t5`

Command 1 was preceded in `P3-T1` by the targeted write pass
`dotnet tool run csharpier format <four explicit file paths>`, which names files rather than
directories so it cannot rewrite anything outside the owned set. Step 3 does **not** carry
`/p:Nullable=enable`, as AC-9 requires. Command 4 carries `/InIsolation` as AC-9 requires, and its
assembly argument names only `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`, so no `.claude` worktree
copy of a test assembly is reached.

These four commands are also restated in the completion report, as AC-9's final sentence requires.

## Single final pass

No task in Phase 3 after `P3-T1` failed or rewrote a file, so the phase completed in one pass and no
restart from `P3-T1` was triggered. `P3-T1`'s own rewrite of the two new files is the pass's intended
work; a confirming second invocation of the identical format command left every SHA-256 unchanged,
proving idempotence, and `P3-T2` then verified the whole tree read-only.

## Library compliance

MSTest, Moq, and FluentAssertions only. The two new files use MSTest attributes and FluentAssertions
assertions; Moq is not needed by the six regression tests and is therefore not referenced by the new
test file, while the pre-existing `Moq` usage in `Part2.cs` is unchanged. No other test framework or
assertion library is introduced.

## Result

`- [ ] **AC-9 …` changed to `- [x] **AC-9 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed.
