# QA Gate 1 — CSharpier Format (Remediation Cycle 1)

- Timestamp: 2026-07-08T00-40
- Command: `dotnet tool run csharpier format .` (initial format pass), then
  `dotnet tool run csharpier check .` (verification pass)
- EXIT_CODE: 0 (check pass)
- Output Summary:
  - `dotnet tool run csharpier format .` -> "Formatted 1284 files in 1503ms." — `git status`
    after this run showed only the plan-scoped files as modified/new
    (`StoreDisableServiceTests.cs`, `StoresWrapperTests.cs`, `UtilitiesCS.Test.csproj`,
    `StoresWrapperDisableTests.cs`); no other files in the repo were reformatted, confirming the
    hand-authored moved/new/edited code already matched CSharpier's canonical formatting.
  - `dotnet tool run csharpier check .` -> "Checked 1284 files in 3445ms.", exit code 0, zero
    files reported as needing reformatting.

## Deviation Note

- Command form: `dotnet tool run csharpier .` (the bare form specified in CLAUDE.md /
  `.claude/rules/csharp.md`) is rejected by the installed CSharpier v1.2.6 CLI ("Required command
  was not provided... Did you mean: format <directoryOrFile> | check <directoryOrFile> | ...").
  Per this repo's known CSharpier v1 CLI syntax change (documented precedent from prior
  remediation cycles), the equivalent v1 subcommand forms `format .` / `check .` were used
  instead. This is a CLI-syntax compatibility substitution, not a change in tool or intent —
  CSharpier is still the sole formatter used, per policy.
