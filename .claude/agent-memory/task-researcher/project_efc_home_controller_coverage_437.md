---
name: efc-home-controller-coverage-437
description: "Issue #437 (epic #136 child F8): EfcHomeController family is already ~90% line-covered and fully seamed; the real gaps are branches/contracts plus an order-dependent default-factory coverage hazard"
metadata:
  type: project
---

The `EfcHomeController` partial family (`QuickFiler/Controllers/EfcHomeController*.cs` plus
`EfcHomeControllerDependencies*.cs`) was already refactored to a full delegate-seam design before
epic #136 child F8 (issue #437) was scoped. Static research on 2026-08-07 estimated
`EfcHomeController.cs` at ~90-91% line coverage and `EfcHomeController.Timing.cs` at ~100% before any
new test was written.

Two of the six files were later *measured* (not estimated) from a committed Cobertura artifact:
`EfcHomeController.ExecuteMoves.cs` = 93.16% line / 83.33% branch with 8 uncovered lines
(`ExecuteMovesAsync`'s try/finally at 39-45, and `HandleMoveResult`'s default metrics arm at 141);
`EfcHomeController.Metrics.cs` = 97.59% line with exactly ONE uncovered line (23, the
`_stopWatch.Elapsed.Seconds` delegation). See [[quickfiler-percoverage-epic-136]] for the technique.

**Why:** ~2,502 lines of existing tests across seven `EfcHomeController*Tests.cs` files already
exercise every construction path through the `EfcHomeControllerDependencies` delegate bundle. The
F8 spec assumed coverage was "unmeasured," which is true, but the assumption that it was *low* was
wrong.

**How to apply:** when planning coverage work on a QuickFiler controller, read the existing test
files first and estimate statement-level coverage before assuming bulk test authoring is needed.
For this family the remaining value is branch/contract scenarios, not line count. Three specific
non-obvious findings worth re-checking rather than rediscovering:

1. `EfcHomeController.Timing.cs` **reads no clock at all** despite its name — it is a
   diagnostic-logging helper. Do not propose an injected-clock seam for it.
2. `_defaultDependenciesFactory` (L24-25) and `ResetDefaultDependenciesFactory` (L37) install two
   *separate* lambda instances with identical bodies, so exactly one body is covered per run
   depending on test-class execution order. Per-file coverage for that file is not reproducible
   until both sites share one static readonly default.
3. The `Run()`/`RunAsync()` condition short-circuits on non-null `Mail`, so the entire
   Finder (`InitType.HasFlag(Find)` without mail) show-the-viewer arm has never been executed by any
   test.

Related: [[qfc-item-controller-227-r2-denial]], [[feedback-exemption-audit-check-proven-techniques]].
