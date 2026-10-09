# PowerShell Lint Gate (P4-T2)

Timestamp: 2026-10-09T14-25
Command: mcp__drm-copilot__run_poshqc_analyze workspace_root = WORKSPACE-ROOT, scan_folders = [scripts/dependencies, tests/scripts/dependencies]
EXIT_CODE: 1
ITERATION: 1
Output Summary:
- MCP ok: false; summary: Command exited with code 1; stderr: PSScriptAnalyzer reported 7 issue(s).
- Diagnosis (Invoke-ScriptAnalyzer default rules over the five Write Set files):
  - scripts/dependencies/BindingRedirectSync.psm1:52 and :55 PSUseOutputTypeCorrectly (Get-ProviderVersion returns Object[] not declared in OutputType)
  - tests/scripts/dependencies/BindingRedirectSync.Tests.ps1:11, :31, :42, :56, :72 PSUseShouldProcessForStateChangingFunctions (helper functions named with the New verb)
- Type checking: not applicable to PowerShell (.claude/rules/powershell.md toolchain step 3).
- Result: FAIL. Loop restart per C7: Get-ProviderVersion returns its values unwrapped with OutputType([string]) and callers wrap with @(); the five test helpers are renamed to the Get verb. Iteration 2 restarts at P4-T1.
