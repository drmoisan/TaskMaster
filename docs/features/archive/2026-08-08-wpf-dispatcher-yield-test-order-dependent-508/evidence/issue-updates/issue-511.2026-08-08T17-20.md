# Issue Update Mirror — Issue #511

Timestamp: 2026-08-08T17-20
PostedAs: comment
Comment URL: https://github.com/drmoisan/TaskMaster/issues/511#issuecomment-5228210079
IssueUpdatedAt: 2026-08-08T21:19:00Z

## Why this update was posted rather than a new issue being opened

While delivering #508, two `QuickFiler.Test` pump-host tests failed and blocked the final toolchain
gate. They were attributed to pre-existing state at merge-base `003c5715` (see
`<FEATURE>/evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md`).

Before promoting them as a new defect, the open-issue list was searched:

```
gh issue list --state open --limit 60 --search "flaky OR pump OR handle"
```

Issue **#511** ("Bug: winformspumphost-tests-load-flaky-visible-window") already tracks exactly this
defect, and its body explicitly names `QfcItemController_InitializationTests` (the
`*ThroughThePumpHost*` cases) as affected. Creating a second issue would have duplicated it, so the
new evidence was contributed to #511 as a comment instead. No new promotion was performed and no
promotion receipt was fabricated.

## What the comment contributed that #511 did not already have

The issue body states: "no captured failure log is retained; ... A fresh capture under induced load
should accompany the fix." The comment supplies that capture, plus three findings:

1. **The exact exception and stack**, localizing the race to
   `QfcItemController.InvokeBeginInvoke` at `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:256`
   marshalling against a not-yet-created window handle — a handle-creation ordering defect, not
   general timing sensitivity.
2. **A cheaper repro than the one in the issue body.** The tests pass in class isolation (9/9) and
   in their own assembly (867/867), and fail only in the combined `dotnet-coverage`-instrumented
   9-assembly run. Coverage instrumentation overhead alone triggers the race; driving the machine to
   ~96% CPU is not required.
3. **Provenance**: the two tests were introduced by commit `8f98264c` ("feat(quickfiler-test): add
   WinForms message-pump test seam (#230)", merged as PR #479), so the defect entered with the
   pump-host seam itself.

## Scope correction requested in the comment

#511 currently lists `WpfDispatcherYieldTests` among the affected suites. That suite's
nondeterminism had a different root cause — an unarranged ambient WPF `Dispatcher` precondition, not
the WinForms pump host — and is fixed under #508. The comment asks for it to be removed from the
affected-suite list so the remaining scope is unambiguously `WinFormsPumpHost` and its
`*ThroughThePumpHost*` consumers.

The comment also records that a WPF `Dispatcher` on an owned STA thread does not create a visible
window; the visible-window symptom in #511 is specific to `WinFormsPumpHost` constructing a real
WinForms control and running `Application.Run` (verified at
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:326`).

## Exact text posted

The full comment text is reproduced in the GitHub comment linked above. Its substantive content is
the four-run experiment table, the stack trace, the provenance commit, and the scope correction, all
of which are reproduced in this repository at
`<FEATURE>/evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md`.

Output Summary: Posted a comment to existing issue #511 rather than opening a duplicate, supplying
the fresh failure capture that issue explicitly requested, a cheaper instrumentation-only repro, the
introducing commit (`8f98264c`, #230), and a scope correction removing `WpfDispatcherYieldTests`
from its affected-suite list. Comment URL recorded above.
