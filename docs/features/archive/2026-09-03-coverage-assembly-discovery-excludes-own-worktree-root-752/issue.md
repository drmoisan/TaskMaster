# coverage-assembly-discovery-excludes-own-worktree-root (Issue #752)

- Date captured: 2026-09-03
- Author: Dan Moisan
- Status: Fix implemented and verified -> docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/ (Issue #752)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #752
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/752
- Last Updated: 2026-09-03
- Work Mode: full-bug

## Summary

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` excludes test assemblies whose absolute `FullName` matches `\.claude\`. When the script is run from a checkout that is itself located under `.claude/worktrees/`, every discovered assembly matches the exclusion, the discovered set is empty, and the script throws a misleading "No test assemblies found ... Build first." error.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, PowerShell 7.
- Python version: n/a (PowerShell).
- Command/flags used: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1` (default `-Configuration Debug`) run with the repo root at `<user-profile>\repos\TaskMaster\.claude\worktrees\agent-<id>\`.
- Data source or fixture: any built `*.Test.dll` under `<worktree>\<Project>\bin\Debug\`.

## Steps to Reproduce

1. Create an agent worktree under `.claude/worktrees/` (the location used by every parallel-run and epic-run child) and build the solution there so `*.Test.dll` assemblies exist under `bin\Debug\`.
2. From that worktree, run `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
3. Observe the script throw `No test assemblies found under '<worktree>' for configuration 'Debug'. Build first.` even though the assemblies exist.

## Expected Behavior

Assembly discovery excludes only sibling agent worktrees nested beneath the search root (the intent of the filter added by #733), and finds the test assemblies of the checkout the script is running from, regardless of where that checkout lives on disk.

## Actual Behavior

The `Where-Object` predicate at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` on `main` (`$_.FullName -notmatch '\\\.claude\\'`) tests the absolute path. When the search root is itself under `.claude\worktrees\`, every candidate path contains the segment and is excluded, so `$testAssemblies` is empty and the script throws with a message that misattributes the cause to a missing build.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `No test assemblies found under '<user-profile>\repos\TaskMaster\.claude\worktrees\agent-...' for configuration 'Debug'. Build first.` (reported by the item #735 child orchestration during parallel run `bugs-2026-09-02`; recorded in `artifacts/orchestration/parallel-orchestrator-state.json` under `environment_regression_from_item_733`).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High for agent-driven development: every in-place coverage run from an agent worktree fails, which affects every C# item executed by the parallel and epic orchestration surfaces. CI is unaffected because the runner checkout path contains no `.claude` segment.

## Suspected Cause / Notes

- Introduced by item #733 (PR #748), which added the `\.claude\` exclusion so that sibling agent worktrees nested under the main checkout would not pollute assembly discovery when the script runs from the main checkout.
- The intent is correct; the defect is that the exclusion is evaluated against the absolute path rather than the path relative to the resolved search root.
- Workaround used by parallel-run children: invoke `vstest.console.exe` directly with explicit assembly paths per CLAUDE.md toolchain step 4 instead of the wrapper script.

ANCHORED-RELATIVE-PATH EXCLUSION
Implementation note recorded at fix time: switching the predicate's match target from the absolute `FullName` to the path relative to `$resolvedSearchRoot` was not sufficient on its own. `[System.IO.Path]::GetRelativePath` returns a descendant path with **no leading separator**, so a nested sibling worktree resolves to a relative path that begins with `.claude\` rather than `\.claude\`. The original pattern requires a separator immediately before the segment and therefore no longer matched, which would have retained the nested sibling worktree and broken the preserved regression test at `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-442`. The exclusion regex was therefore anchored to the start of the relative path as well as to an interior separator. The three measured relative paths and their match results against both patterns are recorded in `evidence/regression-testing/getrelativepath-probe.2026-09-03T07-23.md`.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: compute the path relative to `$resolvedSearchRoot` (for example via `[System.IO.Path]::GetRelativePath`) and apply the `\.claude\` exclusion to that relative path; add Pester tests in `tests/scripts/vscode/` covering (a) a search root under `.claude\worktrees\` that contains assemblies, which must be found, and (b) a sibling `.claude\worktrees\` subtree beneath the search root, which must still be excluded.
- [x] Integration scenario to retest: run the script from an agent worktree and from the main checkout with a nested sibling worktree present; both must discover the correct assembly set.
- [x] Manual verification notes: tests must not create temporary files; drive the predicate through an injectable file-enumeration seam or pure path-filter function.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
