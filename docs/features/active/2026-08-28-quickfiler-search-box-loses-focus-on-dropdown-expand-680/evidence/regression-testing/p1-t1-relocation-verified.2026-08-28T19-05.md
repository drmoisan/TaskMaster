Timestamp: 2026-08-28T19-05
Command: Select-String -SimpleMatch "internal void ShowPopup(Point location, bool takeFocus)" QuickFiler\Viewers\BreadcrumbDropDownHost.cs (and BreadcrumbDropDownHost.Open.cs); Select-String -SimpleMatch "internal void PublishPopupMessengerReady() =>" against both files; Select-String -SimpleMatch "using System;" QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs; (Get-Content ...).Count against both files
EXIT_CODE: 0
Output Summary:
- ShowPopup signature in BreadcrumbDropDownHost.cs: 0 hits
- ShowPopup signature in BreadcrumbDropDownHost.Open.cs: 1 hit
- PublishPopupMessengerReady signature in BreadcrumbDropDownHost.cs: 0 hits
- PublishPopupMessengerReady signature in BreadcrumbDropDownHost.Open.cs: 1 hit
- "using System;" in BreadcrumbDropDownHost.Open.cs: 1 hit
- POST_MOVE_HOST_COUNT = 498 (<= 500, strictly less than BASELINE_HOST_COUNT = 514)
- POST_MOVE_OPEN_COUNT = 107 (<= 500, strictly greater than BASELINE_OPEN_COUNT = 90)
All acceptance conditions satisfied.
