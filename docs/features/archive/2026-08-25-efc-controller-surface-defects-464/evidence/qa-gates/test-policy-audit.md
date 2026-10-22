# [P10-T10] Test-policy audit

Timestamp: 2026-08-28T02-08
Task: [P10-T10]
Command: fixed-string and regex searches with `grep -c -F` / `grep -oE` across the four test files this
feature writes, plus source inspection of their class declarations and `using` directives
EXIT_CODE: 0

The four files audited:

```
QuickFiler.Test/Controllers/EfcFormControllerTests.cs
QuickFiler.Test/Controllers/EfcItemControllerTests.cs
QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs
QuickFiler.Test/Controllers/EfcViewerTests.cs
```

## The two required fixed-string searches

`grep -c -F 'Thread.Sleep'` and `grep -c -F 'Task.Delay'`, per file:

| File | `Thread.Sleep` | `Task.Delay` |
|---|---|---|
| `EfcFormControllerTests.cs` | **0** | **0** |
| `EfcItemControllerTests.cs` | **0** | **0** |
| `EfcItemController.CleanupTests.cs` | **0** | **0** |
| `EfcViewerTests.cs` | **0** | **0** |

**Both searches return zero matching lines across all four files.**

## Additional determinism and isolation searches

Counts are totals across all four files.

| Pattern | Matches | Meaning |
|---|---|---|
| `DateTime.Now` | **0** | no wall-clock read |
| `DateTime.UtcNow` | **0** | no wall-clock read |
| `new Random` | **0** | no unseeded randomness |
| `Path.GetTempFileName` | **0** | no temporary file |
| `Path.GetTempPath` | **0** | no temporary path |
| `File.WriteAllText` | **0** | no file write |
| `File.Create` | **0** | no file creation |
| `.Show()` | **0** | no dialog or form shown |
| `Application.Run` | **0** | no message pump started |
| `BackgroundWorker` | **0** | no background worker started |

## Frameworks and libraries used by every added test

| Measure | Count | Verdict |
|---|---|---|
| `[TestClass]` | 4 (one per file, at `:16`, `:26`, `:25`, `:27`) | MSTest |
| `[TestMethod]` | 41 | MSTest |
| `[DataTestMethod]` | 1 | MSTest |
| `[DataRow(` | 5 | MSTest, the five rows of `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` |
| `Should()` (FluentAssertions) | 86 | FluentAssertions |
| `new Mock<` (Moq) | 13 | Moq |
| MSTest `Assert.` | **0** | no MSTest assertion API used |
| `CollectionAssert` / `StringAssert` | **0** | none |

41 `[TestMethod]` plus 1 `[DataTestMethod]` is 42 attribute-bearing methods, of which 2 are the
pre-existing `EfcFormControllerTests` methods this feature did not author; the 40 remaining are this
feature's, producing 44 results because the `[DataTestMethod]` carries five `[DataRow]` attributes. This
matches the `[P10-T6]` enumeration exactly.

The complete deduplicated `using` set across the four files contains
`Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq` and `FluentAssertions` and no other test framework
or assertion library. Every added test therefore uses **MSTest attributes, Moq mocks and
FluentAssertions assertions**, as `CUT1` and `CUT2` require.

Both files that import Outlook interop write `System.Action` and `System.Exception` fully qualified,
per the namespace trap the upstream-constraints briefing records; no bare `Action` or `Exception` binds
to an Outlook type.

## No test fixture derives from `System.Windows.Forms.Form`

The four class declarations, verbatim:

```
QuickFiler.Test/Controllers/EfcFormControllerTests.cs:17:    public class EfcFormControllerTests
QuickFiler.Test/Controllers/EfcItemControllerTests.cs:27:    public class EfcItemControllerTests
QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs:26:    public class EfcItemControllerCleanupTests
QuickFiler.Test/Controllers/EfcViewerTests.cs:28:    public class EfcViewerTests
```

A search of the four files for a class declaration carrying a base-type list (`class <Name> :`) returns
**zero** matches, so none of the four declares any base type at all, let alone `Form`.

`System.Windows.Forms` is imported by `EfcViewerTests.cs` only for the `Keys` enumeration, which the five
`ClaimsAltChord` tests pass as data. No control, form or handle is constructed.

## `NoLiveFormInTestAssemblyTests` still passes

`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` declares one test,
`ExecutingAssembly_ContainsNoFormDerivedType`. In the `[P10-T6]` TRX it appears exactly once with
outcome **`Passed`**. The assembly-wide assertion that no `Form`-derived type exists in `QuickFiler.Test`
therefore still holds after this feature added four test files to it.

## Other policy points, verified by inspection

- **No external dependency.** No added test contacts a live Outlook instance; every Outlook-typed
  collaborator is a Moq mock or a reflection-injected field. `MailItemHelper.UnRead` is never assigned,
  per the 484 convention, because its setter writes through to `Item.Save()`.
- **No temporary file.** The zero counts above cover the file-system APIs; no added test creates,
  reads or deletes any file.
- **Deterministic timers.** The disposal tests inject a
  `new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite)` that can never fire, and observe
  disposal as state (`ObjectDisposedException` from `timer.Change`) rather than as a race — the 484
  technique, reused verbatim.
- **Arrange–Act–Assert and naming.** Every added method follows `Member_Condition_Expectation`.

Output Summary: PASS. `Thread.Sleep` and `Task.Delay` each return **zero** matching lines across all four
test files this feature writes, as do `DateTime.Now`, `DateTime.UtcNow`, `new Random`, the temporary-file
APIs, `.Show()`, `Application.Run` and `BackgroundWorker`. Every added test uses MSTest attributes, Moq
mocks and FluentAssertions assertions, with **0** MSTest `Assert.` usages. None of the four fixture
classes declares any base type, so none derives from `System.Windows.Forms.Form`, and the pre-existing
assembly-wide assertion `ExecutingAssembly_ContainsNoFormDerivedType` passes in the `[P10-T6]` run.
