# [expect-fail] Fail-Before Compile Gate (P1-T5)

Timestamp: 2026-08-28T15-54
Command (CR-MSBUILD then CR-NULLABLE, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'
```

Full output teed to
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p1-t5-expectfail-build.msbuild.txt`.

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

```
    5 Warning(s)
    22 Error(s)

Time Elapsed 00:00:19.08
```

The 5 warnings are the same pre-existing `System.Reactive.PackagesConfigCheck.targets` advisory
recorded at baseline (P0-T7 / P0-T8) and are unrelated to this change.

### Error-code histogram (22 unique diagnostics)

| Code | Count | Class |
|---|---|---|
| CS1061 | 20 | missing member — the guard surface does not exist at baseline |
| CS1503 | 2 | collateral argument-type failure on the two `Returns(...)` setups whose property (`IsWebView2Focused`) is one of the missing members |

### Affected-file list (all 22 diagnostics)

| File | Unique diagnostics |
|---|---|
| `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` | 18 |
| `QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs` | 3 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs` | 1 |

Every diagnostic is located in one of the three new test files authored by P1-T1..P1-T3. No error
is raised in any production file, and only project node 12 (`QuickFiler.Test`) failed — the
production assemblies compiled cleanly, which is what makes this a genuine *absence* proof rather
than a broken build.

### Distinct missing-member diagnostics (proving each guard-surface member is absent)

```
CS1061: 'BreadcrumbDropDownHost' does not contain a definition for 'MayTakeFocus' ...
CS1061: 'IQfcFormViewer' does not contain a definition for 'FormDeactivated' ...
CS1061: 'IQfcFormViewer' does not contain a definition for 'IsWebView2Focused' ...
CS1061: 'IQfcFormViewer' does not contain a definition for 'ParkFocusOffWebView2' ...
CS1061: 'IQfcItemController' does not contain a definition for 'CancelBreadcrumbSelector' ...
CS1061: 'IItemViewer' does not contain a definition for 'CancelBreadcrumbSelector' ...
CS1061: 'QfcItemControllerCancelBreadcrumbSelectorTests.CancelController' does not contain a definition for 'CancelBreadcrumbSelector' ...
CS1503: Argument 1: cannot convert from 'bool' to '?'
```

All five required member names are present in the error set:

- `MayTakeFocus` — the typed property assignment in `PredicateHarness`
  (`BreadcrumbDropDownHostTests.Part3.cs:323`), which is the compile-time reference decision D2
  requires; it is a typed assignment, not a reflection lookup, so its absence is a compile error
  rather than a runtime one.
- `FormDeactivated`
- `IsWebView2Focused`
- `ParkFocusOffWebView2`
- `CancelBreadcrumbSelector` — on both `IQfcItemController` (the form-controller fan-out hop) and
  `IItemViewer` (the viewer hop), plus on the concrete controller under test.

This is the fail-before evidence for the bugfix workflow: the regression tests reference a guard
surface that genuinely does not exist before the Phase 2/3 fix, so no test assembly can be produced
and no failing *run* is structurally possible. See the companion dossier
`fail-before-exception.2026-08-28T15-55.md`.

## Artifact sanitisation

The teed msbuild log was sanitised in binary mode with case-insensitive substitutions before
staging, per the repository-wide "never embed absolute host paths" rule. Substituted token classes:
workspace-root prefix (both separator spellings), user-profile prefix (both separator spellings),
host identifier, account identifier. 13,070 substitutions applied. Post-condition sweeps
(case-insensitive, fixed-string) return 0 hits for the account identifier, the host identifier,
`:\Users\` and `:/Users/`.
