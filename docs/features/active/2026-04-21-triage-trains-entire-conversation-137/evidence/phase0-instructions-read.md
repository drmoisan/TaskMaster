# Phase 0 — Policy Files Read

Timestamp: 2026-04-21T12:45:00Z

Policy Order:
1. `.github/copilot-instructions.md` — READ CONFIRMED
2. `.github/instructions/general-code-change.instructions.md` — READ CONFIRMED
3. `.github/instructions/general-unit-test.instructions.md` — READ CONFIRMED
4. `.github/instructions/csharp-code-change.instructions.md` — READ CONFIRMED
5. `.github/instructions/csharp-unit-test.instructions.md` — READ CONFIRMED

## Per-File Confirmation

### 1. `.github/copilot-instructions.md`
- Read confirmed. Key content: project uses MSTest + Moq + FluentAssertions; strict professional tone policy.

### 2. `.github/instructions/general-code-change.instructions.md`
- Read confirmed. Key content: bugfix workflow (failing regression test first → minimal fix → full toolchain); design principles (simplicity, reusability, extensibility, SoC); toolchain loop order: format → lint → type-check → test; no temp files in tests.

### 3. `.github/instructions/general-unit-test.instructions.md`
- Read confirmed. Key content: independence, isolation, fast, deterministic tests; repo-wide coverage >= 80%; new code >= 90%; no external deps in unit tests; no temp files; AAA structure.

### 4. `.github/instructions/csharp-code-change.instructions.md`
- Read confirmed. Key content: format with csharpier (not dotnet format); lint via msbuild with EnableNETAnalyzers + EnforceCodeStyleInBuild; type-check via msbuild with Nullable=enable + TreatWarningsAsErrors; nullable by default; no breaking API changes without explicit call-out.

### 5. `.github/instructions/csharp-unit-test.instructions.md`
- Read confirmed. Key content: MSTest framework only; Moq for mocking; FluentAssertions preferred; toolchain sequence: csharpier → msbuild analyzers → msbuild nullable → vstest with coverage.
