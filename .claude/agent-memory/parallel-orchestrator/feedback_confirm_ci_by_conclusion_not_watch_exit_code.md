---
name: confirm-ci-by-conclusion-not-watch-exit-code
description: gh run watch --exit-status returns 0 on a CANCELLED run, so a watch exit code is not a green signal — always read the conclusion field against the final head
metadata:
  type: feedback
---

Never advance an item to `ci_green` on the exit code of `gh run watch --exit-status` or
`gh pr checks --watch`. Read the `conclusion` field, and read it for the run against the item's
FINAL head SHA.

**Why:** `gh run watch --exit-status` returned **0** on a run whose `conclusion` was `cancelled`,
observed on item 647 of run bugs-638-644-647 on 2026-09-01. A cancelled run has not failed, so the
command reports no failure — but it has not passed either, and nothing was verified. Treating that
exit code as green would have merged an item whose checks never ran to completion. The exit code and
the conclusion answer different questions, and only the conclusion answers the one the merge gate
cares about.

**The head SHA moves more often than expected, and each move supersedes the run below it.** The
concurrency group cancels the in-flight run whenever a new commit lands, so any late commit — agent
memory, a status doc, a checkpoint mirror — invalidates a run that may already have been green. The
correct sequence is to land every outstanding commit FIRST and let CI start once against the final
head, rather than committing after a green run and silently superseding it.

**How to apply:**

- At the parent, re-read `gh pr checks <N>` and `gh pr view <N> --json state,mergedAt,headRefOid`
  yourself before writing `ci_green`. A child's "CI is green" is a claim; the check table is the
  evidence. On this run the child's first report said green while all five checks were still
  `pending`.
- Confirm the check run belongs to the CURRENT `headRefOid`. A green run against a superseded head
  proves nothing about the head you are about to merge.
- `mergeStateStatus` is a useful cross-check: `BLOCKED` with `mergeable: MERGEABLE` means checks are
  outstanding, and `CLEAN` means they are satisfied. It disambiguates a pending-checks block from a
  merge conflict.
- Prefer watching from the parent over re-delegating. A child that finishes while CI runs will stop
  and notify repeatedly without advancing anything, costing a full agent turn each time.

**A red check is not automatically a remediation cycle. Re-run the failed job against the IDENTICAL head first.**
Observed 2026-09-07 on item 799 of run `bugs-2026-09-06`: `mstest-coverage` failed with 1 of 7108,
a `NullReferenceException` in a COM test class. Re-running that one job with no code change of any
kind passed. Same commit, two outcomes — which is the only clean discriminator between a flake and a
deterministic break, and it costs one job instead of a full remediation cycle.

Do the cheap diagnosis BEFORE re-delegating, because it changes who owns the fix:

- **Is `main` green at the tip the item merged from?** `gh run list --branch main --json headSha,conclusion`.
  A green `main` rules out an inherited red.
- **Did the item touch the failing code at all?** `git diff --name-only <main-tip> <item-head> -- <paths>`.
  An EMPTY diff over the failing file and its test is decisive: the item cannot have caused it directly.
- **Who last wrote the failing file?** `git log --oneline -1 <main-tip> -- <file>`. On this run it was a
  SIBLING item of the same run, merged an hour earlier.

Those three facts together identify the real shape: an interaction the item EXPOSES rather than a defect
it introduced.

**But they do NOT identify the mechanism, and the plausible mechanism was wrong.** On this run the
obvious story was test-isolation pollution: the failing class documents that it mutates static seams,
and the sibling item's own run had recorded an unguarded seam mutation there as a follow-up it declined
to fix. That story was REFUTED by reading the actual call path — all three seams are save/restored in
`finally` blocks, and none is even reachable from the failing method, which calls `EtlAsync` directly
while the seam is read only by the SYNCHRONOUS sibling method. The real cause was a timing race against
a 250 ms ETL deadline: an inert retry (the wrapped overload returns a faulted proxy Task, so the
`catch (TimeoutException)` never fires) let a swallowed timeout return null through a null-forgiving
suppression into a non-nullable tuple, which the caller then dereferenced. The failing run took 389 ms
against that deadline; the passing runs took 96 and 99 ms.

Two lessons. **A passing re-run establishes nondeterminism and nothing about the mechanism** — do not
let it license a plausible story. And **a genuine production defect can present as a flaky test**: this
one silently yields null on a slow ETL and surfaces as an NRE naming neither the folder nor the step,
which is the same defect class the sibling item was chartered to fix on the adjacent path it did not
cover. Trace the call path before filing; an issue filed under the plausible-but-wrong mechanism sends
the next reader looking in the wrong place.

**Do not let a child patch a sibling's file to green its own pull request.** The only fix available sits
outside the item's declared blast radius, so patching it widens the write set and falsifies whatever
scope assertion the plan already verified. Say so explicitly in the remediation prompt: report the
escape, do not silently widen. Both children on this run reached that conclusion independently, which
suggests it is the natural reading once the three facts above are in hand.

**Guard the merge when a child is live on the item worktree.** A remediation child can commit between
your green confirmation and your merge, and `gh pr merge` would then merge an unverified head. Pass
`--match-head-commit <sha>` naming the head you actually verified; it makes the merge atomic against
that race rather than relying on the gap between two commands. The SHA is safe against the merge gate's
digit scan described in [[issue-merge-and-removal-commands-bare]] — every digit run inside a hex SHA is
preceded by a word character, so the negative lookbehind excludes it — but keep the PR number ahead of
it in the command so the fallback still binds the right number.

**A passing re-run proves the failure is intermittent, not that it is harmless.** Record the latent
hazard and make sure it is promoted; the underlying defect is still there and will resurface under a
different interleaving.

See [[parallel-run-execution-playbook]] and [[issue-merge-and-removal-commands-bare]].
