---
name: csharp-canonical-jacoco-includes-uninstrumented-assemblies
description: A canonical artifacts/csharp/coverage.xml that aggregates first-party assemblies whose .Test projects were not in the coverage collection reads ~0% for those assemblies and unconditionally blocks feature-review termination via the sub-75% branch check
metadata:
  type: project
---

When the executor/remediation emits `artifacts/csharp/coverage.xml` (JaCoCo counter form) as the
"first-party" aggregate, it can include assemblies whose `.Test` project was NOT part of the delivered
vstest collection. Those assemblies then read ~0% LINE/BRANCH because they are UNMEASURED (loaded but
not exercised), not uncovered. This drags the hook's whole-file aggregate below floor.

Concrete case #328: the delivered artifact aggregated 6 packages; QuickFiler (0%), Tags (0%), and
TaskVisualization (0.83%) had no test project in the run (only UtilitiesCS.Test, TaskMaster.Test,
ToDoModel.Test ran), pulling the aggregate to LINE 70.45% / BRANCH 67.11%. Re-scoped to the
instrumented first-party assemblies (UtilitiesCS, TaskMaster, ToDoModel) from the SAME fixed Cobertura
counters it is LINE 85.71% / BRANCH 79.34% — both clear floor. UtilitiesCS alone (the #328 assembly)
is 88.33%/82.00%.

**Why this matters:** `validate-feature-review-coverage.ps1` `Test-LanguageCoverageRow` has an
UNCONDITIONAL block at the end — `if ($BranchPct -lt 75) { Ok=$false }` — that no policy-audit wording
can override. So once C# is enumerated (space-free `.cs` bullet in the summary), a below-75%-branch
canonical artifact blocks termination outright. Note this is worse than an ABSENT artifact: absence
makes `Get-JacocoBranchCoverage` return `$null`, so the branch check is skipped (that is how cycle 1
terminated). A remediation that "adds the canonical artifact" can therefore convert a passing (absent)
state into a hard block if it emits the mis-scoped aggregate.

**How to apply:** For a C#-touching review, verify which `.Test` projects were actually in the coverage
collection (read the vstest evidence / coverage-canonical note). If the canonical JaCoCo file includes
first-party assemblies with no test project in that collection, re-scope it to the instrumented
assemblies using the fixed Cobertura counters (no coverage rerun) and document the correction in
policy-audit §5.1 — analogous to the mandated in-place correction of the mis-classified PR summary
([[project_pr-context-summary-misclassifies-cs]]). Keep the full all-first-party repo-wide figure
deferred to the PR CI run. Verify with `. ./.claude/hooks/validate-feature-review-coverage.ps1;
Get-JacocoRepoCoverage/Get-JacocoBranchCoverage` before finalizing. Contrast with
[[coverage-hook-forces-fail-below-floor-despite-exemption]] (where the C# Cobertura read $null and was
skipped) — here the artifact is genuine JaCoCo and DOES parse.
