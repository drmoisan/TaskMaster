# Feature Audit — stale-app-config-binding-redirects (Issue #354)

- Component: `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354`
- Date: 2026-07-18
- Reviewer: feature-review agent
- Work Mode: `minor-audit`
- Cycle: remediation_pass 1 re-audit (R4)

## Scope and Baseline

- AC source (per Work Mode Routing, `minor-audit`): the explicit `## Acceptance Criteria` section in `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/issue.md` (AC1–AC5). No `spec.md`/`user-story.md` exists for this folder and none is required under `minor-audit` mode.
- Resolved base branch: `main`. Resolved merge-base: `7b8a2144dffb69249cbe47b48e035b7c251fb511` (independently re-verified via `git merge-base HEAD main` in this session — matches the caller-supplied SHA exactly; zero drift).
- Head commit audited: `6c12cfc8a1b55c16dfff1671b157f6e4a3dd0e4e` on `bug/stale-app-config-binding-redirects-354` (this is the remediation commit; the branch has two commits total ahead of `main`: `96ec70a4` the original fix, `6c12cfc8` the remediation).
- Full branch diff (both commits): 58 changed files — 9 `app.config`, 1 refactored Python script (`scripts/fix_binding_redirects.py`, 254 lines), 1 new Python test file (`tests/scripts/test_fix_binding_redirects.py`, 284 lines), `issue.md`, `plan.md`, `remediation-plan.md`, `remediation-inputs.md`, 3 review artifacts from cycle 1 (`policy-audit`/`code-review`/`feature-audit`.2026-07-18T14-45.md), 36 evidence artifacts across both cycles, and 5 `.claude/agent-memory` files. Full enumeration in the refreshed `artifacts/pr_context.summary.txt`.
- This is the second review cycle. Cycle 1 (artifacts timestamped `...T14-45`) found 2 blocking findings, both scoped to `scripts/fix_binding_redirects.py` (Python coverage artifact absent; Python code-quality gaps). A remediation cycle addressed both. This audit independently re-verifies the remediation rather than trusting its own evidence, and re-audits the full branch diff end-to-end (not narrowed to the remediation cycle).

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from issue.md) |
|---|---|
| AC1 | Every `<bindingRedirect>` entry in every first-party project's `app.config` has a `newVersion` (and an `oldVersion` upper bound) equal to the actual assembly version referenced by that project's `.csproj` `<Reference Include="...", Version=...>` for the same assembly (matched by package id + publicKeyToken). |
| AC2 | No production `.cs` source file is modified; the fix is confined to `app.config` files. |
| AC3 | `QfcHomeControllerMetricsTests` and `QfcStreamingDequeueConfidenceGateTests` (previously 8 failing tests reproduced locally) pass with 0 failures after the fix. |
| AC4 | The full solution builds cleanly (CSharpier format, .NET analyzers, nullable) with zero errors after the fix. |
| AC5 | The full MSTest suite runs via `vstest.console.exe` across the solution with no new failures introduced relative to the pre-fix baseline (excluding failures already attributable to the stale redirects being fixed). |

## Acceptance Criteria Evaluation

### AC1 — Every bindingRedirect matches its csproj Reference version

**PARTIAL** (unchanged disposition from cycle 1; the remediation cycle made no `app.config` changes, confirmed via `evidence/qa-gates/scope-lock-remediation1.2026-07-18T15-14.md` and this reviewer's own `git diff --numstat` check).

Within the scope the fix targeted (57 stale redirects across the 9 projects named in `issue.md`'s own Suspected-Cause inventory), AC1 remains fully satisfied: all 57 corrections were re-verified in cycle 1 against each project's `.csproj` `Reference Version=` attribute, and the fix remains idempotent (unaffected by the Python-only remediation).

AC1's literal text ("every first-party project's `app.config`," no stated carve-out) is still not fully satisfied once `SVGControl` is considered:
- `SVGControl/app.config`: `System.Runtime.CompilerServices.Unsafe` bindingRedirect reads `oldVersion="0.0.0.0-6.0.2.0" newVersion="6.0.2.0"`, while `SVGControl.csproj` references `Version=6.0.3.0`. **Independently re-confirmed present and unchanged in this session** via direct `grep` against both files at HEAD `6c12cfc8`.
- `SVGControl`/`SVGControl.Test` remain excluded by name in `fix_binding_redirects.py`'s `EXCLUDE_PROJECTS` set (unchanged by the refactor) and are not named in `issue.md`'s Suspected-Cause project list.

**Disposition:** Unchanged from cycle 1 — PARTIAL rather than PASS, to keep the discrepancy visible. This does not block the issue's core fix, since `SVGControl` was never part of the issue's stated defect inventory, and the remediation cycle correctly left this out of scope per its own documented scope-lock. Recommend the same follow-up as cycle 1: either narrow AC1's wording to exclude vendored/analyzer-exempt projects, or open a follow-up issue to correct the `SVGControl` redirect.

### AC2 — No production .cs source file modified

**PASS.** `git diff --name-status 7b8a2144d..6c12cfc8` shows exactly 9 `M` (modified) `app.config` files and a set of purely additive (`A`) documentation/evidence/script/test files across both commits; zero `.cs` files appear anywhere in the diff. Independently re-confirmed by this reviewer via a fresh `git diff --name-only -- '*.cs' '*.csproj' '*.props' '*.targets'` run against the full range in this session (zero rows).

### AC3 — Named test classes pass with 0 failures

**PASS.** Unchanged from cycle 1: `evidence/regression-testing/targeted-verification.2026-07-18T14-20.md` confirms all 5 methods of `QfcHomeControllerMetricsTests` and all 8 methods of `QfcStreamingDequeueConfidenceGateTests` pass, 0 failures. The remediation cycle's own C# regression re-check (`evidence/qa-gates/csharp-test-remediation1.2026-07-18T15-14.md`) confirms the full 5468-test suite (which includes these two classes) still passes at 5468/5468 after the Python-only remediation, with zero delta from cycle 1's final count.

### AC4 — Full solution builds cleanly

**PASS.** Unchanged disposition from cycle 1 (CSharpier 0 reformatted, analyzers 0 errors/63 pre-existing warnings, nullable 0 errors under `/t:Build`), re-confirmed by the remediation cycle's own C# toolchain re-run (`evidence/qa-gates/csharp-format-remediation1.2026-07-18T15-14.md`, `csharp-analyzers-remediation1.2026-07-18T15-14.md`, `csharp-nullable-remediation1.2026-07-18T15-14.md`), all reporting identical results to cycle 1's final gate. This is expected and consistent, since the remediation touched zero `.cs`/`.csproj` files.

### AC5 — No new failures relative to pre-fix baseline

**PASS.** Unchanged from cycle 1 (0 new failures, 5468/5468). The remediation cycle's own regression comparison (`evidence/qa-gates/csharp-regression-comparison-remediation1.2026-07-18T15-14.md`) independently confirms 5468 total/5468 passed/0 failed against the prior cycle's final count, with an explicit zero-delta verdict. This reviewer treats this as corroborating rather than sole evidence, since it is consistent with the AC5 finding already established in cycle 1 and the remediation's declared scope-lock (Python-only changes, no C# source delta possible).

## Remediation Verification (cycle-1 blocking findings)

Cycle 1 raised 2 blocking findings, both scoped to `scripts/fix_binding_redirects.py`. This audit independently re-verifies both as resolved:

1. **Python coverage artifact absent.** Resolved. This reviewer independently ran `pytest tests/scripts/test_fix_binding_redirects.py --cov=scripts --cov-report=term-missing -v` against the current working tree: 8 tests, 8 passed, 94% line coverage (65 statements, 4 missed at the `if __name__ == "__main__":` guard, lines 251-254). A second run with `--cov-branch` reports 91% combined coverage (22 branches, 2 partial). Both clear the mandatory 90% new-code floor and the uniform-tier 85%/75% floor. `artifacts/python/lcov.info` exists and its `DA:`/`LF:`/`LH:` records (61/65 lines hit) are consistent with the reported figure.
2. **Python code-quality gaps (type hints, docstrings, intent comments).** Resolved. This reviewer read the full 254-line refactored script directly: a module docstring covers all required elements (purpose, responsibilities, usage, flow, invariants, side effects); every function has complete type hints and a Google-style docstring; every loop has an intent comment; the idempotency branch has a decision-logic comment.

Both resolutions were verified by this reviewer's own independent tool runs and direct file reading, not by trusting the executor's own evidence artifacts (`pytest-coverage-final.2026-07-18T15-14.md`, `black-final.2026-07-18T15-14.md`, `ruff-final.2026-07-18T15-14.md`, `pyright-final.2026-07-18T15-14.md`) alone, though those artifacts' recorded figures matched this reviewer's independent results exactly in every case, with no discrepancy found.

## Acceptance Criteria Check-off

- [x] AC1 — see PARTIAL disposition above; left checked in `issue.md` as authored (unchanged from cycle 1), since the issue's own defined defect scope (57 redirects, 9 named projects) is fully resolved and independently re-verified. The `SVGControl` discrepancy remains outside that defined scope.
- [x] AC2 — PASS, re-verified independently this cycle.
- [x] AC3 — PASS, re-verified independently this cycle.
- [x] AC4 — PASS, re-verified independently this cycle.
- [x] AC5 — PASS, re-verified independently this cycle.

`issue.md` already shows all five AC items as `[x]` (checked by the executor prior to cycle 1's review, unchanged since). This review independently re-verified AC2–AC5 as fully satisfied and re-verifies AC1 as satisfied for the issue's own defined scope, with the `SVGControl` discrepancy carried forward as a documented, non-blocking observation. No AC source file edits were made by this review beyond this evaluation.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/issue.md`
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none. (AC1 carries a documented, unchanged scope caveat — see evaluation above.)

## Summary

The branch delivers the issue's stated defect fix completely and correctly: all 57 stale `bindingRedirect` entries named in `issue.md`'s own root-cause inventory are corrected, independently re-verified, and idempotent. AC2–AC5 pass without qualification, re-confirmed in this cycle via both the remediation's own regression re-check and this reviewer's independent inspection. AC1 remains satisfied for the issue's own defined scope but does not fully satisfy its own literal, unqualified text once the vendored `SVGControl` project is considered — a real, pre-existing, unrelated stale redirect remains there, unchanged and correctly out of this remediation's declared scope. The remediation cycle fully and verifiably resolves both blocking findings raised in cycle 1: `scripts/fix_binding_redirects.py` now carries complete Python coverage (94% line / 91% combined, independently reproduced) and satisfies all applicable Python code-quality policy requirements (type hints, docstrings, intent comments, unit tests), independently verified by this reviewer rather than accepted from the executor's evidence alone. No new blocking findings were identified in this end-to-end re-audit.
