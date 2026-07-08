# Phase 0 — Policy Instructions Read

Timestamp: 2026-06-12T18-20

Policy Order: Per `.claude/skills/policy-compliance-order/SKILL.md`:
1. CLAUDE.md (standing instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/powershell.md (PowerShell-specific rules — language in scope)

Files read (in order):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/powershell.md

Key constraints captured for this work:
- PowerShell toolchain order: PoshQC format -> PSScriptAnalyzer analyze -> Pester test (type checking N/A).
- Wrapper-seam pattern mandatory: `Invoke-<Tool>Exe -<Tool>Args <string[]>`; parameter name must NOT be `Args`.
- Mock only the wrapper seam; never mock the real `vstest.console.exe`/`dotnet-coverage`.
- Mock signature parity required (`param([string[]]$VsTestArgs)`).
- Tests must be deterministic and produce identical results in Terminal and VS Code Test Explorer.
- New code coverage target >= 90%; no coverage regression on changed lines.
- Per-batch cap: max 3 production files and 3 test files.
- Files must remain under 500 lines.
