# PowerShell Baseline Format (P0-T5)

Timestamp: 2026-10-09T14-04
Command: mcp__drm-copilot__run_poshqc_format workspace_root = WORKSPACE-ROOT, scan_folders = [scripts/dependencies, tests/scripts/dependencies]
EXIT_CODE: 0
Output Summary:
- MCP ok: true
- MCP summary: Ran bundled PoshQC format against 'WORKSPACE-ROOT' with 2 selected scan folder(s).
- Porcelain before and after: identical (12 lines, all pre-existing; listed in git-and-tools.2026-10-09T14-04.md).
- scripts/dependencies/Repair-PackageManifestConsistency.ps1 hash before e3029f6ba072f6198c40fe985ebfe0e29c261889, after e3029f6ba072f6198c40fe985ebfe0e29c261889 (identical)
- tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 hash before 1167a0be9a5c30f8a13ec44331b7c06bdcd9369f, after 1167a0be9a5c30f8a13ec44331b7c06bdcd9369f (identical)
- Result: no rewrite; no BASELINE-FORMAT-DRIFT.
