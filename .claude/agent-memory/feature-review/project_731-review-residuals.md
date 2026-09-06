---
name: 731-review-residuals
description: "#731 QuickFiler lifecycle/disposal review: PASS, 0 blocking, 19/19 AC; the caller's own deferral framing was wrong; CSharpier forces a blank line above an inserted comment; mtime-vs-artifact-timestamp caught a post-pass probe"
metadata:
  type: project
---

Issue #731 (`bug/quickfiler-controller-lifecycle-disposal-defects-731` @ `c55bfad2`) closed **PASS,
0 Blocking, 19/19 AC**. Five consolidated static-analysis findings on QuickFiler's collection
controller / queue / form controller. Reusable residuals:

**The caller's own framing of his evidence was wrong, in his own disfavour.** The delegation prompt
said `[P5-T5]`'s absolute-floor branch and `[P5-T6]` Branch B both deferred the no-regression
judgment to an empty-population gate, so "no no-regression signal is available from any of the three
sources." The artifacts say the opposite: `[P5-T5]` recorded `Absolute floor result: PASS` so its
deferring FAIL branch was never entered, and `[P5-T6]` resolved D-COMPARABLE and took Branch A. A
`### Deferral reconciliation` section in `coverage-delta.md` stated this explicitly. Reinforces
[[verify-the-callers-factual-correction]] — verify corrections that make the work look *worse* too,
not only flattering ones.

**CSharpier mandates a blank line between a member declaration and a following comment.** A
standalone inserted comment therefore costs **2** insertions, not 1, and this breaks any
hand-computed numstat diff bound. Holds for `///` doc-comment form as well. Reproduce it before
accepting or rejecting the claim; #731 retained two probe logs showing `Was not formatted` with an
`Expected: Around Line N` block containing the blank line.

**Detect post-toolchain-pass tree mutation by comparing source mtimes against gate artifact
timestamps.** At #731 `QfcQueue.cs` had mtime 14:38:25 while the passing loop ended at 14:33 — a
formatter probe had mutated and restored it, and only `csharpier check` was re-run. Confirm the
restore was faithful by diffing the executor's own pre-probe numstat capture
(`coverage/p5t1-numstat-after.txt`) against the live numstat at HEAD. Advisory, not blocking, when
the mutation is confined to comments/whitespace.

**IDE0052 is not configured in TaskMaster's `.editorconfig`**, so the `/t:Rebuild` analyzer gate
cannot catch a private field that is written but never read. #731 introduced one
(`_undoQueueDisposal` in `QfcFormController.SetupDisposal.cs`, a reflection-reached test-observation
seam) and the gate reported 0 warnings. Check for unread private members by hand.

**Other residuals owed at PR time:** `QfcQueue.cs` crossed 505 -> 507 lines (already over the ceiling
at base) without any spec-level disclosure — only `QfcCollectionController.cs`'s overage was declared;
three spec-declared follow-ups were never promoted to potential entries; and both `pr_context`
artifact pairs described unrelated branches (see [[pr-context-artifacts-are-tracked-not-gitignored]]).
Coverage ruling for the two uninstrumented files is in
[[excludefromcodecoverage-attribute-ruling]].
