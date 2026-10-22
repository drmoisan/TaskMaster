# Phase 6 — Coverage-Enabled Full Test Suite

Timestamp: 2026-08-26T11-30
Task: [P6-T5]
Command: `pwsh -NoProfile -File "scripts\vscode\Invoke-MSTestWithCoverage.ps1" -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
EXIT_CODE: 1

RunDisposition: TESTS_FAILED (1 failed test)

This is the exact command from [P0-T9].

## Acceptance status

**This task does NOT meet its acceptance condition.** The condition is zero failed tests; the run
records one. The cause is a conflict between two constraints this plan imposes on itself, described
in full below. It is not a defect in the change under test and it is not an environmental failure.

## Output Summary

### Test counts

```
Test Run Failed.
Total tests: 6518
     Passed: 6517
     Failed: 1
```

- Passed: **6517**
- Failed: **1**
- Skipped: **0** (none reported)

Baseline ([P0-T9]) was 6503 total, 6503 passed, 0 failed. The total rose by 15: this feature added
17 tests across the two owned test files and deleted 2
(`QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract` by [P1-T8] and
`NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` by [P5-T11]).

### The single failing test

`QuickFiler.Controllers.Tests.EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`

> Test method QuickFiler.Controllers.Tests.EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields threw exception:
> System.ArgumentException: Object of type 'System.Boolean' cannot be converted to type 'System.Int32'.
>    at System.Reflection.RtFieldInfo.SetValue(Object obj, Object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
>    at QuickFiler.Controllers.Tests.EfcHomeControllerTests.SetField(Object target, String fieldName, Object value) in QuickFiler\Controllers\EfcHomeControllerTests.cs:line 50
>    at QuickFiler.Controllers.Tests.EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields() in QuickFiler\Controllers\EfcHomeControllerTests.cs:line 64

#### Diagnosis

`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:64` reads:

```csharp
SetField(controller, "_isExecuting", true);
```

`SetField` is a reflection helper that calls `FieldInfo.SetValue`. [P3-T5] changed
`_isExecuting` from `private volatile bool` to `private int`, as mandated by that task and by
AC-14, so the runtime now rejects the boxed `System.Boolean` for a `System.Int32` field.

The test's intent is to place the guard in its taken state before calling `ExecuteMovesAsync`. Under
the new primitive the equivalent value is `1`. The one-line delta that resolves the failure is:

```csharp
SetField(controller, "_isExecuting", 1);
```

#### Why it was not applied

`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` is on this plan's **forbidden to write**
list, and [P7-T6] gates that list with an acceptance condition of "the command produces no output
lines". The delegating instruction marks the ownership boundary as hard and states that three
sibling features (446, 468, 498) are executing concurrently against the same integration branch, so
a write outside the owned surface breaks fan-in.

The two constraints are irreconcilable by any production-side change:

- AC-14 requires `_isExecuting` to be declared `private int`.
- The forbidden test injects a `System.Boolean` into that field by name through reflection.
- `FieldInfo.SetValue` performs a widening/identity type check that rejects `Boolean` for `Int32`.
  No accessibility, attribute, or overload change on the production side alters that outcome, and
  the field name is matched literally by the test, so it cannot be renamed either.

Reverting [P3-T5] would satisfy the sibling test but violate AC-14, [P3-T5], and leave root cause
RC-6 unfixed, which is a core deliverable of #451. Editing the sibling test would satisfy the suite
but violate the hard ownership boundary and fail [P7-T6] by construction.

The production change was kept and the ownership boundary was respected. The conflict is escalated
rather than resolved unilaterally.

The plan's settled decision 8 enumerates four tests expected to break deliberately; this test is
**not** among them, and [P0-T12] was directed to check this same forbidden file only against the
`int`-to-`double` widening (assumption A-10), not against the `_isExecuting` primitive change. The
omission is a gap in the plan, not in its execution.

### Coverage artifact state

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:236` throws when the underlying dotnet-coverage/vstest
process exits non-zero. That throw fired here, **before** `ConvertTo-KoverageCoberturaXml` at `:340`
and before the `Set-Content` at `:344`. The log confirms it: neither
`Post-processing coverage XML for Koverage compatibility...` nor `Done. Coverage artifact:` appears.

The Cobertura file left on disk is therefore the **raw dotnet-coverage output**, not the
post-processed form:

- `filename` attributes are absolute host paths rather than repository-relative,
- there is no `<sources>` element,
- third-party and vendored packages are still present in the denominator,
- one `<class>` element per compiler-generated type, not pre-merged per file.

Per-file line-rates below were therefore computed by matching the absolute `filename` attributes by
suffix against the five owned production files, and aggregating every `<class>` element that shares
the same file, keyed by line number. That is the identical aggregation used on the [P0-T9]
post-processed baseline, so the **per-file** figures are method-comparable across the two documents.
The **repository-wide** figures are not comparable, because the raw denominator includes third-party
code the post-processed denominator excludes. [P6-T6] records that distinction explicitly.

### Repository-wide rates (raw document, not comparable to the post-processed baseline)

| Metric | Raw attribute | Percentage |
| --- | --- | --- |
| `line-rate` | `0.7027563152803458` | **70.28%** |
| `branch-rate` | `0.5886675444930553` | **58.87%** |

Supporting counters: `lines-covered="57392"`, `lines-valid="81667"`,
`branches-covered="13859"`, `branches-valid="23543"`.

### Per-file line-rate, five owned production files

| Owned production file | Line-rate | Covered | Total | `<class>` elements merged |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | **76.23%** | 170 | 223 | 8 |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | **80.00%** | 88 | 110 | 3 |
| `QuickFiler/Controllers/EfcHomeController.cs` | **98.25%** | 224 | 228 | 15 |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | **100.00%** | 64 | 64 | 2 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | **82.61%** | 57 | 69 | 5 |

All five values are numeric, satisfying that half of the acceptance condition. The
zero-failed-tests half is not satisfied.
