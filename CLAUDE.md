# TaskMaster — Claude Instructions

## Project Guidelines

- Repository uses policy skills under `.claude/skills/`, including `csharp-code-change-policy` requiring environment-appropriate C# commands and strict toolchain order.
- C# tests must use **MSTest** as the framework, **Moq** for mocking, and **FluentAssertions** for assertions.
- C# code must pass CSharpier formatting, .NET analyzer diagnostics, nullable checks, and MSTest test coverage.

## Policy Compliance Order

Before any code change, read policies in this priority order:

1. This file (CLAUDE.md)
2. `general-code-change-policy` skill
3. `general-unit-test-policy` skill
4. For C#: `csharp-code-change-policy` and `csharp-unit-test-policy` skills

## Tone Policy

- Use a strictly professional, factual, and neutral tone in all user-facing responses.
- Do not use jokes, humor, metaphors, playful analogies, emojis, GIFs, banter, or conversational filler.
- Avoid motivational hype or theatrical phrasing.
- If wording sounds informal or playful, rewrite it in neutral business language.

## C# Toolchain (run in this exact order)

1. **Format**: `dotnet tool run csharpier .` (or `csharpier .` if installed globally)
2. **Analyze**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type-check**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

If any step fails, fix and restart from step 1.

## Key Skills Reference

### Background skills (auto-applied by Claude)

- `policy-compliance-order` — mandatory policy reading order and hard constraints
- `atomic-plan-contract` — atomic plan format, Phase 0, and final QA loop rules
- `acceptance-criteria-tracking` — AC check-off protocol and status summary
- `evidence-and-timestamp-conventions` — ISO-8601 timestamps and evidence artifact locations
- `feature-promotion-lifecycle` — promotion workflow from potential entry to active feature folder
- `csharp-change-budget-router` — budget-first routing for C# work
- `csharp-orchestration-state-machine` — checkpoint and resume protocol for C# orchestration
- `pr-context-artifacts` — PR context artifact locations
- `pr-base-branch-merge-base` — deterministic base branch resolution
- `policy-audit-template-usage` — policy audit artifact creation rules
- `remediation-handoff-atomic-planner` — remediation trigger and atomic_planner handoff
- `skill-canonical-location-audit` — canonical location duplication audit

### Agent persona skills (invoke explicitly)

- `/orchestrator` — language-agnostic end-to-end feature/bug delivery orchestration
- `/csharp-orchestrator` — C#-specific end-to-end orchestration
- `/atomic-planner` — generate phased atomic implementation plans
- `/atomic-executor` — execute atomic plans verbatim with strict task-by-task verification
- `/csharp-atomic-planner` — C#-specific atomic planning
- `/csharp-atomic-executor` — C#-specific atomic execution with csharpier/msbuild/vstest gates
- `/csharp-typed-engineer` — design and implement testable C# code with MSTest coverage
- `/feature-reviewer` — review feature branches; produce policy/code/feature audit artifacts
- `/task-researcher` — deep research into implementation approaches; writes to `artifacts/research/`
- `/make-skill-template` — scaffold new Claude skill files

### User-invocable commands

- `/orchestrate-csharp-work` — run end-to-end C# workflow via csharp-orchestrator
- `/generate-atomic-plan` — generate and validate an atomic implementation plan
- `/review-feature` — review a feature branch and produce audit artifacts
- `/generate-pr` — write a GitHub PR description from PR context artifacts
