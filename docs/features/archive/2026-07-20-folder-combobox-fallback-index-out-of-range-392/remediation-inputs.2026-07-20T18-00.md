# Remediation Inputs — folder-combobox-fallback-index-out-of-range (Issue #392)

- Timestamp: 2026-07-20T18-00
- Entry cycle: 1
- Source audit: `policy-audit.2026-07-20T18-00.md` Section 5, `feature-audit.2026-07-20T18-00.md` AC-5 caveat
- Trigger: mandatory C# coverage checks below the uniform 85% line / 75% branch floor
  (`.claude/rules/quality-tiers.md`), unratified.

## Why remediation is triggered

All five acceptance criteria for Issue #392 pass. The C# toolchain passes (format, analyzers, tests;
nullable reproduces a dispositioned pre-existing vendored-project condition). The fix's own new/changed
lines are 100% line-covered and both new logical branches are test-exercised. However, three coverage
scopes read below the repository's mandatory 85%/75% floor, and none carries an existing maintainer
ratification (unlike the `StoreWrapper` branch-floor exception ratified for issue #328):

1. **`QfcItemController.FolderHandling.cs` class-level branch coverage: 73.81%** (floor 75%, gap
   1.19 points). Baseline was 71.05% (already below floor); this fix improved it, did not regress it.
2. **`QuickFiler` package-wide coverage: 73.68% line / 64.62% branch** (floor 85%/75%). Baseline was
   73.67%/64.53% — virtually unchanged; this is broad, pre-existing under-coverage across the
   `QuickFiler` assembly's WinForms/UI surface, unrelated to the two lines this bug fix touches.
3. **Canonical repo-wide artifact (`artifacts/csharp/coverage.xml`): 16.25% line / 13.60% branch**
   (raw six-package aggregate) — distorted because only `QuickFiler.Test` ran in this local
   collection; `Tags`, `TaskVisualization`, and `ToDoModel` were not exercised by their own suites here.
   Re-scoping to the one actually-instrumented package (`QuickFiler`) still yields item 2 above, which
   also fails the floor (unlike a prior feature review in this repo where re-scoping cleared the
   floor).

## Enumerated fix list

| # | Item | File(s) | Expected outcome | Verification command |
|---|---|---|---|---|
| R1 | Close the marginal class-level branch-coverage gap (item 1) | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` | Add 1-2 targeted MSTest tests exercising an existing, currently-uncovered branch already inside `QfcItemController.FolderHandling.cs` (not a new file, not a new production code path) to raise class-level branch coverage from 73.81% to >= 75%, with zero regression on any currently-covered line/branch. | `dotnet-coverage collect -f cobertura -s coverage-exclude-deedle.xml -o <ts>-coverage.cobertura.xml -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation`, then recompute class-level `line-rate`/`branch-rate` for `QfcItemController.FolderHandling.cs` and diff against the 91.89%/73.81% baseline captured in `policy-audit.2026-07-20T18-00.md` Section 5.2. |
| R2 | Obtain an explicit maintainer disposition decision for the `QuickFiler` package-wide and canonical repo-wide coverage gaps (items 2-3) | none (documentation/ratification only) | Either (a) ratify the pre-existing `QuickFiler` package coverage gap as a documented exception analogous to the `StoreWrapper` precedent (issue #328), recorded via a `human_interaction` / `orchestrator-state` scope-change entry, with the requirement that the true all-first-party repo-wide figure is measured by the PR CI full-suite run; or (b) schedule a dedicated, separately-scoped coverage-uplift task for `QuickFiler` (outside this minor-audit bug fix's Scope-Lock). Do not attempt to close this gap inside this remediation cycle by adding broad, unrelated test coverage across `QuickFiler`'s WinForms/UI surface — that is disproportionate scope creep for a minor-audit fix. | N/A (decision/documentation task, not a command). |

## Do-not-do list

- Do not modify `.claude/rules/*` or any policy document to weaken the 85%/75% coverage floor.
- Do not add a `coverage.config` or `.csproj` coverage-exclude for `QuickFiler` or any of its classes
  to artificially raise the measured percentage — no production source file may be excluded from
  coverage measurement.
- Do not expand the fix's Scope-Lock beyond `QfcItemController.FolderHandling.cs` and its test file to
  chase the package-wide `QuickFiler` floor (item 2) inside this cycle; that is R2's job, not R1's.
- Do not silently mark AC-5 or any policy-audit coverage row as PASS without the corresponding
  remediation task closing the gap or a recorded maintainer ratification.
- Do not weaken or delete the two new regression tests, the pre-existing re-verified tests, or the
  `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection` test that documents the pre-existing,
  unrelated empty-array gap (code-review Finding CR-3/Info).

## Pointer to audit artifacts

- `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/policy-audit.2026-07-20T18-00.md`
  (Section 5, coverage findings; Appendix A, coverage verdict checklist)
- `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/code-review.2026-07-20T18-00.md`
  (Findings CR-1, CR-2, CR-3)
- `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/feature-audit.2026-07-20T18-00.md`
  (AC-5 coverage caveat)
- Target remediation plan: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/remediation-plan.2026-07-20T18-00.md`
  (to be authored/refined by `atomic-planner` per `remediation-handoff-atomic-planner`; this
  feature-review pass creates the target plan file with the R1/R2 shape above as the starting point).
