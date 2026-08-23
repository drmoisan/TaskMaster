# P5 PopupBoundary Line-Limit Split CSharpier Gate

Timestamp: 2026-07-22T12:58:08Z

Command: `TOOL="/c/Users/DanMoisan/.dotnet/tools/csharpier.exe"; F1="QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs"; F2="QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs"; "$TOOL" format "$F1" "$F2"; "$TOOL" format "$F1" "$F2"; "$TOOL" check "$F1" "$F2"; wc -l "$F1" "$F2"`

EXIT_CODE: 0

Output Summary: PASS. Authoritative `csharpier format` (mutating, on-disk) was applied to exactly the two PopupBoundary partial test sources; a second `format` pass produced no further change and the scoped `csharpier check` over the same two files returned exit code 0. Final physical line counts: `BreadcrumbPopupBoundaryCoverageTests.cs` = 361 lines, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` = 220 lines; both are within the 480-line hard cap for this batch. `csharpier pipe-files` was not used at any point. The split was required because genuine `csharpier format` expands the previously committed single 479-line source (which the prohibited pipe-files gate mis-measured as stable) to 562 lines, beyond the 480/500 limit.
