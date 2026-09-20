# P2-T1 — PoshQC format over the four scan folders

Timestamp: 2026-09-19T14-46

Command: MCP tool `mcp__drm-copilot__run_poshqc_format`, invoked three times.

`workspace_root`: `C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911`

Exact `scan_folders` argument value, supplied explicitly on both invocations:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

The argument is supplied explicitly rather than omitted because the tool resolves its scan set from
`config/poshqc-scan.json`, which does not exist in this repository, so an omitted `scan_folders`
measures nothing.

EXIT_CODE: 0

`MCP Result: ok:true` is recorded as returned but is **not** asserted, per the Command Reference.

## The task restarted twice

The first invocation rewrote one file, so the stated restart condition fired: "When that rewrite
count is greater than zero the phase restarts from P2-T1 after the rewritten files are re-read."
The rewritten file was re-read in full and the task was then re-run. Pass 2 rewrote nothing.

A third pass was required for a different reason. P2-T2 then reported **17** findings against the
baseline's 16, the additional one being `PSUseOutputTypeCorrectly` at
`scripts/dependencies/PackageGraph.psm1:127` — a file this change created, so P2-T2's owned-file
count of 0 was violated by this change's own code rather than by anything pre-existing. The module
was corrected (see the section below), and correcting it modified a tracked-scope source file,
which under the General Code Change Policy restarts the toolchain loop at step 1. Pass 3 is that
restart. It rewrote nothing.

| Pass | Files scanned | Hash differences | Rewritten path | Why the pass ran |
|---|---|---|---|---|
| 1 | 34 | **1** | `tests/scripts/dependencies/PackageGraph.Tests.ps1` | first invocation |
| 2 | 34 | **0** | — | restart triggered by the pass-1 rewrite |
| 3 | 34 | **0** | — | restart triggered by the `PackageGraph.psm1` analyzer fix |

**Rewrite count (the figure the restart rule reads): 0**, measured on pass 3 as the SHA-256
hash-difference count over the 34 `.ps1`, `.psm1` and `.psd1` files under the four scan folders,
computed after the revert step and excluding every derived-set member. `Formatted N files` is not
used as this count, per gate rule 6.

The P0-T15 baseline measured 0 of 32 files rewritten. The population has since grown to 34: P1-T4
added `scripts/dependencies/PackageGraph.psm1` and P1-T5 added
`tests/scripts/dependencies/PackageGraph.Tests.ps1`, and the second of those is the file pass 1
rewrote. The baseline is therefore not contradicted — it measured a population that did not yet
contain the rewritten file.

### Re-read of the rewritten file

`tests/scripts/dependencies/PackageGraph.Tests.ps1` was read in full after the rewrite. The
properties P1-T5's acceptance asserts were re-measured against the post-format text rather than
carried forward from before it:

| P1-T5 property | Re-measured after the rewrite |
|---|---|
| At most 500 lines | **487** |
| `Describe` and `Context` names matching `AC\d` | **0** |
| No `New-TemporaryFile`, `[System.IO.Path]::GetTempPath`, `$env:TEMP` or `Out-File` | none present |

The rewrite is whitespace alignment inside the suite body — the formatter aligned the hashtable
value columns in the two `[ordered]@{}` fixtures — and changes no `Describe`, `Context` or `It`
name, no assertion and no fixture content.

### The `PackageGraph.psm1` correction that forced pass 3

`Get-PackageManifestPath` declared `[OutputType([string])]` while returning
`@($selected | Sort-Object)`, an array whose element type the analyzer could not reconcile with the
scalar declaration. Two one-line changes closed it: the attribute became `[OutputType([string[]])]`
and the return became `return [string[]]@($selected | Sort-Object)`. The explicit cast is the half
that makes the declaration true rather than merely widened — without it the pipeline's inferred
output remains `System.Object`.

The change is a contract annotation and a cast on an already-sorted string collection, so it alters
no behaviour: every element the function returned before is the same string in the same order now.
`Invoke-ScriptAnalyzer -Path "scripts/dependencies" -Recurse` reports **0** findings after the
correction, against 1 before it. The four `Get-PackageManifestPath` cases in the P1-T5 suite
continue to assert against string arrays and are re-run at P2-T3, which is where the behavioural
confirmation is recorded; P1-T6 ran against the pre-correction module and is superseded for this
function by that later run.

The module's pre-correction hash was
`2993A7CE207733A1E725B825CA5022FAAC5A41641FB66925093241E1ECB5A8EB` and its post-correction hash is
`A33C42681F7FE5BF16130E6CA57634041671AEEA70948333311440AB2676DB7D`. Pass 3 left the latter
unchanged, so the formatter had no complaint about the corrected text.

### Encoding observation

The formatter left the file with **no byte-order mark** and **bare LF** line endings: 0 CRLF pairs
and 487 bare LF. That matches the file as P1-T5 created it and matches
`scripts/dependencies/PackageGraph.psm1`, which the formatter did not touch. It differs from the
pre-existing files in the same folders, which are BOM-free with CRLF, so the divergence is line
endings only and not the byte-order mark. It is recorded here as an observation because it is
visible in the hash comparison; no acceptance clause of this task turns on it, and the pre-existing
suite is BOM-free too, so `PSUseBOMForUnicodeEncodedFile` is not implicated. P2-T2 measures the
analyzer finding set over the same files and is where a diagnostic would surface.

## Derived revert pathspec

Derivation, per CMD-REVERT-OUT-OF-SCOPE-FORMAT: the set of paths whose SHA-256 changed across the
format invocation, **minus** every member of the spec `## Write Set`.

| Step | Value |
|---|---|
| Hash-difference set, pass 1 | `tests/scripts/dependencies/PackageGraph.Tests.ps1` |
| Is that path a spec `## Write Set` member? | **Yes** — `spec.md` line 599, added by P1-T1 |
| Derived set = difference minus Write Set | **empty** |

**REVERT-SET: empty**

`git checkout --` was therefore **not run**, which is what the Command Reference directs for the
empty case. On pass 2 the hash-difference set was itself empty, so the derived set was empty again.

The derivation is performed rather than hard-coded, as required: the only prior measurement of
which files this formatter rewrites was taken with `Invoke-Formatter` under PSScriptAnalyzer
defaults rather than with the PoshQC tool's own bundled settings, and pass 1 of this task is a
direct demonstration of why a hard-coded pair would have been wrong — it rewrote a file that
measurement could not have named, because the file did not exist when the measurement was taken.

## Porcelain captures for `scripts/vscode`

Immediately before the revert step:

```
(empty)
```

Immediately after:

```
(empty)
```

Both are empty, and that is the truthful observation rather than a failure: the formatter rewrote
no file under `scripts/vscode`, so nothing there was modified and nothing needed reverting. The
Command Reference states this explicitly — "An empty pre-revert capture is **not** a failure" — and
what would fail is a post-revert capture still listing a derived-set member, or a path disappearing
between the two captures that the derived set does not name. Neither occurred: the derived set is
empty and the two captures are identical.

Repository-wide porcelain over the four scan folders lists only the two untracked files this change
created:

```
?? scripts/dependencies/PackageGraph.psm1
?? tests/scripts/dependencies/PackageGraph.Tests.ps1
```

## Hash sets

Thirty-four files scanned on each pass. The before set is the pass-1 pre-format capture; the after
set is the pass-2 post-format capture. All 34 entries are identical between the pass-1 post-format
capture and the pass-2 post-format capture, which is the zero rewrite count recorded above.

### Before (pass 1, pre-format)

```
scripts/dependencies/PackageGraph.psm1|2993A7CE207733A1E725B825CA5022FAAC5A41641FB66925093241E1ECB5A8EB
scripts/vscode/Install-RepoDotNetSdk.ps1|5D8097B77D58105B5157F7E8E36CBCAA9DFD04B85F4F3D0797FE6E3FA34767C0
scripts/vscode/Invoke-MSTest.ps1|D320DED8A3EC40EC1A4890D1796DE7EABA3257C4F81AC89272D5D06F74112611
scripts/vscode/Invoke-MSTest.TrxSummary.ps1|0622CB7C5E6C31DDAF476D7CCF589D2FB6B385FE2C2F4148671DEBA96D44C9CA
scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1|D46E707423D52F2B1DED5B2207039A93195AA7EAD29F2BD0BC93E7913A1A2BFD
scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1|6FCF7CAFFA1496A956D275F01A0EE6613EBCD0C2CA6BE0D70B24025147A16E4F
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1|FACE0E2BAD8C773878D8FFE8171C5720EB9D627807E87E951E387927A76568CD
scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1|A6F057A086E4CC9462CFC2A6DDBB253C0508C469B717839496DE6193D5FEA5FE
scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1|244D1DD507FA0AB1A3E7D559AC9505B14FBF8E727A59E5A3BF00F04E118430B2
scripts/vscode/Invoke-MSTestWithCoverage.ps1|4D9263A8EB7A81C3EB4BE4F746C6E53070F38BFB29E07EB954AAF53F5C3F184E
scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1|ABA0BB53CFD80E63E714316CF12775C14DB5112C1520F49BCA0EED426F44E2EC
scripts/vscode/Invoke-Restore.ps1|BA3A1A2FEA7F95E87D7D5DAC6A2C94DE75951D24E06F9B6131E6A60276E7A0CF
scripts/vscode/Invoke-VSBuild.ps1|239D1D930DF934716A9606301492F73F24F29BA08BF96867A54E807814CB7487
scripts/vscode/Sync-PackageReferences.ps1|FF7FE7F77E0D1F2272AD69ED5614F52772DB737EC8C2911B9283FEB82450345D
scripts/vscode/TestProcessCleanup.ps1|E1B8C63C98607EEFF2C69CD6E9E31872676E087AD6E876E5CDE994D134092756
tests/scripts/dependencies/PackageGraph.Tests.ps1|9EE375BA1CFE16DC012D7494BA6C6FE8530AAC3F2CC12D1C1823353D2A6399AB
tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1|687201EEC643DBD6FD2FB735B501E0C43D1E135CFAE86CA515BAD6B34DA3D282
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1|91D8A9C1B724705DC28AB12B302EEB940FAE1A6FA6C49321B4E96977C38D29DC
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1|E7ADA9B3B929558921CD6F504DC9045D387EFEC6413E1CF036E2CBE7FF6CFB02
tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1|96F40CCF0172349F663D8009DC85DBD0C9E90A1F07D3FC1F217D42582F0518AD
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1|61400DE13A6B93D4FA4049891659A3BDE957C1DA0AAA22B863A42E575C5BAE83
tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1|AEEDBA3F3D7EF0B962F05AB6AD3EE275D5B9D75738B3937117231A1D6FD5643C
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1|A822876D33EE47B23F32487E7D794DE032AE7B6A25FEF7682EB1482FFA89E098
tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1|BBB2BE59D45F132A6F974E4AE1D34BFF0D72F9D640E5BFB547F65D52F0A867F2
tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1|7F814A5F2CA9FC9498C8E056C4D8E6A343B3F8B177FA98B9C5F10D0659092C50
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1|90D6BC4017D0D5736781741F210382679C286CD624B70E2E2400C2B0883D6365
tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1|034EEE7EF575950551873B96836464F2C88971ABEDD12904DA10879324554D41
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1|01BF5D7D45CF09544F7339AA63CFBD46B64D46BA0CA447D1784198B230DBAEE0
tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1|433E246EB325F55A462E78487D10C22698C5EEAE87410C72B7EA59377236C9EA
tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1|DD0C630F65FF27A02851199F41A0C9F3F9503A19DF04448844592948E28140D3
tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1|D53B7DEF7681D3D101B8B5C09A9F3143B9A3314F9943C6D64841C74F9D9C0570
tests/scripts/vscode/Invoke-Restore.Tests.ps1|89F5595BE8B2737FA140F8926C2D88F1CF3919622EDC2D0D461E8A817AA5C6E4
tests/scripts/vscode/Invoke-VSBuild.Tests.ps1|72ACD227F2CCE441A2B215DF93550BCB17AEF8A27FFE2E4E86E20F3ED05DB7E7
tests/scripts/vscode/TestProcessCleanup.Tests.ps1|0F9ACFDD52927191D8597E391BD947F12D48CD9A4259B56F8C0DD79F30AB067A
```

### After (pass 3, post-format)

Identical to the set above except for two entries — the one the pass-1 rewrite changed and the one
the analyzer correction changed:

```
scripts/dependencies/PackageGraph.psm1|A33C42681F7FE5BF16130E6CA57634041671AEEA70948333311440AB2676DB7D
tests/scripts/dependencies/PackageGraph.Tests.ps1|3081FB429A66B6FC9BD9E45EEAE832A93D4040E493972A267351041E0C232E74
```

All 32 other entries are byte-for-byte the hashes listed above. Neither of these two differences is
a pass-3 rewrite: both were already in place when pass 3 began, and pass 3's own hash-difference
count against its immediately preceding capture is 0 over all 34 files.

## Gate rule 14 check on the P0-T17 finding tuples

Gate rule 14 states that the `(file path, rule name, line)` tuples P0-T17 recorded, and the
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` line citations at 388, 393-401 and 417-425, stay
valid only while the formatter rewrites nothing — and directs the executor to re-derive the
affected citations when a format run's hash-difference set is non-empty **and contains a cited
file**.

The pass-1 difference set contains exactly one file, `tests/scripts/dependencies/PackageGraph.Tests.ps1`,
and passes 2 and 3 have empty difference sets. That file is **not** a cited file: it did not exist
when P0-T17 ran, so it carries none of the 16 baseline finding tuples, and it is not
`Invoke-MSTestWithCoverage.ps1`. That script's hash is unchanged across all three passes —
`4D9263A8EB7A81C3EB4BE4F746C6E53070F38BFB29E07EB954AAF53F5C3F184E` throughout — so its line
citations at 388, 393-401 and 417-425 are untouched and P2-T7 may read them as written.

The `PackageGraph.psm1` correction between passes 2 and 3 is likewise not a cited file: it too
postdates P0-T17. The P2-T2 run that followed confirms the point empirically — all 16 findings
match the baseline tuples at their recorded line numbers, so no baseline citation shifted. No
re-derivation is required.

Output Summary: PoshQC format ran three times over the four explicitly supplied scan folders. Pass
1 rewrote 1 of 34 files, `tests/scripts/dependencies/PackageGraph.Tests.ps1`, which is a spec Write
Set member, so the derived revert set was empty and no revert was run. The restart rule fired; the
file was re-read and its P1-T5 properties re-measured intact at 487 lines with 0 `AC\d` block
names. Pass 2 rewrote 0 of 34. Pass 3 was a second restart, forced by the `[OutputType]`
correction to `scripts/dependencies/PackageGraph.psm1` that P2-T2's 17th finding required, and it
also rewrote 0 of 34. The final rewrite count is **0** and the derived set is
**REVERT-SET: empty**. The pre- and post-revert `scripts/vscode` porcelain captures are both empty,
which is correct because the formatter touched nothing there.
