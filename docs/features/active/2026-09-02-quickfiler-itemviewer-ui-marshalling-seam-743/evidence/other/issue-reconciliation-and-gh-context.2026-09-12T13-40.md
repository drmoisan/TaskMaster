# Issue Reconciliation and GitHub Context (Issue #743)

Timestamp: 2026-09-12T13-40
Collected by: orchestrator (preparation mode)
Command: `gh issue view <N> --repo drmoisan/TaskMaster --comments` for N in 743, 592, 511, 571
EXIT_CODE: 0

## Why this artifact exists

`Agent(task-researcher)` in this repository is granted Read, Grep, Glob, WebFetch and Write only. It has
no Bash tool, so it cannot run `gh` and cannot read GitHub issue bodies or comments. Every fact below was
collected by the orchestrator and is recorded here so the research, spec, and plan stages have it.

## Issue states

| Issue | State | State reason | Closed at |
|---|---|---|---|
| #743 | OPEN | n/a | n/a |
| #592 | CLOSED | NOT_PLANNED | 2026-09-11T23:09:42Z |
| #511 | CLOSED | superseded by #592 | see closing comment |
| #571 | CLOSED | superseded by #592 | see closing comment |

## Finding 1 — the maintainer's first lead names symbols that no longer exist

Issue #592's "Hypothesis to test (not a finding)" section, echoed verbatim in the closing comments of both
#511 and #571, states:

> `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` defines a
> `UiThreadDispatcherGate` and a `SwapUiThreadDispatcher` helper that mutate the process-wide static
> `UtilitiesCS.UiThread._dispatcher` by reflection in order to serialize the pump tests across two test
> classes. `QfcItemController.SeamFactoryTests` and `QfcItemController.InitializationTests` contend on
> that gate.

Neither identifier exists in any `.cs` file in the current tree. A repository-wide Grep for
`UiThreadDispatcherGate|SwapUiThreadDispatcher` restricted to `*.cs` returns zero files.

The mechanism that exists today is `UiThreadDispatcherFixture` and `UiThreadDispatcherTransaction` in
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, whose own documentation
comment states it is the owner of every mutation of the process-wide static
`UtilitiesCS.UiThread._dispatcher` made from this test assembly's owned files, and that swaps are
serialized behind a `TransactionGate`.

**CORRECTION, 2026-09-12T15-05.** An earlier revision of this artifact attributed that replacement to
issue #648. That was WRONG and is corrected here. The fixture's own doc comment at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:12-13` attributes the design
to **issue #493**, not #648. The error arose from inferring authorship from a `git log --grep=648` hit on
the merge commit of PR #719, branch `bug/wpfuidispatchertests-ungated-static-swap-648`; that change added
`WpfUiDispatcherTests` as a CONSUMER of the existing fixture rather than creating it. The delegated
researcher caught this independently and the doc comment was then re-read directly to confirm it. The
substantive point is unaffected: the lead's named symbols no longer exist, and the replacement predates
neither the measurement nor the lead in a way that rescues the lead.

Consequence: the lead is stale as written. It must be re-tested against the current gate, not assumed.
The open question is whether #648 closed the contention or merely renamed and centralized it.

## Finding 2 — the window-handle cause is already falsified, and #511/#571 already say so

Acceptance criterion 5 asks that the closing comments of #511 and #571 be checked so they do not assert
the falsified window-handle cause without correction. Both issues carry two comments that already do this
explicitly, with identical text:

- A "Premise correction from the epic execution run (2026-08-22)" comment stating the stated root cause is
  falsified by measurement.
- A "Closing as superseded by #592" comment repeating the refutation with source citations.

The refutation, as recorded there:

- `ItemViewer()` calls `InitializeComponent()` at `QuickFiler/Viewers/ItemViewer.cs:25`.
- `InitializeComponent` runs `BeginInit()` on both WebView2 children at
  `QuickFiler/Viewers/ItemViewer.Designer.cs:89-90` and `EndInit()` at `:6166-6167`.
- `EndInit` creates the child handles, and WinForms creates a parent's handle when a child's handle is
  created, so the viewer's handle exists the instant construction returns.
- Forcing the handle is therefore a measured no-op.
- The failure signature is seven expiries at 60,000 ms, not an immediate exception; a missing handle makes
  `Control.Invoke` throw at once rather than hang for sixty seconds.

So AC5 is predominantly a verification rather than a repair. What does remain is a forward-pointer defect:
both closing comments direct the reader to #592 for the real defect, and #592 is now itself closed
NOT_PLANNED and consolidated into #743. Both comments also restate the stale gate hypothesis from Finding 1.

## Finding 3 — measured base rate and the statistical bound

Recorded in #592 and repeated in the #511 and #571 closing comments:

- Pre-fix run-level failure rate approximately 1 in 21, that is approximately 4.8 percent.
- Thirty consecutive clean runs has probability approximately `0.952^30`, approximately 0.23, under the
  null hypothesis of no effect. Thirty clean runs is therefore not sufficient evidence of efficacy.
- Under induced 17-node MSBuild contention a supplementary pass was 8 of 10 green.
- The one genuine pre-fix failure was seven expiries at the 60,000 ms `PumpTimeoutMs`.
- `PumpTimeoutMs = 60000` is cited at `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:38`.

## Finding 4 — the contradiction between #511 and #571, and its bearing on the constraint set

The closing comments record that #511 and #571 cannot both be implemented as written: #511 proposes
replacing the real message pump with an injectable context, which executed literally deletes the very
tests #571 exists to stabilize. This is the origin of the inherited constraint forbidding a fake
`SynchronizationContext` that replaces the real pump. That constraint remains in force for this delivery.

The constraint that is deliberately re-opened for issue #743, and only for it, is the prohibition on
production edits.

## Finding 5 — prior-art branch with unmerged evidence

Evidence and a halted implementation are preserved unmerged on branch
`bug/winformspumphost-suite-determinism-511-exec` at commit `53a2a08f`, under
`docs/features/active/winformspumphost-suite-determinism-511/evidence/`, described as 36 markdown
evidence artifacts including `evidence/regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md`.
That branch was not merged and no production file was modified on it.

## Finding 6 — a second, independent failure mode recorded on #511

A #511 comment dated 2026-08-08 records that the two pump-hosted tests
`InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` and
`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` failed with
`System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the window
handle has been created`, and that they pass in class isolation and in their own assembly but fail in the
combined instrumented nine-assembly run. That is an exception signature, not a timeout signature, and the
later 2026-08-22 premise correction concluded the handle attribution was falsified. Whether these are two
distinct failure modes or one misread observation is not resolved by the issue record and should not be
assumed either way.

## Output Summary

Four issues read. The maintainer's first lead is stale: its named symbols were refactored away by #648.
The window-handle cause is already falsified and both #511 and #571 already carry explicit corrections, so
AC5 reduces to a verification plus a forward-pointer update. Base rate 4.8 percent and the 0.23 null
probability for thirty clean runs are confirmed as recorded figures.
