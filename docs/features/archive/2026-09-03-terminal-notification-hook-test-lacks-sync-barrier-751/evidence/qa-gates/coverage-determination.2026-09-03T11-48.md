# P4-T11 — Coverage Determination Re-Derived Against Post-Change State (Issue #751)

Timestamp: 2026-09-03T14-47

Command: `git diff --numstat f8414ee9..HEAD`

EXIT_CODE: 0

The command carries **no pathspec**, for the same reason as P4-T8: a pathspec-restricted numstat cannot
report a production directory it does not name.

## Verbatim output

```
1	1	TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs
2	1	TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs
92	0	docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/issue.md
303	0	docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/plan.2026-09-03T11-48.md
495	0	docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md
410	0	docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md
```

Row count: **6**.

## Row-by-row classification

| Row path | Class |
|---|---|
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | one of the two in-scope `TaskMaster.Test/AppGlobals/` source files |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | one of the two in-scope `TaskMaster.Test/AppGlobals/` source files |
| `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/issue.md` | feature-folder path |
| `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/plan.2026-09-03T11-48.md` | feature-folder path |
| `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md` | feature-folder path |
| `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md` | feature-folder path |

Every row names a path that is either one of the two `TaskMaster.Test/AppGlobals/` source files or begins
with `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/`.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| Every row names one of the two in-scope test files or a feature-folder path, so no production-assembly row exists | 6 of 6 classified; 0 production-assembly rows | PASS |

**The number of changed production lines on this branch is zero.**

## Determination, stated on that observed basis

1. **The coverage denominator is unchanged.** No production source line was added, removed, or modified.
   The only source rows in the diff are in `TaskMaster.Test`, which policy excludes from the denominator
   (`.claude/rules/general-unit-test.md:28`; `CLAUDE.md:302`) and which the module exclusion
   `.*\.Test\.dll$` injected at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:99-113` also excludes for the
   script route.

2. **The changed-line no-regression requirement has an empty subject and therefore cannot be violated.**
   The requirement (`.claude/rules/quality-tiers.md:35`; `.claude/rules/general-unit-test.md:25`;
   `CLAUDE.md:311`) quantifies over changed production lines. That set is empty, so the requirement is
   satisfied vacuously and no percentage comparison is needed to establish it.

3. **The P0-T16 determination is confirmed rather than merely restated.** P0-T16 predicted, before the edits
   were made, that this plan would change three lines all inside `TaskMaster.Test` and that spec AC4 plus
   the P4-T8 gate would leave the changed-production-line set empty. The unscoped numstat above is the
   post-change observation that confirms it: the prediction is now a measurement.

## Coverage instrumentation was active throughout

Every test run in Phases 0, 3, and 4 passed `/EnableCodeCoverage`, so the suite was exercised under coverage
instrumentation throughout:

| Phase | Runs | `/EnableCodeCoverage` |
|---|---|---|
| 0 | P0-T14 (full suite), P0-T15 runs 1-3 (`TaskMaster.Test`) | yes, all 4 |
| 3 | P3-T1 (targeted), P3-T2 runs 1-5 (`TaskMaster.Test`) | yes, all 6 |
| 4 | P4-T5 (full suite) | yes |

Eleven runs in total, each under coverage instrumentation, matching the CI shape at
`.github/workflows/_mstest-coverage.yml:99`.

## Relationship to P4-T12

This task establishes the no-regression conclusion **without** depending on the repository-wide percentages.
P4-T12 separately attempts the numeric baseline/post-change pair from the `.coverage` attachments. Where
P4-T12 cannot produce comparable figures, the no-regression evidence is the zero-changed-production-lines
observation recorded here.
