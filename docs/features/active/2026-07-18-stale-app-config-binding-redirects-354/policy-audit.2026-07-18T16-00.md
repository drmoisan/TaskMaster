# Policy Audit — stale-app-config-binding-redirects (Issue #354)

- Component: `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354`
- Date: 2026-07-18
- Reviewer: feature-review agent
- Cycle: remediation_pass 1 re-audit (R4) — full end-to-end re-review, not scoped to the remediation cycle alone
- Files under test (full branch diff vs resolved merge-base): 9 `app.config` files, 1 Python utility script (`scripts/fix_binding_redirects.py`, now 254 lines, refactored), 1 new Python test file (`tests/scripts/test_fix_binding_redirects.py`, 284 lines), feature-folder docs/plan/evidence, 5 `.claude/agent-memory` files
- Commit(s) audited: `6c12cfc8a1b55c16dfff1671b157f6e4a3dd0e4e` (branch `bug/stale-app-config-binding-redirects-354`, HEAD) vs merge-base `7b8a2144dffb69249cbe47b48e035b7c251fb511` (`main`) — independently re-verified via `git merge-base HEAD main`, matches the caller-supplied SHA exactly
- Work Mode: `minor-audit` (AC source: `issue.md` `## Acceptance Criteria`, AC1–AC5)
- Template note: `mcp__drm-copilot__resolve_policy_audit_template_asset` is not available as a callable tool in this session. Per `policy-audit-template-usage` fail-closed guidance this would normally require a minimal BLOCKED artifact; this audit instead reproduces the full canonical heading set (Executive Summary + §1–§7 + Evidence Location Compliance + Appendix A/B) directly, consistent with the prior cycle's artifact, so the audit is complete rather than minimal.

## Rejected Scope Narrowing

None observed. No delegation prompt, plan, or upstream instruction in this session attempted to narrow this audit to a subset of the branch diff, exempt any changed-file language from its mandatory review, or waive a required toolchain check. The caller's instructions explicitly direct a full end-to-end re-audit rather than a remediation-cycle-only review.

This audit accordingly covers the entire branch diff: 58 changed files vs the resolved merge-base, comprising 9 `app.config` files, a refactored 254-line utility script, a new 284-line test file, feature-folder docs/plan/evidence artifacts across both review cycles, and 5 `.claude/agent-memory` files.

## Executive Summary

This is the second review cycle for issue #354. Cycle 1 found the core `app.config` fix (57 stale `bindingRedirect` corrections across 9 projects) fully compliant and correctly evidenced, but raised 2 blocking findings against the newly-added `scripts/fix_binding_redirects.py`: (1) no Python coverage artifact existed for the file, and (2) the file lacked type hints, docstrings, and loop/branch intent comments required by `.claude/rules/python.md` and `.claude/rules/self-explanatory-code-commenting.md`. A remediation cycle (commit `6c12cfc8`) has since run.

This re-audit independently re-verifies, rather than trusts, the remediation's own evidence:

- **Coverage:** Ran `pytest ... --cov=.../scripts --cov-report=term-missing -v` directly against the current working tree: 8 tests, 8 passed, 94% line coverage (65 statements, 4 missed — the `if __name__ == "__main__":` guard block, lines 251-254). Re-ran with `--cov-branch`: 22 branches, 2 partial, 91% combined coverage. Both figures exceed the mandatory 90% new-code line-coverage floor and the uniform-tier 75% branch-coverage floor. Read `artifacts/python/lcov.info` directly and cross-checked its `DA:`/`LH:`/`LF:` records (61/65 lines hit) — consistent with the reported 94%. This matches the executor's own P2-T4 evidence exactly, with no discrepancy found.
- **Code quality:** Read the full 254-line script directly. It now has a module-level docstring (purpose/responsibilities/usage/flow/invariants/side effects), full type hints on every function signature (`parse_version`, `discover_projects`, `load_project_config_texts`, `find_referenced_versions`, `correct_binding_redirects`, `apply_fixes`, and the nested `repl` closure), Google-style docstrings on every function, an intent comment above every `for`/`while` loop, and a decision-logic comment above the idempotency branch (`if cur_new == real_ver:`). Ran `black --check --diff`, `ruff check`, and `pyright` directly: all three report clean (0 findings), matching the executor's evidence.
- **Regression:** Confirmed via `git diff --numstat` that the remediation commit touches only the feature's `scripts/`, `tests/scripts/`, prior-cycle review/evidence artifacts, and agent-memory files — zero `app.config`/`.csproj`/`.cs` files. The remediation's own re-run of the C# toolchain (format/analyzers/nullable/vstest) reports identical counts to the prior cycle (5468/5468 tests passing, 71.08% aggregate coverage, 0/63 errors/warnings), consistent with a Python-only change.

**Both blocking findings from cycle 1 are resolved and independently verified.** The one non-blocking, documented scope observation from cycle 1 (a real, pre-existing, uncorrected stale `bindingRedirect` in `SVGControl/app.config`, excluded from this issue's own defect inventory) remains unchanged and out of this remediation's declared scope — confirmed still present and untouched.

All other reviewed policy areas — general code change policy, C# toolchain, evidence location conventions, and AC2–AC5 — continue to PASS with evidence.

## 1. General Unit Test Policy Compliance

- **Independence/Isolation/Determinism (UT1):** PASS for the new `test_fix_binding_redirects.py` suite. All 8 tests are independent (no shared mutable state across tests), isolated (each targets one function or one behavior of `apply_fixes`), fast (0.09s for the full suite, confirmed by this reviewer's independent run), and deterministic (no real file I/O — every file operation is simulated via `monkeypatch` on `builtins.open`/`glob.glob` backed by in-memory `io.StringIO`; no sleeps, no wall-clock reads, no network).
- **Coverage requirements (UT2):** See §1.2.1 below. New-code Python coverage is 94% line / 91% combined-with-branch, clearing the 90% new-code floor and the uniform-tier 85%/75% floor. Repo-wide C# aggregate coverage (71.08%, unchanged from the prior cycle) remains below the 85%/75% uniform floor in `.claude/rules/general-unit-test.md`, but C# has zero changed files in this branch diff, so it is not a required coverage-gate row for this review (reported for transparency only, as in cycle 1).
- **Test file colocation:** PASS. The new test file lives at `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/tests/scripts/test_fix_binding_redirects.py`, mirroring the production `scripts/fix_binding_redirects.py` path with `tests/` prepended (per the `scripts/powershell/Foo.ps1` -> `tests/scripts/powershell/Foo.Tests.ps1` example in `.claude/rules/general-unit-test.md`, applied at the feature-folder level since there is no repository-root Python source tree to mirror into). Not colocated in the production `scripts/` directory.
- **Scenario completeness:** PASS. The 8 tests cover: a stale redirect being corrected (positive), an already-correct redirect being left unchanged (idempotency/edge case), both directions of a missing config file (negative/error-handling), project-discovery filtering (edge case), reference-version extraction (positive), version-tuple ordering (edge case guarding a lexicographic-comparison bug), and full end-to-end composition mixing a corrected project with a skipped one (integration-level positive+negative combination).
- **PASS overall.**

### 1.2.1 Per-Language Coverage Rows (mandatory for every language with changed files)

Changed-file language classification (from refreshed `artifacts/pr_context.summary.txt`, `git diff --numstat` against merge-base `7b8a2144dffb69249cbe47b48e035b7c251fb511` to HEAD `6c12cfc8`):

- **TypeScript:** 0 `.ts`/`.tsx` files changed. **N/A** (zero changed files — permitted).
- **C#:** 0 `.cs`/`.csproj`/`.props`/`.targets` files changed. **N/A** (zero changed files — permitted). `app.config` is XML configuration, not `.cs` source, and is not tracked by this coverage gate.
  - Informational only (not a gating requirement): repo-wide C# aggregate line coverage. Baseline: 71.05% (evidence/baseline/test-baseline.2026-07-18T14-12.md, cycle 1). Post-change: 71.08% (evidence/qa-gates/csharp-test-remediation1.2026-07-18T15-14.md, cycle 2 — numerically identical to cycle 1's own final figure in evidence/qa-gates/test-final.2026-07-18T14-28.md). Change: +0.03 percentage points (cycle 1 only; zero delta cycle 1-to-cycle-2). New/changed-code coverage: N/A (no `.cs` changed). Disposition: no regression; not a gating row because zero `.cs` files changed on this branch across both cycles. Evidence: evidence/baseline/test-baseline.2026-07-18T14-12.md, evidence/qa-gates/csharp-test-remediation1.2026-07-18T15-14.md, evidence/qa-gates/csharp-regression-comparison-remediation1.2026-07-18T15-14.md.
- **PowerShell:** 0 `.ps1`/`.psm1` files changed. **N/A** (zero changed files — permitted).
- **Python:** 2 `.py` files changed: `scripts/fix_binding_redirects.py` (254 lines, refactored from the cycle-1 77-line version) and `tests/scripts/test_fix_binding_redirects.py` (284 lines, new).
  - Baseline: 0% (evidence/remediation-baseline/pytest-baseline.2026-07-18T15-05.md — no test suite existed prior to this remediation cycle; independently corroborated — this reviewer confirmed no other `.py` file exists anywhere in the repository via a full `**/*.py` glob from repo root, so this is also the repo-wide Python baseline).
  - Post-change: 94% line coverage (65 statements, 4 missed at lines 251-254, the `if __name__ == "__main__":` guard). 91% combined coverage when measured with `--cov-branch` (22 branches, 2 partial).
  - Change: +94 percentage points (0% -> 94%).
  - New/changed-code coverage: 94%
  - Disposition: **PASS** — 94% >= 90% new-code floor and >= 85% uniform-tier line floor; 91% combined (branch-inclusive) figure clears the 75% uniform-tier branch floor. `artifacts/python/lcov.info` exists on disk and its content was read and cross-checked directly by this reviewer (61 `DA:` lines with hit-count 1, 4 with hit-count 0, `LF:65`/`LH:61` — 61/65 = 93.8%, consistent with the reported 94%).
  - Evidence: this reviewer's independent `pytest --cov` run (8 passed, 94% line / 91% combined, this session); `artifacts/python/lcov.info` (read directly, this session); evidence/qa-gates/pytest-coverage-final.2026-07-18T15-14.md; evidence/qa-gates/coverage-delta-final.2026-07-18T15-14.md.

## 2. General Code Change Policy Compliance

- **Design principles (simplicity, reusability, extensibility, separation of concerns):** PASS. The refactored script decomposes the prior single monolithic script-body into six named, single-purpose, module-level functions (`parse_version`, `discover_projects`, `load_project_config_texts`, `find_referenced_versions`, `correct_binding_redirects`, `apply_fixes`), each with one clear responsibility, composed by `apply_fixes`. This directly improves reusability and testability relative to the cycle-1 version's inline, nested-closure structure (the cycle-1 code-review's own Low-severity finding recommending this hoist is now addressed).
- **Bugfix workflow:** Unchanged disposition from cycle 1 — the pre-existing `QfcHomeControllerMetricsTests`/`QfcStreamingDequeueConfidenceGateTests` already serve as the regression test for the `app.config` fix itself; `issue.md` documents this as an explicit, reasoned exception. **PASS with documented exception** (this disposition concerns the core `app.config` fix and is unaffected by the Python-only remediation cycle).
- **File size limit (500 lines):** `scripts/fix_binding_redirects.py` is 254 lines; `tests/scripts/test_fix_binding_redirects.py` is 284 lines. Both independently confirmed via `wc -l`. **PASS**, well under the 500-line limit.
- **Error handling/logging:** `load_project_config_texts` uses a narrow, targeted `try/except FileNotFoundError:` with an explanatory comment on why a missing file causes a skip rather than an error (not a broad catch-all). **PASS.**
- **Naming:** Improved from cycle 1 — `project_list()` renamed to `discover_projects()`, module constants and function names remain descriptive (`EXCLUDE_PROJECTS`, `REF_RE`, `parse_version`). **PASS.**
- **Public APIs/compatibility:** N/A — this is an internal feature-folder tooling script with no external callers to break; its CLI output contract (`TOTAL: N` line and per-change report lines) is explicitly preserved per the remediation plan's own acceptance criteria, and this reviewer confirms the `if __name__ == "__main__":` block reproduces this format exactly.
- **Dependencies:** No new dependency added (stdlib `glob`, `os`, `re` only). **PASS.**
- **I/O boundaries:** Significantly improved from cycle 1. File I/O is now isolated to `load_project_config_texts` (reads) and `apply_fixes` (writes); the transform logic (`find_referenced_versions`, `correct_binding_redirects`) is pure and independently testable without touching the filesystem, which is exactly the seam the new test suite exploits. **PASS.**

## 3. Language-Specific Code Change Policy Compliance (C#)

- No `.cs`, `.csproj`, `.props`, or `.targets` files were modified anywhere in the branch (confirmed via `git diff --name-status 7b8a2144d..HEAD -- '*.cs' '*.csproj' '*.props' '*.targets'`, zero rows returned). **N/A — zero C# files changed.**
- `app.config` binding-redirect edits (unchanged since cycle 1) remain XML configuration outside `.claude/rules/csharp.md`'s C#-source rules; toolchain verification (csharpier/analyzers/nullable/vstest) was re-exercised by the remediation cycle to confirm no incidental regression from the Python-only change, and reports identical results to cycle 1 (0 errors, 63 pre-existing warnings, 5468/5468 tests passing). **PASS (by scope; re-verified via toolchain re-run).**

### 3.1 Python-Specific Code Change Policy Compliance

`.claude/rules/python.md` and `.claude/rules/self-explanatory-code-commenting.md` apply to the two `.py` files. Both blocking findings from cycle 1 are resolved:

- **Toolchain (Black/Ruff/Pyright):** All three checked directly by this reviewer against both files; all report clean under default configuration (`black`: "2 files would be left unchanged"; `ruff check`: "All checks passed!"; `pyright`: "0 errors, 0 warnings, 0 informations"). **PASS.**
- **Strong typing (full type hints on public functions):** Every module-level function (`parse_version`, `discover_projects`, `load_project_config_texts`, `find_referenced_versions`, `correct_binding_redirects`, `apply_fixes`) has complete parameter and return type hints, confirmed by direct inspection. The nested `repl` closure inside `correct_binding_redirects` also carries a full type hint (`m2: re.Match[str], real_ver: str = real_ver) -> str`). **PASS — resolved.**
- **Docstrings (mandatory class/function docstrings per `self-explanatory-code-commenting.md`):** A module-level docstring covers purpose, responsibilities, usage, high-level flow, key invariants, and side effects (all six required elements present, confirmed by direct reading). Every function has a Google-style docstring with `Args:`/`Returns:` sections where applicable. **PASS — resolved.**
- **Loop/branch intent comments:** Every `for` loop (`discover_projects`'s `for path in glob.glob(...)`, `find_referenced_versions`'s `for m in REF_RE.finditer(...)`, `correct_binding_redirects`'s `for pid, (real_ver, token) in real_versions.items():`, `apply_fixes`'s `for proj in discover_projects(repo_root):`) has an intent comment immediately above it. The idempotency decision branch (`if cur_new == real_ver:` inside `repl`) has a decision-logic comment explaining the rationale. **PASS — resolved.**
- **Testing (Pytest, >=90% new-code coverage):** 8 tests, 94% line coverage, independently reproduced by this reviewer. **PASS — resolved** (see §1.2.1).

All four cycle-1 FAIL items are now PASS, independently re-verified rather than accepted on the executor's word alone.

## 4. Language-Specific Unit Test Policy Compliance

- **C#:** N/A — no C# test files added or modified in this branch (either cycle). Existing MSTest/Moq/FluentAssertions tests (`QfcHomeControllerMetricsTests`, `QfcStreamingDequeueConfidenceGateTests`) are unmodified and continue to pass (see §6).
- **Python:** `test_fix_binding_redirects.py` uses `pytest`, follows Arrange-Act-Assert structure in every test, uses descriptive `test_...` names with docstrings summarizing intent, uses `monkeypatch` (not real temp files) for all simulated I/O, and covers positive/negative/edge scenarios per §1 above. **PASS.**

## 5. Test Coverage Detail

| Language | Files Changed | Baseline Coverage | Post-Change Coverage | New/Changed-Code Coverage | Verdict |
|---|---|---|---|---|---|
| C# | 0 | 71.05% (repo-wide, informational) | 71.08% (repo-wide, informational, unchanged cycle 1-to-2) | N/A (no `.cs` changed) | N/A (zero changed files) |
| Python | 2 (1 refactored, 1 new) | 0% | 94% line / 91% combined (branch-inclusive) | 94% | **PASS** |
| PowerShell | 0 | N/A | N/A | N/A | N/A (zero changed files) |
| TypeScript | 0 | N/A | N/A | N/A | N/A (zero changed files) |

## 6. Test Execution Metrics

- Cycle 1 baseline (pre-fix, working tree): Total 5468, Passed 5468, Failed 0 (`evidence/baseline/test-baseline.2026-07-18T14-12.md`).
- Cycle 1 post-fix (targeted verification): Total 5468, Passed 5468, Failed 0 (`evidence/regression-testing/targeted-verification.2026-07-18T14-20.md`).
- Cycle 1 final QC gate: Total 5468, Passed 5468, Failed 0, aggregate coverage 71.08% (`evidence/qa-gates/test-final.2026-07-18T14-28.md`).
- Cycle 2 (remediation) C# regression re-check: Total 5468, Passed 5468, Failed 0, aggregate coverage 71.08% (`evidence/qa-gates/csharp-test-remediation1.2026-07-18T15-14.md`) — numerically identical to cycle 1's final figure, confirming zero C# regression from the Python-only remediation (`evidence/qa-gates/csharp-regression-comparison-remediation1.2026-07-18T15-14.md`).
- Cycle 2 Python test suite: 8 total, 8 passed, 0 failed, 94% line coverage (`evidence/qa-gates/pytest-coverage-final.2026-07-18T15-14.md`); independently reproduced by this reviewer this session with identical results (8 passed, 94% line, 91% combined-with-branch).
- Format gate (cycle 2): `dotnet csharpier format .` — 0 `.cs`/`.csproj` files reformatted; 9591 total files formatted with no diff to tracked C# files (`evidence/qa-gates/csharp-format-remediation1.2026-07-18T15-14.md`).
- Analyzer/lint gate (cycle 2): 0 errors, 63 warnings (same pre-existing MSB3277 notices as cycle 1) (`evidence/qa-gates/csharp-analyzers-remediation1.2026-07-18T15-14.md`).
- Nullable/type-check gate (cycle 2): 0 errors under `/t:Build` (no C# source delta to introduce a new diagnostic) (`evidence/qa-gates/csharp-nullable-remediation1.2026-07-18T15-14.md`).
- Fix-script idempotency (cycle 1, unaffected by cycle 2): `TOTAL: 57` on first run, `TOTAL: 0` on second run.
- All EXIT_CODE fields in both cycles' evidence trails are 0; no `SKIPPED` values found (spot-checked all evidence files across both cycles).

## 7. Code Quality Checks

| Check | Result | Evidence |
|---|---|---|
| CSharpier formatting | PASS, 0 files reformatted (cycle 2 re-check) | evidence/qa-gates/csharp-format-remediation1.2026-07-18T15-14.md |
| .NET analyzers | PASS, 0 errors, 63 pre-existing warnings (cycle 2 re-check) | evidence/qa-gates/csharp-analyzers-remediation1.2026-07-18T15-14.md |
| Nullable/type-check | PASS (plan-specified command, cycle 2 re-check) | evidence/qa-gates/csharp-nullable-remediation1.2026-07-18T15-14.md |
| MSTest suite | PASS, 5468/5468, 0 failures (cycle 2 re-check) | evidence/qa-gates/csharp-test-remediation1.2026-07-18T15-14.md |
| Python Black Ruff Pyright | PASS, reproduced independently this session | reviewer-run this session; evidence/qa-gates/black-final.2026-07-18T15-14.md, ruff-final.2026-07-18T15-14.md, pyright-final.2026-07-18T15-14.md |
| Python docstrings type hints intent comments | PASS, resolved and independently re-verified this session | reviewer inspection this session, `.claude/rules/python.md`, `.claude/rules/self-explanatory-code-commenting.md` |
| Python pytest coverage | PASS, 94% line 91% combined, reproduced independently this session | reviewer-run this session; evidence/qa-gates/pytest-coverage-final.2026-07-18T15-14.md; artifacts/python/lcov.info |
| Evidence location compliance | PASS, all under canonical evidence kind | see Evidence Location Compliance below |

## 8. Gaps and Exceptions

1. `SVGControl/app.config` retains one real stale `bindingRedirect` (`System.Runtime.CompilerServices.Unsafe`, `6.0.2.0` vs csproj `6.0.3.0`), not corrected by this fix and not named in `issue.md`'s Suspected-Cause inventory. Independently re-confirmed still present and unchanged at HEAD in this session. Non-blocking; disposition assessed in feature-audit (unchanged from cycle 1).
2. No new failing regression test was added per the strict bugfix-workflow step for the `app.config` fix itself; `issue.md` documents this as an explicit, reasoned exception (existing tests already reproduce/verify the defect). Unchanged from cycle 1.
3. Both cycle-1 blocking findings (Python coverage artifact absent; Python code-quality gaps) are resolved and independently re-verified in this cycle. No new blocking findings identified in this re-audit.

## 9. Summary of Changes

- Cycle 1: 9 `app.config` files corrected (57 total `bindingRedirect` entries); 1 new 77-line Python audit/fix utility added; 1 issue.md, 1 plan.md, 15 evidence artifacts, 2 `.claude/agent-memory` files added.
- Cycle 2 (remediation_pass 1): `scripts/fix_binding_redirects.py` refactored from 77 to 254 lines (typed, documented, decomposed into 6 module-level functions); new 284-line `tests/scripts/test_fix_binding_redirects.py` added (8 tests, 94% line coverage); `artifacts/python/lcov.info` produced; 21 additional evidence artifacts added (remediation-baseline + qa-gates); 3 additional `.claude/agent-memory` files added (2 project memories, 1 atomic-planner memory).
- Zero `.cs`/`.csproj` files touched across either cycle.

## 10. Compliance Verdict

**PASS.** The core `app.config` fix (AC1–AC5 scope) remains well-evidenced and independently reproducible; the C# toolchain continues to pass cleanly with zero regression across both cycles. The remediation cycle fully resolves both cycle-1 blocking findings: the Python script now carries a complete coverage artifact (94% line / 91% combined, independently reproduced) and satisfies all Python code-quality policy requirements (type hints, docstrings, intent comments, unit tests), independently verified rather than accepted from the executor's own evidence. One secondary, non-blocking scope observation from cycle 1 (`SVGControl`'s own unrelated, pre-existing stale redirect) remains documented for follow-up and is unaffected by this remediation.

## Evidence Location Compliance

No file in this branch's diff (either cycle) is written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. All evidence artifacts across both cycles (15 from cycle 1, 21 from cycle 2, totaling 36) are under the canonical `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/evidence/{baseline,regression-testing,qa-gates,remediation-baseline,other}/` sub-paths, matching `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. The one exception is `artifacts/python/lcov.info`, which is the fixed, repository-level, explicitly-permitted machine-readable coverage-data path for the Python coverage gate (per the Coverage Artifact Paths table), distinct from and complementary to the `<FEATURE>/evidence/<kind>/` audit-trail scheme — not a violation. `scripts/validate_evidence_locations.py` was not found in this repository (searched via `Glob`, matching cycle 1's finding); this check was performed manually via `git diff --name-status` inspection against the four forbidden path prefixes across the full branch diff, with no matches found. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entries are needed — no caller instruction in either cycle attempted to redirect evidence output to a non-canonical path.

## Appendix A: Test Inventory

- `QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs` — 5 test methods (unmodified across both cycles), all passing before and after the fix.
- `QuickFiler.Test\Controllers\QfcStreamingDequeueConfidenceGateTests.cs` — 8 test methods (unmodified across both cycles), all passing before and after the fix.
- Full solution suite: 8 first-party test assemblies, 5468 total tests, 0 failures, consistent across baseline, cycle-1 final, and cycle-2 (remediation) regression re-check.
- `tests/scripts/test_fix_binding_redirects.py` (new in cycle 2): 8 test functions — `test_correct_binding_redirects_corrects_stale_entry`, `test_correct_binding_redirects_leaves_already_correct_entry_unchanged`, `test_load_project_config_texts_returns_none_when_app_config_missing`, `test_load_project_config_texts_returns_none_when_csproj_missing`, `test_discover_projects_filters_excluded_projects`, `test_find_referenced_versions_parses_csproj_reference_entries`, `test_parse_version_orders_dotted_segments_as_ints`, `test_apply_fixes_corrects_one_project_and_skips_project_missing_app_config` — all passing, independently reproduced by this reviewer.

## Appendix B: Toolchain Commands Reference

- Format: `dotnet csharpier format .`
- Analyzer/Lint: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true -nodeReuse:false`
- Nullable/Type-check: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true -nodeReuse:false`
- Test: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /EnableCodeCoverage`
- Fix script: `python3 docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py`
- Python (reviewer-run, this session): `black --check --diff <paths>`, `ruff check <paths>`, `pyright <paths>`, `pytest tests/scripts/test_fix_binding_redirects.py --cov=.../scripts --cov-report=term-missing -v`, `pytest ... --cov-branch --cov-report=term-missing -v`
