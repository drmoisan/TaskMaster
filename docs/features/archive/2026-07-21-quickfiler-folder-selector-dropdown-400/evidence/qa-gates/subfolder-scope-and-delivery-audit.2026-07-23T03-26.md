# Subfolder Scope and Delivery Audit

- Timestamp: `2026-07-23T03:26:32.8347116Z`
- Evidence-command correction rerun: `2026-07-23T04:41:07.8937103Z`
- Command: PowerShell deterministic scope, hash, line-count, delivery-invariant, protected-file, and whitespace audit:

```powershell
$ErrorActionPreference = "Stop"
$correctionFiles = @(
    "UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs",
    "UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs",
    "QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs",
    "UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs",
    "UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs",
    "QuickFiler/Resources/FolderBreadcrumb.html",
    "UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeMessagesTests.cs",
    "QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs",
    "QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs"
)
$protectedHashes = @{
    "QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs" = "5077FCD19CF8471DAC48D25C579305FC06994B5F909BEB0DC9A973DAD1337A36"
    "QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs" = "A65B2CC6099B3F88F1890A327B9F42B461CA469D6ED351E8D260A0EBF072C825"
}
$failures = 0
foreach ($file in $correctionFiles) {
    $lineCount = (Get-Content -LiteralPath $file).Count
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash
    if ($lineCount -gt 500) { $failures++ }
    "$file`t$lineCount`t$hash"
}
foreach ($entry in $protectedHashes.GetEnumerator()) {
    if ((Get-FileHash -Algorithm SHA256 -LiteralPath $entry.Key).Hash -ne $entry.Value) {
        $failures++
    }
}
function Assert-ExactTextCount {
    param([string]$Name, [string]$Source, [string]$Pattern, [int]$Expected)
    $actual = ([regex]::Matches($Source, [regex]::Escape($Pattern))).Count
    if ($actual -ne $Expected) {
        Write-Error "$Name expected $Expected but found $actual"
        $script:failures++
    }
    "$Name`t$actual"
}
function Assert-ExactSourceCount {
    param([string]$Name, [string]$Path, [string]$Pattern, [int]$Expected)
    Assert-ExactTextCount $Name (Get-Content -Raw -LiteralPath $Path) $Pattern $Expected
}
$selectorMessages = "UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs"
$bridgeMessages = "UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs"
$router = "UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs"
$session = "UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs"
$coordinator = "QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs"
$html = "QuickFiler/Resources/FolderBreadcrumb.html"
Assert-ExactSourceCount "Explicit subfolder message shape" $selectorMessages 'public sealed class BreadcrumbSelectorSubfolderActivationMessage' 1
Assert-ExactSourceCount "Explicit subfolder message type" $selectorMessages 'public override string Type => "selectorSubfolderActivate";' 1
Assert-ExactSourceCount "HTML activation post" $html 'type: "selectorSubfolderActivate",' 1
Assert-ExactSourceCount "Inbound coordinator route" $coordinator 'case BreadcrumbSelectorSubfolderActivationMessage subfolderActivation:' 1
Assert-ExactSourceCount "Router-owned commit transition" $router 'public BreadcrumbSelectionTransition ActivateSelectorSubfolder(' 1
Assert-ExactSourceCount "Session commit transition" $session 'public BreadcrumbSelectionEffects ActivateSubfolder(' 1
$bridgeSource = Get-Content -Raw -LiteralPath $bridgeMessages
Assert-ExactTextCount "Render message shape" $bridgeSource 'public sealed class RenderMessage : BreadcrumbBridgeMessage' 1
$renderMatch = [regex]::Match(
    $bridgeSource,
    '(?s)public sealed class RenderMessage.*?(?=\r?\n    /// <summary>An arrow)'
)
if (-not $renderMatch.Success) {
    Write-Error "RenderMessage scope was not resolved."
    $failures++
}
$renderSource = $renderMatch.Value
Assert-ExactTextCount "Render constructors" $renderSource 'public RenderMessage(' 2
Assert-ExactTextCount "Render selected-index property" $renderSource 'public int SelectedSubfolderIndex { get; }' 1
Assert-ExactTextCount "Render selected-folder property" $renderSource 'public string? SelectedFolder { get; }' 1
Assert-ExactSourceCount "Stable child-option id construction" $html 'subfolderElement.id = "folder-option-" + row.rowIndex + "-subfolder-" + index;' 1
Assert-ExactSourceCount "Bounded legacy selectionChange receiver branch" $html '} else if (msg.type === "selectionChange") {' 1
git diff --quiet -- `
    QuickFiler/Viewers/BreadcrumbMessengerHub.cs `
    QuickFiler/Viewers/BreadcrumbUiDispatcher.cs `
    UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs
if ($LASTEXITCODE -ne 0) { $failures++ }
git diff --check -- $correctionFiles
if ($LASTEXITCODE -ne 0) { $failures++ }
if ($failures -ne 0) { exit 1 }
```

- `EXIT_CODE: 0`

## Output Summary

The audit reported zero failures. The P7-C and P7-D correction batch contains only the nine authorized files, every correction-batch file is at or below 500 lines, the two protected P7-B files retain their recorded hashes, the three unchanged supporting production files have no diff, all audited constructs occur at the required cardinality, and `git diff --check` passes for all nine correction-batch files.

The evidence command was corrected on `2026-07-23T04:41:07.8937103Z` to use the actual authorized path `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` in both the command and scope table. The complete corrected command above was rerun from the repository root and returned `EXIT_CODE: 0`. It reported that file at 383 lines with SHA-256 `2A159CFFADF0EFD59FB979B16DF08D20BEA6804C996D2DDBAC037DE4E3929F17`, all nine correction files at or below 500 lines, all 12 delivery-invariant counts at their expected values, matching protected hashes, and no whitespace error. The only additional output was Git's existing LF-to-CRLF working-copy normalization warning for `FolderBreadcrumb.html`.

## Authorized Correction-Batch Scope

| Plan scope | File | Lines | SHA-256 |
| --- | --- | ---: | --- |
| P7-T16 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | 474 | `FCA443B89681555855D80D8F188FADE7EE2468241A12A2DED4AA2E6BA056C98B` |
| P7-T16 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` | 383 | `2A159CFFADF0EFD59FB979B16DF08D20BEA6804C996D2DDBAC037DE4E3929F17` |
| P7-T16 | `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` | 488 | `D668B9F07259D890FC2A8D97015CE0DEA1BEE80F16B5190E890E69AABE332E05` |
| P7-T23 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs` | 463 | `BEDE51AE5804BA990D2B7BF996484DE1925570FC8967F34CE20B274DEBA8F392` |
| P7-T23 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 485 | `B700E53C1C18B4A15C02697BC873315B86C7DDFFD4ABAE7DB1DDFBEC1427444C` |
| P7-T23 | `QuickFiler/Resources/FolderBreadcrumb.html` | 489 | `26E15586EB0BE89FD902B3FC0924ED8394E455D1BF38801F92053D1E8FC19C7E` |
| P7-T23 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeMessagesTests.cs` | 328 | `32F641421AD7D03B26C64F55993C513E2DF83D1BD32F2C67ECAECC9E947340E9` |
| P7-T23 | `QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs` | 480 | `9881BA541C65E382C97FA5CD0C5E48FF38590EA60D42C43D51A36C163242463D` |
| P7-T23 | `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` | 405 | `3D97EF65CC89E9CFC8979D145FACA060F121AC3259E0F9988DB5AC7A5482EAAB` |

## Protected P7-B Hashes

| Protected file | Expected SHA-256 | Current SHA-256 | Result |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | `5077FCD19CF8471DAC48D25C579305FC06994B5F909BEB0DC9A973DAD1337A36` | `5077FCD19CF8471DAC48D25C579305FC06994B5F909BEB0DC9A973DAD1337A36` | Match |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | `A65B2CC6099B3F88F1890A327B9F42B461CA469D6ED351E8D260A0EBF072C825` | `A65B2CC6099B3F88F1890A327B9F42B461CA469D6ED351E8D260A0EBF072C825` | Match |

The P7-B HTML and two P7-B test files that changed are within the explicit P7-T23 authorization. The two out-of-scope P7-B files above are byte-for-byte unchanged.

## Delivery Invariants

The deterministic source audit produced these exact counts:

| Audited construct | Count |
| --- | ---: |
| Explicit subfolder message shape | 1 |
| Explicit subfolder message type | 1 |
| HTML activation post | 1 |
| Inbound coordinator route | 1 |
| Router-owned commit transition | 1 |
| Session commit transition | 1 |
| Render message shape | 1 |
| Render constructors | 2 |
| Render selected-index property | 1 |
| Render selected-folder property within `RenderMessage` | 1 |
| Stable child-option id construction | 1 |
| Bounded legacy `selectionChange` receiver branch | 1 |

The implementation and composed runtime tests establish the following delivery behavior:

- One explicit subfolder activation flows through one inbound coordinator route and one router-owned/session commit transition. The passing activation tests verify one render, one selection event, one close, and one focus return for a valid activation, and zero outbound effects for an invalid activation.
- The explicit activation path does not emit the legacy `selectionChange` message. The single legacy receiver branch remains for compatibility, but the new explicit route does not depend on it.
- `RenderJson()` enters the router's existing `Read(RenderJsonCore)` synchronization boundary. `RenderJsonCore` builds the rows, selected child index, and canonical child path from that single locked model snapshot, then serializes one cacheable render message.
- The collapsed view renders the canonical selected child full path as its sole folder line and retains the parent row's probability text. It does not render the parent's leaf affordance as selected.
- The selected-index migration fallback is bounded to a nonnegative index below the current subfolder count and requires a selectable target. The P7-C regression batch covers retained-index, out-of-range, and nonselectable fallback behavior.
- Production UI-affinity behavior is unchanged. `BreadcrumbBridgeCoordinator.cs` retains its protected hash, `BreadcrumbUiDispatcher.cs` has no diff, and the dispatcher constructor continues to capture the current synchronization context.
- Full-path output semantics are unchanged. `BreadcrumbSelectionMap.cs` has no diff and continues to return the selected folder's canonical `FolderPath`.
- Child option ids use the stable `folder-option-{rowIndex}-subfolder-{index}` form. On reopen, the committed child exclusively owns the active and `aria-selected="true"` state while pending identity resolves to its parent, and the focused list's `aria-activedescendant` points to that child.
- After Up/Down navigation, only the new pending row owns the active and selected state and `aria-activedescendant`; the previously committed child retains its durable value but no active/selected marker. Committed and pending parent identity semantics remain separate.
- List focus and Left/Right behavior remain unchanged.

## Independent Evidence Correction Verification

- Timestamp: `2026-07-22T23:30:53.0351819-04:00`
- Corrected the displayed P7-T16 path from the nonexistent `QuickFiler/UtilitiesCS/...` location to `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`. The previously recorded 474-line count and SHA-256 already matched the real file.
- Replaced the delivery-cardinality placeholder comments with the exact PowerShell assertions used for all 12 reported constructs and reran them. Every asserted count matched and the command exited `0`.
- No production or test source changed during this evidence correction.

## Verification Gates

- CSharpier final pass: five changed C# files checked with no further formatting changes.
- HTML whitespace check: passed.
- Analyzer build: zero errors; five pre-existing package warnings.
- Nullable build with warnings as errors: zero errors; five pre-existing package warnings.
- P7-D focused regression batch: 43 discovered, 43 passed, zero failed, zero skipped.
- Exact preserved-contract batch: 23 discovered, 23 passed, zero failed, zero skipped.
- `git diff --check` for all nine correction-batch files: passed.
