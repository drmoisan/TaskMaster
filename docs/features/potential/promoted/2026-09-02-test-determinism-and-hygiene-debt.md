# test-determinism-and-hygiene-debt (Issue #729)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/test-determinism-and-hygiene-debt/ (Issue #729)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #729
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/729
- Last Updated: 2026-09-02
## Summary

Four consolidated findings across the C# test suite, all in the same theme: tests that depend on real wall-clock time, real WinForms UI construction, unparallelized-but-unguarded execution, or environmental load — rather than deterministic seams — violating this repo's own determinism-infrastructure policy (`.claude/rules/general-unit-test.md`: controllable clock, no real wall-clock waits, no live UI in unit tests). Consolidated into one issue rather than four since all four are variations of the same root problem (missing determinism seams) and fixing them is one coherent test-infra effort.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C# MSTest suite
- Command/flags used: n/a — findings are from direct test-source inspection
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable — each sub-finding is a static test-source inspection. See "Actual Behavior."

## Expected Behavior

Per this repo's own determinism-infrastructure policy: tests use an injected `TimeProvider`/`Clock` seam rather than reading wall-clock time directly; no live UI construction in a unit test; parallelizable tests either tolerate parallel execution or are explicitly excluded with a documented reason; load-sensitive timeouts don't cause spurious CI failures under contention.

## Actual Behavior

**1. `NonBlockingDelayTests.cs` awaits real wall-clock time.** Confirmed at `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:38-39`: `var interval = TimeSpan.FromMilliseconds(30); var stopwatch = Stopwatch.StartNew();` then awaits `NonBlockingDelay.WaitAsync(interval)` against that real stopwatch — no fake-timer/`TimeProvider` seam is used. *(Source: #694.)*

**2. `UtilitiesCS.Test/ResourceTests.cs` constructs a live WinForms form in a unit test.** Confirmed at line 20: `Form1 frm = new Form1();` inside `TestMethod1`. No structural guard against this exists for `UtilitiesCS.Test` — only `QuickFiler.Test` has an equivalent "no live Form in test assembly" structural test. *(Source: #586.)*

**3. Two duplicate `DASLFilterParser*Tests.cs` classes lack `[DoNotParallelize]`**, while the test assembly runs `[assembly: Parallelize(Workers=0, Scope=ClassLevel)]` and no console-lock/serialization mechanism exists to make concurrent execution of these two classes safe. *(Source: #520.)*

**4. Pump-hosted `QfcItemController` tests expire at the 60s `PumpTimeoutMs` under CPU contention.** Load-sensitive flakiness rather than a straightforward logic defect — no simple code fix, but worth tracking as one line item in this consolidated test-infra debt issue rather than its own standalone tracker, since the underlying cause (no environment-aware timeout scaling, or no mocked pump) is the same class of missing-determinism-seam problem as findings 1-3. *(Source: #711.)*

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations above, each confirmed directly against `origin/main` on 2026-09-02.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: none of these cause incorrect production behavior — they're test-suite reliability/determinism debt that risks intermittent CI failures and slower feedback loops, consistent with this repo's own stated rationale for the determinism-infrastructure policy these findings violate.

## Suspected Cause / Notes

Each finding traces to a specific issue, cited inline above. All four share the same root class: a test depends on real time, real UI, or real environmental load instead of an injected, controllable seam — exactly what `.claude/rules/general-unit-test.md`'s "Determinism Infrastructure" section already mandates repo-wide but which these four tests predate or were missed by.

## Proposed Fix / Validation Ideas

- [ ] `NonBlockingDelayTests.cs`: inject a fake `TimeProvider`/clock instead of a real `Stopwatch`-timed wait
- [ ] `UtilitiesCS.Test/ResourceTests.cs`: remove or mock the live `Form1` construction; consider porting `QuickFiler.Test`'s "no live Form in test assembly" structural guard to `UtilitiesCS.Test`
- [ ] Add `[DoNotParallelize]` to both `DASLFilterParser*Tests.cs` classes, or introduce a console-lock/serialization mechanism if parallel execution is actually required
- [ ] Investigate whether `PumpTimeoutMs` should scale with detected environment load, or whether the pump host can be mocked/faked for these specific tests to remove the timing dependency entirely

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
