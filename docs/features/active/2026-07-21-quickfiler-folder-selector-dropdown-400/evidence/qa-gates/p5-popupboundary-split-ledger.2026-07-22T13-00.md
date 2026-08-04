# P5 PopupBoundary Line-Limit Split Ledger

Timestamp: 2026-07-22T13:00:45Z

Command: `F1="QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs"; F2="QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs"; wc -l "$F1" "$F2"; grep -c '\[TestMethod\]' "$F1" "$F2"; git diff --unified=0 QuickFiler.Test/QuickFiler.Test.csproj | grep -E "^\+.*Compile Include"; git status --short | grep -vE "evidence|remediation-plan"`

EXIT_CODE: 0

Output Summary: PASS. No contradiction; atomic replanning not required.

- Test-name preservation: the two partial files together declare exactly the 18 original test names (5 `[TestMethod]` in the primary `BreadcrumbPopupBoundaryCoverageTests.cs`, 13 in `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`); the VSTest ListTests + 18/18 pass-after (`p5-popupboundary-split-pass-after.2026-07-22T13-00.md`) confirms one-for-one identity with the pre-split file and unchanged assertions (bodies copied verbatim; behavior unchanged).
- Line limits: primary = 361 lines, Part2 = 220 lines; both at most 480 lines.
- Project include: exactly one `QuickFiler.Test.csproj` `Compile Include` was added by this batch, for `Viewers\BreadcrumbPopupBoundaryCoverageTests.Part2.cs`. The second added include shown in the csproj diff (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`) belongs to the immediately-prior OpenCoordinator split batch (P5-T154), not this one.
- Scope: the only non-evidence, non-plan changes are the two OpenCoordinator split files, the two PopupBoundary split files, and `QuickFiler.Test.csproj`. No production, other-test, runsettings, `coverage.config`, threshold, filter, or exclusion file changed.
