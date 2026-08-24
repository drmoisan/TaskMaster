---
name: project-csharp-coverage-gate-jacoco-format
description: The feature-review hook parses artifacts/csharp/coverage.xml as JaCoCo, but repo precedent also accepts Cobertura there (hook nulls out, reviewer parses Cobertura directly) — follow whichever format the remediation delta names
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

**Both formats are accepted precedent — check what the caller/delta names.** #503 shipped JaCoCo at
the canonical path (hook-readable). #230 and #438 shipped **Cobertura** there: their policy audits
explicitly record that the hook's JaCoCo query computes null and "does not itself gate the
percentages," and the reviewer's direct Cobertura parse (root `line-rate`/`branch-rate`) is the
authoritative measurement. #511 R1 (2026-08-23) had an executor preflight delta that mandated the
Cobertura copy with `line-rate >= 85` / `branch-rate >= 75` acceptance — do not "correct" such a
delta to JaCoCo; the Cobertura route is valid because the reviewer, not the hook, computes the figures.

**How to apply:** When a coverage-artifact remediation asks to "place coverage at the canonical path"
and does not name a format, plan a Cobertura -> JaCoCo **conversion**, not a copy, so the hook itself
can read it. When the delta names Cobertura and cites `line-rate`/`branch-rate` gates, plan the copy
verbatim. Scope the JaCoCo aggregate to first-party
production packages (exclude vendored Deedle/FSharp.Core/Swordfish/SVGControl and `*.Test` assemblies)
so the readable repo-wide line number reflects the first-party denominator, not the nondeterministic
whole-process denominator (the whole-process line-rate mixes vendored modules and reads ~62-63%).
`artifacts/csharp/` is NOT a forbidden evidence path — `enforce-evidence-locations.ps1` explicitly
allows it; it is the hook's tooling-input path, distinct from `<FEATURE>/evidence/<kind>/` outputs.
Prefer converting the already-verified Cobertura over re-running vstest (deterministic; avoids the
documented dotnet-coverage vendored-module denominator noise).
