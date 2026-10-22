# Code Review — 2026-09-08-etl-deadline-mechanics-follow-ups (Issue #825)

- Date: 2026-09-10T00-45
- Base: 96fd3dd86cff542226d192158f1c2f63d5eee926
- Scope: the full branch diff — five production files, six test files, one project file
- Method: the complete production diff supplied by the invoking agent, plus direct reads of every
  edited and added file in the delivered tree. Bash was not used (see the policy audit for the
  reason and the consequences).

## Verdict

**Approve.** Blocking findings: **0**.

The change is small, well-motivated, and unusually careful about the failure mode it is closing. Its
central design decision — resolve the deadline source **once** into a local, letting an explicitly
supplied factory win over a clock-derived one — is the right call, and it is the reason the existing
direct-caller seam keeps working without a single edit to the four pre-existing binding sites'
semantics.

Nine non-blocking observations follow. None of them justifies withholding approval.

## 1. Design

### 1.1 The seam resolution is correct and minimal

```csharp
Func<int, CancellationTokenSource> resolvedTimeoutSourceFactory =
    timeoutSourceFactory
    ?? (ms => (timeProvider ?? TimeProvider.System)
                .CreateCancellationTokenSource(TimeSpan.FromMilliseconds(ms)));
```

Three properties make this the right shape:

- **The explicit factory still wins.** Any test that already supplied one keeps its exact prior
  behaviour, so a change that widens a public signature costs zero existing-test rewrites. That is
  the property that makes the change safe to land ahead of sibling feature 826.
- **Resolution happens once, at one site.** The two retry recursions forward the *original*
  `timeoutSourceFactory` and `timeProvider` rather than the resolved local, so each recursion
  re-resolves on identical inputs. That is equivalent and it avoids capturing a closure across a
  recursion boundary, which would have been harder to reason about.
- **The default path is unchanged by construction.** `timeProvider ?? TimeProvider.System` means no
  caller that omits both parameters sees any behavioural difference, and
  `GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes` guards that.

### 1.2 The retry-literal fix is the load-bearing half

Threading a `TimeProvider` without fixing the literal 2000 would have been worse than doing nothing:
attempt one under the injected clock and attempt two under a hard-coded wall-clock deadline, which
*looks* controlled and is not. The spec argues this explicitly and the implementation follows it.
Both attempts are now governed by the same caller-visible value on the same caller-supplied clock,
and the regression test asserts on the value the second attempt used rather than on a side effect.

I traced the value myself against the post-change files and confirm the four-step trace holds:

| Step | Location | Behaviour |
|---|---|---|
| Accept | `TableAccess.cs` 36-43 | `timeoutMs` accepted, unvalidated, forwarded |
| Arm | `TimeOutTask.cs` 52-54 | deadline source built from the resolved factory, outside the `try` at 61 |
| Absorb | `TimeOutTask.cs` 65-92 | `TaskCanceledException` absorbed; with `strict: false` anything else logged and swallowed |
| Retry | `TableAccess.cs` 122-128 | `timeoutMs` propagated; **no numeric literal remains** |

### 1.3 The nullable tuple change is the right shape

Widening `EtlAsync`'s first element to `object[,]?` rather than rethrowing is the correct call, and
the spec's reasoning for rejecting the rethrow is sound: the rethrow would have forced a *rewrite*
of the contract test that exists specifically to catch changes to this contract, which is the
failure mode that test was added to prevent. Widening the type deletes the lie without moving the
reporting point.

Propagation is complete. There are five consumers: one production and four test.

- `DfDeedle.cs` (production, `#nullable enable` at line 23) — the retained guard at 186-193 throws
  before any dereference, so the flow state at 197 and 214 is non-null. No suppression, no warning.
- The four test consumers all sit in nullable-warning-disabled contexts, so the widened element
  produces no diagnostic. Verified by grep: `OlTableExtensionsEtlClockTests.cs` and
  `DfDeedleEtlTimeoutTests.cs` carry no `#nullable` directive at all, and `OlTableExtensions_Tests.cs`
  enables only `#nullable enable annotations` in five narrow scoped regions, none covering its
  `EtlAsync` call site at line 969.

### 1.4 The two deletions are genuinely inert

I verified the unreachability mechanism independently rather than accepting the claim. All three
exits of the inner `(int, TimeProvider?)` `TimeoutAfter` overload return a `Task` and none throws
synchronously:

- `return task` at line 846 when already complete or infinite;
- `tcs.SetException(new TimeoutException()); return tcs.Task` at 856-857 for a zero timeout — the
  exception is *placed into* the completion source and control returns normally;
- `return tcs.Task` at 894 after arming a timer whose callback calls `TrySetException` at 868, on a
  timer thread, after the method has already returned.

So the assignment inside the deleted overloads' `try` never threw, their `catch (TimeoutException)`
never ran, `repeatAttempts` was never read, and the "attempts remaining" log line was unreachable.
The deletions are safe and the log-output claim ("no log output changes at runtime") is correct.

## 2. Error handling and contracts

| Check | Finding |
|---|---|
| Any catch widened? | No. `EtlAsync`'s `catch (TimeoutException)` (Etl.cs 129-135) still swallows and still calls `tokenSource.Cancel()`. `RunWithTimeout`'s `catch (System.Exception e)` (TimeOutTask.cs 85-92) is untouched. `strict` remains `false` at the call site. |
| Any catch removed? | No. Both `catch` blocks in `GetTableInViewAsync` survive at 88 and 113. |
| Consumer guard converted to a catch? | No. `if (tableSnapshot.data is null) throw new InvalidOperationException(...)` at DfDeedle.cs 186-193 is retained verbatim, message included. |
| New exception type introduced? | No. |
| Deleted overloads leave a silent rebind? | No. There is no `int`-to-`TimeProvider` conversion, so a surviving caller would have produced a hard overload-resolution error. All four known callers were deleted in the same change and the solution builds clean. |

## 3. Naming, comments and documentation

Strong throughout. Every added comment answers *why*, which is the standard the general policy sets:

- Why a null provider is safe, and the `CancelAfter` prohibition on pre-.NET 8 runtimes
  (TableAccess.cs 32-35). Recording a **constraint on future edits** in a comment at the creation
  site, rather than only in a risks section that will not be read again, is good practice.
- Why an explicit factory still wins (TableAccess.cs 60-62).
- Why the caller's value is now propagated, replacing the old comment that justified the literal
  (TableAccess.cs 118-121). Replacing a now-false rationale rather than leaving it is exactly right;
  a stale justification is worse than none.
- Why the 250 ms constant is unchanged, and the exact route by which it *could* be measured
  (Etl.cs 84-93). This is the most valuable comment in the change: it converts an unexplained magic
  number into a documented open question with a named capture recipe.
- Why reflection is required rather than preferred (test file 74-80), citing CS1769.
- Why the gate is released inside the `try` and again in the `finally` (test file 194-196, 210-213).

The one stale name corrected in `DfDeedle.QfcColumns.cs` keeps the CS1769 rationale sentence and
changes only the member name, which is the minimal correct edit.

## 4. Test quality

Positive observations:

- **The reflective binder is declared once.** `SignatureTypes` is a single static property with a doc
  comment explaining that a future signature change is corrected in one place. That is a genuine
  improvement over the four pre-existing binding sites in `OlTableExtensions_Tests.cs`, which each
  repeat the `Type[]` literal — and it is the reason those four all had to be edited by this feature.
- **`method.Should().NotBeNull(...)` before `Invoke`.** Without it, a binding mismatch after a future
  signature change would surface as a `NullReferenceException` with no explanation. With it, the
  failure names the cause.
- **Gates are released in a `finally`**, so an orphaned `Task.Run` body cannot outlive the test.
- **The `DfDeedleEtlTimeoutTests` update is genuinely bounded.** One gate parameter on `BuildExplorer`,
  one additional `ManualResetEventSlim`, one further await/re-arm pair, and the two sibling tests pass
  an empty lambda. No assertion was added and none removed; the assertions at lines 192-196 are
  unchanged. The comments at 172-188 label each timer by ordinal and by the deadline it belongs to,
  which is what makes a latch-based ordering readable at all.

### Non-blocking observations

**CR-1 — `await barrier.Armed` has no timeout, so a regression hangs instead of failing.**
`GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider` blocks on
`await barrier.Armed` (line 192) with no bound. If a future change stops arming the acquisition
deadline on the injected provider — precisely the regression this test exists to catch — the task
never completes, the gate set at line 197 is never reached, and the test hangs rather than failing.
That is the same failure mode the AC6 amendment cites as its reason for treating the
`DfDeedleEtlTimeoutTests` ordering problem as unacceptable. The test file's own comment at lines
194-196 shows the author was thinking about exactly this class of hazard on the adjacent line.

This is a pre-existing in-repo pattern (`DfDeedleEtlTimeoutTests` and `OlTableExtensionsEtlClockTests`
both do the same), so it is consistency rather than a new defect, and a hang in a `[DoNotParallelize]`
class is contained. Suggested improvement, not required: an MSTest `[Timeout(...)]` attribute on the
three barrier-awaiting tests converts a hang into a clean, diagnosable failure without introducing a
timing tolerance into any assertion.

**CR-2 — the default-path guard necessarily arms a real system-clock deadline.**
`GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes` runs with both optional
parameters omitted, which arms a genuine 2000 ms `CancellationTokenSource` on the system clock, and
then asserts `getTableCalls.Should().Be(1)`. Structurally this is the same wall-clock-under-contention
shape that justified `[DoNotParallelize]` on `OlTableExtensions_Tests` and that this feature removed
from that class.

It is mitigated by `[DoNotParallelize]` on the new class, which is the correct and stated mitigation,
and the spec mandates a default-path guard, so the trade is deliberate rather than accidental. The
reason to record it: a later reader applying the `ac21-justification.md` argument ("no shared mutable
state, therefore no need for the attribute") to `GetTableInViewAsyncClockTests` would reach the wrong
conclusion, because the hazard here is a wall-clock deadline and not shared state. The class comment
says "drives a real Task.Run gate", which is true but understates the reason. Suggested improvement:
extend that comment to name the 2000 ms system-clock deadline in test 4 as the specific reason.

**CR-3 — mixed named and positional tuple access on one value.** `DfDeedle.cs` now reads
`tableSnapshot.data` at 186, 197 and 214 but `tableSnapshot.Item2` at 204 and 214. The feature
converted `Item1` to `data` (correctly, since the type changed) and left `Item2` alone. The element is
named `columnInfo` in the declaration. Cosmetic; a two-token follow-up.

**CR-4 — a now-nullable value is dereferenced unguarded in a test.**
`OlTableExtensions_Tests.cs` line 969 destructures `EtlAsync`'s result and indexes `data[0, 0]` at
979-981 with no null check. No diagnostic is produced because that region is not under
`#nullable enable`. Harmless today and invisible to the gate; it would surface the moment that file
adopts a whole-file nullable pragma. Test-only.

**CR-5 — `ManualResetEventSlim` instances are not disposed** in the new file (lines 173) or in the
edited `DfDeedleEtlTimeoutTests` (149-151). Consistent with the surrounding code and negligible for
a test process, but `using var` would cost nothing.

**CR-6 — the `TimeProvider.System` equivalence is argued, not proven at the IL level.** The comment
at TableAccess.cs 32-33 asserts that a null provider resolving to `TimeProvider.System` leaves
production timing unchanged. `TimeProvider.System.CreateCancellationTokenSource(TimeSpan)` is
expected to reduce to `new CancellationTokenSource(delay)`, but the evidence for that identity is the
passing default-path test rather than an inspection of the `Microsoft.Bcl.TimeProvider` shim. The
residual risk is low — the member's *existence* was properly proven by compilation at AC8, and the
behaviour is exercised — but the claim is one degree stronger than its evidence. Worth knowing if a
timing anomaly is ever reported on the table-acquisition path.

**CR-7 — the retry test uses the factory seam as an exception-injection point.** The regression test
makes `recordingFactory` throw on its first invocation to reach the `catch (TimeoutException)` branch,
which is reachable no other way in the current tree (the ordinary expiry path returns `default` from
`RunWithTimeout` without throwing — see the reachability trace). I judge this acceptable: it is
disclosed in `evidence/other/ac7-mechanism-note.md`, spec.md Test Strategy explicitly permits it and
invites substitution if a better mechanism is found, and the alternative is leaving the defect
untested. It is worth flagging that the test therefore exercises the *retry argument plumbing* rather
than the production route into that branch. The fail-before record makes the plumbing assertion
meaningful, which is what matters.

**CR-8 — `GetTableInViewAsync` still returns null on timeout behind a non-null contract.** Not
introduced here, correctly out of scope, documented in a comment above `return table!` at
TableAccess.cs 136-138 and recorded as deferred follow-up 3. Noted so it is not lost.

**CR-9 — two file-size violations survive.** Covered in the policy audit (NB-1, NB-2). Both were
reduced by this feature. `OlTableExtensions_Tests.cs` at roughly 1822 lines has no recorded follow-up.

## 5. Ownership boundary with sibling feature 826

Verified directly against the delivered file rather than from the diff alone:

- Both `Console.WriteLine($"Task timed out on try {counter}");` statements are present, at post-change
  lines 96 (20 leading spaces, inside `catch (TaskCanceledException)`) and 115 (16 leading spaces,
  inside `catch (TimeoutException)`). The supplied diff contains no added or removed line matching
  `Console.WriteLine`.
- The `counter` parameter survives at line 39; both `catch` blocks survive at 88 and 113.
- Neither diagnostic has become reachable through the ordinary timeout path. I re-derived this from
  `TimeOutTask.RunWithTimeout` (lines 40-95): on ordinary expiry the linked token cancels, the
  `catch (TaskCanceledException)` at 65 is entered, `token.ThrowIfCancellationRequested()` at 67 does
  not throw because the *caller's* token is not cancelled, the internal recursion at 69-79 exhausts,
  line 82 logs and throws nothing, and line 94 returns `result!` which is still `default`. Nothing
  escapes, so the `try` at TableAccess.cs 72 completes normally and neither `catch` runs. This matches
  `evidence/other/ac35-reachability-observation.md` exactly.

Feature 826's three preconditions all hold.

## 6. Judgment on the five recorded deviations

| # | Deviation | Judgment |
|---|---|---|
| D1 | Third sanitisation rewrite (user profile root) added beyond the two D7 names | **Accept.** Forced by a measured 21-line residual leak in two classes D7 does not name (`MSBuildUserExtensionsPath` from the environment, a `_DeploymentUrl` naming a OneDrive folder). A two-rewrite form could not have satisfied the required zero-count check. This is a strict tightening of a hygiene requirement, applied in the correct order (longest prefix first), and the result is confirmed at zero in all five committed logs. |
| D2 | Repo-local .NET SDK installed before P0-T4 | **Accept.** Environment bootstrap using the repository's own documented script. No tracked file changed, no diff affected. |
| D3 | Plan's pinned `Compile Include` count (470→471) does not describe this tree (474→475) | **Accept.** The executor correctly identified the stale literal as an authoring-time figure and fell back to two clauses that *are* properties of the delivered change: exactly one line containing `GetTableInViewAsyncClockTests.cs`, and a numstat of exactly 1 added / 0 removed. The numstat form is strictly more discriminating for what AC29 actually requires — that nothing was reordered or reformatted — than the absolute count ever was. Substituting a stronger check for a stale one, and disclosing it, is the right handling. |
| D4 | The new test binds through reflection rather than the direct call the plan specified | **Accept; the CS1769 justification holds.** I verified both halves. First, the constraint is real: `Outlook.Table` is an embedded interop type in this solution, so `Task<Outlook.Table>` cannot cross the assembly boundary as a generic argument. The same constraint is independently recorded in-source on the `DefaultTableEtl` seam (the very comment item 5 of this feature corrects) and in #798's research on `TableEtlInvoker`/`StoreTableEtlInvoker`. Second, the claim about the four pre-existing binding sites holds: I read all four in `OlTableExtensions_Tests.cs` (at approximately lines 1215, 1273, 1306 and 1637) and each binds `GetTableInViewAsync` by name with an explicit parameter-`Type[]` through the reflective helper rather than calling it directly, and each array now carries `typeof(TimeProvider)`. The plan's "calls GetTableInViewAsync" wording was unimplementable as written; the mechanism changed and nothing the task asserted did. |
| D5 | Ten early artifact `Timestamp:` values corrected from extrapolated to observed, four having run ahead of the real clock | **Accept as complete on the evidence available; see below.** |

### D5 in detail — is the correction complete, and is anything else extrapolated?

**Completeness.** All 41 `Timestamp:` values on disk run monotonically from `2026-09-09T16-32`
(`phase0-instructions-read.md`) to `2026-09-09T17-37` (`review-handoff.md`), and every value is
consistent with the phase ordering it belongs to: baseline capture 16-32 to 16-42, fail-before 16-48,
phase greens 16-57 / 17-03 / 17-07 / 17-15, QC gates 17-14 to 17-20, coverage comparison 17-22 to
17-24, boundary gates 17-26 to 17-29, reconciliation 17-32 to 17-37. The window D5 names — "between
16-33 and 16-44" — contains **exactly ten** artifacts (`base-commit`, `restore`, `dotnet-tool-restore`,
`csharpier-check`, `build-analyzers`, `build-nullable`, `coverage-baseline`,
`coverage-baseline-by-file`, `pinned-source-facts`, `ac8-...-proof`), matching the stated count
exactly. No value now runs ahead of any later phase.

**Caveat, stated plainly.** The ground truth for D5 is each file's `LastWriteTime`, which I could not
read without Bash. So I can confirm the correction is *internally consistent and complete in count*,
but I did not independently verify that each corrected value equals the file's actual write time. That
one sub-claim is unverified by me.

**Are other figures extrapolated?** I swept every quantitative claim in the evidence folder for the
signature of an extrapolated number — a round figure, or one that cannot be reconciled against
another artifact. I found none:

- The coverage figures reconcile *arithmetically*: 55 − 14 = 41, matching the Gate A reduction, with
  residual 0. An invented deletion or addition sum would not close.
- `ac32-non-vacuity.md` records log line counts of 70228 and 70653 and csc-invocation counts of 4 and
  4. These are of a form that is implausible to guess and trivially re-derivable from the committed
  logs.
- `ac7-fail-before.md` quotes a FluentAssertions failure message including the computed "difference of
  1250", which is `2000 − 750`. That is a machine-produced string, not a paraphrase.
- `phase7-green.md` reports 4919 total and 4919 passed, and cross-checks the omitted-counter
  transcription against `Passed:` equalling `Total tests:` per D10.
- `ac8-...-proof.md` records `CS1061Count: 0` and cites a specific corroborating token
  (`/out:obj\Debug\UtilitiesCS.dll`, two occurrences), plus two named confounder exclusions.

The one figure in the record that *does* disagree with the tree — the plan's 470/471 `Compile Include`
count — is disclosed as D3 rather than presented as measured. That is the correct handling and it is
evidence that the executor was checking pinned figures against the tree rather than transcribing them.

**One residual inconsistency, non-blocking:** `spec.md` states the post-change TimeOutTask.cs size as
968 lines and the reduction as 43 lines. The measured values are 966 and 45. `issue.md` and
`evidence/other/file-size-accounting.md` carry the correct figures. This is a pre-execution estimate
in the spec that was not updated, not an extrapolated measurement presented as observed.

## 7. Judgment on the two AC satisfiability substitutions

**AC11 — sound and disclosed.** The criterion requires "a search of OlTableExtensions.Etl.cs for the
null-forgiving return `data!` returns no hit". That is unsatisfiable: the literal
`return (data!, columnDictionary);` occurs twice in the file, at line 63 inside the *synchronous*
`ETL` method and at (pre-change) line 131 inside `EtlAsync`. Only the second is in scope. I confirmed
the post-change state directly: line 63 still reads `return (data!, columnDictionary);` and line 141
now reads `return (data, columnDictionary);`. The occurrence count therefore went from 2 to 1.

The substitution is sound because the surviving occurrence belongs to a different method with its own
separate, pre-existing non-null tuple contract, documented in the comment at Etl.cs 25-27 and
explicitly outside this feature. Changing it would have been scope creep. The 2→1 count is
discriminating — it fails if the wrong suppression is removed or if none is — whereas the zero-hit
form is not achievable by any in-scope edit. It is disclosed in the plan's "Other disagreements"
section item 1, in the P5-T7 check-off text, and in `evidence/qa-gates/ac11-etlasync-tuple.md` with an
anchored diff. Not quietly applied.

**AC22 — sound and disclosed.** The criterion's stated verification is "a search of the file for
`new CancellationTokenSource(` with a numeric argument returning no hit". That search already returned
no hit before any change, because the real 2000 ms source is constructed inside `TimeOutTask.cs` by
the default factory, not in the test file. A check that cannot fail proves nothing. I confirmed the
post-change state: four occurrences of `new CancellationTokenSource(` in `OlTableExtensions_Tests.cs`,
at lines 966, 1254, 1300 and 1662, every one the no-argument constructor.

The substituted assertion is the substance the criterion actually names: that the test formerly at
line 1646 supplies a `FakeTimeProvider`. I confirmed it —
`GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot` at line 1620 passes
`new FakeTimeProvider()` at line 1652 through the new trailing parameter, and its `Type[]` at 1645
carries `typeof(TimeProvider)`. The zero-hit search is retained as a corroborating guard and is
explicitly labelled already-true. Disclosed in the plan's "Other disagreements" item 2.

Both substitutions replace an undecidable or vacuous check with a discriminating one and record the
reasoning where a reviewer will find it. That is the correct way to handle an unsatisfiable
acceptance criterion short of amending it.

## 8. Adherence to the design principles

| Principle | Assessment |
|---|---|
| Simplicity first | One parameter, one local, one literal replaced. No new abstraction. |
| Reusability | Deadline resolution factored into one expression; `SignatureTypes` declared once rather than repeated per call site. |
| Extensibility | Optional trailing parameters keep every overload-resolution caller source-compatible; the two exception categories were enumerated in advance and handled. |
| Separation of concerns | Clock injected, not read. Deadline policy and deadline mechanism stay in separate types. |
| Interact with existing code in its own style | The `TimeProvider?` trailing-optional-parameter pattern matches what #811 established on `EtlAsync` and `AddQfcColumnsAsync`; the reflective binding matches the four pre-existing sites; the `ArmingBarrierTimeProvider` usage matches the precedent at `DfDeedleEtlTimeoutTests`. |
| Treat existing tests as part of the spec | The contract test `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` kept its body and both assertions; only its doc comment changed. The shape (ii) alternative was rejected *because* it would have required rewriting that test. |
