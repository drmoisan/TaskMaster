# Absolute Host-Path Census — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-27-55
- Task: [P0-T3]
- Finding: R4
- EXIT_CODE: 0

Every path below is repository-relative and every variant is recorded in placeholder-normalised
form. The raw literal appears nowhere in this artifact, per **gate rule 17**.

## The Derived Search Pattern

The pattern is **built at run time and never typed**. Recorded as the expression that built it,
not as its value:

```powershell
$acct    = Split-Path $HOME -Leaf
$short   = $acct.Substring(0, 6) + '~'
$pattern = [regex]::Escape($acct) + '|' + [regex]::Escape($short)
$rx      = [regex]::new($pattern, 'IgnoreCase')
```

This yields the long spelling and the 8.3 spelling, matched case-insensitively. Its length is 17
characters. Recording the expression rather than the value is what keeps this artifact, and the
residual assertion at [P4-T3], satisfiable: an artifact that typed the pattern would match itself.

## Scope

```
git diff --name-only b5621910c5b97d2471e368e87e80dc294207111b..HEAD
git status --porcelain --untracked-files=all
```

The union, restricted to paths that exist as files: **206 paths**. The merge base is the value
[P0-T2] recorded. Scoping to the branch footprint is required: the repository carries the same
literal in more than a thousand historical documents that this cycle must not touch.

## Totals — Two Distinct Metrics

| Metric | Value |
|---|---|
| Matching **lines** | **94** |
| Matching **occurrences** | **103** |
| Matching **files** | **33** |
| In-scope paths examined | 206 |
| `PLACEHOLDERS-BEFORE`, four tokens, whole 206-path scope | **121** |
| `PLACEHOLDERS-BEFORE`, four tokens, the 33 matching files only | **9** |

The metric is ambiguous between lines and occurrences, so both are recorded and every later clause
names which it reads. The review measured **74 occurrences across 27 files**; both figures have
grown, which is the expected direction: the four review artifacts and the remediation plan landed
after the review measured, and three of them quote the literal in their findings.

Every one of the 33 matching paths ends `.md`. Zero script, workflow or configuration files match,
which is what [P4-T5] asserts after the rewrite.

## Distinct Variants Found

Seven distinct normalised variants, well above the four the acceptance requires. A single-literal
substitution would have left six of them behind.

| Variant | Occurrences |
|---|---|
| `<execution-worktree-root>` \| long \| backslash | 53 |
| `<session-worktree-root>` \| long \| backslash | 23 |
| `<user-home>` \| long \| dashed | 9 |
| `<execution-worktree-root>` \| long \| forwardslash | 8 |
| `<user-home>` \| long \| backslash | 7 |
| `<repo-root>` \| long \| backslash | 2 |
| `<user-home>` \| 8.3 \| forwardslash | 1 |
| **Total** | **103** |

`UNCLASSIFIED: 0`. Every matching occurrence is attributed to a map entry, which is the check that
the variant map [P4-T1] derives is complete before [P4-T2] rewrites anything.

The **dashed** spelling is a variant the plan's four-root enumeration did not anticipate and this
census found. The session scratchpad key mangles the drive colon and both separators to a single
dash, so the account name survives in a form none of the separator spellings matches. It is mapped
by its home prefix alone, to `<user-home>`; the tail names a third worktree and carries no account
name. The classifier consumes longest-first, so a shorter root never eats a longer one.

## Per-File Census

`F/` abbreviates `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`.

| Path | Lines | Occurrences | `PLACEHOLDERS-BEFORE` | Variants |
|---|---|---|---|---|
| `F/code-review.2026-09-20T01-37.md` | 1 | 1 | 3 | `<user-home>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t1-worktree-anchor.2026-09-19T09-44.md` | 7 | 7 | 0 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|forwardslash=6 |
| `F/evidence/baseline/p0-t10-cold-state-census.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t3-diff-anchor.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|forwardslash=1 |
| `F/evidence/baseline/p0-t4-batch-budget-state.2026-09-19T09-44.md` | 5 | 5 | 0 | `<session-worktree-root>`\|long\|backslash=3; `<execution-worktree-root>`\|long\|backslash=1; `<user-home>`\|8.3\|forwardslash=1 |
| `F/evidence/baseline/p0-t5-sdk-bootstrap.2026-09-19T09-44.md` | 6 | 6 | 0 | `<execution-worktree-root>`\|long\|backslash=6 |
| `F/evidence/baseline/p0-t6-tool-restore.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t7-package-restore.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t8-dotnet-coverage.2026-09-19T09-44.md` | 5 | 5 | 0 | `<execution-worktree-root>`\|long\|backslash=1; `<user-home>`\|long\|backslash=4 |
| `F/evidence/baseline/p0-t9-pester-provision.2026-09-19T09-44.md` | 1 | 1 | 0 | `<user-home>`\|long\|backslash=1 |
| `F/evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md` | 2 | 2 | 0 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/baseline/phase0-instructions-read.2026-09-19T09-44.md` | 3 | 3 | 0 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|forwardslash=1 |
| `F/evidence/other/p2-t9-batch-a-boundary.2026-09-19T09-44.md` | 3 | 3 | 0 | `<user-home>`\|long\|dashed=3 |
| `F/evidence/other/p4-t8-batch-b-boundary.2026-09-19T09-44.md` | 3 | 3 | 0 | `<user-home>`\|long\|dashed=3 |
| `F/evidence/other/p6-t7-batch-c-boundary.2026-09-19T09-44.md` | 3 | 3 | 1 | `<user-home>`\|long\|dashed=3 |
| `F/evidence/other/p9-t15-plan-checkoff-resync.2026-09-19T09-44.md` | 2 | 2 | 0 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p1-t13-pester-workflow-scope.2026-09-19T09-44.md` | 4 | 5 | 0 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=4 |
| `F/evidence/qa-gates/p1-t14-ac6-cold-analyzer-build-green.2026-09-19T09-44.md` | 4 | 5 | 0 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=4 |
| `F/evidence/qa-gates/p2-t1-poshqc-format.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p2-t2-poshqc-analyze.2026-09-19T09-44.md` | 2 | 2 | 0 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/qa-gates/p2-t4-csharpier-check.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t1-poshqc-format.iter1.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t1-poshqc-format.iter2.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t10-file-size-audit.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t2-poshqc-analyze.iter1.2026-09-19T09-44.md` | 2 | 2 | 0 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/qa-gates/p9-t2-poshqc-analyze.iter2.2026-09-19T09-44.md` | 2 | 2 | 0 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t4-csharpier-check.iter1.2026-09-19T09-44.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t7-mstest-coverage.iter1.2026-09-19T09-44.md` | 1 | 2 | 0 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/regression-testing/898-cold-restore-red-run.2026-09-19T11-40.md` | 1 | 1 | 0 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/plan.2026-09-19T09-44.md` | 11 | 17 | 2 | `<session-worktree-root>`\|long\|backslash=3; `<execution-worktree-root>`\|long\|backslash=14 |
| `F/remediation-inputs.2026-09-20T01-37.md` | 1 | 1 | 3 | `<user-home>`\|long\|backslash=1 |
| `F/research/2026-09-19T11-30-dependabot-nuget-upgrade-automation-research.md` | 14 | 14 | 0 | `<session-worktree-root>`\|long\|backslash=12; `<repo-root>`\|long\|backslash=2 |
| **33 files** | **94** | **103** | **9** | 7 distinct variants |

## Required Membership Checks

| Check | Result |
|---|---|
| List contains `F/plan.2026-09-19T09-44.md` | **yes**, 17 occurrences |
| List contains `F/evidence/baseline/p0-t5-sdk-bootstrap.2026-09-19T09-44.md` | **yes**, 6 occurrences |
| List contains `F/evidence/baseline/p0-t8-dotnet-coverage.2026-09-19T09-44.md` | **yes**, 5 occurrences |
| List contains `F/evidence/qa-gates/p4-t5-actionlint.2026-09-19T09-44.md` | **no** — negative control held |
| List contains `F/remediation-plan.2026-09-20T01-37.md` | **no** — gate rule 17 held on the plan file itself |

The two negative controls are the check that the census matched something real rather than
everything. `p4-t5-actionlint` was already sanitised to `<execution-worktree-root>` by the
predecessor cycle and carries no host path; a census that listed it would have matched its
placeholder rather than the literal. The remediation plan is written under gate rule 17 and derives
its pattern rather than typing it; a census that listed it would mean the plan typed the literal
somewhere and had made its own residual assertion unsatisfiable.

A total of 0 would also have been a failure, not a clean result. The total is 103.

## The Named Exclusion

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`

| Measurement | Result |
|---|---|
| Occurrences on disk | 4 |
| Present in `git diff --name-only <MERGE_BASE>..HEAD` | **no**, 0 paths returned |
| Occurrences at `origin/main` | 4 |
| In this census's scope | **no** — out of scope automatically |

The file carries 4 occurrences naming a worktree dated 2026-07-04 and a canonical root, as **fixture
input strings and expected values** of a path-rewriting test. It predates this branch, so it is
absent from the branch diff and the scope rule excluded it without any by-name intervention.
Rewriting it would change assertion inputs and break the suite.

**Observation for the coordinator, outside this cycle's remit.** A pre-existing host-path disclosure
exists on `origin/main` in that file: the same 4 occurrences are present at `origin/main`, so the
leak is on the default branch and is not attributable to this branch. It is worth promoting as its
own issue. Sanitising it requires rewriting the fixture's expected values in step with its inputs,
which is a test change with its own verification and does not belong in a cycle closing a review.

## Output Summary

206 in-scope paths; 33 match; 94 matching lines and 103 matching occurrences, both above the
review's 74-and-27 figures as expected. Seven distinct normalised variants, zero unclassified.
All 33 matching paths are markdown. Both negative controls held. The named fixture exclusion is
out of scope by the scope rule rather than by exception, and its pre-existing disclosure on
`origin/main` is recorded as an observation. `PLACEHOLDERS-BEFORE` over the whole scope is 121,
which is the figure [P4-T3] adds 103 to.
