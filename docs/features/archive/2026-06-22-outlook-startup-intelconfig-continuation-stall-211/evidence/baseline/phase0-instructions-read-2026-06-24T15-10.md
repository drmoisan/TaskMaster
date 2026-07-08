# Phase 0 — Policy Instructions Read (issue #211)

Timestamp: 2026-06-24T15-10

Policy Order:
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

Files read, in order:
- `CLAUDE.md` (standing instructions; C# toolchain order CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage; COM/VSTO/WinForms coverage exemption; tone policy)
- `.claude/rules/general-code-change.md` (design principles; 500-line file limit; fail-fast error handling; mandatory toolchain loop with restart-on-change)
- `.claude/rules/general-unit-test.md` (independence/isolation/determinism; >= 80% repo coverage, >= 90% new code; no temporary files; no external dependencies)
- `.claude/rules/csharp.md` (CSharpier formatting; .NET analyzer stack; nullable/TWAE type-check; MSTest + Moq + FluentAssertions; banned APIs DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay)

Outcome: All four policy files read in required order prior to executing plan tasks.
