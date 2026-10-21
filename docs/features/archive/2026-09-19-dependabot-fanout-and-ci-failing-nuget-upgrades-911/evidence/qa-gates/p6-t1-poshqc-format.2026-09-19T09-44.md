# P6-T1 — PoshQC format over the four scan folders, with the derived revert pathspec

Timestamp: 2026-09-19T09-44

Command: MCP tool `mcp__drm-copilot__run_poshqc_format`

`scan_folders` argument value, supplied explicitly and verbatim:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

`workspace_root`: the execution worktree root.

EXIT_CODE: 0

The tool is invoked with `scan_folders` supplied explicitly because it otherwise resolves
its scan set from `config/poshqc-scan.json`, which does not exist in this repository, and
an omitted argument therefore measures nothing.

## The phase restarted twice, as the task provides for

The first pass rewrote **3** files, which is a non-zero rewrite count, so the phase
restarted from P6-T1. The second pass rewrote **0** and the phase proceeded to P6-T2, which
reported 25 findings, twelve of them in Batch C files. Fixing those twelve changed tracked
source, which restarts the toolchain loop at its format step, so P6-T1 ran a **third**
time; it rewrote **0**, establishing that the analyzer fixes are formatter-stable. All
three passes are recorded below. `MCP Result: ok:true` is not asserted as acceptance.

## Pass 1

### Hash set before, 44 files

```
scripts/dependencies/AnalyzerItemRepair.psm1 556CC33D89B1F6A0D0F776545888B7CB9D5F2A9A093BE733D2E86BBA232458EF
scripts/dependencies/ConsistencyVerifier.psm1 6A18F8B92457EC482DD8BA4D013B0ADCF48BD9C30E6B872B703B248A05505BDE
scripts/dependencies/PackageCompatibility.psm1 76F0DD00DC6208B8585ADF05D7A392BE8DD3CBFCFFD10463E80596774A1BEB57
scripts/dependencies/PackageGraph.psm1 A33C42681F7FE5BF16130E6CA57634041671AEEA70948333311440AB2676DB7D
scripts/dependencies/ProjectConsistency.psm1 78CEDFBBA679E6A4F2444EB28EC573B5BA83327526A86CBFB6A779C6FCCD9576
scripts/vscode/Install-RepoDotNetSdk.ps1 5D8097B77D58105B5157F7E8E36CBCAA9DFD04B85F4F3D0797FE6E3FA34767C0
scripts/vscode/Invoke-MSTest.ps1 D320DED8A3EC40EC1A4890D1796DE7EABA3257C4F81AC89272D5D06F74112611
scripts/vscode/Invoke-MSTest.TrxSummary.ps1 0622CB7C5E6C31DDAF476D7CCF589D2FB6B385FE2C2F4148671DEBA96D44C9CA
scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 D46E707423D52F2B1DED5B2207039A93195AA7EAD29F2BD0BC93E7913A1A2BFD
scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 6FCF7CAFFA1496A956D275F01A0EE6613EBCD0C2CA6BE0D70B24025147A16E4F
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 FACE0E2BAD8C773878D8FFE8171C5720EB9D627807E87E951E387927A76568CD
scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 A6F057A086E4CC9462CFC2A6DDBB253C0508C469B717839496DE6193D5FEA5FE
scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 244D1DD507FA0AB1A3E7D559AC9505B14FBF8E727A59E5A3BF00F04E118430B2
scripts/vscode/Invoke-MSTestWithCoverage.ps1 4D9263A8EB7A81C3EB4BE4F746C6E53070F38BFB29E07EB954AAF53F5C3F184E
scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 ABA0BB53CFD80E63E714316CF12775C14DB5112C1520F49BCA0EED426F44E2EC
scripts/vscode/Invoke-Restore.ps1 BA3A1A2FEA7F95E87D7D5DAC6A2C94DE75951D24E06F9B6131E6A60276E7A0CF
scripts/vscode/Invoke-VSBuild.ps1 239D1D930DF934716A9606301492F73F24F29BA08BF96867A54E807814CB7487
scripts/vscode/Sync-PackageReferences.ps1 3A5FF84FA42904342BE622267B2D8122D1B0F8660018AD331898CEF5FA628DC3
scripts/vscode/TestProcessCleanup.ps1 E1B8C63C98607EEFF2C69CD6E9E31872676E087AD6E876E5CDE994D134092756
tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1 02C3864E5565CE4F33E71DFDA1BBF6B6CE9F223AFAAAD2653870F405DD8AECFF
tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 7BC61383E4E2CEA3B72464A4E5423E21B72EB1EE4880289D673467D7FA8774A1
tests/scripts/dependencies/DependabotConfig.Tests.ps1 13FDFE011585D8E72BF3DE5185D9C11D12F1DF36393561AC67711FE673CFD9A9
tests/scripts/dependencies/PackageCompatibility.Tests.ps1 8DA4035C38D80C8AED74210EF8284966BA33355C206FB1ABF3C4EAA92CA27E47
tests/scripts/dependencies/PackageGraph.Tests.ps1 3081FB429A66B6FC9BD9E45EEAE832A93D4040E493972A267351041E0C232E74
tests/scripts/dependencies/ProjectConsistency.Tests.ps1 97B1282ED84A2040ED0A6DFD75564E8140B9A4398D8DBDA374D2105395868460
tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1 687201EEC643DBD6FD2FB735B501E0C43D1E135CFAE86CA515BAD6B34DA3D282
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 91D8A9C1B724705DC28AB12B302EEB940FAE1A6FA6C49321B4E96977C38D29DC
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 E7ADA9B3B929558921CD6F504DC9045D387EFEC6413E1CF036E2CBE7FF6CFB02
tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1 96F40CCF0172349F663D8009DC85DBD0C9E90A1F07D3FC1F217D42582F0518AD
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 61400DE13A6B93D4FA4049891659A3BDE957C1DA0AAA22B863A42E575C5BAE83
tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1 AEEDBA3F3D7EF0B962F05AB6AD3EE275D5B9D75738B3937117231A1D6FD5643C
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 A822876D33EE47B23F32487E7D794DE032AE7B6A25FEF7682EB1482FFA89E098
tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 BBB2BE59D45F132A6F974E4AE1D34BFF0D72F9D640E5BFB547F65D52F0A867F2
tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1 7F814A5F2CA9FC9498C8E056C4D8E6A343B3F8B177FA98B9C5F10D0659092C50
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 90D6BC4017D0D5736781741F210382679C286CD624B70E2E2400C2B0883D6365
tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 034EEE7EF575950551873B96836464F2C88971ABEDD12904DA10879324554D41
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 01BF5D7D45CF09544F7339AA63CFBD46B64D46BA0CA447D1784198B230DBAEE0
tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 433E246EB325F55A462E78487D10C22698C5EEAE87410C72B7EA59377236C9EA
tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 DD0C630F65FF27A02851199F41A0C9F3F9503A19DF04448844592948E28140D3
tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 D53B7DEF7681D3D101B8B5C09A9F3143B9A3314F9943C6D64841C74F9D9C0570
tests/scripts/vscode/Invoke-Restore.Tests.ps1 89F5595BE8B2737FA140F8926C2D88F1CF3919622EDC2D0D461E8A817AA5C6E4
tests/scripts/vscode/Invoke-VSBuild.Tests.ps1 72ACD227F2CCE441A2B215DF93550BCB17AEF8A27FFE2E4E86E20F3ED05DB7E7
tests/scripts/vscode/Sync-PackageReferences.Tests.ps1 CD465C973E473FA5AFA7121B38ABC8F29ED193D0B180EBF8FDA5BEB3E29BADB7
tests/scripts/vscode/TestProcessCleanup.Tests.ps1 0F9ACFDD52927191D8597E391BD947F12D48CD9A4259B56F8C0DD79F30AB067A
```

### Hash set after, 44 files — only the three changed entries differ

```
scripts/dependencies/AnalyzerItemRepair.psm1 28F399AE5EBE271CC9879CC8FF384BD5B3B434F2F9FE09F0D074E5758F8A78AC
scripts/dependencies/ConsistencyVerifier.psm1 57B8ACBE0B01E1C4D1ED89455763804E78A2BCC554C87757C3DE2A0C5D87307B
tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 92136A38989D3DC7D713B318E6E22A8A6DF3773BBA061F0122B4F526B2CFD560
```

The remaining 41 entries are byte-identical to the before set above and are not repeated.

### Derived revert pathspec

The hash-difference set is:

```
scripts/dependencies/AnalyzerItemRepair.psm1
scripts/dependencies/ConsistencyVerifier.psm1
tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
```

All three are members of the spec `## Write Set` — the first two under "Production
PowerShell" and the third under "Tests". The derived pathspec is the difference set minus
every Write Set member, which leaves nothing:

**REVERT-SET: empty**

CMD-REVERT-OUT-OF-SCOPE-FORMAT was therefore not run, which is the behaviour Scope
Decision 8 and the command block specify for an empty derived set.

### Porcelain captures over scripts/vscode

Pre-revert capture: empty.
Post-revert capture: empty.

An empty pre-revert capture is not a failure. It is the truthful observation here: no file
under `scripts/vscode` was rewritten by this pass, so none could appear. The post-revert
capture lists no derived-set member, trivially, because the derived set is empty; and no
path disappeared between the two captures.

### Rewrite count

**3**, computed after the revert as the hash-difference count excluding every derived-set
member. The derived set is empty, so nothing is excluded and all three Write Set members
count. Non-zero, so the phase restarted from P6-T1.

## Pass 2

`scan_folders` identical. Before set: the pass-1 after set above. After set: byte-identical
to it — the join over all 44 entries produced **no** differing pair.

- Hash-difference set: empty.
- **REVERT-SET: empty**; CMD-REVERT-OUT-OF-SCOPE-FORMAT not run.
- Pre-revert and post-revert `git status --porcelain --untracked-files=all -- scripts/vscode`:
  both empty.
- **Rewrite count: 0.** The phase proceeds.

## Pass 3, after the P6-T2 analyzer fixes

`scan_folders` identical. The before set was captured over the same 44 files immediately
after the twelve owned analyzer findings were fixed, and the after set is byte-identical to
it — the join over all 44 entries produced no differing pair.

- Hash-difference set: empty.
- **REVERT-SET: empty**; CMD-REVERT-OUT-OF-SCOPE-FORMAT not run.
- Pre-revert and post-revert `git status --porcelain --untracked-files=all -- scripts/vscode`:
  both empty.
- **Rewrite count: 0.**

## Pass 4, after the P6-T3 coverage cases

P6-T3 measured `ProjectConsistency.psm1` below its at-least-90 per-module clause and four
cases were added to `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` to reach it.
That again changed tracked source, so the loop restarted at its format step a fourth time.

`scan_folders` identical. The before and after sets over the same 44 files are
byte-identical: the join produced no differing pair.

- Hash-difference set: empty.
- **REVERT-SET: empty**; CMD-REVERT-OUT-OF-SCOPE-FORMAT not run.
- `git status --porcelain --untracked-files=all -- scripts/vscode`: empty.
- **Rewrite count: 0.**

The loop then closed with the analyzer at exactly 13 and the test run at
`Passed=268 Failed=0` with every per-module coverage clause met, all in one pass with no
file changed after the format step.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Both hash sets recorded | yes | recorded above for both passes |
| Derived set recorded explicitly, including the empty case | yes | `REVERT-SET: empty` in both passes |
| Rewrite count recorded, computed after the revert, excluding derived-set members | yes | 3 in pass 1, then 0 in passes 2, 3 and 4 |
| Post-revert capture lists no derived-set member | yes | empty capture, empty derived set |
| `scan_folders` argument value recorded exactly | yes | recorded above |

## Gate rule 14: what the rewrite invalidated, and what it did not

Gate rule 14 warns that a format run rewriting a file carrying a positional citation makes
that citation stale. Three consequences were checked rather than assumed:

- **Line counts are unchanged.** All six Batch C files were re-measured immediately after
  pass 1 and every count is identical to the figure P5-T22 recorded: 399, 322, 493, 311,
  375 and 272. The formatter's edits were within-line. P5-T22's audit therefore stands and
  is not re-run.
- **No file carrying a cited line number was rewritten.**
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — whose lines 388, 393-401 and 417-425 are
  cited by gate rule 12, P2-T7 and P9-T7 — has the same hash before and after both passes.
- **The P0-T17 analyzer baseline tuples are unaffected**, because all sixteen baseline
  findings sit in five files outside the Write Set and none of those five was rewritten.
  P6-T2 compares against those tuples and is measured next.

## Batch C files are formatter-stable, unlike Batch A and B

Pass 1 rewrote three of the six Batch C files. That differs from the 0-of-32 result P0-T15
recorded and the 1-of-34 result P2-T1 recorded, and the difference is the population rather
than the tool: those measurements predate these files. Every rewritten file is a Write Set
member, so the revert machinery correctly degraded to a no-op, and pass 2 confirms the
formatter is idempotent over its own output.
