# P0-T15 — PowerShell Formatter Baseline

Timestamp: 2026-09-19T23-02

Command: CMD-POSHQC-FORMAT-BASELINE — MCP tool `mcp__drm-copilot__run_poshqc_format`.

Exact `scan_folders` argument value passed:

```
["scripts/vscode", "tests/scripts/vscode"]
```

`workspace_root` passed as the execution worktree root. `scan_folders` was supplied explicitly
because `config/poshqc-scan.json` does not exist in this repository and an omitted argument
measures nothing.

EXIT_CODE: 0

MCP Result: `ok:true`. Not asserted, per the Command Reference; the observation is the hash-difference
count and the porcelain capture below.

## Rewrite count

**0.** Derived from the hash difference between the two sets below: 32 files were hashed before the
run and 32 after, and every one of the 32 SHA-256 values is identical across the two captures.

Per gate rule 6 this is a hash-difference count and not any figure the tool printed.

## Hash set — before the format run

```
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
FF7FE7F77E0D1F2272AD69ED5614F52772DB737EC8C2911B9283FEB82450345D  scripts/vscode/Sync-PackageReferences.ps1
E1B8C63C98607EEFF2C69CD6E9E31872676E087AD6E876E5CDE994D134092756  scripts/vscode/TestProcessCleanup.ps1
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
0F9ACFDD52927191D8597E391BD947F12D48CD9A4259B56F8C0DD79F30AB067A  tests/scripts/vscode/TestProcessCleanup.Tests.ps1
```

## Hash set — after the format run

Byte-identical to the set above, file for file and value for value. The 32 pairs are not repeated
here; the equality is the measurement, and the hash-difference count it yields is **0**.

## Verbatim porcelain, taken immediately after the run

```
git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode
```

produced **no output**. The capture is empty.

This is the authoritative list of pre-existing formatting drift and is the only set later tasks may
exclude from a changed-line audit. **It is empty**, so no later task may exclude any path from a
changed-line audit on the grounds of pre-existing drift.

## Non-vacuity control — the formatter is live

An empty result is not by itself evidence that anything ran, and gate rule 2 prohibits accepting
one. A bounded control was therefore run and reverted.

1. `scripts/vscode/TestProcessCleanup.ps1`, a file outside the spec `## Write Set`, was transiently
   perturbed by over-indenting its `[CmdletBinding(SupportsShouldProcess)]` line from 4 spaces to
   12. The write used `[System.IO.File]::WriteAllText`, not the `Write` or `Edit` tool, so no
   batch-budget slot could be consumed by a transient control.
   Hash after perturbation: `004DAB825E9BB0CFFDE18D0EA7CE6C87C11AD1CA9D229624726CE8B51A1C0EA7`.
2. The same MCP format call was repeated with the same `scan_folders` value.
   Hash after that run: `5805BE198DC035C4A956789E4152FB192EBF912E2B9334D81CF78E490FFE3494`.
   The formatter changed the file, so the tool reaches these folders and rewrites what it finds
   wrong. The indentation perturbation was corrected.
3. `git checkout -- scripts/vscode/TestProcessCleanup.ps1` restored the file.
   Hash after revert: `E1B8C63C98607EEFF2C69CD6E9E31872676E087AD6E876E5CDE994D134092756`, equal to
   its value in both sets above. The porcelain capture over both folders is empty again.

**Side effect observed and recorded.** The post-format hash at step 2 is not equal to the original
hash at step 1, and the `git diff` showed why: when the tool rewrites a file it also strips the
UTF-8 BOM and converts CRLF line endings to LF, over and above the formatting fix it was invoked
for. That is a property of a rewriting run only — it did not occur on either run over the pristine
tree, because those runs rewrote nothing. Any later task that observes a PoshQC format rewrite must
expect a BOM and line-ending change alongside the formatting change, and `.claude/rules/powershell.md`
requires a BOM on PowerShell files.

## Discrepancy against the plan's Measured Tree Facts — recorded, not absorbed

The plan's Measured Tree Facts table carries the row "Files the PowerShell formatter rewrites",
naming `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and
`scripts/vscode/Sync-PackageReferences.ps1`. **This run rewrote none of the three.**

The row states its own provenance as an "executor preflight run of `Invoke-Formatter` under
PSScriptAnalyzer defaults, **not** a PoshQC format run", and adds that "the PoshQC tool's bundled
settings may differ, which is why the revert pathspec is derived at run time rather than
hard-coded". The measurement above confirms that the two differ: the three files are clean under
PoshQC's bundled settings and drifted under PSScriptAnalyzer defaults.

This task's own acceptance is unaffected — it records a hash-difference count rather than asserting
one — and P0-T16's is explicitly satisfied by the empty case. The consequence is recorded here for
the coordinator because it reaches further than this task:

- **Scope Decision 8's revert half degrades as designed.** The two out-of-scope files were not
  rewritten, so the derived revert set is empty and P0-T16 records `REVERT-SET: empty`, which the
  Command Reference pre-authorises.
- **Scope Decision 8's keep half rests on a premise that does not hold at this point in the run.**
  It states that `scripts/vscode/Sync-PackageReferences.ps1` "sits modified from the P0-T15 format
  run onward", and P2-T8 and P2-T9 each assert its presence in the Batch A commit on that basis —
  P2-T8 requires `git show --name-only` to list it, and P2-T9 requires a Batch A production count of
  exactly 2 with that path named as a member. As measured here the file is unmodified, and the plan
  itself records that no Batch A task creates or edits it before P3-T4. This is flagged now rather
  than at P2-T8 so the coordinator can evaluate it while Phase 1 proceeds. No acceptance condition
  is adjusted and no plan text is edited.

## Acceptance evaluation

- The artifact carries both hash sets. PASS.
- An integer rewrite count derived from the hash difference is recorded — **0**. PASS.
- The exact `scan_folders` argument value passed is recorded. PASS.
- The verbatim `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode`
  output taken immediately after the run is recorded — the capture is empty and is recorded as
  empty. PASS.
- `MCP Result: ok:true` is recorded but not asserted. Per instruction.

Output Summary: CMD-POSHQC-FORMAT-BASELINE ran over `["scripts/vscode", "tests/scripts/vscode"]` and
rewrote **0** of the 32 PowerShell files in scope, measured as a SHA-256 hash difference across the
invocation. The porcelain capture taken immediately afterwards is empty, so there is no pre-existing
formatting drift for a later changed-line audit to exclude. A bounded reverted control confirmed the
formatter is live: a transiently over-indented file was rewritten and corrected by the same call,
and the revert restored its original hash. The tool also strips the BOM and converts CRLF to LF on
any file it rewrites. The plan's Measured Tree Facts row predicting three rewritten files was
measured under `Invoke-Formatter` defaults rather than PoshQC and does not hold for this tool; the
divergence and its consequence for Scope Decision 8 are recorded above rather than absorbed.
