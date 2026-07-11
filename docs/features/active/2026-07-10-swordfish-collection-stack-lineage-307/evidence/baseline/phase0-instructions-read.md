# Phase 0 — Policy / Instructions Read (P0-T1)

Timestamp: 2026-07-11T03-04

Policy Order: CLAUDE.md → .claude/rules/general-code-change.md → .claude/rules/general-unit-test.md → .claude/rules/csharp.md

Files read (all confirmed present in this feature worktree
`C:/Users/DanMoisan/repos/TaskMaster-wt/swordfish-collection-stack-lineage-307`):

1. `CLAUDE.md` — present. Standing instructions: policy-compliance order, general + C# code-change
   and unit-test policy, C# toolchain (csharpier → msbuild analyzers → msbuild nullable → vstest
   `/EnableCodeCoverage`), 500-line file limit, MSTest + Moq + FluentAssertions, coverage floor
   80% repo-wide / 90% new code.
2. `.claude/rules/general-code-change.md` — present. Cross-language code-change policy (design
   principles, toolchain loop, 500-line file limit, I/O boundaries, no temp files in tests).
3. `.claude/rules/general-unit-test.md` — present. Cross-language unit-test policy (independence,
   isolation, determinism, no temp files, coverage exclusion policy).
4. `.claude/rules/csharp.md` — present. C#-specific toolchain and standards (csharpier, .NET
   analyzers, nullable, MSTest/Moq/FluentAssertions, DI seams, coverage >= 80% repo / >= 90% new,
   analyzer stack, prohibited behaviors).

Supporting feature documents also read: `spec.md`, `user-story.md`, and the research artifact
`research/2026-07-10T20-45-swordfish-collection-stack-lineage-research.md`.

Output Summary: All four policy files read in the required order and confirmed present. Toolchain
and constraints understood. Coverage bar per repo CLAUDE.md/csharp.md: repo-wide line >= 80% (the
delegation prompt tightens to >= 85% line / >= 75% branch, which is applied as the operative floor),
new code >= 90%.
