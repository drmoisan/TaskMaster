# Phase 0 policy reads — issue #877

Timestamp: 2026-09-13T10-42
Policy Order: CLAUDE.md, then `.claude/rules/general-code-change.md`, then `.claude/rules/general-unit-test.md`, then the language-specific rule `.claude/rules/csharp.md`, then `.claude/rules/tonality.md`, then the sole requirements source `issue.md`.
Command: read-only verification, no command executed
EXIT_CODE: 0
Output Summary: All six files below were read in full in this execution session, in the order listed, before any implementation task was attempted. No policy document was modified.

## Files read, in order

1. `CLAUDE.md` — [P0-T1]. Names the four-step C# toolchain order: (1) format with `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`; (2) analyze with `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; (3) type-check with `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; (4) test with `vstest.console.exe`. Restart from step 1 on any failure or file change.
2. `.claude/rules/general-code-change.md` — [P0-T2]. Specifies the 500-line file-size limit: no production code, test code, or reusable script file may exceed 500 lines, with exceptions for throwaway session scripts, raw text fixtures, and Markdown documentation.
3. `.claude/rules/general-unit-test.md` — [P0-T3]. Its Coverage Exclusion Policy states that no production file may be excluded from coverage measurement; every production source file is in the denominator regardless of reachability in the test environment. Permitted `exclude` entries are non-production paths only (build output, test files and test infrastructure, non-production config, `node_modules/**`); any `exclude` matching a production source path is a Blocking finding.
4. `.claude/rules/csharp.md` — [P0-T4]. Read in full.
5. `.claude/rules/tonality.md` — [P0-T5]. Read in full.
6. `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/issue.md` — [P0-T6]. Read in full, including `## Verified Mechanism`, `## Refuted Explanations` and `## Out of Scope (non-negotiable)`. Its `## Acceptance Criteria` section holds exactly 8 `- [ ]` items at the time of this read.

## Threshold precedence recorded

`CLAUDE.md` governs the coverage thresholds for this repository and this item. The 85% line and 75% branch figures in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` are not authoritative here. The applicable position is recorded separately in `evidence/baseline/coverage-applicability.2026-09-13T10-42.md`.
