# P0-T10 Baseline PowerShell format step

Timestamp: 2026-10-02T03-08
Command: git -C <execution-worktree-root> hash-object <14 paths> (CMD-HASHSET before); MCP mcp__drm-copilot__run_poshqc_format with workspace_root `<execution-worktree-root>` and scan_folders `["scripts/dependencies","tests/scripts/dependencies"]`; git -C <execution-worktree-root> hash-object <14 paths> (CMD-HASHSET after); git -C <execution-worktree-root> status --porcelain --untracked-files=all -- scripts/dependencies tests/scripts/dependencies
EXIT_CODE: 0

Payload (verbatim, worktree root replaced per C4):

```text
{"ok":true,"tool":"run_poshqc_format","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC format against '<execution-worktree-root>' with 2 selected scan folder(s)."}
```

EXIT_CODE derivation (C3): `ok` is true, so 0.

HASH set before and after (identical; the 14 hashes were obtained in one `git hash-object` invocation each time, sorted by path):

```text
HASH scripts/dependencies/AnalyzerItemRepair.psm1 97e07385e44d57462975875d03dc639a0cf37448
HASH scripts/dependencies/ConsistencyVerifier.psm1 a3d71fac2681531816fc4d9f6062e4a47fbf7a84
HASH scripts/dependencies/PackageCompatibility.psm1 0a7a10463911d2e4d9bea73522a6b1bb4aa97c56
HASH scripts/dependencies/PackageGraph.psm1 ca1579f7eb17066fe86578a6b5f04c92e3682a18
HASH scripts/dependencies/ProjectConsistency.psm1 334c1b5bf74d0d5f688aae7532f7498246b0a6d4
HASH scripts/dependencies/Repair-PackageManifestConsistency.ps1 6d13903d57eab1d0f751ff0452c989103500bf35
HASH tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1 8c9be4625935462e5c1d59200dcaaf3978b0277b
HASH tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 26e5d3e8be66c729b2d67d423e93729d3ed0c06f
HASH tests/scripts/dependencies/DependabotConfig.Tests.ps1 b740c118927caf1ef1aed74250602b2ea43f09d7
HASH tests/scripts/dependencies/PackageCompatibility.Tests.ps1 d5409dff1556ef0770f98b51db1c2f1d6abd9428
HASH tests/scripts/dependencies/PackageGraph.Tests.ps1 2977c2c7527b120ab807e171b102366b924cbf89
HASH tests/scripts/dependencies/ProjectConsistency.Tests.ps1 57dc48c5439f01b21bc0948fe9369451108ec0d9
HASH tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1 7c8d6b697a99ad9c48d79448806a6cf629dafb7f
HASH tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 4fbea957f91aa94fb90c03e829a2783208879a0b
```

Porcelain over the two folders after the format call: empty (no output).

Acceptance: `ok` true with the 2-folder summary literal; the two hash sets are identical (14 of 14 hashes equal); porcelain over the two folders is empty. No FORMAT-DRIFT-REVERTED entries.

Output Summary: PoshQC format ok true, 2 scan folders; hash sets identical before and after; porcelain empty; the formatter rewrote no file.
