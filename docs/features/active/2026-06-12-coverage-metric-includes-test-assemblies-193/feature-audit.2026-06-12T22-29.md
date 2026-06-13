# Feature Audit: Koverage Coverage Metric Excludes Test Assemblies (Issue #193)

**Audit Date:** 2026-06-12
**Feature Folder:** `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/`
**Base Branch:** `origin/main` (commit `7798ae1d` per request; branch HEAD `4a21a5b8` equals `origin/main`)
**Head Branch:** `feature/csharp-coverage-uplift` (working-tree scope; #193 change set is uncommitted)
**Work Mode:** `minor-audit`
**Audit Type:** Initial acceptance review (minor-audit)

---

## Scope and Baseline

- **Base branch:** `origin/main` (commit `7798ae1d` per request)
- **Head branch/commit:** `feature/csharp-coverage-uplift` working tree (HEAD `4a21a5b8`)
- **Merge base:** `4a21a5b8` (branch HEAD equals `origin/main`); the #193 change set exists only as uncommitted working-tree modifications plus the untracked feature folder.
- **Evidence sources:**
  - Primary: `git diff` of the two changed files (working tree vs HEAD)
  - Secondary baseline diff: `git show HEAD:<file>` for baseline line counts and analyzer comparison
  - Feature evidence: `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/evidence/**`
  - Additional evidence: independent re-runs of `Invoke-Formatter`, `Invoke-ScriptAnalyzer`, and `Invoke-Pester` during this review
- **Feature folder used:** `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/`
- **Requirements source:** `issue.md` (`## Acceptance Criteria`, AC1-AC6) — sole source for `minor-audit`
- **Work mode resolution note:** `issue.md` line 12 contains `- Work Mode: minor-audit`. Per the acceptance-criteria-tracking rule, only the explicit `## Acceptance Criteria` section in `issue.md` is the AC source.
- **Scope note:** Working-tree-only validation. PR context artifacts were not regenerated because the #193 change set is uncommitted and was reviewed directly via `git diff` against HEAD. Other branch artifacts (root `coverage.xml`, `artifacts/`) are unrelated orchestration bookkeeping and excluded from the #193 acceptance scope.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/issue.md` — only source (work mode `minor-audit`)

### Acceptance criteria

1. AC1: `Get-KoverageProjectAllowlist` excludes projects that resolve to a test assembly (assembly name matching `.Test`), so test projects are not added to the allowlist.
2. AC2: `ConvertTo-KoverageCoberturaXml` output contains no `<package>` whose name corresponds to a `.Test` assembly; both their covered and valid lines are removed from the aggregate `lines-covered` and `lines-valid`.
3. AC3: A failing-first Pester regression in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` asserts test-project exclusion (allowlist excludes `.Test`; post-processed report strips `.Test` packages from numerator and denominator).
4. AC4: Non-test first-party and vendored production packages (UtilitiesCS, QuickFiler, TaskMaster, ToDoModel, Tags, TaskVisualization, VBFunctions, SVGControl, Swordfish.NET.General) remain in the report unchanged.
5. AC5: PowerShell toolchain passes in order for the change scope — PoshQC format clean; PSScriptAnalyzer zero new findings (the single `PSUseSingularNouns` pre-exists on HEAD and is outside the changed function); Pester for `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` 6/6 pass. The folder-level Pester run has one pre-existing, unrelated failure (`Install-RepoDotNetSdk.Tests.ps1`), tracked separately, not part of #193.
6. AC6: No production file exceeds 500 lines; change scope limited to the helper module and its test file.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | AC1 — allowlist excludes `.Test` assemblies | PASS | `Invoke-MSTestWithCoverage.Helpers.ps1` lines 39-41 skip any resolved name ending `.Test`; test `excludes projects that resolve to a .Test assembly name` and `applies the .Test exclusion to the project-file base-name fallback` both pass. | `Invoke-Pester -Path ./tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | Exclusion applies to both the `<AssemblyName>` path and base-name fallback. |
| 2 | AC2 — post-processed report strips `.Test` from numerator and denominator | PASS | Test `excludes .Test packages from the report and from the aggregate covered/valid line totals` asserts `UtilitiesCS.Test` absent and `lines-covered=1`, `lines-valid=2` (production-only). `ConvertTo-KoverageCoberturaXml` removes non-allowlisted packages (line 306) then recomputes totals (lines 328-334). | `Invoke-Pester ...` | Behavior achieved via the allowlist change; post-processor unchanged. |
| 3 | AC3 — failing-first Pester regression | PASS | `evidence/regression-testing/fail-before.2026-06-13T01-56.md` shows the two key regressions fail when the production file is stashed (3 passed, 2 failed before fix); all 6 pass after. | stash + `Invoke-Pester` (recorded in fail-before artifact) | Failing-first demonstrated and documented per evidence conventions. |
| 4 | AC4 — production/vendored packages retained | PASS | `retains non-test production projects in the allowlist` asserts `UtilitiesCS` retained; the strip test retains the `UtilitiesCS` package and its lines. Allowlist logic only excludes `.Test`; all listed production names lack that suffix. | `Invoke-Pester ...` | Swordfish.NET.General has no `.Test` suffix and is retained; only `Swordfish.NET.Test` would be excluded. |
| 5 | AC5 — PowerShell toolchain passes; 0 new analyzer findings | PASS | This review re-ran: Invoke-Formatter FORMAT-CLEAN both files; PSScriptAnalyzer 0 findings on test file, 1 pre-existing on production file (confirmed on HEAD baseline, outside changed function); Pester 6/6 pass. SDK-PIN-001 confirmed unrelated. | `Invoke-Formatter`; `Invoke-ScriptAnalyzer -Path <file>`; `Invoke-Pester ...` | Analyzer finding is `PSUseSingularNouns` on `Get-CoberturaLineConditionCoverageParts` (issue.md/prior evidence said `Merge-CoberturaClassesByFilename`); pre-exists on HEAD either way. |
| 6 | AC6 — no file over 500 lines; scope limited to helper + test | PASS | `Invoke-MSTestWithCoverage.Helpers.ps1` 344 lines; `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` 171 lines. Diff confined to these two files (plus untracked feature docs). | `awk 'END{print NR}' <file>` | Within 500-line limit and within the 2-production/3-test change budget. |

---

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**
- **PASS:** 6 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Blocking findings count:** 0

**Top gaps preventing PASS:**

1. None.

**Recommended follow-up verification steps:**

1. Regenerate the post-processed Cobertura report end-to-end (full `Invoke-MSTestWithCoverage.ps1` run) to confirm the production-only headline rate matches the ~58.95% figure cited in `issue.md`. This is confirmatory only; the unit-level behavior is verified.
2. Optionally reconcile the analyzer-finding function attribution in the prior QA-gate evidence note and address the pre-existing `PSUseSingularNouns` finding in a separate change.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

All six AC items in `issue.md` were already checked (`- [x]`) by the implementing agent before this review and are confirmed PASS here. No checkbox state change was required by this audit.

### AC Status Summary

- Source: `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/issue.md`
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `issue.md` | 6 | 6 | 0 | Checkbox-backed; all PASS; pre-checked by implementer, confirmed by this review. |
