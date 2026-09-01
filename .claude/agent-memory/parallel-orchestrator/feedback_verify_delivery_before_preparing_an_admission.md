---
name: verify-delivery-before-preparing-an-admission
description: Before spending a preparation cycle on a /parallel-add candidate, grep main for merged fix commits, read the guard sites, settle residual scope from the delivering feature's spec.md AC table, and test any merge claim with git merge-tree — an OPEN issue is not proof of outstanding work, and four distinct mechanisms make one unpreparable
metadata:
  type: feedback
---

Run a delivery pre-check BEFORE delegating the preparation child on any `/parallel-add` candidate:

```bash
git log origin/main --oneline --grep="fix(<N>)"      # merged fix commits for the issue
git merge-base --is-ancestor <sha> origin/main       # each one actually on main
```

If that returns commits, read the named guard sites out of `git show origin/main:<file>` before
delegating anything. Cheap, and it is the same evidence the preparation child would eventually
produce.

**A commit that REFERENCES the issue is not a commit that DELIVERS it.** Widen the grep to the bare
number (`--grep="<N>"`), because the delivering commit may carry a sibling issue's `fix(...)` scope;
but then decide delivery from the GUARD SITES, never from the subject line. Observed 2026-08-29 on
`/parallel-add 637`: commit `98b7a5e1 fix(quickfiler): reject rooted selections at filing boundary`
carries `Refs: #614, #637` and is on `main`, yet #637 was fully outstanding. #637 is a PRODUCER-side
normalization and that commit fixed the CONSUMER-side boundary guard — the two sit on opposite ends
of the same value's path, so the subject reads like delivery. Reading
`BreadcrumbBridgeRouter.SelectRow` on `main` settled it in one command: it still passed the rooted
value through `TryMakeArchiveRelative(..., out _)`, discarding the stem, and committed the rooted
value verbatim. When an issue body names the producer and the boundary as separate sites, check the
site the issue actually asks you to change.

**Why:** Issue state is a weak signal in this repository. A fix merged under a SIBLING issue's
feature folder, with a `fix(NNN):` subject that carries no closing keyword, leaves the issue OPEN
while the work is fully shipped. Observed 2026-08-29 on `/parallel-add 470`: preparation ran ~8
minutes and 137k tokens, then halted because all three defects were already on `main` — a two-second
grep would have caught it. See [[qfc-collection-468-family-shipped-issues-left-open]] for the
family this bit.

**Four families confirmed, so treat this as a repository-wide property, not a one-off.** The fourth
is [[breadcrumb-coordinator-501-family-shipped-issues-left-open]], caught on `/parallel-add 462` in
six tool calls with no preparation delegated. It is the worst case for the slug heuristic: the
delivering commit's subject names **neither the issue nor any feature slug** — it reads
`fix(breadcrumb): enforce close, lifetime, broadcast and lease invariants` — so the numbers survive
only in the merge subject and in the fix-commit BODY, and the bare-number grep is the sole finder.
Read the fix-commit body as well as the merge body: it carried one bullet per issue naming the exact
remedy, which made the guard-site read a single targeted grep. The third
is [[webview2-host-476-family-shipped-issues-left-open]], caught on `/parallel-add 458` in six tool
calls with no preparation delegated: the delivering commit `b1dec0c2` was scoped
`fix(webview2-host-476)` and named `#458:` only in its BODY, so the bare-number grep was again the
only one that found it. The second is
[[efc-464-family-shipped-issues-left-open]], caught by this pre-check on `/parallel-add 465`
before any preparation was delegated — zero tokens spent, versus ~137k on the 470 miss. The
generalized signal is a subject whose scope names a DIFFERENT issue's feature slug
(`fix(efc-464): correct the #465 action paths`): the slug is the FEATURE, so the bare-number grep
is the only one that finds it, and multi-issue feature folders close their own issue and orphan
every sibling. When the bare-number grep returns a commit scoped to another issue's slug, expect
delivery and go read the guard sites.

**Third mechanism, and it is NOT "already delivered": the premise never held.** An issue can be
outstanding, unreferenced by any commit, and still be unpreparable because its reproduction steps do
not reproduce. Observed 2026-08-31 on `/parallel-add 690`: the issue claimed branch
`feature/quickfiler-breadcrumb-bridge-coverage-r2` (#495) would "silently revert" the #440 fix on
merge. Grep found no delivering commit — the one bare-number hit was a cherry-pick trailer SHA
containing the digits `...227146**90**ca...`, a false positive worth expecting. But the hazard itself
was false: r2 changed 361 files since its merge base, ALL markdown, and its blobs for the named guard
files were byte-identical to the merge-base blobs, so it had modified none of them.

Decide this with `git merge-tree --write-tree <main> <branch>`, which performs a real three-way merge
writing nothing to refs or the worktree, then `git rev-parse <resulting-tree>:<guard-file>`. The
merged blob equalled main's post-fix blob: the merge PRESERVES the fix. That one command settles a
"would merging revert X" claim in a way no amount of reading the diff does. Note it exits 1 when
conflicts exist, so capture the tree from stdout line 1 rather than gating on exit status.

Why such a report is generated in good faith: an epic integration branch in its PREPARATION phase
carries only research and feature docs, so a reporter reading the branch's INTENT (it targets that
surface) rather than its CONTENTS concludes it will collide. Check contents.

Also weigh a structural ground independent of the facts: a parallel item must ship its own PR into
`main`. An issue whose remedy is a rebase or hygiene operation on ANOTHER branch yields no diff
against main and has no deliverable for this surface, whatever its merits.

**Fourth mechanism: the candidate is CLOSED and was shipped from the very branch a prepared-run
artifact names.** Check `gh issue view <N> --json state,stateReason` in the same first call as the
grep — a `CLOSED`/`COMPLETED` issue settles the question outright and costs nothing. Observed
2026-08-31 on `/parallel-add 635`: the issue was CLOSED, PR #688 was MERGED from
`bug/issue-468-residual-reflective-caller-risk-635`, and that branch was an ancestor of
`origin/main` with an EMPTY three-dot diff. The trap is that a `parallel-kickoff-*.md` artifact
still advertised #635 as a prepared, preflight-clear item of a ready-to-run parallel run — see
[[bugs-635-440-planned-run-is-obsolete]]. A prepared item branch is the most likely branch for a
standalone PR to have shipped from, so a kickoff artifact naming an item is weak evidence the item
is outstanding, not strong evidence. Verify the item, never the artifact.

**When the bare-number grep returns a MERGE commit, read its body first: `git log -1 --format=%B
<merge>`.** A squash-free merge subject carries only the PR title, but the body often enumerates
every issue the branch closed, which the constituent `fix(<slug>):` subjects never do. Observed
2026-08-31 on `/parallel-add 460`: the sole bare-number hit was `ee7d0ec4`, whose body reads
`fix(efc): close eight EFC controller-surface defects (#459, #460, #461, #463, #464, #465, #466,
#467)` — one command named the candidate as delivered and identified the family. This is why a
lone merge-commit hit is a strong signal rather than the weak coincidence it looks like: the
per-issue commits are invisible to the bare-number grep precisely because they are scoped to the
feature slug, so the merge is the only place the issue number survives.

Two cheap confirmations worth adding once the grep hits a merge commit:
`git merge-base --is-ancestor origin/<item-branch> origin/main` and
`git diff --name-only origin/main...origin/<item-branch>`. An empty diff is conclusive: there is no
deliverable left for a parallel item, which must ship its own PR into `main`.

**When the remedy was DELETION, the guard site is an ABSENCE — and a substring grep will lie to
you.** Confirming a deletion means proving a zero count, so the pattern you count has to be the
exact token the AC names, not the property path or method name it contains. Observed 2026-08-31 on
`/parallel-add 461`: the AC required zero occurrences of the token
`nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)`, which was indeed 0, but a
substring grep for `ConversationInfo.Expanded` returned a hit — an unrelated `.ForEach(item =>
item.ToggleDark(...))` iteration over that same collection in the dark-mode path. Read the matched
line before believing it; a delivered deletion otherwise reads as outstanding and buys a wasted
preparation cycle.

Pair the zero count with a POSITIVE: a deletion is only safe because some other route already
delivers the behaviour, and the delivering commit body names that route. Verify the surviving route
exists and that a named test pins it. For #461 that was `PopulateConversation` assigning
`SetTopicThread` to `ConversationResolver.UpdateUI`. An absence alone cannot distinguish "removed
because redundant" from "removed and the behaviour went with it".

**Settle RESIDUAL scope from the delivering feature's AC table, not by re-reading the guard sites.**
Guard sites answer "was the code changed"; they cannot answer "is anything left", which is the
question that made #469 a genuine item after an "already shipped" verdict. A multi-issue feature's
`spec.md` on `main` maps each sibling's defects to numbered ACs, so
`git show origin/main:<feature>/spec.md | grep -n "<N>"` returns those rows with their `[x]`/`[ ]`
state in one command. Observed 2026-08-31 on `/parallel-add 474`: AC-14 (#474 defect 1) and AC-15
(#474 defect 2) were both `[x]`, which closed the residual question that the source read alone had
left open. **A follow-up the delivering commit PROMOTED to its own issue is not residual scope for the
candidate.** Observed 2026-08-31 on `/parallel-add 462`: the fix commit recorded #655 and #656 as
real issues rather than prose, and #656 is a direct residual of the very field #462 asked to be
split. It still does not make #462 partially delivered — the promotion is what discharged it, per
[[promote-latent-defects-to-issues]] — so admit #656 on its own merits and reject #462. Discount a
`[ ]` AC that is pure bookkeeping — the 468 feature's AC-28 is "close all seven
issues" and is permanently unchecked while any sibling stays OPEN, so it flags no deliverable and
needs no branch. The whole #474 pre-check ran in five tool calls with no preparation delegated.

**How to apply:** Make the pre-check the first step of the add, before the `proposed` entry and
before the preparation delegation. When it shows the work is delivered — or that the defect does not
reproduce, or that it produces no diff against main — REJECT the admission rather than preparing it: the skill's own constraint already covers this outcome — "a failed preparation
appends no entry and leaves `items[]` without the candidate" — so append no `mutations[]` entry,
add no item record, and leave `recolor_generation` untouched.

Two things this pairs with. First, deferring the checkpoint write
([[defer-the-checkpoint-write-until-admission]]) is what makes the rejection free: because nothing
was written at `proposed` time, a rejection needs zero rollback and the post-rejection validation
comes back byte-identical to the pre-add baseline. Confirmed on this run. Second, always take that
BASELINE validation before starting, so a pre-existing failure from a concurrent add is not
misattributed to your own operation.

Also verify the halt claim rather than relaying it: a child's "already delivered" summary is a
claim, not evidence. Confirm ancestry and read the guard sites yourself before rejecting.
