# Failure-First to Pass-After Trace — Authoritative Reissue

- Timestamp: `2026-07-23T04:48:07Z`
- Dependency-wording refresh rerun: `2026-07-23T04:53:52.7448057Z`
- Result: PASS
- Supersedes as current execution proof: `failure-first-to-pass-after-trace.2026-07-23T00-29.md`

## Complete Mapping Ledger

The complete 66-case failure-first mapping ledger is incorporated from `failure-first-to-pass-after-trace.2026-07-23T00-29.md` at SHA-256 `D815990A3773585B4096BFF6660543E7DAC6CBBDCE0050A1290D106B561253A1`. That incorporated ledger enumerates 62 unique named failures plus four parameterized invalid-subfolder rows and maps every required P1, P5, P7, and P8 task to its passing evidence. Its complete 54-artifact, 33-task-mapping verifier is rerun by the command below; the earlier file remains the detailed mapping appendix but is not current execution-order proof.

This reissue follows the corrected P7 delivery audit and the authoritative-order P8-T17 rerun:

- `subfolder-scope-and-delivery-audit.2026-07-23T03-26.md`, SHA-256 `CD2DD09CB041E3BD210DD64DEFD9949DB37C811BE7360A287B3C18CB6D41F52B`, now uses `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` in its command and table and records the corrected command rerun at exit code zero.
- `scope-project-file-size-integrity.2026-07-23T04-43.md`, SHA-256 `290E2AAB51A04FFCD511DDC1491B45297753A1AB96746D81F6B9D77B665CFA19`, supersedes the earlier P8-T17 artifact and records the full structural assertion at exit code zero after the P7 correction.

## Verification Command

```powershell
$ErrorActionPreference = 'Stop'
function Require {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}
function HashOf {
    param([string]$Path)
    (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash
}
$feature = 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400'
$oldTrace = Join-Path $feature 'evidence/regression-testing/failure-first-to-pass-after-trace.2026-07-23T00-29.md'
$p7Audit = Join-Path $feature 'evidence/qa-gates/subfolder-scope-and-delivery-audit.2026-07-23T03-26.md'
$t17Audit = Join-Path $feature 'evidence/qa-gates/scope-project-file-size-integrity.2026-07-23T04-43.md'
$currentTrace = Join-Path $feature 'evidence/regression-testing/failure-first-to-pass-after-trace.2026-07-23T04-48.md'
Require ((HashOf $oldTrace) -eq 'D815990A3773585B4096BFF6660543E7DAC6CBBDCE0050A1290D106B561253A1') 'Incorporated complete mapping ledger changed.'
Require ((HashOf $p7Audit) -eq 'CD2DD09CB041E3BD210DD64DEFD9949DB37C811BE7360A287B3C18CB6D41F52B') 'Corrected P7 audit changed.'
Require ((HashOf $t17Audit) -eq '290E2AAB51A04FFCD511DDC1491B45297753A1AB96746D81F6B9D77B665CFA19') 'Authoritative P8-T17 audit changed.'
$p7Raw = Get-Content -Raw -LiteralPath $p7Audit
$validP7Path = 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs'
$invalidP7Path = 'QuickFiler.Test/Viewers/FolderBreadcrumbBridgeRouterInFlightTests.cs'
Require ($p7Raw.Contains($validP7Path)) 'Correct P7 test path is absent.'
Require (-not $p7Raw.Contains($invalidP7Path)) 'Invalid P7 test path remains.'
Require ($p7Raw.Contains('Evidence-command correction rerun:')) 'P7 correction rerun timestamp is absent.'
Require ($p7Raw.Contains('complete corrected command above was rerun from the repository root and returned `EXIT_CODE: 0`')) 'P7 corrected rerun result is absent.'
$p7CommandMatch = [regex]::Match($p7Raw, '(?s)```powershell\r?\n(.*?)\r?\n```')
Require $p7CommandMatch.Success 'P7 executable command was not found.'
$p7Output = (& ([scriptblock]::Create($p7CommandMatch.Groups[1].Value)) | Out-String)
Require ($LASTEXITCODE -eq 0) 'P7 corrected command rerun failed.'
Require ($p7Output.Contains("$validP7Path`t383`t2A159CFFADF0EFD59FB979B16DF08D20BEA6804C996D2DDBAC037DE4E3929F17")) 'P7 corrected file/hash output is absent.'
$t17Raw = Get-Content -Raw -LiteralPath $t17Audit
Require ($t17Raw.Contains('This artifact supersedes `scope-project-file-size-integrity.2026-07-23T04-31.md`')) 'P8-T17 supersession marker is absent.'
Require ($t17Raw.Contains($validP7Path)) 'P8-T17 does not cite the corrected P7 path.'
$t17CommandMatch = [regex]::Match($t17Raw, '(?s)```powershell\r?\n(.*?)\r?\n```')
Require $t17CommandMatch.Success 'P8-T17 executable command was not found.'
$t17Output = (& ([scriptblock]::Create($t17CommandMatch.Groups[1].Value)) | Out-String)
Require ($LASTEXITCODE -eq 0) 'P8-T17 assertion rerun failed.'
Require ($t17Output.Contains('P8_T17_SCOPE_INTEGRITY_OK')) 'P8-T17 success sentinel is absent.'
Require ((Get-Item -LiteralPath $p7Audit).LastWriteTimeUtc -lt (Get-Item -LiteralPath $t17Audit).LastWriteTimeUtc) 'P7 correction does not precede P8-T17 evidence.'
Require ((Get-Item -LiteralPath $t17Audit).LastWriteTimeUtc -lt (Get-Item -LiteralPath $currentTrace).LastWriteTimeUtc) 'P8-T17 evidence does not precede the P8-T18 reissue.'
$oldRaw = Get-Content -Raw -LiteralPath $oldTrace
$oldCommandMatch = [regex]::Match($oldRaw, '(?s)## Verification Command\s+```powershell\r?\n(.*?)\r?\n```')
Require $oldCommandMatch.Success 'Complete mapping-ledger verifier was not found.'
$oldOutput = (& ([scriptblock]::Create($oldCommandMatch.Groups[1].Value)) | Out-String)
Require ($LASTEXITCODE -eq 0) 'Complete mapping-ledger verifier failed.'
$oldSentinel = 'P8_T18_TRACE_OK artifacts=54 task_mappings=33 unique_named_failures=62 parameterized_rows=4 final_pass=358/358 missing=0 still_failing=0'
Require ($oldOutput.Contains($oldSentinel)) 'Complete mapping-ledger success sentinel is absent.'
"P8_T18_REISSUE_OK ledger_hash=D815990A artifacts=54 task_mappings=33 unique_named_failures=62 parameterized_rows=4 p7_corrected=true t17_ordered=true final_pass=358/358 missing=0 still_failing=0"
```

The command must return exit code zero and the exact `P8_T18_REISSUE_OK` sentinel before this artifact can serve as current P8-T18 proof.

## Verification Result

`EXIT_CODE: 0`

```text
P8_T18_REISSUE_OK ledger_hash=D815990A artifacts=54 task_mappings=33 unique_named_failures=62 parameterized_rows=4 p7_corrected=true t17_ordered=true final_pass=358/358 missing=0 still_failing=0
```

The only additional output was the existing Git LF-to-CRLF working-copy normalization notice for `FolderBreadcrumb.html`; no whitespace error or source mutation occurred.

The complete verifier was rerun after the T17 wording correction and dependency-hash refresh; it again returned the recorded sentinel and `EXIT_CODE: 0`.
