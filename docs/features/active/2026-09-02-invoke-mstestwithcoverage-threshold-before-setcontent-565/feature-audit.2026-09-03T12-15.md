# Feature Audit — invoke-mstestwithcoverage-threshold-before-setcontent (#565)

- Timestamp: 2026-09-03T12-15
- Work mode: `full-bug` — AC source: `spec.md` § `## Acceptance Criteria` only (per
  `acceptance-criteria-tracking` mode-resolution table; `user-story.md` is not required or present
  for `full-bug`, confirmed absent from the feature folder's tracked file list)

## Acceptance Criteria Evaluation

| # | Criterion (spec.md) | Verdict | Evidence |
|---|---|---|---|
| 1 | New Pester test fails against the pre-fix statement order and passes after the fix | **PASS** | `evidence/regression-testing/expect-fail-run.2026-09-03T11-09.md` (27 Passed/1 Failed, new test fails with "called 0 times") and `pass-after-run.2026-09-03T11-09.md` (28 Passed/0 Failed, new test passes) — a genuine RED-then-GREEN pair against the same test, both independently re-derivable from the committed diff (pre-fix order confirmed absent from HEAD via the production diff review) |
| 2 | `Set-Content` invoked before `Assert-CoberturaLineCoverageThreshold` can throw on a sub-threshold run, via `Should -Invoke Set-Content -Times 1 -Exactly` inside `{ ... } \| Should -Throw`, using the `line-rate="0.5"` fixture | **PASS** | Test body at `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-422`, read directly — matches the criterion's exact mechanics (mock, fixture, assertion shape) verbatim |
| 3 | Threshold value (80%) unchanged; no diff to `Assert-CoberturaLineCoverageThreshold`'s literal or message text (now in `Invoke-MSTestWithCoverage.Threshold.ps1` post-#733) | **PASS** | `git diff origin/main -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` empty (independently run); threshold text confirmed present and unchanged by direct read |
| 4 | No production file other than `Invoke-MSTestWithCoverage.ps1` changed (in particular `Helpers.ps1` and `ClosureFilter.ps1` untouched) | **PASS** | `git diff origin/main...HEAD --stat` shows exactly one production file changed; `Helpers.ps1` diff independently confirmed empty; `ClosureFilter.ps1` does not appear anywhere in the diff |
| 5 | PoshQC format, PSScriptAnalyzer, and Pester all pass cleanly on the changed files, no auto-fixes, no regression in `Describe 'Invoke-MSTestWithCoverageMain'` or the Helpers boundary tests | **PASS** | `evidence/qa-gates/poshqc-format.iter1...md` (0 auto-fixes), `poshqc-analyze.iter1...md` (0 diagnostics), `poshqc-test.iter1...md` (93/93 passing); `evidence/regression-testing/helpers-boundary-regression...md` (25/25 across both post-#733 threshold-boundary/helpers files) |
| 6 | Repro steps now produce expected behavior: sub-threshold-run artifact on disk is the post-processed document, not the raw `dotnet-coverage` output | **PASS** | Directly entailed by the production diff (Set-Content now precedes the throw unconditionally) and by AC-2's passing regression test, which is the automated equivalent of the manual repro steps per `spec.md` § Test Strategy ("no manual validation required beyond the automated toolchain") |

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/spec.md`
- Total AC items: 6
- Checked off (delivered): 6 (all already `[x]` in the committed `spec.md`; independently re-verified against evidence above, no discrepancies found)
- Remaining (unchecked): 0
- Items remaining: none

## Baseline Comparison

- Pre-fix (baseline, captured at merge commit `dc5e8c0f`): 92/92 Pester tests passing across
  `tests/scripts/vscode`; `Invoke-MSTestWithCoverage.ps1` line coverage 90.09%.
- Post-fix (final, HEAD `e165f7ba`): 93/93 Pester tests passing (the +1 is the new regression
  test); same file, same 90.09% line coverage, identical `MainScriptCommands=111` total —
  confirms the reorder added no new executable command and caused no coverage regression.
- No pre-existing test in the suite changed outcome between baseline and final.

## Out-of-Scope Item Disposition

The `.claude\` worktree-exclusion filter (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, filter
predicate `-notmatch '\\\.claude\\'`) was independently confirmed present, byte-identical, in
`origin/main` and untouched by this branch's diff. It is correctly treated as pre-existing and out
of scope for this fix; not flagged as a regression.

## Overall Feature-Audit Verdict

**PASS.** All 6 acceptance criteria independently verified against evidence, with no gaps or
discrepancies. Working tree is clean (`git status --porcelain` empty); the branch is reconciled
against `origin/main` at merge commit `dc5e8c0f`.
