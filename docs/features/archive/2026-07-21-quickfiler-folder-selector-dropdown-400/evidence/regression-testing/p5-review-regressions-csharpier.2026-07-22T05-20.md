# P5 review regression formatting

Timestamp: 2026-07-22T05:20:11.6209612Z

Command: `@('QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs') | csharpier pipe-files`

EXIT_CODE: 0

Output Summary: CSharpier 1.3.0 formatted exactly the three P5-T22 test sources through one pipe-files invocation and emitted no diagnostics. The formatter was stable: the SHA-256 values remained `C716F99A80117261F721E28AA49FB37A89341EFDB042C47AAF6E62DEE668ED1D`, `2688DF27049F6B74C4BB61D2E0667C69A9BE82EA90C82FD59EB6696D61BD1A69`, and `4D4B5348EDBE20BEFE48DA6C75DBBCAFF6A3421F6DC2DD6EEB6301118909A4BF`. Final line counts are 500, 494, and 499 respectively, so no P5-T23 restart was required.
