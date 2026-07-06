---
name: csharp-repowide-coverage-below-80
description: Canonical first-party repo-wide C# coverage (Koverage, third-party excluded) is ~79.4%, marginally under the 80% gate; a ~59% figure is an artifact of leaving third-party DLLs in the denominator
metadata:
  type: project
---

The repo-wide C# coverage number depends entirely on which assemblies are in the denominator. Two methodologies give very different results, and only one is canonical:

- **Canonical (Koverage / `scripts/vscode/Invoke-MSTestWithCoverage.ps1`):** discovers all first-party `*.Test.dll`, runs them under `dotnet-coverage collect --settings coverage.config` (which EXCLUDES third-party/F# assemblies: Deedle, FSharp.Core, log4net, FluentAssertions, Swordfish, etc.) and post-processes the Cobertura XML to drop non-solution `<package>` elements. Writes `coverage/coverage.cobertura.xml`. **This yields ~79.4% first-party repo-wide line coverage** (measured 2026-07-06). This is the meaningful number and the one the CLAUDE.md ">= 80%" gate applies to. It sits ~0.6pp under 80% as a pre-existing condition.
- **Non-canonical (raw `vstest /EnableCodeCoverage` + `dotnet-coverage merge`, no third-party exclusion):** leaves uninstrumented third-party DLLs in the denominator, deflating the root line-rate to ~58.9%. This ~59% figure is a denominator artifact, NOT the real first-party coverage. Do not cite it as the repo-wide number.

Separately, a single-assembly isolated run (e.g. `QuickFiler.Test.dll` alone) reports only that one production assembly's rate (QuickFiler ~72.46%), which is lower than the first-party aggregate because better-covered assemblies (UtilitiesCS, etc.) are excluded from an isolated run.

**Why:** first-party coverage is genuinely ~79.4% once third-party noise is removed via `coverage.config`; QuickFiler/TaskVisualization/ToDoModel/Tags are the under-covered first-party assemblies that keep the aggregate just under 80%.

**How to apply:** When judging the repo-wide C# coverage gate for a C#-touching feature, use the Koverage/`coverage.config`-excluded number (~79.4%), not a raw all-DLL merge (~59%) and not a single-assembly isolated rate. The canonical artifact the feature-review gate wants is `artifacts/csharp/coverage.xml`; the Koverage task writes `coverage/coverage.cobertura.xml`, so copy/point it to the canonical path. A ~79.4% result is a marginal, pre-existing shortfall under the 80% gate — coverage-neutral changes do not cause it. Note the 80% (CLAUDE.md / `.claude/rules/csharp.md`) vs 85% (`.claude/rules/quality-tiers.md`/`general-unit-test.md`) policy-doc conflict; per the #178 governance-sync decision CLAUDE.md's 80/90 is authoritative and the 85/75 tiers were not adopted. Relates to [[csharp-local-fullsuite-coverage-blocked]] and [[csharp-coverage-artifact-is-cobertura]].
