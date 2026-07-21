# Phase 0 — Instructions Read

Timestamp: 2026-07-16T03-32

Policy Order:
1. CLAUDE.md (standing instructions; C# toolchain order, MSTest/Moq/FluentAssertions, coverage regime)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/quality-tiers.md (T1-T4 module rigor tiers; uniform coverage gates)
5. .claude/rules/csharp.md (C#-specific toolchain and coding standards)

Files Read (explicit list):
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a198a222a991ea783\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a198a222a991ea783\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a198a222a991ea783\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a198a222a991ea783\.claude\rules\quality-tiers.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a198a222a991ea783\.claude\rules\csharp.md

Supporting skills read: policy-compliance-order, atomic-plan-contract, evidence-and-timestamp-conventions, acceptance-criteria-tracking (provided via command context).

Key constraints noted:
- net48 target: no record / record struct / init accessor (CS0518 under TreatWarningsAsErrors). Use plain readonly struct with constructor + get-only auto-properties (precedent ResourceTimingRow in UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs).
- UtilitiesCS.csproj and UtilitiesCS.Test.csproj are legacy non-SDK projects with explicit <Compile Include> entries (no glob). Every new .cs file must be wired explicitly.
- Type-check build uses /p:TreatWarningsAsErrors=true, which promotes analyzer warnings to errors; new files must be warning-clean.
- Coverage regime: stricter of CLAUDE.md (80% floor, >=90% new) and general-unit-test.md (>=85% line, >=75% branch). Target >=90% line on new members with branch coverage of empty/all-zero/tie/topN paths; no reduction on changed lines; no production file excluded.
- Tests: MSTest + Moq + FluentAssertions only. Do not exercise AddBayesianSuggestionsAsync (COM/model-bound).

Environment note (toolchain invocation adaptation):
- No Visual Studio / msbuild.exe / vstest.console.exe on this host. Repo-local .NET SDK 8.0.205 installed via scripts/vscode/Install-RepoDotNetSdk.ps1 into .dotnet-sdk/.
- Format tool: csharpier local tool (dotnet-tools.json, v1.2.6) invoked as `dotnet csharpier <subcommand>` (v1 subcommand syntax: check/format).
- Analyzer/type-check builds: `dotnet msbuild TaskMaster.sln ...` (repo SDK MSBuild 17.9.8), using dash-form switches with MSYS_NO_PATHCONV=1 in git-bash.
- Tests + coverage: `dotnet vstest` wrapped by `dotnet-coverage collect --output-format cobertura` (dotnet-coverage global tool 18.5.2) to produce a readable Cobertura report for per-class line %.
- The canonical commands recorded in each command-step artifact are the plan-specified forms; the adapted actual invocation is recorded alongside.
