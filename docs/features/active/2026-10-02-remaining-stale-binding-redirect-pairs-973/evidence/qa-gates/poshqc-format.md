# P4-T1 PowerShell format (final PowerShell pass, iteration 1)

Timestamp: 2026-10-06T18-22
Command: MCP mcp__drm-copilot__run_poshqc_format workspace_root=<execution-worktree-root> scan_folders=["scripts/dependencies","tests/scripts/dependencies"], bracketed by CMD-HASHSET (git -C <execution-worktree-root> hash-object --no-filters -- <16 paths>) before and after, then git -C <execution-worktree-root> status --porcelain --untracked-files=all -- scripts/dependencies tests/scripts/dependencies
EXIT_CODE: 0
Output Summary: ok true with the 2-folder summary literal. The before and after hash sets are identical (16 of 16), so the formatter rewrote nothing (FORMAT-REWROTE: none). Porcelain over both folders is empty. Test file 402 lines, 402 CR (no CRLF strip).

## MCP payload (C3, C4)

    {"ok":true,"tool":"run_poshqc_format","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC format against '<execution-worktree-root>' with 2 selected scan folder(s)."}

## CMD-HASHSET before

HASH scripts/dependencies/AnalyzerItemRepair.psm1 aebe9f76f985d7b18a97ebe2ec10cb9e6496ef01
HASH scripts/dependencies/BindingRedirectVerification.psm1 51d6664b281cad0b6c8cd01e78c6bc8491a75862
HASH scripts/dependencies/ConsistencyVerifier.psm1 6d2bf3e946cb3094e322b3e5830898d8426c2820
HASH scripts/dependencies/PackageCompatibility.psm1 9b94fc1a2d436ff08fde1f697cf6a2ddcf7b5366
HASH scripts/dependencies/PackageGraph.psm1 a87251583b7e0f1c64127a175fd0d123ecc3d62d
HASH scripts/dependencies/ProjectConsistency.psm1 f589781ed85e6daaa73047153cdb17279ba8a2f1
HASH scripts/dependencies/Repair-PackageManifestConsistency.ps1 e3029f6ba072f6198c40fe985ebfe0e29c261889
HASH tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1 7b091ce5ebd081a5ee96c2e5c2e67a8da9e8ff2e
HASH tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 bd93c01d5b587b8fe67f16f1efa3562b3f89ee7d
HASH tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 32b67ee7ecf784f42517a2abe6e377fa0c3c1631
HASH tests/scripts/dependencies/DependabotConfig.Tests.ps1 6301e6aa0809ef91acffbf1f59615a51b5f7d247
HASH tests/scripts/dependencies/PackageCompatibility.Tests.ps1 2f0af653645bc54f548dca84279cfc3cab7b6f25
HASH tests/scripts/dependencies/PackageGraph.Tests.ps1 dbd9939523608df2a3c4c2e2870b0d7572c26d76
HASH tests/scripts/dependencies/ProjectConsistency.Tests.ps1 e2ee9d764cc09c26284b1888a5b808c0a7d439af
HASH tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1 6e522d17d2c91be83179845658d1c65799bb4dc4
HASH tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 1167a0be9a5c30f8a13ec44331b7c06bdcd9369f

## CMD-HASHSET after

Identical to the before set, line for line (same 16 hashes in the same order).

HASHSETS-IDENTICAL: True
FORMAT-REWROTE: none
PORCELAIN: (empty)
TESTFILE-LINES: 402
TESTFILE-CR: 402
EOL-NORMALISED: no
