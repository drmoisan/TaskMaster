# utilitiescs-test-form1-live-form (Issue #586)

- Date captured: 2026-08-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-test-form1-live-form/ (Issue #586)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #586
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/586
- Last Updated: 2026-08-22
## Summary

A live `System.Windows.Forms.Form` is constructed inside a unit test in `UtilitiesCS.Test`,
violating the "never construct live forms in unit tests" policy that issue #491 established a
structural MSTest guard for in the sibling `QuickFiler.Test` assembly. `UtilitiesCS.Test` has no
equivalent guard, and this construction is real (not merely a declared-but-unused type), so it is a
higher-severity instance of the same defect class than the dead code #491 removed.

## Environment

- OS/version: Windows (repo CI target)
- Repo: TaskMaster (drmoisan/TaskMaster)
- Command/flags used: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- Data source or fixture: `UtilitiesCS.Test/ResourceTests.cs`

## Steps to Reproduce

1. Open `UtilitiesCS.Test/ResourceTests.cs`.
2. Locate `[TestMethod] TestMethod1`.
3. Observe line 20: `Form1 frm = new Form1();`, where `Form1` resolves to
   `UtilitiesCS.Test/Form1.cs` and `UtilitiesCS.Test/Form1.Designer.cs` (a distinct type from the
   `QuickFiler.Test.Form1` pair removed by issue #491 — this pair lives in a different assembly).
4. Run the `UtilitiesCS.Test` suite; `TestMethod1` constructs a real WinForms `Form` instance during
   the run.

## Expected Behavior

Per `.claude/rules/general-unit-test.md` and the "no live form" policy already enforced for
`QuickFiler.Test` by issue #491's new guard test
(`NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`), no unit test should
construct a live `System.Windows.Forms.Form`-derived instance, and no unit-test run should be able
to create a visible window on the desktop.

## Actual Behavior

`ResourceTests.TestMethod1` directly instantiates `Form1` via `new Form1()`. This is a real
construction, not merely a compiled-but-unused type (the class distinction from #491's Item 1,
where `QuickFiler.Test.Form1` was dead code never instantiated by any test).

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `UtilitiesCS.Test/ResourceTests.cs:20` — `Form1 frm = new Form1();`

## Impact / Severity

- [ ] Blocker
- [x] Medium
- [ ] Low

Medium: no evidence of CI instability caused by this today, but it is a live violation of the
unit-test policy this repo is actively hardening (see epic
`quickfiler-suite-determinism-foundation` and issue #491), and it is a real construction rather than
dead code, making it a stronger violation than the one #491 fixed.

## Suspected Cause / Notes

- Discovered while executing issue #491 (`docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/`),
  which removed the analogous `QuickFiler.Test.Form1` dead type and added a structural guard test
  scoped to the `QuickFiler.Test` assembly only. `UtilitiesCS.Test` was explicitly out of scope for
  #491 (different assembly) and has no equivalent guard.
- A second, related-but-distinct in-assembly finding from the same #491 remediation cycle
  (`QuickFiler.Controllers.Tests.QfcHomeControllerTests+QfcFormViewerDerived`, a dead nested class
  deriving from a `Form`-derived production type) was fixed directly within #491's remediation cycle
  because it was in-scope (same assembly, blocking #491's own acceptance criteria) and provably dead
  code with zero callers. This `UtilitiesCS.Test/Form1` finding is a different, out-of-scope
  assembly and involves a real (non-dead) construction, so it is tracked here instead.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: determine whether `TestMethod1`'s use of `Form1` can be replaced with a
      non-visual fixture, or whether `Form1`/`Form1.Designer.cs`/`Form1.resx` can be deleted
      entirely (mirroring #491's Item 1 disposition) if the type is otherwise unused.
- [x] Integration scenario to retest: re-run `UtilitiesCS.Test` after the fix to confirm no
      regression and no visible window during the run.
- [ ] Manual verification notes: consider adding a `UtilitiesCS.Test`-scoped structural guard test
      analogous to `NoLiveFormInTestAssemblyTests`, following the design already landed in #491.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
