# HTML accessibility and theme contracts

Timestamp: 2026-07-23T00:15:57.9449374-04:00

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest) { throw 'VSTest was not resolved.' }; & $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~FolderBreadcrumbAssetContractTests' /Logger:'console;Verbosity=normal'; exit $LASTEXITCODE`

EXIT_CODE: 0

Output Summary: VSTest 18.8.0 discovered all 15 compiled `FolderBreadcrumbAssetContractTests`; all 15 passed with 0 failed and 0 skipped in 1.3476 seconds.

## Passing contract inventory

- `CompiledResource_RemainsSelfContainedAndThemeAware`
- `CollapsedMode_RendersOnlyTheCommittedSelectedDataRow`
- `Percentage_UsesVisibleHostSuppliedPercentTextWithoutRecomputation`
- `CollapsedDocumentAndList_HideVerticalOverflowWithoutScrollControls`
- `Markup_ContainsExactlyOneAccessibleDropDownButton`
- `SelectorView_UpdatesModeAndAccurateAriaExpandedState`
- `ExpandedRows_ExposeListboxOptionsAndOneActiveSelectedOption`
- `ExpandedDuplicatePathState_YieldsExactlyOneActiveAriaSelectedOption`
- `ActiveRow_ScrollsIntoViewOnlyInExpandedMode`
- `SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages`
- `ButtonAndRows_PostToggleAndStableIdentityActivationMessages`
- `ExpandedSubfolders_UseOneAccessibleStableIdentityActivationPath`
- `RenderReceiver_OwnsSelectedChildExpandedAndCollapsedProjection`
- `LeftAndRightBreadcrumbMessages_RemainSupported`
- `ModeAndThemeHooks_RemainIndependentAndFocusTheActiveListTarget`

## Verified behavior

- Collapsed mode renders only the committed row and uses host-supplied `percentText`.
- Collapsed document, body, and list overflow remain hidden without scroll controls.
- The markup contains one accessible drop-down toggle, and `selectorView` controls collapsed/expanded mode plus accurate `aria-expanded`.
- Expanded rows expose listbox/option semantics with exactly one active and `aria-selected` option.
- Duplicate output paths retain one active logical option through stable row identity.
- A selected child owns active state only while its parent identity remains pending. Pending-row navigation clears child ownership and gives the pending row exclusive active state.
- Selected children use the stable `folder-option-{rowIndex}-subfolder-{index}` id, and the focused list assigns that id to `aria-activedescendant`.
- The active expanded option uses nearest-position visibility; collapsed mode does not scroll in place.
- Up, Down, Enter, and Escape prevent browser scrolling and emit native `selectorKey` messages. Left and Right remain on the existing breadcrumb route.
- The button emits `selectorToggle`; rows emit stable-identity activation; subfolders emit one `selectorSubfolderActivate` message.
- Theme and view-mode hooks remain independent. Expanded focus enters the list and collapsed focus returns to the drop-down button.
