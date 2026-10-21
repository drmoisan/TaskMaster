# Feature Audit: coverage-runner-scoped-threshold-and-format (Issue #928) - Remediation Cycle 1 Exit

- Timestamp label: 2026-09-29T11-15 (assigned without a clock read; see the policy audit header)
- Work mode: minor-audit (issue.md line 12, `- Work Mode: minor-audit`)
- AC source: `docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/issue.md`, section `## Acceptance Criteria` only (AC1 to AC7). spec.md and user-story.md are absent by design (Glob over the feature folder confirms) and were not consulted.
- Method: Read, Grep and Glob over the worktree; no command executed (Bash withheld by the caller). Coverage figures verified against the gitignored JaCoCo documents named in the policy audit.

## Scope and Baseline

- Branch: bug/coverage-runner-scoped-threshold-and-format-928; head 408ac211f4f24376f525af2a477a4d2da50525ec (read from `.git/worktrees/agent-aa9f4097fba69c282/HEAD` and the branch ref file; matches the caller).
- Base anchor: 177b6d78e1b2408e5aedbd794cef3aad6b7fb372, the `git merge-base HEAD origin/main` recorded in evidence/remediation-baseline/r1-p0-t2-identity-and-state.2026-09-29T10-42.md.
- Code footprint (P2-T6 anchored name-only diff and P2-T5 numstat, both dated 2026-09-29T10-57): scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 (new, +104), scripts/vscode/Invoke-MSTestWithCoverage.ps1 (+24/-2), tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1 (new, +306). scripts/vscode/Invoke-MSTest.ps1 unchanged. No path under .github, .vscode, .claude (other than the pre-existing, unstaged agent-memory paths the executor recorded) or config.
- Baselines: format drift set empty on the tree (r1-p0-t4, with a liveness control); analyzer 13 findings in scripts/vscode, 0 in the entry point, 2 in Invoke-MSTest.ps1 (r1-p0-t5); comparison coverage baseline fixed at the original P0-T7 Route C figure, 94.49% (1613/1707), entry point 113/126 = 89.68%; pre-remediation reproduction (r1-p0-t6) 334 tests, 94.46% (1620/1715), entry point 115/129 with changed line 408 at 0 hits.
- Post-change (P2-T3 iter2): 342 tests passing in 27 suites; population 94.53% (1626/1720); entry point 113/126 = 89.68% with no changed line uncovered; part file 13/13 = 100.00%.
- Prior review (2026-09-29T10-00): 6 PASS, AC6 PARTIAL (blocking). This audit re-evaluates all seven criteria against the remediated tree.

## Acceptance Criteria Inventory

| AC | Text (abridged; full text in issue.md lines 43 to 49) | Checkbox state in issue.md at review start |
|---|---|---|
| AC1 | Scoped run with successful collection exits without error below both floors and emits exactly one warning naming the skip and the scoped search root; Pester test with in-memory below-floor fixture and mocked collection | [x] |
| AC2 | Scoped run with a non-zero collector exit code still terminates with an error; Pester test with mocked collection | [x] |
| AC3 | Unscoped run (omitted or `.`) below the floor still throws the existing messages (one line case, one branch case with line at or above 80); CI coverage workflow not modified; 80 and 75 literals unchanged | [x] |
| AC4 | Scoped-run behavior and the definition of a scoped run documented in comment-based help in the coverage script | [x] |
| AC5 | Both scripts formatter-clean: a PoshQC format run over scripts/vscode after the change leaves both files byte-identical | [x] |
| AC6 | PoshQC analyze reports no findings on any changed PowerShell file; the Pester suite passes; population line coverage over the CI Pester population stays at or above 80% and does not fall below its recorded baseline, with every changed production line covered | [x] (checked off by the executor at P2-T7, evidence/regression-testing/p2-t12-ac6-check-off.2026-09-29T10-57.md) |
| AC7 | Committed evidence follows the Committed Test Evidence Format and contains no absolute host path, account name or host name; placeholders used | [x] |

Total: 7. Checked at review start: 7. Unchecked: 0.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence verified by this review |
|---|---|---|
| AC1 | PASS | Entry-point path: It 9 (lines 239 to 247) invokes `Invoke-MSTestWithCoverageMain -SearchRoot 'QuickFiler.Test'` with `ConvertTo-KoverageCoberturaXml` mocked to a line-rate 0.4 / branch-rate 0.5 document and the real threshold assertions in the path; `Should -Not -Throw`; four `Set-Content` writes. It 10 (249 to 257): `Should -Invoke Write-Warning -Times 1 -Exactly` plus a filtered assertion on `Coverage threshold assertions skipped*` and `*QuickFiler.Test*`. Direct path: It 19 and 20 (163 to 184) call `Assert-CoberturaCoverageThresholdForRun` with the same fixture and a scoped search root; no throw; exactly one warning naming the search root. Production: part file lines 96 to 100 (predicate, one `Write-Warning`, `return`); entry point line 406 calls it unconditionally. The collector seam `Invoke-DotnetCoverageExe` is mocked with exit 0. All four cases pass (r1-p1-t6, P2-T3 iter2). |
| AC2 | PASS | It 11 (259 to 272): `Invoke-DotnetCoverageExe` mocked to `$global:LASTEXITCODE = 7`; the real `Invoke-DotnetCoverageCollection` throws `MSTest with coverage failed with exit code 7` (entry point line 262); `ConvertTo-KoverageCoberturaXml` invoked 0 times, so the run stops before post-processing and before the gate. Unchanged by the remediation; passes before and after. |
| AC3 | PASS | Entry-point path: It 12 (omitted `-SearchRoot`, line-rate 0.4) throws `Cobertura line coverage 40% is below the required 80% threshold.`; It 13 (`-SearchRoot '.'`, line-rate 0.8, branch-rate 0.5) throws `Cobertura branch coverage 50% is below the required 75% threshold.`. Direct path: It 21 and 22 (186 to 204) assert the same two messages through the new function with the repository root as the search root. Literals: Threshold.ps1 read by this review, `-lt 80` line 52, message line 54, `-lt 75` line 122, message line 124, unchanged; P2-T5 counts one each. Both assertion calls run unconditionally on an unscoped run: part file lines 102 and 103, in that order, after the scoped-arm `return` at 100; the entry point reaches them through the single unconditional call at 406 (Grep for `Assert-CoberturaLineCoverageThreshold` in the entry point: 0, so no second path exists). Workflow: P2-T6 name-only diff over `.github` lists nothing; `_mstest-coverage.yml` line 95 still invokes `-SearchRoot . -Configuration Debug`. No opt-out switch exists (the script-level `param` block at lines 1 to 13 and the function `param` block at 293 to 301 carry no new parameter). |
| AC4 | PASS | Entry-point help lines 278 to 291: `.DESCRIPTION` describes the skip and its reason; `.PARAMETER SearchRoot` defines a scoped run (full-path comparison, ordinal case-insensitive, trailing separators ignored; omitted, `.` and `.\` unscoped) and states the one-warning behavior. It 14 (295 to 305) reads the parsed help and matches `scoped`, `repository root`, `skip`. The part file's help restates the definition on the predicate (lines 9 to 28) and documents the gate on the wrapper (lines 60 to 83), including the absolute-input rule added at cycle 1. |
| AC5 | PASS | r1-p0-t4: the formatter is live on this tree (an injected indent on line 272 was removed) and the tree carries no drift. p2-t1 iter2: `git hash-object --no-filters` identical before and after a `mcp__drm-copilot__run_poshqc_format` run over scripts/vscode and tests/scripts/vscode for the entry point (36f9595e...), the part file (cb9b9a74...), Invoke-MSTest.ps1 (9aec072f..., also its base-anchor hash) and the test file (2bf36b22...). Both named scripts are byte-identical across a format run after the change. Executor-attested (this review cannot hash files); the cycle-1 figures for Invoke-MSTest.ps1 agree. |
| AC6 | PASS (executor check-off confirmed) | Analyzer: p2-t2 iter2 runs C (entry point), D (part file), E (test file) `ok: true`; folder run A 13 equals the r1-p0-t5 baseline 13; no suppression added. Pester suite: JUnit root re-read by this review `tests="342" errors="0" failures="0"`, new suite `tests="22" failures="0" errors="0" skipped="0"`; direct run passed 342, failed 0, skipped 0. Population: the post-change JaCoCo document's report-level LINE counter reads covered 1626, missed 94 (94.53%), at or above 80% and at or above the recorded baseline 94.49% (1613/1707), measured by the identical CMD-PESTER-DIRECT command (decision R7; the P2-T3 iter2 artifact records exactly the two permitted substitutions and no `UseBreakpoints` change). Changed production lines: the entry point's `<sourcefile>` lists 126 nodes with `ci="0"` on exactly 149, 171, 187, 229, 333, 341, 345, 358, 364, 370, 436, 452, 460, none of which lies in the changed ranges 278 to 291, 311 to 314 or 404 to 409; the changed analyzable lines 313, 406 and 410 read `ci` 2, 1 and 2. The part file's `<sourcefile>` lists 13 nodes, all with `ci` at least 1 (the file is entirely new, so every node is a changed line). The pre-remediation document reproduces the defect being fixed (line 408 `mi="2" ci="0"`), so the before/after pair measures the same route. The executor's check-off at P2-T7 is confirmed; the AC6 text is unchanged. |
| AC7 | PASS | Glob over the feature folder for `*.xml`, `*.trx`, `*.json`, `*.txt`: no file (48 evidence markdown files plus the plan, review and issue files); raw JaCoCo output remains under the gitignored `coverage/` directory and the JUnit document under gitignored `artifacts/`. Grep over the feature folder for the drive-letter path pattern `(^\|[^A-Za-z])[A-Za-z]:[\\/]`: 0 matches. Grep (case-insensitive) for the developer account name and for the host name, both read from the gitignored JUnit document and not transcribed here: 0 matches each. Grep for the user-profile prefix `[\\/]Users[\\/]`: 0. The same four sweeps over the promoted record docs/features/potential/promoted/2026-09-28-coverage-runner-scoped-threshold-and-format.md: 0. Every `workspace_root` line in the evidence subfolder carries `<repo-root>` (P2-T14 iter, spot-checked in r1-p0-t4, r1-p0-t5, r1-p0-t6, r1-p1-t3, r1-p1-t6, p2-t1, p2-t2, p2-t3). The GitHub handle in the issue URL and the plan `Owner:` fields is public repository metadata, not an account, host or path (policy audit I-1). The neutral drive-letter fixture root literal in the test file (line 30; transcribed as `<fixture-root>` per plan rule R12) is test code, not committed evidence, and matches the sibling suites. |

AC verdict totals: 7 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.

## Summary

Remediation cycle 1 closes the single blocking finding of the prior audit. The scoped-run gate now lives in `Assert-CoberturaCoverageThresholdForRun` in the path-loaded Scope part file behind one unconditional entry-point call; the CI-equivalent breakpoint coverage run credits every changed production line, the population figure is above its recorded baseline, and the entry point returned to its base-anchor coverage. The unscoped gate is not weakened: both assertion statements run unconditionally on an unscoped run, in their original order, with the 80 and 75 literals unchanged, and no workflow, task, governance or configuration file is touched. CR-2 is folded in with four negative tests. All seven acceptance criteria are verified PASS against the code, the committed evidence and the raw coverage documents left in the worktree.

Overall verdict: PASS. Blocking findings: 0. Non-blocking: 1 (canonical hook coverage artifact validity, pre-existing tooling; policy audit NB-1). No remediation-inputs artifact is produced.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/issue.md (section `## Acceptance Criteria`)
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

- Items evaluated PASS and already checked in issue.md: AC1, AC2, AC3, AC4, AC5, AC6, AC7. Each check-off was verified against the evidence above; the AC6 check-off made by the executor at P2-T7 is confirmed on independent reading of the coverage documents.
- Items newly checked off by this review: none (all seven were already checked).
- Items left unchecked: none.
- issue.md was not edited by this review; no AC text was modified.
