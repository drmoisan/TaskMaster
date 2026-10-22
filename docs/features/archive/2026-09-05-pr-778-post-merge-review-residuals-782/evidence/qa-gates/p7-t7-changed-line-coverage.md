# QA Gate — Changed-Line Coverage Delta (P7-T7, AC9 and AC-U5)

Timestamp: 2026-09-05T23-12

Command:

```powershell
git diff pre-782-base..HEAD -- UtilitiesCS/Threading/UiThread.cs UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs UtilitiesCS/Threading/ProgressTracker.cs UtilitiesCS/Threading/ProgressTrackerAsync.cs TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs
```

Lookup method:

```powershell
[xml]$doc = Get-Content -LiteralPath 'coverage\782-p7-final.cobertura.xml'
$classNodes = $doc.SelectNodes('//class')
# For each production file, select every <class> whose filename attribute ends with the
# backslash-separated form of that path, then take .//line under each.
# A changed line number is counted once and is covered when ANY matching element has hits > 0.
```

The diff is anchored to `pre-782-base` with an explicit ref operand. Every added line is mapped to
its post-change line number from the hunk headers.

The lookup uses the all-descendant `.//line` selection pinned by SD22 in P0-T7 and P7-T5, not
`classes/class/lines/line`, which yields 65896 against this document, nor
`classes/class/methods/method/lines/line`, which yields 67065 against it. Because that selection
reaches a line both at class level and inside its method, one changed line number can match more
than one `<line>` element; each changed line number is counted once and is treated as covered when
any matching element for that file carries `hits` greater than zero. A line number that matches no
element is not executable and is excluded from both numerator and denominator.

Changed-line coverage is covered over covered-plus-uncovered.

**Filename matching note.** The `filename` attribute in this document carries an absolute path with
backslash separators. A forward-slash match returns zero rows and would make this gate unevaluable,
so the matcher converts each path to its backslash form and matches on the suffix.

EXIT_CODE: 0

Output Summary:

## Changed line numbers per file

| File | Added lines | Line numbers |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 28 | 135-152, 157-160, 162-166, 168 |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 5 | 57, 58, 59, 60, 65 |
| `UtilitiesCS/Threading/ProgressTracker.cs` | 1 | 39 |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 1 | 39 |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 2 | 72, 115 |

## Executable subset and coverage

| File | Added | Executable | Covered | Uncovered |
|---|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 28 | 4 | 4 | 0 |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 5 | 1 | 1 | 0 |
| `UtilitiesCS/Threading/ProgressTracker.cs` | 1 | 1 | 1 | 0 |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 1 | 1 | 1 | 0 |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 2 | 0 | 0 | 0 |
| **Total** | **37** | **7** | **7** | **0** |

```text
CHANGED_LINE_COVERAGE=100.00%
```

The gap between 37 added lines and 7 executable ones is the non-executable remainder: the
`internal const string DispatcherNotInitializedMessage` declaration and its literal, the added XML
documentation block on the `Dispatcher` property, and the added explanatory comments. None of those
appears in the Cobertura document, so each is excluded from both numerator and denominator rather
than counted as uncovered.

## Condition 1 — enumeration of uncovered changed lines

```text
UNCOVERED_ENUMERATION_COUNT=0
```

**The enumeration is empty.** Every added executable line in the changed set is covered, which is
what the plan expects after SD18 withdrew the `try`/`catch` construct that the previous form of this
condition exempted.

The covering tests, by line:

- the getter's single field read, its null test, its throw, and its return are exercised by
  `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` and
  `Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance`;
- the `WpfDispatcherYield` throw is exercised by `YieldAsync_WithoutDispatcher_RemainsStrict`;
- the two `UiDispatcher = UiDispatcher,` initializer lines are exercised by the `ProgressTracker`
  and `ProgressTrackerAsync` initialization tests.

All five of those tests are recorded as `Passed` in
`evidence/qa-gates/p7-t5-tests-coverage.md`.

## Condition 2 — aggregate first-party comparison

Both sides are drawn from the SD23-corrected set. The baseline side is read from the re-recorded
`evidence/baseline/p0-t7-coverage.md`; the post-change side from
`evidence/qa-gates/p7-t5-tests-coverage.md`. Neither superseded figure — 112359 or 26496 — is used.

### Comparability of the denominators

| Side | first-party `lines-valid` |
|---|---|
| Baseline (`p0-t7-coverage.md`) | 132967 |
| Post-change (`p7-t5-tests-coverage.md`) | 132961 |

The difference is 6 lines, which is 0.0045% of the baseline denominator and therefore **well within
1%**. The two runs are comparable and the aggregate comparison is asserted rather than waived. No
`COVERAGE COMPARISON: NOT COMPARABLE` record is required.

### The comparison

| Metric | Baseline | Post-change | Change | Floor (baseline minus 0.50pp) | Verdict |
|---|---|---|---|---|---|
| First-party line | 84.50% | 84.51% | +0.01pp | 84.00% | PASS |
| First-party branch | 79.15% | 79.15% | 0.00pp | 78.65% | PASS |

Underlying counters: line `112355/132967` to `112363/132961`; branch `26500/33480` to
`26500/33480`.

## Condition 3 — `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`

That file contributes **zero executable changed lines**, and the lookup returns zero `<class>`
elements for it rather than a set of uncovered lines.

The reason is that `TaskMaster/Ribbon/RibbonViewer.cs` line 32 declares the partial type
`[ExcludeFromCodeCoverage]`:

```csharp
    [System.Runtime.InteropServices.ComVisible(true)]
    [ExcludeFromCodeCoverage]
    public partial class RibbonViewer : Office.IRibbonExtensibility
```

The attribute applies to every part of the partial type, including
`RibbonViewer.EngineCommands.cs`, so the instrumentation emits no `<class>` element for it at all.
Its two changed lines, 72 and 115, are therefore absent from the document. **This is recorded here
rather than reported as a spurious zero-coverage row**, which is what an enumeration that treated an
absent file as fully uncovered would have produced.
