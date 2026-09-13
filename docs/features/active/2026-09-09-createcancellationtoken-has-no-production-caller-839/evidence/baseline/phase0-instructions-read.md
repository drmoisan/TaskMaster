# Phase 0 — Policy and requirements reads (issue #839)

Timestamp: 2026-09-13T02-28
Command: pwsh -NoProfile -Command 'Get-Date -Format yyyy-MM-ddTHH-mm'
EXIT_CODE: 0

Policy Order: CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/tonality.md, .claude/rules/csharp.md

Files Read:
1. CLAUDE.md — states the C# toolchain order as format (dotnet tool run csharpier format ., verified with csharpier check .), then analyze (msbuild /t:Rebuild with EnableNETAnalyzers and EnforceCodeStyleInBuild), then type-check (msbuild /t:Rebuild with TreatWarningsAsErrors), then test (vstest.console.exe with coverage), restarting from step 1 whenever a step fails or changes files.
2. .claude/rules/general-code-change.md — states the file size limit that no production code, test code, or reusable script file may exceed 500 lines, with exceptions only for throwaway session scripts, raw text fixtures, and Markdown documentation.
3. .claude/rules/general-unit-test.md — records line coverage >= 85 percent and branch coverage >= 75 percent across tiers T1 through T4 as an observation only; the governing figures for this item are CLAUDE.md's (policy rank 1) repository-wide 80 percent line floor and 90 percent floor for new modules, classes and methods.
4. .claude/rules/quality-tiers.md — defines the T1 through T4 module rigor tiers, with quality-tiers.yml at the repository root as the project-to-tier map and uniform line and branch coverage thresholds across tiers.
5. .claude/rules/tonality.md — requires a professional, factual, neutral tone in all agent-authored content and prohibits humor, hyperbole and decorative metaphor.
6. .claude/rules/csharp.md — forbids passing the solution-wide Nullable property (projects opt into nullable per file with a #nullable enable directive) and requires the Rebuild target for both msbuild gates, because a warm Build target can skip CoreCompile and exit 0 without running analyzers or nullable-flow diagnostics.

Feature Documents Read:
1. spec.md — the bug specification; carries AC1 through AC12 under its `## Acceptance Criteria` heading at lines 211-222, all unchecked at the time of this read.
2. issue.md — the promoted bug record; line 12 carries `- Work Mode: full-bug`.
3. user-story.md — narrative context only under full-bug work mode; carries no checkboxes and is not an acceptance-criteria source.
4. research/2026-09-12T18-05-createcancellationtoken-init-path-research.md — the read-only analysis establishing the zero-production-caller finding, the five candidate remedies, and the six-member family baseline.

AC Source: spec.md (AC1-AC12)

Output Summary:
- CMD-TS returned the timestamp above.
- Policy rank 1 read in full from the worktree root.
