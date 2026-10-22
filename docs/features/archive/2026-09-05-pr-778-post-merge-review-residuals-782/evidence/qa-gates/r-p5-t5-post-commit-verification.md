# [P5-T5] Post-commit verification

Timestamp: 2026-09-06T02-00

Command:

```powershell
git rev-parse pre-782-base
git diff --name-only pre-782-base..HEAD -- .claude
git diff --name-only e01cf434197d34e0fff1ba408616dc175dfa5fd6..HEAD -- '*.cs'
git status --porcelain --untracked-files=all
```

All four were run from the worktree root after the [P5-T4] commit `b91dd859`. The base SHA in the
third command was read from the `REMEDIATION-BASE-SHA:` line of
`evidence/remediation-baseline/r-p0-t11-anchor.md` rather than from any value tabled in the
remediation plan.

EXIT_CODE: 0

Output Summary: the `pre-782-base` tag is unmoved, no `.claude/` path differs across the branch, the
C# diff lists exactly the two `UtilitiesCS.Test` files, and the porcelain status lists only the two
paths the plan anticipates.

### 1. `git rev-parse pre-782-base`

```text
736c2cf234cdd71b604c908f348b6aa89b256b53
```

The value begins `736c2cf2` and is byte-identical to the value [P0-T11] recorded before Phase 1. No
task in this remediation created, moved, deleted, or re-pointed the tag.

### 2. `git diff --name-only pre-782-base..HEAD -- .claude`

```text
(no output)
```

DOTCLAUDE_DIFF_LINES: 0

### 3. `git diff --name-only <REMEDIATION-BASE-SHA>..HEAD -- '*.cs'`

```text
UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs
UtilitiesCS.Test/Threading/UiThread_Tests.cs
```

CS_DIFF_LINES: 2

Exactly the two files the remediation edits, and no other `.cs` path. In particular
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, mutated temporarily by [P1-T5], is absent,
which is the post-commit confirmation that the [P1-T8] revert reached the commit.

### 4. `git status --porcelain --untracked-files=all`

```text
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/remediation-plan.2026-09-06T00-15.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p5-t3-staged-set.md
```

PORCELAIN_LINES: 2

Both are expected. The plan file is modified because it carries the [P5-T2] through [P5-T4]
check-offs written after the staging that produced the commit, and `r-p5-t3-staged-set.md` is
untracked because it was written after `git add` ran and therefore could not be a member of the
staged set it records. Both are under this feature's folder. `TestResults/`, `coverage/`, and
`artifacts/` are git-ignored by `.gitignore:39`, `.gitignore:144`, and `.gitignore:57` and are
correctly absent.

## The base-SHA read, and the counting convention applied to it

`Select-String -SimpleMatch 'REMEDIATION-BASE-SHA:'` over `r-p0-t11-anchor.md` returns two matching
lines: the field line, and one prose line that quotes the key in backticks while naming its consumer.
Counted as the plan counts its other artifact field keys — as **line-start** fields, the convention
the plan's "Evidence locations" section states for `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`Output Summary:` — the count is **1**, which is what [P0-T11]'s acceptance requires. The measurement
is recorded here explicitly rather than left implicit:

```text
LINE_START_COUNT=1
CONTAINS_COUNT=2
```

The value read was `e01cf434197d34e0fff1ba408616dc175dfa5fd6`, taken from the single line-start
field.

## Why both an anchored diff and a porcelain status are required

This task is the post-commit counterpart of [P4-T6]'s pre-commit porcelain enumeration, and the pair
is required because each mechanism is blind in one state. A name-listing diff enumerates tracked
changes only, so it cannot see an untracked path; a porcelain status goes empty once a change is
committed. [P4-T6] ran the porcelain form while the two edits were uncommitted, and this task runs
the anchored diff now that they are committed. The two agree on the same two files.
