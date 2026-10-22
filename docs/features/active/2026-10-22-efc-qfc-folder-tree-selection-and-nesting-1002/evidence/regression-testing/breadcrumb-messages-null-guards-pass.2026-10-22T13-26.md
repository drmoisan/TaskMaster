# BreadcrumbMessages null-guard tests pass (P2-T5, #1002 cycle 1)

Timestamp: 2026-10-22T13-26
Command: dotnet tool run csharpier format . ; msbuild UtilitiesCS.Test/UtilitiesCS.Test.csproj /t:Build; vstest.console.exe UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~BreadcrumbMessagesIssue1002Tests ; dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary:
- Scoped run: total 7, executed 7, passed 7, failed 0.
- Tests: BreadcrumbInboundMessage_NullType/NullRowId, BreadcrumbOutboundMessage_NullType, BreadcrumbRenderMessage_NullHtml, BreadcrumbSubfolderResultMessage_NullRequestId/NullRowId/NullChildren; each asserts ArgumentNullException with the expected ParamName.
- `git grep -n -e BreadcrumbMessagesIssue1002Tests -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` returns exactly one hit (the Compile Include).
- `dotnet tool run csharpier check .`: "Checked 1675 files in 4820ms.", exit 0.
