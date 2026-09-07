# Phase 0 — Policy documents read

Timestamp: 2026-09-07T00-46
Task: [P0-T1]
Issue: #798

Policy Order: the repository-mandated reading order defined by the Policy Compliance Order section
of the repository-root instruction file and by the policy-compliance-order skill. The six documents
below were read in the order listed, top to bottom, before any other Phase 0 work was performed.

## Files read, in order

1. CLAUDE.md — repository-root standing instructions, including the embedded General Code Change
   Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, and Tone Policy.
2. .claude/rules/general-code-change.md — cross-language code change policy, design principles,
   mandatory toolchain loop, 500-line file size limit.
3. .claude/rules/general-unit-test.md — cross-language unit test policy, coverage requirements,
   coverage exclusion policy, determinism infrastructure.
4. .claude/rules/quality-tiers.md — T1 through T4 module rigor tiers and the uniform-versus-tier
   dependent gate matrix.
5. .claude/rules/tonality.md — required professional tone policy for all agent-authored content.
6. .claude/rules/csharp.md — C#-specific toolchain and coding standards, analyzer stack, DI seams,
   deterministic test rules.

All six paths above were opened and read in full in this session.

## Constraints carried forward into execution

- Toolchain order is format, then lint, then type-check, then test; the loop restarts at step 1
  whenever any step fails or rewrites a file.
- `/t:Rebuild` is mandatory for both msbuild gates; a warm `/t:Build` skips `CoreCompile` and runs
  no analyzers.
- `/p:Nullable=enable` is not supplied; nullable enforcement in this repository is per-file opt-in
  through the `#nullable enable` pragma.
- CSharpier is invoked only through `dotnet tool run` so the manifest-pinned version is used.
- Tests use MSTest, Moq and FluentAssertions. Temporary files in tests are prohibited.
- No production file may be excluded from coverage measurement; the C# coverage floors are
  repository-wide 80 percent on the testable denominator and 90 percent for new modules.
- Policy documents under the dot-claude rules directory are not modified by this work.

EXIT_CODE: 0

Output Summary: All six mandated policy documents were read in the required order. No conflicting
instruction was encountered between them. Execution proceeds to the baseline capture tasks.
