# Fail-Before Exception Dossier — Root Cause RC-6 (non-atomic re-entrancy guard)

Timestamp: 2026-08-26T11-10
Task: [P3-T4]
Command: not applicable; this artifact records why a failing-first run is structurally impossible
EXIT_CODE: 0

## WhyFailingRunImpossible

Root cause RC-6 is a non-atomic read-then-write at
`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:48-57`. The pre-change
`TryBeginExecuteMoves` reads `_isExecuting`, and only afterwards writes `true`:

```csharp
internal bool TryBeginExecuteMoves()
{
    if (_isExecuting)
    {
        return false;
    }

    _isExecuting = true;
    return true;
}
```

`volatile` gives the field acquire/release ordering, but it does not make the read and the write a
single atomic operation. The defect is therefore only observable when two threads interleave
between the read at line 50 and the write at line 55, so that both observe `false` and both return
`true`. On a single thread the method is already correct, and no sequential call sequence can
distinguish the pre-change form from the post-change `Interlocked.CompareExchange` form.

Reproducing the interleaving requires a genuine data race. `.claude/rules/general-unit-test.md`
forbids exactly that: it requires determinism ("given the same inputs and environment, tests must
produce the same results; avoid flakiness") and bans `Thread.Sleep`, `Task.Delay`, and real
wall-clock waits, which are the only mechanisms available for widening the race window in this
codebase. A test that spawns competing threads and asserts a double-entry would pass or fail by
scheduler chance; it would be a flaky test asserting a probabilistic outcome, not a regression
test. The plan records this in [P3-T1] as an explicit instruction that "a genuinely concurrent
assertion on a compare-and-swap is not deterministic and must not be attempted".

A failing-first run for RC-6 is therefore not merely inconvenient to produce; it is precluded by
repository test policy. The alternative proof below is supplied in its place.

## SearchScope, SearchPatterns, SearchResult

- SearchScope: `docs/features/active/quickfiler-home-controller-metrics-442/evidence/regression-testing/`
  and `docs/features/active/quickfiler-home-controller-metrics-442/evidence/`
- SearchPatterns: `*-red.*.md`, `fail-before-exception.*.md`
- SearchResult: `efc-metrics-red.2026-08-26T11-06.md` (covers RC-3, RC-5, RC-7, RC-8, RC-9);
  no failing run exists for RC-6, and this file is its exception dossier.

## Alternative proof

### 1. Source form before the change

`QuickFiler/Controllers/EfcHomeController.cs:389`

```csharp
private volatile bool _isExecuting;
```

`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:48-62`

```csharp
internal bool TryBeginExecuteMoves()
{
    if (_isExecuting)
    {
        return false;
    }

    _isExecuting = true;
    return true;
}

internal void ResetExecuteMovesState()
{
    _isExecuting = false;
}
```

The read and the write are two separate operations on the field. Two threads can both execute the
read before either executes the write.

### 2. Source form after the change ([P3-T5])

`_isExecuting` becomes `private int`, `TryBeginExecuteMoves` returns the result of a single
`Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0`, and `ResetExecuteMovesState` becomes
`Interlocked.Exchange(ref _isExecuting, 0)`. `Interlocked.CompareExchange` performs the compare and
the assignment as one indivisible hardware operation, so exactly one of any number of competing
callers can observe the comparand and take the guard. The defect is eliminated by construction, not
by timing.

This is a proof by change of primitive: the class of interleaving that produced the defect has no
representation in the post-change instruction sequence.

### 3. Behaviour-preservation evidence

The two pinning tests written by [P3-T1] and [P3-T2] assert the full observable single-threaded
contract:

- `TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse`
- `TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue`

They were run against the pre-change `volatile` primitive and passed, recorded in
`evidence/regression-testing/efc-reentrancy-pin.2026-08-26T11-10.md` with EXIT_CODE 0. They are run
again against the post-change `Interlocked` primitive by [P3-T6] and recorded in
`evidence/regression-testing/efc-reentrancy-green.<timestamp>.md`. Identical results on both sides
establish that the primitive swap changed no observable sequential behaviour, which is the property
a fail-before/pass-after pair would otherwise be asked to demonstrate.

### 4. Search-gate evidence

AC-14 additionally requires that `git grep -n "volatile" QuickFiler/Controllers/EfcHomeController.cs`
return no match after the change. The pre-fix census
(`evidence/baseline/defect-site-census.2026-08-26T10-42.md`) records one hit at line 389; the
post-fix reading is recorded by [P3-T5]'s verification. The transition from one hit to zero is
itself falsifiable and is not satisfiable by any change other than removing the `volatile`
declaration.
