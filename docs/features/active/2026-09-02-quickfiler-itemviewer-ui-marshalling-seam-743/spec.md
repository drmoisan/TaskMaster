# QuickFiler `ItemViewer` UI-marshalling seam (Spec)

- **Issue:** #743
- **Work Mode:** full-bug
- **Parent (optional):** none (consolidates closed #592, which itself superseded #511 and #571)
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Draft
- **Version:** 1.0

> This file is the sole authoritative acceptance-criteria source for this item, per the
> `acceptance-criteria-tracking` skill's `full-bug` rule. The sibling user story is narrative context
> only and carries no checkboxes.

---

## 1. Context

`QuickFiler.Test` pump-hosted `QfcItemController` and WebView2 breadcrumb-host tests intermittently
expire at the 60,000 ms `[Timeout(PumpTimeoutMs)]` harness bound when the suite runs with
`/EnableCodeCoverage` on a loaded machine, and pass on a byte-identical tree when the machine is idle.

The item was reported as #711, consolidated into #729, ruled out of scope there because #729 is a
test-only item, and re-promoted here. Issue #592 was closed as a duplicate root cause on
2026-09-11 (state reason NOT_PLANNED) and its acceptance criteria are carried into the Acceptance
Criteria section of this spec. #511 and #571 were previously closed as superseded by #592.

This spec replaces the auto-generated template that mapped the promoted record's prose. Two sentences
from that template were removed rather than carried forward, because both name remedies this item
forbids: "scaling the harness bound to the environment" is a timing tolerance, and "allowing a
synchronous fake to replace the real message pump" is the prohibition recorded in section 5.

---

## 2. Corrections to the inherited record (established before planning)

These four corrections are load-bearing. Planning and execution must use them in preference to the
prose they correct.

**C1 — the maintainer's first lead is falsified as literally stated.** The identifiers
`UiThreadDispatcherGate` and `SwapUiThreadDispatcher` exist in **zero** `.cs` files in the current
tree. A repository-wide search restricted to `*.cs` returns no files; the surviving references are in
the docs tree and agent memory only, and the most recent code-bearing reference is the record of their
*removal* under issue #493. The mechanism that exists today is `UiThreadDispatcherFixture` /
`UiThreadDispatcherTransaction`, whose own doc comment at lines 12-13 of
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs attributes the design to
issue #493. Issue #648 only added a consumer. Any acceptance criterion, plan task, or evidence
artifact that identifies the mechanism by the two removed names is invalid on its face.

**C2 — #493 changed the owner of the serialization, not its shape.** `TransactionGate` is still a
one-permit `SemaphoreSlim(1,1)`, still awaited without timeout or cancellation token, and still held
from acquisition to disposal. The lead must therefore be re-tested against the current gate, not
assumed closed and not assumed open.

**C3 — three of the four supplied `ViewerSetup.cs` citations do not resolve.** Lines 320, 331 and 336
carry no marshalling site. The actual sites in `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
are lines 64, 282, 287, 298 and 303 (synchronization-context path) and line 371 (WPF dispatcher path).
Lines 36, 273 and 365 are comment references, not executable sites. Line 64 is inside a method that is
excluded from coverage measurement; see C4.

**C4 — two coverage exclusions make the naive form of criterion 4 unfalsifiable.**
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` line 47 carries
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` on `InitializeWebViewAsync`, declared at
line 48, and the primary cited edit site at line 64 is inside that method. An edit confined to that
method produces no coverage delta at all. Separately, `QuickFiler/Viewers/ItemViewer.cs` line 20
carries a **type-level** `[ExcludeFromCodeCoverage]`, so the whole `ItemViewer` type — including its
6,223-line Designer partial — emits no Cobertura `<class>` element whatsoever and is unmeasurable, not
0 %. No acceptance criterion may be phrased over `ItemViewer` coverage, and no acceptance criterion may
be phrased as a bare per-file percentage over the two controller partials without a stated denominator.

---

## 3. Reproduction and measured evidence

### 3.1 Reported reproduction

1. Run the full discovered `QuickFiler.Test` set with `/EnableCodeCoverage` while the machine is under
   concurrent load.
2. Observe pump-hosted controller and breadcrumb-host tests fail at approximately 60 seconds each, with
   a `[Timeout]` message rather than an assertion-failure message.
3. Re-run the same command against a byte-identical tree with the machine idle.
4. Observe the same tests pass.

Note a discrepancy that changes which hypothesis a run tests: #743's stated command passes no
`/Settings:`, and CI passes no `/Settings:` either, so MSTest runs the assembly serially. #711's
original command *did* pass `/Settings:TaskMaster.runsettings`, which declares `<Workers>0</Workers>`
and `<Scope>ClassLevel</Scope>`. The two commands are not the same experiment. The plan must state
which one it runs, per run.

### 3.2 What is measured and what is not

| Figure | Value | Source quality |
|---|---|---|
| `PumpTimeoutMs` census | 4 declarations, 19 usages, all `[Timeout(...)]` arguments, 0 in a wait or poll position | Re-derived twice on 2026-09-12, unchanged from the #729 census, same line numbers |
| Gate-taking population | 21 test methods in 6 `[TestClass]` types contend for the single `TransactionGate` permit | Two independent censuses, acquire-side and release-side, agreeing member for member |
| Bypassing production sites | 8 sites in the `QfcItemController` partial read a marshalling primitive off the viewer instead of the injected seam (5 `UiSyncContext`, 3 `UiDispatcher`) | Usage census cross-checked against the interface's published marshalling surface |
| Measured gate blocking | approximately 4.10 s, i.e. 6.8 % of the 60,000 ms bound | Committed TRX timestamps, class-parallel idle run |
| Measured pump-test cost | first test in a class 3.3 s to 6.2 s; siblings 0.19 s to 1.04 s | Two committed TRX files, both idle, both 24-worker class-parallel |
| Measured load multiplier | 6x to 26x unloaded duration under sustained CPU saturation | Ten-run #511 determinism evidence |
| Pre-fix run-level failure rate | approximately 1 in 21, approximately 4.8 % | **Point estimate from a single observed failing run.** Exact 95 % interval for 1 of 21 is approximately [0.0012, 0.2382] |

Applying the upper measured multiplier to the largest measured pump test gives 26 x 6.223 s, or
approximately 162 s, which exceeds the 60,000 ms bound; the lower multiplier gives approximately 37 s,
which does not. The bound therefore sits **inside** the measured load-multiplier band, which is the
signature of an intermittent load-correlated expiry. This is stated no more strongly than that: the
multiplier was measured suite-wide, not per test.

### 3.3 Recorded unknowns, carried forward unresolved

These are not to be resolved by assertion in the plan or in any evidence artifact.

- **U1.** The identity of the seven tests that expired in the one recorded genuine failure is
  **unrecoverable**. The underlying TRX is not in this worktree; #511 never merged, and its work is
  preserved unmerged on branch bug/winformspumphost-suite-determinism-511-exec at commit 53a2a08f.
  A repository-wide search of the docs tree for timeout signatures returns zero matches, so **there is no
  committed artifact of an actual expiry anywhere in this tree.**
- **U2.** Whether the gate leak described in section 4.2 has ever actually occurred is **unknown and
  untested**. Weak counter-evidence exists (no CI hang of the predicted shape has been reported, and
  four gate-taking tests carry no `[Timeout]` at all, so a leak should hang one of them under CI's
  serial ordering), but ordering is not pinned and this is not proof.
- **U3.** #711 reported fourteen failing tests; #511 recorded seven expiries. These are two
  observations on two different days under two different commands and cannot be reconciled from
  in-repo evidence.
- **U4.** Whether an MSTest-abandoned `async` continuation later resumes — releasing the gate late
  rather than never — is not verified. Deciding it requires either testfx source inspection or an
  experiment.

---

## 4. Root cause analysis

### 4.1 The production defect, which is not in dispute

Eight executable sites in the `QfcItemController` partial class obtain a UI-marshalling primitive
directly from the viewer rather than through the `UtilitiesCS.Threading.IUiDispatcher` seam that the
controller already accepts by injection:

| Channel | Sites |
|---|---|
| `_itemViewer.UiSyncContext` | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` lines 64, 282, 287, 298, 303 |
| `_itemViewer.UiDispatcher` | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` line 371, plus one site each in the Initialization and FolderHandling partials |

`QuickFiler/Viewers/ItemViewer.cs` captures both primitives in its constructor at lines 23-29, from
`SynchronizationContext.Current` and `Dispatcher.CurrentDispatcher`, and publishes them read-only. A
test therefore cannot supply either one without a live WinForms message loop.

Compounding this, `QfcItemController` reaches the viewer through 14 `(ItemViewer)_itemViewer` concrete
casts plus one concrete signature, `internal async Task ResolveControlGroupsAsync(ItemViewer itemViewer)`
at line 276 of `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`. At line 288 that method calls
`itemViewer.GetAllChildren()`, an extension method on `System.Windows.Forms.Control`, which cannot be
invoked through `IItemViewer` at all. **This is the single most important design conclusion available:
making the marshalling injectable does not by itself make the tests cheap.** As long as the concrete
casts stand, every test of those members must construct a real viewer.

The construction cost is structural: `QuickFiler/Viewers/ItemViewer.cs` calls `InitializeComponent()`
unconditionally, and the Designer partial is 6,223 lines containing 110 control constructions including
two WebView2 children whose `EndInit()` calls create their handles, which in turn creates the parent's
handle.

### 4.2 Two competing mechanisms for the expiry, neither yet measured

The spec deliberately does not pick between these. Acceptance criterion 1 exists to settle it.

**Hypothesis H-COST — raw elapsed fixture cost.** The real elapsed cost of the control-tree
construction, inflated by coverage instrumentation and by CPU contention from the other assemblies in
the combined run, crosses the 60,000 ms bound. Favoured by the #729 research. Predicts expiries that
arrive **independently** of one another.

**Hypothesis H-LEAK — a leaked or late-released transaction.** MSTest 4.4.0 is configured with
`useCooperativeCancellation` at its default of `false`, and the authoritative Microsoft Learn wording
for the non-default `true` setting is that it "will only trigger cancellation of the `CancellationToken`
but will **not stop observing the method**". At the repository's configuration the adapter therefore
does stop observing a timed-out method. For an `async Task` test there is no mechanism to abort a
suspended state machine, so the remainder of the method — including the `finally` that calls
`harness.Restore()` and thence `UiThreadDispatcherTransaction.Dispose` and
`ReleaseTransactionGate` — is not awaited. Every subsequent acquirer then blocks on a permit whose
holder the runner already considers finished. Predicts a **cluster** of expiries following one initial
failure, and an unbounded hang on the four gate-taking tests that carry no `[Timeout]` at all.

The #511 spec recorded H-LEAK precisely at its lines 132-139 and explicitly left it unfixed. It
survives #493 verbatim with `TransactionGate` substituted for the removed name.

**Discriminating observable.** The two hypotheses are distinguished by whether the permit is free at
acquisition. Under H-COST every acquisition on a serial run finds the permit free and the elapsed cost
is charged to construction; under H-LEAK at least one acquisition finds the permit held with no live
holder. This is the observable that criterion 1 requires to be declared before the measuring run and
recorded after it.

**Note on serial execution.** `QuickFiler.Test` declares no `[assembly: Parallelize]`; the only
parallelization attributes in the assembly are two `[DoNotParallelize]` class attributes in the helper
tests. Under CI's serial ordering the simple queue-wait story cannot arise, which narrows the gate
hypothesis sharply to its leak variant. Cross-assembly parallelism does not contend on this static
gate, which lives only in `QuickFiler.Test`, but it does contend for CPU.

---

## 5. Scope and non-goals

### 5.1 In scope

- An **additive** UI-marshalling seam that lets `QfcItemController` members which do not require a real
  Win32 message loop be exercised without constructing the full pump fixture.
- Instrumentation sufficient to decide between H-COST and H-LEAK, and the corresponding fix for
  whichever is found operative.
- A deterministic regression test for the identified mechanism.
- Reconciliation of the forward pointers on #511 and #571.

### 5.2 Explicit non-goals (each is a hard prohibition, not a preference)

1. **No timing tolerance of any kind.** Raising `PumpTimeoutMs`, scaling it by core count, scaling it
   by measured machine speed, adding a retry, adding a sleep, adding a `Stopwatch`-based tolerance, or
   applying `[Ignore]` are all forbidden. This is the inherited epic constraint and it remains in force.
2. **No fake `SynchronizationContext` replacing the real message pump in the existing pump-hosted
   tests.** Those tests keep `WinFormsPumpHost` and keep exercising the real message loop. Their
   coverage contribution is exactly what this constraint protects. A nuance the plan must not trip
   over: a `DrainableSynchronizationContext` test double already exists in the assembly and is used by
   breadcrumb tests that never used the pump. Its existence is not a precedent for introducing one into
   a pump-hosted test.
3. **The final clause of the inherited #729 recommendation is rejected.** That recommendation ends
   "...so a synchronous fake can replace the message loop **entirely**." The seam is adopted; the word
   "entirely" is what fails. A partial, additive seam is permitted and is the intended remedy. Recorded
   in full in the constraint-conflict artifact listed in section 11.
4. **The seam must be additive to `IItemViewer`, never a replacement.** Reflection contract tests at
   lines 248 and 264 of QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs assert that
   `UiDispatcher` and `UiSyncContext` still exist as declared members, with the stated reason that
   `UiDispatcher` "still has production consumers and must survive". Removing, renaming or narrowing
   either member fails existing tests.
5. **`[Timeout(PumpTimeoutMs)]` is not to be removed.** It is a documented deadlock guard, not the
   load-sensitivity source. Removing it trades a bounded, diagnosable failure for an unbounded CI hang.
6. **No interface over the `WebView2` control.** Rejected: it addresses at most the eight breadcrumb-host
   tests, whose measured durations are already 0.018 s to 0.16 s, i.e. the cheapest tests in the
   population; and it would require the interface to carry two events and `ConditionalWeakTable` key
   identity, which is disproportionate. The control is already constructor-injected today; what is
   concrete is its type.
7. **No `[DoNotParallelize]` on the six gate-taking classes.** A no-op under CI, and under a
   `/Settings:`-driven run it would serialize classes the gate already serializes. It would also make
   the H-LEAK cascade deterministic rather than order-dependent, which is worse for diagnosis.
8. **No shared pump host or shared viewer instance across a test class.** This is the single
   highest-leverage cost reduction available but it introduces order-coupling between tests, contrary
   to the independence principle in the repository's general unit-test rule. Recorded here as
   considered-and-rejected; promote it separately if it is ever wanted.
9. **No `.trx` and no `.cobertura.xml` committed.** See section 8.
10. **No edits to the WebView2 breadcrumb host or to the Initialization and FolderHandling controller
    partials.** Deferred, with reasons in section 6.4.

### 5.3 Constraint deliberately re-opened

**Production edits ARE permitted for this item, and only for this item.** The
`quickfiler-suite-determinism-foundation` epic forbade them; #743 exists because the defect cannot be
closed without them. The other two epic prohibitions (non-goals 1 and 2 above) remain in force.

---

## 6. Proposed fix

### 6.1 Invariant

Every UI-thread marshal performed by `QfcItemController` on behalf of a member that does not create or
manipulate a Win32 window handle must go through the injected `IUiDispatcher`, so that the marshaller
is substitutable; and every marshal performed by a member that does create or manipulate a handle must
continue to go through the real message pump, so that no coverage or guarantee is lost. The seam
changes **which marshaller is chosen**, never **whether a real pump exists**.

### 6.2 Part A — route the interface-reachable marshalling sites through the existing seam

The controller already declares an `IUiDispatcher _uiDispatcher` field, already accepts it by
injection, already defaults it to `WpfUiDispatcher` in the production path, and the test assembly
already has a synchronous double for it. No new abstraction is introduced and no new injection wiring
is required.

In `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, replace the `await _itemViewer.UiSyncContext`
and `_itemViewer.UiDispatcher.InvokeAsync(...)` forms at lines 282, 287, 298, 303 and 371 with
`_uiDispatcher`-mediated marshals.

Line 64 is **not** converted in this item. It sits inside `InitializeWebViewAsync`, which is excluded
from coverage measurement and whose residual barrier is a real WebView2 runtime, an external process
barred by the unit-test policy. Converting it would change nothing measurable and would enlarge the
diff on a contended file.

Two risks must be established **before** the edit, not after:

- `await ctx` and `IUiDispatcher.InvokeAsync` are not the same primitive. A WinForms
  `SynchronizationContext` and a WPF `Dispatcher` are different queues on the same thread. Production
  ordering on the UI thread must be shown unchanged.
- `_uiDispatcher` is null until `SaveParameters` applies its default, and the assembly already records
  an observed null in the seam-factory tests. Every converted site needs the same null tolerance the
  existing sites have.

### 6.3 Part B — remove the concrete-viewer dependency from `ResolveControlGroupsAsync`

Widen `ResolveControlGroupsAsync(ItemViewer)` to accept `IItemViewer`. This is the change that actually
removes the need to construct a real viewer for that member, and therefore the change that Part A alone
does not deliver.

It requires two **additive** members on `QuickFiler/Viewers/IItemViewer.cs`, implemented on
`QuickFiler/Viewers/ItemViewer.cs`:

1. A descendant-control enumeration replacing the `GetAllChildren()` extension call at line 288 of
   `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, which cannot be invoked through the
   interface.
2. An accessor for the item-number label consumed at line 281. The interface publishes
   `ItemNumberText` as a string but not the underlying `Label`, and the call site needs the control.

The widening is source-compatible for existing callers passing a concrete viewer. The precedent for
adding intent members to this interface is already established in the same file: the display-state
members at lines 39-52 and the re-declared `InvokeRequired` / `Invoke` / `BeginInvoke` trio at lines
192-194 were added for exactly this reason. `QuickFiler/Viewers/IItemViewer.cs` is 200 lines against
the 500-line cap, so there is ample headroom.

The `#230` justification comment at lines 272-275 of
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` names the test that covers the de-exempted
member and **must** be updated to name the retained test, per the de-exemption rule restated in
section 7.3.

**Dependency to confirm at Phase 0.** The determinism epic assigned `IItemViewer` rewriting to issue
#489. Its display-state members are present in the tree, so #489 landed at least partially, but its
disposition was not confirmable during preparation. Confirm it before executing Part B; if #489 is
still open, coordinate or defer Part B and deliver Part A plus the mechanism work alone.

### 6.4 Part C — the mechanism fix, branch-dependent

Which branch executes is decided by criterion 1, not by this spec.

- **If H-LEAK is operative:** move the gate release out of the abandoned async path in
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, for example by
  anchoring it to a `[TestCleanup]`- or `[AssemblyCleanup]`-reachable owner rather than to the timed-out
  test's own `finally`. Add the deterministic regression test to
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, which is 353 lines
  and has 147 lines of headroom.
- **If H-COST is operative:** Parts A and B are the fix, and
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` is touched only by the
  instrumentation needed to record the discriminating observable, which is then either retained as a
  permanent assertion or reverted.

### 6.5 Deferrals, stated as prose without paths so the scheduler does not serialize on them

The two marshalling sites in the Initialization and FolderHandling controller partials are **not**
converted in this item. Both partials are near the 500-line cap (Initialization is 497 lines, three
lines of headroom), both are single `UiDispatcher` reads, and converting them adds contended surface
without changing any measured figure. The existing viewer-setup test file is 498 lines with two lines
of headroom, so no test may be added to it; the interface widening is source-compatible, so it is
expected to compile unchanged, and the plan must verify that expectation rather than assume it. The
WebView2 breadcrumb host is out of scope per non-goal 6 and is concurrently edited by a sibling item.

---

## 7. Test disposition

Every test the item touches is classified below. A test may be placed in the MOVED category only with a
stated justification that it does not depend on real Win32 handle creation or `Control.BeginInvoke`
marshalling.

### 7.1 Touched tests

| Test / file | Classification | Justification |
|---|---|---|
| New seam tests in `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` covering `ResolveControlGroupsAsync` through `IItemViewer` | **MOVED to the seam** | The member reads labels, enumerates descendant controls and marshals. After Part B none of that requires handle creation: it is exercisable against a mock viewer plus a handful of real `Label` instances on a bare `UserControl`. Drive it with the existing synchronous `IUiDispatcher` double and **no** pump host. |
| New deterministic mechanism regression test (file as above, or the fixture test file if H-LEAK is operative) | **MOVED to the seam** | Asserts a structural property — that the member completes with a synchronous dispatcher and constructs no real viewer, or that a subsequent gate acquisition completes after its predecessor's owner is abandoned. Neither assertion involves the message loop. |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | **RETAINED pump-hosted**, edited additively | Its six existing gate tests keep their current shape and their 60,000 ms `[Timeout(GateTimeoutMs)]`. Only new cases are added. |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | Support file, not a test | Instrumented and, under H-LEAK, repaired. Its acquire-at-build-start hold window and its restore-before-release ordering are invariants and must be preserved. |

### 7.2 Retained pump-hosted and unchanged

None of the following is edited. They keep the real message loop and their existing timeouts.

- The eight pump-hosted initialization tests in QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs.
- The two pump-hosted seam-factory tests in QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs.
- The one pump-hosted test in QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs. This
  file is at 498 of 500 lines; nothing may be added to it.
- The eight breadcrumb-host tests in QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs. Note the
  correct directory: these are under the Viewers folder of the test project, not the Controllers folder.
- The reflection contract tests in QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs,
  which are the guard that keeps the seam additive.

### 7.3 De-exemption coverage rule, inherited and binding

Every one of the pump-hosted consumer tests is the named coverage evidence for at least one de-exempted
production member. Deleting, `[Ignore]`-ing, or reclassifying any of them out of the unit suite
invalidates the corresponding `#230` justification comment and re-opens the exemption question. The
affected comment blocks are in the Initialization partial and at lines 272-275 of
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`; the line numbers recorded in the #511 spec
are pre-drift and must be re-derived before use.

### 7.4 Determinism rules for new tests

No `Thread.Sleep`, `Task.Delay`, `Stopwatch`, wall-clock comparison, retry loop or polling loop in any
new or modified test. The only permitted time-valued construct is a single `[Timeout(...)]` attribute
used solely as a deadlock bound, which is the established in-repo precedent. No temporary files.
MSTest, Moq and FluentAssertions only.

---

## 8. Evidence convention

Per the maintainer decision on issue #671 dated 2026-09-11, this item commits **projections only**.

- Every evidence artifact is a Markdown file under the feature folder's evidence directory, in one of
  the canonical kind subdirectories: baseline, regression-testing, qa-gates, issue-updates or other.
- Numeric coverage and run figures are transcribed **into** those Markdown artifacts. Raw tool output is
  read and then discarded.
- No `.trx` file and no `.cobertura.xml` file is added to the repository by this item. This matters
  doubly here because criterion 3 requires many repeated runs; dozens of raw result files must not be
  committed.
- No evidence is written to artifacts/baselines, artifacts/qa, artifacts/coverage or any other
  non-canonical location.

---

## Acceptance Criteria

Five criteria, consolidated from #592 and restated so each is falsifiable. All five must be satisfied.

- [x] **AC1 — Mechanism identified by measurement, not inference.** An evidence artifact under this
  feature folder's evidence/baseline directory names which of the two candidate mechanisms defined in section 4.2 —
  H-COST (raw elapsed fixture cost) or H-LEAK (a leaked or late-released `TransactionGate` permit) — is
  operative, and does so from a direct instrumented observation. The artifact PASSES only if it contains
  all four of: (i) the discriminating observable stated in advance, namely whether any acquisition of
  the one-permit `TransactionGate` finds the permit held with no live holder; (ii) the measured value of
  that observable, with the command and the load condition under which it was measured; (iii) the
  rejected hypothesis named explicitly together with the observation that rejects it; and (iv) the
  per-test elapsed durations that bound the contribution of construction cost. It FAILS if it reasons
  only from static reading, if it identifies the mechanism as `UiThreadDispatcherGate` or
  `SwapUiThreadDispatcher` (which exist in zero `.cs` files, per correction C1), or if it reports
  agreement with both hypotheses. If the instrumented run produces no expiry at all, that is a recorded
  negative result, not a pass.

- [x] **AC2 — Deterministic regression test, no sleep, no retry, no timing tolerance.** A named test,
  cited as file path plus test-method name, reproduces the mechanism identified under AC1. It PASSES
  only if all four hold: (i) it fails on the pre-change tree and passes on the post-change tree, in each
  of 3 consecutive pre-change runs and 3 consecutive post-change runs on the same machine in the same
  session, with the six outcomes transcribed into an artifact under this feature folder's
  evidence/regression-testing directory; (ii) a grep of the named test file for `Thread.Sleep`,
  `Task.Delay`, `Stopwatch`, `DateTime.Now`, `DateTime.UtcNow`, `Environment.TickCount` and `while`
  returns no match inside the new test bodies, with the grep output transcribed; (iii) the only
  time-valued construct in the test is a single `[Timeout(...)]` attribute serving as a deadlock bound;
  and (iv) the test asserts a structural property, not an elapsed duration.

- [x] **AC3 — Efficacy demonstrated, with the run-count scope named.** Two components, of which the
  first is blocking.
  **(a) BLOCKING, deterministic, single run.** The member named in AC2 completes when driven with the
  synchronous `IUiDispatcher` double and **no** `WinFormsPumpHost`, and constructs zero instances of the
  concrete `ItemViewer` type, asserted structurally in one run. A deterministic assertion has no base
  rate, so one run suffices and no statistics are required for this component.
  **(b) SUPPORTING, statistical.** At least 62 consecutive clean runs of the **targeted reproduction
  scope** — defined as `vstest.console.exe` over the QuickFiler test assembly filtered to the test
  classes named in AC2, under the load condition recorded in AC1 — with the aggregate outcome
  transcribed into an artifact under this feature folder's
  evidence/regression-testing directory. 62 is the smallest integer N satisfying
  (20/21)^N <= 0.05 given the recorded 1-in-21 base rate; 95 would be required at alpha 0.01; 30 clean
  runs alone has probability 0.23 under the null and establishes nothing. The 62-run count applies to
  the targeted scope **only** and explicitly does **not** apply to the full instrumented multi-assembly
  suite, for which 62 loaded runs projects to roughly 15 hours. If fewer than 62 runs are executed, the
  artifact must state the achieved N and its exact p-value (20/21)^N and must state plainly that the
  statistical claim is not established; the criterion then rests on component (a). The artifact must
  also record that the 4.8 % base rate is a point estimate from a single observed failure, with exact
  95 % interval approximately [0.0012, 0.2382].

- [x] **AC4 — Coverage of the two named controller partials retained or improved, against a named
  denominator and named tests.** The subject files are `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
  and the Initialization controller partial at QuickFiler/Controllers/QfcItemController.Initialization.cs,
  which is measured but not edited by this item. PASSES only if all five hold: (i) a **fresh** pre-change
  and post-change coverage measurement is taken in the **same session** with the **same command**, and
  both figures are transcribed into a Markdown artifact under this feature folder's
  evidence/qa-gates directory — the
  committed cross-session figures (0.950382 and 0.904762 in the 2026-09-08 report; 1.0 and 0.863014 in
  the 2026-09-06 report) **disagree** and neither may be used as the baseline; (ii) the denominator is
  stated explicitly as the set of lines Cobertura reports for those two filenames in the same session's
  pre-change run, and the artifact records that `InitializeWebViewAsync` is excluded by attribute at
  line 47 and therefore contributes nothing, so edits confined to it cannot move the figure; (iii)
  post-change line rate is greater than or equal to pre-change line rate for both files; (iv) if the
  denominator line count differs between the two runs, the artifact states the delta and accounts for
  it, because extracting logic out of the excluded method changes the denominator and can move the rate
  in either direction for reasons unrelated to test quality; and (v) each test named in the section 7
  disposition table exists in the post-change tree and passes. This criterion is **not** phrased over
  `ItemViewer` coverage, which is unmeasurable: its type-level exclusion at line 20 of
  `QuickFiler/Viewers/ItemViewer.cs` means the type emits no Cobertura element at all.

- [x] **AC5 — #511 and #571 reconciled.** This is predominantly a verification plus a forward-pointer
  update; the existing closing comments are **not** wrong and must not be described as such. PASSES only
  if all three hold: (i) an artifact under this feature folder's evidence/issue-updates directory
  records, quoting each,
  that both issues already carry a premise correction refuting the window-handle cause, and confirms
  that the refutation's cited mechanics still hold against the current tree; (ii) one comment is posted
  on each issue replacing the now-stale forward pointer to #592 — closed NOT_PLANNED on 2026-09-11 and
  consolidated into #743 — with a pointer to #743 and its resolution; and (iii) that same comment marks
  the `UiThreadDispatcherGate` / `SwapUiThreadDispatcher` hypothesis restated in both closing comments as
  superseded by the mechanism identified under AC1, citing correction C1. The two comment URLs are
  recorded in the artifact. No claim that the window-handle refutation was in error is permitted.

---

## Write Set

Every file the implementation diff will create, modify or delete. One repository-relative path per line.

- `QuickFiler/Viewers/IItemViewer.cs`
- `QuickFiler/Viewers/ItemViewer.cs`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`

Notes on the set, which is deliberately as narrow as the evidence permits:

- `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` is a new file. Both projects
  in this repository are legacy non-SDK MSBuild projects with fully enumerated `<Compile Include>`
  lists, so adding it **requires** an edit to `QuickFiler.Test/QuickFiler.Test.csproj`. The entry takes
  the plain shape used by the other controller test entries. An omitted entry silently excludes the file
  from the build, and the tests in it would then not exist.
- The QuickFiler production project file is **not** in the set. The production change adds members to existing
  files and creates no new production `.cs` file, so the production project's item list does not change.
  If planning later concludes that a new production partial is needed, the production project file must
  be added to this set before that file is created, and the new entry must carry a `DependentUpon`
  child in the shape the other viewer partials use.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is known to be concurrently edited by a
  sibling item in the same parallel run, and has only 33 lines of headroom against the 500-line cap.
  The WebView2 breadcrumb host, also concurrently edited by a sibling, is deliberately absent from this
  set.

---

## 10. Risks and mitigations

| Risk | Mitigation |
|---|---|
| The synchronization-context marshal and the WPF-dispatcher marshal are different queues on the same thread, so Part A could change UI-thread ordering | Establish equivalence for each converted site before the edit, not after. Production behaviour change is out of scope and would be a defect. |
| `_uiDispatcher` is observed null in at least one existing test path | Every converted site carries the same null tolerance the existing sites carry. |
| Part B overlaps issue #489's assigned scope | Confirm #489's disposition at Phase 0. If open, coordinate or defer Part B and ship Part A plus the mechanism work. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is contended and near the size cap | Keep the diff minimal; if the file would exceed 500 lines, extract to a new partial and add the production project file to the write set first. |
| The instrumented run reproduces no expiry, leaving AC1 unresolved | A recorded negative result is an honest outcome, not a pass. Escalate rather than infer. |
| The seam is used to delete pump-hosted coverage | Non-goal 2, the section 7 disposition table, and AC4 are three independent guards on this. |
| 62 targeted runs proves infeasible | AC3(a) is the blocking component and needs one run. AC3(b) degrades to a recorded partial signal with its p-value stated. |

---

## 11. Source artifacts

All under this feature folder, docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743.
They are documentation inputs, not implementation targets, and are therefore listed as plain text.

- research/2026-09-12T14-30-itemviewer-ui-marshalling-seam-research.md
- evidence/other/issue-reconciliation-and-gh-context.2026-09-12T13-40.md
- evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md
- evidence/other/orchestrator-gate-contention-mechanism.2026-09-12T14-05.md
- evidence/other/orchestrator-run-count-derivation.2026-09-12T14-15.md
- evidence/other/orchestrator-seam-design-constraints.2026-09-12T14-25.md
- evidence/other/orchestrator-constraint-conflict.2026-09-12T14-45.md

The consolidation note carrying #592's criteria, and the orchestrator verification note recording
corrections C1 and C4, are in the issue record for this feature folder.

---

## 12. Toolchain

The full C# toolchain runs in the order defined by CLAUDE.md: CSharpier format, then the analyzer
rebuild, then the nullable rebuild, then `vstest.console.exe` with coverage. Any step that fails or
auto-fixes restarts the loop from the first step. Nothing in this item is complete until one pass
completes with all four steps clean.
