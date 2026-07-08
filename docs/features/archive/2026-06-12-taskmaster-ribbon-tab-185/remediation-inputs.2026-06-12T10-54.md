# Remediation Inputs — TaskMaster Ribbon Tab (Issue #185)

**Cycle Entry Timestamp:** 2026-06-12T10-54
**Feature Folder:** `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185`
**Base Branch:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head Branch:** `TaskMaster-wt-2026-06-12-10-29` (`9db230d50a49bf4831174f2d4aef8bec624b5358`)
**Work Mode:** `minor-audit`

## Source Audit Artifacts

These findings originate from the following review artifacts (same cycle timestamp):

- `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/policy-audit.2026-06-12T10-54.md` (Section 1.2.1, Section 8 — coverage FAIL)
- `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/code-review.2026-06-12T10-54.md` (Major finding — coverage artifact absent)
- `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/feature-audit.2026-06-12T10-54.md` (Overall readiness: NEEDS REVISION)

## Blocking Findings (must remediate)

### R1 (BLOCKING) — Canonical C# coverage artifact absent

- **Finding:** The canonical C# coverage artifact `artifacts/csharp/coverage.xml` does not exist. The branch diff contains two C# files (`TaskMaster/Ribbon/RibbonExplorer.xml`, `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`), so a coverage artifact is mandatory. Its absence makes the repository-wide >= 80% C# line-coverage gate non-evaluable. The single-assembly aggregate in `evidence/qa-gates/coverage-delta.md` (8.34% -> 8.40%) is explicitly not a repository-wide figure (it is dominated by unexercised third-party DLLs).
- **Affected paths:**
  - `artifacts/csharp/coverage.xml` (to be produced)
  - `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` (instrumentation target)
- **Expected behavior after remediation:** `artifacts/csharp/coverage.xml` exists in Cobertura format and either (a) carries a repository-wide C# line-coverage figure >= 80%, or (b) is accompanied by a documented repository-wide C# coverage figure >= 80% produced by the full CI coverage suite. The policy audit's C# coverage Disposition can then be re-evaluated against a real repo-wide figure.
- **Verification commands:**
  - `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage`
  - Convert the produced `.coverage` to Cobertura and write to the canonical path: `dotnet-coverage merge -f cobertura -o artifacts/csharp/coverage.xml <.coverage path>` (or equivalent repo-standard conversion).
  - Re-run feature-review coverage verification; confirm `Get-JacocoRepoCoverage -Path 'artifacts/csharp/coverage.xml'` yields a repo-wide figure and that figure is >= 80%.
- **Note for planner:** The in-scope production change is a non-compiled XML resource with no instrumentable IL; remediation is about producing the mandatory repository-wide coverage evidence artifact, not about adding production code or new tests. If a true repository-wide figure cannot be produced locally (the local full-assembly run is documented as blocked by a Moq binding redirect — see project memory), record the repository-wide C# coverage figure from the PR CI run and cite it as the authoritative repo-wide evidence.

## Non-Blocking Findings (recommended)

### R2 (MINOR) — PR context summary misclassifies C# scope

- **Finding:** `artifacts/pr_context.summary.txt` reports "Core logic changes: 0 files" and omits the two C# files present in `git diff 742d4f1..9db230d`.
- **Expected behavior:** Regenerate the PR context artifacts per `pr-context-artifacts` so the changed-files overview lists `RibbonExplorer.xml` and `RibbonExplorerXmlTests.cs`.
- **Verification command:** Regenerate PR context and confirm the C# files appear in the "Changed files overview" section.

### R3 (INFO) — Nullable build pre-existing vendored failures

- **Finding:** `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` exits 1 with 84 pre-existing errors confined to vendored `SVGControl` (68) and `UtilitiesSwordfish` (16).
- **Expected behavior:** No remediation required for #185. These errors are excluded from this repo's standards per `.claude/rules/csharp.md` and are identical to the documented baseline.

## Do Not Do

- Do not modify `RibbonExplorer.xml` group/control content; AC4 (verbatim preservation) is satisfied and must remain so.
- Do not weaken, skip, or reword any coverage policy threshold to make the gate pass.
- Do not relocate evidence outside the canonical `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/<kind>/` tree.
- Do not expand scope beyond producing the mandatory C# coverage evidence and regenerating PR context.
- Do not touch the vendored projects to silence pre-existing nullable errors.

## Handoff

Per `remediation-handoff-atomic-planner`, this `remediation-inputs.2026-06-12T10-54.md` is the cycle-entry artifact. The remediation plan (`remediation-plan.2026-06-12T10-54.md`) must be authored by `atomic-planner` (not by feature-review), conform to `.claude/skills/atomic-plan-contract/SKILL.md`, and pass `validate_orchestration_artifacts` with `artifact_type: "plan"` before `atomic-executor` preflight. feature-review does not author the plan file or invoke workers; control returns to the orchestrator to delegate plan authoring.
