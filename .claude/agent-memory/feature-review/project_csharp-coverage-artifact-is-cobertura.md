---
name: csharp-coverage-artifact-is-cobertura
description: The canonical C# coverage artifact is Cobertura XML, but the coverage hook parses it as JaCoCo, so it cannot read the C# repo-wide percentage
metadata:
  type: project
---

The canonical C# coverage artifact `artifacts/csharp/coverage.xml` is **Cobertura** format (root `<coverage line-rate=...>`, per-line `<line number= hits=>` elements), produced by converting vstest `/EnableCodeCoverage` output (e.g., via dotnet-coverage `-f cobertura`).

**Why this matters:** `.claude/hooks/validate-feature-review-coverage.ps1` `Get-JacocoRepoCoverage` selects `//counter[@type="LINE"]` nodes, which a Cobertura file does NOT contain. So for C# the hook computes `$null` for repo-wide coverage and skips its "must carry a FAIL verdict when repo-wide < 80%" enforcement (the check is guarded on `$null -ne $RepoWidePct`). The hook therefore only requires that the policy-audit has a C#/coverage-scoped row with an explicit PASS or FAIL and no scope-narrowing phrase.

**How to apply:** Do not rely on the hook to compute the real C# repo-wide figure. Parse `artifacts/csharp/coverage.xml` yourself: repo-wide is the root `line-rate` attribute; per-file is `<line hits>` aggregated by class `filename`. The reviewer owns the actual PASS/FAIL coverage judgment. See [[pr-context-summary-misclassifies-cs]].

For Issue #171 the repo-wide C# line-rate was 57.99%, below 80%, but that is a documented pre-existing COM/WinForms condition (oversized controllers ~3-7% covered, not unit-testable without live Outlook). The governing change-scope gates (>= 90% new module; no changed-line regression) were met, so the verdict was PASS with a pre-existing-condition justification.
