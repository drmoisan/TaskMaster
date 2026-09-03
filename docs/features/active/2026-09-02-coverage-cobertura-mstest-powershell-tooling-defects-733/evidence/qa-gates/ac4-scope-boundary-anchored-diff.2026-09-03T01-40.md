# AC4 Scope-Boundary Verification — Anchored Diff (P5-T9)

Timestamp: 2026-09-03T01-40

Task: [P5-T9] [AC4] "No unintended behavior changes outside the defined scope."

## Why this artifact supersedes the plan's stated check

P5-T9 as written verifies scope compliance with a repository-root `git status --porcelain`
and requires every reported path to fall under one of exactly three allowed prefixes:
`scripts/vscode/`, `tests/scripts/vscode/`, or the feature folder.

The plan chose porcelain status deliberately and, at plan-authoring time, correctly: it
recorded that no task in the plan stages or commits before P5-T9 runs, so an anchored
`git diff` against a ref would have reported nothing regardless of what the executor
touched. Under that state the anchored form was vacuous and porcelain status was the only
non-vacuous option.

That condition no longer holds. The item's work is now committed at 6c9329a3, which
inverts the two checks:

- Porcelain status now reports only files that are NOT part of this item's footprint,
  because everything in the footprint has been committed and is therefore absent from it.
- The anchored diff now reports exactly this item's committed footprint, and can fail if
  any out-of-scope path was committed.

The anchored diff is therefore the stronger check for the property AC4 actually asserts,
and it is recorded here alongside the porcelain output rather than in place of it.

## Command 1 — anchored footprint diff

Command: git diff origin/main...HEAD --name-only

EXIT_CODE: 0

Base: origin/main = 8be5a6aac3b5a82c86241fbbf989fd9118602c56
Head: HEAD = 6c9329a3599a590ac7699d48d103f96de0d0ac5d

Three-dot degeneration note, recorded so a later reader does not have to re-derive it:
origin/main is an ancestor of HEAD, because this branch merged origin/main at 357b5770.
Where the base is an ancestor, `A...B` and `A..B` select the same commit range. That
degeneration is benign here and does not inflate the footprint: origin/main at 8be5a6aa
already contains every commit this branch merged in, so the merged sibling content appears
on both sides of the comparison and is excluded from the diff. What remains is this item's
own work only.

Result: 63 paths. Every path falls under one of the three allowed prefixes.

Prefix distribution (recounted mechanically; see the correction note below):
- docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/ : 49
- scripts/vscode/ : 6
- tests/scripts/vscode/ : 8

49 + 6 + 8 = 63, reconciling with the total.

Paths outside the three allowed prefixes: 0.

CORRECTION. The first version of this artifact recorded the distribution as 51 / 6 / 6.
Those figures were wrong: the feature-folder count was overstated by two and the test count
understated by two. The error was caught by the feature-review pass, which recounted
independently rather than accepting the figures as written, and the counts above were then
re-derived mechanically by filtering the diff output per prefix. The verdict is unaffected,
because it turns on the count of paths OUTSIDE the three prefixes, which was zero under both
counts and remains zero. The correction is recorded rather than silently overwritten, since
an evidence artifact that was wrong once should show that it was corrected and by what.

The 49 feature-folder paths comprise issue.md, spec.md, plan.2026-09-02T12-01.md, the
research findings document, and 45 evidence artifacts. issue.md and the research document
were committed earlier in the planning commit f782d4fa and appear here because the diff is
anchored at origin/main rather than at the last commit. The 8 test paths are the three
pre-existing test files plus the five added by this item.

## Command 2 — repository-root porcelain status (the plan's literal check)

Command: git status --porcelain

EXIT_CODE: 0

Verbatim output:

```
 M .claude/agent-memory/orchestrator/MEMORY.md
?? .claude/agent-memory/orchestrator/powershell-change-budget-override-for-consolidated-issue.md
?? .claude/agent-memory/orchestrator/pwsh-blanket-blocked-in-isolated-worktree-for-orchestrator.md
```

Three paths are reported and none falls under the three allowed prefixes, so the literal
check as authored does not pass. Disposition of each:

All three are orchestrator agent-memory files that predate this item's implementation work
entirely. They were present in the worktree before the first Phase 1 edit and are recorded
as such in the run checkpoint under `resume_record.worktree_dirt_at_resume`, which was
written at resume time before any implementation began. Every executor delegated during
this run was explicitly prohibited from writing under `.claude/agent-memory/`, and each
confirmed independently that it did not touch them. They are session noise produced by
other agents and are outside this item's footprint by the launching directive.

They were not deleted, not committed, and not reverted, because none of those actions is
this item's to take: modifying or discarding another agent's memory files would itself be
an out-of-scope change, which is the exact class of action AC4 exists to prevent. The
correct handling is to leave them untouched and to demonstrate their absence from the
committed footprint, which Command 1 does.

## Verdict

AC4 PASSES on the substantive property it asserts: this item committed no change outside
`scripts/vscode/`, `tests/scripts/vscode/`, and its own feature folder. The evidence is
Command 1's anchored diff, which enumerates the complete committed footprint at 63 paths
with zero paths outside those three prefixes, and which is capable of failing had any
out-of-scope path been committed.

The three porcelain-reported paths are not counter-evidence to that property. They are
uncommitted, pre-existing, third-party files that Command 1 proves are absent from this
item's committed footprint.

## Output Summary

Anchored diff origin/main...HEAD reports 63 paths, all within the three allowed prefixes
(49 feature folder, 6 scripts/vscode, 8 tests/scripts/vscode), zero outside. Porcelain
status reports three uncommitted orchestrator agent-memory paths that predate this item's
work, were never touched by it, and are absent from the committed footprint. AC4 is
satisfied on the substantive property; the literal porcelain-only formulation is superseded
by the stronger anchored check now that a commit exists.
