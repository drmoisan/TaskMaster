# [P3-T10] Post-format production surface and file-size re-verification

Timestamp: 2026-08-11T02-02
Command: `wc -l <files>`; `git status --porcelain -uall -- scripts/vscode tests/scripts/vscode`;
`git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`;
`git diff --numstat -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`;
`git status --porcelain -uall -- CLAUDE.md .claude/rules`
EXIT_CODE: 0

Run after the final clean toolchain iteration recorded by `[P3-T5]` (iteration 2), against the
post-format state.

## Production file sizes (re-run of the [P2-T10] measurement)

| File | Post-format lines | Ceiling | Strictly below 500 |
|---|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | **389** | 500 | **yes** (111 headroom) |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | **457** | 500 | **yes** (43 headroom) |

Both production files are strictly below 500 lines after formatting.

`ClosureFilter.ps1` moved 387 -> 389 since `[P2-T10]`. That is the iteration-1 remediation, not the
formatter: the dead-`else` removal replaced one line with three (a two-line explanatory comment plus
the simplified assignment) and the guard simplification replaced one line with one. The `[P3-T2]`
iteration-2 formatter fixed-point check measured `ANY_FILE_WOULD_BE_REWRITTEN: False` for all four
files.

## Test file sizes (post-format)

| File | Post-format lines | Ceiling | Strictly below 500 |
|---|---|---|---|
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` | **443** | 500 | **yes** (57 headroom) |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | **490** | 500 | **yes** (10 headroom) |

`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Unit.Tests.ps1` does not exist: the
`[P1-T12]` pre-authorized split was not taken, so it is correctly not measured.

The 500-line ceiling in `.claude/rules/general-code-change.md` and `.claude/rules/powershell.md`
applies to test code as well as production code. `[P1-T12]` measured these files pre-format at 367 and
490; the ClosureFilter test file moved to 443 through the two coverage tests added during the
iteration-1 remediation, and the helpers test file is unchanged at 490. Every one of these files is
strictly below 500 lines after formatting.

## Changed-file set, restricted to the production and test surface

`git status --porcelain -uall -- scripts/vscode tests/scripts/vscode` (verbatim):

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
```

**BYTE-IDENTICAL** to the restricted listing recorded by `[P2-T11]`. Same four paths, same order, same
status codes.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is absent from the listing, as required. No format run
rewrote it — both `[P3-T2]` iterations measured `FORMAT_SCAN_GRANULARITY: file-honored` with no file
rewritten, so the `[P0-T6]` / `[P3-T2]` restore branch was never taken and no `git checkout` was
needed.

Paths added under `<FEATURE>/evidence/` since `[P2-T11]` are expected and are not part of this
comparison, which is deliberately restricted to `scripts/vscode` and `tests/scripts/vscode` for
exactly that reason.

## The "exactly two edits" re-measurement (spec AC 13), post-format

`git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (verbatim):

```diff
diff --git a/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 b/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
index 2af80765..e3db4157 100644
--- a/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
+++ b/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
@@ -1,4 +1,5 @@
 Set-StrictMode -Version Latest
+. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')

 function Get-KoverageProjectAllowlist {
     [CmdletBinding()]
@@ -423,6 +424,7 @@ function ConvertTo-KoverageCoberturaXml {
         $classNode.filename = ConvertTo-KoverageRelativePath -Path $classNode.filename -RepoRoot $RepoRoot -PathSeparator $PathSeparator
     }

+    Remove-CoberturaExemptClosureCoverage -XmlDocument $xml
     Merge-CoberturaClassesByFilename -XmlDocument $xml

     if (-not $xml.SelectSingleNode('//sources')) {
```

`git diff --numstat`:

```
2	0	scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
```

| Measure | Value |
|---|---|
| Added lines | **2** |
| Removed lines | **0** |
| Baseline format hunks to exclude | none — `[P0-T6]` recorded `baseline format diff: empty` |

Still exactly two added lines and zero removed lines after formatting, and the blob hashes in the
diff header (`2af80765..e3db4157`) are unchanged from the `[P2-T11]` measurement, so the file is
byte-identical to its state at that task. The two added lines are the dot-source line and the
`Remove-CoberturaExemptClosureCoverage -XmlDocument $xml` call, in the positions the hard ordering
criterion requires.

## Governance-file re-check

`git status --porcelain -uall -- CLAUDE.md .claude/rules` (verbatim, recorded including the empty
result):

```
```

The command returned **no output**. Neither `CLAUDE.md` nor anything under `.claude/rules/` is
modified.

## Output Summary

Post-format: both production files strictly below 500 lines (389, 457); both test files strictly
below 500 lines (443, 490); the third test file does not exist because the split was not taken. The
restricted changed-file listing is byte-identical to `[P2-T11]`'s. The helpers-module diff is still
exactly 2 added and 0 removed lines with no baseline format hunks to exclude.
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is absent from the listing. The governance-file check
returns empty.
