# Phase 0 — Instructions Read (Issue #181)

Timestamp: 2026-06-08T12-27

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (language-specific rules; C# files in scope)
5. .claude/rules/ci-workflows.md (CI workflow authoring rule)
6. .claude/skills/policy-compliance-order/SKILL.md (policy precedence)

Files Read (explicit list):
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\CLAUDE.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\rules\general-code-change.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\rules\general-unit-test.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\rules\csharp.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\rules\ci-workflows.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\rules\tonality.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\skills\policy-compliance-order\SKILL.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\skills\atomic-plan-contract\SKILL.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\skills\evidence-and-timestamp-conventions\SKILL.md
- <user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\.claude\skills\acceptance-criteria-tracking\SKILL.md

Supporting context read:
- docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/plan.2026-06-08T12-12.md (authoritative plan)
- docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/issue.md (AC source)
- artifacts/research/2026-06-08-csharp-analyzer-stack-feasibility-181.md (feasibility research)

Toolchain availability verified for execution:
- dotnet SDK 10.0.300
- csharpier 1.2.6 via dotnet tool manifest (<user-profile>\repos\TaskMaster-wt-2026-06-08-12-10\dotnet-tools.json)
- MSBuild at C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
- vstest.console.exe at C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
- nuget.exe (standalone NuGet 7.6.0) at <user-profile>\AppData\Local\Temp\nuget.exe (required for packages.config restore; dotnet nuget cannot restore packages.config)
