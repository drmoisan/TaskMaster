# Phase 0 — Policy Read Evidence

Timestamp: 2026-03-25T13:45:00Z

Policy Order:
1. `.github/instructions/general-code-change.instructions.md`
2. `.github/instructions/csharp-code-change.instructions.md`
3. `.github/instructions/general-unit-test.instructions.md`
4. `.github/instructions/csharp-unit-test.instructions.md`

## Files Read (in order)

1. `c:\Users\DanMoisan\repos\TaskMaster\.github\instructions\general-code-change.instructions.md`
   - Covers: bugfix workflow, design principles, error handling, module structure, naming, toolchain loop (format → lint → type-check → test).

2. `c:\Users\DanMoisan\repos\TaskMaster\.github\instructions\csharp-code-change.instructions.md`
   - Covers: CSharpier formatting (not dotnet format), .NET analyzer linting via MSBuild, nullable type-check via MSBuild, VS Code task equivalents.

3. `c:\Users\DanMoisan\repos\TaskMaster\.github\instructions\general-unit-test.instructions.md`
   - Covers: independence, isolation, determinism, coverage thresholds (≥80% repo-wide, ≥90% new code), AAA pattern, no external dependencies, no temp files.

4. `c:\Users\DanMoisan\repos\TaskMaster\.github\instructions\csharp-unit-test.instructions.md`
   - Covers: MSTest framework, Moq for mocking, FluentAssertions for assertions, toolchain commands (csharpier → msbuild analyzers → msbuild nullable → vstest).

## Key Constraints Noted

- Bugfix workflow: write failing regression test first, then implement minimal fix.
- No temp files in tests; no external services.
- `dotnet format` is prohibited; use `csharpier` only.
- MSBuild scripts are invoked via `scripts/vscode/Invoke-VSBuild.ps1`.
- vstest.console.exe for test runner.
