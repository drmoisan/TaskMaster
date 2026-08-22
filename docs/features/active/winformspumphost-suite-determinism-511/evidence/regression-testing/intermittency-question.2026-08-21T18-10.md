# Phase 1 — Disposition of the Open Intermittency Question (P1-T6)

Timestamp: 2026-08-22T10-28

The plan carried one open question into Phase 1 and required it to be settled by execution rather than
by static reading. This artifact records what the measured data supports, what it rules out, and what
remains open. It does not close the question by assertion; every sentence below that claims a mechanism
cites an observation.

## The question as posed

Static reading predicted that both named tests should fail on **every** run: `Control.Invoke` throws
unconditionally without a created handle, and the research found no handle-creating call anywhere in
the `ResolveControlGroups` then `SetupThemes` then `PopulateControls` path. Issue #571 nevertheless
recorded the tests passing on some runs. Two candidate explanations were carried forward:

- **Candidate A** — the traced initialization sequence really does lack a handle-creating call, and
  some path *outside* that traced sequence creates the handle. The plan named third-party
  `Microsoft.Web.WebView2.WinForms.WebView2` `ISupportInitialize` or implicit-initialization behaviour
  as the prime suspect, noting its source is not present in this repository.
- **Candidate B** — the handle is genuinely absent, and the tests fail whenever the initialization path
  is reached, so any passing run would have to be explained some other way.

## What the measured data supports

**Candidate A is supported. Candidate B is ruled out.**

The P1-T5 table records twenty runs — rows 1 through 10 class-filtered, rows 11 through 20 full
nine-assembly suite. **Every one of those twenty rows records
`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` as `Passed` and therefore
`IsHandleCreated: true`.** Naming the runs as the task requires: the probe reported
`IsHandleCreated: true` on P1-T5 rows 1, 2, 3, 4, 5, 6, 7, 8, 9, and 10 (class-filtered runs 1 through
10) and on rows 11, 12, 13, 14, 15, 16, 17, 18, 19, and 20 (full-suite runs 1 through 10) — that is,
on all twenty pre-fix runs.

Because the probe asserts `harness.Viewer.IsHandleCreated` is `true` **and** that
`harness.Viewer.InvokeRequired` evaluated on the pump thread is `false`, those twenty observations
establish two things about the pre-fix code:

1. The viewer's window handle **did exist** by the time `BuildPumpHarnessAsync` returned, on every one
   of the twenty runs. Candidate B, which requires the handle to be absent, is therefore ruled out for
   these runs.
2. The handle was owned by the **pump thread**, not by some other thread, on every one of the twenty
   runs. `InvokeRequired` returning `false` when evaluated on the pump thread is what establishes
   this.

Since the research's traced `ResolveControlGroups` then `SetupThemes` then `PopulateControls` sequence
contains no handle-creating call, and the handle nevertheless existed on all twenty runs, **some path
outside that traced initialization sequence created the handle.** That inference rests directly on the
twenty `IsHandleCreated: true` observations in the P1-T5 table.

## The prime suspect, and why it remains unverified

The plan named third-party WebView2 `ISupportInitialize` or implicit-initialization behaviour as the
unverified prime suspect. This execution found structural evidence consistent with that route, which
raises its plausibility without confirming it.

The harness constructs its viewer with `new QuickFiler.ItemViewer()` on the pump thread
(`QfcItemController.InitializationTests.Part2.cs`, inside `BuildPumpHarnessCoreAsync`, at the line
reading `QuickFiler.ItemViewer viewer = await host.InvokeAsync(() => new QuickFiler.ItemViewer())`).
That constructor runs `InitializeComponent()`, and `QuickFiler/Viewers/ItemViewer.Designer.cs` routes
both WebView2 children plus one further control through the `ISupportInitialize` protocol:

```
89:   ((System.ComponentModel.ISupportInitialize)(this._l0v2h2_WebView2)).BeginInit();
90:   ((System.ComponentModel.ISupportInitialize)(this._l0vhBreadcrumb_WebView2)).BeginInit();
92:   ((System.ComponentModel.ISupportInitialize)(this._topicThread)).BeginInit();
...
6166: ((System.ComponentModel.ISupportInitialize)(this._l0v2h2_WebView2)).EndInit();
6167: ((System.ComponentModel.ISupportInitialize)(this._l0vhBreadcrumb_WebView2)).EndInit();
6170: ((System.ComponentModel.ISupportInitialize)(this._topicThread)).EndInit();
```

(Line numbers re-derived in this worktree rather than taken from any prior citation.)

This places an `ISupportInitialize` `BeginInit`/`EndInit` pair on the WebView2 children **inside the
constructor** — that is, genuinely outside the traced `ResolveControlGroups` / `SetupThemes` /
`PopulateControls` sequence, which is where the inference above says the handle creation must live.

**This does not verify the mechanism, and the question is not closed.** What remains open:

- The `Microsoft.Web.WebView2.WinForms.WebView2` implementation of `EndInit` is third-party and its
  source is **not present in this repository**, so whether it creates a parent window handle cannot be
  read here. The correlation is structural and positional only.
- `_topicThread` also goes through the same protocol, so the designer evidence does not isolate the
  WebView2 controls as the responsible participant even if the `ISupportInitialize` route is the right
  one.
- No experiment in this execution attributed handle creation to any specific call. Doing so would
  require instrumenting or bisecting the constructor, which is outside this plan's scope and is not
  required by any task in it.

## The one pre-fix failure observed in this execution, and what it does and does not show

A run in which **both named tests failed** was observed during P0-T16 and is recorded in
`evidence/baseline/coverage.2026-08-21T18-10.md`: the second invocation of
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` reported `Total tests: 6437, Passed: 6430, Failed: 7,
Test Run Failed.`, with all seven failures being 60,000 ms `PumpTimeoutMs` expiries and both
`InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` and
`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` among them.

That run is deliberately **not** among the twenty P1-T5 rows, because it is neither a P1-T3
class-filtered run nor a P1-T4 full-suite run and used a different harness (`dotnet-coverage`
instrumentation rather than plain `vstest.console.exe /EnableCodeCoverage`).

What it shows: the failure mode is **reachable** on this machine against the pre-fix code, so the zero
failure rate in the twenty-row table is not evidence that the defect cannot occur.

What it does **not** show: the mechanism. The probe did not run in that invocation's failing set in a
readable way, so no `IsHandleCreated` value is available for it, and the failures were timeouts rather
than the `Control.Invoke` handle exception the static reading predicts. A 60-second timeout is
consistent with more than one cause — a handle-less control is one, and pump starvation under load is
another — and this execution did not distinguish them. That ambiguity is recorded, not resolved.

The only environmental difference between that failing invocation and the passing invocations
immediately before and after it was machine load: 17 idle MSBuild node-reuse processes from the P0-T13
and P0-T14 builds were resident during it and were stopped before the next invocation, which passed. No
stray `testhost`, `vstest.console`, or `dotnet-coverage` process belonging to another agent was present
at any point, so a competing test runner is ruled out as the cause.

## Summary of disposition

| Item | Status |
| --- | --- |
| Candidate A (handle created outside the traced sequence) | **Supported** by twenty `IsHandleCreated: true` observations (P1-T5 rows 1 through 20) |
| Candidate B (handle genuinely absent) | **Ruled out** for all twenty measured runs |
| WebView2 `ISupportInitialize` as the responsible route | **Remains open** — named as the prime suspect, plausibility raised by the designer evidence, source not in this repository, not verified |
| Which specific call creates the handle | **Remains open** — not investigated; outside plan scope |
| Whether the observed 60-second timeouts share the same root cause as the handle question | **Remains open** — timeout is consistent with both a handle-less control and load-induced pump starvation |
| Measured pre-fix failure rate across the twenty planned runs | **0 / 20**, recorded as measured |
| Reachability of the failure mode against pre-fix code | **Demonstrated once**, outside the twenty-row table, under added machine load |

## Effect on the remedy — none

Per the plan's binding instruction, this result does **not** narrow, widen, or abandon the chosen
remedy. Forcing the handle with `_ = await host.InvokeAsync(() => viewer.Handle).ConfigureAwait(false)`
is correct under either explanation: if some incidental path currently creates the handle, the fix
removes the dependency on that incidental behaviour; if the handle is sometimes absent, the fix supplies
it. The twenty green pre-fix runs are recorded as data about the race window, not as evidence the defect
is absent. Phase 2 proceeds as written.
