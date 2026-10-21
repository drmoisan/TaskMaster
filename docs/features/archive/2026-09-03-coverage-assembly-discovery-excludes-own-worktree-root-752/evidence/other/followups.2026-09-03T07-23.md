# Follow-up Observations ([P4-T9])

Timestamp: 2026-09-03T12-28

These are recorded observations only. None was opened, modified, or acted on within this plan.

## 1. Pre-existing format drift — not applicable

The `[P0-T5]` probe recorded `DRIFT-IN-PRODUCTION-FILE: NONE` and `PRE-EXISTING FORMAT DRIFT FILES: NONE`, so Branch A was selected and there is no pre-existing format drift to report under `scripts/vscode` or `tests/scripts/vscode`. This entry exists because the plan conditions it on Branch B; the condition did not hold.

## 2. No `.claude` exclusion clause in `Invoke-MSTest.ps1`

`scripts/vscode/Invoke-MSTest.ps1`'s `Get-MSTestAssemblyPathList` carries no `.claude` exclusion clause of any kind. The `[P3-T8]` sweep confirms this: the only `.claude` reference in that file is a documentation comment at line 142 naming a rules file. The consequence is that the non-coverage runner has no sibling-worktree exclusion at all, which is a different behaviour from the coverage wrapper rather than the same defect. The spec's `## Scope & Non-Goals` places discovery-filter parity for that script out of scope for this item, and `## Rollout & Follow-up` records it as a separate follow-up if ever needed. Nothing was changed in that file here.

## 3. Pre-existing absolute host path in a research artifact

`docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md` line 5 carries an absolute worktree-root path — that is, a filesystem path rooted at the machine's user profile and naming this item's checkout directory — in a commit that predates this plan. The leaked value is described here by class only and is deliberately not quoted, because quoting it would reintroduce the identifier into a second committed file. That file is not in this plan's Write Set, no task in this plan modified it, and it is outside the `[P4-T11]` residual sweep, whose scope is `evidence/` only. It is therefore not repaired here.

## 4. `[P3-T6]` branch divergence

`[P3-T6]` enumerates two branches keyed on the baseline changed-line coverage value and classifies every other combination as a stop-and-report. The measured combination was a third: the baseline recorded no per-line counter for file line 301, and the post-change run records that line as covered. This is a coverage gain caused by the fix introducing a newly analyzable command on that line, raising Pester's analyzed-command total from 802 to 803. The full measurement and mechanism are recorded in `evidence/qa-gates/coverage-delta.2026-09-03T07-23.md`. The plan's blocking coverage condition, post-change greater than or equal to baseline, holds with a positive delta. `[P3-T6]`'s checkbox is left unchecked because its written acceptance requires exactly one of the two enumerated branches to hold and neither does.
