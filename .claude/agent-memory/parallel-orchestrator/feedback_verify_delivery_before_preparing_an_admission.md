---
name: verify-delivery-before-preparing-an-admission
description: Before spending a preparation cycle on a /parallel-add candidate, grep main for merged fix commits and check the guard sites — an OPEN issue is not proof of outstanding work
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

**Two families confirmed, so treat this as a repository-wide property, not a one-off.** The second
is [[efc-464-family-shipped-issues-left-open]], caught by this pre-check on `/parallel-add 465`
before any preparation was delegated — zero tokens spent, versus ~137k on the 470 miss. The
generalized signal is a subject whose scope names a DIFFERENT issue's feature slug
(`fix(efc-464): correct the #465 action paths`): the slug is the FEATURE, so the bare-number grep
is the only one that finds it, and multi-issue feature folders close their own issue and orphan
every sibling. When the bare-number grep returns a commit scoped to another issue's slug, expect
delivery and go read the guard sites.

**How to apply:** Make the pre-check the first step of the add, before the `proposed` entry and
before the preparation delegation. When it shows the work is delivered, REJECT the admission rather
than preparing it: the skill's own constraint already covers this outcome — "a failed preparation
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
