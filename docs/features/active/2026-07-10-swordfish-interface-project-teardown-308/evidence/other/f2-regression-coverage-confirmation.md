# AC-12 — F2 Clean-Base Regression Coverage Confirmation (P2-T5)

- **Timestamp:** 2026-07-11T13-12
- **Feature:** swordfish-interface-project-teardown (#308), F5

## What was inspected

The three WI-4 test files were examined against git HEAD (pre-removal) to determine their actual
subject-type binding:

| Removed file | Binds to | Direct-Swordfish? |
|---|---|---|
| `ObservableDictionary_Tests.cs` | `using Swordfish.NET.Collections;` → `ObservableDictionary<TKey,TValue>` | YES (vendored type deleted wholesale) |
| `ConcurrentObservableCollectionSenderTests.cs` | `using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;` → clean `ConcurrentObservableCollection<T>` | NO — clean first-party base (F2 deliverable) |
| `ConcurrentObservableCollectionLockRecursionTests.cs` | same clean first-party namespace | NO — clean first-party base (F2 deliverable) |

The two collection test files test the clean, Swordfish-free base, consistent with the F5 research
directive (research lines 170-172): "F5 should remove these three tests and flag the
sender-identity/lock-recursion regression coverage of the clean collection base as part of F2 (verify
at F5 execution; if absent, raise a new issue rather than authoring)."

## Finding

### Sender-identity — PRESENT (coverage confirmed)

The surviving
`UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection_Tests.cs`
carries the sender-identity regression at lines ~213-227:

```
public void CollectionChanged_RaisedOnAdd_WithWrapperSender()
...
capturedSender.Should().BeSameAs(sut);
```

This asserts the `CollectionChanged` sender is the wrapper instance (the SubjectMapSco pattern)
against the clean base. Equivalent sender-identity coverage confirmed present.

### Lock-recursion — ABSENT after WI-4 removal (issue raised)

After the WI-4 removal, no surviving test asserts the lock-recursion invariant (reading the
collection from inside a `CollectionChanged` handler during `Add` must not throw). A repo-wide search
of surviving `UtilitiesCS.Test/**/*.cs` found no equivalent (the only `LockRecursion` /
`ReaderWriterLockSlim` matches are in unrelated Theme/Folder/Threading tests, not the clean
`ConcurrentObservableCollection` base).

Per spec AC-12 and the research directive, F5 does NOT author this coverage. A new issue was raised:

- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/317
- **Title:** Re-express lock-recursion regression coverage against the clean ConcurrentObservableCollection base

## AC-16 note

The removal does not reduce line coverage of the clean `ConcurrentObservableCollection<T>` production
type: its production lines remain exercised by the surviving `ConcurrentObservableCollection_Tests.cs`
(Add/Remove/CollectionChanged/serialization). This is verified numerically at P5-T5 (coverage delta).
The raised issue concerns the behavioral regression assertion only, not production line coverage.

## Verdict

AC-12 delivered: sender-identity coverage present (file reference recorded); lock-recursion
behavioral coverage absent after removal, new issue #317 raised (F5 does not author it).
