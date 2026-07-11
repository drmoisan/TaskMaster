---
name: deletion-only-pr-absent-coverage-artifact-309
description: a pure-deletion, zero-new-code PR (#309 ScoSortedDictionary removal) still gets a FAIL C# coverage verdict under the mandatory artifact-absence rule, even with strong substitute per-module evidence clearing the floor
metadata:
  type: project
---

Reviewed #309 (epic swordfish-removal child F3): deletion-only change (class + its dedicated
test + two `<Compile Include>` lines, zero new files, zero modified production logic). Full
independent re-verification (repo-wide grep, diff-stat scope check, csproj diff inspection,
TestMethod-count cross-check) confirmed the change exactly as scoped, toolchain-clean, 8/8 ACs
genuinely earned.

Even so, `artifacts/csharp/coverage.xml` (the canonical path this review's coverage procedure
requires) was absent, forcing a FAIL verdict on the repo-wide C# coverage row per the binding
"flag as FAIL if artifact absent" instruction — despite the feature's own captured Cobertura
evidence (`evidence/baseline/*.cobertura.xml`, `evidence/qa-gates/*.cobertura.xml`) showing the
only touched module (`UtilitiesCS.dll`) at 88.19%→88.23% with zero per-class regressions.

**How to apply:** Do not let a clean, low-risk, well-evidenced deletion-only PR talk you into
silently upgrading an artifact-absence FAIL to PASS. Disposition it explicitly: PASS on
substance in the executive summary / feature-audit, FAIL-with-remediation on the specific
coverage-artifact-presence procedural gate, and write a `remediation-inputs.<ts>.md` pointing
at "produce the canonical coverage artifact" as a CI/tooling task, not a code fix. See also
[[coverage-hook-label-substring-false-positive]], [[csharp-coverage-artifact-is-cobertura]],
[[csharp-repowide-coverage-below-80]].
