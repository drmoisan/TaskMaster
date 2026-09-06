---
name: never-mix-gh-utc-with-local-timestamps
description: gh mergedAt is UTC while every other checkpoint lifecycle timestamp is local, so copying it verbatim fabricates a PARALLEL_COHORT_BARRIER_VIOLATION that never happened
metadata:
  type: feedback
---

Convert a `gh` timestamp to local before writing it into `items[].merged_at`. Never copy the
`mergedAt` field verbatim.

**Why:** `gh pr view --json mergedAt` returns UTC (`2026-09-03T05:05:10Z`). Every other lifecycle
timestamp on the parallel surface — `worktree_created_at`, `pr_opened_at`, `ci_green_at` — is
written from the local clock, which on this host is UTC minus four hours. The retrospective
cohort-barrier validator applies a TEMPORAL reading that rejects
`merged_at(earlier) > worktree_created_at(later)`, and it compares the two strings ordinally with
no timezone awareness. A verbatim UTC merge time therefore reads four hours late and trips
`PARALLEL_COHORT_BARRIER_VIOLATION` against every item launched in the four hours after that merge.

Observed on run `bugs-2026-09-02`, 2026-09-03: item 729 merged at 01-05 local, items 735 and 737
were launched at 01-08 local after the barrier was re-evaluated on durable state — a correct,
three-minute-later launch. Recording 729's merge as `2026-09-03T05-05` produced two violation lines
naming 735 and 737. **Nothing was wrong with the schedule; the report was an artifact of the units.**

**How to apply:**

- The failure is silent in one direction and loud in the other. A merge whose UTC value happens to
  sort BELOW the following launch stamp passes and leaves the inconsistency in place: item 564's
  merge at `02:04Z` was recorded as `02-04` and compared against a launch stamp of `02-15`, so it
  validated while being just as wrong. Do not treat a passing validation as evidence the scale is
  consistent.
- Derive the offset once with `datetime.now() - datetime.utcnow()` and apply it to every `gh` value
  you record. Do not eyeball it: the sign is easy to invert and both directions produce a plausible
  string.
- The same hazard applies to any field the validators compare ordinally, which includes the drift
  gate's `computed_at` versus `at` comparison. That gate fails CLOSED on an ordinal mismatch, so a
  UTC/local mix there denies review rather than reporting a violation.
- Do not invent a local timestamp from memory of when something happened. Read the clock. My own
  invented stamps were running roughly two hours ahead of real local time, which is what put the
  UTC merge value and the launch value on opposite sides of the comparison in the first place.

See [[parallel-run-execution-playbook]] and
[[confirm-ci-by-conclusion-not-watch-exit-code]].
