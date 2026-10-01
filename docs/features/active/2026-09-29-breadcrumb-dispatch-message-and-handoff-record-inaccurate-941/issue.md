# breadcrumb-dispatch-message-and-handoff-record-inaccurate (Issue #941)

- Date captured: 2026-09-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-dispatch-message-and-handoff-record-inaccurate/ (Issue #941)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #941
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/941
- Last Updated: 2026-09-30
- Work Mode: minor-audit

## Summary

The breadcrumb test dispatcher's exception message, and one earlier handoff record, describe the `DispatchValue` path inaccurately. These are wording and documentation defects only; behavior is correct.

## Environment

- OS/version: n/a
- Python version: n/a (C#)
- Command/flags used: n/a
- Data source or fixture: n/a

## Steps to Reproduce

1. Read `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` lines ~101 and ~183, and the assertion at `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:305`.
2. Compare the message with what the owner-thread-only `DispatchValue` path actually does.
3. Read the earlier breadcrumb thread-affinity follow-up handoff record, which attributes one `DispatchValue` site to the owner-thread-id check.

## Expected Behavior

The message and the asserted wording describe the specific `DispatchValue` behavior, and the handoff record attributes each site to the check that actually governs it.

## Actual Behavior

- The message "The owner-thread-only test dispatcher cannot marshal cross-thread UI work." is broader than the `DispatchValue` mechanism, and the test asserts the broad wording with `*cannot marshal cross-thread UI work*`.
- The earlier handoff record misattributes one `DispatchValue` site to the owner-thread-id check.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: see the PR #939 body, section Follow-ups, items 3 and 4.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

Both originate from the breadcrumb thread-affinity work, and both were found by the #931 review. Source: `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t13-follow-up-handoff.2026-09-29T09-46.md`.

## Proposed Fix / Validation Ideas

- [ ] Narrow the message text at both sites and update the test's `WithMessage` pattern to match.
- [ ] Correct the handoff record's attribution.

## Acceptance Criteria

Scope is bounded by `research/2026-09-29T23-05-breadcrumb-dispatch-message-research.md`, which found that the `Dispatch` message site (line 101) is governed by the owner-thread guard and is accurate, and that the `DispatchValue` message site (line 183) is governed by the executing-callback check and the null-context check, never by owner-thread identity, so its message is inaccurate. Line numbers are as observed on the branch before the fix.

- [x] AC1: In `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, the `DispatchValue` no-context fault message (line 183) reads exactly `The owner-thread-only test dispatcher cannot run value-producing UI work outside an executing Dispatch callback.` and no longer contains the text `cannot marshal cross-thread UI work`.
- [x] AC2: In `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, the `Dispatch` no-context message (line 101) is unchanged, because that site is reached only after the owner-thread guard fails and its wording is accurate.
- [x] AC3: In `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs`, the `WithMessage` pattern at line 305 is changed from `*cannot marshal cross-thread UI work*` to `*outside an executing Dispatch callback*`, and the test passes.
- [x] AC4: `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` line 86 (`Contain("cannot marshal")`, which exercises the unchanged `Dispatch` site) passes without modification.
- [x] AC5: A new MSTest regression test in `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` calls `DispatchValue` on the owner thread of an owner-thread-only dispatcher, outside any executing callback, and asserts the faulted task carries `outside an executing Dispatch callback`. The test fails before the AC1 change and passes after it.
- [x] AC6: The handoff record `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md` attributes only the `Dispatch` site test (`BreadcrumbPopupBoundaryCoverageTests.cs`) to the owner-thread-id check, attributes the `BreadcrumbUiThreadDispatchTests.cs` `DispatchValue` site to the executing-callback and null-context checks, and cites the owner-thread comparison at `BreadcrumbUiDispatcher.cs` lines 276-277 rather than the storage and pass-through lines 40, 54 and 64.
- [x] AC7: No other test in the solution asserts either message text without a matching update: a search of all `*.cs` files for `cannot marshal` returns only the line 86 assertion and the unchanged line 101 production literal.
- [ ] AC8: The C# toolchain passes in order with no failures: `dotnet tool run csharpier check .`, the analyzers `msbuild TaskMaster.sln /t:Rebuild ...` command, the nullable `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` command, and MSTest with coverage through `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
- [ ] AC9: Coverage does not regress on the changed lines, and no changed file exceeds 500 lines.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
