# Phase 0 — Policy / Instructions Read Evidence

Timestamp: 2026-07-11T11-42

Policy Order:
1. CLAUDE.md (all sections)
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/csharp.md

Files read in this session (P0-T1..P0-T4):
- P0-T1: C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/CLAUDE.md (read in full)
- P0-T2: C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/.claude/rules/general-code-change.md (read in full)
- P0-T3: C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/.claude/rules/general-unit-test.md (read in full)
- P0-T4: C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/.claude/rules/csharp.md (read in full)

Notes:
- C# toolchain order (format -> analyzers -> nullable -> test) confirmed from CLAUDE.md and csharp.md.
- CSharpier is the only approved formatter; do not use dotnet format.
- Coverage floor per general-unit-test.md: line >= 85%, branch >= 75%; changed-line no-regression. This change is a net removal of production+test code plus type-swaps.
