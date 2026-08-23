---
name: premise-falsified-child-halt
description: A child that halts because its own evidence falsified the plan's premise is the system working; verify the falsification yourself, preserve the branch, descope that child, and deliver the rest rather than forcing completion
metadata:
  type: feedback
---

When a child reports that it halted because its measurements contradict the defect hypothesis its
approved plan was built on, do NOT re-delegate it to "just finish". Verify the falsifying
measurement yourself against the source, then descope that child and deliver the epic's remaining
children.

**Why:** In the QuickFiler determinism epic, child #511/#571 completed Phases 0-4 with every
delegation and validator green, then halted: its remedy forced a window handle that already
existed. `ItemViewer`'s constructor calls `InitializeComponent`, which runs `ISupportInitialize`
`BeginInit`/`EndInit` on both WebView2 children, `EndInit` creates their handles, and WinForms
creates a parent's handle when a child's is created. I confirmed it directly in
`ItemViewer.Designer.cs` rather than taking it on report. Completing the route would have produced
a pull request whose headline claim its own evidence contradicted, and closed two issues that were
not fixed. The child was right to stop and right to escalate: choosing which issues close is the
epic's call, not the child's.

Two traps this exposes, both worth checking on any epic:

1. **A misdiagnosed root cause poisons the constraint set, not just the remedy.** This epic
   forbade production edits, timing tolerances, and the synchronization-context seam. Every one of
   those bans was chosen to protect against the *wrong* failure mode. Against the real one — a
   60,000 ms pump-timeout expiry under CPU contention — the constraints left no in-scope remedy at
   all. When a premise falls, re-derive whether the constraints still make sense; do not assume
   only the fix needs rework.
2. **Thirty green runs is not evidence.** Against a ~4.8% base failure rate, thirty consecutive
   clean runs has probability ≈ 0.23 under the null hypothesis of no effect. Demand the arithmetic
   before accepting a flake fix, from any child, including one reporting success.

**How to apply:** Verify the falsification against the source yourself. Commit and push the child's
working tree to its branch as a clearly-labelled preservation commit — do NOT merge it — see
[[feedback_preserve_halted_child_worktree]]. Post the premise correction as a comment on every
issue the child was scoped to close, so the issue text is not later acted on as written. File the
*real* defect as a new issue. Record the decision, its rationale, and its consequences in the epic
checkpoint and in `epic-status.md`. Then deliver the other children: they are green, independently
valuable, and (verify this) touch disjoint file sets. Report the epic as
delivered-with-one-child-descoped and say plainly that `require_complete=True` fails by design —
never fabricate a terminal `merge_status` to make the gate pass. The `merge_status` enum has no
member for this, which is itself worth an issue.
