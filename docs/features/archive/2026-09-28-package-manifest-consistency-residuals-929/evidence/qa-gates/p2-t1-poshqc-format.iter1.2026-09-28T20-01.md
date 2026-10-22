# P2-T1 — PowerShell QC step 1, format (iteration 1)

Timestamp: 2026-09-30T10-23
Command: Get-FileHash -Algorithm SHA256 over every .ps1, .psm1 and .psd1 file under scripts/dependencies and tests/scripts/dependencies; MCP mcp__drm-copilot__run_poshqc_format (workspace_root <execution-worktree-root>, scan_folders ["scripts/dependencies","tests/scripts/dependencies"]); re-hash; git status --porcelain --untracked-files=all -- scripts/dependencies tests/scripts/dependencies; @(Get-Content -LiteralPath "scripts/dependencies/ConsistencyVerifier.psm1").Count
EXIT_CODE: 0
Output Summary:
- MCP payload (host prefix replaced): {"ok":true,"tool":"run_poshqc_format","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC format against '<execution-worktree-root>' with 2 selected scan folder(s)."}
- scan_folders: ["scripts/dependencies","tests/scripts/dependencies"]
- Hash sets: 14 entries before and 14 after (6 production, 8 test files); every hash identical
- Write Set rewrite count: 0
- REVERT-SET: empty
- Post-revert porcelain over the two folders: empty
- ConsistencyVerifier.psm1 line count: 499 (equals VERIFIER-LINES)
- No REWRITE-NUMSTAT capture was needed (no rewrite); no commit made; the loop does not restart.

Hash set (identical before and after):
```
scripts\dependencies\AnalyzerItemRepair.psm1 102E3EBCF014F8365C3C65A834EDEC5FC9DAC9B1FFD6477E68D813335DB48F68
scripts\dependencies\ConsistencyVerifier.psm1 00604D4F0731A2B432797CBBF85AE5DABAE1D17190A07DCA2348D272D1FBA920
scripts\dependencies\PackageCompatibility.psm1 89B31603872ED09C8E2DF2E94A597D42BF3A2019D4F49298D6859CEF7CBF41CD
scripts\dependencies\PackageGraph.psm1 1CE9EABA3A43FF2446C3B63FFA53CE501537362C8E6EED6BB9DB4774F15C9BC6
scripts\dependencies\ProjectConsistency.psm1 0E7B005A65B5614A0099F7A7286FE6692F87CC49FF09D749877F832C37D69EFE
scripts\dependencies\Repair-PackageManifestConsistency.ps1 E69F61364EB1640C0FADFBBAE1F418250D2F508E8F26A048BE43F70DD659078A
tests\scripts\dependencies\AnalyzerItemRepair.Tests.ps1 7C4CBC097681F98A629F94AD12D800A7E87652CA773825FA26F621969671D011
tests\scripts\dependencies\ConsistencyVerifier.Tests.ps1 CC9AD3DFE9799F943AE464A04C4E8AA00A389283B510DB8759A1BA474231C719
tests\scripts\dependencies\DependabotConfig.Tests.ps1 3F738E78A77F028BBB97D86BD2B9A33DC8CB94F1ABB31F1DC306413CDE3B3C48
tests\scripts\dependencies\PackageCompatibility.Tests.ps1 0EF34C85EBA0B70C24CC4EA662472FA97F33D20B527120278606B12F6D7010DC
tests\scripts\dependencies\PackageGraph.Tests.ps1 910DD957F6377DD60A9F2EFC26E7597421E2C69505DE21E78E4B087729305EBE
tests\scripts\dependencies\ProjectConsistency.Tests.ps1 BCB15A04404BD3792A1DCB65315DE2B4CD822087B9D3EB3006E8BE9EB81CA761
tests\scripts\dependencies\Repair-PackageManifestConsistency.Tests.ps1 3348C5D17773A9DCC6EEFD3A85EF6874BB96CA9BE8B1F2A85C1EA9CB96756942
tests\scripts\dependencies\RepositoryTreeConsistency.Tests.ps1 B4312DE871FCA6A2E690D9F3B6B9269F7E4631735A87719FC4BFE85919CBBD9A
```
