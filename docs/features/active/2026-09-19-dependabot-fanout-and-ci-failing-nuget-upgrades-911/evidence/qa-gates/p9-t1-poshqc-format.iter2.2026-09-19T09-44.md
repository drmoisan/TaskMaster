# P9-T1 — PowerShell QA step 1, PoshQC format (iteration 2)

Timestamp: 2026-09-20T09-44

Iteration 2 exists because P9-T2 iteration 1 failed: the analyzer reported 18 findings against the
expected 13, the five extra findings all in `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`,
which is one of the fifteen files this change owns. The loop restarted from P9-T1 after that file was
corrected. Iteration 1's artifact is retained at
`evidence/qa-gates/p9-t1-poshqc-format.iter1.2026-09-19T09-44.md` and is not overwritten.

Command:

```
mcp__drm-copilot__run_poshqc_format
  workspace_root = <execution-worktree-root>
  scan_folders   = ["scripts/dependencies","scripts/vscode","tests/scripts/dependencies","tests/scripts/vscode"]
```

The exact `scan_folders` argument value passed was
`["scripts/dependencies","scripts/vscode","tests/scripts/dependencies","tests/scripts/vscode"]`.

EXIT_CODE: 0

MCP Result: `ok:true`, recorded as context only.

Output Summary: the formatter rewrote no file. 46 PowerShell files were hashed before and after the
run and the two hash sets are byte-identical, so the rewrite count is 0 and the derived revert set is
empty.

REVERT-SET: empty

REWRITE-COUNT-AFTER-REVERT: 0

## Derivation of the revert pathspec

The derived set is the set of paths whose `Get-FileHash -Algorithm SHA256` changed across the format
invocation, minus every member of the spec `## Write Set`. The changed set is empty, so the derived
set is empty and CMD-REVERT-OUT-OF-SCOPE-FORMAT was not run.

## `git status --porcelain --untracked-files=all -- scripts/vscode`

Pre-revert capture: empty. Post-revert capture: empty. No revert was run, so the two are identical.
The post-revert capture lists no derived-set member, and no path present in the pre-revert capture is
absent from the post-revert capture.

## Before hash set — 46 files

```
89D1839C96EDB4E8E3AF8873E574FE876763B85EA01C5B57179C4E2638FCC274  scripts/dependencies/AnalyzerItemRepair.psm1
261586AA4FEA2AA5E72F26E8E3D4988098DBF3163A4A83BF5E288733503C7EAE  scripts/dependencies/ConsistencyVerifier.psm1
76F0DD00DC6208B8585ADF05D7A392BE8DD3CBFCFFD10463E80596774A1BEB57  scripts/dependencies/PackageCompatibility.psm1
A33C42681F7FE5BF16130E6CA57634041671AEEA70948333311440AB2676DB7D  scripts/dependencies/PackageGraph.psm1
DA3D2995D32FBB89356611624DCE967D9714013969653CAFF314A400355EDD4C  scripts/dependencies/ProjectConsistency.psm1
2ECF35861E2E7DBB0A3596533907AE6619FBB87876A617A7E4DE254420ED3866  scripts/dependencies/Repair-PackageManifestConsistency.ps1
5D8097B77D58105B5157F7E8E36CBCAA9DFD04B85F4F3D0797FE6E3FA34767C0  scripts/vscode/Install-RepoDotNetSdk.ps1
D320DED8A3EC40EC1A4890D1796DE7EABA3257C4F81AC89272D5D06F74112611  scripts/vscode/Invoke-MSTest.ps1
0622CB7C5E6C31DDAF476D7CCF589D2FB6B385FE2C2F4148671DEBA96D44C9CA  scripts/vscode/Invoke-MSTest.TrxSummary.ps1
D46E707423D52F2B1DED5B2207039A93195AA7EAD29F2BD0BC93E7913A1A2BFD  scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
6FCF7CAFFA1496A956D275F01A0EE6613EBCD0C2CA6BE0D70B24025147A16E4F  scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1
FACE0E2BAD8C773878D8FFE8171C5720EB9D627807E87E951E387927A76568CD  scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
A6F057A086E4CC9462CFC2A6DDBB253C0508C469B717839496DE6193D5FEA5FE  scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1
244D1DD507FA0AB1A3E7D559AC9505B14FBF8E727A59E5A3BF00F04E118430B2  scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
4D9263A8EB7A81C3EB4BE4F746C6E53070F38BFB29E07EB954AAF53F5C3F184E  scripts/vscode/Invoke-MSTestWithCoverage.ps1
ABA0BB53CFD80E63E714316CF12775C14DB5112C1520F49BCA0EED426F44E2EC  scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
BA3A1A2FEA7F95E87D7D5DAC6A2C94DE75951D24E06F9B6131E6A60276E7A0CF  scripts/vscode/Invoke-Restore.ps1
239D1D930DF934716A9606301492F73F24F29BA08BF96867A54E807814CB7487  scripts/vscode/Invoke-VSBuild.ps1
3A5FF84FA42904342BE622267B2D8122D1B0F8660018AD331898CEF5FA628DC3  scripts/vscode/Sync-PackageReferences.ps1
E1B8C63C98607EEFF2C69CD6E9E31872676E087AD6E876E5CDE994D134092756  scripts/vscode/TestProcessCleanup.ps1
02C3864E5565CE4F33E71DFDA1BBF6B6CE9F223AFAAAD2653870F405DD8AECFF  tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1
C0069CD548405DDE6FA5E88583B1BA23CD32D0CCD8C462CFED58F1C29EF1B68D  tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
6525202ACF5873E278AE4C323C45F5137AEFF24572D077281B9355B08D2D5999  tests/scripts/dependencies/DependabotConfig.Tests.ps1
8DA4035C38D80C8AED74210EF8284966BA33355C206FB1ABF3C4EAA92CA27E47  tests/scripts/dependencies/PackageCompatibility.Tests.ps1
3081FB429A66B6FC9BD9E45EEAE832A93D4040E493972A267351041E0C232E74  tests/scripts/dependencies/PackageGraph.Tests.ps1
F70209773A3A5EAF485E3DED4CBDFE1B0CB094587BD97E13654F6C5E7847B51B  tests/scripts/dependencies/ProjectConsistency.Tests.ps1
0BF00C619CC50264026792EE6A0E08240D7E344001C9FDFF9E37723C652666DE  tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1
687201EEC643DBD6FD2FB735B501E0C43D1E135CFAE86CA515BAD6B34DA3D282  tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1
91D8A9C1B724705DC28AB12B302EEB940FAE1A6FA6C49321B4E96977C38D29DC  tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1
E7ADA9B3B929558921CD6F504DC9045D387EFEC6413E1CF036E2CBE7FF6CFB02  tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
96F40CCF0172349F663D8009DC85DBD0C9E90A1F07D3FC1F217D42582F0518AD  tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
61400DE13A6B93D4FA4049891659A3BDE957C1DA0AAA22B863A42E575C5BAE83  tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
AEEDBA3F3D7EF0B962F05AB6AD3EE275D5B9D75738B3937117231A1D6FD5643C  tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1
A822876D33EE47B23F32487E7D794DE032AE7B6A25FEF7682EB1482FFA89E098  tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
BBB2BE59D45F132A6F974E4AE1D34BFF0D72F9D640E5BFB547F65D52F0A867F2  tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
7F814A5F2CA9FC9498C8E056C4D8E6A343B3F8B177FA98B9C5F10D0659092C50  tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1
90D6BC4017D0D5736781741F210382679C286CD624B70E2E2400C2B0883D6365  tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
034EEE7EF575950551873B96836464F2C88971ABEDD12904DA10879324554D41  tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1
01BF5D7D45CF09544F7339AA63CFBD46B64D46BA0CA447D1784198B230DBAEE0  tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
433E246EB325F55A462E78487D10C22698C5EEAE87410C72B7EA59377236C9EA  tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
DD0C630F65FF27A02851199F41A0C9F3F9503A19DF04448844592948E28140D3  tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
D53B7DEF7681D3D101B8B5C09A9F3143B9A3314F9943C6D64841C74F9D9C0570  tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
89F5595BE8B2737FA140F8926C2D88F1CF3919622EDC2D0D461E8A817AA5C6E4  tests/scripts/vscode/Invoke-Restore.Tests.ps1
72ACD227F2CCE441A2B215DF93550BCB17AEF8A27FFE2E4E86E20F3ED05DB7E7  tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
CD465C973E473FA5AFA7121B38ABC8F29ED193D0B180EBF8FDA5BEB3E29BADB7  tests/scripts/vscode/Sync-PackageReferences.Tests.ps1
0F9ACFDD52927191D8597E391BD947F12D48CD9A4259B56F8C0DD79F30AB067A  tests/scripts/vscode/TestProcessCleanup.Tests.ps1
```

## After hash set — 46 files

```
89D1839C96EDB4E8E3AF8873E574FE876763B85EA01C5B57179C4E2638FCC274  scripts/dependencies/AnalyzerItemRepair.psm1
261586AA4FEA2AA5E72F26E8E3D4988098DBF3163A4A83BF5E288733503C7EAE  scripts/dependencies/ConsistencyVerifier.psm1
76F0DD00DC6208B8585ADF05D7A392BE8DD3CBFCFFD10463E80596774A1BEB57  scripts/dependencies/PackageCompatibility.psm1
A33C42681F7FE5BF16130E6CA57634041671AEEA70948333311440AB2676DB7D  scripts/dependencies/PackageGraph.psm1
DA3D2995D32FBB89356611624DCE967D9714013969653CAFF314A400355EDD4C  scripts/dependencies/ProjectConsistency.psm1
2ECF35861E2E7DBB0A3596533907AE6619FBB87876A617A7E4DE254420ED3866  scripts/dependencies/Repair-PackageManifestConsistency.ps1
5D8097B77D58105B5157F7E8E36CBCAA9DFD04B85F4F3D0797FE6E3FA34767C0  scripts/vscode/Install-RepoDotNetSdk.ps1
D320DED8A3EC40EC1A4890D1796DE7EABA3257C4F81AC89272D5D06F74112611  scripts/vscode/Invoke-MSTest.ps1
0622CB7C5E6C31DDAF476D7CCF589D2FB6B385FE2C2F4148671DEBA96D44C9CA  scripts/vscode/Invoke-MSTest.TrxSummary.ps1
D46E707423D52F2B1DED5B2207039A93195AA7EAD29F2BD0BC93E7913A1A2BFD  scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
6FCF7CAFFA1496A956D275F01A0EE6613EBCD0C2CA6BE0D70B24025147A16E4F  scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1
FACE0E2BAD8C773878D8FFE8171C5720EB9D627807E87E951E387927A76568CD  scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
A6F057A086E4CC9462CFC2A6DDBB253C0508C469B717839496DE6193D5FEA5FE  scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1
244D1DD507FA0AB1A3E7D559AC9505B14FBF8E727A59E5A3BF00F04E118430B2  scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
4D9263A8EB7A81C3EB4BE4F746C6E53070F38BFB29E07EB954AAF53F5C3F184E  scripts/vscode/Invoke-MSTestWithCoverage.ps1
ABA0BB53CFD80E63E714316CF12775C14DB5112C1520F49BCA0EED426F44E2EC  scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
BA3A1A2FEA7F95E87D7D5DAC6A2C94DE75951D24E06F9B6131E6A60276E7A0CF  scripts/vscode/Invoke-Restore.ps1
239D1D930DF934716A9606301492F73F24F29BA08BF96867A54E807814CB7487  scripts/vscode/Invoke-VSBuild.ps1
3A5FF84FA42904342BE622267B2D8122D1B0F8660018AD331898CEF5FA628DC3  scripts/vscode/Sync-PackageReferences.ps1
E1B8C63C98607EEFF2C69CD6E9E31872676E087AD6E876E5CDE994D134092756  scripts/vscode/TestProcessCleanup.ps1
02C3864E5565CE4F33E71DFDA1BBF6B6CE9F223AFAAAD2653870F405DD8AECFF  tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1
C0069CD548405DDE6FA5E88583B1BA23CD32D0CCD8C462CFED58F1C29EF1B68D  tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
6525202ACF5873E278AE4C323C45F5137AEFF24572D077281B9355B08D2D5999  tests/scripts/dependencies/DependabotConfig.Tests.ps1
8DA4035C38D80C8AED74210EF8284966BA33355C206FB1ABF3C4EAA92CA27E47  tests/scripts/dependencies/PackageCompatibility.Tests.ps1
3081FB429A66B6FC9BD9E45EEAE832A93D4040E493972A267351041E0C232E74  tests/scripts/dependencies/PackageGraph.Tests.ps1
F70209773A3A5EAF485E3DED4CBDFE1B0CB094587BD97E13654F6C5E7847B51B  tests/scripts/dependencies/ProjectConsistency.Tests.ps1
0BF00C619CC50264026792EE6A0E08240D7E344001C9FDFF9E37723C652666DE  tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1
687201EEC643DBD6FD2FB735B501E0C43D1E135CFAE86CA515BAD6B34DA3D282  tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1
91D8A9C1B724705DC28AB12B302EEB940FAE1A6FA6C49321B4E96977C38D29DC  tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1
E7ADA9B3B929558921CD6F504DC9045D387EFEC6413E1CF036E2CBE7FF6CFB02  tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
96F40CCF0172349F663D8009DC85DBD0C9E90A1F07D3FC1F217D42582F0518AD  tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
61400DE13A6B93D4FA4049891659A3BDE957C1DA0AAA22B863A42E575C5BAE83  tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
AEEDBA3F3D7EF0B962F05AB6AD3EE275D5B9D75738B3937117231A1D6FD5643C  tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1
A822876D33EE47B23F32487E7D794DE032AE7B6A25FEF7682EB1482FFA89E098  tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
BBB2BE59D45F132A6F974E4AE1D34BFF0D72F9D640E5BFB547F65D52F0A867F2  tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
7F814A5F2CA9FC9498C8E056C4D8E6A343B3F8B177FA98B9C5F10D0659092C50  tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1
90D6BC4017D0D5736781741F210382679C286CD624B70E2E2400C2B0883D6365  tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
034EEE7EF575950551873B96836464F2C88971ABEDD12904DA10879324554D41  tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1
01BF5D7D45CF09544F7339AA63CFBD46B64D46BA0CA447D1784198B230DBAEE0  tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
433E246EB325F55A462E78487D10C22698C5EEAE87410C72B7EA59377236C9EA  tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
DD0C630F65FF27A02851199F41A0C9F3F9503A19DF04448844592948E28140D3  tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
D53B7DEF7681D3D101B8B5C09A9F3143B9A3314F9943C6D64841C74F9D9C0570  tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
89F5595BE8B2737FA140F8926C2D88F1CF3919622EDC2D0D461E8A817AA5C6E4  tests/scripts/vscode/Invoke-Restore.Tests.ps1
72ACD227F2CCE441A2B215DF93550BCB17AEF8A27FFE2E4E86E20F3ED05DB7E7  tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
CD465C973E473FA5AFA7121B38ABC8F29ED193D0B180EBF8FDA5BEB3E29BADB7  tests/scripts/vscode/Sync-PackageReferences.Tests.ps1
0F9ACFDD52927191D8597E391BD947F12D48CD9A4259B56F8C0DD79F30AB067A  tests/scripts/vscode/TestProcessCleanup.Tests.ps1
```

## Acceptance

| Criterion | Observed | Result |
|---|---|---|
| Both hash sets recorded | 46 entries each | PASS |
| Derived set recorded explicitly, empty case as `REVERT-SET: empty` | `REVERT-SET: empty` | PASS |
| Hash-difference rewrite count after the revert equals 0 | 0 | PASS |
| Post-revert capture lists no derived-set member | capture empty | PASS |

## File-size ceiling observation

`tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` grew from 374 to 382 lines
with the analyzer correction, well inside the 500-line ceiling. The formatter rewrote no file, so
`scripts/dependencies/Repair-PackageManifestConsistency.ps1` and
`scripts/dependencies/ConsistencyVerifier.psm1`, which sit nearest the ceiling, did not move and no
content was removed to fit.
