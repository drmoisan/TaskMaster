# Phase 9 Formatter-Stabilization Preflight

- Timestamp: `2026-07-23T04:58:24Z`
- Result: `PREFLIGHT: REVISIONS REQUIRED`
- Mutation: none; every command in this artifact is read-only

## Root Formatter Conflict

`csharpier check .` with CSharpier 1.3.0 checked 1,457 files and returned exit code 1 for exactly seven files:

| File | Current lines | SHA-256 | Scope |
|---|---:|---|---|
| `coverage.config` | 24 | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | Protected coverage configuration; mixed line endings only |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | Authorized issue #400 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 479 | `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77` | Authorized issue #400 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 473 | `46E602D89378582538FFA53B80338C186CC14BE87CF5F4E44BF550986B41B1F5` | Authorized issue #400 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 480 | `98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104` | Authorized issue #400 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 480 | `F21541FDB8F60D2F9123A6D4D471B2B5DB97FD55DA975BD326942F40EB294991` | Authorized issue #400 |
| `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` | 118 | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` | Unrelated committed user change; protected by P8-T17 |

Running the planned `csharpier format .` would therefore mutate both protected configuration and unrelated committed scope.

## Exact Authorized Issue Scope

The live merge-base plus untracked inventory resolves exactly 62 issue-#400 C# paths after excluding `SpamBayes.Actions.cs`. The SHA-256 of the ordinally sorted paths joined with LF and no trailing LF is `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD`.

`csharpier check @authorized` checked all 62 files and returned exit code 1 for only the five authorized files above. Read-only `csharpier format <file> --skip-write --write-stdout --log-level None` simulation produced:

| Authorized file | Current lines | CSharpier lines | Required bound |
|---|---:|---:|---:|
| `BreadcrumbMessengerHub.cs` | 456 | 456 | 480 |
| `BreadcrumbDropDownCoverageThresholdTests.cs` | 479 | 477 | 480 |
| `BreadcrumbSelectorOpenRetryTests.cs` | 473 | 470 | 480 |
| `BreadcrumbSelectorToggleUiBoundaryTests.cs` | 480 | 478 | 480 |
| `BreadcrumbPopupControlDispatchTests.cs` | 480 | 483 | 480 |

The first four files can accept formatter output within their approved bounds. `BreadcrumbPopupControlDispatchTests.cs` requires a bounded pre-format headroom correction. A read-only CSharpier stdin simulation showed that replacing its empty private `ExceptionRecorder : ConcurrentQueue<Exception>` wrapper with direct `ConcurrentQueue<Exception>` use and adding a private file alias for `Tuple<string, SynchronizationContext>` used by `OperationRecorder` formats to 479 lines without changing any test, assertion, production seam, public API, project include, or dependency.

## Requirements Conflict

`spec.md` AC-18 literally requires `csharpier format .`. The scope-safe exact-62 command is not textually identical. Final AC reconciliation therefore requires one explicit decision before AC-18 can be marked PASS: authorize the protected/unrelated changes, authorize scope-specific AC wording, or record an approved exception and leave AC-18 unchecked. The preflight does not infer that authority.

## Authoritative Baseline

- Merge base: `df5ad49c909f6b739edef45d0336151f44e827a6`
- Baseline Cobertura: `evidence/baseline/coverage-remediation-baseline.2026-07-21T22-13.cobertura.xml`
- Baseline Cobertura SHA-256: `4BD88CD3B9786FA8E8142A39A84EEBEF44581C321D8146F6A8EB46A58D2D8FE8`
- Repository coverage: 89,240/106,048 = 84.1506%
- Baseline execution: eight assemblies; 5,849 passed; zero failed; zero skipped
- Baseline changed/new production coverage: 1,141/1,143 = 99.8250%

## Complete Read-Only Verification Command

```powershell
$ErrorActionPreference = 'Stop'
function Require { param([bool]$Condition, [string]$Message) if (-not $Condition) { throw $Message } }
function HashOf { param([string]$Path) (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash }
$base = (git merge-base HEAD origin/main).Trim()
Require ($base -eq 'df5ad49c909f6b739edef45d0336151f44e827a6') 'Unexpected merge base.'
$patterns = @('QuickFiler/**/*.cs', 'QuickFiler.Test/**/*.cs', 'UtilitiesCS/**/*.cs', 'UtilitiesCS.Test/**/*.cs')
$spam = 'UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs'
$authorized = @(
    @(git diff --name-only --diff-filter=ACMR $base -- $patterns) +
    @(git ls-files --others --exclude-standard -- $patterns) |
        Sort-Object -Unique |
        Where-Object { $_ -ne $spam }
)
Require ($authorized.Count -eq 62) "Expected 62 authorized C# files; found $($authorized.Count)."
$pathBytes = [Text.Encoding]::UTF8.GetBytes(($authorized -join "`n"))
$pathHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($pathBytes))
Require ($pathHash -eq 'E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD') 'Authorized path-set hash changed.'
$protected = @{
    'coverage.config' = 'B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943'
    '.csharpierignore' = '362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25'
    $spam = '99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA'
}
foreach ($entry in $protected.GetEnumerator()) { Require ((HashOf $entry.Key) -eq $entry.Value) "Protected hash changed: $($entry.Key)" }
$rootOutput = @(& csharpier check . 2>&1)
$rootExit = $LASTEXITCODE
Require ($rootExit -eq 1) "Root formatter check unexpectedly returned $rootExit."
$rootErrors = @($rootOutput | Where-Object { $_ -match '^Error\s+\.\\(.+?)\s+- Was not formatted\.$' } | ForEach-Object { $Matches[1].Replace('\', '/') })
$expectedRoot = @('coverage.config', 'QuickFiler/Viewers/BreadcrumbMessengerHub.cs', 'QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs', $spam) | Sort-Object
Require (((($rootErrors | Sort-Object) -join "`n") -ceq ($expectedRoot -join "`n"))) 'Root formatter failure set changed.'
$scopedOutput = @(& csharpier check @authorized 2>&1)
$scopedExit = $LASTEXITCODE
Require ($scopedExit -eq 1) "Scoped formatter check unexpectedly returned $scopedExit."
$scopedErrors = @($scopedOutput | Where-Object { $_ -match '^Error\s+\.\\?(.+?)\s+- Was not formatted\.$' } | ForEach-Object { $Matches[1].Replace('\', '/') })
$expectedScoped = @('QuickFiler/Viewers/BreadcrumbMessengerHub.cs', 'QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs') | Sort-Object
Require (((($scopedErrors | Sort-Object) -join "`n") -ceq ($expectedScoped -join "`n"))) 'Scoped formatter failure set changed.'
$expectedFormattedLines = @{
    'QuickFiler/Viewers/BreadcrumbMessengerHub.cs' = 456
    'QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs' = 477
    'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs' = 470
    'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs' = 478
    'QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs' = 483
}
foreach ($entry in $expectedFormattedLines.GetEnumerator()) {
    $formatted = @(& csharpier format $entry.Key --skip-write --write-stdout --log-level None)
    Require ($formatted.Count -eq $entry.Value) "Unexpected formatted line count for $($entry.Key)."
}
$baseline = 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/baseline/coverage-remediation-baseline.2026-07-21T22-13.cobertura.xml'
Require ((HashOf $baseline) -eq '4BD88CD3B9786FA8E8142A39A84EEBEF44581C321D8146F6A8EB46A58D2D8FE8') 'Authoritative baseline Cobertura changed.'
"P9_FORMATTER_PREFLIGHT_REVISIONS_REQUIRED root_unformatted=7 authorized_files=62 authorized_unformatted=5 popup_formatted_lines=483 protected_changes=0 ac18_decision=required"
```

Expected result:

```text
P9_FORMATTER_PREFLIGHT_REVISIONS_REQUIRED root_unformatted=7 authorized_files=62 authorized_unformatted=5 popup_formatted_lines=483 protected_changes=0 ac18_decision=required
EXIT_CODE: 0
```

The complete command was executed after the precedence correction in its two set-comparison assertions and produced the expected sentinel with numeric process exit code zero. No file was changed.
