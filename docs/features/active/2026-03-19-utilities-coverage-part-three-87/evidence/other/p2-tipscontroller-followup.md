# P2-T21 Evidence: TipsController Follow-Up Tests

## Task
Add MSTest scenarios covering uncovered branches in `UtilitiesCS\HelperClasses\ToolTips\TipsController.cs`
until line coverage reaches ≥ 0.80.

## Coverage Result
- **Before:** `line-rate = 0.4` (40%)
- **After:**  `line-rate = 0.9833333333333333` (~98.3%)
- **Threshold:** ≥ 0.80 ✓

## Test Methods Added

### `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs`
1. `Constructor_WithGroupNumber_StoresGroupNumberAndColumnWidthDefaultsToZero`
2. `GroupNumber_Setter_UpdatesStoredValue`
3. `Constructor_LabelWithNullParent_ThrowsArgumentException`
4. `Constructor_LabelWithInvalidParentType_ThrowsArgumentException`
5. `Toggle_WithSharedColumnParameter_BothStateTransitions_TogglesLabelCorrectly`
6. `ToggleColumnOnly_WithPanelParent_DoesNotThrowAndUpdatesState`

### `UtilitiesCS.Test\HelperClasses\TipsController_TableLayoutPanel_Tests.cs` (new file)
7. `Constructor_LabelUnderTableLayoutPanel_SetsTlpAndColumnMetadata`
8. `Toggle_DesiredStateWithSingleRowTlp_AdjustsColumnWidth`
9. `ToggleColumnOnly_WithTlpParent_AdjustsColumnWidth`

## Branches Newly Covered
- Lines 18–22: `TipsController(label, groupNumber)` constructor
- Lines 30–34: `InitializeLabel` TableLayoutPanel branch
- Lines 44–46: `ResolveParent<T>` generic method
- Lines 51–55: `ResolveParentType` null-parent ArgumentException
- Lines 62–67: `ResolveParentType` invalid-type ArgumentException
- Line 98: `ColumnWidth` getter
- Lines 104–105: `GroupNumber` setter
- Lines 121–130: `Toggle(bool sharedColumn)` both branches
- Lines 133–155: `Toggle(ToggleState, bool sharedColumn)` both branches (Panel path)
- Lines 164, 171: `Toggle(ToggleState)` TLP column-width side-effect
- Lines 177–189: `ToggleColumnOnly(ToggleState)` all paths

## Toolchain Pass (final)
- **Format (csharpier):** EXIT_CODE 0
- **Lint (analyzers):** EXIT_CODE 0, 0 errors
- **Type-check (nullable):** EXIT_CODE 0, 0 warnings
- **Tests:** 3461 total / 3459 passed / 0 failed / 2 skipped

## File Sizes (policy: ≤ 500 lines)
- `TipsController_Tests.cs`: 414 lines ✓
- `TipsController_TableLayoutPanel_Tests.cs`: 180 lines ✓
