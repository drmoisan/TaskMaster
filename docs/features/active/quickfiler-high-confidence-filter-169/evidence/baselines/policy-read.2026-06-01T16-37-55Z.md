# Policy Read Baseline — Issue #169

Recorded before Phase 1 execution.

| Policy document | Read at (ISO-8601 UTC) |
| --- | --- |
| `CLAUDE.md` § General Code Change Policy | 2026-06-01T16-37-55Z |
| `CLAUDE.md` § General Unit Test Policy | 2026-06-01T16-37-55Z |
| `CLAUDE.md` § C# Code Change Policy | 2026-06-01T16-37-55Z |
| `CLAUDE.md` § C# Unit Test Policy | 2026-06-01T16-37-55Z |
| `.claude/rules/general-code-change.md` | 2026-06-01T16-37-55Z |
| `.claude/rules/general-unit-test.md` | 2026-06-01T16-37-55Z |
| `.claude/rules/csharp.md` | 2026-06-01T16-37-55Z |

## Reading order applied

1. CLAUDE.md (all sections)
2. General Code Change Policy
3. General Unit Test Policy
4. C# Code Change Policy and C# Unit Test Policy

## Key constraints carried into execution

- C# toolchain order: csharpier -> analyzer build -> nullable/TreatWarningsAsErrors build -> vstest with coverage. Restart from step 1 on any failure or auto-fix.
- Tests: MSTest + Moq + FluentAssertions only. No temp files, no external services/COM in tests. Independent, isolated, deterministic.
- Repository-wide line coverage must remain >= 80%; new members target >= 90%; no changed-line coverage regression.
- File-size limit 500 lines for production/test/script files. Pre-existing oversize in `QfcItemController.cs`, `QfcCollectionController.cs`, `QfcFormController.cs` is acknowledged as a pre-existing condition; small additions per plan are authorized without a file split.
- Evidence written only under `docs/features/active/quickfiler-high-confidence-filter-169/evidence/<kind>/`.

## Tool availability confirmed

- msbuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
- dotnet: `C:\Program Files\dotnet\dotnet.exe`
- csharpier: 1.2.6 (via `dotnet tool run csharpier`)
- vstest.console.exe: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (not on PATH; full path will be used)
