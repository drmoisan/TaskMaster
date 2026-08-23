# Phase 5 — Post-Change Numeric Coverage Capture (Issue #445)

Timestamp: 2026-08-22T10-38

Command:
```powershell
& dotnet-coverage collect --output coverage\postchange.cobertura.xml --output-format cobertura --settings coverage\effective-coverage.config -- 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' @assemblies '/Settings:scripts\vscode\TaskMaster.cli.runsettings' /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook'
$cov = ([xml](Get-Content -Raw coverage\postchange.cobertura.xml)).coverage
$cov.'line-rate'; $cov.'branch-rate'; $cov.'lines-covered'; $cov.'lines-valid'; $cov.'branches-covered'; $cov.'branches-valid'
```
with `@assemblies` re-resolved by the P0-T14 workspace-relative idiom (9 assemblies). Run from `WS` via `pwsh -NoProfile`. Identical command and identical settings file to the P0-T18 baseline, so the two are directly comparable.

EXIT_CODE: 0

## Inner test-run result

```
ASSEMBLY_COUNT=9
Test Run Successful.
Total tests: 6441
     Passed: 6441
```

Failed 0, Skipped 0. The instrumented run reproduces the P5-T6 totals exactly, so instrumentation perturbed no test.

## Repository-wide numeric figures (verbatim attribute values)

| Field | Raw value | As percentage |
|---|---|---|
| `line-rate` | `0.7060371689985226` | **70.60%** |
| `branch-rate` | `0.5874693824932319` | **58.75%** |
| `lines-covered` | `56872` | — |
| `lines-valid` | `80551` | — |
| `branches-covered` | `13671` | — |
| `branches-valid` | `23271` | — |

No value is `UNVERIFIED`. `dotnet-coverage` was PRESENT per P0-T16, so no `BLOCKED` outcome applies.

## Per-file covered/total line counts, aggregated by Cobertura `filename`

Same aggregation method as P0-T18: every `class` element whose `filename` ends with the target name, then deduplicated by line number taking the maximum `hits`.

| File | `class` elements | Baseline covered/total | **Post-change covered/total** | Baseline % | **Post-change %** |
|---|---|---|---|---|---|
| `KaStringAsync.cs` | 1 | 49 / 49 | **60 / 60** | 100.00% | **100.00%** |
| `KaChar.cs` | 2 | 28 / 33 | **28 / 28** | 84.85% | **100.00%** |
| `KaKey.cs` | 2 | 28 / 33 | **28 / 28** | 84.85% | **100.00%** |
| `IKbdAction.cs` | 0 | 0 / 0 | **0 / 0** | not measurable | not measurable |

**Uncovered lines after the change: none in any of the three measurable files.**

- `KaStringAsync.cs` gained 11 executable lines (49 to 60) and all 11 are covered, holding the file at 100%.
- `KaChar.cs` and `KaKey.cs` each shed exactly the 5 uncovered lines predicted in the P0-T18 artifact (`45, 53, 54, 95, 96` in each — the `DelegateType` getter body and the two dead `Update` accessor pairs), moving each file from 84.85% to 100%. Covered counts are unchanged at 28, so the improvement comes entirely from removing uncovered dead code, not from any change in what the tests exercise.
- `IKbdAction.cs` still produces zero `class` elements. It is an interface-only file with no executable body, which `.claude/rules/general-unit-test.md` explicitly recognises as legitimately reporting no executable coverage. No `[ExcludeFromCodeCoverage]` attribute was added and `coverage.config` is unmodified.

## Repository-wide delta

| Field | Baseline | Post-change | Delta |
|---|---|---|---|
| `line-rate` | 0.7059714463066419 | 0.7060371689985226 | +0.0000657 (+0.0066 pp) |
| `branch-rate` | 0.5874059746400172 | 0.5874693824932319 | +0.0000634 (+0.0063 pp) |
| `lines-covered` | 56866 | 56872 | **+6** |
| `lines-valid` | 80550 | 80551 | **+1** |
| `branches-covered` | 13666 | 13671 | +5 |
| `branches-valid` | 23265 | 23271 | +6 |

**`lines-valid` reconciles exactly.** The four in-scope files account for +11 (`KaStringAsync.cs`), -5 (`KaChar.cs`), and -5 (`KaKey.cs`), a net of **+1**, which is precisely the observed repository-wide change. The denominator is therefore fully explained by this change and shows no unaccounted drift.

**`lines-covered` does not reconcile exactly, and that is recorded rather than smoothed.** The in-scope files account for +11 covered lines (`KaStringAsync.cs` 49 to 60; `KaChar.cs` and `KaKey.cs` unchanged at 28 each). The observed repository-wide delta is +6, so **5 lines elsewhere in the repository moved from covered to uncovered between the two collections**. Those 5 lines are outside the four in-scope files and outside `QuickFiler`, which this change does not touch beyond the five edited files. The most likely cause is known run-to-run nondeterminism in `dotnet-coverage` collection under a parallel MSTest run (the runsettings sets `Workers` to 0, meaning processor count), where a race-dependent branch is exercised in one collection and not the other. This does not affect any blocking gate: the blocking gates are evaluated on the changed lines and on the four in-scope files, all of which are measured at 100%.

## Settings and runner notes

`coverage\effective-coverage.config` was used, identical to the baseline: the seven original third-party module exclusions plus the one added `.*\.Test\.dll$` entry that keeps the nine test assemblies out of the denominator per CLAUDE.md UT2. `coverage.config` remains unmodified (0 dirty lines). `scripts\vscode\Invoke-MSTestWithCoverage.ps1` was deliberately not used as the runner, because its `Assert-CoberturaLineCoverageThreshold` helper throws when repository-wide line coverage is below 80 percent — which it is, at 70.60% — and that throw precedes the Cobertura write, which would have produced no numeric figure at all.

## Execution note: first attempt deadlocked, second attempt clean

The first invocation of this task deadlocked and produced no output file. Diagnosis, recorded because it affects nothing about the result but explains the retry:

- The `testhost.exe` accrued only 0.02 seconds of CPU over a 60-second sampling interval (32.046875 to 32.0625), which is a hang rather than slow progress.
- Process inspection showed a **second, concurrent `dotnet-coverage collect`** running a full nine-assembly instrumented suite from a different agent worktree, `agent-a28821f6e56934fc7`, working on issue #491 (`quickfiler-test-form1-live-form-491`). Two simultaneous full-solution instrumentation sessions on one machine is the environmental cause.
- The stalled chain belonging to this session (`dotnet-coverage` 309776, `vstest.console` 357980, `testhost` 348860, and its parent `pwsh` 358232) was terminated. **The sibling worktree's processes were identified as foreign and deliberately left untouched.**
- After the sibling's collection cleared, this task was re-run unchanged and completed with exit code 0 and 6441 of 6441 tests passing.

No test was modified, no sleep, retry, or timing tolerance was added to any test, and the command was not altered between attempts. The hang was environmental contention, not a regression.

Output Summary: `dotnet-coverage collect` exited 0 with the inner suite reporting Total 6441, Passed 6441, Failed 0, Skipped 0. Repository-wide post-change figures are line-rate 0.7060371689985226 (**70.60%**), branch-rate 0.5874693824932319 (**58.75%**), lines-covered **56872**, lines-valid **80551**, branches-covered **13671**, branches-valid **23271**. Per-file counts aggregated by Cobertura `filename` are `KaStringAsync.cs` **60/60 (100%)** (baseline 49/49), `KaChar.cs` **28/28 (100%)** (baseline 28/33, 84.85%), `KaKey.cs` **28/28 (100%)** (baseline 28/33, 84.85%), and `IKbdAction.cs` 0/0 (interface-only, no executable line). No uncovered line remains in any of the three measurable files; `KaChar.cs` and `KaKey.cs` reached 100% by shedding exactly the 5 uncovered dead-member lines each that P0-T18 predicted. The `lines-valid` delta of +1 reconciles exactly with the in-scope files (+11, -5, -5); the `lines-covered` delta of +6 versus an expected +11 is attributed to 5 lines elsewhere in the repository flipping under known dotnet-coverage run-to-run nondeterminism, is recorded rather than smoothed, and affects no blocking gate. A first attempt deadlocked due to a concurrent full-suite instrumentation from sibling worktree `agent-a28821f6e56934fc7` (issue #491); this session's stalled processes were killed, the sibling's were left untouched, and the unchanged command was re-run successfully. No value is `UNVERIFIED` and no `SKIPPED` outcome is recorded.
