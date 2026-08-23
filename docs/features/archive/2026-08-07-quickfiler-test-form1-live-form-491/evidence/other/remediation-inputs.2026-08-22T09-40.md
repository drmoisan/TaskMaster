Timestamp: 2026-08-22T09-40

## Cycle 1 — inputs

### Finding (discovered by atomic-executor at [P1-T6]/[P3-T7], not by feature-review)

The plan's guard test `NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`
reflects over every `System.Windows.Forms.Form`-derived type declared in the executing
`QuickFiler.Test` assembly, exactly as designed and exactly as required by acceptance criterion 1
in `spec.md` ("No `Form`-derived type is compiled into `QuickFiler.Test`"). Both preflight research
and the plan itself searched only for the literal `Form1` and did not discover a second,
pre-existing, unrelated `Form`-derived type already compiled into the same assembly:

- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:243-250` declares a nested
  `public class QfcFormViewerDerived : QfcFormViewer`, where `QfcFormViewer` (production type,
  `QuickFiler/Viewers/QfcFormViewer.cs:18`) itself derives from `System.Windows.Forms.Form`.
- `git blame` dates this nested class to 2025-02-01 (further touched 2026-03-19 and by the
  issue #218 file-split commit `27ca7717`); it predates this plan and is unrelated to Item 1
  or Item 2 of issue #491.
- A repository-wide `grep -rn "QfcFormViewerDerived" --include="*.cs" .` returns only the two
  lines of its own declaration (the class name and its constructor name). It is never
  instantiated (`new QfcFormViewerDerived(...)` appears nowhere) and never referenced from any
  other file. It is dead code: a test-double shape (`new virtual Show()` / `new virtual
  WindowState`) apparently prepared for future Moq-style spying but never wired up.

Because the class is declared but never constructed, no unit-test run creates a visible window
because of it — the epic's actual acceptance signal ("no unit-test run may create a visible
window on the desktop") is not at risk from this type. However, the plan's literal, approved
guard test checks type declaration via reflection, not instantiation, and this dead declaration
makes that guard permanently red, blocking:

- AC1 (guard test proves no `Form`-derived type is compiled into the assembly)
- AC8 (vstest run with coverage/isolation/`LiveOutlook` filter completes with zero failing tests)
- AC9 (test-count/pass-count parity apart from the one new guard test)
- AC10 (post-change coverage >= baseline, both recorded as actual numbers — blocked because the
  coverage-capture harness aborts on any failing test before reaching Koverage post-processing)

Tasks left unchecked by the executor for this reason: [P1-T6], [P3-T6], [P3-T7], [P3-T8],
[P4-T1], [P4-T2], [P4-T3], [P4-T4], [P4-T6], [P4-T13], [P4-T14], [P4-T15].

### Orchestrator's disposition (recorded before delegating the remediation plan)

This is treated as an in-scope root-cause fix, not a deferral, for three reasons:

1. `QfcHomeControllerTests.cs` is not owned by any sibling epic child's declared scope (siblings
   #511/#571, #445, and #449 own only specific regions of `QuickFiler.Test.csproj`; none of them
   touches this `.cs` file).
2. The dead nested class is the exact defect class issue #491 exists to eliminate — a
   `Form`-derived type compiled into the `QuickFiler.Test` unit-test assembly with no production
   caller — just located in a second file the original research missed. It is not a "deeper,
   unrelated design problem" under the bugfix-workflow scope-widening guard; it is squarely on
   the path of this child's own guard test succeeding as designed.
3. Removal is provably zero-risk: the type has no callers anywhere in the tree (verified above),
   so deleting it cannot regress any existing test or production behavior.

Disposition: delete the dead `QfcFormViewerDerived` nested class (lines 243-250 as currently
read; the remediation plan re-derives the exact lines at execution time) from
`QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, then re-run the Phase 3 verification loop
and complete Phase 4 exactly as originally planned. No exemption, no guard-scope narrowing, and no
change to `spec.md` criterion text. No separate issue promotion is needed for this finding because
it is fixed within this cycle rather than deferred.

This does not replace or extend the two other out-of-scope dispositions already recorded for this
child: (a) Item 2 (`ItemViewer.Breadcrumb.cs` internal members) remains deferred to the
ItemViewer-owning epic child, already posted at
https://github.com/drmoisan/TaskMaster/issues/491#issuecomment-5380720016; (b) the unrelated
`UtilitiesCS.Test/Form1` live form in a different assembly remains out of scope for #491 and is
promoted separately via the MCP potential-bug lifecycle.

### Required remediation-plan scope (for atomic-planner)

- Delete the dead `QfcFormViewerDerived` nested class from
  `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, re-deriving its exact current line
  range at execution time (do not trust line numbers cited in this document).
- Re-run the full Phase 3 verification loop (csharpier format/check, both msbuild rebuilds,
  full-suite vstest, named-guard vstest) from a clean state.
- Complete the previously-blocked Phase 4 tasks: coverage capture/comparison, test-count parity,
  and AC1/AC8/AC9/AC10/AC13/AC14/AC15 check-offs (renumber against the actual unchecked list above).
- Produce exactly one new commit confined to `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`
  plus this feature folder's evidence/spec.md/plan.md updates. Do not reopen or amend the existing
  commits `c7557c3d`, `5cec657b`, or `3f2fb8d1`.
- Evidence root is unchanged: `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/`.
