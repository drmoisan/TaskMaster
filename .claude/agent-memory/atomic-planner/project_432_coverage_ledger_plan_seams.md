---
name: project-432-coverage-ledger-plan-seams
description: Epic #136 F1 (#432) plan derivations — the 121-file classification group arithmetic, the entry-function-returns-exit-code seam, and why the ledger's classification and disposition axes must stay separate
metadata:
  type: project
---

Planning seams for `quickfiler-coverage-ledger` (#432, epic #136 child F1). Plan at `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/plan.2026-08-07T20-41.md` (12 phases, 163 tasks after the 2026-08-07 preflight revision).

**Why:** The research artifact enumerates the 121 compiled files and 40 attribute usages but never partitions them into disjoint classification groups; three planning derivations below were computed during planning and are not recoverable by re-reading the source documents. A revision pass that recomputes them from scratch will burn a cycle.

**How to apply:**

1. **Disjoint classification partition of the 121 compiled files** (verified to sum exactly, no overlap):
   `7 generated designer + 3 Properties + 23 interface-only + 2 zero-executable-line (QfEnums.cs, cInfoMail.cs) + 20 type-level-suppressed-not-already-designer + 40 Controllers residual + 11 (10 Helper Classes + 1 Interfaces\MailItemActionsAdapter.cs) + 15 Viewers residual = 121`.
   The 24-file suppressed set overlaps the 7 designer files by exactly 4 (ItemViewer, EfcViewer, QfcFormViewer, QfcItemViewerExpanded designers), which is why the suppressed group is 20, not 24. `Interfaces\MailItemActionsAdapter.cs` is NOT interface-only (research says "all 13 `Interfaces\I*.cs`", i.e. 13 of 14). Inherited disposition rows total 11 (6 ItemViewer + 2 QfcDatamodel + 1 each for EfcViewer/QfcFormViewer/QfcItemViewerExpanded), not 24 minus 14.

2. **Classification and attribute disposition are orthogonal axes.** A file can be `ratified-exempt` / `generated-designer` while the type-level attribute that currently suppresses it is disposed `remove` (e.g. `ItemViewer.Designer.cs`). Plans that fold disposition into classification produce contradictions in the ledger and unsatisfiable Pester assertions. Phases record dispositions first, classification second.

3. **`Invoke-PerFileCoverageGate` must return `[pscustomobject] @{ ExitCode; ReportText }`, never call `exit`.** `spec.md` requires exit codes 0/1/2 asserted *against the entry function* in Pester; a function that calls `exit` terminates the test host. The dot-source-safe script guard does the `Write-Output`/`exit`. Related: [[reference_invoke_mstest_with_coverage_script]].

4. **Zero-executable-line files may never be classified `testable`.** They are absent from Cobertura entirely, so `testable` yields a permanent `NO DATA` failure no test can clear. Applies to the 23 interface-only files plus `QfEnums.cs`, `cInfoMail.cs`, `AssemblyInfo.cs`, `Resources.Designer.cs` (27 files).

5. **Viewers residual is 15 = 12 Breadcrumb\* + BayesianPerformanceViewer.cs + ItemViewerExpanded.cs + ToolStripMenuItemCb.cs.** There are exactly **12** compiled `Viewers\Breadcrumb*` files, not 13; `IBreadcrumbDropDownHost.cs`/`IBreadcrumbWebHost.cs` start with `I` and are interface-only, and `Controllers\BreadcrumbBridgeRouter.cs`/`Controllers\BreadcrumbOutboundQueue.cs` are Breadcrumb-named but live under `Controllers/`. Preflight caught a 14-vs-15 miscount here. Enumerate Breadcrumb file names explicitly rather than writing "the N Breadcrumb files". See [[enumerate-condition-outcomes-before-case-list]].

6. **The figure 33 from the epic manifest is refuted.** Verified: 40 usages / 21 compiled files / 14 type-level / 26 member-level / 24 fully-suppressed; `33 = 21 + 5 + 7` files-containing-the-string. 33 appears in deliverables only inside the reconciliation decomposition, never as a target. See [[research-claims-as-acceptance-clauses]].
