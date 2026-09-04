# P4-T6 — QuickFiler.Test (final QA loop step 4, second assembly) — GATE FAILED

Timestamp: 2026-09-03T08-42

Result: **FAILED**. This artifact records a gate that did not pass. Per the plan's fail-closed
evidence rule the outcome is BLOCKED, not PASS, and P4-T6 is left unchecked in the plan.

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p4-t6 /TestCaseFilter:TestCategory!=LiveOutlook
```

EXIT_CODE: 1

## Output Summary

Console summary block, verbatim:

```text
Test Run Failed.
Total tests: 1312
     Passed: 1304
     Failed: 8
 Total time: 12.8534 Seconds
```

- **Total tests: 1312** (console summary block) — equal to the P0-T11 baseline of 1312, as expected,
  since this plan adds no test to this assembly.
- **Passed: 1304** (console summary block)
- **Failed: 8** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
  written under `TestResults/p4-t6/`)
- **Skipped: 0** (derived as `total` minus `executed`)

TRX `<Counters .../>` values: `total="1312" executed="1312" passed="1304" failed="8"`.
Derived `Skipped` = 1312 - 1312 = **0**. The `notExecuted` attribute was NOT used.

The TRX is identified by its repository-relative results directory `TestResults/p4-t6/` only; its
own name is not recorded and the run's `Results File:` console line is not quoted.

## Failing set — 8 tests, all in one class

All eight failures are in `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`
(`QuickFiler.Test` class `EmailMoveMonitorTests`), and each reports the same message,
`One or more errors occurred.`:

```text
  Failed HookItem_FirstItemOfFolder_SubscribesBeforeItemMoveOnce_AndSharedFolderDoesNotResubscribe [9 ms]
  Failed UnhookItem_RemovesLastItemForFolder_UnsubscribesBeforeItemMoveOnlyOnLastItem [< 1 ms]
  Failed UnhookItem_Null_IsNoOp_NoComAccessNoMarshalInvocation [< 1 ms]
  Failed UnhookItem_UsesCachedEntryIds_RemovesExactlyTheMatchingEntry [< 1 ms]
  Failed AllComAccess_FlowsThroughInjectedMarshalDelegate [< 1 ms]
  Failed UnhookAll_UnsubscribesEveryFolder_AndClearsState [< 1 ms]
  Failed DuplicateHookOfSameItem_AndUnhookNeverHookedItem_DoNotThrowOrSpuriouslyUnsubscribe [< 1 ms]
  Failed UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread [< 1 ms]
```

That is every `[TestMethod]` in the class.

## Acceptance evaluation

- The failing-test set is **not** empty and is **not** a subset of the `BASELINE_FAILURE_SET`
  recorded in P0-T11, which was empty (1312 of 1312 passed at BASE). All eight are **new members**.
  **Clause FAILED.**
- The `total` and `executed` values from which `Skipped` was derived are recorded. Clause satisfied.
- `Total tests` equals the baseline `Total tests` from P0-T11 (1312 = 1312). Clause satisfied.

## Root cause, established by a controlled counterfactual rather than inferred

`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` reads `UiThread.Dispatcher` **reflectively,
through the property**, in both its `[TestInitialize]` and its `[TestCleanup]`:

```csharp
        private static readonly System.Reflection.PropertyInfo DispatcherProperty =
            typeof(UiThread).GetProperty(
                "Dispatcher",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            );
        ...
        [TestInitialize]
        public void Setup()
        {
            _capturedDispatcher = DispatcherProperty?.GetValue(null);
            _marshalInvocationCount = 0;
        }

        [TestCleanup]
        public void Cleanup()
        {
            object current = DispatcherProperty?.GetValue(null);
            current.Should().BeSameAs(_capturedDispatcher);
        }
```

The class's own comment states the design intent: "These tests never invoke the default (production)
marshal delegate, so they do not depend on UiThread being initialized." That premise held only while
the accessor returned `null` silently. With P2-T1's guard in place, `PropertyInfo.GetValue(null)`
invokes a getter that throws `InvalidOperationException`, which reflection wraps and MSTest surfaces
as an initialize/cleanup failure for every test in the class — hence 8 of 8, with sub-millisecond
durations consistent with bodies that never ran.

### Counterfactual actually executed (not inferred)

| Working-tree state of `UtilitiesCS/Threading/UiThread.cs` | Isolated run, `/TestCaseFilter:"FullyQualifiedName~EmailMoveMonitorTests"` | Result |
|---|---|---|
| Fixed (P2-T1 applied) | `Total tests: 8` | **Failed: 8** |
| BASE `87cb4df338322844abfa580abea14df77e738e5c` restored to the working tree, solution rebuilt | `Total tests: 8` | **Passed: 8** |

Both runs used the same filter, the same assembly path, and the same worktree, and differed only in
the content of `UtilitiesCS/Threading/UiThread.cs`. The BASE file was restored to the working tree
only (the index was untouched), the counterfactual was run, and the fixed file was then restored and
the solution rebuilt; `git status --porcelain -- UtilitiesCS UtilitiesCS.Test` afterwards shows the
five owned paths at `M ` (working tree matching index), confirming the probe left no residue.

The regression is therefore deterministic and attributable to P2-T1, not to load-related flakiness.

## Why the plan's blast-radius analysis did not find this consumer

`spec.md`'s Risks & Mitigations asserts that `git grep -n "UiThread.Dispatcher\b"` enumerated the
complete set of reads of the property. That search cannot match this call site, because the read is
not spelled `UiThread.Dispatcher` anywhere in the file: it is spelled
`typeof(UiThread).GetProperty("Dispatcher", ...)` and later `DispatcherProperty?.GetValue(null)`. A
reflective read over the property name is invisible to a source grep for the qualified member
expression.

The plan's P0-T13 census covers the complementary case — reflective reads of the private **field**,
found with `git grep -n -F '"_dispatcher"'` — and correctly returned three files, all in
`UtilitiesCS.Test`. No equivalent census was run for reflective reads of the **property** name
`"Dispatcher"`, and that is the gap this failure occupies.

## Remedy required, and why this executor did not apply it

The remedy lies in `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, which is **outside this
plan's declared Scope**. The plan's "Scope: files this plan's diff writes" section names exactly five
source files, and the delegation instruction states that no file outside that list may be touched.
Adding this file to the write set and changing its snapshot/restore helpers to tolerate a throwing
getter is a new independent outcome that no task in this plan describes.

Execution is therefore halted here and reported as BLOCKED, rather than resolved by editing an
out-of-scope file or by weakening this gate's acceptance.
