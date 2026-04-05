# P3-T10: Skip Re-Validation — SystemThemeDetector.cs

## File
`UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs`

## Current Coverage
`line-rate="0.625"` (62.5%) — tests already exist in `UtilitiesCS.Test\ThemeHelpers\SystemThemeDetectorTests.cs`.

## Source Analysis
The covered path reads the current user's `AppsUseLightTheme` registry value successfully. The remaining uncovered branches are the defensive paths for missing registry keys, missing values, non-`int` values, or registry-access exceptions.

## Skip Rationale
Those remaining branches depend on mutating or denying access to the user's Windows registry or introducing new seams around `Registry.CurrentUser`. Without such seams, the remaining paths are environment-driven and not deterministic unit-test targets.

## Decision: Skip Confirmed
