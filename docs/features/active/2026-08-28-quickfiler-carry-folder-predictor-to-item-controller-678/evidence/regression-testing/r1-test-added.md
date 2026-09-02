# R1 — Regression test added

- Timestamp: 2026-09-02T01-14
- Issue: #678
- Task: [P1-T1]
- Test: `RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary`
- File: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`

## Clause 1 — the file exists and the `<Compile Include>` entry is present

`git add -N -- QuickFiler.Test` was run first, because an unstaged new file is invisible to a
name-listing diff.

Command: `git status --porcelain -- QuickFiler.Test`

```
 A QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
```

Both paths are reported: the new test part as added, the project file as modified. The entry
added to `QuickFiler.Test/QuickFiler.Test.csproj`, verbatim, placed immediately after the
existing Part2 entry:

```xml
    <Compile Include="Controllers\QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs" />
```

Both projects use explicit `<Compile Include>` item lists, so this entry is what makes the
new file part of the compilation.

The new part declares `public partial class QfcHomeControllerRunAsyncTests` in namespace
`QuickFiler.Controllers.Tests` and carries **no** `[TestClass]` attribute of its own. The
attribute on the base part at
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:23-24` covers the whole partial
class; a second attribute would be a duplicate-attribute error.

## Clause 2 — the analyzer build exits 0 against the current, unfixed production code

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0. Summary: `5 Warning(s)`, `0 Error(s)` — the same five pre-existing
System.Reactive `packages.config` notices recorded at P0-T6. `CoreCompile:` occurrences: 58.

This is what makes the P1-T2 red run a *runtime* failure rather than a build error: the test
compiles, so any failure it reports comes from executing production code, not from the
compiler. The test body uses only APIs that exist today; `QfcPreScoredItem.ResolveCarrier`
and `QfcPreScoredItem.ReconcileCarriersToItems` are added later by P1-T3 and are not
referenced by the test.

## Clause 3 — stage-one assertions require a genuine divergence

The batch the stage-two assertions are made against is produced by the real
`QfcDatamodel.DequeueNextItemGroupWithOutcomeAsync`, driven down the real
`TryUnhookOrReplace` throw branch, and is never hand-built. The stage-one assertions are:

- `batch.Items` holds exactly one element (`ContainSingle`);
- `batch.Items[0]` is reference-equal to the substitute item (`BeSameAs(substituteItem)`);
- `batch.PreScored` holds exactly one element (`ContainSingle`);
- `batch.PreScored[0].MailItem` is reference-equal to the failed item
  (`BeSameAs(failedItem)`).

Mechanism, re-derived against the current tree. The master queue holds two loose `MailItem`
mocks whose `EntryID` getters return the distinct values `entry-failed` and
`entry-substitute`. The gate accepts the first candidate because the strict
`IFolderScoringService` returns 950, which is at or above the cutoff of
`(long)Math.Round(0.90 * 1000, 0)` = 900, so the quantity-1 loop exits immediately as
`QuantitySatisfied` with `accepted = [carrier(failedItem)]`.
`QfcDatamodel.QueueProcessing.cs:192` then builds `nodes` as `[failedItem]`.
`UnhookDequeuedNodes` calls `TryUnhookOrReplace(ref nodes, 0)`; the strict
`IEmailMoveMonitor` throws on its first `UnhookItem` call, so `:54` removes `failedItem`,
`:55` pulls `substituteItem` from the master queue, and `:62` inserts it at index 0. The
second `UnhookItem` call succeeds and the loop ends. The returned batch therefore has
`Items = [substituteItem]` and `PreScored = [carrier(failedItem)]`.

The quantity argument of **1** is load-bearing and is not a free choice. With 2 the gate
accepts both queued items, `_masterQueue.TryTakeFirst()` at
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:55` returns null, no substitute is
inserted at `:62`, and `batch.PreScored` holds two entries rather than the one the stage-one
assertion requires.

## Clause 4 — stage-two assertions at the consuming boundary

The captured carrier list is the argument `QfcHomeController.RunAsync` passes to
`IQfcFormController.LoadItemsAsync`. `QfcFormController.Actions.cs:120-153` forwards it to
`QfcCollectionController.LoadControlsAndHandlers_01Async`, whose body at
`QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs:41` derives the displayed item
spine as `preScored.Select(x => x.MailItem)` and at `:70-84` builds one `QfcItemGroup` per
carrier. The captured list is therefore exactly the displayed set, which is the boundary R1
requires the invariant to be pinned at.

The four stage-two assertions are:

- the captured list contains exactly one element;
- that element's `MailItem` is reference-equal to the substitute;
- no element's `MailItem` is reference-equal to the failed item;
- that element's `FolderHandler` is null, because the substitute left the master queue after
  the scoring pass and no carrier was ever built for it.

## Clause 5 — policy conformance

- Framework: **MSTest** (`[TestMethod]`, `Microsoft.VisualStudio.TestTools.UnitTesting`).
- Mocking: **Moq** (`Mock<MailItem>`, `Mock<IEmailMoveMonitor>`, `Mock<IApplicationGlobals>`,
  `Mock<IAppQuickFilerSettings>`, `Mock<IFolderScoringService>`, `Mock<IQfcDatamodel>`,
  `Mock<IQfcFormController>`, `Mock<IQfcFormViewer>`, `Mock<IFolderSearchHandler>`).
- Assertions: **FluentAssertions** throughout; no MSTest `Assert` call.
- No temporary file is created. No filesystem, network or external process is touched.
- No live Outlook COM: every `MailItem` is a Moq proxy and the monitor, globals and settings
  are all mocks. The test carries no `LiveOutlook` category.
- Determinism: the datamodel's `TimeProvider` is a `FakeTimeProvider`, which is mandatory
  because `FormatterServices.GetUninitializedObject` runs no field initialiser and leaves the
  property null, and because `.claude/rules/general-unit-test.md` bans real wall-clock waits
  in test code. The clock is never advanced: the quantity-satisfied exit is reached on the
  first loop iteration and needs no simulated time to elapse.
- Structure: Arrange-Act-Assert, marked by section comments, twice (once per stage).

## Output Summary

New test part created with one `[TestMethod]`, and its `<Compile Include>` entry added to
`QuickFiler.Test/QuickFiler.Test.csproj`. `git status --porcelain -- QuickFiler.Test`
reports both paths after `git add -N`. The analyzer build exits 0 with 5 warnings and 0
errors and 58 `CoreCompile:` occurrences, so the test compiles against the current unfixed
production code. All five acceptance clauses hold.
