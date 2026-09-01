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

See [[parallel-run-execution-playbook]] and [[issue-merge-and-removal-commands-bare]].
