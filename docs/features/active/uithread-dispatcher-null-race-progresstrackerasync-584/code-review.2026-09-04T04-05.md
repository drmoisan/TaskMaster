# Code Review — uithread-dispatcher-null-race-progresstrackerasync (#584)

- **Date:** 2026-09-04
- **Timestamp:** 2026-09-04T04-05
- **Base / merge-base:** `87cb4df338322844abfa580abea14df77e738e5c`
- **Branch:** `bug/uithread-dispatcher-null-race-progresstrackerasync-584`
- **Scope:** full branch diff — 6 files, 1 production, 5 test

**BLOCKING FINDINGS COUNT (this artifact): 0**

---

## 1. Overall Assessment

The production change is the smallest correct fix for the reported defect. It converts an unguarded
static accessor from a silent-null return into an explicit, self-diagnosing failure, and it does so
at the single source rather than duplicating a guard across roughly forty call sites. The chosen
exception type and message shape match an idiom already established elsewhere in the same assembly
(`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:62-66`), so the change increases internal
consistency rather than introducing a new convention.

Two aspects of the execution are worth naming specifically because they are stronger than typical:

1. **The regression was found by the work, not by the reviewer.** The initial blast-radius census
   matched only the literal qualified expression `UiThread.Dispatcher` and therefore could not match
   a reflective read. That gap materialised as a real failure — 8 of 1312 in `QuickFiler.Test` — and
   the response was to run the missing census (`p0-t14-reflective-dispatcher-census.md`), record the
   limitation verbatim in `spec.md` "Risks & Mitigations", widen the write set, and add the sixth
   file to AC4 with an amendment note explaining why omitting it was not tenable. That is the correct
   handling of a discovered gap.
2. **The fail-before evidence is genuine at the assertion level, on both halves.** For the new tests,
   `p1-t4-expect-fail.md` records `Failed: 1` with a FluentAssertions message
   ("Expected a `<System.InvalidOperationException>` to be thrown, but no exception was thrown") on a
   tree that built with `0 Error(s)`, while the sibling positive test passed in the same run — so the
   red is attributable to the defect and not to the harness. For the `EmailMoveMonitorTests` repair,
   `p4-t6-first-pass-failure.md` preserves the 8-of-1312 failure and `p4-t6-quickfiler-tests.md`
   names all eight methods as passing afterwards.

No defect was found in the delivered code. All findings below are non-blocking; the three Low items
are observations a maintainer may reasonably decline.

## 2. Findings Table

| ID | Severity | Blocking | File / location | Finding |
|---|---|---|---|---|
| CR-1 | Minor | No | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` (whole file, 514 lines) | Exceeds the 500-line limit. Pre-existing at BASE at exactly 514; branch delta is zero. |
| CR-2 | Minor | No | `spec.md` "Rollout & Follow-up" item 1 | Deferred residual recorded only as feature-folder prose, which does not survive merge. |
| CR-3 | Low | No | `UtilitiesCS.Test/Threading/UiThread_Tests.cs:164` | Missing `field.Should().NotBeNull()` guard that the sibling test has at `:138`. |
| CR-4 | Low | No | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:55, 64` | `DispatcherField?.GetValue(null)` degrades to a vacuous assertion if `_dispatcher` is ever renamed. |
| CR-5 | Low | No | `UtilitiesCS.Test/Threading/UiThread_Tests.cs:166` | `Dispatcher.CurrentDispatcher` creates thread-affine WPF state on the MSTest worker thread that outlives the test. |
| CR-6 | Low | No | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:14` | Combined `[TestClass, DoNotParallelize]` form diverges stylistically from the three sibling files. Deliberate and documented. |
| CR-7 | Low | No | `UtilitiesCS/Threading/UiThread.cs:135-167` | Three static accessors in one class now have three different null policies. |

## 3. Detailed Findings

### CR-1 — `ProgressTracker_Tests.cs` is 514 lines (Minor, non-blocking)

- **Rule:** `.claude/rules/general-code-change.md`, "File Size Limit" — 500 lines, applying to
  production code, test code, and reusable scripts alike; test code is not excepted.
- **Evidence:** `evidence/baseline/p0-t13-parallel-bucket-census.md` (baseline 514),
  `evidence/qa-gates/p2-t3-file-size.md` (head 514),
  `evidence/qa-gates/p4-t8-loop-closure.md` (post-format 514).
- **Assessment.** The breach pre-dates the branch and the branch does not deepen it. The single-line
  attribute addition was deliberately written as `[TestClass, DoNotParallelize]` on the existing
  line precisely so the count would not move from 514 to 515 — the reasoning is recorded in
  `p1-t5-donotparallelize.md` and re-checked after formatting in `p4-t8-loop-closure.md`, since
  CSharpier could have re-wrapped the attribute list. It did not.
- **Recommendation:** extract a cohesive group of test methods into a `partial class` in a sibling
  file, as separate follow-up work, with no weakening of any assertion. Not a condition of this merge.

### CR-2 — Deferred residual is prose-only (Minor, non-blocking)

`spec.md` "Rollout & Follow-up" records two candidate follow-ups. Item 2 (the `IUiDispatcher` seam
conversion of ~62 remaining direct reads across ~29 production files) is already durable on the
GitHub issue thread. Item 1 is not:

> Add synchronization (or an injectable seam, per the already-partially-adopted `IUiDispatcher`)
> around `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`'s reflection-based mutation of
> `UiThread._dispatcher`, mirroring the fixture-level fix `#493` applied in `QuickFiler.Test`.

The feature folder is removed when the branch merges, so this residual disappears with it. It is a
genuine test-isolation concern on a process-global static, of the same shape as an issue that was
already worth fixing once (#493) in a different assembly.

**Recommendation:** promote item 1 to a GitHub issue before merge.

### CR-3 — Asymmetric null guard on the reflection helper (Low, non-blocking)

```csharp
// UiThread_Tests.cs:136-139  (first test — guarded)
var field = DispatcherField();
field.Should().NotBeNull();
var prior = field.GetValue(null);

// UiThread_Tests.cs:163-165  (second test — unguarded)
var field = DispatcherField();
var prior = field.GetValue(null);
```

`FieldInfo.GetField` returns `null` when the member does not exist. If `_dispatcher` is renamed in a
future refactor, the first test fails with a clear FluentAssertions message naming the expectation,
while the second fails with a bare `NullReferenceException` inside test setup — the exact class of
unattributed failure this issue exists to eliminate. `UiThread_Tests.cs` carries no `#nullable enable`
directive, so the compiler raises nothing here.

**Recommendation:** add `field.Should().NotBeNull();` after line 164 to match the sibling.

### CR-4 — Null-conditional reflection lookup can silently vacate the cleanup assertion (Low, non-blocking)

```csharp
// EmailMoveMonitorTests.cs:55 (TestInitialize) and :64-65 (TestCleanup)
_capturedDispatcher = DispatcherField?.GetValue(null);
...
object current = DispatcherField?.GetValue(null);
current.Should().BeSameAs(_capturedDispatcher);
```

If `DispatcherField` is ever `null` — the field renamed, or its binding flags changed — both sides
become `null` and `current.Should().BeSameAs(_capturedDispatcher)` passes vacuously. The class then
silently loses the static-mutation detection its own comment says it exists to provide.

This is not a defect introduced here: the pre-change code used `DispatcherProperty?.GetValue(null)`
with the identical `?.` shape, and the change only retargeted the member being looked up. AC4's
"unmodified assertions" clause also constrains how far this file could be improved in this change.
Noted so it is not lost.

**Recommendation:** in a separate change, replace `?.` with a one-time `NotBeNull` assertion on the
cached `FieldInfo`.

### CR-5 — `Dispatcher.CurrentDispatcher` leaves thread-affine state (Low, non-blocking)

```csharp
// UiThread_Tests.cs:166
var expected = System.Windows.Threading.Dispatcher.CurrentDispatcher;
```

`CurrentDispatcher` *creates* a `Dispatcher` for the calling thread if none exists and associates it
with that thread permanently. MSTest reuses worker threads, so the created dispatcher outlives the
test. The `UiThread._dispatcher` static itself is correctly restored in `finally`, so no cross-test
state leaks through the unit under test, and `[DoNotParallelize]` prevents interleaving.

The residue is a WPF dispatcher object bound to a pooled thread, with no message pump started
(`PushFrame` is correctly absent, and its absence is affirmatively verified by AC5's token filter).
This is the standard way to obtain a real `Dispatcher` instance without a live host and no better
seam is available given `Initialize()` shows a WinForms window. Recording it as an observation only.

### CR-6 — Attribute-form inconsistency across the four modified classes (Low, non-blocking)

Three files use the two-line form:

```csharp
[TestClass]
[DoNotParallelize]
```

`ProgressTracker_Tests.cs:14` uses `[TestClass, DoNotParallelize]`. The divergence is deliberate — it
is how the 514-line count was held constant (see CR-1) — and is documented in
`p1-t5-donotparallelize.md`. Semantically identical. If CR-1 is ever resolved by a file split, this
should be normalised to the two-line form at the same time.

### CR-7 — Three null policies across three static accessors in one class (Low, non-blocking)

`UtilitiesCS/Threading/UiThread.cs` now exposes:

| Accessor | Backing field null policy |
|---|---|
| `UiSyncContext` (`:113-125`) | Lazy: calls `Init()`, then returns `_uiSyncContext!` |
| `Dispatcher` (`:135-148`) | Fail fast: throws `InvalidOperationException` |
| `AutoScaleFactor` (`:156-167`) | Lazy with fallback: calls `Init()`, then `?? new SizeF(1f, 1f)` |

`spec.md` Root Cause Analysis cites the two lazy accessors as evidence that the omission on
`Dispatcher` was unintentional, then the Proposed Fix deliberately chooses a *different* policy —
throw — rather than matching them. That choice is well-founded and is argued in the spec: `Initialize()`
shows a real hidden WinForms window, so lazily initialising it from an arbitrary caller's thread is
not a safe default, whereas the throw matches the precedent `WpfDispatcherYield.cs` already
established for this exact hazard. Recording this as an observation so the divergence is a conscious,
visible property of the class rather than an accident. The `_uiSyncContext!` suppression at `:122` is
an untouched pre-existing instance of the same pattern this change removed from `_dispatcher`, and is
a candidate for the same treatment in future work.

## 4. Design Principles Review

| Principle | Assessment |
|---|---|
| Simplicity first | The guard is four lines with no indirection. The alternative — guarding at each of ~40 call sites — was correctly rejected. |
| Reusability | The single-source guard is the reusable form. |
| Extensibility | Public type unchanged; the getter's contract is now total and can be extended (e.g. to lazy init) without breaking callers further. |
| Separation of concerns | No I/O or UI added; the guard is pure. |
| Error handling — fail fast | Correct. Named exception, actionable message identifying both the entry point (`UiThread.Init()`) and the initialiser (`UiThread.Initialize()`). |
| Contracts enforced at access | The invariant previously stated only in a trailing comment is now enforced in code, and that comment is correctly deleted rather than left to rot. |
| Comment *why*, not *what* | Exemplary at `EmailMoveMonitorTests.cs:33-37`: the comment records the causal chain (throwing getter -> `PropertyInfo.GetValue` -> `TargetInvocationException` in setup/teardown) and the reason field access is equivalent, which is exactly the information a future reader needs to avoid reverting the change. |
| Public API compatibility | A behavioural break, correctly identified as such and called out in `spec.md` "Backward-compatibility expectations". Blast radius established across all three read routes (see policy-audit §8/B1). |

## 5. Test Quality Review

**Strengths.**

- Both new tests are deterministic by construction: they drive the accessor through its backing
  field rather than attempting to reproduce a timing race. The original failure was 1-of-3
  reproducible; the replacement is 2-of-2 by design.
- Capture-and-restore in `finally` on both tests, so the process-global static is left exactly as
  found even if an assertion throws.
- Message assertion is behavioural, not brittle: `WithMessage("*UiThread.Initialize()*")` pins the
  contract AC1 states (the message names the missing initialisation) without pinning the full
  sentence, so message wording can be improved without a test edit.
- The positive test is not redundant. It passed both before and after the fix, which is what proves
  the RED in the negative test is caused by the defect and not by the reflection arrangement.
- The `[DoNotParallelize]` decision is verified rather than asserted: `p0-t13` records the
  false-before state (zero occurrences across the four files), `p1-t5` the true-after (exactly one
  each), and `p3-t3`/`p4-t5` confirm empirically that serialising exposed no latent ordering
  dependency (41 of 41 and 4787 of 4787).

**Determinism trade-off assessed (prompt point 4).** `[DoNotParallelize]` on four classes trades
parallel throughput for freedom from concurrent interleaving on a process-global static.

- Against `.claude/rules/general-unit-test.md` Core Principle 4 (Determinism) and the
  "Determinism Infrastructure" section: this is the right instrument. The banned-API list targets
  wall-clock waits and uncontrolled time; none is used here. Serialising access to a shared static
  is not a timing tolerance and does not mask a race — it removes the concurrency under which the
  race can occur, while the capture/restore in `finally` independently preserves order-independence.
- Against Core Principle 1 (Independence): `[DoNotParallelize]` alone would not deliver
  independence, since it constrains concurrency rather than order. Independence is delivered by the
  `finally` restore in each mutating test. The two mechanisms are complementary and both are present.
- Against Core Principle 3 (Fast Execution): four classes out of a 4787-test assembly. `p3-t3`
  reports 41 tests in 2.24 s; the final full run is green. The cost is not material.
- **Residual, recorded as informational.** The census behind the `[DoNotParallelize]` decision
  enumerated *reflective* writers of `_dispatcher` only. It did not enumerate the production writer
  path — `UiThread.UiSyncContext` / `UiThread.AutoScaleFactor` getters call `Init()`, which runs
  `Initialize()`, which assigns `Dispatcher`. This review verified the path is not reachable from a
  parallel-bucket test today: no `UtilitiesCS.Test` file reads either property; `ThreadMonitor.cs:143`
  is covered by a class already marked `[DoNotParallelize]`; and `FolderPredictor.cs:178` is driven
  by `FolderPredictorTests.cs:479`, which pre-sets `_uiSyncContext` so the lazy branch is never
  taken. The guarantee therefore holds, but partly by coincidence of an unrelated test's arrangement.
  The durable fix is the deferred `IUiDispatcher` seam conversion. No action required here.

**AC4 "unmodified assertions" clause (prompt point 5) — verified against the complete file.**
Read all 320 lines of `EmailMoveMonitorTests.cs`:

- 8 `[TestMethod]` declarations, matching the base count of 8.
- The only assertion in the touched region, `current.Should().BeSameAs(_capturedDispatcher);` at
  line 65, is byte-identical to its pre-change form; only the expression producing `current` changed
  from `DispatcherProperty?.GetValue(null)` to `DispatcherField?.GetValue(null)`.
- No `.Should()` call anywhere else in the file sits inside the diff region. All eight test bodies,
  every `folder.VerifyAdd` / `VerifyRemove` with its `Times` argument, every `Mock` setup, and the
  `CountingPassThrough` / `CreateMail` / `CreateFolder` helpers are unchanged.
- No `using` directive added; the retarget uses the fully-qualified `System.Reflection.FieldInfo` /
  `BindingFlags` spelling the pre-change code already used.

The clause holds. This is corroborated by `p2-t4-emailmovemonitor-reflection-target.md`'s command 9
(no added or removed diff line carries `.Should()`, exit 1), but the conclusion above is from direct
reading of the file, not from that artifact.

## 6. Verification Performed by This Review

Independently re-derived against the worktree rather than accepted from evidence artifacts:

| Claim | How verified | Result |
|---|---|---|
| `null!` removed; field is `Dispatcher?` | Read `UiThread.cs:149` | Confirmed — `private static Dispatcher? _dispatcher;`, no `null!` in the file |
| Nullable gate is non-vacuous | Read `UiThread.cs:1` | Confirmed — `#nullable enable` present, so `CS8603` would fire without the guard |
| Message names `Initialize()` | Read `UiThread.cs:142` vs `UiThread_Tests.cs:152` | Confirmed — literal contains `UiThread.Initialize()`; assertion pattern matches |
| Exactly one reflective property read existed and is gone | Repo-wide search for `GetProperty(` with a `"Dispatcher"` operand in `*.cs` | Zero hits |
| All remaining `"Dispatcher"` literals are doc cross-references | Repo-wide search for `"Dispatcher"` in `*.cs` | 4 hits, all `<see cref="Dispatcher"/>` |
| Reflective field consumers | Repo-wide search for `"_dispatcher"` in `*.cs` | 6 hits, all field reads, matching the census plus the new helper |
| No `using static` alias route (a route the census did not enumerate) | Repo-wide search for `using static .*UiThread` | Zero hits — gap closed |
| `[DoNotParallelize]` present on all four classes | Attribute search across `UtilitiesCS.Test/Threading` | Confirmed on `UiThread_Dispatcher_Tests`, `IdleAsyncQueue_Tests`, `ProgressTrackerAsync_Tests`, `ProgressTracker_Tests` |
| AC4 assertion-preservation | Full read of `EmailMoveMonitorTests.cs` | 8 `[TestMethod]`, sole touched assertion unchanged |
| No timing constructs in new test code | Full read of `UiThread_Tests.cs` | None of the seven banned tokens present |
| Evidence-location compliance | Directory enumeration of `artifacts/**` | No `baselines/`, `qa/`, `evidence/`, or `coverage/` subtree |
| Plan completion | Checkbox count in `plan.2026-09-02T09-02.md` | 50 of 50 complete, zero unchecked |

**Not verified (no-shell constraint).** csharpier, msbuild, and vstest were not re-executed. Every
head-state fact those gates assert was re-derived by file read and matched, so the residual risk is
limited to the possibility that the recorded exit codes and summary blocks are themselves inaccurate
— for which there is no supporting signal in the artifacts.

## 7. Verdict

**APPROVE.**

**BLOCKING FINDINGS: 0** (0 FAIL, 0 blocking-PARTIAL)

Non-blocking: 2 Minor (CR-1, CR-2), 5 Low (CR-3 through CR-7).

The two Minor items are follow-up work rather than defects in this change. Recommended sequencing:
promote CR-2 to a GitHub issue before merge (it is otherwise lost with the feature folder); handle
CR-1 and CR-3 through CR-7 as separate maintenance work.
