# Issue #752 Update Mirror ([P4-T7])

Timestamp: 2026-09-03T12-24

PostedAs: unknown

This update was applied to the local feature-folder `issue.md` only. No GitHub API call was made from this execution, so no comment URL or `IssueUpdatedAt` value exists to record.

## Status line as updated (issue.md line 5)

```
- Status: Fix implemented and verified -> docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/ (Issue #752)
```

## Note appended under `## Suspected Cause / Notes`, exact text

```
ANCHORED-RELATIVE-PATH EXCLUSION
Implementation note recorded at fix time: switching the predicate's match target from the absolute `FullName` to the path relative to `$resolvedSearchRoot` was not sufficient on its own. `[System.IO.Path]::GetRelativePath` returns a descendant path with **no leading separator**, so a nested sibling worktree resolves to a relative path that begins with `.claude\` rather than `\.claude\`. The original pattern requires a separator immediately before the segment and therefore no longer matched, which would have retained the nested sibling worktree and broken the preserved regression test at `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-442`. The exclusion regex was therefore anchored to the start of the relative path as well as to an interior separator. The three measured relative paths and their match results against both patterns are recorded in `evidence/regression-testing/getrelativepath-probe.2026-09-03T07-23.md`.
```
