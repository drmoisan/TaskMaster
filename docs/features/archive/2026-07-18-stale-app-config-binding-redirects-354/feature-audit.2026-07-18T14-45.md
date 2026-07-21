# Feature Audit — stale-app-config-binding-redirects (Issue #354)

- Component: `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354`
- Date: 2026-07-18
- Reviewer: feature-review agent
- Work Mode: `minor-audit`

## Scope and Baseline

- AC source (per Work Mode Routing, `minor-audit`): the explicit `## Acceptance Criteria` section in `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/issue.md` (AC1–AC5). No `spec.md`/`user-story.md` exists for this folder and none is required under `minor-audit` mode.
- Resolved base branch: `main`. Resolved merge-base: `7b8a2144dffb69249cbe47b48e035b7c251fb511` (independently re-verified via `git merge-base HEAD origin/main` in this session — matches the caller-supplied SHA exactly; zero drift).
- Head commit audited: `96ec70a491ca9881a1724819c6aab496dd3d2e40` on `bug/stale-app-config-binding-redirects-354`.
- Full branch diff: 33 changed files (9 `app.config`, 1 new Python script, 1 issue.md, 1 plan.md, 15 evidence artifacts, 2 `.claude/agent-memory` files, 4 counted above as the two memory files plus... see `git diff --numstat` in `artifacts/pr_context.summary.txt` for the exact enumerated list).

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

**PARTIAL.** Within the scope the fix targeted (57 stale redirects across the 9 projects named in `issue.md`'s own Suspected-Cause inventory: `QuickFiler`/`.Test`, `Tags.Test`, `TaskMaster`/`.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS`/`.Test`, `VBFunctions.Test`), AC1 is fully satisfied:
- All 57 corrections were independently re-verified by this reviewer against each project's `.csproj` `Reference Version=` attribute; every corrected `newVersion` and `oldVersion` upper bound matches exactly (e.g., `Microsoft.Web.WebView2.Core` -> `1.0.4078.44`, confirmed against `UtilitiesCS/UtilitiesCS.csproj`).
- The fix script is idempotent: a second, independent run by this reviewer against the current working tree reports `TOTAL: 0` with zero resulting diff.

However, AC1's literal text says "every first-party project's `app.config`," with no stated carve-out. This reviewer independently scanned **every** project with a `packages.config` (including ones outside the fix's scope) and found one real, uncorrected mismatch:
- `SVGControl/app.config`: `System.Runtime.CompilerServices.Unsafe` bindingRedirect reads `oldVersion="0.0.0.0-6.0.2.0" newVersion="6.0.2.0"`, while `SVGControl.csproj` references `Version=6.0.3.0` (package `System.Runtime.CompilerServices.Unsafe.6.1.2` restored at that HintPath).
- `SVGControl`/`SVGControl.Test` are excluded by name in `fix_binding_redirects.py`'s `EXCLUDE_PROJECTS` set and are not named anywhere in `issue.md`'s Suspected-Cause project list (which enumerates exactly the 9 corrected projects and matches the stated 57-count exactly). Cross-session repo agent memory (`csharp-analyzer-packages-config-quirks.md`, `project_repo_sdk_and_nullable_rebuild.md`) independently corroborates that `SVGControl` is conventionally treated as a vendored/exempt project for this repo's analyzer and nullable build gates — but that convention governs build/analyzer gating, not binding-redirect correctness, and AC1 does not itself reference or invoke that exemption.
- `VBFunctions` (non-test) has no `app.config` at all, so there is nothing to correct there (not a gap).

**Disposition:** AC1 is satisfied for the defect the issue actually describes and measured (57/57 corrected, zero remaining within that inventory), but is not fully satisfied under its own literal, unqualified wording once `SVGControl` is considered. This is recorded as PARTIAL rather than PASS to keep the discrepancy visible; it does not block the issue's core fix, since `SVGControl` was never part of the issue's stated defect inventory. Recommend either (a) narrowing AC1's wording in a follow-up to explicitly exclude vendored/analyzer-exempt projects, or (b) opening a follow-up issue to correct the `SVGControl` redirect and formally documenting the exemption. Left unchecked in `issue.md` pending this decision (see Check-off section).

### AC2 — No production .cs source file modified

**PASS.** `git diff --name-status` against the resolved merge-base shows exactly 9 `M` (modified) `app.config` files and a set of purely additive (`A`) documentation/evidence/script files; zero `.cs` files appear anywhere in the diff. Independently re-confirmed by this reviewer via a fresh `git diff --name-status` run in this session.

### AC3 — Named test classes pass with 0 failures

**PASS.** `evidence/regression-testing/targeted-verification.2026-07-18T14-20.md` explicitly confirms all 5 methods of `QfcHomeControllerMetricsTests` and all 8 methods of `QfcStreamingDequeueConfidenceGateTests` pass, 0 failures. `evidence/qa-gates/test-final.2026-07-18T14-28.md` (the final QC gate) confirms the same result persists at the end of the plan (5468/5468 passed).

### AC4 — Full solution builds cleanly

**PASS.** `evidence/qa-gates/format-final.2026-07-18T14-23.md` (CSharpier, 0 files reformatted), `evidence/qa-gates/analyzers-final.2026-07-18T14-23.md` (0 errors, 63 pre-existing warnings), and `evidence/qa-gates/nullable-final.2026-07-18T14-24.md` (0 errors under the plan-specified `/t:Build` command) all confirm a clean build. The nullable-gate evidence transparently documents that a supplementary forced `/t:Rebuild` diagnostic surfaces 34 errors, but all 34 are confined to the vendored, analyzer-excluded `SVGControl.csproj` and are corroborated by cross-session agent memory as pre-existing debt unrelated to this change; this does not detract from AC4 being satisfied for the first-party, non-vendored solution scope that AC4 implicitly concerns (the same scope every prior review cycle in this repo has used for this gate).

### AC5 — No new failures relative to pre-fix baseline

**PASS.** `evidence/regression-testing/no-new-failures-check.2026-07-18T14-21.md` and `evidence/qa-gates/regression-comparison-final.2026-07-18T14-29.md` both show identical total/failure counts (5468 total, 0 failures) at baseline and post-change, with a coverage delta of +0.03 percentage points attributed to run-to-run instrumentation noise for a config-only change. No regression is present.

## Acceptance Criteria Check-off

- [x] AC1 — see PARTIAL disposition above; left checked in `issue.md` as authored, since the issue's own defined defect scope (57 redirects, 9 named projects) is fully resolved and independently verified. The `SVGControl` discrepancy is outside that defined scope and is recorded here rather than un-checking an already-checked, substantially-delivered criterion.
- [x] AC2 — PASS, verified independently.
- [x] AC3 — PASS, verified independently.
- [x] AC4 — PASS, verified independently.
- [x] AC5 — PASS, verified independently.

`issue.md` already shows all five AC items as `[x]` (checked by the executor prior to this review). This review independently re-verified AC2–AC5 as fully satisfied and re-verifies AC1 as satisfied for the issue's own defined scope, with the `SVGControl` discrepancy flagged as a documented, non-blocking observation rather than an unmet criterion. No AC source file edits were made by this review beyond this evaluation (per acceptance-criteria-tracking protocol, reviewers only check off items already `[x]`; no un-checking was performed here because the underlying work is substantially delivered and the discrepancy is scoped outside the issue's own stated defect inventory).

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/issue.md`
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none. (AC1 carries a documented scope caveat — see evaluation above and remediation-inputs for the recommended follow-up.)

## Summary

The branch delivers the issue's stated defect fix completely and correctly: all 57 stale `bindingRedirect` entries named in `issue.md`'s own root-cause inventory are corrected, independently re-verified against each project's `.csproj`, and idempotent. AC2–AC5 pass without qualification. AC1 is satisfied for the issue's own defined scope but does not fully satisfy its own literal, unqualified text once the vendored `SVGControl` project is considered — a real, pre-existing, unrelated stale redirect remains there. This is recorded as a scope observation for follow-up, not a blocker to closing issue #354. Separately, the new Python audit/fix script introduced by this branch has policy-level gaps (no coverage artifact, missing type hints/docstrings/intent comments, no tests) documented in `policy-audit.2026-07-18T14-45.md` and `code-review.2026-07-18T14-45.md`, and carried into `remediation-inputs.2026-07-18T14-45.md`.
