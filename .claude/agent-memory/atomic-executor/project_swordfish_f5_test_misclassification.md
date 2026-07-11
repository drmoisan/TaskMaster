---
name: swordfish-f5-test-misclassification
description: F5 (#308) plan/spec labeled two F2 clean-base tests as "direct-Swordfish tests"; they actually bind to the clean first-party type. Verify using/namespace before treating a test removal as Swordfish-only.
metadata:
  type: project
---

In the swordfish-removal epic, once F1/F2 re-based types onto clean, same-named first-party
replacements (e.g. `ConcurrentObservableCollection<T>` now lives in
`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection`, not `Swordfish.NET.Collections`),
NAME-based classification of "is this a Swordfish test" became unreliable.

**Why:** F5's spec/research WI-4 listed three "direct-Swordfish test files" to delete. Only
`ObservableDictionary_Tests.cs` genuinely bound to `Swordfish.NET.Collections` (`using Swordfish`).
The other two — `ConcurrentObservableCollectionSenderTests.cs` and
`ConcurrentObservableCollectionLockRecursionTests.cs` — bound to the CLEAN first-party type
(`using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;`) and were F2's own
clean-base regression coverage. Their doc comments literally said "for the clean, Swordfish-free ...".

**How to apply:** Before treating a test-file deletion as coverage-neutral in this epic, `git show`
the file's `using`/`namespace`/instantiation. If it binds to the clean first-party type, deleting it
IS a first-party coverage change. F5's AC-12 handled this correctly by design: sender-identity
coverage was duplicated in the surviving `ConcurrentObservableCollection_Tests.cs`, but the
lock-recursion behavioral assertion was unique — F5 removed it per plan and raised issue #317 for the
collection-lineage owner to re-express (F5 does not author clean-base coverage). Per-package coverage
confirmed no first-party PRODUCTION regression (the production lines stay covered by other tests).
