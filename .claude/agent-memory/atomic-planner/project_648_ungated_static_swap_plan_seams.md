---
name: project-648-ungated-static-swap-plan-seams
description: Issue #648 planning seams — vstest prints no "Failed:" line on green, dotnet-tools.json sits at repo root, a scoped coverage run cannot meet the solution-wide 80% floor, and two assemblies both declare a WpfUiDispatcherTests class
metadata:
  type: project
---

Seams found while authoring the `minor-audit` plan for issue #648 (`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` ungated static swap). All re-derived against `origin/main` on 2026-08-31.

**Why:** each one silently makes an acceptance condition unsatisfiable or a cited command unexecutable, and none is visible from the plan text or the tool documentation.

**How to apply:** when a C# plan asserts over a test run, a coverage figure, or a class-scoped filter in this repo, check these first.

1. **`vstest.console.exe` prints a `Failed:` line only when at least one test failed.** A gate demanding `Failed: 0` is unsatisfiable on a green run. Assert the literal `Test Run Successful.` instead, and record the `Total tests:` and `Passed:` counts.
2. **`Invoke-MSTest.ps1 -SearchRoot <SingleProject>.Test` is still unexecutable on current main.** Lines 107-113 `Select-Object -ExpandProperty FullName` yields a scalar for one match and line 115/120 read `.Count` under `Set-StrictMode -Version Latest` (line 77). The sibling `Invoke-MSTestWithCoverage.ps1` does NOT share the defect — its line 296 wraps discovery in `@(...)`. See [[reference-invoke-mstest-single-searchroot-defect]].
3. **`Invoke-MSTestWithCoverage.ps1` prints no coverage percentage at all.** Its only numeric output is `coverage/coverage.cobertura.xml`; read `line-rate`, `lines-covered`, `lines-valid` off the root `coverage` element. `Assert-CoberturaLineCoverageThreshold` (`Invoke-MSTestWithCoverage.Helpers.ps1:459-491`) only *throws* below 80%.
4. **A `-SearchRoot <one project>` coverage run cannot meet that 80% floor**, because the floor is evaluated over the whole instrumented image while only one assembly's tests ran. Use `-SearchRoot .` when the plan needs a passing coverage gate, and gate Phase 2 on equality with the recorded Phase 0 exit code rather than on `EXIT_CODE: 0`.
5. **`coverage.config` excludes only third-party modules (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest).** Test assemblies ARE instrumented, so a test-only change grows `lines-valid`; a `lines-valid` equality gate is unsatisfiable. Gate on `lines-covered` monotonicity.
6. **The CSharpier manifest is `dotnet-tools.json` at the repository ROOT, not `.config/dotnet-tools.json`.** A plan task that cites the `.config/` path names a file that does not exist.
7. **Two different assemblies declare a class named `WpfUiDispatcherTests`**: `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` and `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:12`. `/TestCaseFilter:"FullyQualifiedName~WpfUiDispatcherTests"` is a substring match, so a `Total tests: 2` assertion is only correct when the run passes exactly one assembly path.
8. **`scripts/vscode/Invoke-Restore.ps1` is the repo-standard restore** (MSBuild `/t:Restore` with `/p:RestorePackagesConfig=true`, line 36), which covers both `packages.config` and `PackageReference` projects. Prefer it to a bare `nuget restore`.

Related: [[project-csharp-phase0-toolchain-bootstrap]], [[agent-worktrees-need-sdk-and-nuget-bootstrap]], [[project-494-threshold-reconciliation-plan-seams]].
