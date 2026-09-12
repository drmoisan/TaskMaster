---
name: qfc-keyboard-actions-430
description: "#430 (epic #136 F3) keyboard-actions research: KaStringAsync has NO async/timer despite issue.md claiming a fake-timer seam is needed; only CLAUDE.md:303 (not .claude/rules/csharp.md) names KbdActions<> as non-exempt"
metadata:
  type: project
---

Research completed 2026-08-07 for issue #430 (`quickfiler-keyboard-actions-coverage`, child F3 of epic
#136). Five per-file artifacts written to
`docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/research/` (04-KbdActions,
05-KaChar, 06-KaKey, 07-KaStringAsync, 08-IKbdAction).

Two corrections to written inputs that a future session should not re-derive:

1. **`KaStringAsync` needs no fake timer or injected clock.** `issue.md` line 73-74 and the F3
   delegation brief both assert it does. Verified across all 95 lines: zero `async`, `await`,
   `Task.Delay`, `Thread.Sleep`, timer, `DateTime`, or `TimeProvider`. The "Async" suffix names only
   the stored delegate's type (`Func<string, Task>`). The real async driver is
   `KeyboardHandler.KeyDownTaskAsync` — a different file. The existing `KaStringAsyncTests.cs` has no
   wall-clock wait either, so there is no determinism policy defect to remediate.
2. **Only `CLAUDE.md` line 303 names `KbdActions<>` as a non-exempt testable seam.**
   `.claude/rules/csharp.md` contains no occurrence of `KbdActions` (it supplies the general 80%/90%
   floors and the seam hierarchy only). Cite the right document.

**Why:** both errors would cost a planner real budget — one funds a seam task with nothing to isolate,
the other cites a clause that does not exist and invites a reviewer challenge.

**How to apply:** when F3 planning or review resumes, propagate correction 1 into `spec.md` (rescope
the fake-timer constraint to `KeyboardHandler.cs` or drop it) and use `CLAUDE.md:303` as the sole
citation for the `KbdActions<>` obligation.

Related: [[qfc-item-controller-227-r2-denial]] (precedent that exemption boundaries get challenged),
[[feedback-exemption-audit-check-proven-techniques]].
