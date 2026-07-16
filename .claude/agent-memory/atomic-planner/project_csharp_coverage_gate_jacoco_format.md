---
name: project-csharp-coverage-gate-jacoco-format
description: The feature-review coverage hook expects JaCoCo XML at artifacts/csharp/coverage.xml, but executors emit Cobertura — a conversion is required, not Cobertura-as-is
metadata:
  type: project
---

The C# coverage gate `.claude/hooks/validate-feature-review-coverage.ps1` reads the canonical path
`artifacts/csharp/coverage.xml` and parses it as **JaCoCo XML**: `Get-JacocoRepoCoverage` sums
`//counter[@type="LINE"]` `missed`/`covered`; `Get-JacocoBranchCoverage` sums
`//counter[@type="BRANCH"]`. The line floor it enforces is `>= 85%` and branch floor `>= 75%`.

**Why:** The vstest/dotnet-coverage runs in this repo emit **Cobertura** (`line-rate`/`branch-rate`
attributes), and feature evidence is stored as `*.cobertura.xml`. The hook cannot read Cobertura, so a
missing/absent `artifacts/csharp/coverage.xml` is graded a mandatory coverage FAIL even when coverage
was in fact produced (this triggered the #328 R1 remediation).

**How to apply:** When a coverage-artifact remediation asks to "place coverage at the canonical path,"
plan a Cobertura -> JaCoCo **conversion**, not a copy. Scope the JaCoCo aggregate to first-party
production packages (exclude vendored Deedle/FSharp.Core/Swordfish/SVGControl and `*.Test` assemblies)
so the readable repo-wide line number reflects the first-party denominator, not the nondeterministic
whole-process denominator (the whole-process line-rate mixes vendored modules and reads ~62-63%).
`artifacts/csharp/` is NOT a forbidden evidence path — `enforce-evidence-locations.ps1` explicitly
allows it; it is the hook's tooling-input path, distinct from `<FEATURE>/evidence/<kind>/` outputs.
Prefer converting the already-verified Cobertura over re-running vstest (deterministic; avoids the
documented dotnet-coverage vendored-module denominator noise).
