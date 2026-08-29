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

**Why:** Issue state is a weak signal in this repository. A fix merged under a SIBLING issue's
feature folder, with a `fix(NNN):` subject that carries no closing keyword, leaves the issue OPEN
while the work is fully shipped. Observed 2026-08-29 on `/parallel-add 470`: preparation ran ~8
minutes and 137k tokens, then halted because all three defects were already on `main` — a two-second
grep would have caught it. See [[qfc-collection-468-family-shipped-issues-left-open]] for the
family this bit.

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
