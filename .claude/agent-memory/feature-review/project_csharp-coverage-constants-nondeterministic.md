---
name: csharp-coverage-constants-nondeterministic
description: 'TaskMaster full-suite C# repo-wide coverage is nondeterministic (~0.015 line-point band); never gate on a cross-session constant — compare same-session baseline vs final, and attribute variance via per-file covered-line diffs'
metadata:
  type: project
---

TaskMaster full-suite Cobertura repo-wide line-rate varies run-to-run in a ~0.858485–0.858665 band on an identical tree (#438 R1 remediation: five clean runs plus an unmodified-tree control that missed the cycle-1 constant by itself). Variance is isolated to a rotating set of untouched legacy classes — `SegmentStopWatch.cs` (wall-clock timing helper), `PropertyStore.cs`, `EfcHomeController.cs`, `SubjectMapSco.Orchestration.cs` — driven by the WinFormsPumpHost load-flakiness family, tracked in issue #511.

**Why:** a remediation-inputs acceptance clause I authored ("repo-wide figures not lower than <cycle-1 constants>") produced a false FAIL that cost five full-suite retries; the control run proved the constant unreachable regardless of the change.

**How to apply:**
1. When authoring remediation acceptance/verification clauses, gate repo-wide no-regression against a **same-session Phase-0 baseline**, never against figures carried across sessions.
2. When adjudicating a small repo-wide coverage delta, run a per-file covered-line diff between the two Cobertura XMLs (parse `<class>`/`<line hits>`); if the differing files are all outside the branch diff, the delta is measurement variance, not a regression. In #438 this was decisive: baseline->final differed in exactly one untouched file (+6 lines); cycle1->final in exactly three untouched files (net −3 of ~111k).
3. Policy floors (85% line / 75% branch, no changed-line regression) remain the real gate; a sub-0.0001 miss against a historical constant with clean per-file attribution is non-blocking.
