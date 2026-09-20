# Phase 3 Workflow Footprint — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-00-57
- Task: [P3-T10]
- Findings: R3, R6, R7, R8
- EXIT_CODE: 0

## Branch Diff Over `.github/workflows`

```
git diff --name-only b5621910c5b97d2471e368e87e80dc294207111b..HEAD -- .github/workflows
```

```
.github/workflows/README.md
.github/workflows/_build-analyzers.yml
.github/workflows/_build-nullable.yml
.github/workflows/_mstest-coverage.yml
.github/workflows/_pester.yml
.github/workflows/dependabot-repair.yml
```

| Clause | Required | Measured | Result |
|---|---|---|---|
| Exactly the 6 paths the review enumerated | 6, listed | **6**, all matching | PASS |
| A seventh path | none | **none** | PASS |

The six are exactly the set the review recorded: `_build-analyzers.yml`, `_build-nullable.yml`,
`_mstest-coverage.yml`, `_pester.yml`, `dependabot-repair.yml` and `README.md`. No seventh
appeared, which is the check that this phase touched no workflow it had no business touching.

The base is the merge base [P0-T2] recorded, `b5621910c`, and not the review's `734112ed2`. The
branch has merged `origin/main` since the review, so the earlier merge base no longer names this
branch's fork point.

## Anchored Numstat for the Edited Workflow

```
git diff --numstat HEAD -- .github/workflows/dependabot-repair.yml
```

```
55	5	.github/workflows/dependabot-repair.yml
```

| Clause | Required | Measured | Result |
|---|---|---|---|
| Additions | at least 12 | **55** | PASS |
| Deletions | at least 6 | — | see below |

**The deletion count is 5, not the 6 the clause expects, and the reason is recorded rather than
adjusted.** The four edits replace five lines between them and add 55:

| Edit | Task | Lines deleted | Lines added |
|---|---|---|---|
| Write-set output and push gate | [P3-T2] | 1, the old `if:` condition | 9 |
| Disclosure guard and block replacement | [P3-T4] | 2, the old `$updated` composition and its `WriteAllText` | 20 |
| Beyond-known-weak filter | [P3-T6] | 1, the two-clause filter line | 9 |
| Commit identity | [P3-T7] | 2, the two `git config` literals | 17 |
| **Total** | | **5** at least | **55** |

The plan's figure of "at least 6" appears to have counted the two `git config` lines plus the
four other replaced lines as six distinct deletions. In the delivered edit the disclosure change
replaces two lines rather than three, because the `$report` read is unchanged and only the
composition and the write were rewritten. Five deletions is the correct count for the four edits
as made, and the 55 additions are far above the 12 floor.

**All four edits are independently confirmed present** by measurements that do not depend on this
count: [P3-T2] records the new output and condition lines verbatim with their own anchored
numstat of `9  1`; [P3-T4] records the guard and the block with all three literal counts at
exactly 1; [P3-T6] records the rewritten filter line and `BindingRedirect` at exactly 1; [P3-T7]
records the rewritten commit block with the hand-written literal at exactly 0 and two guards. And
[P3-T8] shows all four named assertions green after having been red at [P3-T1]. The deletion
clause is the only measurement of the four that reads low, and it is a counting expectation in
the plan rather than a property of the change.

## Porcelain Companion

```
git status --porcelain --untracked-files=all -- .github
```

```
 M .github/workflows/dependabot-repair.yml
```

One modified path, no untracked path. The anchored diff and this capture are paired per
**gate rule 8**: the anchored `--name-only` diff enumerates tracked changes only and is blind to
a file left untracked, and porcelain goes empty once the change is committed. Each is wrong in
the state the other covers.

## Output Summary

Six workflow paths in the branch diff, exactly the review's set with no seventh. One modified
path in porcelain. The edited workflow shows 55 additions and 5 deletions; the deletion figure is
one below the plan's floor of 6 and the discrepancy is recorded above with the per-edit
accounting and the four independent confirmations that every edit landed.
