# Policy Audit (reduced audit, minor-audit): issue 953

Timestamp: 2026-10-02T04-05

## Template Resolution Deviation

The policy-audit template MCP asset resolver is not in this agent's tool set. This document is hand-authored with the canonical headings. PR-context artifacts were not regenerated; scope was derived from `git -C <execution-worktree-root> diff --numstat 860d67bf4fddecb929e0d6c166065fd1ee752feb 2449cdd34` and the plan Write Set.

## Rejected Scope Narrowing

None. The caller prompt asked for a reduced audit; the full branch-versus-base diff was audited. The caller instruction not to mark coverage FAIL for lack of a local figure is a legitimate statement of where the figure comes from (CI Pester job, `COVERAGE-SOURCE: CI`), not a narrowing; the PowerShell verdict is recorded as pending CI rather than as a pass.

## Executive Summary

Overall verdict: PASS with one item pending CI. 0 blocking findings.

- PowerShell is the only language with changed code (2 files). The 11 `app.config` edits are configuration, not code.
- The PowerShell line-coverage figure (floor 85 percent) is produced by the CI Pester job and is pending. The orchestrator records it.
- No temporary files, no `$TestDrive`, no absolute host paths in committed evidence, both files under 500 lines, Write Set matches the plan, Unsafe redirects untouched.

## 1. Policy Compliance Summary

### 1.1 Policies evaluated

| Policy | Verdict | Evidence |
|---|---|---|
| CLAUDE.md (policy order, tonality) | PASS | Artifacts neutral; no emoji or hyperbole. |
| general-code-change.md (500-line cap, error handling, I/O seam) | PASS | Module 139 lines, tests 335 lines (`evidence/qa-gates/p2-t6-file-size-audit.2026-10-02T03-42.md`); parser errors propagate; scriptblock seam for provider. |
| general-unit-test.md (AAA, no temp files, scenarios, location) | PASS | Test at `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` mirrors `scripts/dependencies/`; no `TestDrive`/temp APIs (Grep over the file returned no match); AAA comments present. |
| powershell.md (advanced functions, Pester v5, toolchain order) | PASS | `CmdletBinding`, mandatory params, StrictMode; format iter2, analyze, test recorded under `evidence/qa-gates/p2-t1..p2-t4`. |
| powershell.md (change budget) | PASS | 1 production file and 1 test file (within 2 and 3 caps); `evidence/baseline/p0-t13-budget-baseline`, `p2-t17` reports no denial. |
| quality-tiers.md / coverage | PENDING CI | See section 1.2. |
| tonality.md | PASS | Feature-folder artifacts and this audit use plain wording. |

### 1.2 Coverage

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 2 (1 production, 1 test) | 14 new (151 total) | PASS (0 failures) | N/A - new module, no baseline | N/A - pending CI Pester job | N/A - pending CI Pester job |
| C# | 0 | 0 | N/A | N/A | N/A | N/A |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |

Coverage source: `COVERAGE-SOURCE: CI` (pending). No local Pester route instruments `scripts/dependencies` (plan exception D8, recorded in `evidence/other/p2-t17-reduced-audit-handoff.2026-10-02T03-56.md`). The orchestrator records the CI run ID, head SHA and the per-file figure for `scripts/dependencies/BindingRedirectVerification.psm1` from the `pester-coverage` artifact. Required: line coverage at least 85 percent (repository rule; workflow floor 80 percent); no branch threshold applies to PowerShell. Both exported functions are exercised on positive, negative and edge paths (`p2-t5-function-test-map`), so a high line figure is expected, but this review states no figure.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `N/A - out of scope`
- C# post-change coverage artifact: `N/A - out of scope`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - new module`
- PowerShell post-change coverage artifact: `CI Pester job artifact pester-coverage, pending`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: N/A. Post-change: N/A. Change: new module, figure produced by the CI Pester job. Disposition: INCOMPLETE. Evidence: `COVERAGE-SOURCE: CI` pending; `evidence/other/p2-t17-reduced-audit-handoff.2026-10-02T03-56.md`.
- C#: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero C# source files changed (only `app.config` redirects; `evidence/qa-gates/p2-t7-no-csharp-scope`).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Coverage artifact | State |
|---|---|---|
| PowerShell | CI `pester-coverage` JaCoCo | Pending; orchestrator records run ID and head SHA |

## 2. Evidence Location Compliance

PASS. All evidence lives under `docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/{baseline,qa-gates,regression-testing,other}/`. The diff contains no file under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. `validate_evidence_locations.py` was not run (Bash restricted to git); the path check was made from the numstat listing.

## 3. Host-Path and Identity Hygiene

PASS. A Grep over the feature folder for drive-letter paths, `DanMoisan`, `/Users/` and the worktree directory name returned no match; evidence uses `<execution-worktree-root>`. The four `.claude/agent-memory/atomic-planner/*953*` files also returned no match.

## 4. Configuration Edit Integrity

- 11 files at 1 insertion and 1 deletion (numstat): PASS.
- Footprint equals plan section 5 once `.claude/agent-memory/**` (Convention C6) and the inherited potential-entry move are removed: PASS.
- Unsafe redirects untouched (17 at 6.0.3.0; no diff line mentions Unsafe): PASS.
- CRLF and BOM preservation: PASS on executor evidence (`ls-files --eol` reports `w/crlf`, BOM bytes `239,187,191`, per file in `p1-t4` to `p1-t14`). This reviewer's own CR-anchored Grep returned no match, which reflects the search tool's handling of CRLF and not a defect; no byte-level independent check was possible under the Bash restriction. Residual risk: low (a lost CR would also add diff noise beyond one line only if the whole file were rewritten, which numstat 1/1 excludes).

## 5. Toolchain Evidence

- Format: iteration 1 rewrote the two new files (indentation), iteration 2 rewrote none (`p2-t1-poshqc-format.iter1`, `.iter2`).
- Analyze: `PoshQC analyze: pass (0 findings); tool reports no count` (`p2-t2-poshqc-analyze.iter2`).
- Test: 9 suites, 151 tests, 0 failures (`p2-t3-poshqc-test.iter2`); fail-before run: 2 failures (`p1-t3-fail-before`).
- Loop closure: terminal iteration 2 (`p2-t4-loop-closure`). C# toolchain not applicable (no C# source change).

## 6. Findings and Remediation Triggers

- Blocking: none.
- Non-blocking: see code-review CR-1 to CR-4.
- Remediation trigger: none. Pending item for the orchestrator: record the CI PowerShell line-coverage figure for `scripts/dependencies/BindingRedirectVerification.psm1`; if it is below 85 percent, that becomes a remediation trigger at that time.

## 7. Verdict Table

| Item | Verdict |
|---|---|
| Policy order and tone | PASS |
| File-size cap | PASS |
| No temp files or TestDrive | PASS |
| AAA and naming | PASS |
| Scenario completeness | PASS (CR-2 note) |
| Ratchet fails on new mismatch, Fizzler regression, stale entry | PASS |
| No absolute host paths in evidence | PASS |
| Write Set footprint | PASS |
| Unsafe untouched | PASS |
| PowerShell line coverage | PENDING CI (not FAIL, not PASS) |

## Appendix A: Test Inventory

`tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`: 14 tests. Describe `Find-StaleBindingRedirect (in-memory fixtures)` 9 tests (lines 102-211); Describe `ConvertTo-ReferenceVersionMap (in-memory fixtures)` 3 tests (216-241); Describe `Repository binding redirects (issue 953)` 2 tests (246, 275).

## Appendix B: Toolchain Commands Reference

PoshQC MCP tools: `run_poshqc_format`, `run_poshqc_analyze`, `run_poshqc_test` with the item worktree as workspace root and `scan_folders` `["tests/scripts/dependencies"]`. Pester coverage: CI `_pester.yml` job, artifact `pester-coverage`.
