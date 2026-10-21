# Policy Audit - Issue #961 (re-run after remediation Phase 3)

- Timestamp: 2026-10-02T06-18
- Work Mode: minor-audit (reduced small-audit, second cycle)
- Base: 94287369908cc920b21b0e3256314f988ad7d2f5 (`git merge-base origin/main HEAD`; equals `origin/main`); Head: 03f4b37c110222149e8a3f12300de22b60594d4d
- Scope: full branch diff against the resolved base (`git diff origin/main...HEAD`)
- Supersedes for the final tree: `policy-audit.2026-10-02T05-30.md` (not overwritten)

## Executive Summary

Verdict: PASS with no blocking findings. Remediation Phase 3 (commits 16202b688 to 03f4b37c1) closed the three prior low findings. CR-1: the bundled lookalike `It` is now one `-ForEach` `It` over five cases, each with a `-Because` that names the case. CR-2: every `Should` assertion in the two modified test files carries `-Because` (29 of 29 and 31 of 31 by count). CR-3: `.gitignore` carries only `*.bak`; `*.rptproj.bak` and `*.csproj.bak` are removed; AC-2 still holds and its negative-control evidence is present. Reviewer-run guard output on the final tree is `HYGIENE Findings=0` (exit 0); `git ls-files -- "*.bak"` prints nothing; the worktree is clean.

CI-measured coverage for the current head is pending. CI run 36990562362 on the earlier head b00fd1f19 measured `scripts/hygiene` at 94.23 percent line coverage (98 of 104). The diff from that head to the current head touches only `.gitignore` and `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1`, so no production line changed since that measurement. The figure for the current head is read from the CI run that follows the push and is recorded as pending here.

Non-blocking: two orchestrator memory files remain in the branch diff outside the declared footprint (carried from the first cycle, classified RELATED/Info); `Test-RepositoryHygiene.Git.Tests.ps1`, which this branch does not touch, has 7 assertions without `-Because` (pre-existing, UNRELATED/Info).

## Rejected Scope Narrowing

- Caller text (verbatim): "CI-measured coverage is cited as pending (the orchestrator will read the CI run)".
- Disposition: not a language exclusion. The numeric CI figure is recorded as pending for the current head, the earlier-head CI measurement is cited as prior evidence, and an explicit PASS verdict is still recorded for PowerShell on the evidence that exists.
- No other caller text narrowed the scope. The "reduced small-audit" framing is the work-mode artifact set, not a scope reduction; the audit covered the full branch diff.

## Evidence Location Compliance

- Scanned the branch diff for paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` and `artifacts/coverage/`: zero paths found.
- All evidence lives under `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/evidence/<kind>/` (kinds `baseline`, `other`, `qa-gates`, `regression-testing`), including the 26 Phase 3 documents.
- `validate_evidence_locations.py` is not present in this worktree; the scan was done on the full `git diff --numstat origin/main...HEAD` listing. Result: PASS.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required.
- No raw junit, trx or collector XML is added by this branch.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence and determinism | PASS | Rules tests call a pure predicate with literal strings. Orchestration tests mock `Invoke-GitExe` (the wrapper seam) in `BeforeEach` and reset `$script:Content` and `$script:Listing` per test; the `-ForEach` cases share no state across cases. |
| No temp files, network or external process | PASS | Tests use in-memory listings and a content delegate; no file system access. |
| Arrange-Act-Assert | PASS | Each added `It`, including the `-ForEach` one, separates setup, one invocation and assertions by blank lines. |
| Failure messages (`-Because`) | PASS | Count of `Should -` lines equals count of `Should -.*-Because` lines in `Test-RepositoryHygiene.Tests.ps1` (29 and 29) and `Test-RepositoryHygiene.Rules.Tests.ps1` (31 and 31). The `-ForEach` assertions interpolate `$Name` in the reason text. See section 4 for the untouched Git test file. |
| Scenario completeness | PASS | Positive, negative, governance skip, combined rule C plus rule B, clean listing; the five lookalike names are now five separately reported cases. |
| Negative control | PASS | Phase 1 RED run (12 failures) and real-tree control (`HYGIENE Findings=3`, exit 1); Phase 3 `.gitignore` negative controls in section 7. |
| Test file location | PASS | `tests/scripts/hygiene/` mirrors `scripts/hygiene/`. |
| Banned determinism APIs | PASS | No sleep, wall-clock or random use in added or changed tests. |
| Line coverage threshold | PASS (conditional) | See section 5. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero C# files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: pending CI measurement for the base commit. Post-change: 94.23 percent line coverage for `scripts/hygiene` at the earlier head b00fd1f19 (CI run 36990562362); current head pending CI. Change: no production PowerShell line changed between that head and the current head. New/changed-code coverage: 100 percent (3 of 3 added statements; the new function file is 32 of 32 in the CI document). Disposition: PASS. Evidence: evidence/other/ci-pester-coverage.2026-10-02T09-45.md and evidence/qa-gates/p2-t4-statement-coverage-map.2026-10-02T05-17.md.

### 1.2.2 Coverage Artifact State

### Coverage Evidence Checklist

- C# baseline coverage artifact: `N/A - out of scope`
- C# post-change coverage artifact: `N/A - out of scope`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `CI _pester.yml run on the base commit (pester-coverage JaCoCo artifact), pending`
- PowerShell post-change coverage artifact: `CI run 36990562362 pester-coverage JaCoCo artifact at head b00fd1f19 (scripts/hygiene 94.23 percent line); current head 03f4b37c1 pending CI`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

PowerShell Pester coverage verdict: PASS (conditional) - production lines unchanged since the CI-measured head at 94.23 percent, all 3 added executable statements are exercised by named passing tests, and the current-head CI figure is pending.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity and separation of concerns | PASS | `Test-BackupFilePath` is a one-statement pure predicate; orchestration only calls it; no exemption mechanism added. Phase 3 changed no production file. |
| Reuse | PASS | Same `GetExtension ... -ieq` idiom as the existing `.xml` gate. |
| Error handling | PASS | No new catch blocks. |
| Documentation | PASS | Comment-based help on the new function; header comment, `.DESCRIPTION` and README `_hygiene.yml` row updated. |
| File size limit (500 lines) | PASS | Read from disk: `Test-RepositoryHygiene.ps1` 93, `.Rules.ps1` 146, `Test-RepositoryHygiene.Tests.ps1` 209, `.Rules.Tests.ps1` 288. |
| Public API compatibility | PASS | No signature changed; one function and one output line kind added. |
| Footprint | PASS (note) | Non-docs, non-memory footprint is exactly three deletions and six modifications (P3-T24 and reviewer diff). Phase 3 added no path to it. Two orchestrator memory files remain (section 7). |
| Dependencies | PASS | None added. |
| Toolchain loop (format, analyze, test) | PASS | PoshQC format: six file hashes identical before and after (P3-T19). PoshQC analyze: ok result; tool reports no finding count (P3-T20). PoshQC test: tests=49, errors=0, failures=0 (P3-T21). |

## 3. Language-Specific Code Change Policy Compliance

PowerShell (`.claude/rules/powershell.md`):

| Requirement | Verdict | Evidence |
|---|---|---|
| Advanced function, `CmdletBinding`, mandatory typed parameter, `OutputType` | PASS | `Test-BackupFilePath` declaration unchanged from the first cycle. |
| Approved verb | PASS | `Test-`; analyze reported no finding. |
| PowerShell 7 compatibility | PASS | Reviewer ran the guard under `pwsh`: `HYGIENE Findings=0`. |
| Gates through PoshQC MCP only | PASS | P3-T19 to P3-T21 record the MCP tool calls; no raw `Invoke-Pester` or `Invoke-ScriptAnalyzer`. |
| Mocking rules | PASS | `Mock Invoke-GitExe` with `param([string[]]$GitArgs)`; unchanged by Phase 3. |
| Change budget | PASS | Two production PowerShell files plus two test files. |

Other changed file types: `.gitignore` (net: `*.rptproj.bak` and `*.csproj.bak` replaced by `*.bak`) and the README row are configuration and documentation changes with no language policy.

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Pester v5, `Describe`/`It`, `*.Tests.ps1` | PASS | New `Describe 'Test-BackupFilePath'`; five `It` blocks (one `-ForEach`) added to `Test-RepositoryHygiene.Tests.ps1`. |
| One behavior per `It` | PASS | The prior exception (five names in one `It`) is resolved: one `-ForEach` `It` with five hashtables, one per line; PoshQC junit shows five expanded passed names (P3-T21). |
| Mock before use, signature parity | PASS | Unchanged. |
| Violating fixtures assembled at run time | PASS | No contiguous profile path in the changed files; reviewer sweep of the feature folder found only the repository URL in `issue.md`. |
| No temp files | PASS | None. |
| `-Because` on assertions | PASS (note) | Complete in both modified files. `Test-RepositoryHygiene.Git.Tests.ps1` (not in the branch diff) has 15 `Should -` lines and 8 with `-Because`; the 7 gaps pre-date this branch. Classified UNRELATED, Info; no change required by this item. |

## 5. Test Coverage Detail

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 4 | 49 total (31 baseline, 18 added) | PASS | N/A - CI-sourced, pending | 94.23% (scripts/hygiene, CI at head b00fd1f19); current head pending CI | 100% |
| C# | 0 | N/A | N/A | N/A | N/A | N/A |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Coverage artifact state and verdict by language:

| Language | Coverage artifact | Verdict | Disposition |
|---|---|---|---|
| PowerShell | CI pester-coverage JaCoCo artifact (run 36990562362, head b00fd1f19, read); current-head run pending. Local PoshQC document carries no `scripts/hygiene` entry (P0-T16). | PASS (conditional) | Orchestrator reads the current-head CI LINE figure after the push. A figure below 85 percent for a changed file would convert the verdict to FAIL. |
| C#, TypeScript, Python | none required | not evaluated | Zero changed files for these languages |

Statement-to-test map (re-derived from the diff): `Test-BackupFilePath` return statement exercised by 10 rule tests (true by 4, false by 6); the `if` and the `$lines.Add` in `Invoke-RepositoryHygieneMain` exercised on the true side by the seeded `.sln.bak` control and the `.bak` plus profile-path test, on the false side by the five lookalike cases and the clean listing; the governance skip by the governance backup test. Added statements: 3 of 3 (100 percent). Pester reports no branch figure.

Per-file CI figures at head b00fd1f19 (production unchanged since): `Test-RepositoryHygiene.Rules.ps1` 32 of 32 (100 percent); `Test-RepositoryHygiene.ps1` 32 of 35 (91.43 percent; `Invoke-RepositoryHygieneMain` 28 of 28, the 3 missed lines are in the script-level entry). Both meet the 85 percent line floor. The Phase 3 test split does not reduce coverage of any changed line: the five names that previously ran in one `It` still run, each as its own case.

## 6. Test Execution Metrics

| Metric | Baseline | Post-change | Evidence |
|---|---|---|---|
| Tests in `tests/scripts/hygiene` | 31 | 49 | P0-T15, P3-T21 |
| Failures | 0 | 0 | P3-T21 |
| Errors | 0 | 0 | P3-T21 |
| Tests after Phase 2 | 45 | 49 (the lookalike `It` became five cases) | P3-T21 |
| Guard run on tree with the three backups present | `HYGIENE Findings=0` (pre-rule) | `HYGIENE Findings=3`, exit 1 | P0-T12, P1-T9 |
| Guard run on final tree | `HYGIENE Findings=0` | `HYGIENE Findings=0`, exit 0 | P3-T23; reviewer re-run |
| CI Pester at head b00fd1f19 | not applicable | Passed=407 Failed=0, LinePercent=94.64 | evidence/other/ci-pester-coverage.2026-10-02T09-45.md |

## 7. Code Quality Checks

| Check | Result | Command / Evidence |
|---|---|---|
| Format | PASS | PoshQC format, six hashes unchanged (P3-T19). |
| Analyze | PASS | PoshQC analyze: ok result; tool reports no finding count (P3-T20). |
| Test | PASS | PoshQC test: tests=49, errors=0, failures=0 (P3-T21). |
| Guard on final tree | PASS | Reviewer ran the guard under `pwsh` from the worktree root: `HYGIENE Findings=0`. |
| AC-1 index check | PASS | `git ls-files -- "*.bak"` prints nothing at HEAD; `git status --short` is empty. |
| AC-2 after CR-3 | PASS | `.gitignore` line 257 is `*.bak` and the only `.bak` rule in the file (reviewer read lines 250 to 261). Reviewer `git check-ignore -q TaskMaster.sln.bak` exits 0. P3-T14: three names attributed to `.gitignore:257:*.bak`. P3-T16: `README.md` exits 1 (discriminating). |
| CR-3 negative controls | PASS | P3-T9 and P3-T10: with `*.bak` removed, `foo.bak` matches no rule and `check-ignore` exits 1, so the check can fail and `*.bak` is the covering rule. P3-T11: the two specific lines cover only their own names. P3-T12: restoration byte-identical (hash before equals hash after, `e0c040d9...`). |
| No reader of the deleted files | PASS | First-cycle search unchanged; no build input names them. |
| Evidence artifact shape | PASS (note) | Command-bearing Phase 3 documents carry `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; expected non-zero commands carry `ExpectedExitCode:`. Handoff notes (`p3-t1`, `p3-t27`) carry no command. |
| Host path and account sweep | PASS | Regex sweep over the feature folder (excluding the three audit artifacts) for drive letters, profile paths and the account handle returned one match: the repository URL in `issue.md` line 10. Evidence uses `<worktree-root>` and `<BASE-SHA>`. |
| Raw junit or collector XML committed | PASS | None added. |
| Tonality | PASS | Phase 3 artifacts and the changed test text are factual; no humor, hyperbole or metaphor found. |
| Suppression scan | PASS | None added. |
| Workflow change scan | PASS | No workflow YAML changed. |
| Footprint | PASS (note) | `.claude/agent-memory/orchestrator/MEMORY.md` (modified) and `poshqc-gates-observed-outputs-for-scripts-hygiene.md` (added) remain outside the declared footprint. Governance-directory content, excluded by the guard, no host paths. RELATED, Info; the orchestrator confirms they belong in the pull request. |
| Issue.md untouched in Phase 3 | PASS | `issue.md` shows 7 of 7 criteria checked; Phase 3 evidence states no check-off changed. |

## Appendix A: Test Inventory

Added to `tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1` (`Describe 'Test-BackupFilePath'`, 10 tests): root-level solution backup; nested project backup; upper-case extension; bare dot-bak name; directory named bak; `.bakery`; `.bak.md`; the word backup; no extension; bare word bak.

Added to `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1` (`Describe 'Invoke-RepositoryHygieneMain'`, 4 `It` blocks, 8 executed tests):

1. reports a tracked backup file as a finding and fails the guard
2. reports zero findings for the backup-lookalike name `<Name>` (`-ForEach`, five cases: `docs/bak/notes.md`, `notes.bakery`, `notes.bak.md`, `backup`, `docs/features/x/Makefile`)
3. drops a governance-directory backup record before the backup rule runs
4. still scans a backup file for a profile path

Baseline 31 plus 10 plus 1 plus 5 plus 1 plus 1 = 49.

## Appendix B: Toolchain Commands Reference

- Format: `mcp__drm-copilot__run_poshqc_format` with the worktree root and scan folders `scripts/hygiene`, `tests/scripts/hygiene`.
- Analyze: `mcp__drm-copilot__run_poshqc_analyze` with the same scan folders.
- Test: `mcp__drm-copilot__run_poshqc_test` with scan folder `tests/scripts/hygiene`.
- Guard: `pwsh -NoProfile -Command "Set-Location <worktree-root>; & ./scripts/hygiene/Test-RepositoryHygiene.ps1; exit $LASTEXITCODE"`.
- Index check: `git -C <worktree-root> ls-files -- "*.bak"`.
- Ignore check: `git -C <worktree-root> check-ignore -q TaskMaster.sln.bak`.
- Diff: `git -C <worktree-root> diff origin/main...HEAD`.
