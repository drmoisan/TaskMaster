# P2-T1 Final QC format step, iteration 1 (non-terminal)

Timestamp: 2026-10-02T03-29
Command: git -C <execution-worktree-root> hash-object <16 .ps1/.psm1 paths under scripts/dependencies and tests/scripts/dependencies> (CMD-HASHSET, before); MCP mcp__drm-copilot__run_poshqc_format with workspace_root `<execution-worktree-root>` and scan_folders `["scripts/dependencies","tests/scripts/dependencies"]`; CMD-HASHSET (after)
EXIT_CODE: 0

Payload (verbatim, worktree root replaced per C4):

```text
{"ok":true,"tool":"run_poshqc_format","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC format against '<execution-worktree-root>' with 2 selected scan folder(s)."}
```

EXIT_CODE derivation (C3): payload `ok` true, so 0. The 2-folder summary literal is present.

Files enumerated with the Glob tool at run time: 7 under scripts/dependencies, 9 under tests/scripts/dependencies, 16 in all.

HASH before (sorted by path):

```text
HASH scripts/dependencies/AnalyzerItemRepair.psm1 97e07385e44d57462975875d03dc639a0cf37448
HASH scripts/dependencies/BindingRedirectVerification.psm1 8c1539be3e316e887abd9381abe4e4a93c7f76bc
HASH scripts/dependencies/ConsistencyVerifier.psm1 a3d71fac2681531816fc4d9f6062e4a47fbf7a84
HASH scripts/dependencies/PackageCompatibility.psm1 0a7a10463911d2e4d9bea73522a6b1bb4aa97c56
HASH scripts/dependencies/PackageGraph.psm1 ca1579f7eb17066fe86578a6b5f04c92e3682a18
HASH scripts/dependencies/ProjectConsistency.psm1 334c1b5bf74d0d5f688aae7532f7498246b0a6d4
HASH scripts/dependencies/Repair-PackageManifestConsistency.ps1 6d13903d57eab1d0f751ff0452c989103500bf35
HASH tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1 8c9be4625935462e5c1d59200dcaaf3978b0277b
HASH tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 8a20728b437b6ddc7f5e44e7f15c1184fbf80931
HASH tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 26e5d3e8be66c729b2d67d423e93729d3ed0c06f
HASH tests/scripts/dependencies/DependabotConfig.Tests.ps1 b740c118927caf1ef1aed74250602b2ea43f09d7
HASH tests/scripts/dependencies/PackageCompatibility.Tests.ps1 d5409dff1556ef0770f98b51db1c2f1d6abd9428
HASH tests/scripts/dependencies/PackageGraph.Tests.ps1 2977c2c7527b120ab807e171b102366b924cbf89
HASH tests/scripts/dependencies/ProjectConsistency.Tests.ps1 57dc48c5439f01b21bc0948fe9369451108ec0d9
HASH tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1 7c8d6b697a99ad9c48d79448806a6cf629dafb7f
HASH tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 4fbea957f91aa94fb90c03e829a2783208879a0b
```

HASH after (sorted by path); the two differing entries are marked:

```text
HASH scripts/dependencies/AnalyzerItemRepair.psm1 97e07385e44d57462975875d03dc639a0cf37448
HASH scripts/dependencies/BindingRedirectVerification.psm1 620bc82376f8d9b849b6728cec2c7aabbda60e5b   (changed)
HASH scripts/dependencies/ConsistencyVerifier.psm1 a3d71fac2681531816fc4d9f6062e4a47fbf7a84
HASH scripts/dependencies/PackageCompatibility.psm1 0a7a10463911d2e4d9bea73522a6b1bb4aa97c56
HASH scripts/dependencies/PackageGraph.psm1 ca1579f7eb17066fe86578a6b5f04c92e3682a18
HASH scripts/dependencies/ProjectConsistency.psm1 334c1b5bf74d0d5f688aae7532f7498246b0a6d4
HASH scripts/dependencies/Repair-PackageManifestConsistency.ps1 6d13903d57eab1d0f751ff0452c989103500bf35
HASH tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1 8c9be4625935462e5c1d59200dcaaf3978b0277b
HASH tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 5f67da131c8655530ba8a4e67998d0f153506bcf   (changed)
HASH tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 26e5d3e8be66c729b2d67d423e93729d3ed0c06f
HASH tests/scripts/dependencies/DependabotConfig.Tests.ps1 b740c118927caf1ef1aed74250602b2ea43f09d7
HASH tests/scripts/dependencies/PackageCompatibility.Tests.ps1 d5409dff1556ef0770f98b51db1c2f1d6abd9428
HASH tests/scripts/dependencies/PackageGraph.Tests.ps1 2977c2c7527b120ab807e171b102366b924cbf89
HASH tests/scripts/dependencies/ProjectConsistency.Tests.ps1 57dc48c5439f01b21bc0948fe9369451108ec0d9
HASH tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1 7c8d6b697a99ad9c48d79448806a6cf629dafb7f
HASH tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 4fbea957f91aa94fb90c03e829a2783208879a0b
```

REWRITE: scripts/dependencies/BindingRedirectVerification.psm1
REWRITE: tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1
REWRITE-COUNT: 2

Both rewritten files are Write Set files (new files of this item, committed at HEAD 45e3f8a2e). `git diff -U0` against the working tree shows only continuation-line indentation changes: one line in the module (`ForEach-Object { [string]$_ })` indented 4 further spaces) and three lines in the test file (lines 251, 281 and 286: `Where-Object` / `ForEach-Object` continuation lines indented 4 further spaces). No file outside the Write Set was rewritten. This iteration is non-terminal; the loop restarts at P2-T1 as iteration 2 (P2-T2 and P2-T3 are not run on this iteration).

Output Summary: PoshQC format ok true; 2 of 16 hash entries changed (the two new Write Set files, indentation-only rewrites); REWRITE-COUNT 2, so the iteration is non-terminal and the loop restarts as iteration 2.
