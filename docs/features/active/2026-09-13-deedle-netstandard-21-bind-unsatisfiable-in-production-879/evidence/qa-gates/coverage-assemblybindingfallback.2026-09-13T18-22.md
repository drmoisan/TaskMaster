# Phase 5 — Per-File Line Coverage, `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`

Recorded by `[P5-T9]`.

Timestamp: 2026-09-14T11-55

Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>
$x = [xml](Get-Content -LiteralPath "coverage/coverage.cobertura.xml" -Raw)
$rows = @($x.SelectNodes("//class")).Where({ $_.filename -and $_.filename.Replace("\","/").EndsWith("UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs") })
$valid = 0
$covered = 0
foreach ($r in $rows) { foreach ($l in @($r.lines.line)) { $valid = $valid + 1
if ([int]$l.hits -gt 0) { $covered = $covered + 1 } } }
Write-Output ("ASSEMBLYBINDINGFALLBACK_CLASS_ROWS=" + $rows.Count)
Write-Output ("ASSEMBLYBINDINGFALLBACK_LINES_VALID=" + $valid)
Write-Output ("ASSEMBLYBINDINGFALLBACK_LINES_COVERED=" + $covered)'`

EXIT_CODE: 0

Output Summary:

```
ASSEMBLYBINDINGFALLBACK_CLASS_ROWS=1
ASSEMBLYBINDINGFALLBACK_LINES_VALID=201
ASSEMBLYBINDINGFALLBACK_LINES_COVERED=166
COMPUTED_LINE_PERCENT=82.59
```

## Acceptance Condition: NOT MET — BLOCKED

Of the three acceptance conditions, two pass and one fails:

- `ASSEMBLYBINDINGFALLBACK_CLASS_ROWS` is at least 1 — MET, observed as 1.
- `ASSEMBLYBINDINGFALLBACK_LINES_VALID` is greater than 0 — MET, observed as 201.
- the computed percentage is at least 90 — **NOT MET**, observed as 82.59.

166 of 201 instrumented lines are covered, which is 82.59 percent against a floor of 90. The
shortfall is 35 uncovered lines; reaching 90 percent requires at least 181 covered lines, so
at least 15 of those 35 must be covered.

This task is therefore left unchecked and the verdict is BLOCKED, per this plan's fail-closed
evidence rule. The gate was not weakened, no alternative measurement was substituted, and the
command was run exactly as the plan pins it.

## The Measurement Is Genuine, Not An Artefact

Four confounders were checked before recording this verdict, because a false BLOCKED costs a
round as surely as a false PASS:

1. **The class-row aggregation is complete.** A search for every `class` row whose `filename`
   contains `AssemblyBindingFallback` returns exactly 1 row,
   `UtilitiesCS.Bootstrap.AssemblyBindingFallback` at
   `UtilitiesCS\Bootstrap\AssemblyBindingFallback.cs`, carrying all 201 lines and a
   `line-rate` of `0.825871` that agrees with the computed 166/201. The nested
   `AssemblyBindingLadder` type is folded into that same row rather than emitted separately,
   so the aggregation over rows is not missing a second row.
2. **The tests that drive this file ran, and passed.** The `[P5-T7]` TRX carries all eleven
   `AssemblyBindingFallbackTests` methods and all nine `NetstandardBindChildDomainTests`
   methods, and the run recorded zero failures. The figure is not depressed by a test that
   failed to execute or to discover.
3. **The document is the post-processed one.** `Cobertura Document State: POSTPROCESSED`,
   confirmed by a `<sources>` hit. This is the document the plan's command names.
4. **The percentage is a line percentage, not a branch percentage**, computed by the plan's
   own aggregation over `hits` and not read from any rolled-up attribute.

## The 35 Uncovered Lines, By Construct

| Lines | Construct | Why the existing tests do not reach it |
|---|---|---|
| 83, 84, 87, 88 | `Install()` boundary `catch` | No test forces `Install()` to throw. |
| 105, 106 | `Resolve` guard: `requested is null` | No test passes a null `AssemblyName` to the outer static `Resolve`. |
| 111, 112 | `Resolve` guard: empty simple name | No test passes an `AssemblyName` with an empty `Name` to the outer static `Resolve`. |
| 157, 158 | `OnAssemblyResolve` guard: null or empty `args.Name` | The eleventh test drives the handler through the real binder, which never supplies a null or empty name. |
| 170, 171, 174, 175 | `OnAssemblyResolve` boundary `catch` | No test makes the handler throw. |
| 229, 230 | `AssemblyBindingLadder.Resolve` guard: `requested is null` | The ten delegate-driven tests always supply a non-null identity. |
| 322, 323 | Rung 3 `!_fileExists(path)` early return | The rung-3 test supplies a `_fileExists` that returns true. |
| 328, 329, 331, 332 | Rung 3 rung-local `catch` | `Resolve_WhenARungThrows_AbsorbsAndContinuesToTheNextRung` exercises a throwing rung, but not this rung's catch. |
| 345, 346 | Rung 4 guard: empty simple name | Not exercised. |
| 355, 356 | Rung 4 guard: empty assembly directory | Not exercised. |
| 365 | Rung 4 success path `_loadFromPath(path)` | No test drives rung 4 to a successful load. |
| 367, 368, 370, 371 | Rung 4 rung-local `catch` | Not exercised. |
| 434, 435, 439, 440 | Two `return false` guards in the facade-identity predicate | Not exercised. |

The pattern is consistent: the uncovered set is guard clauses, rung-local and boundary
`catch` blocks, and the rung-4 path. Rung 4 (`FromProbeDirectory`) has no delegate-driven
test of its own; rungs 1 to 3 do.

## Why This Executor Did Not Repair It

Closing this gap requires new test methods. That is not within the Phase 5 and Phase 6 range
this executor was given, and three separate constraints make it a plan change rather than an
executor micro-action:

- No task in Phase 5 or Phase 6 authorises editing
  `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs`. The Phase 5 tasks are
  toolchain, measurement and projection tasks only.
- `[P2-T3]` is checked complete, and Revision R6 relabelled its acceptance conditions as
  standing guards pinning **exactly eleven** `[TestMethod]` occurrences in that file. A
  twelfth method would falsify a standing guard of a completed task.
- `[P4-T2]`'s passed-test floor is pinned at 11 for the same file.

The repo policy alternative — lowering the floor or selecting a more favourable denominator —
is explicitly prohibited: a Phase 5 gate is fixed by fixing the code or by reporting blocked,
never by weakening the gate.

## Note On The Governing Threshold

The 90 percent figure is this plan's own new-module obligation, recorded at `[P0-T9]` from
`CLAUDE.md`: new modules target `>= 90%`. It is not issue #891's document-level 80 percent
runner assertion, which passed in this run, and it is not the repository-wide 80 percent floor
on the testable denominator. The repository-wide first-party rate this run reported is 85.83
percent. This artifact records a per-file shortfall against the new-module obligation only.
