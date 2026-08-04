# P5 OpenCoordinator Line-Limit Split CSharpier Gate

Timestamp: 2026-07-22T12:48:51Z

Command: `TOOL="/c/Users/DanMoisan/.dotnet/tools/csharpier.exe"; F1="QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs"; F2="QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs"; "$TOOL" format "$F1" "$F2" --log-level Information; "$TOOL" format "$F1" "$F2" --log-level Information; "$TOOL" check "$F1" "$F2" --log-level Information; wc -l "$F1" "$F2"`

EXIT_CODE: 0

Output Summary: PASS. Authoritative `csharpier format` (mutating, on-disk) was applied to exactly the two OpenCoordinator partial test sources; a second `format` pass produced no further change and the scoped `csharpier check` over the same two files returned exit code 0. Final physical line counts: `BreadcrumbDropDownOpenCoordinatorTests.cs` = 386 lines, `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` = 144 lines; both are within the 480-line hard cap for this batch. `csharpier pipe-files` was not used at any point. The split was required because genuine `csharpier format` expands the previously committed single 395-line source (which the prohibited pipe-files gate mis-measured as stable) beyond the 480/500 limit.
