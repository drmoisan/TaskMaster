# Baseline Solution Analyzer Build — Remediation Cycle 1

- Task: `[P0-T7]`
- Issue: #418
- Branch / HEAD: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-26 (UTC)

Command:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

EXIT_CODE: 0

Summary lines:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.06
```

- Error count: **0**
- Warning count: **5**

## Incrementality of this run

`Invoke-VSBuild.ps1` hardcodes MSBuild target `Build`, and the tree is up to date at `ea106111`. This
run executed **18 `CoreCompile` targets, all 18 skipped**, and **0 `csc.exe` invocations**. Measured by:

```
grep -c "CoreCompile:"                  -> 18
grep -c 'Skipping target "CoreCompile"'  -> 18
grep -c "csc.exe"                        -> 0
```

Consequence: any diagnostic emitted **only at `CoreCompile` time** cannot appear in this run. That is
why `CS2002` is absent here but present in the non-vacuous comparison artifact (see below). This is the
same timestamp-based up-to-date behavior recorded as policy-audit gap G-3 for the nullable gate.

## Per-code per-project diagnostic inventory

| Count | Severity | Code | Emitting project | Text |
|---|---|---|---|---|
| 1 | warning | (no code) | `UtilitiesCS/UtilitiesCS.csproj` | `System.Reactive.PackagesConfigCheck.targets(31,5)`: "The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later." |
| 1 | warning | (no code) | `ToDoModel/ToDoModel.csproj` | same |
| 1 | warning | (no code) | `QuickFiler/QuickFiler.csproj` | same |
| 1 | warning | (no code) | `TaskMaster/TaskMaster.csproj` | same |
| 1 | warning | (no code) | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | same |
| 0 | error | — | — | — |

- Distinct diagnostic codes: **none** (all five warnings are code-less MSBuild target warnings).
- `SVGControl` and `SVGControl.Test` emitted **zero** warnings and zero errors.
- Zero `MSB3277`, zero `MSB3245`, zero `CS0234`, zero `CS2002`, zero `MSB3061`.

## Comparison against `evidence/qa-gates/analyzer-build.2026-08-04T14-36.md`

**Does the inventory match exactly? NO — and the difference is fully explained by incrementality, not
by any change in the tree.**

| Metric | `qa-gates/analyzer-build.2026-08-04T14-36.md` | This run (`[P0-T7]`) | Cause of difference |
|---|---|---|---|
| EXIT_CODE | 0 | 0 | — |
| Errors | 0 | 0 | — |
| Warnings | 6 | 5 | one fewer |
| `CoreCompile` targets executed | 36 | 0 (18 skipped) | that run followed source edits; this run is on an untouched, up-to-date tree |
| Code-less System.Reactive warnings | 5 | 5 | identical: `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test` |
| `CS2002` | 1, in `UtilitiesCS.Test.csproj` | 0 | `CS2002` is emitted by `csc` at `CoreCompile` time; no project recompiled here |

`CS2002` text as recorded in the cited artifact: `Source file
'...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` — a
pre-existing duplicate `<Compile>` include in `UtilitiesCS.Test.csproj`, out of scope for issue #418.

## Comparison basis for `[P2-T4]` (stated explicitly to keep that gate sound)

`[P2-T4]` compares its inventory "line by line against
`evidence/remediation-baseline/analyzer-build.2026-08-05T01-50.md`" and treats anything absent as newly
introduced. Because this run is incrementally vacuous, the pre-existing diagnostic set is the **union**
of the two rows below, and `[P2-T4]` must be read against that union:

| Pre-existing diagnostic | Emitting project | Source of record |
|---|---|---|
| code-less System.Reactive `packages.config` warning x5 | `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test` | this artifact (measured) |
| `CS2002` duplicate `<Compile>` x1 | `UtilitiesCS.Test` | `evidence/qa-gates/analyzer-build.2026-08-04T14-36.md`, measured at 36-`CoreCompile` scope on this same HEAD |

Neither is attributable to this cycle and neither is inside the Scope Lock. A reappearance of `CS2002`
at `[P2-T4]` is therefore **not** a newly introduced diagnostic; it is the pre-existing condition
becoming visible again once `CoreCompile` runs.

## Output Summary

`EXIT_CODE: 0`, **0 errors, 5 warnings**, all five the code-less System.Reactive `packages.config`
warning across `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, and `UtilitiesCS.Test`. The run
is incrementally vacuous (18/18 `CoreCompile` targets skipped, 0 `csc.exe` invocations), so it does not
match `evidence/qa-gates/analyzer-build.2026-08-04T14-36.md` exactly: that non-vacuous run additionally
carried 1 pre-existing `CS2002` in `UtilitiesCS.Test`. The union of the two sets is the pre-existing
baseline against which `[P2-T4]` must be assessed. `SVGControl` and `SVGControl.Test` are clean.
