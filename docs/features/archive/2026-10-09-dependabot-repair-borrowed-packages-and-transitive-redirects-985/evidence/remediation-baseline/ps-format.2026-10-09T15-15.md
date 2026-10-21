# PowerShell Baseline Format (R1, issue #985)

Timestamp: 2026-10-09T15-15
Command: mcp__drm-copilot__run_poshqc_format workspace_root = WORKSPACE-ROOT, scan_folders = [scripts/dependencies, tests/scripts/dependencies]
EXIT_CODE: 0
Output Summary:
- MCP ok: true
- MCP summary (workspace root substituted per C3): "Ran bundled PoshQC format against 'WORKSPACE-ROOT' with 2 selected scan folder(s)."
- git status --porcelain: identical before and after (12 lines each).
- git hash-object --no-filters, identical before and after:
  - scripts/dependencies/BindingRedirectSync.psm1 ef8eee178ce3296a872281467320860f759ffd27
  - scripts/dependencies/Repair-PackageManifestConsistency.ps1 2b845c5aad4952f581e26e66284e1901217c0198
  - tests/scripts/dependencies/BindingRedirectSync.Tests.ps1 61b018e7270d5ca11d849a0300fe8514457c6533
  - tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 d82cfd5c6de703e82425c7be5cf2d078b071c990
- Result: no rewrite; no BASELINE-FORMAT-DRIFT.
