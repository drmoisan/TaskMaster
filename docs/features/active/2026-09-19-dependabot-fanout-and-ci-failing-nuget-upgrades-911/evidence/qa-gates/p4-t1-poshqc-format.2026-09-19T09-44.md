# P4-T1 — PoshQC format and derived revert, Batch B close-out

Timestamp: 2026-09-20T00-56

Command: CMD-POSHQC-FORMAT — MCP tool `mcp__drm-copilot__run_poshqc_format`, `workspace_root`
passed as the execution worktree root.

Exact `scan_folders` argument value passed, in both rounds:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

EXIT_CODE: 0 (MCP `ok:true` in both rounds)

This task ran **twice**. Round 1 produced a non-zero rewrite count, which the task text requires to
restart the phase from P4-T1; round 2 produced zero. The record below carries both.

## Round 1

### Before hash set, 38 files

`BEFORE_FILE_COUNT=38`. All 38 SHA-256 hashes were captured over every `.ps1`, `.psm1` and `.psd1`
under the four scan folders, sorted by path. The full set is reproduced here:

```
scripts/dependencies/PackageCompatibility.psm1 6A60198C34846373AF0A3C8B43181F67933BD7F9C2F0F23EE10553E656097271
scripts/dependencies/PackageGraph.psm1 A33C42681F7FE5BF16130E6CA57634041671AEEA70948333311440AB2676DB7D
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
tests/scripts/dependencies/DependabotConfig.Tests.ps1 07515C672CAD94B081DF54A76701185DB254FC093DAA74E6DC8D8A34A0EFA020
tests/scripts/dependencies/PackageCompatibility.Tests.ps1 8DA4035C38D80C8AED74210EF8284966BA33355C206FB1ABF3C4EAA92CA27E47
tests/scripts/dependencies/PackageGraph.Tests.ps1 3081FB429A66B6FC9BD9E45EEAE832A93D4040E493972A267351041E0C232E74
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

### After hash set, round 1

`AFTER_FILE_COUNT=38`. Identical to the before set at 36 of 38 paths. The two that differ:

| Path | Before | After |
|---|---|---|
| `scripts/dependencies/PackageCompatibility.psm1` | `6A60198C…97271` | `76F0DD00DC6208B8585ADF05D7A392BE8DD3CBFCFFD10463E80596774A1BEB57` |
| `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | `07515C67…FA020` | `13FDFE011585D8E72BF3DE5185D9C11D12F1DF36393561AC67711FE673CFD9A9` |

### Derived revert pathspec, round 1

```
hash-difference set = {
  scripts/dependencies/PackageCompatibility.psm1,
  tests/scripts/dependencies/DependabotConfig.Tests.ps1
}
minus spec ## Write Set  =  { }

REVERT-SET: empty
```

Both members of the difference set are members of the spec `## Write Set` — the first under
"Production PowerShell", the second under "Tests" — so the derived set is empty and
CMD-REVERT-OUT-OF-SCOPE-FORMAT was **not run**, exactly as Scope Decision 8 describes for the
empty case. Nothing outside the Write Set was rewritten.

### Rewrite count, round 1

The derived set is empty, so nothing is excluded from the count. The hash-difference count
computed after the (no-op) revert is **2**. That is greater than zero, so the phase restarts from
P4-T1.

### Porcelain captures, round 1

```
pre-revert:   git status --porcelain --untracked-files=all -- scripts/vscode
 M scripts/vscode/Sync-PackageReferences.ps1

post-revert:  git status --porcelain --untracked-files=all -- scripts/vscode
 M scripts/vscode/Sync-PackageReferences.ps1
```

Identical, because no revert ran. Neither capture lists a derived-set member, the derived set
being empty. The one entry shown is the P3-T4 rewrite, which is a Write Set member and is intended
to stay.

### Re-reading the two rewritten files before the restart

Both were re-read before round 2, as the task requires. The formatter's change in each is
confined to pipeline-continuation indentation: in `PackageCompatibility.psm1` the two pipelines
inside `Select-CompatibleAssetFolder` at lines 96 to 102, and in `DependabotConfig.Tests.ps1` the
`Should` continuations and the `ForEach-Object` continuation inside two `It` blocks. No statement,
assertion, literal or identifier changed in either file. Line counts are 172 and 264, and both
files carry 0 non-ASCII bytes.

**Citation re-derivation, per gate rule 14.** The hash-difference set was non-empty and one of its
members carries line citations this run has already asserted against, so those citations were
re-derived rather than assumed:

| Citation | Recorded at | Re-derived value | Status |
|---|---|---|---|
| `netstandard2.1` literal in `PackageCompatibility.psm1` | P3-T1, line 21 | line 21 | unchanged |
| `$script:ConsumableAssetFolder` collection span | P3-T1, "lines 41 to 64" | lines 42 to 65 | **corrected** |

The span correction is an off-by-one in the P3-T1 artifact's prose, not a consequence of the
format run: the formatter touched only lines 96 to 102, below the collection, and the file's line
count is unchanged at 172, so the collection cannot have moved. The P3-T1 artifact has been
amended in place with the re-derived span and a note recording the correction. The P0-T17
analyzer baseline tuples all name files under `scripts/vscode`, none of which the formatter
rewrote, so P4-T2's tuple comparison is unaffected.

## Round 2

### Before hash set, round 2

`R2_BEFORE_FILE_COUNT=38`, equal to round 1's after set at all 38 paths, including
`scripts/dependencies/PackageCompatibility.psm1` at
`76F0DD00DC6208B8585ADF05D7A392BE8DD3CBFCFFD10463E80596774A1BEB57` and
`tests/scripts/dependencies/DependabotConfig.Tests.ps1` at
`13FDFE011585D8E72BF3DE5185D9C11D12F1DF36393561AC67711FE673CFD9A9`.

### After hash set, round 2

`R2_AFTER_FILE_COUNT=38`. A line-by-line comparison of the two captures produced a single
difference, the label line itself:

```
1c1
< R2_BEFORE_FILE_COUNT=38
---
> R2_AFTER_FILE_COUNT=38
```

No path line differs. All 38 hashes are identical across the round-2 invocation.

### Derived revert pathspec, round 2

```
hash-difference set = { }
minus spec ## Write Set  =  { }

REVERT-SET: empty
```

CMD-REVERT-OUT-OF-SCOPE-FORMAT was not run.

### Rewrite count, round 2

**0.** The phase does not restart again.

### Porcelain captures, round 2

```
pre-revert and post-revert:  git status --porcelain --untracked-files=all -- scripts/vscode
 M scripts/vscode/Sync-PackageReferences.ps1
```

No derived-set member is listed; the derived set is empty.

## Non-vacuity

`Formatted N files` was not used as the rewrite count anywhere, per gate rule 6, and
`MCP Result: ok:true` is not asserted as an acceptance condition. The formatter is demonstrably
live in this run rather than a tool that failed to resolve its scan set: round 1 rewrote two real
files and their hashes changed, which is a stronger control than the bounded reverted experiment
P0-T15 needed. The round-2 zero is therefore a converged formatter, not a formatter that ran on
nothing — and the 38-file population is reported identically in all four captures.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Both hash sets recorded | before and after, each round | 38 paths each, all four captures recorded | PASS |
| Derived set recorded explicitly, including the empty case | explicit | `REVERT-SET: empty` in both rounds, with the derivation shown | PASS |
| Rewrite count computed after the revert, excluding derived-set members | integer | round 1: **2**; round 2: **0** | PASS |
| Post-revert capture lists no derived-set member | none | neither capture lists one; the set is empty in both rounds | PASS |
| Exact `scan_folders` value recorded | verbatim | recorded, identical in both rounds | PASS |
| Non-zero rewrite count restarts the phase | restart | round 1 was 2 and the phase restarted from P4-T1 | PASS |

Output Summary: CMD-POSHQC-FORMAT over the four explicitly supplied `scan_folders` ran twice.
Round 1 rewrote **2 of 38** files — `scripts/dependencies/PackageCompatibility.psm1` and
`tests/scripts/dependencies/DependabotConfig.Tests.ps1`, both pipeline-continuation indentation
only, both members of the spec `## Write Set` — so the derived revert set was empty
(`REVERT-SET: empty`), no revert ran, and the non-zero rewrite count restarted the phase. The two
files were re-read and the one live line citation over them was re-derived: the `netstandard2.1`
literal is still at line 21, and the preference collection spans lines 42 to 65, which corrects an
off-by-one in the P3-T1 artifact that the formatter did not cause. Round 2 rewrote **0 of 38**
files, with all 38 hashes identical before and after and the derived set empty again. The
`scripts/vscode` porcelain capture shows only `Sync-PackageReferences.ps1`, the intended P3-T4
edit, in every capture.
