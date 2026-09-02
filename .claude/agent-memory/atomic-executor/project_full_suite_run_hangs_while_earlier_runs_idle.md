---
name: full-suite-run-hangs-while-earlier-runs-idle
description: A QuickFiler.Test full-assembly run that passed in 14s at baseline can hang mid-run later in the same session; sample testhost CPU to prove hang vs slow, and check the baseline before calling the failures a regression.
metadata:
  type: project
---

A full-assembly `vstest.console.exe` run that completed in ~14 seconds during
Phase 0 can hang partway through when the same command is re-run later in the
same session. Prove hang versus slow by sampling the `testhost` process CPU over
60 seconds: a hung run moves the counter by hundredths of a second while the
transcript line count stays frozen. Kill only the `vstest.console` and
`testhost` processes whose `StartTime` matches your own run, then re-run once.

**Why:** On issue #662 the P0-T11 baseline ran the byte-identical command and
reported 1286/1286 passed in 14.5s. The P2-T7 re-run of that same command hung at
1328 transcript lines with 15 failures, every one a 60000 ms timeout, all in
`WinFormsPumpHost` harness tests and `UiThread` dispatcher-scope tests. Testhost
CPU moved 24.05 -> 24.08 over a 60s window. The single re-run passed all 1287
tests in 13.4s, including all 15. The failures were an environmental scheduling
flake, not a regression: the same tests pass before the change and after it, and
they fail only by wall-clock timeout rather than by assertion. Two unrelated
`vstest.console` processes over 24 hours old were present in the process table
during both the passing baseline and the hung run, so they are not the
differentiator and must not be killed.

**How to apply:** When a full-assembly run shows failures, diff the failing set
against the Phase 0 baseline for the same assembly before treating it as caused
by the change. Timeout-only failures in pump-host or dispatcher tests are the
load-flaky class. Re-run exactly once to characterise, record BOTH runs in the
evidence artifact with the hang diagnosis and the per-test names, and never
retry silently until green. Launch these runs detached: they outlive the
foreground 600s tool timeout. See [[project_long_runs_need_detached_process]] and
[[project_winformspumphost_tests_load_flaky]].
