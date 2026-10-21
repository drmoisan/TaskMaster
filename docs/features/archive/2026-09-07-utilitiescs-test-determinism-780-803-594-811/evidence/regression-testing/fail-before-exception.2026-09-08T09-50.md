# Fail-before exception dossier — AC1 (issue #780)

Timestamp: 2026-09-08T09-50
Task: [P4-T3]
Acceptance criterion: AC1
EXIT_CODE: 0

## Why a failing run is impossible

WhyFailingRunImpossible: The #780 failure is load-dependent thread-pool scheduling latency above 500 ms, and the only ways to force it on demand are a sleep or a deliberately starved thread pool, both forbidden by AC5 and by the repository unit-test policy. A test built either way would also be nondeterministic in the opposite direction, passing or failing with the same host load the defect depends on.

The #780 failure is thread-pool scheduling latency exceeding 500 ms under a 24-worker coverage
run, so it is load-dependent rather than deterministic: `TryAddValuesAsync_UpdatesExistingValue`
passes in about 2 ms when run alone and fails intermittently only under full-suite parallel load.
The only ways to force the latency on demand are a sleep or a deliberately starved thread pool,
and AC5 forbids stabilising or reproducing behaviour with a sleep, a retry, or a timing tolerance,
as does the repository unit-test policy's ban on wall-clock waits in test code. A test that waited
for the pool to fall behind would also be nondeterministic in the opposite direction: it would
pass or fail depending on the same host load the defect depends on. No deterministic failing run
is therefore constructible, and this dossier stands in its place.

This is recorded as decision D6 in the plan and is a deviation from the spec Test Strategy's
"token cancelled after the work is observed to start" bullet, which is not authored: `TryAddValues`
offers no observation point without a custom operator type, and such a test would pass identically
before and after the fix, so it would carry no discriminating power.

## Alternative proof

### 1. Structural count gate on the deleted window

The defect is a hard-coded wall-clock cancellation window in production code. Its presence and its
removal are both directly measurable, and the measurement is exact rather than probabilistic.

| Token in `UtilitiesCS/Extensions/DictionaryExtensions.cs` | Before P4-T1 | After P4-T1 |
|---|---|---|
| `CancelAfter(` | 1 | 0 |
| `CreateLinkedTokenSource` | 1 | 0 |
| `TryAddValues(key, value), token)` | 0 | 1 |

The "before" figures are the ones P0-T6 measured against the base tree `bb1c7d4b` and recorded in
`evidence/baseline/p0-t6-citation-baseline.md`; both matched the plan's stated values exactly. The
"after" figures were measured by P4-T1 against the edited file. The third row is the positive
control: it shows the caller's own token, rather than a linked token carrying an internal deadline,
is now what `Task.Run` receives.

The deleted lines were:

```csharp
var linkedTS = CancellationTokenSource.CreateLinkedTokenSource(token);
linkedTS.CancelAfter(500);
```

With `CancelAfter(` at 0 occurrences, no fixed wall-clock window remains in the method, which is
AC1's first disjunct discharged by construction rather than by observation. A future reintroduction
would raise the count above 0 and is additionally caught by the P7-T10 AC5 diff search, which
counts `CancelAfter` over the added lines of the anchored diff.

Secondary defect also resolved: `linkedTS` was never disposed, so every call leaked a timer until
it fired. With the linked source gone, nothing is leaked.

### 2. Contract-lock test

Deleting a cancellation window could in principle weaken the surviving cancellation contract. It
does not, and P4-T2 adds a test that fails if it ever does:
`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged`
asserts that a caller token cancelled before the call still yields `TaskCanceledException` from
`Task.Run` and that the dictionary value is unchanged. Its passing run is recorded in
`p4-t4-ac1-pass-after.md`.

### 3. Zero production call sites

`TryAddValuesAsync` has no production caller. The only invocation anywhere in the repository is
`UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs:244`; the other two search hits are the
definition at `DictionaryExtensions.cs:169` and the test method name at line 237. This is decision
D3, and it is why deletion rather than a `TimeProvider` seam is the correct fix: the window guarded
nothing, could not interrupt a `Task.Run` body already running, and protected a bounded
compare-and-swap loop (`TryAddValues`, lines 123-136) that performs no I/O and cannot hang.

## Negative evidence record

SearchScope: `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/regression-testing/`
SearchPatterns: `fail-before-exception.*.md`
SearchResult: `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/regression-testing/fail-before-exception.2026-09-08T09-50.md` (this file)

## Output Summary

AC1 has no deterministic failing run, for a stated and verifiable reason. The alternative proof is
a three-part structural argument: an exact before-and-after count gate on the deleted window
(1 to 0 for both tokens, with a positive control at 0 to 1), a contract-lock test for the
cancellation semantics that survive, and the absence of any production call site.
