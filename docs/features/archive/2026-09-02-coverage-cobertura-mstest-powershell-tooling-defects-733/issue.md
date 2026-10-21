# coverage-cobertura-mstest-powershell-tooling-defects (Issue #733)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/coverage-cobertura-mstest-powershell-tooling-defects/ (Issue #733)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #733
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/733
- Last Updated: 2026-09-02
- Work Mode: full-bug

## Summary

Seven consolidated findings from a blast-radius review of open bug reports, all clustered on the `scripts/vscode/*.ps1` MSTest/Cobertura coverage tooling. Consolidated into one issue rather than seven since all seven are small, same-subsystem PowerShell fixes.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — PowerShell 7+ coverage/test-runner scripts
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `Invoke-MSTest.ps1`, and their helper/closure-filter scripts
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable in the usual sense — each sub-finding below is a static code-review finding with its own reachability note.

## Expected Behavior

Each sub-finding's expected behavior is stated inline below.

## Actual Behavior

**1. `Merge-CoberturaClassesByFilename` never recomputes package-level `line-rate`/`branch-rate`.** (`Invoke-MSTestWithCoverage.Helpers.ps1`) Confirmed: the function sets `line-rate`/`branch-rate` on the merged CLASS node (~line 374-375) and the root `<coverage>` node is set elsewhere (~line 442-443), but no code path targets the intermediate `<package>` node's own rate attributes — they go stale after a class merge. *(Source: #529.)*

**2. The same function only clones the PRIMARY class's `<methods>`, not a real merge.** `$mergedClassNode = $primaryNode.CloneNode($true)` then only ensures a `<methods>` node exists — it never unions method entries from the other classes being merged into the group. Confirmed unchanged. *(Source: #530.)*

**3. `Invoke-MSTestWithCoverage.ps1`'s assembly-discovery filter has no `.claude` exclusion.** The `Where-Object` filter (~line 296-302) checks for `\bin\<Configuration>\`, excludes `\obj\` and `\ref\`, but has no exclusion for paths under `.claude\` (e.g. agent worktrees), so a stray build under an agent worktree can be discovered and counted. Confirmed unchanged. *(Source: #531.)*

**4. No test exercises the `max(hits)` overwrite branch in the line-merge logic.** In the same merge function, `$existingNode.SetAttribute('hits', [string]([math]::Max(...)))` has no fixture where the SECOND-seen class-level entry has a higher hit count than the first (all existing fixtures present hits already `>=` later entries). *(Source: #537.)*

**5. `Invoke-MSTestWithCoverage.ClosureFilter.ps1`: local functions are deliberately excluded from the coverage presence set.** Its own doc comment says local functions (`g__Local` members) are "deliberately NOT admitted" — confirmed present verbatim (~line 154). This is stated as intentional but its correctness as a policy is unverified; a local function inside a covered member currently cannot be measured for coverage exclusion purposes at all. *(Source: #559.)*

**6. The same script's presence set is keyed by member NAME, not full signature.** The set is `Dictionary<"$declaringType|$filename", HashSet[string] of member names>` (~line 140, 168-169) — two overloads with the same name under the same declaring type/file collide in the set, so excluding one overload silently excludes both. *(Source: #560.)*

**7. `Invoke-MSTest.ps1`'s single-assembly discovery pipeline throws under `StrictMode` on exactly one match.** `Get-ChildItem ... | Where-Object {...} | Select-Object -ExpandProperty FullName` (~line 107-113) is not wrapped in `@(...)`, so when the filter matches exactly one assembly, the pipeline collapses to a bare scalar string rather than an array; a later `.Count` read then throws under `Set-StrictMode -Version Latest`/`2.0+`, since a bare scalar has no native `.Count` member once the adapted-property fallback is disabled. The sibling script (`Invoke-MSTestWithCoverage.ps1`, finding 3 above) has the identical unwrapped-pipeline shape but is less likely to hit the single-match edge case since it typically discovers the whole suite. *(Source: #713.)*

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations inline above, each independently re-verified against `origin/main` before this consolidation.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: finding 7 is a real crash under `-SearchRoot` matching exactly one assembly (already tracked as a live blocker on a related item this session), and findings 1/2/6 silently corrupt coverage reporting numbers rather than crashing — both classes matter for trusting this repo's coverage gates, but neither is a build-breaking or data-loss defect.

## Suspected Cause / Notes

Findings 1, 2, and 4 are all in the same `Merge-CoberturaClassesByFilename` function and likely share one fix pass. Findings 5 and 6 are both in `ClosureFilter.ps1`'s presence-set logic and likely share a second fix pass (moving from name-keyed to signature-keyed, and revisiting the local-function exclusion policy). Finding 3 and finding 7 are the same missing-`@()`-array-safety class of defect in two sibling scripts. All seven independently re-verified against current `origin/main` as part of this consolidation pass on 2026-09-02.

## Proposed Fix / Validation Ideas

- [ ] `Merge-CoberturaClassesByFilename`: recompute and set `<package>`-level `line-rate`/`branch-rate` after class merges; actually union `<methods>` entries across the merged group, not just clone the primary's; add a fixture where a later class-level entry has strictly higher hits than the first
- [ ] `Invoke-MSTestWithCoverage.ps1`: add a `.claude\` (or agent-worktree-path) exclusion to the assembly-discovery filter
- [ ] `ClosureFilter.ps1`: re-key the presence set by full member signature instead of bare name; get an explicit decision on whether local functions should remain excluded from the presence set
- [ ] Wrap both `Invoke-MSTestWithCoverage.ps1`'s and `Invoke-MSTest.ps1`'s assembly-discovery pipelines in `@(...)` so a single match stays an array

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
