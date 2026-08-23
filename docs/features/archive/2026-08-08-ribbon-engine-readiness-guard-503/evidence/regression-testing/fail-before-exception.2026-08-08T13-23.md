# Fail-Before Exception Dossier — Issue #503 (P1-T3)

Timestamp: 2026-08-08T13-23

## WhyFailingRunImpossible

A runtime (non-compile) red is structurally impossible for #503 because the defective production path lives entirely inside `[ExcludeFromCodeCoverage]`, COM-bound `RibbonViewer` handlers that require a live `Office.IRibbonControl` supplied by a running Outlook host, and those handlers route the engine dereference through `RibbonController.SB` / `RibbonController.Triage`, whose getters install a real `WindowsFormsSynchronizationContext` on the calling thread as a side effect and are therefore prohibited by the spec's `## Test Strategy` ("Tests must not reach the readiness decision through `RibbonController.SB`/`Triage`/`TriageAsync`"). The unit under test, `EngineGatedCommandRunner`, does not exist at the merge-base, so the only observable red that a deterministic automated gate can produce is a compile-time red.

## Search record

- SearchScope: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\evidence\regression-testing\`
- SearchPatterns: `fail-before-*.md`
- SearchResult: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\evidence\regression-testing\fail-before-503.2026-08-08T13-22.md`

## Alternative proof

The P1-T2 artifact `fail-before-503.2026-08-08T13-22.md` records a non-zero build exit (`EXIT_CODE: 1`) whose entire error set is four `CS0246` diagnostics naming `EngineReadinessGate` and `EngineGatedCommandRunner`, sourced from `TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs`. That is an absence-of-implementation proof: the two AC11 regression tests are authored, wired into `TaskMaster.Test.csproj`, and cannot compile at the merge-base precisely because the guard they exercise does not exist. The corresponding green is proven by P2-T6 (`pass-after-503.<TS>.md`), which runs the same two test methods to `Passed` after the four decision types are created.

EXIT_CODE: 0
