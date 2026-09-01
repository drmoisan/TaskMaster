---
name: project-663-qfc-alt-chord-plan-seams
description: "#663 QFC ProcessCmdKey Alt over-claim plan seams: defect-preserving seam gives a runtime red without a dossier; `Task \"Csc\"` only exists at detailed verbosity; no TRX means named-test outcomes must be derived from total-count + failed-name list"
metadata:
  type: project
---

Plan seams for the `full-bug` delivery of issue #663 (`plan.2026-08-31T20-16.md` in feature
`qfc-twin-processcmdkey-alt-chord-over-claim-663`). Three files change, no new file, no csproj edit.

**Why:** `QfcFormViewer.ProcessCmdKey` claims every Alt chord; the fix adds
`QfcFormKeyHandler.ClaimsAltChord` and routes the guard through it.

**How to apply:**

- **A defect-preserving seam converts a compile-red into a runtime red.** New tests citing a
  not-yet-existing member redden the whole test assembly at compile time and yield no per-test
  evidence. Land `ClaimsAltChord` first in a form exactly equivalent to the old guard
  (`handler is not null && keyData.HasFlag(Keys.Alt)`), route the caller through it, then add the
  tests: exactly 3 of 7 fail at runtime. Phase 3 adds the `keyData & Keys.KeyCode` mask. No
  fail-before exception dossier is needed. Same pattern as #468's "sign-defect seam must land
  carrying the defect".
- **`Task "Csc"` is a detailed-verbosity literal.** MSBuild does not print it at default (normal)
  verbosity, so an acceptance demanding it from a plain console run is unsatisfiable. Attach
  `"/flp:LogFile=coverage\<name>.msbuild.log;Verbosity=detailed"`, grep that file, record the count,
  then delete the log (`.gitignore:144` is `coverage/*`).
- **Neither test wrapper emits a TRX.** `Invoke-MSTest.ps1` has no `/Logger` parameter, and
  vstest prints failing test names only. "Test X passed" must therefore be derived, once, from three
  observations: X is a declared `[TestMethod]` in a compiled file, X is absent from the run's failed
  list, and `Total tests:` equals BASELINE_TOTAL + N. State the derivation once and reuse it.
- **MSBuild resolved inline through vswhere**, not assumed on PATH, inside a single
  `pwsh -NoProfile -Command '...'` with outer single quotes and inner doubles.
- **Scope the mutating csharpier pass to the changed paths** (one invocation per file) and use the
  repo-wide `check .` as the read-only gate. `.github/workflows/_format-check.yml:41` runs a
  CSharpier check on every PR, so `origin/main` is format-clean and exit 0 is a reachable gate.
- **Analyzer versions agree solution-wide on this tree**: Meziantou.Analyzer 3.0.194 and
  Roslynator.Analyzers 5.0.0 in every csproj `<Analyzer Include>` and every `packages.config`. The
  #511-era skew (3.0.156/4.16.0 vs 3.0.174/4.16.1) is gone. Still re-derive with two independently
  shaped greps; a mismatch is `error CS0006`, not a warning.
- **`origin/main` @ `2b85134b` line citations, re-derived 2026-08-31:** `QfcFormKeyHandler.cs` is 20
  lines with `IsAltKeyCommand` at :18; `QfcFormViewer.cs` is 296 lines with `ProcessCmdKey` at 56-73,
  the guard at 58-61, the retained unused locals at 64-67 and the parameterless dispatch at :68;
  `QfcFormKeyHandlerTests.cs` is 67 lines with four `[TestMethod]`s at :16, :29, :42, :55, compiled
  through `QuickFiler.Test.csproj:151`.
- **Removing the last compiled consumer of `IsAltKeyCommand` is a plan-time analyzer risk.** Gate the
  seam build on "the warning set naming the three files equals the Phase 0 baseline set", and forbid
  deleting `IsAltKeyCommand` (AC-8 requires it unchanged).

Related: [[declaration-only-seam-task-for-fail-before]], [[expect-fail-needs-a-synchronous-seam]],
[[reference-invoke-mstest-single-searchroot-defect]], [[project-494-threshold-reconciliation-plan-seams]],
[[csharpier-repowide-format-breaks-zero-diff-acs]].
