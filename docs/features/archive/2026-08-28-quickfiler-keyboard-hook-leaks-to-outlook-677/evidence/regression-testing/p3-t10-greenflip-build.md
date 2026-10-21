# Green-Flip Compile Gate (P3-T10)

Timestamp: 2026-08-28T16-02
Command (CR-MSBUILD then CR-NULLABLE, fully expanded — byte-identical to the P1-T5 command):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'
```

Full output teed to
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p3-t10-greenflip-build.msbuild.txt`.

EXIT_CODE: 0

## Output Summary

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.40
```

- Errors: **0**. The 22 compiler diagnostics recorded by the P1-T5 `[expect-fail]` gate are all
  resolved.
- Warnings: **5** — the same pre-existing `System.Reactive.PackagesConfigCheck.targets`
  packages.config advisory present at the P0-T7 / P0-T8 baseline. No new warning was introduced,
  and none was promoted to an error by `/p:TreatWarningsAsErrors=true`.

## The red-to-green flip was produced by production code, not by weakening a test

`git status --porcelain -- QuickFiler.Test/` lists exactly five paths and nothing else:

```
 M "QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs"
 M QuickFiler.Test/QuickFiler.Test.csproj
?? QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs
?? QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs
?? QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs
```

The three untracked files are the P1-T1..P1-T3 regression tests themselves. `QuickFiler.Test.csproj`
carries only the three `<Compile Include>` items that wire them in. The only edit to a pre-existing
test file is the single sanctioned structural enabler of P3-T7, whose complete diff is:

```
@@ -398,0 +399,4 @@ namespace QuickFiler.Test.HelperClasses
+            // Issue #677 structural enabler: completes IQfcItemController after the additive
+            // deactivate-cancel member. No test behavior in this file changes.
+            public void CancelBreadcrumbSelector() { }
```

That hunk is purely additive, sits inside the manual fake `FakeQfcItemController`, and touches no
`[TestMethod]`, no assertion, and no test body. No assertion, test method, or `[TestMethod]` body in
any test file was changed between P1-T5 and this task.

## Artifact sanitisation

The teed msbuild log was sanitised in binary mode with case-insensitive substitutions before
staging, per the repository-wide "never embed absolute host paths" rule. Substituted token classes:
workspace-root prefix (both separator spellings), user-profile prefix (both separator spellings),
host identifier, account identifier. 13,301 substitutions applied. Post-condition sweeps
(case-insensitive, fixed-string) return 0 hits for the account identifier and the host identifier.
