# Feature Audit: global-json-sdk-pin-regressed-to-10 (Issue #194)

**Audit Date:** 2026-06-13
**Feature Folder:** `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194`
**Base Branch:** `origin/main`
**Head Branch:** `feature/csharp-coverage-uplift` (PR-context head `bug/global-json-sdk-pin-194` @ `057dbc82`)
**Work Mode:** `minor-audit`
**Audit Type:** Minor-audit acceptance validation

---

## Scope and Baseline

- **Base branch:** `origin/main` (commit `1b3f5350065b27c538c01542eb1400f8cca20d9d`)
- **Head branch/commit:** `bug/global-json-sdk-pin-194` (commit `057dbc82e318fb1ec8fc215c358fda6f67d11801`, per PR-context summary)
- **Merge base:** `1b3f5350065b27c538c01542eb1400f8cca20d9d`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/**`
  - Additional evidence: `git diff 1b3f5350...HEAD` and an independent re-run of `mcp__drm-copilot__run_poshqc_analyze`
- **Feature folder used:** `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194`
- **Requirements source:** `issue.md` (`## Acceptance Criteria`, AC1–AC4)
- **Work mode resolution note:** `issue.md` line 12 contains `- Work Mode: minor-audit`. Per the work-mode contract, the only authoritative AC source is the explicit `## Acceptance Criteria` section in `issue.md`. Neither `spec.md` nor `user-story.md` exists in the feature folder (confirmed by directory listing), consistent with the minor-audit mode.
- **Scope note:** The audit covers the full branch diff against the resolved base. The only non-documentation change is `global.json` (single-field revert). All other diff entries are documentation/evidence files and one promotion rename into `issue.md`.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/issue.md` — only source (minor-audit)

### Acceptance criteria

1. AC1: `global.json` `sdk.version` is `8.0.205` (reverted from `10.0.200`); `rollForward`, `allowPrerelease`, and `paths` are unchanged.
2. AC2: `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` passes, including the `global.json SDK selection` assertions (version `8.0.205`, `rollForward` `latestFeature`, `allowPrerelease` false, `paths` contains `.dotnet-sdk` and `$host$`).
3. AC3: No other `global.json` keys or unrelated files are modified (scope limited to the one-field revert).
4. AC4: The PowerShell toolchain (PoshQC format, PSScriptAnalyzer, Pester) passes with no new findings on changed/related files.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | `global.json` `sdk.version` = `8.0.205`; other keys unchanged | PASS | `git diff` shows exactly one changed line (version `10.0.200`->`8.0.205`); `rollForward`=`latestFeature`, `allowPrerelease`=false, `paths`=`[".dotnet-sdk","$host$"]`, `errorMessage` all unchanged. Current `global.json` line 3 = `"version": "8.0.205"`. | `git diff 1b3f5350...HEAD -- global.json` | Independently inspected during this review. |
| 2 | Regression test passes incl. `global.json SDK selection` assertions | PASS | `final-qa-pester-2026-06-13T09-00.md`: Passed 2, Failed 0; version `8.0.205`, `rollForward` `latestFeature`, `allowPrerelease` false, `paths` contains `.dotnet-sdk` and `$host$`. Fail-before captured in `baseline-pester-2026-06-13T09-00.md`. | `mcp__drm-copilot__run_poshqc_test (scan_folders: tests/scripts/vscode)` | Fail-before/pass-after chain complete. |
| 3 | No other `global.json` keys or unrelated files modified | PASS | `git diff --name-only 1b3f5350...HEAD \| grep -vE '^docs/'` returns only `global.json`; the `global.json` diff is a single line. Other diff entries are docs/evidence files and one promotion rename into `issue.md`. | `git diff --name-status 1b3f5350...HEAD` | Documentation/evidence and the promotion rename are expected by-products of the minor-audit workflow, not unrelated code/config edits. |
| 4 | PowerShell toolchain passes; no new findings on changed/related files | PASS | Format EXIT 0 clean (`final-qa-format`); analyzer 16 post-change = 16 baseline, delta 0 (`final-qa-analyze` + Phase 0 baseline), independently reproduced (exactly 16) during this review; Pester 2/2 pass. | `mcp__drm-copilot__run_poshqc_format` / `run_poshqc_analyze` / `run_poshqc_test` | Analyzer non-zero exit reflects pre-existing baseline debt in unrelated `scripts/vscode` scripts; zero new findings attributable to this branch. |

---

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**
- **PASS:** 4 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. None.

**Recommended follow-up verification steps:**

1. Optional, non-blocking: update the `global.json` `errorMessage` to reference `csharpier .` instead of `dotnet format` (already recorded as optional in `issue.md`).
2. Confirm the codex-web-setup workflow run uses the restored `8.0.205` marker directory on the next CI execution.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if they are represented as markdown checkboxes and are not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

All four AC items (AC1–AC4) in `issue.md` are already marked `[x]` (checked off by the executor after verification). All four are confirmed PASS by this audit, so no check-off change is required; the existing checked state is correct and is retained.

### AC Status Summary

- Source: `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/issue.md`
- Total AC items: 4
- Checked off (delivered): 4
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `issue.md` | 4 | 4 | 0 | Checkbox-backed; all already checked and confirmed PASS by this audit. |
