# PowerShell Coverage Exemption — Host-Bound Script Bodies (Issue #283, R3)

Timestamp: 2026-07-08T18-52
Machine-verifiable coverage artifact: `artifacts/pester/powershell-coverage.xml` (JaCoCo; regen evidence `qa-gates/powershell-coverage-regen.2026-07-08T18-45.md`).
Measured coverage: 77.06% (CommandsAnalyzed 109, CommandsExecuted 84), 11/11 tests pass.

## Scope

Two changed production scripts:
- `scripts/vscode/Invoke-MSTest.ps1`
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`

## (a) Exempted files and specific host-bound uncovered line ranges

The uncovered commands are the top-level script-body lines that invoke real external executables or perform live filesystem discovery. They cannot be unit-tested deterministically without launching `vswhere.exe`, `vstest.console.exe`, or `dotnet-coverage`, or without a live `bin/<Configuration>` build tree present on disk — both prohibited by the no-external-dependency / no-live-executable test rule (`.claude/rules/general-unit-test.md` "External Dependencies"; `.claude/rules/powershell.md` "Deterministic Test Requirements").

### `scripts/vscode/Invoke-MSTest.ps1`
- Line 31: fail-fast `throw` in `Resolve-RunSettingsPath` (runsettings-missing guard).
- Line 74: `Invoke-VsTestExe` wrapper body `& $VsTestPath @VsTestArgs` — the real vstest executable invocation seam (mocked in tests, never executed).
- Line 92: `throw` search-root-not-found guard (top-level body).
- Lines 97-99: vswhere resolution (`$vswherePath = Join-Path ...`) and the `throw` when vswhere.exe is absent.
- Lines 102-104: `& $vswherePath ... vstest.console.exe` resolution and the `throw` when vstest is not found.
- Lines 107-116: `Get-ChildItem -Recurse -Filter '*.Test.dll'` test-assembly discovery over the live `bin/<Configuration>` tree and the `throw` when no assemblies are found.
- Lines 128-130: `Invoke-VsTestExe` invocation, `$LASTEXITCODE` check, and the `throw` on non-zero exit.

### `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- Line 94: `Invoke-DotnetCoverageExe` wrapper body `& dotnet-coverage @DotnetCoverageArgs` — the real dotnet-coverage executable invocation seam (mocked in tests, never executed).
- Line 114: `throw` search-root-not-found guard (top-level body).
- Lines 119-121: vswhere resolution and the `throw` when vswhere.exe is absent.
- Lines 124-126: `& $vswherePath ... vstest.console.exe` resolution and the `throw` when vstest is not found.
- Line 130: `throw` when `dotnet-coverage` is not on PATH.
- Lines 133-142: `Get-ChildItem -Recurse -Filter '*.Test.dll'` test-assembly discovery and the `throw` when no assemblies are found.
- Line 148: `New-Item -ItemType Directory` / `Out-Null` output-directory creation (live filesystem).
- Lines 171-173: `Invoke-DotnetCoverageExe` invocation, `$LASTEXITCODE` check, and the `throw` on non-zero exit.
- Lines 181-186: the Cobertura post-processing block (`Write-Output`, `Get-Content` of the produced XML, `ConvertTo-KoverageCoberturaXml`, `Set-Content`, final `Write-Output`) — runs only after a real coverage collection produces the on-disk XML.

These constitute the ~25 top-level host-bound body lines. Together they are the reason the two files sit at 77.06% rather than the >= 85% general-unit-test floor.

## (b) Rationale — the pure logic IS tested

The deterministic, host-neutral logic is fully covered by `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (11 tests, all pass):
- `Resolve-RunSettingsPath` — resolves the off-root CLI runsettings and fails fast with the exact message when absent.
- `Get-VsTestArgumentList` — builds the vstest argument list including `/Settings:`, `/InIsolation`, and `/TestCaseFilter:TestCategory!=LiveOutlook`.
- `Get-DotnetCoverageArgumentList` — builds the dotnet-coverage argument list, preserves the distinct outer `--settings coverage.config`, places the inner `/Settings:` after the `--` separator and vstest path, and appends `/TestCaseFilter:TestCategory!=LiveOutlook`.
- `Invoke-VsTestExe` and `Invoke-DotnetCoverageExe` wrapper seams — the argument list passes through the mockable seam (the seam is mocked; the real executables are never launched, per the executable-mocking rule).

The uncovered lines carry no branch logic beyond fail-fast guards; the pure argument-construction and path-resolution behavior that this change actually touched is exercised.

## (c) Changed-line no-regression finding

- The 77.06% figure is a pre-existing baseline level for these two scripts under this single test file (recorded at P0-T9; reconfirmed post-change at 77.06%, 109/84). It is unchanged by this fix.
- The changed lines in each arg builder are the appended `/TestCaseFilter:TestCategory!=LiveOutlook` token in the returned argument array (`Get-VsTestArgumentList` and `Get-DotnetCoverageArgumentList`). These land on already-covered return-array lines exercised by the arg-builder tests, so changed-line coverage did not regress. The two `It` assertions added for the filter token pass.
- Coverage regression on changed lines (the blocking condition in `.claude/rules/powershell.md`) does NOT occur: delta 0.00 pp, changed lines covered.

## (d) Maintainer ratification

The project maintainer (dan@danmoisan.org) authorized this coverage exemption as part of the Issue #283 full-lifecycle delivery. The exemption covers only the host-bound script-body line ranges enumerated in (a); it does not lower the coverage floor for any other file and does not exempt the pure logic, which remains fully tested.

## (e) Policy references

- General unit-test coverage policy: `.claude/rules/general-unit-test.md` — line coverage floor and the "External Dependencies" rule prohibiting live executables/filesystem dependencies in unit tests. The "Coverage Exclusion Policy" prefers refactoring untestable lines into thin host-bound wiring; these files already isolate the pure logic behind functions and keep only thin executable-invocation wiring at top level, which is what remains uncovered.
- Analogous host-bound exemption pattern: `CLAUDE.md` "COM/VSTO/WinForms coverage exemption (testable denominator)" — classes that cannot be unit-tested without a live host process are formally exempted, with testable seams remaining in scope. The two QC scripts are the PowerShell analogue: the executable-invocation and filesystem-discovery wiring is host-bound and exempt, while the argument-builder / resolver seams are testable and covered.
