# Phase 0 — Instructions Read (Policy Evidence)

Timestamp: 2026-06-12T19-22

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/powershell.md (language-specific rule for the in-scope `.ps1` and Pester test files)

Files read (explicit list):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/powershell.md
- .claude/skills/policy-compliance-order/SKILL.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md

Notes:
- `.claude/rules/powershell.md` is the authoritative language-specific rule for the two in-scope scripts
  (`scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`) and the Pester test
  (`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`). Its toolchain (format -> analyze -> Pester
  via PoshQC MCP) is the primary gate for this change.
- csharpier and msbuild analyzer/nullable gates are N/A: no `*.cs`/`*.csproj`/`*.props`/`*.targets` in scope.
- The two `.runsettings` files are XML configuration data, not C# source; csharpier MUST NOT be run on them.
