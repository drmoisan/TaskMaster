# Phase 2 — HEAD JaCoCo Coverage Artifact Generation (P2-T5)

Timestamp: 2026-07-20T23-16

Commands:
1. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:cobertura.remediation398.runsettings /InIsolation`
   — Code Coverage collector in Cobertura output format, ModulePaths scoped to the first-party
   production denominator (UtilitiesCS.dll + QuickFiler.dll), [ExcludeFromCodeCoverage] honored,
   vendored/mixed-mode modules and `*.Test.dll` excluded. 5061/5061 passed.
2. Throwaway session Python converter (created and deleted within the executor session; exempt from the
   500-line/durable-script rules; no committed reusable script, no new package dependency): parses the
   Cobertura aggregate and emits JaCoCo XML.

Conversion mechanism: Cobertura -> JaCoCo transform (not a copy). The converter reads the dotnet-coverage
Cobertura root aggregate (`lines-covered`/`lines-valid`, `branches-covered`/`branches-valid` — the
authoritative deduped totals over the two first-party packages) and emits a JaCoCo `<report>` with a
single-level `<counter type="LINE">` and `<counter type="BRANCH">`. Single-level emission ensures the
coverage-gate hook (`Get-JacocoRepoCoverage`/`Get-JacocoBranchCoverage`, which sum all `//counter`
nodes) does not double-count.

EXIT_CODE: 0

Output Summary:
- Output written to artifacts/csharp/coverage.xml (the coverage-gate tooling-input path; permitted by
  enforce-evidence-locations.ps1, not an evidence path).
- Valid JaCoCo XML containing `//counter[@type="LINE"]` (missed 6708, covered 43143) and
  `//counter[@type="BRANCH"]` (missed 2210, covered 9331), aggregated over the first-party production
  denominator (UtilitiesCS + QuickFiler packages).
- Line 86.54% (43143/49851), Branch 80.85% (9331/11541). Reflects HEAD (branch
  bug/breadcrumb-suggestions-upgrade-race-398) with the R1 splits applied; production code is unchanged.
