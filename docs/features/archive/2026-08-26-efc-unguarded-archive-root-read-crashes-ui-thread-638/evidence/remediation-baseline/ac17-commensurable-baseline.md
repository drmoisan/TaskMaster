# AC17 remediation — commensurable merge-base coverage baseline (Issue 638)

Timestamp: 2026-08-29T12-57

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary: the merge-base coverage harness completed successfully and reached
post-processing, producing a `koverage-processed` baseline commensurable with the
post-change `[P7-T1]` figure. `Test Run Successful. Total tests: 6859, Passed: 6859,
Total time: 44.9009 Seconds.` Root attributes of the post-processed Cobertura file:
`line-rate=0.852636`, `branch-rate=0.792376`, `lines-covered=54735`, `lines-valid=64195`,
across 9 `<package>` elements.

```
BASELINE_REPO_LINE_COVERAGE_PERCENT:    85.26
BASELINE_REPO_BRANCH_COVERAGE_PERCENT:  79.24
BASELINE_LINES_COVERED:                 54735
BASELINE_LINES_VALID:                   64195
BASELINE_PACKAGE_COUNT:                 9
COVERAGE_XML_MODE:                      koverage-processed
```

## Why this measurement was taken

`[P7-T3]` recorded `REMEDIATION-REQUIRED` because `[P0-T12]` produced
`COVERAGE_XML_MODE: raw` while `[P7-T1]` produced `COVERAGE_XML_MODE: koverage-processed`.
A raw figure and a post-processed figure are computed over different denominators — the raw
file carried 14 `<package>` elements including every `.Test` assembly and `lines-valid=82363`,
the post-processed file carries 9 and `lines-valid=64221` — so their difference measures the
denominator rather than this change. AC17 requires the repository-wide figure to be captured
for both runs and the change shown not to lower it, which is not evaluable across unequal
modes.

The cause of the raw baseline was not this change. `[P0-T12]`'s harness run terminated early
on one pre-existing failing test,
`QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`,
a five-second wall-clock timing assertion in a file this change does not touch, so the run
never reached the post-processing step. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341`
asserts the threshold on post-processed content and `:343` writes that content back only
afterwards, so an earlier termination leaves the raw `dotnet-coverage` root attributes on
disk.

## How this measurement was isolated

The measurement was taken at the merge base `ecdb1c84ba8541ab67042985919cfed4df768c01` in a
separate detached git worktree, so the feature worktree, its build outputs and its committed
evidence were not disturbed. The gitignored `packages/` and `.dotnet-sdk/` directories were
copied from the feature worktree rather than restored, so the analyzer package set and SDK
version are identical between the two measurements and cannot contribute to the difference.
The solution was rebuilt with `/t:Rebuild` before the harness ran.

The same flaky liveness test passed in this run, all 6859 tests passed, the harness exited 0
and the post-processing step ran. That is the whole of the remediation: the baseline is now
measured in the same mode as the post-change figure.

## Relationship to the original baseline

The original `[P0-T12]` artifact remains the authoritative record of what was measured during
plan execution and is not superseded or edited. This artifact adds the commensurable
measurement that `[P7-T3]` identified as missing, per the remediation-reconciliation rule in
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. The recomputed delta is recorded
in `evidence/qa-gates/ac17-coverage-delta-recomputed.md`.

## Negative evidence claims

SearchScope: `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/`
SearchPatterns: `p0-t12-*.md`
SearchResult: `p0-t12-vstest-coverage.md` (raw mode) and `p0-t12-direct-harness-baseline.md`
(direct harness, exit 0, `BASELINE_FAILURE_SET: none`). No pre-existing `koverage-processed`
baseline artifact existed before this measurement.
