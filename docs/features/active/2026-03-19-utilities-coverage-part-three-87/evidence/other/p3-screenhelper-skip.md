# P3-T6: Skip Re-Validation — ScreenHelper.cs

## File
`UtilitiesCS\HelperClasses\Windows Forms\ScreenHelper.cs`

## Current Coverage
`line-rate="0.032086"` (~3.2%) — limited tests already exist in `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs`.

## Source Analysis
`ScreenHelper` includes some pure area helpers, but most uncovered logic depends on `Screen.AllScreens`, live monitor topology, container handles, and screen-to-screen coordinate translation on the host machine.

## Skip Rationale
The existing tests already cover the deterministic pure helpers. The remaining uncovered branches depend on multi-monitor configuration and runtime screen geometry that varies by environment and cannot be guaranteed in CI or on developer machines. That makes broad additional coverage unreliable and environment-coupled.

## Decision: Skip Confirmed
