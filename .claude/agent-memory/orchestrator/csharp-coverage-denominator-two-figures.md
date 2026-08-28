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

**What decides which figure you get (mechanism, confirmed on #442 2026-08-27):** it is not an invocation flag — it is whether the test run passed. `Invoke-MSTestWithCoverage.ps1` calls `Invoke-DotnetCoverageCollection` first, and that function `throw`s when the coverage exit code is non-zero (`scripts/vscode/Invoke-MSTestWithCoverage.ps1:236`). Post-processing — which is what strips the third-party `<package>` elements — runs only *after* that call returns. So **any run with a failing test aborts before filtering and leaves the unfiltered denominator on disk**, while a green run leaves the filtered one.

Consequence: comparing a green baseline against a red post-change run compares post-processed against raw and manufactures a huge phantom regression. On #442 the prior session recorded "84.84% → 70.28%, **-14.56 pp**" and reasoned about it as a real regression; it was purely this artifact. Once the single failing test was fixed, the same tree measured 85.1255% — i.e. **+0.29 pp, coverage went up**.

Cheap tells for an un-post-processed file, before you trust any delta:
- file size roughly 17-18 MB vs roughly 10-11 MB for the filtered one;
- `<package>` set contains third-party names;
- note `<package ...>` puts `name` *after* `line-rate`, so `grep '<package name='` returns **zero matches** and looks like "no packages". Match `'<package [^>]*>'` and extract `name` instead.

A green run also passes `Assert-CoberturaLineCoverageThreshold`, but that helper's floor is **80%** (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487`), not 85 — clearing it corroborates >= 80% only, so still compute the 85% line / 75% branch check yourself.
