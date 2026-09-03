## Suggested title
fix(vscode): persist the post-processed Cobertura document before the coverage-threshold assertion (#565)

## Summary
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` previously called `Assert-CoberturaLineCoverageThreshold` before `Set-Content` persisted the post-processed Cobertura XML, so a failing (sub-threshold) run threw before the judged document was ever written to disk, leaving the raw, un-post-processed `dotnet-coverage` output at the `-CoverageOutput` path instead.
- Fix is a pure two-statement reorder inside `Invoke-MSTestWithCoverageMain`: `Set-Content` now runs immediately after `$processedXmlContent` is computed and before `Assert-CoberturaLineCoverageThreshold` is called. No logic, threshold value, or message text changed.
- Adds one new Pester regression test to `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` proving `Set-Content` is invoked exactly once before the threshold assertion can throw on a sub-threshold run.
- This branch was cut before sibling issue #733 (already merged to `main`) refactored this same file, splitting `Assert-CoberturaLineCoverageThreshold` out into a new file, `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`. The branch was reconciled against the current `main`, the defect was re-confirmed still present at the drifted line numbers, and the fix was executed and verified against the reconciled tree.
- Threshold value (80%) and message text are unchanged and independently confirmed unmodified in both `Invoke-MSTestWithCoverage.Helpers.ps1` and the relocated `Invoke-MSTestWithCoverage.Threshold.ps1` (empty diffs against `main` for both files).
- 6/6 acceptance criteria in `spec.md` verified and checked off; zero blocking findings in policy-audit, code-review, and feature-audit.

## Why
Only the failure path was affected — a passing run always wrote the correct document. But the failure path is exactly when someone reads the artifact on disk to diagnose a coverage shortfall, and what they found was a document with different numbers than the one that produced the failure message (absolute paths, unmerged third-party packages, duplicate classes, and the double-counted line totals that issue #441 had already corrected). A failed gate also left behind an artifact that, if fed to any downstream consumer, silently reported the pre-#441 inflated denominator. Issue #563 (a separate, unrelated contradiction in the threshold *value*) is explicitly out of scope for this fix, which touches statement order only.

## What Changed
**Core logic:**
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — swapped the `Set-Content` and `Assert-CoberturaLineCoverageThreshold` calls inside `Invoke-MSTestWithCoverageMain` (net +2/-2 lines).

**Tests:**
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` — added one new `It` to the existing `Describe 'Invoke-MSTestWithCoverageMain'` block (net +8 lines), inserted between the pre-existing `'fails when the search root cannot be found'` test and the sibling-issue-#733-added `'.claude worktree'` exclusion test.

**Docs/evidence:**
- `docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/` — issue, spec, research, atomic plan, full Phase 0-6 evidence trail, and policy-audit/code-review/feature-audit artifacts.

## Architecture / How It Fits Together
`Invoke-MSTestWithCoverageMain` collects coverage via `dotnet-coverage`, post-processes the raw Cobertura XML for Koverage compatibility (`ConvertTo-KoverageCoberturaXml`), and previously validated the resulting line-rate before writing it to disk. The fix only reorders when the write happens relative to the validation; `Assert-CoberturaLineCoverageThreshold` remains a pure read-and-throw function with no side effect that could observe or be observed by `Set-Content`, so the reorder is behaviorally safe under both the passing and failing case.

## Verification
**Completed (from evidence trail):**
- RED before / GREEN after: pre-fix simulation showed the new test failing (`Set-Content` invoked 0 times before the throw); post-fix run showed 28/28 tests passed in the target file.
- Full PowerShell suite (10 files, 93 tests): 93 passed, 0 failed, 0 skipped.
- PoshQC format: no rewrites needed on either owned file.
- PSScriptAnalyzer: 0 diagnostics on either owned file (matches baseline).
- Coverage on `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: 90.09% before and after (no regression).
- `git diff` against `main` confirmed empty for both `Invoke-MSTestWithCoverage.Helpers.ps1` and `Invoke-MSTestWithCoverage.Threshold.ps1` (threshold value/message untouched).
- feature-review: policy-audit PASS, code-review PASS (0 blocking), feature-audit PASS (6/6 AC verified).

**Recommended (CI):**
- PowerShell workflow(s) covering `scripts/vscode/**` and `tests/scripts/vscode/**`.

## Backward Compatibility / Migration Notes
None. No function signature, parameter, output format, or configuration surface changed. Passing runs (coverage at or above 80%) are unaffected because both statements always executed in that case; only the failing-run artifact-on-disk behavior changes, and only for the better (it now matches the document that produced the failure).

## Risks and Mitigations
- **Risk:** none identified beyond the reordered two statements themselves — this is a minimal, self-contained fix confined to a single production file plus one test file.
- **Mitigation / rollback:** the change is a direct two-line swap; rollback is a straightforward revert of the diff.

## Review Guide
Suggested order:
1. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — the actual fix (4-line diff).
2. `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` — the new regression test (8-line diff).
3. `docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/spec.md` — acceptance criteria and their check-off state.
4. Remaining files are evidence artifacts (baseline/QA-gate/regression-testing captures) and the three review artifacts (policy-audit, code-review, feature-audit); skim for the pass/fail signal in each rather than reading line by line.

## Follow-ups
- Issue #563 (threshold *value* contradiction) remains open and is deliberately out of scope for this fix.

## GitHub Auto-close
- Closes #565
