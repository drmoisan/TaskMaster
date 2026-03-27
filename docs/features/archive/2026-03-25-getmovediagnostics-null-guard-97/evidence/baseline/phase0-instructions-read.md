# Phase 0 Policy Read Evidence

Timestamp: 2026-03-25T00:00:00Z

Policy Order:
1. `CLAUDE.md`
2. `.claude/skills/general-code-change-policy/SKILL.md`
3. `.claude/skills/general-unit-test-policy/SKILL.md`
4. `.claude/skills/csharp-code-change-policy/SKILL.md`
5. `.claude/skills/csharp-unit-test-policy/SKILL.md`

## Files Read

1. `c:\Users\DanMoisan\repos\TaskMaster\CLAUDE.md` — full read; all sections including General Code Change Policy, C# Code Change Policy, General Unit Test Policy, C# Unit Test Policy, and Tone Policy
2. `c:\Users\DanMoisan\repos\TaskMaster\.claude\skills\general-code-change-policy\SKILL.md` — full read; confirms general code-change rules, bugfix workflow (regression test first, then minimal fix), design principles, toolchain loop
3. `c:\Users\DanMoisan\repos\TaskMaster\.claude\skills\general-unit-test-policy\SKILL.md` — full read; confirms UT1-UT5 policies: independence, isolation, determinism, no external deps, no temp files
4. `c:\Users\DanMoisan\repos\TaskMaster\.claude\skills\csharp-code-change-policy\SKILL.md` — full read; confirms csharpier for formatting, msbuild with analyzers, msbuild with nullable, vstest for testing; no dotnet format
5. `c:\Users\DanMoisan\repos\TaskMaster\.claude\skills\csharp-unit-test-policy\SKILL.md` — full read; confirms MSTest framework, Moq for mocking, FluentAssertions for assertions

## Summary

All five policies read in required order. Key constraints confirmed:
- Bugfix workflow: failing regression test before fix
- C# toolchain: csharpier → msbuild analyzers → msbuild nullable → vstest
- Tests: MSTest + Moq + FluentAssertions; no temp files; no external deps
- Null safety: nullable reference types must remain enabled; no broad suppressions
