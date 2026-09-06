---
name: dont-trust-the-unreachable-label-on-a-coverage-escape-set
description: When a plan excuses a changed-line coverage miss via an "unreachable set U", check each member against the BASELINE Cobertura — lines that were hit before are not unreachable
metadata:
  type: feedback
---

A coverage escape that names a set `U` of "unreachable" lines must be tested member by member against
the **baseline** coverage document, not just the post-change one. Verifying "0 uncovered changed lines
lie outside `U`" is necessary but not sufficient: it says nothing about whether `U`'s own members
deserve the label.

**Why:** #736 (2026-09-04). The plan's D2 escape declared `U` = 7 lines and the arithmetic checked out
exactly — 59 changed coverable, 52 covered, 88.14%, uncovered set identical to `U`, zero outside it.
But 3 of the 7 (`EfcDataModel.cs:359-361`, the body of a newly extracted
`protected internal virtual InvokeFilerAsync` seam) were **not** host-unreachable. Their pre-change
equivalents at `EfcDataModel.cs:343-344` on the base commit carry `hits="1"` in the P0-T6 baseline
Cobertura. A test did execute them; a `TestableEfcDataModel` override now deliberately does not. That
is a *chosen* non-execution, not an environmental impossibility, and the discharge note's claim that
"every reachable changed line is covered" was inaccurate for those three.

It was still non-blocking — the lines are a zero-branch delegation, the prior hit came only from the
incidental `NullReferenceException` the item existed to remove, the same edit newly covered two lines
the crash had blocked, and the file's covered count rose 188 -> 189. But the finding only exists
because the label was checked rather than accepted.

**How to apply:** for each member of any declared unreachable/exempt set, join it back to the baseline
document and classify it:

- **no `<line>` node in either document** — the tool never saw it (e.g. `[ExcludeFromCodeCoverage]`
  removed the whole member). Genuinely out of measurement.
- **`hits=0` at baseline and now** — plausibly unreachable; confirm by reading the code for a host
  crossing (COM, WinForms, filesystem).
- **`hits>0` at baseline, `hits=0` now** — **not unreachable.** Say so plainly, then judge on merits:
  what logic was lost, did anything assert it, and did the same edit cover anything new?

Two related traps in the same file: `[ExcludeFromCodeCoverage]` does not reach `this`-capturing lambda
arguments (they lift into instance members of the class and keep their `<line>` nodes at `hits=0`),
and a per-line `hits` join must take the max across every `<class>` node sharing a filename — see
[[cobertura-class-line-double-count-trap]] and [[736-review-residuals]].
