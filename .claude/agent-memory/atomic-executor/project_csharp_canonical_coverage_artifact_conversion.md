---
name: csharp-canonical-coverage-artifact-conversion
description: How to emit the hook-mandated artifacts/csharp/coverage.xml (JaCoCo) from a feature Cobertura, and why the first-party aggregate under-counts below 85%
metadata:
  type: project
---

The C# coverage gate hook (`.claude/hooks/validate-feature-review-coverage.ps1`) reads
`artifacts/csharp/coverage.xml` as **JaCoCo** XML: `Get-JacocoRepoCoverage` sums
`//counter[@type="LINE"]` missed/covered; `Get-JacocoBranchCoverage` sums `//counter[@type="BRANCH"]`.
Feature evidence is usually **Cobertura** (`line-rate`/`branch-rate`), which the hook cannot read — so
a remediation emitting the canonical artifact must CONVERT Cobertura -> JaCoCo, not copy it.

Conversion method that reproduces sane numbers: iterparse the Cobertura, scope to first-party
production packages (TaskMaster repo: `QuickFiler, Tags, TaskMaster, TaskVisualization, ToDoModel,
UtilitiesCS`; exclude vendored NuGet `Deedle/FSharp.Core/FluentAssertions/log4net/Mono.Reflection/
SVGControl/System.*` and all `*.Test`), dedup `<line>` by `(filename, line-number)` (covered if any
occurrence `hits>0`; Cobertura repeats lines in both `<methods>` and class-level `<lines>`), read
branches from `condition-coverage="p% (a/b)"`. Emit ONE `<counter type=LINE>` + ONE
`<counter type=BRANCH>` per package at a single level so the hook's `//counter` sum doesn't double-count.

**Why:** #328 remediation R1. A single local `dotnet-coverage` run instruments entire first-party
assemblies but only the `.Test` projects actually run contribute a numerator — so QuickFiler/Tags/
TaskVisualization showed 0% (their test projects weren't in that vstest collection) and dragged the
first-party aggregate to 70.45%, below the 85% floor, even though `UtilitiesCS` (the assembly holding
the change) was 88.33%. This is the documented denominator/instrumentation nondeterminism.

**How to apply:** the canonical artifact's real deliverable is *presence + hook-parseability* (resolves
the "artifact absent" feature-audit finding). The repo-wide first-party aggregate is authoritatively
deferred to the PR CI coverage run per policy-audit §5.4 — do NOT cherry-pick only the instrumented
assemblies to force >=85% (that violates the no-production-file-excluded coverage rule). When the plan
offers a "cite the CI workflow-run URL" alternative but no PR/branch run exists yet
(`gh pr list`/`gh run list` empty), record the URL as PENDING PR creation rather than blocking.
