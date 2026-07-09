---
name: csharp-coverage-artifact-is-cobertura
description: The canonical C# coverage artifact is Cobertura XML, but the coverage hook parses it as JaCoCo, so it cannot read the C# repo-wide percentage
metadata:
  type: project
---

The canonical C# coverage artifact `artifacts/csharp/coverage.xml` is **Cobertura** format (root `<coverage line-rate=...>`, per-line `<line number= hits=>` elements), produced by converting vstest `/EnableCodeCoverage` output (e.g., via dotnet-coverage `-f cobertura`).

**Why this matters:** `.claude/hooks/validate-feature-review-coverage.ps1` `Get-JacocoRepoCoverage` selects `//counter[@type="LINE"]` nodes, which a Cobertura file does NOT contain. So for C# the hook computes `$null` for repo-wide coverage and skips its "must carry a FAIL verdict when repo-wide < 80%" enforcement (the check is guarded on `$null -ne $RepoWidePct`). The hook therefore only requires that the policy-audit has a C#/coverage-scoped row with an explicit PASS or FAIL and no scope-narrowing phrase.

**Artifact-format variance (Issue #292, 2026-07-09):** `artifacts/csharp/coverage.xml` is NOT always Cobertura. On #292 it was **Visual Studio merged `.coverage` XML** (root `<results><modules>`, per-`<function ... line_coverage="NN.NN" lines_covered="N" lines_not_covered="N">` and per-`<module ... line_coverage=>` attributes; a percentage already, not hits+line-rate), produced by `vstest.console.exe ... /EnableCodeCoverage` with Cobertura companions written separately as `coverage.baseline.cobertura.xml`/`coverage.postchange.cobertura.xml`. The hook's JaCoCo `//counter[@type="LINE"]` selector returns `$null` on this format too, so the same "only needs a PASS/FAIL row, no narrowing" rule holds. To manually verify a specific changed method on the VS format, grep for `name="<Method>()"` and read its `line_coverage`/`lines_covered`/`lines_not_covered` attributes (e.g., on #292 `MaterializeFilteredStores()` read `line_coverage="100.00"` 5/5). Note the merged XML can contain duplicate module/function entries (same class loaded into two test processes) — one instance may show 0.00 while the real one shows the covered figure; use the instance with non-zero `lines_covered`.

**How to apply:** Do not rely on the hook to compute the real C# repo-wide figure. Parse `artifacts/csharp/coverage.xml` yourself: for Cobertura, repo-wide is the root `line-rate` attribute and per-file is `<line hits>` aggregated by class `filename`; for the VS `.coverage` XML, read `line_coverage` attributes on `<module>`/`<function>` nodes. The reviewer owns the actual PASS/FAIL coverage judgment. See [[pr-context-summary-misclassifies-cs]].

For Issue #171 the repo-wide C# line-rate was 57.99%, below 80%, but that is a documented pre-existing COM/WinForms condition (oversized controllers ~3-7% covered, not unit-testable without live Outlook). The governing change-scope gates (>= 90% new module; no changed-line regression) were met, so the verdict was PASS with a pre-existing-condition justification.
