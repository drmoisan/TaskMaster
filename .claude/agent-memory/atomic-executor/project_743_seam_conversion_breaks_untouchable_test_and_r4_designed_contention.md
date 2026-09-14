---
name: 743-seam-conversion-breaks-untouchable-test-and-r4-designed-contention
description: Issue #743 preflight (2026-09-12) - converting ViewerSetup.cs line 371 to _uiDispatcher without null tolerance NREs an existing 498-line test off the Write Set; the fixture's R4 test deliberately contends the TransactionGate so a serial "contended count 0" rule is non-discriminating; the coverage runner throws at line 236 before post-processing on ANY failing test
metadata:
  type: project
---

Three plan-shape facts found while preflighting the #743 plan (`plan.2026-09-12T13-23.md`).

1. **`_uiDispatcher` is null under the parameterless `HarnessController`.** `QfcItemController.TestSupport.cs` 162-165 records it; the field is assigned only at Initialization.cs 59 / 391. The existing test `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` (ViewerSetupTests.cs 309-344, file at 498 lines, NOT in the Write Set) injects only `_itemViewer` + `_globals`. Any conversion of `_itemViewer.UiDispatcher.InvokeAsync` (ViewerSetup.cs 371) to `_uiDispatcher.InvokeAsync` with no null tolerance makes that test NRE and the whole-assembly `failed=0` gate unsatisfiable. Spec 6.2's second risk bullet REQUIRES "the same null tolerance the existing sites have"; the reference shape is `NotifyMoveFailure` at MailActions.cs 35-46 (local copy, direct call when null).

2. **R4 (`Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, FixtureTests.cs 204-262) starts a second transaction while the first is held.** In the SERIAL regime it contributes 0 or 1 "contended acquisition" per run nondeterministically, so any decision rule of the form "serial contended count > 0 => leak" is polluted by design. Exclude it by `FullyQualifiedName!~` from the measurement run and say why. R4 is also the #823 known-intermittent (1 failure in 4 parallel-regime runs per the flake-watch log), so 62-run streaks over its class will not stay clean.

3. **`Invoke-MSTestWithCoverage.ps1` throws `MSTest with coverage failed with exit code` at line 236 on ANY non-zero vstest exit**, which is BEFORE `ConvertTo-KoverageCoberturaXml` at 342. A single flaky test leaves the raw document with absolute filenames, and a per-file XPath on workspace-relative backslash filenames returns zero class nodes. The 80%-threshold throw at 344 is the only non-zero path that leaves the document post-processed. Recovery: dot-source `scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` (function at line 406) and post-process manually. The runner has no exclusion parameter, so `-SearchRoot QuickFiler.Test` is the way to dodge the UtilitiesCS shell-icon hang.

**How to apply:** when a plan converts a viewer-owned marshal to the injected seam, grep the test project for existing tests of that member that build the controller through the harness and check whether they inject `_uiDispatcher`; when a plan builds a gate observable from `TransactionGate` counters, read every `BeginTransactionAsync` caller for nested/concurrent acquisitions first.
