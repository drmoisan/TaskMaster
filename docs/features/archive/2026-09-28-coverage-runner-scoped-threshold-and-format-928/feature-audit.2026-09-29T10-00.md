# Feature Audit: coverage-runner-scoped-threshold-and-format (Issue #928)

- Timestamp label: 2026-09-29T10-00 (assigned without a clock read; see the policy audit header)
- Work mode: minor-audit (issue.md line 12, `- Work Mode: minor-audit`)
- AC source: `docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/issue.md`, section `## Acceptance Criteria` only (AC1 to AC7). spec.md and user-story.md are absent by design and were not consulted.
- Method: Read, Grep and Glob over the worktree; no command executed (Bash withheld by the caller).

## Scope and Baseline

- Branch: bug/coverage-runner-scoped-threshold-and-format-928; head a17ca5bcf (caller-supplied).
- Base anchor: 177b6d78e1b2408e5aedbd794cef3aad6b7fb372, the `git merge-base HEAD origin/main` recorded in evidence/baseline/p0-t3-base-anchor.2026-09-29T08-54.md.
- Code footprint (P2-T6 anchored name-only diff, re-read by this review): scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 (new, +49), scripts/vscode/Invoke-MSTestWithCoverage.ps1 (+29/-2), tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1 (new, +232). scripts/vscode/Invoke-MSTest.ps1 unchanged.
- Baselines: format drift set empty on the base tree (P0-T5); analyzer 13 findings in scripts/vscode, 0 in the entry point, 2 in Invoke-MSTest.ps1 (P0-T6); tests 320 passing in 26 suites; Route C population line coverage 94.49% (1613/1707); entry point 113/126 = 89.68%; Invoke-MSTest.ps1 49/56 = 87.50% (P0-T7).
- Post-change: tests 334 passing in 27 suites; population 94.46% (1620/1715); entry point 115/129 = 89.15% with changed line 408 at 0 hits; part file 5/5 = 100.00% (P2-T3).

## Acceptance Criteria Inventory

| AC | Text (abridged; full text in issue.md lines 43 to 49) | Checkbox state in issue.md at review start |
|---|---|---|
| AC1 | Scoped run with successful collection exits without error below both floors and emits exactly one warning naming the skip and the scoped search root; Pester test with in-memory below-floor fixture and mocked collection | [x] |
| AC2 | Scoped run with a non-zero collector exit code still terminates with an error; Pester test with mocked collection | [x] |
| AC3 | Unscoped run (omitted or `.`) below the floor still throws the existing messages (one line case, one branch case with line at or above 80); CI coverage workflow not modified; 80 and 75 literals unchanged | [x] |
| AC4 | Scoped-run behavior and the definition of a scoped run documented in comment-based help in the coverage script | [x] |
| AC5 | Both scripts formatter-clean: a PoshQC format run over scripts/vscode after the change leaves both files byte-identical | [x] |
| AC6 | PoshQC analyze reports no findings on any changed PowerShell file; the Pester suite passes; population line coverage over the CI Pester population stays at or above 80% and does not fall below its recorded baseline, with every changed production line covered | [ ] |
| AC7 | Committed evidence follows the Committed Test Evidence Format and contains no absolute host path, account name or host name; placeholders used | [x] |

Total: 7. Checked at review start: 6. Unchecked: 1 (AC6).

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence verified by this review |
|---|---|---|
| AC1 | PASS | Test file It 9 (lines 165 to 173): scoped invocation with `ConvertTo-KoverageCoberturaXml` mocked to a document with line-rate 0.4 and branch-rate 0.5, real threshold assertions in the path, `Should -Not -Throw`, four `Set-Content` writes. It 10 (lines 175 to 183): `Should -Invoke Write-Warning -Times 1 -Exactly` and a filtered assertion on `Coverage threshold assertions skipped*` and `*QuickFiler.Test*`. Both fail before the fix with the line-threshold message (P1-T3 items 9 and 10) and pass after (P1-T6, P2-T3). Entry-point lines 407 to 410 implement the single warning; the collector seam `Invoke-DotnetCoverageExe` is mocked with exit 0. |
| AC2 | PASS | It 11 (lines 185 to 198): `Invoke-DotnetCoverageExe` mocked to `$global:LASTEXITCODE = 7`; real `Invoke-DotnetCoverageCollection` throws `MSTest with coverage failed with exit code 7`; `ConvertTo-KoverageCoberturaXml` invoked 0 times. Control passes before and after the fix. |
| AC3 | PASS | It 12 (omitted `-SearchRoot`, line-rate 0.4): throws `Cobertura line coverage 40% is below the required 80% threshold.`; It 13 (`-SearchRoot '.'`, line-rate 0.8, branch-rate 0.5): throws `Cobertura branch coverage 50% is below the required 75% threshold.`. Threshold.ps1 read: `-lt 80` line 52, message line 54, `-lt 75` line 122, message line 124, unchanged. Workflow: P2-T6 name-only diff over `.github` lists nothing; `_mstest-coverage.yml` line 95 still invokes `-SearchRoot . -Configuration Debug`. Entry-point lines 411 to 414 keep the two assertion statements in their original text and order. |
| AC4 | PASS | Entry-point help lines 278 to 291: `.DESCRIPTION` describes the skip and its reason; `.PARAMETER SearchRoot` defines a scoped run (full-path comparison, ordinal case-insensitive, trailing separators ignored; omitted, `.` and `.\` unscoped) and states the one-warning behavior. It 14 reads the parsed help and matches `scoped`, `repository root`, `skip`; fails before (no parameter entry) and passes after. The part file's own help (lines 9 to 30) restates the definition. |
| AC5 | PASS | P0-T5: the formatter is live on this tree (an injected indent was removed) and the base-tree drift set is empty. P1-T7 and P2-T1: `git hash-object --no-filters` identical before and after a `mcp__drm-copilot__run_poshqc_format` run over scripts/vscode and tests/scripts/vscode for the entry point (d71b98ec...) and Invoke-MSTest.ps1 (9aec072f..., also its base-anchor hash). Both files are byte-identical across a format run after the change. The issue's expectation that the formatter "would rewrite" both scripts was measured with a bare `Invoke-Formatter`, not the repository route (plan D9); against the repository route there was nothing to rewrite, which satisfies the AC as worded. |
| AC6 | PARTIAL (blocking) | Met: analyzer 0 findings on the entry point, the part file and the test file (P2-T2 runs C, D, E `ok: true`), folder count equal to baseline; Pester suite 334/334 (P2-T3, JUnit root re-read: `tests="334" errors="0" failures="0"`); population 94.46% at or above 80%. Not met: 94.46% is below the recorded baseline 94.49% (same route, same command); entry-point changed line 408 (`Write-Warning` in the scoped arm) is uncovered in the full-population breakpoint run. Independent evaluation: the arithmetic reproduces (1620/1715 = 94.461%; +7 covered, +1 missed against the 8 new analyzable lines); the line is executed by It 10, which passes; the crediting gap is order-dependent (three ordered diagnostic runs) and disappears under profiler-based coverage (fourth run), consistent with Pester 5.6.1 line breakpoints binding to the first parsed copy of the entry point (`Invoke-MSTest.RunSettings.Tests.ps1`, which never takes the scoped arm). The AC text and the rule file's changed-line clause are explicit, the plan's loop is recorded as not closed (P2-T4), and a reviewer cannot ratify a measurement exception, so the item is classified blocking. Recommended remediation: relocate the conditional into the path-loaded Scope part file behind one unconditional entry-point call, after confirming with a two-file diagnostic that part-file lines are credited from a later-sorting suite (detail in code-review CR-1 and remediation-inputs R-1). Fallback: maintainer-ratified exception transcribed into issue.md beside AC6. |
| AC7 | PASS | Glob over the feature folder: 27 files, all `.md`; no `.xml` or `.trx` (raw JaCoCo output was written under the gitignored `coverage/` directory and not committed). Grep over the feature folder and the promoted record for the developer account name, the host name (read from the gitignored JUnit document; not transcribed), the drive-letter path pattern, the user-profile directory prefix pattern and the fixture drive root: the only match is the scheme separator in the issue URL. Every evidence line naming `workspace_root` carries `<repo-root>` (P2-T14 step 4, spot-checked in P0-T5, P0-T7, P1-T3, P1-T6, P1-T7, P2-T1, P2-T2, P2-T3). |

AC verdict totals: 6 PASS, 1 PARTIAL (blocking), 0 FAIL.

## Summary

The bug is fixed as specified: a scoped run is judged on its tests, the unscoped run and the CI gate are unchanged, the behavior is documented, and both scripts are formatter-clean under the repository route. Six of seven acceptance criteria are verified PASS against the code and the committed evidence. AC6 is PARTIAL: the analyzer and test-pass clauses hold, but the coverage clauses fail by the agreed CI-equivalent measurement route through a crediting defect of breakpoint coverage over per-file parsed copies of the entry point, not through an untested line. The finding is blocking because the AC and the rule file are explicit; the recommended remediation is a small structural relocation of the gate into the path-loaded part file, with a maintainer-ratified exception as the fallback.

Overall verdict: REMEDIATION REQUIRED. Blocking findings: 1.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/issue.md (section `## Acceptance Criteria`)
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining:
  - AC6: PoshQC analyze reports no findings on any changed PowerShell file, the Pester suite passes, and the Pester line-coverage figure over the CI Pester population (the scripts/dependencies and scripts/vscode folders, as measured by the Pester workflow) remains at or above 80% and does not fall below its recorded baseline, with every changed production line covered. (Amended 2026-09-28 by the preparation orchestrator: the scripts/vscode folder alone measured below 80% in earlier committed evidence, so the floor is stated against the population the CI gate actually enforces.)

## Acceptance Criteria Check-off

- Items evaluated PASS and already checked in issue.md: AC1, AC2, AC3, AC4, AC5, AC7. Each check-off was verified against the evidence above; no change to issue.md was required.
- Items newly checked off by this review: none.
- Items left unchecked: AC6 (PARTIAL, blocking; see remediation-inputs.2026-09-29T10-00.md). The AC6 text was not modified.
- issue.md was not edited by this review.
