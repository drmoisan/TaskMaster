# P1-T15 — Phase 1 instrumentation commit

Timestamp: 2026-09-07T14-28
Task: [P1-T15]
Issue: #796
Channel used: A
Branch: bug/quickfiler-folder-dropdown-closes-on-open-796

## COMMIT-SHA: 0dfcb402f4e3323c7f652b63701edd9bc5eb9fe0

Short form: `0dfcb402`. Parent: `336e30db`. Base anchor for every diff gate in this
plan is unchanged at `c7ae69f1`.

This is the SHA the Phase 2 runbook's step 1 requires as the commit of the build
under test. The Debug output confirmed at P1-T13 was produced from this working tree
state.

Commands:

```
pwsh -NoProfile -Command 'git commit -m "instrument(796): add AC6 debug logging at the two named close-ordering sites" -m "Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"'
pwsh -NoProfile -Command 'git rev-parse HEAD'
```

EXIT_CODE: 0 for both.

Staging used explicit pathspecs (`QuickFiler`, `QuickFiler.Test`, and the feature
folder), never a repository-wide stage, so no unrelated queued file could be swept
onto this branch.

## Commit summary

```
[bug/quickfiler-folder-dropdown-closes-on-open-796 0dfcb402] instrument(796): add AC6 debug logging at the two named close-ordering sites
 34 files changed, 1743 insertions(+), 41 deletions(-)
```

34 files: the 8 write-set paths audited at P1-T14, plus the plan file carrying this
run's check-offs, plus 25 evidence artifacts (13 Phase 0 baseline, 8 Phase 1
qa-gates, 2 Phase 1 regression-testing, and the P1-T14 scope audit; the qa-gates
count includes p1-t14-phase1-scope.md).

Two paths were created rather than modified:
QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs and
QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs.

No TRX, no raw MSBuild log, and no Cobertura XML is in the commit. Those live under
the gitignored paths TestResults/796/ and coverage/ and are outside the staged
pathspecs regardless.

## Commit message, verbatim

```
instrument(796): add AC6 debug logging at the two named close-ordering sites

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>
```

The trailer is the last line of the message and nothing follows it.

## Porcelain immediately after the commit

Command: `pwsh -NoProfile -Command 'git status --porcelain --untracked-files=all'`

Output: empty. Zero entries. The working tree was fully clean at that moment, so it
listed no path outside the feature folder, and none inside it either. The
`PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md is
EMPTY, so the gate was evaluated strictly.

## State of this artifact

This file records the SHA of the commit that precedes it, so it cannot be inside that
commit. After it is written the working tree carries exactly one entry, this file,
which lies inside the feature folder. The acceptance condition — that porcelain lists
no path outside the feature folder and the PRE-EXISTING-DIRTY-SET — therefore still
holds. P1-T15's command sequence contains no second commit, so this artifact is left
staged and uncommitted for a later phase to sweep.

Output Summary: Phase 1 instrumentation committed as
0dfcb402f4e3323c7f652b63701edd9bc5eb9fe0 with 34 files changed. Working tree clean
immediately after the commit.
