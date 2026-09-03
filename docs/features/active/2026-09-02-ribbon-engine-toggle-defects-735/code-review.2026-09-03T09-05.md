# Code Review — ribbon-engine-toggle-defects (#735)

- **Timestamp:** 2026-09-03T09-05
- **Branch:** `bug/ribbon-engine-toggle-defects-735`
- **HEAD:** `30e66833e73267327a18e58228f493e8c8e3a4dd`
- **Audited range:** `b13d5b7b..HEAD` (equivalently `a679cd08...3e45428e`) — 78 files, 12 of them source

**Blocking findings: 0. Non-blocking findings: 8** (shared numbering with the policy audit).

---

## Production Change Inventory

| File | Change | Lines |
|---|---|---|
| `TaskMaster/Ribbon/RibbonExplorer.xml` | 4 `onAction` renames + 1 element deletion | +4 / -5 |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | new host-neutral gate | +141 |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | gate field/property + `ClearSpamManagerAsync` body deferral | +58 / -26 |
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | ticket capture, compare-and-apply, conditional invalidation, `CompletePrime` restructure | +56 / -26 |
| `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | new extracted versioned cache | +157 |
| `TaskMaster/TaskMaster.csproj`, `TaskMaster.Test/TaskMaster.Test.csproj` | 5 compile items | +5 |

---

## 1. The Concurrency Fix — Adversarial Read

This is the substance of the change and received the closest scrutiny. **The invariant holds.** The
fix is not cosmetic and does not merely reorder the same race.

### Ticket capture points

Both capture points were checked against the plan's literal requirement, not against the commit
message.

- **P3-T7** (`plan.2026-09-02T12-04.md:261`) requires the toggle writer to take a ticket "after the engine toggle completes and before the activation read". `EngineToggleStateCoordinator.cs:224-229`:

  ```
  await engines.ToggleEngineAsync(engineName).ConfigureAwait(false);
  var sequence = _pressedState.NextSequence();
  var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);
  ```
  Exact match. Capturing *after* the toggle await is the load-bearing detail: it is the moment this
  observation window opens, and capturing before the toggle would let a concurrent prime that began
  later carry a higher ticket than the toggle whose effect it cannot yet see.

- **P3-T8** (`:263`) requires the prime writer to take a ticket "immediately before the activation read". `EngineToggleStateCoordinator.cs:318-319`: `NextSequence()` on the line immediately preceding `EngineActiveAsync`. Exact match, with no intervening statement.

### The compare-and-swap loop

`EngineTogglePressedStateCache.cs:98-128`. Traced exhaustively:

- **Key absent** -> `TryAdd`. On success return `true`. On failure `continue`; the key now exists, so the next iteration cannot re-enter this branch. No livelock, because the cache exposes no removal API.
- **Stored ticket >= mine** -> return `false` without writing. This is the guard that refuses the stale write.
- **Stored ticket < mine** -> `TryUpdate(key, new PressedState(...), existing)`. The comparand is the reference just read.

Termination is genuine: each iteration either returns or observes a strictly newer stored value.

**The reference-type choice is load-bearing and correct.** `ConcurrentDictionary.TryUpdate` compares
the comparand against the stored value using `EqualityComparer<TValue>.Default`. `PressedState` is a
`sealed class` with no equality override, so that comparison is reference identity — a true CAS. The
XML doc at `:134-141` explains this, and the explanation is accurate: a value tuple would compare
structurally, and an unrelated writer that happened to store an equal `(bool, long)` pair would
satisfy the comparand check. Because a fresh `PressedState` is allocated on every write, there is no
ABA hazard.

### Does the invariant actually hold?

Yes, under the definition the spec states. Worked through the two production interleavings:

- **Prime-before-toggle:** prime takes ticket *n*, reads; toggle completes its engine call, takes ticket *n+1*, reads post-toggle truth, applies (`n+1 > `absent`), invalidates. Prime resolves late with ticket *n* -> `existing.Sequence (n+1) >= n` -> refused, no invalidate. Cache holds the toggle's value.
- **Toggle-versus-toggle:** identical arithmetic; the earlier-ticketed writer loses regardless of completion order.

The one honest limitation is recorded as NB-5: the ticket orders the *invocation* of
`EngineActiveAsync`, not the instant the engine samples state internally. That is precisely the
freshness definition `spec.md:83` adopts, so the implementation is faithful to its contract rather
than deviating from it.

### Conditional invalidation

`if (_pressedState.TryApplyState(...)) _invalidateControl(controlId);` at `:231-234` and `:320-323`.
Correct, and correct for a non-obvious reason: a refused write means a newer writer already stored
its value *and already invalidated after storing it*. The update-before-invalidate ordering that the
existing fixture pins is preserved on every applied path. The risk of over-suppression is real
enough to warrant its own test, and one exists (see §2).

### `CompletePrime` (CR-2)

`:334-352`. The pre-fix handler keyed on `completed.Exception`, which is `null` for a canceled task,
so a cancellation returned early: nothing logged, marker left registered, re-prime blocked for the
session. The fix tests `completed.Status == TaskStatus.RanToCompletion` instead and synthesizes a
`TaskCanceledException` when there is no exception to unwrap. The faulted path still reports
`GetBaseException()`, preserving the existing test that asserts the unwrapped instance by reference.
The `(Exception)` cast on `completed.Exception?.GetBaseException()` is redundant (the expression is
already `Exception`) but harmless and arguably aids readability of the `??` operand types.

**Assessment: the concurrency fix is sound.** No Blocking finding.

---

## 2. Can the Tests Fail? — Adversarial Read

### Determinism

All six race tests drive interleaving through held `TaskCompletionSource<bool>` instances and await
the coordinator's own `GetPrimeTask` handle. No test sleeps, polls, reads the wall clock, touches
the filesystem, creates a temporary file, or starts a message pump. This satisfies
`.claude/rules/general-unit-test.md` Determinism Infrastructure and the "banned APIs in test code"
list.

The ordering guarantees were checked rather than assumed. In
`ExecuteToggleAsync_WhenOlderObservationCompletesLast_...`
(`EngineToggleStateCoordinatorTests.Race.cs:94-95`), the two toggles are started back to back and
ticket order is deterministic because `ToggleEngineAsync` returns `Task.CompletedTask`: awaiting an
already-completed task does not suspend, so each call runs synchronously through `NextSequence()`
and only suspends at the held `EngineActiveAsync`. The first-started toggle therefore reliably holds
the older ticket. This is a real guarantee of the await state machine, not a timing race.

### Independent prediction versus recorded evidence

Before opening the fail-before TRX, I derived from first principles which of the six tests *must*
fail against the pre-fix coordinator:

| Test | Predicted pre-fix | Recorded in `evidence/regression-testing/p3-t5/p3-t5.trx` |
|---|---|---|
| `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult` | FAIL | **Failed** |
| `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult` | FAIL | **Failed** |
| `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` | FAIL | **Failed** |
| `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce` | pass (pre-fix invalidates unconditionally) | Passed |
| `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationException...` | pass (CR-3, zero production change) | Passed |
| `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked` | pass (pre-fix also leaves cache unset) | Passed |

**Six of six predictions match.** The plan's "exactly 3 of 6 failed pre-fix" claim is confirmed
against the TRX, not accepted from prose. (The TRX contains 7 `outcome=` tokens; the seventh is the
run-level `ResultSummary`, not a test.)

The failure messages confirm the tests failed for the *defect* reason rather than a compile or
harness error:

```
Expected harness.Coordinator.GetPressed(SpamEngine) to be True because the newer observation
must survive regardless of completion order, but found False.
Expected harness.Errors to contain a single item because a canceled prime is a failure and must
be reported, not silently ignored, but the collection is empty.
```

Timeline corroborates RED-first: the fail-before run is stamped `2026-09-03T01:41:47`; the race-fix
commit `a68c8598` is dated `2026-09-03 01:47:13`, roughly six minutes later.

### Finding 1 fail-before

`evidence/regression-testing/p1-t2/p1-t2.trx` — both new XML tests Failed, with messages naming
**5 of 84** unresolved callbacks (`BtnMigrateIDs_Click`, `MoveEntireConversation_Clicked`,
`SaveAttachments_Clicked`, `SaveEmailCopy_Clicked`, `SavePictures_Clicked`) and **4** check-box
defects. Matches the spec's stated counts exactly.

### Test quality

Assertions carry `because` reasons throughout, producing actionable failure messages. AAA structure
is explicit and commented. The `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` test
documents at `:195-202` why assertion order is load-bearing (a re-prime re-enters the same canceled
task and logs a second error, so the single-error assertion must precede it) — this is a genuine
hazard, correctly identified and correctly handled, and the marker-cleared conclusion is drawn from
prime-handle identity, which is deterministic rather than count-based.

`EngineTogglePressedStateCacheTests.cs` covers the ticket source, the reader, ordinal
case-sensitivity, and all four `TryApplyState` outcomes including the equal-ticket boundary. The two
uncovered lines (94.87% line coverage) are the two CAS retry paths, which require a real thread
collision to reach; the commit message says so and the claim is consistent with the arithmetic
(39 lines x 0.948718 = 37 covered, 2 uncovered).

**Assessment: the tests can fail, and three of them demonstrably did.** No Blocking finding.

---

## 3. Scope Creep via the Contingency Branch — Adversarial Read

Verified independently rather than accepted. Measuring the coordinator at `a68c8598`, the commit
immediately preceding the extraction:

```
git grep -c "" a68c8598 -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
-> 515
```

**515 lines, against a 500-line hard ceiling.** The extraction was compelled, not opportunistic. It
was pre-authorised by the plan (`:67`, `:302`) and the plan's scope gate explicitly admits the
resulting paths (`:326`). Post-extraction the coordinator is 415 lines and the new cache is 157.

**Documentation was not trimmed to fit.** The coordinator's cache-related XML documentation was
relocated into the new file's `<remarks>` block, which carries 33 lines of type-level documentation
including the rationale for the reference-type choice and an explicit note that the extraction
exists to respect the file ceiling. No `///` block was deleted without a destination.

The extraction is also behaviour-preserving in the strict sense: the coordinator's public and
internal surface is unchanged, the only structural difference being that `GetPressed` now calls
`TryGetActive` instead of indexing a dictionary directly.

**Assessment: legitimate contingency, correctly executed and correctly documented.** No finding.

---

## 4. Finding 2 — The Gate

`SpamManagerResetGate.cs` follows the `EngineReadinessGate` / `EngineGatedCommandRunner` precedent
closely and correctly:

- Three constructor dependencies, each validated with `?? throw new ArgumentNullException(nameof(x))`.
- `RunAsync` throws for a null `reset` **before** either accessor is invoked (`:106-109` precede `:111`), so a caller error is never masked by a "not ready" notice.
- The manager is resolved through a null-conditional (`autoFile?.Manager`), the gate notifies exactly once and returns `Task.CompletedTask` when either dependency is null, and otherwise `return reset(manager, engines)` — returned directly, not awaited, so a fault propagates unchanged. There is no `catch` clause anywhere in the type, so it cannot degenerate into a swallow-all.
- Usings are exactly the four the spec permits: `System`, `System.Globalization`, `System.Threading.Tasks`, `UtilitiesCS`. No `Microsoft.Office`, no `System.Windows.Forms`, no logger field.

The call site (`RibbonController.Intelligence.cs:206-227`, `:229-266`) is faithful: the
synchronization-context preamble and the confirmation dialog are unchanged and in their original
order, the confirmation is inverted to an early return, and the four engine-touching statements move
verbatim into the lambda with `manager` and `engines` substituted for the globals chain. No inline
ad-hoc null guard was introduced. The `Globals?.AF!` / `Globals?.Engines!` null-forgiving accessors
carry an explanatory comment matching the established precedent.

Only NB-1 (the inert `string.Format`) applies here.

---

## 5. Finding 1 — The XML and Its Tests

The XML diff is exactly four `onAction` renames plus one `<button id="BtnMigrateIDs">` deletion —
nine changed lines, no other attribute or element touched, no unintended CSharpier reflow.

`RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod` enumerates
`document.Descendants()`, which yields element nodes only, so the commented-out callbacks are
excluded structurally rather than by regex — this is a genuinely better design than a comment-
stripping heuristic. Because `XDocument.Descendants()` includes the root, the `customUI` `onLoad`
callback is covered without a special case. The callback predicate (`onAction`/`onChange`/`onLoad`
or `StartsWith("get")`) is exact for the 2009 CustomUI schema and stays correct if a new getter is
introduced.

`HasCheckBoxActionShape` correctly uses `candidates.Any(...)` rather than inspecting only the first
overload, and compares the first parameter by `FullName` because the test project carries no Office
PIA reference. Both failure messages aggregate all offenders into one report, so a single run lists
everything — which the fail-before TRX demonstrates it does.

No finding.

---

## 6. Design, Naming, and Structure

- **Separation of concerns** — good. The versioned cache is pure decision logic with no I/O; the gate is pure availability logic; both are host-neutral and neither touches COM, WinForms or a logger. This is exactly the extraction direction `.claude/rules/general-unit-test.md` prescribes for host-bound code.
- **Naming** — `TryApplyState`, `NextSequence`, `TryGetActive`, `SpamManagerResetGate` are descriptive and behaviour-named. No cryptic abbreviations.
- **Comments explain why, not what** — consistently. The reference-type rationale, the ticket-capture rationale, the conditional-invalidation rationale and the `CompletePrime` status-versus-exception rationale are all recorded at the point of the decision.
- **Public surface** — both new types are `internal sealed`, and `PressedState` is `private sealed` nested. Minimal and intentional.
- **Error handling** — fail-fast throughout: `ArgumentNullException` in constructors, `ArgumentException` for an unmapped key, `InvalidOperationException` for unavailable engines. Exactly one `catch` remains in the coordinator, at the `async void` click boundary, which is the correct place for it.
- **Test file location** — all new test files are under `TaskMaster.Test/Ribbon/`, mirroring `TaskMaster/Ribbon/`. No colocation in the production tree.
- **Framework compliance** — MSTest attributes, Moq mocks, FluentAssertions assertions throughout, per CLAUDE.md CUT1/CUT2.

---

## Findings Recap

| # | Severity | Summary | Location |
|---|---|---|---|
| NB-1 | Non-blocking | `string.Format` with a constant format string and zero arguments; inert, latent `FormatException` | `TaskMaster/Ribbon/SpamManagerResetGate.cs:132-139` |
| NB-2 | Non-blocking | Committed PR context artifacts describe issue #730, not #735 | `artifacts/pr_context.summary.txt` |
| NB-3 | Non-blocking | False reconciliation claim covering an off-by-one test count; same error in commit message | `evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md:25-34`; commit `3e45428e` |
| NB-4 | Non-blocking | Test fixture at 496/500 lines | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` |
| NB-5 | Non-blocking | Ticket orders read-initiation, not engine-sampling instant (spec-consistent limitation) | `EngineToggleStateCoordinator.cs:228`, `:318` |
| NB-6 | Non-blocking | Rule-text conflict on the pre-existing `[ExcludeFromCodeCoverage]` COM/VSTO exemption | `TaskMaster/Ribbon/RibbonController.cs:36` |
| NB-7 | Non-blocking | Coverage figures not reproducible against the post-#733 script (judged still trustworthy) | `evidence/qa-gates/coverage-final...cobertura.xml` |
| NB-8 | Non-blocking | Four `.claude/agent-memory/**` files in the branch footprint, from prep commit `044551f0` | `.claude/agent-memory/**` |

**No Blocking findings. No remediation cycle is required.**
