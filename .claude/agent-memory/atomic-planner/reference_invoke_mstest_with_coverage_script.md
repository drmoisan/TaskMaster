---
name: reference-invoke-mstest-with-coverage-script
description: repo-canonical full-suite MSTest+coverage runner at scripts/vscode/Invoke-MSTestWithCoverage.ps1 — use for baseline/final-QC coverage capture tasks
metadata:
  type: reference
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` discovers every `*.Test.dll` under `-SearchRoot` (filtered to `bin\<Configuration>\`, excluding `obj\`/`ref\`) and drives them through one `dotnet-coverage collect` invocation wrapping `vstest.console.exe` (`/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`), emitting a Cobertura-format XML at `-CoverageOutput` (default `coverage\coverage.cobertura.xml`, relative to repo root). It requires `dotnet-coverage` (global tool) and VS Test Platform components (resolved via `vswhere.exe`) and reads `coverage.config` at repo root for instrumentation excludes plus `scripts/vscode/TaskMaster.cli.runsettings` for MSTest parallelization.

This is the correct command to cite in atomic plans needing a full first-party-assembly baseline/final-QC coverage figure (numeric `line-rate`/`branch-rate` from the emitted Cobertura XML root `<coverage>` element), satisfying the CUT3 `vstest.console.exe ... /EnableCodeCoverage` toolchain requirement without inventing new tooling. `-CoverageOutput` can be pointed at `<FEATURE>/evidence/<baseline|qa-gates>/coverage-<stage>.cobertura.xml` to keep the artifact in the canonical evidence location — see [evidence-path-normalization](evidence-path-normalization.md).

Caveat: its discovery filter excludes `\obj\` and `\ref\` but does **not** exclude `\.claude\`, so running it with `-SearchRoot .` from the main repo root picks up stale `.claude/worktrees/agent-*` builds and yields bogus `AssemblyInitialize` signature failures (see the user-scope memory on excluding `.claude/worktrees`). Every plan task that invokes this script must assert the discovered-assembly list contains no `\.claude\` path, or scope `-SearchRoot` to a single project. When a task only needs pass/fail identity (not coverage), prefer invoking `vstest.console.exe` against an explicitly named assembly path plus `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, which bypasses globbing entirely.

Note: this Cobertura-format output is a different artifact/format from the JaCoCo-format `artifacts/csharp/coverage.xml` expected by `validate-feature-review-coverage.ps1` (see [project_csharp_coverage_gate_jacoco_format](project_csharp_coverage_gate_jacoco_format.md)) — do not conflate the two when a plan needs to satisfy the feature-review coverage gate specifically.
