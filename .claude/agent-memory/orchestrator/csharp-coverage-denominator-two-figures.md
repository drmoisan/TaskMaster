---
name: csharp-coverage-denominator-two-figures
description: TaskMaster C# coverage has two legitimate figures ~15 points apart; the filtered first-party one (~85.9%) clears the 85% gate, the unfiltered one (~70.4%) does not. Measure before believing the "85% floor trap" warning.
metadata:
  type: project
---

Running `scripts/vscode/Invoke-MSTestWithCoverage.ps1` two different ways yields two very different repo-wide numbers, and only one is the policy denominator.

- **Unfiltered** (what a plain default invocation produced on 2026-08-08): `line-rate` 0.7043, `lines-valid` 80166. Includes vendored third-party packages — `log4net`, `Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive`, `System.Linq.Async`.
- **Filtered by `coverage.config`** (the policy denominator): `line-rate` 0.8585, `branch-rate` 0.7925, `lines-valid` 111207. Exactly nine first-party packages — `QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`. No `*.Test` assembly in the denominator.

**Why:** `.claude/rules/general-unit-test.md` requires coverage tooling to be configured so metrics reflect application code, and CLAUDE.md § UT2 defines the floor against the testable first-party denominator. Vendored libraries are not application code.

**How to apply:** before acting on [[feature-review-coverage-85-floor-trap]], *measure*. That note says never generate `artifacts/csharp/coverage.xml` because the hook hard-codes 85% and will force a false FAIL. On issue #503 that was wrong: the filtered figure was 85.85%, over the floor, so generating the artifact was safe and honest. Confirm which figure you have by listing the `<package name=...>` set — if `log4net` appears, you have the unfiltered one.

**Format gotcha:** `.claude/hooks/validate-feature-review-coverage.ps1` parses **JaCoCo** (`//counter[@type="LINE"]` with `missed`/`covered`), not Cobertura. The repo's runner emits Cobertura. A format projection is required, not optional — a Cobertura file at that path parses to zero counters and the percentage gate silently does not run. Verify with the hook's own parsers:

```powershell
. ./.claude/hooks/validate-feature-review-coverage.ps1 | Out-Null
Get-JacocoRepoCoverage -Path 'artifacts/csharp/coverage.xml'
```

`artifacts/` is gitignored (`.gitignore:57`), so the gate artifact is local-only and regenerated, never committed.

**Do not commit raw Cobertura as evidence.** Executors do this by default; two reports added ~20 MB and 374,000 lines to history on one bug fix. Commit a package-level JaCoCo projection instead and record the substitution.
