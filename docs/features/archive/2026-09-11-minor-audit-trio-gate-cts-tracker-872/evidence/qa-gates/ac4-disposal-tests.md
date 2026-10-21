# AC4 — The Three Disposal Tests

Timestamp: 2026-09-13T15-45
Task: [P2-T14]

Verdict: PASS

TrxCount: 1
OperativeTrx: p2-t5-final-utilitiescs.trx

## Outcomes Transcribed From The TRX

The TRX read is `TestResults/vstest/p2-t5/p2-t5-final-utilitiescs.trx`, produced by the P2-T5 run. The
results directory holds exactly one TRX file, so the most-recent-last-write selection rule is inert;
the count and the file name are recorded above so that a third party re-running the selection obtains
the same file.

| Test name | Matching results | Outcome |
|---|---|---|
| Dispose_WhenPackageConstructedTheSource_ReleasesIt | 1 | Passed |
| Dispose_WhenCallerSuppliedTheSource_LeavesItUsable | 1 | Passed |
| Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource | 1 | Passed |

Each name matched exactly one unit test result, so no outcome here is an aggregate over several
matches. A named test and its node identity are used rather than a phrase search, because a test name
is stable under reformatting and the P2-T1 formatter pass did rewrite this file.

## Source Reading Of The Three Tests

Read from `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`:

**Dispose_WhenPackageConstructedTheSource_ReleasesIt**, lines 128 to 157. Awaits `InitializeAsync` with
a null first argument and the named arguments `progressTracker: progressTracker` and
`stopWatch: stopWatch`, both non-null. Captures `package.CancelSource` and asserts it is not the same
instance as the locally created tracker source, which establishes that the package constructed its own.
Disposes the package, then probes release with `Action readToken = () => _ = constructed.Token;` and
asserts it throws `ObjectDisposedException`.

**Dispose_WhenCallerSuppliedTheSource_LeavesItUsable**, lines 163 to 188. Awaits `InitializeAsync` with
a caller-created `cancelSource` and the same two named non-null arguments. Disposes the package, then
asserts the token getter does not throw and that calling `Cancel` on the caller's source sets
`IsCancellationRequested` to true.

**Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource**, lines 195 to 218. Awaits `InitializeAsync`
with a null first argument and the same two named non-null arguments so the parent constructs and owns
a source, captures the parent's source, spawns a child with `parent.SpawnChild(25)`, disposes only the
child, then asserts the parent's token getter does not throw.

All three therefore pass a non-null `progressTracker` argument and an explicit `stopWatch` argument and
probe release through the token getter rather than through a timer. The named `progressTracker`
parameter is what disambiguates the tracker overload from the pane overload, which a null third
argument would leave ambiguous. The non-null tracker and the explicit stop watch are what keep the run
headless and deterministic: no dispatcher is touched and no background task is started. No test creates
a temporary file, sleeps, waits on a wall clock or touches an external process.

## The Four Pre-Existing Tests Are Unchanged

The anchored numstat for this file against the base commit
`430e2a11db0fa7069d02d42e18df46d21f7db7b5` is:

```
100	0	UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs
```

One hundred insertions and zero deletions. A change to any pre-existing line would have registered as a
deletion paired with an insertion, so the zero deletion count establishes that none of the four
pre-existing tests was modified. The file now carries seven test methods: the four pre-existing ones
and the three added by P1-T6 through P1-T8.

## Fail-Before Position

Per the P1-T17 exception dossier, a failing run of these three tests before the fix was structurally
impossible: they call a `Dispose` method that did not exist on the progress package before P1-T4, so
the test assembly would not compile and the tests could not be observed to fail at run time.
