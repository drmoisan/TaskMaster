---
name: coverage-runner-throws-before-postprocessing
description: Invoke-MSTestWithCoverage.ps1 throws at line 236 on any test failure, BEFORE the Koverage post-processing at line 341, so the Cobertura document survives as raw XML with absolute host paths; and a .cobertura.xml under a feature evidence dir is NOT gitignored
metadata:
  type: reference
---

Two facts that together turn a plan's coverage-baseline task into an unsatisfiable gate. Both
measured in the item-871 worktree on 2026-09-13.

## 1. The throw precedes the post-processing

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` has this order:

- line 236 `throw "MSTest with coverage failed with exit code $coverageExitCode"` — fires whenever
  the `dotnet-coverage collect` child exits non-zero, which includes **any failing test**.
- line 341 `ConvertTo-KoverageCoberturaXml` — rewrites absolute paths to repository-relative
  backslash form and injects `<sources>`.
- line 344 `Assert-CoberturaLineCoverageThreshold` — the document-level 80 percent assertion.

With `$ErrorActionPreference = 'Stop'`, a failing test therefore aborts at 236 and the document is
**never post-processed**. `Test-Path` on the output still returns true because `dotnet-coverage`
wrote the raw file itself, so a `COVERAGE-ARTIFACT-WRITTEN` style probe passes while the artifact is
useless: class `filename` attributes carry absolute host paths, `<sources>` is empty, and third-party
packages (log4net, Mono.Reflection, System.Linq.Async) are still present. Measured shape on that run:
root line-rate 0.2013, 16143/80163 lines, 11 packages, 3166 class elements, and the required
`QuickFiler\Controllers\QfcQueue.cs` form present in **none** of them.

**Why this matters for plan review:** a plan that reasons "the artifact exists on disk even when the
assertion throws" is describing the line-344 threshold branch only. Any acceptance condition that
reads the post-processed *form* (repository-relative filename attributes) is unsatisfiable on the
line-236 branch. Check which throw a coverage acceptance condition is relying on.

Related: [[repo-coverage-runner-parallelism-poisons-deedle]] (the runsettings that causes the
failures in the first place), [[cobertura-postprocessing-is-a-zero-exit-proxy-not-a-test-result]].

## 2. A raw .cobertura.xml under evidence/ is NOT gitignored

Verified with `git check-ignore -v`:

- `TestResults/` → ignored at `.gitignore:39`, so `.trx` files are safe automatically.
- `coverage/*` → ignored at `.gitignore:144`.
- `docs/features/active/<FEATURE>/evidence/baseline/*.cobertura.xml` → **not matched by any rule.**

So a plan that writes its Cobertura output under an evidence directory and then commits "the Phase 0
evidence" will commit raw XML. On the measured run that file contained **2026 occurrences** of the
absolute host path including the account name, while every committed Markdown artifact contained
zero. This violates the issue-671 decision that only projections may be committed.

**How to apply:** write the XML to the plan's stated evidence path so every acceptance condition that
reads it stays satisfiable, but never stage it; stage explicit Markdown pathspecs only. Delete the
raw XML after its last consumer task and before any task asserting an empty porcelain under the
evidence directories. Most plan wording ("the commit contains at least the N artifacts", all of them
Markdown) permits this without any deviation.

Related: [[../_shared_no_absolute_host_paths]], [[gitignore-does-not-cover-trx]].
