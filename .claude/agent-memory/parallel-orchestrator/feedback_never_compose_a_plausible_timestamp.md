---
name: never-compose-a-plausible-timestamp
description: Every timestamp written to the checkpoint must come from a clock read or a converted gh value — I once composed a plausible-looking one while writing, and it landed 44 minutes in the future
metadata:
  type: feedback
---

Read the clock, or convert a `gh` UTC value. Never compose a timestamp because one is needed and the
surrounding values make a plausible one obvious.

**Why:** on run `bugs-2026-09-06` (2026-09-08) I wrote `pr_opened_at`, `last_updated` and a child
report's `recorded_at` as `2026-09-08T09-30` while composing a checkpoint write. That value came from
nowhere. It was not read from a clock, not returned by any command, and not converted from anything.
It simply looked right beside the timestamps already on the record. When I read the wall clock a few
minutes later it said `08-53`, and `gh pr view --json createdAt` said `12:46:48Z`, which converts to
`08-46`. My invented value was about 44 minutes in the FUTURE.

The reason this matters is specific rather than general tidiness. The Layer-2 barrier's TEMPORAL
reading rejects `merged_at(earlier) > worktree_created_at(later)`. A future-dated timestamp can
therefore fabricate an ordering violation that never happened, or mask a real one, and the checkpoint
carries no way to tell an invented value from a read one. It is the same untrustworthy-timestamp
family as the item 809 worktree checkpoint that self-reported an hour ahead of its own file mtime,
and as [[never-mix-gh-utc-with-local-timestamps]].

**How to apply:**

- Run `pwsh -NoProfile -Command "Get-Date -Format 'yyyy-MM-ddTHH-mm'"` immediately before a write that
  needs a stamp. It costs one call. Do not carry a stamp forward across several writes separated by a
  long CI wait — on this run the gap between two adjacent checkpoint writes was over two hours.
- For a lifecycle moment that already happened, take it from `gh` and CONVERT: `createdAt`,
  `mergedAt` and `closedAt` are UTC, every checkpoint lifecycle timestamp is local, and the offset on
  this machine is UTC-4. Keep the raw UTC value beside the converted one rather than discarding it.
- **Correct it in place and record the correction.** When you find a fabricated value, fix it and
  write a short record naming what was wrong, what the real value is, how you established it, and why
  it mattered. A silent fix leaves the next reader unable to tell which other values were composed.
- The tell is a timestamp that was never the output of anything. If you cannot name the command that
  produced it, you invented it.
