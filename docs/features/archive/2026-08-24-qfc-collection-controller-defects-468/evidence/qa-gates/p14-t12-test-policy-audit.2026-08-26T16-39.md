# [P14-T12] Test-policy audit (AC-23)

Timestamp: 2026-08-26T16-39

Command:

```
FILES="QfcCollectionController.TestSupport.cs \
       QfcCollectionControllerDefects468Tests.cs \
       QfcCollectionControllerDefects468MoveTests.cs \
       QfcCollectionControllerDefects468ConversationTests.cs \
       QfcCollectionControllerLayout.StaTests.cs \
       QfcCollectionControllerTests.cs \
       QfcCollectionControllerDarkModeTests.cs"

# raw search, every line
for lit in 'Thread.Sleep' 'Task.Delay' 'UiThread.Init' 'ShowDialog'; do grep -F -c "$lit" $FILES; done

# executable-code search: comment lines removed first
for lit in 'Thread.Sleep' 'Task.Delay' 'UiThread.Init' 'ShowDialog'; do
    cat $FILES | grep -v -E '^\s*(///|//|\*|/\*)' | grep -F -c "$lit"
done

# temporary-file APIs
cat $FILES | grep -v -E '^\s*(///|//|\*|/\*)' \
  | grep -c -E 'GetTempPath|GetTempFileName|File\.Create|File\.WriteAllText|Directory\.CreateDirectory'
```

All commands run from `QuickFiler.Test/Controllers/`.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**All four banned APIs have zero executable occurrences** across the five new test files and the two
changed existing test files. No temporary-file API occurs. Every new test uses MSTest, Moq, and
FluentAssertions and requires no live Outlook. The STA class is marked `[STATestClass]` and disposes
its panel in a `[TestCleanup]` that runs after every test.

**One discrepancy is recorded rather than papered over.** The raw, every-line search returns four
hits, not zero. All four are inside XML doc comments that state the API is *not* used. The task's
acceptance clause says "all four searches return zero hits across those files"; on the raw reading
that clause is **not** satisfied, and on the substantive reading — no executable use of a banned API
— it **is**. Both measurements are given below so a reviewer can apply either reading.

---

## 1. Banned-API searches

### Raw search, every line

| Literal | TestSupport | Defects468 | Defects468Move | Defects468Conversation | Layout.StaTests | ControllerTests | DarkModeTests | total |
|---|---|---|---|---|---|---|---|---|
| `Thread.Sleep` | 0 | **1** | 0 | 0 | 0 | 0 | 0 | **1** |
| `Task.Delay` | 0 | **1** | 0 | 0 | 0 | 0 | 0 | **1** |
| `UiThread.Init` | 0 | **1** | 0 | 0 | 0 | 0 | 0 | **1** |
| `ShowDialog` | 0 | 0 | 0 | 0 | **1** | 0 | 0 | **1** |

### Executable-code search, comment lines removed

| Literal | total |
|---|---|
| `Thread.Sleep` | **0** |
| `Task.Delay` | **0** |
| `UiThread.Init` | **0** |
| `ShowDialog` | **0** |

### The four raw hits, in context

| Literal | Location | The line |
|---|---|---|
| `UiThread.Init` | `QfcCollectionControllerDefects468Tests.cs:106` | `/// <c>(QfcFormController)_parent</c> downcast, sits behind <c>UiThread.Init()</c>, which` |
| `Thread.Sleep` | `QfcCollectionControllerDefects468Tests.cs:345` | `/// The test is fully deterministic and uses no wall-clock wait, no <c>Thread.Sleep</c> and` |
| `Task.Delay` | `QfcCollectionControllerDefects468Tests.cs:346` | `/// no <c>Task.Delay</c>. Two <see cref="TaskCompletionSource{TResult}"/> instances stand in` |
| `ShowDialog` | `QfcCollectionControllerLayout.StaTests.cs:31` | `/// <c>Show()</c> or <c>ShowDialog()</c>, never parents the panel to a form, never creates a` |

Every one of the four is an XML doc comment line, marked `///` at the start, and every one is a
statement that the named API is **not** used. Two of them are required by other decisions in this
plan: the `ShowDialog` line is part of the in-file comment D9 mandates for the STA class, and the
`Thread.Sleep`/`Task.Delay` line documents the determinism of the `#473` defect 1 test.

### Why the comment occurrences were not removed

Deleting these four comment lines would turn the raw search to zero, but it would delete the
documentation the plan itself requires elsewhere, and it would do so purely to satisfy a text search.
That is a gate-driven edit, not a quality improvement, and it would make the test files worse.

The plan already establishes the principle that governs this case, in its
`### Literals asserted by acceptance conditions` section: every literal search in this plan is scoped
to a named file or directory "because these identifiers legitimately appear in `docs/features/**`
prose (including this plan) and a repository-wide zero-hit gate would be unsatisfiable by
construction." The same distinction — prose mention versus executable use — applies inside an XML doc
comment. The executable-code search is the measurement that carries the policy meaning; the raw
search is reported alongside it so the discrepancy is visible rather than hidden.

## 2. Framework compliance

| File | MSTest | Moq | FluentAssertions |
|---|---|---|---|
| `QfcCollectionController.TestSupport.cs` | n/a — no test method | n/a | yes |
| `QfcCollectionControllerDefects468Tests.cs` | yes | yes | yes |
| `QfcCollectionControllerDefects468MoveTests.cs` | yes | yes | yes |
| `QfcCollectionControllerDefects468ConversationTests.cs` | yes | yes | yes |
| `QfcCollectionControllerLayout.StaTests.cs` | yes | n/a — no mock needed | yes |
| `QfcCollectionControllerTests.cs` (existing) | yes | yes | yes |
| `QfcCollectionControllerDarkModeTests.cs` (existing) | yes | yes | yes |

`QfcCollectionController.TestSupport.cs` contains zero `[TestMethod]` attributes; it is the shared
asserting-reflection helper file required by D14, and it uses FluentAssertions for its own
non-null assertions. It declares no test, so it needs no MSTest attribute surface.

`QfcCollectionControllerLayout.StaTests.cs` uses no mock: it exercises a real in-memory
`TableLayoutPanel`, which is the whole point of the STA test. Moq is not required where no
collaborator is faked.

No xUnit and no NUnit reference appears in any of the seven files.

## 3. Temporary files

The executable-code search for `GetTempPath`, `GetTempFileName`, `File.Create`, `File.WriteAllText`,
and `Directory.CreateDirectory` across all seven files returns **0**. No test creates a temporary
file, which the General Unit Test Policy prohibits outright with no currently approved exceptions.

## 4. No live Outlook

Every Outlook type used in these tests is reached through `Moq`:
`Mock<Outlook.MailItem>`, `Mock<IQfcItemController>`, `Mock<ConversationResolver>` collaborators, and
the like. No test constructs an `Outlook.Application`, opens a `Store`, or resolves a `MAPIFolder`.
The controller under test is built by `FormatterServices.GetUninitializedObject` through
`QfcCollectionControllerTestSupport.CreateUninitializedController()`, which bypasses the WinForms and
COM-bound constructor entirely, so no COM apartment or Outlook process is required.

The full suite runs in roughly 10 seconds on a machine with no Outlook session open
(`evidence/qa-gates/p13-t7-suite.2026-08-26T16-20.md`), which is the practical confirmation.

## 5. The STA class

Required confirmations, both verified in
`QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs`:

- **Marked appropriately.** The class carries `[STATestClass]` at line 36 and is named
  `QfcCollectionControllerLayoutStaTests`. It is the single STA class D9 authorises.
- **Disposes its panel per test.** Lines 68-73:

```
        [TestCleanup]
        public void DisposePanel()
        {
            _panel?.Dispose();
            _panel = null;
        }
```

`[TestCleanup]` runs after **every** test method in the class, not once per class, so no panel
survives a test. The matching `[TestInitialize] CreatePanel()` builds a fresh
`TableLayoutPanel { ColumnCount = 1, RowCount = 1 }` per test, so no state crosses between tests.

The class additionally never calls `Show()` or `ShowDialog()`, never parents the panel to a form,
never creates a window handle, and relies on no message pump. Its in-file comment states why no seam
can replace it: `ShrinkByRows` is deliberately sign-agnostic, so both the correct and the defective
call are valid uses of the helper, and only executing the real `EliminateSpaceForItems` against a
real `TableLayoutPanel` observes the sign at the call site.

## Acceptance verification

| Clause | Status |
|---|---|
| the artifact exists | met |
| all four searches return zero hits across those files | **met on the executable-code search (0, 0, 0, 0); NOT met on the raw every-line search (1, 1, 1, 1), all four being `///` doc-comment statements that the API is not used** |
| the artifact confirms the STA class is marked appropriately | met — `[STATestClass]` at line 36 |
| the artifact confirms the STA class disposes its panel per test | met — `[TestCleanup] DisposePanel()` at lines 68-73 |
