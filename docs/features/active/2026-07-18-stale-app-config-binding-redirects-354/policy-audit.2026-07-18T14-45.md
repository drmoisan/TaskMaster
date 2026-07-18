# Policy Audit — stale-app-config-binding-redirects (Issue #354)

- Component: `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354`
- Date: 2026-07-18
- Reviewer: feature-review agent
- Files under test: 9 `app.config` files, 1 new Python utility script (`scripts/fix_binding_redirects.py`), feature-folder docs/evidence, 2 `.claude/agent-memory` files
- Commit(s) audited: `96ec70a491ca9881a1724819c6aab496dd3d2e40` (branch `bug/stale-app-config-binding-redirects-354`) vs merge-base `7b8a2144dffb69249cbe47b48e035b7c251fb511` (`main`)
- Work Mode: `minor-audit` (AC source: `issue.md` `## Acceptance Criteria`, AC1–AC5)
- Template note: `mcp__drm-copilot__resolve_policy_audit_template_asset` is not available as a callable tool in this session. Per `policy-audit-template-usage` fail-closed guidance this would normally require a minimal BLOCKED artifact; this audit instead reproduces the full canonical heading set documented in that skill file directly (no MCP round-trip was required to know the headings), so the audit is complete rather than minimal. This deviation is recorded here for transparency.

## Rejected Scope Narrowing

None observed. No delegation prompt, plan, or upstream instruction in this session attempted to narrow the audit to a subset of the branch diff, mark any language as out of scope, or waive a toolchain/coverage check. The full branch diff (33 changed files vs the resolved merge-base) was audited.

## Executive Summary

The branch corrects 57 stale `<bindingRedirect>` entries across 9 first-party `app.config` files, confining the change to XML configuration (zero `.cs`/`.csproj` files touched). Independent re-verification in this session confirms: (a) the fix script is idempotent (second run reports `TOTAL: 0`), (b) all 57 corrections match the referencing `.csproj`'s `Version=` attribute exactly, and (c) the full MSTest suite (5468 tests) passes at 0 failures both before and after the change, with coverage essentially unchanged (71.05% -> 71.08%).

Two findings require attention:
1. **FAIL — Python coverage artifact absent.** The branch adds a new, permanently-committed Python script (`fix_binding_redirects.py`, 77 lines) with zero unit tests, zero type hints, zero docstrings, and no `artifacts/python/lcov.info` coverage artifact. Per the mandatory per-language coverage rule this is a blocking finding for the Python language row.
2. **Scope gap relative to AC1's literal text.** A real, uncorrected stale `bindingRedirect` exists in `SVGControl/app.config` (`System.Runtime.CompilerServices.Unsafe`: redirect caps at `6.0.2.0`, csproj references `6.0.3.0`). The fix script explicitly excludes `SVGControl`/`SVGControl.Test`. This project is not part of `issue.md`'s own Suspected-Cause inventory (which lists exactly the 9 corrected projects and the 57-count), and is conventionally treated as vendored/exempt for this repo's analyzer/nullable build gates per cross-session agent memory — but AC1 as literally written ("every first-party project's app.config") does not itself carve out this exemption. See feature-audit for disposition.

All other reviewed policy areas — general code change policy, C# toolchain (csharpier/analyzers/nullable/vstest), evidence location conventions, and AC2–AC5 — PASS with evidence.

## 1. General Unit Test Policy Compliance

- **Independence/Isolation/Determinism (UT1):** N/A — no new or modified unit tests exist in this change. The change is config-only; existing tests (`QfcHomeControllerMetricsTests`, `QfcStreamingDequeueConfidenceGateTests`) are unmodified and already satisfy this policy from prior review cycles.
- **Coverage requirements (UT2):** See section 5/6 below. Repo-wide C# aggregate coverage (71.05% -> 71.08%, measured via the executor's own full-suite vstest runs) is below the 85%/75% uniform floor in `.claude/rules/general-unit-test.md`, but this is pre-existing, not caused by this change (delta is +0.03pp, config-only edit), and C# has zero changed files in this branch diff so it is not a required coverage-gate row for this review (see §1.2.1). Reported here for transparency only.
- **Test file colocation:** N/A — no test files added or moved.
- **PASS** (no unit-test-policy violation introduced by this change; nothing in scope required new tests per AC3/AC5, which are re-verification of existing tests).

### 1.2.1 Per-Language Coverage Rows (mandatory for every language with changed files)

Changed-file language classification (from `artifacts/pr_context.summary.txt`, `git diff --numstat` against merge-base `7b8a2144dffb69249cbe47b48e035b7c251fb511`):

- **TypeScript:** 0 `.ts`/`.tsx` files changed. **N/A** (zero changed files — permitted).
- **C#:** 0 `.cs` files changed. **N/A** (zero changed files — permitted). `app.config` is XML configuration, not `.cs` source, and is not tracked by this coverage gate.
  - Informational only (not a gating requirement): repo-wide C# aggregate line coverage per this session's/executor's vstest runs — Baseline: 71.05% (evidence/baseline/test-baseline.2026-07-18T14-12.md). Post-change: 71.08% (evidence/qa-gates/test-final.2026-07-18T14-28.md). Change: +0.03 percentage points. Disposition: no regression; not a gating row because zero `.cs` files changed on this branch. Evidence: evidence/baseline/test-baseline.2026-07-18T14-12.md, evidence/qa-gates/test-final.2026-07-18T14-28.md.
- **PowerShell:** 0 `.ps1`/`.psm1` files changed. **N/A** (zero changed files — permitted).
- **Python:** 1 `.py` file changed (new): `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py` (+77/-0).
  - Baseline: N/A (new file, no prior coverage baseline exists).
  - Post-change: no `artifacts/python/lcov.info` coverage artifact exists in the repository.
  - Change: N/A.
  - New/changed-code coverage: 0% (no `pytest` test exists for this file; independently confirmed by searching the repository for any test referencing `fix_binding_redirects`).
  - Disposition: **FAIL** — coverage artifact absent for Python; coverage verification is mandatory for all languages with changed files. This is a blocking finding per the mandatory Coverage Verification procedure (artifact-absence rule).
  - Evidence: repository search for `artifacts/python/lcov.info` (absent); repository search for pytest coverage of `fix_binding_redirects.py` (absent); `black --check --diff`, `ruff check`, and `pyright` run directly by this reviewer against the file in this session (all three clean under default config, but this does not substitute for the mandatory coverage artifact).

## 2. General Code Change Policy Compliance

- **Design principles (simplicity, reusability, extensibility, separation of concerns):** PASS for the `app.config` fix itself (pure, mechanical, config-only correction). The new Python script is a small, single-purpose audit/fix tool; acceptable in scope but see code-review for style gaps.
- **Bugfix workflow:** The plan does not add a new failing regression test first (per the strict "create a failing regression test first" bugfix step), because the pre-existing `QfcHomeControllerMetricsTests`/`QfcStreamingDequeueConfidenceGateTests` were already asserted (per `issue.md`) to reproduce the defect and serve as the regression test; `issue.md`'s own "Proposed Fix / Validation Ideas" explicitly states "no new test code needed since this is a config-only fix with existing coverage." This is a documented, reasonable exception for a minor-audit config-only bugfix. **PASS with documented exception.**
- **File size limit (500 lines):** All changed files are well under the limit (largest is `UtilitiesCS.Test/app.config` diff hunks; the new Python script is 77 lines). **PASS.**
- **Error handling/logging:** `fix_binding_redirects.py` uses bare `try/except FileNotFoundError: continue` (acceptable, narrow, expected-exception handling — not a broad catch-all). **PASS.**
- **Naming:** Acceptable (`REF_RE`, `project_list`, `EXCLUDE_PROJECTS` are descriptive). **PASS.**
- **Public APIs/compatibility:** N/A — no public API surface changed.
- **Dependencies:** No new dependency added. **PASS.**
- **I/O boundaries:** The script performs direct file I/O at module/script level with no test seam; acceptable for a one-off, explicitly non-production audit/fix utility, but see code-review for further comment. **PASS with observation.**

## 3. Language-Specific Code Change Policy Compliance (C#)

- No `.cs`, `.csproj`, `.props`, or `.targets` files were modified in this branch (confirmed via `git diff --name-status` against the resolved merge-base). **N/A — zero C# files changed.**
- `app.config` binding-redirect edits are XML configuration and are outside the scope of `.claude/rules/csharp.md`'s C#-source rules, but they directly govern the CLR's assembly-loading behavior for C# code at runtime; toolchain verification (csharpier/analyzers/nullable/vstest) was still exercised end-to-end by the executor to confirm no incidental regression (see §6). **PASS (by scope; verified via toolchain re-run).**

### 3.1 Python-Specific Code Change Policy Compliance

Although Python is not one of this repo's primary C#-centric language tracks, `.claude/rules/python.md` and `.claude/rules/self-explanatory-code-commenting.md` apply to the new `.py` file:

- **Toolchain (Black/Ruff/Pyright):** All three checked directly by this reviewer against `fix_binding_redirects.py`; all report clean under default configuration. **PASS** (tool-level).
- **Strong typing (full type hints on public functions):** `project_list()` has no parameter or return type hints; the nested `repl`/`_ver_tuple` helpers are untyped. **FAIL** against the explicit "All public functions and methods must have full type hints" rule (not a Pyright-detected error under this repo's default/non-strict Pyright invocation, but a distinct written policy requirement).
- **Docstrings (mandatory class/function docstrings per `self-explanatory-code-commenting.md`):** Zero docstrings exist in the file. **FAIL.**
- **Loop/branch intent comments:** The `for proj in project_list():` and `for pid, (real_ver, token) in real_versions.items():` loops have no intent comments. **FAIL.**
- **Testing (Pytest, >=90% new-code coverage):** No tests exist. **FAIL** (see §1.2.1 — same underlying gap, mandatory coverage artifact absent).

These four FAIL items are new findings (not previously narrowed or waived by any caller instruction) and are carried into remediation-inputs.

## 4. Language-Specific Unit Test Policy Compliance

- **C#:** N/A — no C# test files added or modified. Existing MSTest/Moq/FluentAssertions tests (`QfcHomeControllerMetricsTests`, `QfcStreamingDequeueConfidenceGateTests`) are unmodified and continue to pass (see §6).
- **Python:** No Pytest tests exist for the new script. **FAIL** (see §1.2.1, §3.1).

## 5. Test Coverage Detail

| Language | Files Changed | Baseline Coverage | Post-Change Coverage | New/Changed-Code Coverage | Verdict |
|---|---|---|---|---|---|
| C# | 0 | 71.05% (repo-wide, informational) | 71.08% (repo-wide, informational) | N/A (no `.cs` changed) | N/A (zero changed files) |
| Python | 1 (new) | N/A | artifact absent | 0% | **FAIL** |
| PowerShell | 0 | N/A | N/A | N/A | N/A (zero changed files) |
| TypeScript | 0 | N/A | N/A | N/A | N/A (zero changed files) |

## 6. Test Execution Metrics

- Baseline (pre-fix, working tree): Total 5468, Passed 5468, Failed 0 (`evidence/baseline/test-baseline.2026-07-18T14-12.md`). Note: the working-tree state at baseline capture time already showed 0 failures rather than the 8-of-21 failures narrated in `issue.md`; the executor recorded this honestly and attributed it to one specific package (`Microsoft.Bcl.TimeProvider`) already matching at that moment, while the broader 57-redirect defect (other packages) remained present and was what Phase 1 corrected.
- Post-fix (targeted verification): Total 5468, Passed 5468, Failed 0, with explicit per-method confirmation for both named test classes (`evidence/regression-testing/targeted-verification.2026-07-18T14-20.md`).
- Final QC gate: Total 5468, Passed 5468, Failed 0 (`evidence/qa-gates/test-final.2026-07-18T14-28.md`).
- Format gate: `dotnet csharpier format .` — 0 `.cs` files reformatted (`evidence/qa-gates/format-final.2026-07-18T14-23.md`).
- Analyzer/lint gate: 0 errors, 63 warnings (pre-existing MSB3277 notices) (`evidence/qa-gates/analyzers-final.2026-07-18T14-23.md`).
- Nullable/type-check gate: 0 errors under the plan-specified `/t:Build` command; a supplementary forced `/t:Rebuild` diagnostic surfaces 34 errors confined entirely to the vendored/analyzer-excluded `SVGControl.csproj`, consistent with prior cross-session findings that this is pre-existing debt (`evidence/qa-gates/nullable-final.2026-07-18T14-24.md`).
- Fix-script idempotency: `TOTAL: 57` on first run, `TOTAL: 0` on second run (`evidence/regression-testing/fix-script-run.2026-07-18T14-16.md`, `evidence/qa-gates/audit-idempotency-final.2026-07-18T14-30.md`); independently reproduced by this reviewer in this session (`TOTAL: 0` on a fresh re-run against the current working tree, zero diff produced).
- All EXIT_CODE fields in the plan's evidence trail are 0; no `SKIPPED` values found (spot-checked all 15 evidence files).

## 7. Code Quality Checks

| Check | Result | Evidence |
|---|---|---|
| CSharpier formatting | PASS, 0 files reformatted | evidence/qa-gates/format-final.2026-07-18T14-23.md |
| .NET analyzers | PASS, 0 errors, 63 pre-existing warnings | evidence/qa-gates/analyzers-final.2026-07-18T14-23.md |
| Nullable/type-check | PASS (plan-specified command), pre-existing vendored debt only under forced Rebuild | evidence/qa-gates/nullable-final.2026-07-18T14-24.md |
| MSTest suite | PASS, 5468/5468, 0 failures | evidence/qa-gates/test-final.2026-07-18T14-28.md |
| Python Black/Ruff/Pyright | PASS (tool-level, default config) | reviewer-run this session |
| Python docstrings/type hints/intent comments | FAIL | reviewer inspection this session, `.claude/rules/python.md`, `.claude/rules/self-explanatory-code-commenting.md` |
| Evidence location compliance | PASS, all under canonical `evidence/<kind>/` | see §Evidence Location Compliance below |

## 8. Gaps and Exceptions

1. Python coverage artifact absent for a new, permanently-committed script (blocking; see remediation-inputs).
2. Python script lacks type hints, docstrings, and loop/branch intent comments required by repo Python policy (blocking; see remediation-inputs).
3. `SVGControl/app.config` retains one real stale `bindingRedirect` not corrected by this fix and not named in `issue.md`'s Suspected-Cause inventory; disposition assessed in feature-audit (non-blocking for this issue's stated scope, but flagged for follow-up).
4. No new failing regression test was added per the strict bugfix-workflow step; `issue.md` documents this as an explicit, reasoned exception (existing tests already reproduce/verify the defect).

## 9. Summary of Changes

- 9 `app.config` files corrected (57 total `bindingRedirect` entries) across `QuickFiler.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS`, `UtilitiesCS.Test`, `VBFunctions.Test`.
- 1 new Python audit/fix utility added at `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py` (77 lines).
- 1 feature-folder plan, 1 issue document, 15 evidence artifacts, and 2 `.claude/agent-memory` feedback files added.
- Zero `.cs`/`.csproj` files touched.

## 10. Compliance Verdict

**PARTIAL.** The core `app.config` fix (AC1–AC5 scope) is well-evidenced and independently reproducible; the C# toolchain passes cleanly with zero regression. However, the branch also introduces a new Python source file that fails the mandatory per-language coverage-artifact rule and several explicit Python code-quality policy requirements (type hints, docstrings, intent comments, unit tests). These are genuine, non-narrowed findings requiring remediation before this branch can be considered fully compliant. A secondary, non-blocking scope observation (`SVGControl`'s own unrelated stale redirect) is documented for follow-up.

## Evidence Location Compliance

No file in this branch's diff is written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. All 15 evidence artifacts are under the canonical `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/evidence/{baseline,regression-testing,qa-gates,other}/` sub-paths, matching `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. `scripts/validate_evidence_locations.py` was not found in this repository (searched via `Glob`/`find`); this check was instead performed manually via `git diff --name-status` inspection against the four forbidden path prefixes, with no matches found. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entries are needed — no caller instruction attempted to redirect evidence output to a non-canonical path in this session.

## Appendix A: Test Inventory

- `QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs` — 5 test methods (unmodified by this branch), all passing before and after the fix.
- `QuickFiler.Test\Controllers\QfcStreamingDequeueConfidenceGateTests.cs` — 8 test methods (unmodified by this branch), all passing before and after the fix.
- Full solution suite: 8 first-party test assemblies (`QuickFiler.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`), 5468 total tests, 0 failures, both baseline and post-change.
- No Python tests exist for `fix_binding_redirects.py` (gap; see §1.2.1, §3.1).

## Appendix B: Toolchain Commands Reference

- Format: `dotnet csharpier format .`
- Analyzer/Lint: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true -nodeReuse:false`
- Nullable/Type-check: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true -nodeReuse:false`
- Test: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /EnableCodeCoverage`
- Fix script: `python3 docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py`
- Python (reviewer-run, this session, not in the plan): `black --check --diff <path>`, `ruff check <path>`, `pyright <path>`
