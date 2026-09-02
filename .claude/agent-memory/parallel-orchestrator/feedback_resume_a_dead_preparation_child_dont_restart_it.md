---
name: resume-a-dead-preparation-child-dont-restart-it
description: When a /parallel-add preparation child dies, its work is committed on the item branch — detach the stale worktree and re-delegate only the missing preparation step, never restart preparation from promotion
metadata:
  type: feedback
---

A `/parallel-add` preparation child that dies mid-run usually leaves its work COMMITTED on the item
branch, not lost. Before re-delegating, read what survived and scope the resume to the gap.

**Why:** Preparation is expensive (~8 minutes, >100k tokens) and its outputs are durable. Observed
2026-08-29 resuming `/parallel-add 637`: the item sat at `admitted` in the checkpoint and looked
abandoned, but `git log origin/main..<item-branch>` showed one commit,
`wip(637): checkpoint preparation artifacts before session pause`, carrying `issue.md`, `spec.md`,
a 1338-line atomic plan, and the research artifact. Only preflight clearance was missing. Restarting
preparation would have rebuilt all four and produced a second plan file, violating the Plan-Path
Continuity Contract.

**How to apply:**

- **Diagnose from the branch, not the checkpoint.** `git log origin/main..<item-branch> --oneline`
  and `git ls-tree -r --name-only <head> -- <feature-folder>/` tell you exactly which preparation
  outputs exist. The checkpoint's item `state` does not: it says `admitted` whether preparation
  wrote nothing or wrote everything but preflight.
- **A live lock pid does not mean a live child.** The stale worktree carried
  `locked claude agent <id> (pid <n>)` and that pid was a running `claude.exe`, because the lock pid
  is the SESSION, not the subagent. Judge liveness from the worktree's index/HEAD mtimes and from
  the child's own last commit message.
- **Free the branch by detaching the stale worktree**, then let the fresh child check the existing
  branch out by name as its first action. See [[free-item-branches-by-detaching]] — both removal
  gates fail closed, so `git worktree remove` is not the move.
- **Enumerate the done set in the delegation prompt.** Name promotion, research, feature documents,
  and plan authoring as already complete, and state the remaining step as the whole scope. Also
  instruct the child to revise the existing plan IN PLACE, or it will author a timestamped sibling.
- **Tell the child the plan may have gone stale.** The plan cites line numbers against the `main` of
  its authoring time; if `main` advanced, correcting those citations is legitimate preflight work
  rather than scope creep.
- **Prove the preflight gap by SEARCHING for the signal, not by inferring it from the last commit
  subject.** On `/parallel-add 670` the branch's last commit read `close 11 preflight defects in the
  atomic plan`, which is consistent both with "revised, awaiting the confirming round" and with
  "revised and cleared". A repository-wide grep for `PREFLIGHT: ALL CLEAR` settled it in one
  command: every hit belonged to an unrelated feature folder and none to this item, so the
  confirming round was genuinely missing. Scope the grep to the ITEM's own folder when reading the
  result — the repository is full of other items' clearances and they match the same pattern.
- **Expect a resumed preflight to need more than one round, and read the round count as a signal
  rather than as failure.** 670 took five rounds, 27 defects, against a two-round target. The cause
  was sibling invalidation: rounds 3 through 5 were almost entirely consequences of the preceding
  round's own fix, where replacing a gate taxonomy or adding a sweep invalidated an assumption in a
  neighbouring task. The round that finally converged was the first to bundle the consequential
  sibling fix into its own delta. When relaying this, say which rounds were misses and which were
  self-inflicted; the two have different remedies.

**The commonest death point is AFTER preflight and BEFORE the commit, and that gap needs no child
at all.** Observed 2026-09-01 on `/parallel-add 633`: the child's last words were
`PREFLIGHT: ALL CLEAR` plus "finalizing the checkpoint, then committing and pushing", and it left
`issue.md`, `spec.md`, the research artifact and a 1,642-line plan UNCOMMITTED in its worktree with
the item branch still at the `origin/main` tip — so `git log origin/main..<branch>` was empty and
the branch-diagnosis step above reported nothing. Look at `git status --porcelain` in the worktree
as well as the log; an empty log with a dirty tree is this case, not a child that did nothing.

The parent can close that gap itself in two commands rather than re-delegating an entire
orchestrator boot: `git -C <wt> add <feature-folder>` then
`git -C <wt> commit -m ... -- <feature-folder>`, both of which clear the pre-implementation gate's
operand exemption because the pathspec is under `docs/features/active/`. Verify first that the
work is real rather than assumed — run the plan through
`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"`, passing the
WORKTREE as `workspace_root`, and confirm the work-mode marker and the AC section. Say in the
commit message that the parent completed only the terminal commit step and authored no plan or
specification content.

What the parent CANNOT commit is the child's `.claude/agent-memory/**` writes: those paths are
outside the operand exemption, and the parent has no `orchestrator-state.json` to satisfy the
gate's other allow path. Leave them, and note the consequence for the radius —
[[reconcile-derived-radius-against-branch-diff]] covers it.

**Require the resumed child to COMMIT its clearance as an evidence artifact.** The grep-for-the-signal
step above keeps being necessary because the clearance normally lives only in the child's returned
text and in an untracked checkpoint, both of which evaporate when the child dies. Instruct the child
to write `evidence/other/preflight-clearance.<yyyy-MM-ddTHH-mm>.md` into the item's own feature folder
carrying the `PREFLIGHT: ALL CLEAR` line, the round count, and the plan blob it cleared. Observed
2026-09-01 on `/parallel-add 287`: the resumed child did this unprompted after finding that the
previous attempt's clearance had vanished, and the signal is now greppable in the committed tree
for any future reader. One committed line removes a whole diagnostic step.

**A RECORDED clearance is not a trustworthy one — check what it was pinned to.** On the same 287
resume, the dead attempt's untracked checkpoint pinned `PREFLIGHT: ALL CLEAR` to a plan blob
byte-identical to the plan the resumed child started from, so on its face the plan was already
cleared and the resume was unnecessary. A fresh transitive review of those exact bytes found two
blocking defects, one of them an under-scoped analyzer preflight that would have reported four
passing checks and then failed the next gate. So a recorded clearance plus an unchanged blob does
NOT license skipping the confirming round. This is the same failure the round-count guidance in
[[self-review-before-preflight-round-one]] describes — a review scoped to the files the plan names
rather than the transitive set its edits force — and it is why the earlier rounds passed.

**A plan file's EXISTENCE proves nothing: `new_active_feature_folder` emits a scaffold plan.** Add a
third diagnostic to the log-and-status pair above — open the plan and check it for `### Phase`
headings and for the `PREFLIGHT:`/`SELF-REVIEW:` signals. Observed 2026-09-01 on `/parallel-add 678`:
the dirty worktree held `issue.md`, an 837-line research artifact, and `plan.<timestamp>.md`, which
reads as a complete preparation until you open the plan and find 44 lines of the untouched template
with placeholder tasks like `[P0-T1] Link approved spec: <spec link>`. Plan authoring had never run.
Scoping the resume from the file list alone would have skipped straight to preflight on a scaffold.
A real minor-audit plan here runs to hundreds of lines; 44 with zero `### Phase` headings is the
scaffold's signature.

So the death points form a ladder, and each needs a different resume scope: died before planning
(commit what survived, re-delegate planning AND preflight); died after planning, before preflight
(re-delegate preflight only); died after preflight, before the commit (no child at all — the parent
commits). Read the plan to tell the first two apart.

**Commit the scaffold anyway.** It is the canonical plan path, so committing it is what makes the
resumed planner revise IN PLACE instead of authoring a timestamped sibling. Say in the commit
message which artifacts are real and which is still a scaffold, so the next reader is not misled the
same way.

**Also expect a stray untracked copy of the feature folder in the SESSION worktree.** The MCP
folder-creation tool writes to the server's `workspace_root`, not to the child's isolated worktree,
so the session tree gets its own `issue.md` — the pre-AC original, while the child's authored copy
lives in the child's worktree. Two consequences: the session copy is NOT the authoritative one, and
it is untracked, so a `git add -A` in the session tree would sweep it onto whatever branch is
checked out there. Leave it and commit from the child's worktree with an explicit pathspec.

**A fourth rung sits above the ladder: preparation FINISHED and only the checkpoint write was lost.**
Observed 2026-09-01 on `/parallel-add 648`. The checkpoint held no 648 item at all, which reads as
"never started" — but that is precisely the deferred-write posture
[[defer-the-checkpoint-write-until-admission]] prescribes, so an ABSENT item is ambiguous between
"never attempted" and "prepared but never written". Resolve it by looking for the item BRANCH before
concluding anything: `git branch -a --list "*<N>*"` is one call, and a pushed
`bug/<slug>-<N>` branch means a preparation ran. Here it carried the feature folder, an 837-line
research artifact, and a 332-line three-phase plan revised across three preflight rounds.

The clearance was recoverable even though nothing was greppable in the item folder. The child's
untracked `artifacts/orchestration/orchestrator-state.json` survived in its worktree, and reading
`completed_steps` plus the `preflight_round_*` keys settled it outright: steps through `S4_preflight`
complete, `next_step: S5_atomic_execution`, and `preflight_round_4: PREFLIGHT: ALL CLEAR`. So when the
item-folder grep comes back empty, read the child's checkpoint before assuming preflight is missing —
the two diagnostics disagree, and the checkpoint is the one carrying the answer. It also narrates the
round history, which is worth relaying: four rounds here, and round 2 was killed by a rate limit and
re-run from scratch because preflight writes no artifact.

At this rung there is no child to re-delegate at all — not even the terminal commit, which the child
had already made and pushed. The whole remaining operation is: re-derive the radius from the committed
plan, reconcile it against the branch diff, decide admission, write. Confirm liveness first, though:
an UNLOCKED worktree whose only dirty path is the tracked `orchestrator-state.json` is a finished
child, not a running one (see [[orchestrator-state-json-tracked-on-main]]).

**Re-derive the radius rather than reusing the child's recorded one.** The 648 child recorded
`blast_radius` with `source`, `computed_at`, `modules`, `shared_surfaces`, `contracts` and a
`path_count` of 91 — but NOT the `paths` array itself, the same scratchpad loss that hit item 644.
Re-running `Get-BlastRadius` over the committed plan and `issue.md` reproduced all four recorded
values exactly, which turns the recorded `path_count` into a free correctness check on the
re-derivation. Ask for it: a re-derived count that disagrees with the recorded one means the plan
moved under you.

**The committed-clearance recommendation has now paid off, and it collapses the fourth rung to a
five-minute diagnosis.** Observed 2026-09-01 on `/parallel-add 662`, the second fourth-rung case
after 648. The item was absent from `items[]`, but `git branch -a --list "*662*"` found a pushed
branch carrying fourteen commits, one of which was
`docs(issue-662): record preflight round 3 clearance` — an `evidence/other/preflight-round-3-clearance.md`
artifact holding `PREFLIGHT: ALL CLEAR`, `CONVERGENCE: NO FURTHER ROUNDS EXPECTED`, the round count,
the plan of record, and the tree it reviewed. Compare 648, where the same rung required opening the
dead child's untracked `orchestrator-state.json` because nothing was greppable in the item folder.
Read the clearance artifact FIRST when the branch log shows one; the child checkpoint then only
needs to corroborate it (`completed_steps` through `S4_preflight`, `next_step: S5_atomic_execution`),
rather than being the sole source. The whole 662 add ran with no child delegated at all.

Two cheap confirmations to keep pairing with it, both of which held on 662: the clearance names the
tree it reviewed (`db59adfe` here), so check that the later commits touched only evidence and
agent-memory rather than the plan; and re-run the committed plan through
`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"`, passing the
child's WORKTREE as `workspace_root`. Also re-verify mode integrity from the branch, not from
memory of what the folder should hold — for `minor-audit` that is `- Work Mode: minor-audit` plus an
explicit `## Acceptance Criteria` in `issue.md`, and the ABSENCE of `spec.md` and `user-story.md`.

**Three fourth-rung cases in a row now (648, 662, 663), so on a long-running parallel add this is
the FIRST hypothesis, not the last.** Reorder the diagnosis accordingly: before assuming a candidate
was never attempted, run `git branch -a --list "*<N>*"`, and if a branch exists go straight to
`git log origin/main..<branch> --oneline` looking for a clearance commit. On `/parallel-add 663`
that sequence identified the whole situation in three calls — the branch carried ten commits ending
in `docs(issue-663): clear the atomic plan through preflight and record the rounds`, which named
itself. The entire add then ran with no child delegated.

Two confirmations that keep earning their cost on this rung:

- **A clean worktree plus a recent last-commit time is a FINISHED child, not a running one.** On 663
  the worktree was locked (`pid 23120`) and its last commit was under three minutes old, which reads
  alarmingly like live work. `git status --porcelain` returning empty settled it: a running
  preparation child has a dirty tree essentially all the time, and the lock pid is the session, not
  the subagent.
- **Re-verify work-mode integrity from the BRANCH for the mode the folder actually declares.** The
  earlier note covers `minor-audit`; 663 was `full-bug`, whose shape is `- Work Mode: full-bug` plus
  `spec.md` PRESENT and `user-story.md` ABSENT. Checking for the minor-audit shape would have
  reported a false integrity failure on a correct folder.

**The same do-not-restart rule governs an EXECUTION child, and its durable evidence is different.**
Observed 2026-09-01 on item 287 of run `bugs-638-644-647`: the child stopped with no DONE report,
saying only that it would wait for the CI watch. Restarting would have re-run a full C# delivery. Four
reads settled it instead — `gh pr list --head <branch>` showed pull request 716 OPEN and MERGEABLE,
the branch tip matched locally and on `origin`, the worktree was clean, and
`git show <branch>:<feature-folder>` listed `code-review`, `policy-audit` and `feature-audit` all
committed at the same timestamp. Grepping those three for `Overall verdict` and for a Blocking
severity returned PASS and zero. So execution, review and PR authoring were all complete and the ONLY
outstanding work was the CI wait — which is the PARENT's under merge-on-green, not the child's.

The generalisable point: for an execution child the committed AUDIT ARTIFACTS are the durable proof
that review finished, exactly as a committed clearance artifact is for preparation. Check them before
concluding anything from the child's silence, and never merge on the artifacts' mere existence — read
the verdict line and the Blocking count, because a review that ran and FAILED also leaves three files.

See [[defer-the-checkpoint-write-until-admission]] for why the checkpoint stays untouched while the
resumed preparation runs, and [[parallel-run-execution-playbook]].
