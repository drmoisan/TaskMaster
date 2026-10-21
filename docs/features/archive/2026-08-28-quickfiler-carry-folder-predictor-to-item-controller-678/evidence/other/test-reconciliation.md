# P1-T10 — Pinned test-suite reconciliation (AC13, AC17, AC18)

Timestamp: 2026-09-01T23-30

Every test listed here was **rewritten**, never deleted and never weakened. Each carries a named
reason. The reason common to the enabled-mode sites is the same one throughout and is stated once
here rather than repeated: **P1-T5 switched high-confidence-enabled `RunAsync` from
`DequeueNextItemGroupAsync` to `DequeueNextItemGroupWithOutcomeAsync`, and from the
`IList<MailItem>` overload of `LoadItemsAsync` to the `IList<QfcPreScoredItem>` overload.** A test
that sets up or verifies the superseded member no longer describes the code under test.

## Which tests actually broke

Before reconciliation, a scoped run over `FullyQualifiedName~QfcHomeController` reported
`Total tests: 54`, `Passed: 49`, `Failed: 5`. The five were, in the run's own order:

1. `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`
2. `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch`
3. `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand`
4. `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter`
5. `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration`

Every one is an enabled-mode test. **Both disabled-mode tests passed unchanged in that same run**,
which is direct evidence for AC13 taken before any reconciliation edit was made.

## `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`

| Baseline site | Disposition | Reason |
|---|---|---|
| Shared `DequeueNextItemGroupAsync` setup at `:102` | **Rewritten (extended)** | A `DequeueNextItemGroupWithOutcomeAsync` setup was added alongside it, returning an empty `QfcDequeueBatch`. The original setup was **kept** so the disabled-mode path in this class stays configured. |
| `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` declared at `:138` — `LoadItemsAsync(IList<MailItem>)` `Times.Once` verification at `:160-164` | **Rewritten** | Now a `Times.Once` verification on `LoadItemsAsync(IList<QfcPreScoredItem>)`. Enabled mode selects the carrier overload, so this is where the once-per-run constraint now belongs. |
| Same test — `DequeueNextItemGroupAsync` `Times.Once` verification at `:165-176` | **Rewritten** | Retargeted to `DequeueNextItemGroupWithOutcomeAsync`, keeping `Times.Once` and all four `It.IsAny` argument matchers. |
| Same test — carrier `Times.Never` verification at `:177-181` | **Rewritten (inverted)** | Now a `Times.Never` on `LoadItemsAsync(IList<MailItem>)`. The pair of verifications still pins exactly one overload as used and the other as unused; only which is which has changed, which is the landed decision the change makes. |
| `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` declared at `:185` — `DequeueNextItemGroupAsync` setup at `:206` | **Rewritten** | Replaced by the outcome-returning setup. This test builds its own data model rather than using the shared helper, so the disabled-mode consideration does not apply. |
| Same test — `LoadItemsAsync(IList<MailItem>)` setup and sequence callback at `:221-223` | **Rewritten** | Retargeted to the carrier overload. The `Callback(() => sequence.Add("LoadItemsAsync"))` is unchanged, so the sequence assertion still observes the same event under the same name. |
| Same test — `sequence.Should().Equal("LoadItemsAsync")` at `:244` | **Unchanged** | Byte-identical. The callback was moved to the other overload, so the assertion still holds and still fails if the load is skipped. |
| Same test — `DequeueNextItemGroupAsync` `Times.Once` verification at `:245-254` | **Rewritten** | Retargeted to `DequeueNextItemGroupWithOutcomeAsync`, `Times.Once` and all matchers unchanged. |
| Same test — carrier `Times.Never` verification at `:255-258` | **Rewritten (inverted)** | Now `Times.Never` on `LoadItemsAsync(IList<MailItem>)`, with a stated reason. |
| `preFilterInvoked` assertion at `:157` | **UNCHANGED, byte-identical** | AC13. See the identity proof below. |

## `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`

| Baseline site | Disposition | Reason |
|---|---|---|
| Shared `ArrangeRunAsyncController` dequeue setups at `:44-56` | **Rewritten (extended)** | A `DequeueNextItemGroupWithOutcomeAsync` setup was added, exactly as the plan requires. Both plain overloads were **kept**, because the two disabled-mode tests in this class use this helper and must continue to exercise their own path. |
| `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` — its own dequeue setup at `:137-146` | **Rewritten** | Retargeted to the outcome-returning member, returning a `QfcDequeueBatch` whose `PreScored` holds a carrier for the streamed candidate. **This site is not in the plan's P1-T10 enumeration** and is recorded as a plan defect below. |
| Same test — its `LoadItemsAsync(IList<MailItem>)` setup at `:152-160` | **Rewritten** | Retargeted to the carrier overload with an equivalent `It.Is` constraint. |
| Same test — enabled-mode dequeue and load verifications at `:180-201` | **Rewritten** | The dequeue verification retargeted to the outcome member, keeping all four exact argument constraints (`itemsPerIteration`, `200`, `DefaultFirstBatchDeadline`, non-null sink) so the issue #424 deadline bound and progress sink stay pinned. The load verification retargeted to the carrier overload and **strengthened**: it now additionally requires `ReferenceEquals(carriers[0].FolderHandler, streamedHandler)`, so the carried handler must survive the hop. |
| Same test — `Times.Never` on the unfiltered initialization batch at `:202-209` | **Rewritten onto the carrier overload** | See the dedicated section below. |
| `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` declared at `:289` — dequeue setup at `:318` | **Rewritten** | Retargeted to the outcome member. The scripted five-signal scan and all four argument constraints are unchanged, so the 0-to-30 band assertion still measures what issue #424 wrote it to measure. |
| Same test — `IList<MailItem>` load setup at `:347` | **Rewritten** | Retargeted to the carrier overload. |
| `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration` declared at `:396` — dequeue setup at `:420` | **Rewritten** | Retargeted to the outcome member, returning an empty batch with `QfcDequeueStop.DeadlineExpired`, which is the stop reason this test's scenario describes and which the plain overload could not express. |
| Same test — load setup at `:446` | **Rewritten** | Retargeted to the carrier overload. |
| Same test — `LoadItemsAsync(It.Is<IList<MailItem>>(items => items.Count == 0))` `Times.Once` at `:462-463` | **Rewritten** | Now `It.Is<IList<QfcPreScoredItem>>(carriers => carriers.Count == 0)` with `Times.Once`. The empty-not-null constraint is the point of the test and is preserved exactly: an empty carrier list must still reach the form path. |
| `preFilterInvoked` assertion at `:239` | **UNCHANGED, byte-identical** | AC13. |
| `Times.Never` verification at `:246` inside `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload` | **UNCHANGED, byte-identical** | AC13. |
| `Times.Never` verification at `:277` inside `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly` | **UNCHANGED, byte-identical** | AC13. |

## Acceptance conditions

### 1. The two disabled-mode `Times.Never` verifications are byte-identical to their base-ref text

Verified by direct byte comparison against `git show <base>:<path>`, not by inspection. The
comparison was made over the **whole enclosing test method**, which is stronger than the line the
plan names:

```
RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload: identical = True
RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly: identical = True
```

Base-ref line `:246` and base-ref line `:277` both carry the text

```
                m => m.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()),
```

and that exact text is present in the current file at lines 294 and 325, inside those two methods.

### 2. The two `preFilterInvoked` assertions are byte-identical, recorded by file, line and quoted text

| File | Base-ref line | Current line | Quoted text |
|---|---:|---:|---|
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs | 239 | 287 | `            preFilterInvoked.Should().BeFalse("disabled mode must not run the pre-filter");` |
| QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs | 157 | 176 | `            preFilterInvoked` followed by `                .Should()` and `                .BeFalse("remaining-queue admission now owns high-confidence filtering");` |

Both were compared byte-for-byte against the base ref, the second across its full three-line
assertion. `HighConfidencePreFilterLoader` therefore remains uninvoked and
`QfcHighConfidencePreFilter.FilterAsync` remains dormant, as AC13 requires. No production call site
of `HighConfidencePreFilterLoader` was added by this change.

### 3. The `Times.Never` on the unfiltered initialization batch is rewritten onto the carrier overload

Base-ref form at `:202-209`:

```csharp
mockFormController.Verify(
    m => m.LoadItemsAsync(It.Is<IList<MailItem>>(items => items == unfilteredInitialBatch)),
    Times.Never,
    "RunAsync must not load the unfiltered initialization batch"
);
```

Post-change form:

```csharp
mockFormController.Verify(
    m =>
        m.LoadItemsAsync(
            It.Is<IList<QfcPreScoredItem>>(carriers =>
                carriers.Count == unfilteredInitialBatch.Count
                && carriers.Count > 0
                && ReferenceEquals(carriers[0].MailItem, unfilteredInitialBatch[0])
            )
        ),
    Times.Never,
    "RunAsync must not load a carrier list projected from the unfiltered initialization batch"
);
```

**Leaving the original `IList<MailItem>` form in place would have satisfied it trivially after the
change**, because that overload is no longer invoked at all in enabled mode: any `Times.Never`
assertion on it would hold whatever the production code did with the unfiltered batch, so it would
have stopped being a gate the moment P1-T5 landed. The rewritten form asserts over the overload that
IS invoked, so it can still fail. This is recorded because the trivially-satisfied form is the
likelier and quieter mistake.

### 4. No `[TestMethod]` is deleted anywhere in `QuickFiler.Test`

| Measurement | Count |
|---|---:|
| `[TestMethod]` occurrences at base ref `807fb0bb…`, over every `.cs` file under `QuickFiler.Test` | **1276** |
| `[TestMethod]` occurrences post-change, same scope | **1284** |
| Difference | **+8** |

Both numbers are reported, as the plan requires. The +8 accounts exactly for the tests this plan
adds and for nothing else:

- P1-T3: `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` (1)
- P1-T6: `IterateQueueAsync_WhenBatchCarriesPreScoredItems_ForwardsCarriersToEnqueue`,
  `ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler`,
  `ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull`,
  `ItemControllerFactory_OnAFreshQueue_HasANonNullProductionDefault` (4)
- P1-T8: `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory` (1)
- P1-T9: `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder`,
  `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` (2)

1276 + 8 = 1284. Since the total rose by exactly the number added, **nothing was deleted**. The base
count was taken from `git show` rather than from a working-tree scan, so it is the true base-ref
figure and not a re-measurement of an already-edited tree.

### 5. Every rewritten test still uses MSTest, Moq and FluentAssertions, creates no temporary file, and requires no live Outlook COM (AC18)

Every edit in this task changed a Moq `Setup`, `Returns`, `ReturnsAsync` or `Verify` expression, or
an `It.Is` matcher, inside a method that already carried `[TestMethod]` from
`Microsoft.VisualStudio.TestTools.UnitTesting` and already used FluentAssertions for its
non-Moq assertions. No test framework, mocking library or assertion library was introduced or
changed. No file API is called by any rewritten test. No `MailItem`, `Application`, `Store` or
`MAPIFolder` is constructed other than through `new Mock<...>()`, and every run in this task carried
`/TestCaseFilter:TestCategory!=LiveOutlook`.

### 6. This artifact records one named reason for every changed test

The two tables above; the shared reason is stated once at the head of the document and the
test-specific reason in each row.

## Verification run

After reconciliation, a scoped Derivation D7 run over `FullyQualifiedName~QfcHomeController`
(`/ResultsDirectory:TestResults\p1-t10-final`) reported:

```
Test Run Successful.
Total tests: 54
     Passed: 54
```

Up from `Passed: 49, Failed: 5` before the reconciliation, with the same 54 discovered, so the five
that failed now pass and none of the 49 regressed.

## Collateral edit forced by file size

The reconciliation took `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`
from its baseline 473 lines to **544**, past the 500-line limit, because every rewritten setup gained
a `QfcDequeueBatch` construction spanning several lines under CSharpier. Two whole tests were
therefore relocated into a new part
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs`:
`RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` and
`RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration`, each with its documentation
comment, bodies otherwise unchanged.

No `partial` keyword had to be added: this file was already a further part of
`QfcHomeControllerRunAsyncTests`, whose `[TestClass]` attribute lives on the base file, so the new
part carries none either. `<Compile Include="Controllers\QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs" />`
was added to `QuickFiler.Test/QuickFiler.Test.csproj`.

| File | Baseline | Peak | After split |
|---|---:|---:|---:|
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs | 473 | 544 | **333** |
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs | new | — | **241** |
| QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs | 261 | — | **290** |

## Plan defect found while executing this task

The plan's P1-T10 enumeration is authoritative and closes with "a site not listed here is not
rewritten for the reason this task governs". **One site that this task must rewrite is missing from
it**: the dequeue setup at
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:137-146`, inside
`RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`.

The plan lists that test's *verifications* at `:180-201` and `:202-209` but not its *setup*. The test
does not use the shared `ArrangeRunAsyncController` helper whose setups the plan does list at
`:44-56`; it builds its own data model inline. Leaving the setup on `DequeueNextItemGroupAsync` while
retargeting the verification to `DequeueNextItemGroupWithOutcomeAsync` would have left the test
failing, so the enumeration as written is not executable in full. The site was rewritten and is
recorded in the table above. The plan was not edited.
