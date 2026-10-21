# P1-T10 "cannot marshal" census over *.cs

Timestamp: 2026-10-01T07-25
Command: git grep -n -F "cannot marshal" -- "*.cs"
EXIT_CODE: 0
Output Summary:
- Exactly two hit lines, no other path:
  - QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:86 (the line 86 assertion, Contain("cannot marshal")).
  - QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:101 (the unchanged Dispatch production literal).
- Before the fix the same search printed four hits (fact 5): the two above plus BreadcrumbUiThreadDispatchTests.cs:305 and BreadcrumbUiDispatcher.cs:183, both now updated.
- AC7 checked off in FEATURE/issue.md.
