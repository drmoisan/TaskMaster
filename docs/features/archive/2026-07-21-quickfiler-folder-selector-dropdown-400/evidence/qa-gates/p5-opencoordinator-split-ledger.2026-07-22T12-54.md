# P5 OpenCoordinator Line-Limit Split Ledger

Timestamp: 2026-07-22T12:54:52Z

Command: `F1="QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs"; F2="QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs"; wc -l "$F1" "$F2"; grep -c '\[TestMethod\]' "$F1" "$F2"; grep -n "CapturingSynchronizationContext =" "$F1"; git diff --unified=0 QuickFiler.Test/QuickFiler.Test.csproj | grep -E "^\+.*Compile Include"; git status --short | grep -Ei "OpenCoordinator|csproj"`

EXIT_CODE: 0

Output Summary: PASS. No contradiction; atomic replanning not required.

- Test-name preservation: the two partial files together declare exactly the 10 original test names (5 `[TestMethod]` in the primary `BreadcrumbDropDownOpenCoordinatorTests.cs`, 5 in `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`); the VSTest ListTests + 10/10 pass-after (`p5-opencoordinator-split-pass-after.2026-07-22T12-54.md`) confirms one-for-one identity with the pre-split file and unchanged assertions (bodies copied verbatim; behavior unchanged).
- `CapturingSynchronizationContext` alias: unchanged, present once in the primary file at line 12 (`using CapturingSynchronizationContext = QuickFiler.Test.Viewers.BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext;`).
- Line limits: primary = 386 lines, Part2 = 144 lines; both at most 480 lines.
- Project include: exactly one `QuickFiler.Test.csproj` `Compile Include` added, for `Viewers\BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`; no other include changed.
- Scope: only `BreadcrumbDropDownOpenCoordinatorTests.cs`, the new `.Part2.cs`, and `QuickFiler.Test.csproj` changed. No production, other-test, runsettings, `coverage.config`, threshold, filter, or exclusion file changed.
