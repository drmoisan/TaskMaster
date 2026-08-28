---
name: epic-kickoff-facts-need-independent-measurement
description: An epic kickoff can assert a measured fact that is false by file conflation and instruct the child to act on it; measure the named file yourself before accepting a predicted Blocking finding
metadata:
  type: feedback
---

Measure every quantitative fact an epic kickoff states about a file before acting on it, especially
when the kickoff uses that fact to PREDICT a finding and prescribe a response.

**Why:** On the #449 child of `quickfiler-suite-determinism-foundation`, the kickoff stated that
`QuickFiler/Controllers/QfcExplorerController.cs` "is 1,065 lines today", that `feature-review` would
therefore raise the 500-line-cap violation, and that the child should record it as out-of-scope and
"promote the split as its own issue". Measured on the branch, the file was **323** lines — under the
cap. The 1,065-line file is `QuickFiler/Legacy/QuickFileController.cs`, an uncompiled file the change
never touches. The kickoff had conflated two similarly named controllers in the same feature area.

Acting on the stated figure would have produced a fabricated cap-violation finding, a spurious
promoted issue, and possibly an out-of-scope partial-class split — all defended by citing the
kickoff. The committed plan's own tasks already carried the correct attribution, so the plan and the
kickoff disagreed and the plan was right.

**How to apply:**
- Treat a kickoff's `file:line` and line-count claims exactly like a promoted potential's citations:
  as claims to re-derive, not inputs. `wc -l` is one command.
- A kickoff that predicts a specific finding is the highest-risk case, because the prediction primes
  both the orchestrator and the reviewer to confirm it. Brief the reviewer with the MEASURED value
  and tell it explicitly not to raise a finding on a file that is not in the diff.
- When the kickoff and the committed plan disagree on a fact, prefer the plan — it was written
  against the tree — then verify both against the file.
- Record the correction in the checkpoint (a `plan_drift_notes`-style key) so the epic parent can see
  the kickoff defect rather than silently inheriting it.

**Second instance, 2026-08-24 (#476 child of `quickfiler-bug-family`).** The kickoff asserted that
the `Compile Include` ItemGroup at `QuickFiler.Test/QuickFiler.Test.csproj:57-175` "is alphabetically
ordered", and instructed that any new test file's entry go "in the alphabetical WebView2*
neighbourhood". The group is NOT alphabetically ordered — around the WebView2 entry it reads
`...SeamFactoryTests`, `MailItemActionsAdapterTests`, `WpfUiDispatcherTests`,
`WebView2CoreInitializerTests`, `QfcQueueTests`. Acting on the stated ordering would have meant
re-sorting an ItemGroup the kickoff itself says is "shared with siblings" — a large gratuitous diff
in exactly the file most likely to conflict at fan-in. The salvaged spec had already caught this and
recorded it under a `### Premise correction` heading, and the plan's own decision (insert two
contiguous lines immediately after the existing WebView2 entry, do not re-sort) was right.

Note the shape: both instances are kickoff claims about a file's *structure* used to prescribe an
action. `sed -n '57,175p'` settles it. When the kickoff and the salvaged/committed artifacts
disagree, the artifacts were written against the tree — and both times they were correct.

Related: [[feedback_plan_phase0_paths_are_stale_in_epic_children]] (the same kickoff also named a
stale preparation worktree as WORKTREE), [[feedback_verify_subagent_capability_claims]],
[[feedback_verify_child_preflight_clearance]], [[parallel-epic-children-name-collisions]].
