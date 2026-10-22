# P3-T3 — Findings on issue #782

Timestamp: 2026-09-13T23-29

The status of issue #782 in this tree is **two separate findings**, not one. They have different
subjects, different evidence, and different dispositions. This delivery acts on one of them.

---

## Finding 4A — the retry-after-failed-initialize behaviour is already present in production

**Supporting line range:** lines 47-59 of `UtilitiesCS/Threading/UiThread.cs` (pre-change
numbering; the change this delivery makes lies at lines 175-179 of the same pre-change file and
shifts nothing above it).

```csharp
// The flag is set after Initialize() returns, not before it runs, so a failed first
// attempt leaves it false and a later call from an STA thread retries. The lock
// additionally serializes concurrent first attempts, which the previous
// Interlocked.Exchange latch never did.
lock (InitLock)
{
    if (_initialized)
    {
        return;
    }
    Initialize();
    _initialized = true;
}
```

The initialization flag `_initialized` is assigned **after** `Initialize()` returns, inside
`lock (InitLock)`. Three consequences follow directly from that ordering:

1. A first attempt that throws inside `Initialize()` propagates out of the `lock` without ever
   reaching the assignment, so the flag remains `false`.
2. A later call from an STA thread therefore re-enters and retries, rather than short-circuiting on
   a latch that a failed attempt had already consumed.
3. The `lock` serializes concurrent first attempts, which the pre-fix `Interlocked.Exchange` latch
   never did.

**Disposition: no production change is in scope for 4A, and none was made.** The behaviour issue
#782 asks for is already the behaviour of the tree.

---

## Finding 4B — the retry test exists and passes, but its premise was unmeasured and its first
assertion could not discriminate

**Supporting line ranges**, in `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`,
pre-change numbering:

- The test: `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields`,
  declared at line 309, in the class `UiThreadInitRetryContract_Tests` declared at line 306 with
  `[STATestClass]` and `[DoNotParallelize]`.
- The weak assertion: line 318, `failing.Should().Throw<InvalidOperationException>();` with no
  message constraint.
- Its correctly constrained sibling, for contrast: lines 352-354, which do carry
  `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)`.
- The two message sources that assertion could not tell apart: the non-STA rejection built by the
  private helper at lines 233-234 of `UtilitiesCS/Threading/UiThread.cs` from the constant at lines
  230-231, and the capture-failure message declared at lines 25-26 of the test file.

The test calls `UiThread.Init()` directly on the test method's own thread. If that thread were MTA,
the apartment precondition would throw before `Initialize()` ever ran, and the unconstrained
assertion would still pass — for the wrong reason, on a different exception from a different
source. The test never measured the apartment it depended on. Whether the pinned test framework's
STA test-class attribute forces STA for plain test methods is not established by this tree, and the
in-tree record is explicitly contradictory on the mechanism, so the premise could not be settled by
reading either.

**Disposition: this delivery acts on 4B only.** Two in-file test edits were made and no production
code was changed for this finding:

- `Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA);` was added as the
  first Arrange step of that test, converting the unmeasured premise into a measured assertion.
- `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)` was added to the assertion at line 318,
  so it now distinguishes the capture failure from the non-STA rejection. The message comparison
  proving the two texts differ is recorded in the P2-T6 projection.

---

## Evidence that this delivery changed no production code for 4A

The P2-T7 artifact, `evidence/other/p2-t7-ac01-diff-confinement.md`, records the anchored diff of
`UtilitiesCS/Threading/UiThread.cs` against `refs/issue816/base`. It establishes that the removed-line
set has exactly two members — the `_uiSyncContext` condition line and its one-line comment — and
that the count of removed lines containing either `lock (InitLock)` or `_initialized` is **zero**.
`UiThread.Init` and its initialization flag are therefore unchanged by this delivery, which is the
condition AC8 requires.

The research of record does not assert a production residual for 4A; it establishes the opposite,
and this artifact states the status accordingly rather than collapsing the two findings into one.
