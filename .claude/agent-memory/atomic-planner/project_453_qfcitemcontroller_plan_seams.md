---
name: project-453-qfcitemcontroller-plan-seams
description: F10/#453 QfcItemController coverage — AC-8 caps de-exemption at 15 and overrides three research artifacts; FlagTasks ctor touches COM; per-file phase mandate
metadata:
  type: project
---

Planning epic #136 child F10 (`quickfiler-item-controller-coverage`, issue #453) surfaced four
non-obvious constraints that a naive read of the research artifacts gets wrong.

**Why:** three of the 13 research artifacts recommended de-exemptions that the corrected `spec.md`
forbids, and one recommended a test whose fixture touches live COM. Following research verbatim
would have produced a plan that violates its own acceptance criteria.

**How to apply:** when planning any child of epic #136 that touches a maintainer-ratified exemption
boundary, reconcile the research recommendations against the spec's AC set *before* writing tasks.

1. **AC-8 is a floor, not a target.** The exemption arithmetic is 19 − 3 dead members deleted − 1
   unratified resolved = **15, and no lower**. A count below 15 is a failure, not an improvement,
   because the remaining 15 are ratified under issue #227 (maintainer decision 2026-07-02). This
   overrides `file-QfcItemController.Initialization.md` Group B, `file-QfcItemController.ViewerSetup.md`
   Group D, and `file-QfcItemController.Navigation.md` NV-1/NV-2/NV-4/NV-5 — all four are
   de-exemption proposals and none is scheduled. Every affected file clears 80/75 without them.
2. **AC-4 ("removal lands in the same atomic task as its tests") is satisfied by ordering, not
   bundling.** Bundling 5 tests into one task breaks atomicity. The operative clause is "no per-file
   coverage measurement taken between tasks shows a file below either floor as a result of a
   de-exemption", which *mandates* tests-first-then-removal: removing `EnsureBreadcrumbPipeline`'s
   attribute before its tests drives `ViewerSetup.cs` to ~78% line. State the interpretation in the
   plan preamble.
3. **`TaskVisualization.FlagTasks`'s constructor is not test-safe.** `FlagTasks.cs:52` calls
   `globals.Ol.App.ActiveExplorer()` and `:56-61` can raise a `MessageBox`. Any research proposal
   that invokes the default `_flagTasksFactory` (e.g. Initialization A2) must be dropped, not
   scheduled — its `Run(bool)` at `:89` is also non-virtual, so Moq cannot intercept it.
4. **`Invoke-MSTestWithCoverage.ps1`'s emitted rates are unusable for gate decisions** (open issue
   #441). `MailActions.cs` emits `branch-rate="0.75"` against a true 72.73% — a false pass. Recompute
   from the class-level `<line>` children and commit both figures side by side.

See also [[coverage-gate-clr-invoked-private-members]], [[named-coverage-exception-verify-member-body]],
[[research-claims-as-acceptance-clauses]], [[plan-validator-phase-heading-constraint]].
