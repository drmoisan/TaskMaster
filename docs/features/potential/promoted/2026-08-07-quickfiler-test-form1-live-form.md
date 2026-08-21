# quickfiler-test-form1-live-form (Issue #491)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-test-form1-live-form/ (Issue #491)
- Discovered during: preparation research for issue #456 (epic #136, child F14)

- Issue: #491
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/491
- Last Updated: 2026-08-08
## Summary

A live `System.Windows.Forms.Form` is compiled into the `QuickFiler.Test` assembly, and a second,
unrelated item of dead production surface exists in `ItemViewer.Breadcrumb.cs`. Both are test-policy
and design-debt items rather than runtime defects, and both are outside epic #136 child F14's
production file set.

## Item 1 — live `Form` compiled into the unit-test assembly

`QuickFiler.Test/Form1.cs:5` and `QuickFiler.Test/Form1.Designer.cs:3` declare
`public partial class Form1 : System.Windows.Forms.Form`, whose `InitializeComponent` constructs three
`QuickFiler.ItemViewer` instances (`Form1.Designer.cs:32-34`).

No test instantiates it — verified: the only `Form1` references in the test project are its own two
files — so no policy violation occurs today. But `.claude/rules/general-unit-test.md` and epic #136's
"never construct live forms" rule are one `new Form1()` away from being breached, and the type is dead
weight in the test assembly.

Candidate disposition: delete both files, or move them to a manual harness project outside the unit
test assembly.

## Item 2 — three `internal` members of `ItemViewer.Breadcrumb.cs` have no production caller

`AttachBreadcrumbMessengerWhenReadyAsync` (`ItemViewer.Breadcrumb.cs:100-124`),
`AttachBreadcrumbMessenger` (`:126-140`), and `BreadcrumbOpenTask` (`:29-30`) are invoked only from
tests. A repository-wide search for each identifier returns the declaration plus call sites in
`QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:438`,
`BreadcrumbSubfolderActivationTests.cs:340`,
`BreadcrumbSelectorOpenRetryTests.cs:38,41,61,69,265`,
`BreadcrumbCoordinatorLifecycleTests.cs:123`, and
`BreadcrumbDropDownIntegrationTests.cs:415-421`. No `QuickFiler/**` production file references them.

This is roughly 40 lines of production surface maintained solely for tests. It is not a bug, and it
must not simply be deleted — doing so would break seven existing tests. The disposition is either to
promote these members to the production attach path (the `AttachCollapsedMessenger` route is
arguably what `CreateCollapsedBreadcrumbCandidate` should use) or to mark them explicitly as test
seams so their status is legible.

## Acceptance Criteria (early draft)

- [ ] No `Form`-derived type is compiled into `QuickFiler.Test`, or it is isolated in a non-unit-test
      project.
- [ ] The three test-only `internal` members are either wired into the production path or explicitly
      documented as test seams.
- [ ] Existing tests continue to pass.

## Constraints & Risks

- Item 2 touches `ItemViewer.Breadcrumb.cs`, assigned to epic child F14 (issue #456); reconcile
  against F14's plan before scheduling.
- Deleting `Form1` changes the `QuickFiler.Test.csproj` compile set; preserve CRLF and keep the edit
  to minimal adjacent hunks.

## Next Step

- [ ] Promote to GitHub issue (bug template)
