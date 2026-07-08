# Feature Audit: PR #190 CI-failure remediation (cycle 1) — `.csharpierignore` project-file exclusion (#189 / #188 branch)

**Audit Date:** 2026-06-13
**Feature Folder:** `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189`
**Base Branch:** `main` (merge-base `aa63315b`)
**Head Branch:** `bug/vscode-test-runner-parity-188` (HEAD `ece26866`); cycle-1 change uncommitted in the working tree.
**Work Mode:** `minor-audit`
**Audit Type:** Post-remediation acceptance verification (cycle 1, CI-failure remediation)

---

## Scope and Baseline

- **Base branch:** `main` (merge-base commit `aa63315bd432ffbf092cfbb5caa02ee673e7b326`)
- **Head branch/commit:** `bug/vscode-test-runner-parity-188` (commit `ece2686649edae363c148be0751641b04a2ec1d2`); cycle-1 change is uncommitted in the working tree (`.csharpierignore`, 6 insertions).
- **Evidence sources:**
  - Primary: `remediation-inputs.2026-06-13T01-05.md`, `remediation-plan.2026-06-13T01-05.md`
  - QA gates: `evidence/qa-gates/csharpier-check-after.2026-06-13T01-05.md`, `evidence/qa-gates/scope-and-cs-noregress.2026-06-13T01-05.md`, `evidence/qa-gates/ci-rerun-required.2026-06-13T01-05.md`
  - Baseline: `evidence/baseline/csharpier-check-before.2026-06-13T01-05.md`, `evidence/baseline/csharpierignore-preedit.2026-06-13T01-05.md`
  - Direct inspection: `git diff -- .csharpierignore`, `git diff --stat HEAD`, current `.csharpierignore` contents
- **Feature folder used:** `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189`
- **Requirements source:** `remediation-plan.2026-06-13T01-05.md` "Acceptance Criteria (remediation)" — the authoritative AC source for this cycle. `issue.md` (work mode `minor-audit`) defines the feature ACs AC1–AC8, which describe the #189 runsettings work, not the cycle-1 CI-failure remediation; those were adjudicated in the cycle-0 `feature-audit.2026-06-12T20-04.md` and are not re-evaluated here except AC7/AC8 status notes below.
- **Work mode resolution note:** Work mode marker `- Work Mode: minor-audit` is present in `issue.md` and is used as the single source of truth.
- **Scope note:** This is a cycle-1 CI-failure remediation. Its functional change is exclusively `.csharpierignore`. The acceptance criteria evaluated below are the remediation's own five criteria. The original feature ACs (AC1–AC7 marked complete in `issue.md`; AC8 pending user action in Visual Studio) are not affected by cycle 1, which introduces no source change to the runsettings/scripts.

---

## Acceptance Criteria Inventory

**Instructions applied:** The authoritative AC source for cycle 1 is the remediation plan's numbered "Acceptance Criteria (remediation)" list. These are prose/numbered requirements, not markdown checkboxes, so no source-file check-off is performed; status is recorded in this audit only.

**Authoritative AC source files for this run:**
- `remediation-plan.2026-06-13T01-05.md` — primary (cycle-1 remediation ACs)
- `issue.md` — secondary (original feature ACs AC1–AC8; status noted, not re-adjudicated by cycle 1)

### Acceptance criteria (remediation, transcribed from `remediation-plan.2026-06-13T01-05.md`)

1. `.csharpierignore` contains `*.csproj`, `*.props`, `*.targets` with a rationale comment, alongside existing globs. (P1-T1)
2. `dotnet csharpier check .` exits 0 after the edit; the 8 previously-failing `.csproj` files are no longer reported. (P2-T1)
3. No `.cs` formatting regressed and `.csharpierignore` is the only modified tracked source file. (P2-T2)
4. Before/after csharpier evidence captured under canonical evidence paths. (P0-T3, P2-T1)
5. CI re-run requirement recorded; N/A toolchain gates explicitly justified. (P2-T3)

### From issue.md (original feature ACs — status note only)

AC1–AC7 are marked `[x]` in `issue.md` and were adjudicated in the cycle-0 `feature-audit.2026-06-12T20-04.md`. AC8 is `[ ]` and is an explicit pending user action (Visual Studio confirmation of the coverage exclusion's effect), out of scope for cycle 1. Cycle 1 changes no runsettings, script, or `.cs` file, so it neither advances nor regresses AC1–AC8.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | `.csharpierignore` contains `*.csproj`, `*.props`, `*.targets` with a rationale comment, alongside existing globs | PASS | Post-edit `.csharpierignore` lines 9-14 contain the 3-line rationale comment plus the three globs; existing globs (`**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`) are preserved and unreordered. | `git diff -- .csharpierignore`; inspect current `.csharpierignore` | Comment cites CLAUDE.md C#1. |
| 2 | `dotnet csharpier check .` exits 0 after the edit; the 8 previously-failing `.csproj` files no longer reported | PASS | After-edit run: EXIT_CODE 0, 1040 files checked, zero failures. Before-edit run: EXIT_CODE 1, 8 `.csproj` failures, 1060 files. | `dotnet csharpier check .` | `evidence/qa-gates/csharpier-check-after.2026-06-13T01-05.md`; `evidence/baseline/csharpier-check-before.2026-06-13T01-05.md`. 20-file delta = excluded project files. |
| 3 | No `.cs` formatting regressed and `.csharpierignore` is the only modified tracked source file | PASS | After-edit csharpier run reports zero `.cs` formatting failures; `git diff --stat HEAD` shows only `.csharpierignore` changed (6 insertions). Reviewer independently confirmed via `git diff --stat HEAD`. | `dotnet csharpier check .`; `git diff --stat HEAD` | `evidence/qa-gates/scope-and-cs-noregress.2026-06-13T01-05.md`. No `.csproj`/`.props`/`.targets`/`.cs`/workflow file modified. |
| 4 | Before/after csharpier evidence captured under canonical evidence paths | PASS | Both fail-before and pass-after artifacts exist under `evidence/baseline/` and `evidence/qa-gates/`; verified present. | `ls evidence/baseline evidence/qa-gates` | Canonical `<FEATURE>/evidence/<kind>/` paths used; no non-canonical evidence path. |
| 5 | CI re-run requirement recorded; N/A toolchain gates explicitly justified | PASS | Artifact records: (a) required CI check must re-run green on branch head; (b) `modified-workflow-needs-green-run` does not apply (no workflow YAML changed); (c) analyzer/nullable/test gates N/A because no `.cs`/build input changed. | inspect artifact | `evidence/qa-gates/ci-rerun-required.2026-06-13T01-05.md`. |

---

## Adjudication of the four review questions

1. **Does the change correctly and minimally resolve the failing gate?** Confirmed. Evidence shows `dotnet csharpier check .` moved from exit 1 (8 `.csproj` trailing-newline failures, 1060 files) to exit 0 (1040 files, zero failures). The 6-line append is the minimal change that achieves this; the two alternatives (pin/downgrade CSharpier; add trailing newlines to 8 `.csproj`) were considered and rejected. (Remediation AC1, AC2.)
2. **Scope: only `.csharpierignore` changed?** Confirmed. `git diff --stat HEAD` shows a single modified tracked file (`.csharpierignore`, 6 insertions). No `.csproj`/`.props`/`.targets`/`.cs`/workflow file and no #188/#189 file was modified in cycle 1. (Remediation AC3.)
3. **Policy alignment with CLAUDE.md C#1?** Confirmed. CLAUDE.md C#1 states "`csharpier` is file-based and formats only `*.cs` without touching project files." The added globs exclude only project files and contain no `.cs` pattern, so no C# source formatting is weakened; the after-edit run still inspects all 1040 `.cs`/source files and reports zero `.cs` failures.
4. **Not a workflow change; `modified-workflow-needs-green-run` inapplicable?** Confirmed. No `.github/workflows/*.yml`/`*.yaml` was modified. The rule does not apply. The mandatory CI re-run green on the branch head after push remains the final gate, recorded in `evidence/qa-gates/ci-rerun-required.2026-06-13T01-05.md`.

---

## Summary

**Overall Feature Readiness:** PASS

**blocking_count:** 0 (zero FAIL findings and zero blocking PARTIAL findings across the policy audit, code review, and feature audit). The exit gate `blocking_count == 0` is satisfied.

Breakdown across the three cycle-1 artifacts:
- `policy-audit.2026-06-13T01-15.md`: Overall FULLY COMPLIANT; 0 FAIL, 0 blocking PARTIAL.
- `code-review.2026-06-13T01-15.md`: 0 Blocker, 0 Major findings; readiness Go. Three Info-level findings (non-blocking).
- `feature-audit.2026-06-13T01-15.md`: 5/5 remediation ACs PASS; 0 FAIL, 0 PARTIAL.

**Criteria summary (remediation ACs):**
- **PASS:** 5 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. None.

**Recommended follow-up verification steps:**

1. Orchestrator pushes the `.csharpierignore` edit and confirms the required CI check "Format, build, analyze, and test" re-runs green on the branch head (PR #190). This is the final runner-side gate and is not a blocking finding against the code change.
2. Track PR #192 separately; it will require the same ignore globs to pass the same gate (noted in `remediation-inputs.2026-06-13T01-05.md`, out of scope for this cycle).

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- The cycle-1 remediation ACs live in `remediation-plan.2026-06-13T01-05.md` as a prose/numbered list, not markdown checkboxes, so no source-file check-off is performed; status is recorded in this audit only.
- The original feature ACs in `issue.md` are checkbox-backed. Cycle 1 changes no runsettings/script/`.cs` file, so it satisfies none of AC1–AC8 anew. AC1–AC7 remain `[x]` (delivered in cycle 0); AC8 remains `[ ]` (pending user Visual Studio confirmation, out of scope for cycle 1). No `issue.md` checkbox state is changed by this audit.

### AC Status Summary

- Source: `remediation-plan.2026-06-13T01-05.md` (cycle-1 remediation ACs); `issue.md` (original feature ACs, status note only)
- Total AC items (cycle-1 remediation): 5
- Checked off (delivered): 5 (all PASS; recorded in this audit, no checkbox source to mark)
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `remediation-plan.2026-06-13T01-05.md` | 5 | 5 | 0 | Prose-only; status recorded in this audit, no checkbox to mark |
| `issue.md` | 8 | 7 | 1 | Checkbox-backed; not advanced by cycle 1; AC8 pending user VS confirmation (out of scope) |
