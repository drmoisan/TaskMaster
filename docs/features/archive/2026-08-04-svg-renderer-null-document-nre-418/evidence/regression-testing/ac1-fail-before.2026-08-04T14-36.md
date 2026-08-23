# AC-1 Fail-Before Evidence — Pre-Fix Regression Failures (Issue #418, task P1-T9) [expect-fail]

Timestamp: 2026-08-04T18-22

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P1-T9]` `[expect-fail]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `296eac953c5ac3f69c429c7554ab47218e64e852`
Tree state: `[P1-T8]` regression tests present; **no production fix applied yet** (`[P1-T10]` onward not started)

A failing test run is the expected and required outcome for this task, per the Bugfix Workflow in
`CLAUDE.md`: the deterministic regression test is written and observed failing before any
production change.

## Commands

### 1. Build (plan-commanded)

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU
```

EXIT_CODE: 0 — Build succeeded, **0 errors, 0 warnings**. The new
`SvgRendererParseContractTests.cs` compiled, confirming the `[P1-T4]` direct `Svg` reference
resolves the `SvgDocument` type dependency (no `CS0012`).

### 2. Test run (plan-commanded)

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot SVGControl.Test -Configuration Debug
```

EXIT_CODE: 1

This invocation **did not reach the test runner**. It terminated inside the wrapper script with:

```
Invoke-MSTest.ps1: The property 'Count' cannot be found on this object. Verify that the property exists.
```

Root cause, diagnosed and verified (see "Harness defect" below): a pre-existing defect in
`scripts/vscode/Invoke-MSTest.ps1` that manifests whenever assembly discovery yields exactly one
assembly. It is not a test failure and not a defect in any file this plan changes.

### 3. Test run (faithful equivalent, actually executed)

Because `scripts/vscode/Invoke-MSTest.ps1` is **outside the issue #418 Scope Lock** and was
therefore not edited, the suite was executed by invoking `vstest.console.exe` with the identical
argument list that the wrapper's own pure `Get-VsTestArgumentList` function constructs
(`scripts/vscode/Invoke-MSTest.ps1:37-55`): the discovered assemblies, `/Settings:` pointing at
`scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, and
`/TestCaseFilter:TestCategory!=LiveOutlook`.

```
vstest.console.exe C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\bin\Debug\SVGControl.Test.dll /Settings:C:\Users\DanMoisan\repos\TaskMaster\scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook
```

EXIT_CODE: 1 (non-zero, as required by this `[expect-fail]` task)

## Output Summary

**Total tests: 41. Passed: 37. Failed: 4. Skipped: 0.**

All four failures are the four `[P1-T8]` regression tests, and every failure is the issue #418
defect itself — a `System.NullReferenceException` raised inside the `SvgRenderer` byte-array
constructor.

| # | Failed test | Duration |
|---|---|---|
| 1 | `Constructor_WithMalformedBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull` | 123 ms |
| 2 | `Constructor_WithMalformedBytesAndMargin_DoesNotThrowAndLeavesDocumentNull` | 2 ms |
| 3 | `Constructor_WithEmptyBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull` | 1 ms |
| 4 | `Constructor_WithEmptyBytesAndMargin_DoesNotThrowAndLeavesDocumentNull` | 1 ms |

**Pre-existing tests in same run: 37 total, 37 passed, 0 failed.** `Invoke-MSTest.ps1 -SearchRoot
SVGControl.Test` discovers `*.Test.dll` under `bin\Debug\` and therefore runs the whole assembly,
so the pre-existing suites execute alongside the new tests. Breakdown by source file:
`GetRelativePath_Test` contributes 9 test cases (9 `[TestMethod]`); `RelativePathCoverageTests`
contributes 28 test cases (12 `[TestMethod]` plus 4 `[DataTestMethod]` expanding to 16 `[DataRow]`
cases). 9 + 28 = 37, which reconciles exactly with the reported pass count. No pre-existing test
regressed.

### Verbatim failure detail — malformed-bytes case (test #1)

```
Failed Constructor_WithMalformedBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull [123 ms]
Error Message:
 Did not expect any exception because issue #418 requires the byte-array constructor to degrade
 rather than throw when the payload cannot be parsed, but found
 System.NullReferenceException: Object reference not set to an instance of an object.
 at SVGControl.SvgRenderer..ctor(Byte[] doc, Size size, AutoSize autoSize) in C:\Users\DanMoisan\repos\TaskMaster\SVGControl\SvgRenderer.cs:line 133
 at SVGControl.Test.SvgRendererParseContractTests.<>c__DisplayClass2_0.<Constructor_WithMalformedBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull>b__0() in C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\SvgRendererParseContractTests.cs:line 35
```

The required `SVGControl.SvgRenderer..ctor` stack frame is present, and it points at
`SVGControl/SvgRenderer.cs:line 133` — the statement `_original = _doc.Draw().Size;`. This
confirms the defect mechanism described in the research artifact end to end: `GetSvgDocument`
swallowed the parse failure in its `catch (Exception)` and returned `null`, the constructor's
`GetSvgDocument(doc)!` null-forgiving operator suppressed the compiler's null warning, and the
immediately following `_doc.Draw()` dereferenced null.

### Verbatim failure detail — element-free case (test #3)

```
Failed Constructor_WithEmptyBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull [1 ms]
Error Message:
 Did not expect any exception because an element-free payload must not surface as a constructor
 exception, but found System.NullReferenceException: Object reference not set to an instance of an object.
 at SVGControl.SvgRenderer..ctor(Byte[] doc, Size size, AutoSize autoSize) in C:\Users\DanMoisan\repos\TaskMaster\SVGControl\SvgRenderer.cs:line 133
```

Same NRE at the same line, reached by the distinct exception-free path: `SvgDocument.Open` returns
`null` for element-free input without throwing at all. Both failure shapes therefore converge on
the same unguarded dereference, which is what `[P1-T14]` must fix.

The two four-argument-overload failures (tests #2 and #4) are identical in shape, differing only
in the constructor frame signature `..ctor(Byte[] doc, Size size, Padding margin, AutoSize autoSize)`
and reaching the equivalent unguarded dereference at `SVGControl/SvgRenderer.cs:line 143`.

## Harness defect in `scripts/vscode/Invoke-MSTest.ps1` (out of scope, reported not fixed)

`scripts/vscode/Invoke-MSTest.ps1:115` evaluates `$testAssemblies.Count`, while line 77 sets
`Set-StrictMode -Version Latest`. When `Get-ChildItem ... | Select-Object -ExpandProperty FullName`
matches exactly one file it returns a scalar `System.String` rather than an array, and StrictMode
`Latest` rejects `.Count` on a scalar. Verified empirically in isolation:

```
pwsh -NoProfile -Command "Set-StrictMode -Version Latest; $s = 'one'; $s.Count"
  -> The property 'Count' cannot be found on this object. Verify that the property exists.
pwsh -NoProfile -Command "Set-StrictMode -Version Latest; $a = @('one','two'); $a.Count"
  -> 2
```

`-SearchRoot SVGControl.Test` discovers exactly one assembly
(`SVGControl.Test/bin/Debug/SVGControl.Test.dll`), so the plan-commanded form in `[P1-T9]` and
`[P1-T23]` can never execute regardless of test outcome. `-SearchRoot .` discovers nine assemblies
and is unaffected, which is why the Phase 0 baseline and `[P2-T6]` are unaffected.

`scripts/vscode/Invoke-MSTest.ps1` does not appear in this plan's Scope Lock, so it was **not**
edited. The defect is reported to the orchestrator for separate disposition. The equivalent
invocation recorded above is a faithful substitute: it runs the same executable, the same
assembly, the same runsettings, and the same test-case filter that the wrapper would have passed.

## Cross-reference

Post-fix counterpart: `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` (`[P1-T23]`).
