# Policy Audit - Issue #961 (remaining tracked backup files and hygiene guard rule C)

- Timestamp: 2026-10-02T05-30
- Work Mode: minor-audit (reduced small-audit)
- Base: 94287369908cc920b21b0e3256314f988ad7d2f5 (merge-base with origin/main, verified by P2-T14 evidence and unchanged); Head: f93005d38288020a2b0ebcfd2a51c1d713f7a357
- Scope: full branch diff against the resolved base

## Executive Summary

Verdict: PASS with no blocking findings. The branch deletes the three tracked backup files, adds `*.bak` to `.gitignore`, adds rule C (`Test-BackupFilePath`) to the repository hygiene guard, extends the `_hygiene.yml` row in `.github/workflows/README.md`, and adds 14 Pester tests (31 to 45). Reviewer-run guard output on the final tree is `HYGIENE Findings=0`. The numeric repo-wide Pester LINE figure for `scripts/hygiene` is produced by CI (`_pester.yml`) and is pending; it is not available locally because the PoshQC coverage document does not carry `scripts/hygiene`. Every added executable statement is exercised by a named passing test, so the changed-line obligation is met on local evidence.

Non-blocking observations: the branch diff contains two orchestrator memory files under `.claude/agent-memory/orchestrator/` that are outside the stated footprint; three evidence documents (`preflight-clearance`, `p1-t1-implementation-handoff`, `p2-t25-audit-handoff`) are handoff notes without an `EXIT_CODE` field; one orchestration test bundles five lookalike names in one `It`.

## Rejected Scope Narrowing

- Caller text (verbatim): "Pester line coverage comes from the CI Pester job (pester-coverage JaCoCo artifact), not locally; the PoshQC coverage document does not carry scripts/hygiene, so mark the coverage figure as CI-sourced and pending rather than FAIL."
- Disposition: not a language exclusion, so the full audit proceeds. The numeric figure is recorded as CI-sourced and pending, and an explicit PASS verdict is still recorded on the evidence that exists, because a changed language may not carry a pending-only verdict.
- No other caller text narrowed the scope.

## Evidence Location Compliance

- Scanned the branch diff for paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` and `artifacts/coverage/`: zero paths found.
- All new evidence lives under `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/evidence/<kind>/` with kinds `baseline`, `other`, `qa-gates` and `regression-testing`.
- `validate_evidence_locations.py` is not present in this worktree; the scan was done by reading the full `git diff --name-status` listing. Result: PASS.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; no caller instruction supplied a non-canonical path.
- No raw junit, trx or collector XML is tracked by this branch (`git ls-files` shows only pre-existing JaCoCo projections from other features, none added here).

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence and determinism | PASS | Rules tests call pure `Test-BackupFilePath` with literal strings. Orchestration tests mock `Invoke-GitExe` (the wrapper seam, never git itself) in `BeforeEach` and reset `$script:Content` and `$script:Listing` per test. |
| No temp files, no network, no external process | PASS | The added tests use in-memory listings and a content delegate; no file system access. |
| Arrange-Act-Assert | PASS | Each added `It` separates setup, one invocation and assertions with blank lines, matching the existing file style. |
| Failure messages (`-Because`) | PASS | All 10 rule tests and 3 of 4 orchestration tests carry `-Because` on the primary assertion. Array-equality assertions on `$result.Lines` carry no `-Because`, but the expected array is self-describing. See code-review finding CR-2. |
| Scenario completeness | PASS | Positive (root, nested, upper-case, bare `.bak`), negative (directory `bak`, `.bakery`, `.bak.md`, `backup`, extensionless, bare `bak`), governance skip, combined rule C plus rule B, clean listing. |
| Negative control | PASS | P1-T4 RED run: 12 failures (10 rule tests plus 2 orchestration tests) before the production change; P1-T8/P2-T3 GREEN: 45 tests, 0 failures, 0 errors. P1-T9 real-tree control printed `HYGIENE Findings=3`, exit 1, with the three files still present. |
| Test file location | PASS | Tests are in `tests/scripts/hygiene/` mirroring `scripts/hygiene/`. |
| Banned determinism APIs | PASS | No `Start-Sleep`, wall-clock or random use in the added tests. |
| Line coverage threshold | PASS (conditional) | See section 5. Numeric figure pending CI. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero C# files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: pending CI measurement. Post-change: pending CI measurement. Change: 6 changed PowerShell files, 14 added tests. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/qa-gates/p2-t4-statement-coverage-map.2026-10-02T05-17.md and evidence/baseline/p0-t16-coverage-limitation.2026-10-02T05-11.md.

### 1.2.2 Coverage Artifact State

### Coverage Evidence Checklist

- C# baseline coverage artifact: `N/A - out of scope`
- C# post-change coverage artifact: `N/A - out of scope`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `CI _pester.yml run on the base commit (pester-coverage JaCoCo artifact), pending`
- PowerShell post-change coverage artifact: `CI _pester.yml run on the pull request head (pester-coverage JaCoCo artifact), pending`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

PowerShell Pester coverage verdict: PASS (conditional) - all 3 added executable statements are exercised by named passing tests; the repo-wide LINE figure is CI-sourced and pending.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity and separation of concerns | PASS | `Test-BackupFilePath` is a pure one-statement predicate in `Test-RepositoryHygiene.Rules.ps1`; orchestration only calls it. Rule C does not add an exemption mechanism. |
| Reuse | PASS | Same `[System.IO.Path]::GetExtension(...) -ieq` idiom already used for the `.xml` gate in the orchestrator. |
| Error handling | PASS | No new catch blocks; the existing unreadable-file handling is unchanged. |
| Documentation | PASS | Comment-based help on the new function; header comment and `.DESCRIPTION` of `Invoke-RepositoryHygieneMain` updated; README `_hygiene.yml` row updated. |
| File size limit (500 lines) | PASS | Re-read from disk: `Test-RepositoryHygiene.ps1` 93, `.Rules.ps1` 146, `.Tests.ps1` 204, `.Rules.Tests.ps1` 288 lines (P2-T5; the main script was independently read in full by this review at 93 lines). |
| Public API compatibility | PASS | No signature changed; one function added; output format extended by one new line kind. |
| Footprint | PASS (with note) | Non-docs footprint is exactly three deletions and six modifications as required (P2-T15, confirmed by the diff listing). Two extra files are present under `.claude/agent-memory/orchestrator/` (see section 7, row Footprint). |
| Dependencies | PASS | None added. |
| Toolchain loop (format, analyze, test) | PASS | PoshQC format run changed nothing (hash-before equals hash-after, six files); PoshQC analyze: pass (0 findings); tool reports no count; PoshQC test: 45 tests, 0 errors, 0 failures. |

## 3. Language-Specific Code Change Policy Compliance

PowerShell (`.claude/rules/powershell.md`):

| Requirement | Verdict | Evidence |
|---|---|---|
| Advanced function with `CmdletBinding`, mandatory typed parameter, `OutputType` | PASS | `Test-BackupFilePath` declares `[CmdletBinding()]`, `[OutputType([bool])]`, `[Parameter(Mandatory = $true)] [string]$RelativePath`. |
| Approved verb and descriptive noun | PASS | `Test-` verb; PSScriptAnalyzer reported zero findings. |
| PowerShell 7 compatibility | PASS | Uses .NET `System.IO.Path`; the guard run by this review under `pwsh` printed `HYGIENE Findings=0`. |
| No `Invoke-Expression`, secrets or hard-coded paths | PASS | None added. |
| Gates run through PoshQC MCP only | PASS | P2-T1, P2-T2, P2-T3 evidence records the MCP tool calls; no raw `Invoke-Pester` or `Invoke-ScriptAnalyzer`. |
| Mocking rules | PASS | Orchestration tests mock `Invoke-GitExe` with parity signature `param([string[]]$GitArgs)`; git is never mocked directly. |
| Change budget | PASS | Two production PowerShell files plus two test files. |

Other changed file types: `.gitignore` (one added line, `*.bak`) and the README table row are configuration and documentation changes with no language policy.

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Pester v5, `Describe`/`It`, `*.Tests.ps1` naming | PASS | New `Describe 'Test-BackupFilePath'` in `Test-RepositoryHygiene.Rules.Tests.ps1`; four new `It` blocks in `Test-RepositoryHygiene.Tests.ps1`. |
| One behavior per `It` | PASS (note) | One exception: the lookalike-names test asserts five names in one `It`; see code-review finding CR-1. |
| Mock before use, signature parity | PASS | `Mock Invoke-GitExe` in `BeforeEach`. |
| Violating fixtures assembled at run time | PASS | The existing `$script:Violation` concatenation is reused; no contiguous profile path is committed (reviewer sweep of the feature folder and the changed files found none). |
| No temp files | PASS | None. |

## 5. Test Coverage Detail

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 4 | 45 total (31 baseline, 14 added) | PASS | N/A - CI-sourced, pending | N/A - CI-sourced, pending | 100% |
| C# | 0 | N/A | N/A | N/A | N/A | N/A |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Coverage artifact state and verdict by language:

| Language | Coverage artifact | Verdict | Disposition |
|---|---|---|---|
| PowerShell | CI pester-coverage JaCoCo artifact from `_pester.yml` (pending); local PoshQC document carries 13 packages and zero `scripts/hygiene` entries (P0-T16) | PASS (conditional) | Numeric LINE figure to be read from CI after the push |
| C#, TypeScript, Python | none required | not evaluated | Zero changed files for these languages |

Statement-to-test map (P2-T4, re-derived by this review from the diff):

- `Test-BackupFilePath` body (one `return` statement): exercised by 10 passing tests; the true result by 4 tests and the false result by 6 tests.
- `if (Test-BackupFilePath ...)` in `Invoke-RepositoryHygieneMain`: true side by the seeded `.sln.bak` control and the `.bak` plus profile-path test; false side by the lookalike and clean-listing tests; the governance skip that precedes it by the governance backup test.
- `$lines.Add('HYGIENE backup-file ' + ...)`: exercised by the two true-side tests.
- Added statements: 3 of 3 exercised (100%). Pester measures command and line coverage only, so no branch figure exists.

Repo-wide threshold (85% line): not locally measurable for `scripts/hygiene`; CI `_pester.yml` asserts the `LINE` figure at 80 and publishes the JaCoCo document. The orchestrator is to read it after the push and record it against this audit. A figure below the floor would convert the conditional PASS to FAIL.

Pester breakpoint-binding check: `Test-BackupFilePath` is dot-sourced through the main script from both test files, which is the pattern already used by the existing rule tests, so no new first-parse-copy hazard is introduced.

## 6. Test Execution Metrics

| Metric | Baseline | Post-change | Evidence |
|---|---|---|---|
| Tests in `tests/scripts/hygiene` | 31 | 45 | P0-T15, P2-T3 |
| Failures | 0 | 0 | P2-T3 |
| Errors | 0 | 0 | P2-T3 |
| RED run before production change | not applicable | 12 failures, exit 1 | P1-T4 |
| Guard run on tree with the three backups present | `HYGIENE Findings=0` (pre-rule) | `HYGIENE Findings=3`, exit 1 | P0-T12, P1-T9 |
| Guard run on final tree | `HYGIENE Findings=0` | `HYGIENE Findings=0`, exit 0 | P2-T17; reviewer re-run |

## 7. Code Quality Checks

| Check | Result | Command / Evidence |
|---|---|---|
| Format | PASS | PoshQC format over `scripts/hygiene` and `tests/scripts/hygiene`; six file hashes unchanged (P2-T1). |
| Analyze | PASS | PoshQC analyze: pass (0 findings); tool reports no count (P2-T2). |
| Test | PASS | PoshQC test: tests=45, errors=0, failures=0 (P2-T3). |
| Guard on final tree | PASS | Reviewer ran the guard under `pwsh` from the worktree root: `HYGIENE Findings=0`, exit 0. |
| AC-1 index check | PASS | `git ls-files -- "*.bak"` printed nothing at HEAD (reviewer re-run); worktree status is clean. |
| No reader of the deleted files | PASS | Reviewer search for `sln.bak` and `vbproj.bak` outside `docs/`, `.claude/` and `artifacts/` returned no match. |
| Evidence artifact shape | PASS (note) | All 47 evidence documents carry `Timestamp:`. 43 command-bearing documents also carry `EXIT_CODE:` and `Output Summary:`. The other 4 (`phase0-instructions-read`, `preflight-clearance`, `p1-t1-implementation-handoff`, `p2-t25-audit-handoff`) are narrative notes without a command. |
| Host path and account sweep | PASS (note) | Evidence uses `<worktree-root>` and `<BASE-SHA>` placeholders. The only matches for the account handle are the repository URL in `issue.md` and the promoted record, and `Owner: drmoisan` in the plan header; no drive-letter or profile path is present. |
| Raw junit or collector XML committed | PASS | The branch diff adds no XML file. |
| Tonality | PASS | Executor artifacts are factual; no humor, hyperbole or metaphor found in the changed documents. |
| Suppression scan (added lines) | PASS | No `SuppressMessage` or analyzer-disable attribute added. |
| Workflow change scan | PASS | No workflow YAML changed; only the README table row. |
| Footprint | PASS (note) | Two orchestrator memory files outside the stated footprint: `.claude/agent-memory/orchestrator/MEMORY.md` (one line added) and `.claude/agent-memory/orchestrator/poshqc-gates-observed-outputs-for-scripts-hygiene.md` (new, 17 lines). They sit under the governance directory, which the guard excludes, carry no host paths, and are routine agent-memory commits. Recorded for the orchestrator; not blocking. |

## Appendix A: Test Inventory

Added to `tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1` (`Describe 'Test-BackupFilePath'`, 10 tests):

1. returns true for a root-level solution backup
2. returns true for a nested project backup
3. returns true for an upper-case extension
4. returns true for a bare dot-bak file name
5. returns false for a directory named bak
6. returns false for a longer extension that begins with bak
7. returns false when bak is not the final extension
8. returns false for the word backup
9. returns false for a path with no extension
10. returns false for the bare word bak

Added to `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1` (`Describe 'Invoke-RepositoryHygieneMain'`, 4 tests):

1. reports a tracked backup file as a finding and fails the guard
2. reports zero findings for backup-lookalike names
3. drops a governance-directory backup record before the backup rule runs
4. still scans a backup file for a profile path

## Appendix B: Toolchain Commands Reference

- Format: `mcp__drm-copilot__run_poshqc_format` with the worktree root and scan folders `scripts/hygiene`, `tests/scripts/hygiene`.
- Analyze: `mcp__drm-copilot__run_poshqc_analyze` with the same scan folders.
- Test: `mcp__drm-copilot__run_poshqc_test` with scan folder `tests/scripts/hygiene`.
- Guard: `pwsh -NoProfile -Command "Set-Location <worktree-root>; & ./scripts/hygiene/Test-RepositoryHygiene.ps1; exit $LASTEXITCODE"`.
- Index check: `git -C <worktree-root> ls-files -- "*.bak"`.
- Diff: `git -C <worktree-root> diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`.
