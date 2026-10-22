# Issue #743 — QuickFiler `ItemViewer` UI-marshalling seam: research

- **Issue:** #743 (bug, work mode full-bug)
- **Date:** 2026-09-12T14-30
- **Worktree:** agent-a190dd2fffe21a25d (branch `worktree-agent-ae73e8a4777540363`)
- **Mode:** research only. No production file, configuration, or project file was modified. No
  `msbuild`, `csharpier`, `dotnet-coverage`, or `vstest` invocation was made.

## 0. Tooling limitation that constrains this artifact (stated first, because it bounds every claim)

**The `Bash` tool is disabled in this session**, in this agent and in any subagent
(`Error: No such tool available: Bash. Bash is disabled for this session, in subagents as well as
here.`). Consequently:

- `gh issue view 743`, `gh issue view 592`, `gh issue view 511`, `gh issue view 571` and the
  `--comments` variants **could not be executed**. The 2026-09-11 consolidation comment on #743 that
  carries the #592 acceptance criteria **was not read**, and the closing comments of #511 and #571
  **were not read from GitHub**.
- `git log` / `git blame` were unavailable, so line-number drift is reported as an observed
  difference against the cited values, not as an attributed commit.

Everything below is derived from on-disk repository state plus one authoritative external source
(Microsoft Learn, §5.2). Where a question could only be answered from GitHub, it is recorded as an
explicit unknown rather than guessed.

In-repo substitutes that were read in full instead:

- `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/issue.md` (the
  promoted body of #743)
- `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md`
  §4 and §5 (N5)
- `docs/features/active/2026-08-21-winformspumphost-suite-determinism-511/spec.md` — the #511/#571
  feature spec, which records the disposition of both issues (this is the closest in-repo
  equivalent of their closing comments)
- `docs/features/epics/quickfiler-suite-determinism-foundation/epic-status.md` — the halt record for
  #511 and the origin of the 1-in-21 failure-rate figure
- `docs/features/potential/promoted/2026-08-31-quickfiler-pump-host-tests-load-sensitive-under-coverage.md`
  (#711)

---

## 1. Findings summary

| # | Finding | Verdict |
|---|---|---|
| F1 | `UiThreadDispatcherGate` and `SwapUiThreadDispatcher` exist in **zero** `.cs` files. They were removed by issue #493 and replaced by `UiThreadDispatcherFixture` / `UiThreadDispatcherTransaction`. | Maintainer's lead **falsified as literally stated** |
| F2 | The surviving `TransactionGate` **does** serialize, and blocking on it **does** burn the blocked test's own `[Timeout]` budget. Measured at ≈4.1 s of pure blocking in a committed TRX. | Mechanism **real but small** |
| F3 | Gate contention cannot be the dominant cause: **9 of the 19** `[Timeout(PumpTimeoutMs)]` tests never touch the gate, and 8 of those 9 are the `WebView2BreadcrumbHostTests` that #743 names in its own summary. | Lead **refined, not confirmed** |
| F4 | Gate contention requires class-level parallelization. CI passes **no** `/Settings:`, the assembly declares no `[assembly: Parallelize]`, and #743's own stated repro command passes no `/Settings:` either. | Lead **inapplicable under #743's stated repro** |
| F5 | The strongest surviving form of the lead is a **gate leak, not gate contention**: MSTest's default non-cooperative `[Timeout]` stops observing a timed-out method, so a gate-holding test that expires may never run its `finally`. `#511 spec.md:132-139` already recorded exactly this and left it unfixed. | **Open, untested, worth an AC** |
| F6 | The dominant cost is the real `ItemViewer` construction: `ItemViewer.Designer.cs` is **6223 lines** and `InitializeComponent` instantiates 110 controls including two `WebView2` children whose `EndInit` creates the handles. First-in-class pump tests measure 3.3 s and 6.2 s on an idle 24-worker box; siblings measure 0.18–0.43 s. | **Confirmed (b)** |
| F7 | An `IUiDispatcher`-style marshalling seam **alone does not remove that cost**, because `QfcItemController` reaches the viewer through 14 `(ItemViewer)_itemViewer` concrete casts and one `ResolveControlGroupsAsync(ItemViewer)` signature. | **Central design constraint** |
| F8 | `QuickFiler/Interfaces/IItemViewer.cs` does not exist. The file is `QuickFiler/Viewers/IItemViewer.cs`. | Citation **corrected** |
| F9 | `QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]`, and the type is therefore **absent** from committed Cobertura, not reported at 0%. | **Confirmed** |
| F10 | The #729 census of `PumpTimeoutMs` (4 declarations, 19 usages, all `[Timeout]`) **still holds exactly**, at the same line numbers. | **Re-derived, unchanged** |
| F11 | The AC-3 arithmetic checks out: 0.952381^30 = 0.2314. The run count for α = 0.05 is **N = 62**; for α = 0.01 it is **N = 95**. | **Verified, with a caveat on the base rate** |

---

## 2. Verdict on the maintainer's first lead

> "the `UiThreadDispatcherGate` / `SwapUiThreadDispatcher` reflection swap of the `UtilitiesCS.UiThread`
> private `_dispatcher` field, shared by two test classes."

**Verdict: falsified as stated; refined into two separable mechanisms, one small and one untested.**

### 2.1 The named symbols do not exist

A repository-wide grep for `UiThreadDispatcherGate|SwapUiThreadDispatcher` returns hits in **`docs/`
and `.claude/agent-memory/` only** — zero `.cs` files. The most recent code-bearing references are
the *removal* records:

- `docs/features/active/2026-08-24-quickfiler-test-uithread-dispatcher-493/policy-audit.2026-08-27T15-07.md:129`
  — "private `UiThreadDispatcherGate` and `SwapUiThreadDispatcher` removed; `BuildPumpHarnessAsync`/`PumpHarness`
  now consume the shared transaction, preserving the acquire-at-build-start hold window and
  restore-before-release ordering."
- `docs/features/active/2026-08-24-quickfiler-test-uithread-dispatcher-493/evidence/qa-gates/duplicate-swap-removal.2026-08-27T11-36.md:7`

### 2.2 What replaced them, and under which issue

`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (278 lines). Its own
doc comment names the owning issue:

- `:12-13` — "Single owner of every mutation of the process-wide static `UtilitiesCS.UiThread._dispatcher`
  made from this test assembly's owned files (**issue #493**)."

**Correction to the delegation prompt:** the fixture is issue **#493**, not #648. #648 is the issue
that *added a consumer* — `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:38-45` states
"Issue #648: the swap of the process-wide static `UtilitiesCS.UiThread._dispatcher` is routed
through `UiThreadDispatcherFixture` ... rather than performed by raw reflection here."

Structure (all line numbers current):

| Member | Line | Behaviour |
|---|---|---|
| `FieldLock` (`object`) | `:31` | Guards one straight-line read-modify-write of the static |
| `TransactionGate` (`SemaphoreSlim(1,1)`) | `:32` | Mutual exclusion across whole install-to-restore transactions |
| `DispatcherField` | `:34`, resolved at `:133-141` | `typeof(UiThread).GetField("_dispatcher", NonPublic\|Static)` |
| `Exchange` | `:55-63` | Atomic read-then-write |
| `CompareExchange` | `:70-82` | Restores only if the static still holds the expected instance |
| `BeginTransactionAsync` | `:122-126` | `await TransactionGate.WaitAsync()` then returns an un-installed transaction |
| `ReleaseTransactionGate` | `:88-91` | Called only from `UiThreadDispatcherTransaction.Dispose` |
| `UiThreadDispatcherTransaction.Dispose` | `:261-276` | `CompareExchange` restore **then** `ReleaseTransactionGate`, in that order |

`:19-20` records the lock ordering invariant: "Lock ordering is `TransactionGate` then `FieldLock`,
never the reverse, so no cycle and therefore no deadlock exists."

### 2.3 Did #493 close the contention the lead describes?

**No — #493 changed the owner of the serialization, not its shape.** The pre-#493 gate was a
`private static SemaphoreSlim(1,1)` held from fixture build to restore; the post-#493 gate is a
`private static readonly SemaphoreSlim(1,1)` (`:32`) held from `BeginTransactionAsync` to
`UiThreadDispatcherTransaction.Dispose`. `BuildPumpHarnessAsync` still acquires at build start
(`QfcItemController.InitializationTests.Part2.cs:51-55`, with the in-file comment "Held until
`PumpHarness.Restore`, so only one pump fixture owns the static `UiThread.Dispatcher` at a time
across all test classes in this assembly").

What #493 **did** fix is correctness — the clobber hazard (`CompareExchange` skips a restore that
would overwrite a newer owner) and the over-release hazard (idempotent `Dispose`). It did not and
could not fix the fact that a blocked waiter consumes wall-clock time.

### 2.4 Why the lead nevertheless cannot be the dominant cause

Three independent reasons, each separately sufficient:

**(a) Most of the failing population never takes the gate.** Of the 19 `[Timeout(PumpTimeoutMs)]`
tests, exactly 10 reach `TransactionGate` and 9 do not (§3.2). The 9 are the eight
`WebView2BreadcrumbHostTests` plus `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`.
#743's own Summary names "`QfcItemController`/`WebView2BreadcrumbHost` tests" as the failing set, so
the failing set demonstrably includes tests the gate cannot reach.

**(b) Contention requires parallelization that the stated repro does not enable.**
`QuickFiler.Test/Properties/AssemblyInfo.cs` (21 lines) declares no `Parallelize` attribute. CI runs
`& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
(`.github/workflows/_mstest-coverage.yml:99`) with no `/Settings:`. #743's own stated command
(`issue.md:22`) likewise carries no `/Settings:`. Under those conditions MSTest runs the assembly
serially and the gate is never contended. Note the discrepancy: **#711's** repro command
(`.../2026-08-31-quickfiler-pump-host-tests-load-sensitive-under-coverage.md:20`) *did* include
`/Settings:TaskMaster.runsettings`, and `TaskMaster.runsettings:4-7` declares
`<Workers>0</Workers><Scope>ClassLevel</Scope>`. The two repro commands are not the same experiment.

**(c) The measured magnitude is an order of magnitude too small.** See §4.2: ≈4.1 s of blocking
against a 60,000 ms bound.

### 2.5 The surviving, untested form of the lead: a leak, not contention

`docs/features/active/2026-08-21-winformspumphost-suite-determinism-511/spec.md:132-139` states it
precisely, and explicitly leaves it unfixed:

> "**The MSTest `[Timeout]` / `UiThreadDispatcherGate` cascade is not fixed here.** The research
> identifies a second, independent load amplifier: MSTest's `[Timeout]` on a `Task`-returning test
> records a failure without aborting the continuation, so a timed-out pump test has not yet run its
> `finally` and therefore has not released the process-wide `UiThreadDispatcherGate` semaphore or
> reverted `UtilitiesCS.UiThread._dispatcher`."

That statement survives #493 verbatim with `TransactionGate` substituted for `UiThreadDispatcherGate`,
because the release still happens only inside the test's `finally`
(`QfcItemController.InitializationTests.Part3.cs:63-71` → `harness.Restore()` →
`Part2.cs:317-330` → `_transaction.Dispose()` → `UiThreadDispatcherFixture.cs:275`).

The authoritative MSTest semantics are given in §5.2. The consequence, if the leak occurs:

- The 10 gate-taking tests with `[Timeout(PumpTimeoutMs)]` and the 7 with `[Timeout(GateTimeoutMs)]`
  each convert into a bounded 60 s failure — a **cascade** from one expiry to up to 20 more.
- The **4 gate-taking tests that carry no `[Timeout]` at all** convert into an unbounded hang:
  `QfcFormControllerUndoHandoffTests` at `:227-228`, `:278-279`, `:334-335` and
  `QfcHomeControllerRunAsyncTests` at `:324-325`.

**Counter-evidence worth recording:** no CI hang of that shape has been reported, and under CI's
serial ordering a leak would reliably hang one of those four un-timed tests. That is weak evidence
that the leak is not occurring in CI, but it is not proof, because ordering is not pinned.

---

## 3. Citation verification against the current tree

All line numbers below were re-derived in this worktree on 2026-09-12.

### 3.1 Cited positions

| Cited (2026-09-11) | Current | Status |
|---|---|---|
| `QuickFiler/Viewers/ItemViewer.cs:23-29` (constructor) | `:23-29` — `public ItemViewer() { InitializeComponent(); _context = SynchronizationContext.Current; _uiDispatcher = Dispatcher.CurrentDispatcher; InitControlGroups(); }` | **Exact** |
| `QuickFiler/Viewers/ItemViewer.cs:59-62` (`UiSyncContext`) | `:58-62` — field `private SynchronizationContext _context;` at `:58`, property `:59-62` | **Exact for the property** |
| `QuickFiler/Interfaces/IItemViewer.cs:37` | **File does not exist.** `QuickFiler/Interfaces/` holds 15 files, none named `IItemViewer.cs`. The declaration is at `QuickFiler/Viewers/IItemViewer.cs:37` — `SynchronizationContext UiSyncContext { get; }` | **Path corrected, line exact** |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64` | `:64` — `await _itemViewer.UiSyncContext;` | **Exact** |
| `...ViewerSetup.cs:320` | `:287` — `await itemViewer.UiSyncContext;` | **Drifted −33** |
| `...ViewerSetup.cs:331` | `:298` — `.SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, Token))` | **Drifted −33** |
| `...ViewerSetup.cs:336` | `:303` — same shape, second collection | **Drifted −33** |
| *(not cited)* | `:282` — `_itemViewer.UiSyncContext,` as an argument to `QfcTipsDetails.CreateAsync` | **Omitted from the original citation** |

`QfcItemController.ViewerSetup.cs` is now **467 lines** (closing brace at `:467`), i.e. 33 lines of
headroom against the 500-line cap. The uniform −33 drift and the file's current length are
consistent with a 33-line net deletion since the citation was taken, but **no commit was identified**
(no `git` available).

Also verified: `ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]`; `ItemViewer.cs:64-68` exposes
`public Dispatcher UiDispatcher { get; }`; `IItemViewer.cs:36` declares `Dispatcher UiDispatcher { get; }`;
`IItemViewer.cs:192-194` declares `bool InvokeRequired`, `object Invoke(Delegate)`,
`IAsyncResult BeginInvoke(Delegate)` (the epic cited these at `:135-137` and `:95-100`; both are
drifted).

### 3.2 Re-derivation of the #729 `PumpTimeoutMs` census

`Grep pattern="PumpTimeoutMs" path="QuickFiler.Test"` returns **23 lines: 4 declarations + 19
usages**, every usage of the literal form `[Timeout(PumpTimeoutMs)]`, at exactly the line numbers
#729 recorded.

Declarations:

| File | Line | Modifier |
|---|---|---|
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` | `:25` | `private const int PumpTimeoutMs = 60000;` |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | `:38` | `internal const int PumpTimeoutMs = 60000;` |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | `:327` | `private const` |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | `:34` | `private const` |

Usages, with the gate classification this artifact adds:

| File | Lines | Count | Reaches `TransactionGate`? |
|---|---|---|---|
| `Viewers/WebView2BreadcrumbHostTests.cs` | 32, 82, 135, 181, 226, 257, 302, 348 | 8 | **No** |
| `Controllers/QfcItemController.InitializationTests.Part3.cs` | 39, 82, 130, 174, 244, 352, 400, 455 | 8 | Yes (via `BuildPumpHarnessAsync`) |
| `Controllers/QfcItemController.SeamFactoryTests.cs` | 338, 409 | 2 | Yes (via `BuildPumpHarnessAsync`) |
| `Controllers/QfcItemController.ViewerSetupTests.cs` | 425 | 1 | **No** (builds its own host at `:429`, no transaction) |

**Both #729 counts still hold: 4 declarations, 19 usages, 0 in a wait/poll position.** The new
information is the 10/9 split.

---

## 4. What the dominant cost actually is

### 4.1 The construction cost, structurally

- `QuickFiler/Viewers/ItemViewer.Designer.cs` is **6223 lines** (closing brace at `:6223`).
- It contains **110** occurrences of `new System.Windows.Forms.*` / `new Microsoft.Web.WebView2.WinForms.WebView2` /
  `ISupportInitialize` (single count over the file).
- Two `WebView2` children are constructed at `:46` (`_l0vhBreadcrumb_WebView2`) and `:49`
  (`_l0v2h2_WebView2`); their `BeginInit()` calls are at `:89-90` and their `EndInit()` calls at
  `:6165-6166`.
- `ItemViewer()` calls `InitializeComponent()` unconditionally (`ItemViewer.cs:25`).

The measured consequence is already recorded in-repo and was re-read verbatim:
`QfcItemController.InitializationTests.Part2.cs:77-83` — "#571 (measured 2026-08-22): both WebView2
children — and therefore the parent ItemViewer — are already handle-created when construction
returns, because InitializeComponent runs the Designer-emitted `ISupportInitialize.EndInit()` calls
on both children and WinForms creates a parent's handle when a child's is created."

The same measurement is the reason issue #511 was halted:
`docs/features/epics/quickfiler-suite-determinism-foundation/epic-status.md:151` — "The remedy is a
measured no-op."

### 4.2 The construction cost, measured from committed TRX evidence

Two committed TRX files record per-test durations for the pump-hosted tests. Both ran with
**`Test Parallelization enabled ... (Workers: 24, Scope: ClassLevel)`**, so both are class-parallel
runs on an otherwise-idle machine.

Sample A — `docs/features/active/2026-08-31-efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/qa-gates/p2-t7/quickfiler-postchange.trx`
(`ResultSummary` at `:8836-8839`: `total="1287" ... failed="0" ... timeout="0"`):

| Test | Duration | Window | Gate |
|---|---|---|---|
| `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` | **00:00:06.223** | 16:24:07.221 → 16:24:13.445 | holds |
| `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` | **00:00:04.726** | 16:24:09.350 → 16:24:14.076 | waits |
| `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` | 00:00:01.929 | 16:24:11.504 → 16:24:13.434 | none |
| `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` | 00:00:01.039 | 16:24:13.446 → 16:24:14.485 | holds |
| `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing` | 00:00:00.671 | 16:24:14.076 → 16:24:14.747 | holds |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | 00:00:00.428 | 16:24:14.486 → 16:24:14.914 | holds |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | 00:00:00.212 | | holds |
| `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` | 00:00:00.185 | | holds |
| `NavigateToString_PostsExactlyOnceToTheUiContext` (WebView2 host) | 00:00:00.160 | | none |
| `InitializeAsync_InstallsUiDispatcherFromUiSyncContext` (WebView2 host) | 00:00:00.018 | | none |

Sample B — `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p4-t3/p4-t3-quickfiler-test-whole-assembly.trx`
(`:8423`: `total="1218" ... failed="0" ... timeout="0"`; `:8425` same 24-worker banner):
`InitializeSequentialAsync` 00:00:03.304, `CreateSequentialAsync` 00:00:03.983,
`ResolveControlGroupsAsync` 00:00:00.522.

**Two conclusions follow directly from the timestamps in Sample A.**

1. **Gate blocking is real and quantifiable.** `CreateSequentialAsync` started at 16:24:09.350 and
   ended at 16:24:14.076, but `InitializeSequentialAsync` held the gate until 16:24:13.445. So
   ≈**4.10 s** of `CreateSequentialAsync`'s 4.73 s duration was spent blocked on `TransactionGate`
   and ≈0.63 s was work. This confirms that gate blocking is charged to the blocked test's own
   `[Timeout]` budget. It is **6.8 %** of the 60,000 ms bound.
2. **First-touch cost dominates the rest.** Within a class the first pump test costs 3.3–6.2 s and
   its siblings cost 0.19–1.04 s — a 10× to 30× ratio. That gap is JIT, assembly load, WinForms and
   WebView2 type initialization, and the first `ItemViewer` Designer walk. It is not separable into
   its components from the available evidence and is **not** claimed to be so here.

### 4.3 Bridging 6 s to 60 s

`docs/features/active/2026-08-21-winformspumphost-suite-determinism-511/evidence/regression-testing/determinism-ten-runs.2026-08-21T18-10.md:76-79`
records the measured load multiplier: ten full-suite runs under sustained 100 % CPU saturation ran
"at **6x to 26x** the unloaded run duration". Applying the upper measured multiplier to the largest
measured pump test: 26 × 6.223 s ≈ **162 s**, which exceeds the 60,000 ms bound by ≈2.7×. Applying
the lower: 6 × 6.223 s ≈ 37 s, which does not. **The 60 s bound sits inside the measured load-multiplier
band**, which is exactly the signature of an intermittent, load-correlated expiry. This is the
quantitative link between finding F6 and the reported symptom, and it is stated no more strongly
than that: the multiplier was measured suite-wide, not per test.

### 4.4 Answer to "(a), (b), or something else"

**(b), with (a) as a secondary amplifier and a first-touch/instrumentation co-factor that cannot be
separated from (b) on the present evidence.**

- (a) lock convoy on `TransactionGate`: **real, measured at ≈4.1 s**, reaches only 10 of the 19
  tests, and only when class-level parallelization is enabled. Cannot alone produce a 60 s expiry.
- (b) real elapsed construction of a WinForms control tree with handle-created `WebView2` children:
  **dominant**. 6223-line Designer, 110 control constructions, two `WebView2` handles, measured
  3.3–6.2 s idle, inside the measured 6×–26× load band relative to the 60 s bound.
- something else: per-class first-touch (JIT + type init) and `/EnableCodeCoverage` instrumentation
  overhead. Both are real (the 10×–30× first-vs-sibling ratio proves first-touch is large) but
  neither was isolated by any measurement in this repository.

### 4.5 Honest unknowns

1. **The identity of the seven expiring tests is not recoverable.** The only recorded expiry event
   is described at `.../511/evidence/regression-testing/determinism-ten-runs.2026-08-21T18-10.md:81-85`:
   "the second P0-T16 coverage invocation reported 6430 / 6437 with seven 60,000 ms `PumpTimeoutMs`
   expiries including both named tests. Its distinguishing condition was 17 idle MSBuild node-reuse
   processes." The underlying TRX is **not in this worktree** — #511 never merged (work preserved on
   `bug/winformspumphost-suite-determinism-511-exec` at `53a2a08f`, per
   `epic-status.md:149`). A repository-wide grep for `exceeded execution timeout period`,
   `Test timed out`, and `timeout="[1-9]` across `docs/` returns **zero matches**, so **there is no
   committed artifact of an actual expiry anywhere in this tree.**
2. **#711's reported "fourteen" failing tests vs. #511's "seven" expiries** are two different
   observations on two different days under two different commands, and cannot be reconciled from
   in-repo evidence.
3. Whether the gate leak of §2.5 has ever actually occurred is **unknown and untested**.

---

## 5. Secondary mechanisms verified

### 5.1 Gate-contention census

**21 test methods across 6 `[TestClass]` types acquire `TransactionGate`.** Full derivation in §8/N2.

| `[TestClass]` | File | Tests | Timeout attribute |
|---|---|---|---|
| `QfcItemController_InitializationTests` (`:30`, partial over 3 files) | `Controllers/QfcItemController.InitializationTests{,.Part2,.Part3}.cs` | 8 | `[Timeout(PumpTimeoutMs)]` = 60000 |
| `QfcItemController_SeamFactoryTests` (`:27`) | `Controllers/QfcItemController.SeamFactoryTests.cs` | 2 | `[Timeout(PumpTimeoutMs)]` = 60000 |
| `QfcItemController_UiThreadDispatcherFixtureTests` (`:31`) | `Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | 6 | `[Timeout(GateTimeoutMs)]` = 60000 (`:33`) |
| `QfcFormControllerUndoHandoffTests` (`:29`) | `Controllers/QfcFormControllerUndoHandoffTests.cs` | 3 | **none** |
| `QfcHomeControllerRunAsyncTests` (`:24`) | `Controllers/QfcHomeControllerRunAsyncTests.cs` | 1 | **none** |
| `WpfUiDispatcherTests` (`:19`) | `Controllers/WpfUiDispatcherTests.cs` | 1 | `[Timeout(GateTimeoutMs)]` |

The epic's premise that the gate is "shared by two test classes"
(`epic.md:92-98`) is **out of date by a factor of three**: it is now six classes and 21 tests.

### 5.2 MSTest `[Timeout]` semantics — authoritative

`QuickFiler.Test/packages.config:123-124` pins `MSTest.TestAdapter` and `MSTest.TestFramework` at
**4.4.0**. The repository has **no** `testconfig.json` and no `CooperativeCancellation*` entry in any
`.runsettings` (grep returns zero matches repo-wide).

Microsoft Learn, *Configure MSTest* (`https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-configure`,
`ms.date: 2026-09-02`), `mstest.timeout` table, entry `useCooperativeCancellation`, default **`false`**:

> "When set to `true`, in case of timeout, MSTest will only trigger cancellation of the
> `CancellationToken` but will **not stop observing the method**. This behavior is more performant but
> relies on the user to correctly flow the token through all paths."

At the repository's configuration (the default, `false`), MSTest therefore **does** stop observing a
timed-out method. For an `async Task` test method there is no mechanism to abort a suspended state
machine, so "stop observing" means the remainder of the method — including its `finally` — is not
awaited by the adapter. That is the mechanical basis of §2.5.

**Not verified:** whether, in practice, the abandoned continuation still resumes later (releasing the
gate late rather than never). Determining that requires either testfx source inspection or an
experiment, neither of which was in scope for a preparation-only run.

### 5.3 `WinFormsPumpHost` contains no timeout of its own

`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (483 lines) was read in full. Every wait is
untimed: `_ready.Wait()` at `:60`, `await _stopped.Task` at `:263`, `_thread.Join()` at `:264`, and
all completions are `new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously)`
at `:364-365`. There is no `Thread.Sleep`, `Task.Delay`, `Stopwatch`, or poll anywhere in the file.
**`[Timeout]` is the only bound on any pump-hosted test.** This corroborates #729 §4.1 independently.

### 5.4 `WebView2BreadcrumbHost` already takes injected collaborators

#743's `Proposed Fix` bullet 2 ("Introduce an interface over the `WebView2` control that
`WebView2BreadcrumbHost` accepts via constructor injection") is **partly already satisfied and partly
mis-specified**. `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`:

- `:70-71` — `public WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer)`
- `:88-92` — `internal WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer, BreadcrumbUiDispatcher? dispatcher)`

The control **is** constructor-injected today; what is concrete is its **type**. The tests already
exploit the internal overload: `WebView2BreadcrumbHostTests.cs:45-52` passes a recording dispatcher.
What forces a real `WebView2` is not the constructor parameter shape but three hard dependencies on
the concrete type inside the constructor body:

- `:46-47` — `ConditionalWeakTable<WebView2, WebView2BreadcrumbHost> _owners` (keyed on the control)
- `:112` — `_control.CoreWebView2InitializationCompleted += OnCoreInitializationCompleted;`
- `:113` — `_control.Disposed += OnControlDisposed;`

An interface over the control must therefore expose those two events and be usable as a
`ConditionalWeakTable` key. That is a materially larger change than "accept an interface".

---

## 6. Coverage facts

### 6.1 `[ExcludeFromCodeCoverage]` makes `ItemViewer` invisible, not 0 %

`QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on the `partial class`
declaration. The attribute is **type-scoped**, so it covers every partial of the type, including the
6223-line `ItemViewer.Designer.cs`.

Direct confirmation from committed evidence
(`docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/coverage-postchange.cobertura.xml`):
a grep for `filename="QuickFiler\Viewers\ItemViewer` matches exactly **two** lines, `:4797` and
`:6039`, and both are `ItemViewerExpanded` (`ItemViewerExpanded.Designer.cs` and
`ItemViewerExpanded.cs`), which carries no exemption. **There is no `<class>` element whose filename
is `QuickFiler\Viewers\ItemViewer.cs` or `QuickFiler\Viewers\ItemViewer.Designer.cs` at all.**

**Consequence for acceptance criteria.** Any AC phrased as "`ItemViewer` coverage must be ≥ X %" or
"must not decrease" is **unmeasurable as written and will be scored INCOMPLETE**, because the type
contributes no numerator, no denominator, and no element to the report. Three admissible
reformulations:

1. Phrase the AC over `QfcItemController.*.cs` (which *is* measured) rather than over `ItemViewer`.
2. Phrase it over a **new, non-exempt** type that the seam introduces (e.g. a marshalling adapter in
   its own file with no `[ExcludeFromCodeCoverage]`).
3. If the AC genuinely intends to de-exempt `ItemViewer`, state that removal of the attribute at
   `ItemViewer.cs:20` is itself the deliverable, and expect the Designer partial's ~6200 lines to
   enter the denominator. That is a large, deliberate coverage-metric event and must be costed, not
   discovered.

Incidental data point for (3): `ItemViewerExpanded.Designer.cs` reports **line-rate 0.99506** in the
same report, i.e. a Designer partial that *is* measured scores near-100 % because the tests construct
the control. So de-exempting would likely *raise* the reported rate — but it would also make the
whole 6223-line Designer a coverage-relevant surface, and that consequence should be stated
explicitly rather than left to be observed.

### 6.2 Measured coverage of the two named controller partials

Committed coverage evidence **does** exist; no coverage tool was run for this artifact. Most recent
committed Cobertura found:
`docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/coverage-postchange.cobertura.xml`.

| File | `<class>` line | line-rate | branch-rate | complexity |
|---|---|---|---|---|
| `QuickFiler\Controllers\QfcItemController.Initialization.cs` | `:36340` | **0.950382** | 0.90625 | 35 |
| `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` | `:37064` | **0.904762** | 0.825 | 85 |

Caveats, stated because they change how these numbers may be used:

- These are the top-level `QuickFiler.Controllers.QfcItemController` class elements for each
  filename. Each file *also* emits additional class elements for compiler-generated closures and
  async state machines (e.g. `...&lt;&gt;c__DisplayClass132_0`, `...&lt;AssignControlsAsync&gt;d__137`),
  which carry their own rates. A file-level figure is not the same as this one class element's rate.
- A second, older committed report
  (`.../2026-09-06-...-798/evidence/qa-gates/coverage-final.cobertura.xml:371950,:372411`) gives
  `1.0` and `0.863014` for the same two files. **The two committed reports disagree.** Any baseline
  for #743 must be measured fresh rather than lifted from either.

---

## 7. Design options for the seam

### 7.1 The inherited constraints, restated exactly

1. **No timing tolerance.** `.claude/rules/csharp.md` (cited at `epic.md:117`) prohibits "adding
   sleeps, retries, or timing hacks to mask flaky behavior". This rules out raising `PumpTimeoutMs`,
   scaling it by core count, adding retries, or `[Ignore]`.
2. **No fake `SynchronizationContext` replacing the real message pump.** Recorded as an explicit
   non-goal at `#511 spec.md:121-123` and `epic.md:78-79`.
3. **Coverage of the de-exempted members must not be lost.** `#511 spec.md:265-267`: "Every one of
   the eight pump-hosted consumer tests is the named coverage evidence for at least one de-exempted
   production member. Deleting, `[Ignore]`-ing, or reclassifying any of them out of the unit suite
   invalidates the corresponding comment and re-opens the exemption question." The affected
   justification blocks are `QfcItemController.Initialization.cs` at 135, 164, 196, 259, 291, 403,
   447 and `QfcItemController.ViewerSetup.cs` at 254 (`#511 spec.md:246-258`; those line numbers are
   themselves pre-drift and must be re-derived before use).
4. Production edits **are** permitted for this item.

Constraint 2 is the one that most constrains the design. It does **not** prohibit an injected
*marshalling abstraction* — `UtilitiesCS.Threading.IUiDispatcher` already exists, is already a
production constructor parameter, and is already mocked by these very tests. It prohibits
substituting a counterfeit `SynchronizationContext` for the Win32 loop.

### 7.2 The production surface that bypasses the existing seam

`QfcItemController` marshals through **three** distinct channels. Only the first is injectable.

| Channel | Type | Sites |
|---|---|---|
| `IUiDispatcher _uiDispatcher` | `UtilitiesCS.Threading.IUiDispatcher` (`QfcItemController.cs:66`) | ~20 sites; injected at `Initialization.cs:59`, `:438`, `:480`; defaulted `??= new WpfUiDispatcher()` at `:391` |
| `_itemViewer.UiSyncContext` | raw `SynchronizationContext` | `ViewerSetup.cs:64, 282, 287, 298, 303` — **5** |
| `_itemViewer.UiDispatcher` | raw WPF `Dispatcher` | `Initialization.cs:200`, `FolderHandling.cs:188`, `ViewerSetup.cs:371` — **3** |

**8 production sites in `QfcItemController` bypass the existing seam.** That is the precise surface
#743 is about. (`EfcItemController.cs:191, 846, 855, 1104` have the same shape but are a different
controller and out of scope.)

### 7.3 The blocker that no marshalling seam removes

`QfcItemController` reaches the viewer through **14** `(ItemViewer)_itemViewer` concrete casts plus
one concrete signature:

- `Initialization.cs:179, 184, 216, 220, 274, 279, 308, 312`
- `EventWiring.cs:37, 50, 313, 318, 327, 332`
- `ViewerSetup.cs:75, 85, 114` (inside the already-exempt `InitializeWebViewAsync`)
- `ViewerSetup.cs:276` — `internal async Task ResolveControlGroupsAsync(ItemViewer itemViewer)`

and at `ViewerSetup.cs:288` it calls `itemViewer.GetAllChildren()`, which is an **extension method on
`System.Windows.Forms.Control`** (`UtilitiesCS/Extensions/WinFormsExtensions.cs:146`) and therefore
**cannot be invoked through `IItemViewer`** without a new intent member on the interface.

**Therefore: making the marshalling injectable does not make the tests cheap.** As long as those
casts stand, every test of those members must construct a real 6223-line `ItemViewer`. This is the
single most important design conclusion in this artifact.

### 7.4 Recommended approach

**Two-part, sequenced. Part A is the #743 deliverable; Part B is what actually removes the 60 s risk
and should be scoped explicitly rather than assumed.**

**Part A — route the 8 bypassing sites through the existing `IUiDispatcher` seam.**

Replace each `await _itemViewer.UiSyncContext` with an `_uiDispatcher`-mediated marshal, and each
`_itemViewer.UiDispatcher.InvokeAsync(...)` with `_uiDispatcher.InvokeAsync(...)`. No new abstraction
is introduced; the seam already exists, is already production-default-wired (`Initialization.cs:391`),
and already has a synchronous test double (`QfcItemControllerTestSupport.BuildSyncDispatcher()` at
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:105-145`).

*Why this satisfies constraint 2:* the substitute is an `IUiDispatcher`, not a counterfeit
`SynchronizationContext`. The real pump still services the control tree; only the *choice of
marshaller* becomes injectable.

*Blast radius (production):*
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (467/500 lines — 33 headroom; **a sibling
  work item is concurrently editing this file to add `CultureInfo.InvariantCulture`**)
- `QuickFiler/Controllers/QfcItemController.Initialization.cs`
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`

*Blast radius (test):*
- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` and `.Part3.cs`
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`

*Cost:* does **not** reduce the dominant construction cost. On its own it will not make the
60-second expiry go away. Say so in the spec.

**Part B — remove the concrete-viewer dependency from `ResolveControlGroupsAsync`.**

Add one intent member to `IItemViewer` (`QuickFiler/Viewers/IItemViewer.cs`, 200/500 lines — 300
headroom) exposing the descendant control enumeration that `ViewerSetup.cs:288` currently obtains via
the `Control` extension, then widen `ResolveControlGroupsAsync(ItemViewer)` to
`ResolveControlGroupsAsync(IItemViewer)`. The interface-widening precedent is already established in
this exact file: `IItemViewer.cs:39-52` carries display-state intent members added for the same
reason, and `:184-194` re-declares `InvokeRequired`/`Invoke`/`BeginInvoke` with the in-file rationale
"declared on the interface (not resolved by a concrete cast) so InvokeRequired-guarded [callers stay
mockable]".

*Effect:* the member becomes coverable against a `Mock<IItemViewer>` plus a handful of real `Label`s
on a bare `UserControl` — a control tree three orders of magnitude smaller than the Designer's —
while **the same production member keeps the same coverage**, satisfying constraint 3. The `#230`
justification comment at `ViewerSetup.cs:272-275` must be updated to name the new test, per
`#511 spec.md:265-267`.

*Scope warning:* `epic.md:75-77` assigns `IItemViewer` rewriting to issue **#489**
(`docs/features/active/2026-08-25-itemviewer-surface-defects-489/`). #489 appears to have landed at
least partially (the display-state intent members are present). **Before planning Part B, confirm
#489's disposition** — this could not be done here because `gh` was unavailable. If #489 is still
open, Part B must either be coordinated with it or deferred.

**Explicitly recommended against, and why:**

- *Raising or scaling `PumpTimeoutMs`* — constraint 1.
- *Removing `[Timeout]`* — trades a bounded failure for an unbounded hang; the in-file rationale is
  at `QfcItemController.InitializationTests.cs:33-37`.
- *Adding `[DoNotParallelize]` to the six gate-taking classes* — a no-op under CI (no parallelization
  is enabled there), and under a `/Settings:`-driven run it would serialize classes that the gate
  already serializes. It would, however, make the gate-leak cascade of §2.5 deterministic rather than
  order-dependent, which is arguably worse for diagnosis.
- *Sharing one `WinFormsPumpHost`/`ItemViewer` across a test class* — this is the single highest-leverage
  cost reduction available (11 real `ItemViewer` constructions today would fall to ~3), but it is a
  **test-only** change that introduces order-coupling between tests, contrary to
  `.claude/rules/general-unit-test.md`'s independence principle. Record it as a considered-and-rejected
  alternative, or promote it as its own issue with the coupling risk stated.

### 7.5 Rejected alternatives (brief)

- **Fake `SynchronizationContext` / virtual pump.** Prohibited by constraint 2, and independently
  insufficient: `#729 research §4.2` point 2 and `#511 spec.md` establish that a fake context cannot
  service `Control.Handle` creation or `Control.BeginInvoke` marshalling.
- **Interface over the `WebView2` control alone.** Addresses at most the 8 `WebView2BreadcrumbHostTests`,
  whose measured durations are already 0.018–0.16 s (§4.2) — i.e. it optimises the cheapest tests in
  the population. It also requires the interface to carry `CoreWebView2InitializationCompleted`,
  `Disposed`, and `ConditionalWeakTable` key identity (§5.4), which is disproportionate.
- **Injectable `SynchronizationContext` on `ItemViewer`'s constructor.** Moves where the context is
  captured but leaves `InitializeComponent()` — the actual cost — untouched.

---

## Numeric Derivation Evidence

> Heading note (orchestrator, 2026-09-12T15-05): this heading previously read
> `## 8. Numeric Derivation Evidence`. The section number was removed because
> `.claude/hooks/validate-prd-feature-output.ps1` line 27 matches the heading with
> `^##\s+Numeric\s+Derivation\s+Evidence\s*$`, which a section number defeats. No content was changed.

### N0 — `[TestClass]` types contending for the `UiThreadDispatcherFixture.TransactionGate` permit

Machine-readable restatement authored by the orchestrator, placed first deliberately. The hook at
`.claude/hooks/validate-prd-feature-output.ps1` reads the FIRST occurrence of each of its eleven labels
independently across the whole section, and it requires a plain `- Label: value` on one line with the
count as a bare integer. The `- **Label:**` bold form and the multi-line member sets used in N1 through
N4 below do not match that regex, and `Primary Count` values such as "4 declarations, 19 usages" are not
bare integers. N0 therefore carries the machine-readable form; N1 through N4 remain the authoritative
human-readable derivations and are unchanged. The figures in N0 are taken from N2, not newly derived.

- Complete Family: QfcFormControllerUndoHandoffTests, QfcHomeControllerRunAsyncTests, WpfUiDispatcherTests, QfcItemController_UiThreadDispatcherFixtureTests, QfcItemController_InitializationTests, QfcItemController_SeamFactoryTests
- Exhaustive Search Scope: all `.cs` files in the entire repository source tree, cross-referenced against every `Compile Include` entry in `QuickFiler.Test/QuickFiler.Test.csproj` so that a file absent from the project list cannot contribute a phantom member
- Inclusion Rules: the type declares at least one `[TestMethod]` whose call graph reaches `UiThreadDispatcherFixture.BeginTransactionAsync`, whether the call appears in the method body or transitively through a helper such as `BuildPumpHarnessAsync`
- Exclusion Rules: helper methods that are not tests; callers of `EnsureDispatcher`, which the fixture doc at lines 23 to 27 states never acquires the gate; readers of `UiThreadDispatcherFixture.Current`, which take only `FieldLock`; and pump-host constructions that take no transaction
- Primary Search Strategy or Query Expression: acquire-side call-graph closure over all `.cs` files from a census of the tokens `BeginTransactionAsync` and `BuildPumpHarnessAsync`, attributing every call site to its enclosing test class, which yields QfcFormControllerUndoHandoffTests, QfcHomeControllerRunAsyncTests, WpfUiDispatcherTests, QfcItemController_UiThreadDispatcherFixtureTests, QfcItemController_InitializationTests and QfcItemController_SeamFactoryTests
- Primary Member Set: QfcFormControllerUndoHandoffTests, QfcHomeControllerRunAsyncTests, WpfUiDispatcherTests, QfcItemController_UiThreadDispatcherFixtureTests, QfcItemController_InitializationTests, QfcItemController_SeamFactoryTests
- Primary Count: 6
- Cross-check Search Strategy or Query Expression: release-side census over all `.cs` files using the disjoint tokens `harness.Restore`, `transaction.Install` and `UiThreadDispatcherTransaction`, recovering the declaring type of each release site, which independently yields QfcFormControllerUndoHandoffTests, QfcHomeControllerRunAsyncTests, WpfUiDispatcherTests, QfcItemController_UiThreadDispatcherFixtureTests, QfcItemController_InitializationTests and QfcItemController_SeamFactoryTests
- Cross-check Member Set: QfcFormControllerUndoHandoffTests, QfcHomeControllerRunAsyncTests, WpfUiDispatcherTests, QfcItemController_UiThreadDispatcherFixtureTests, QfcItemController_InitializationTests, QfcItemController_SeamFactoryTests
- Cross-check Count: 6
- Member-set Comparison: the primary and cross-check member sets are equal, ignoring order and case; the two strategies share no token and reach the same closure from opposite ends of the transaction lifetime

Each of the six class declarations was confirmed against the tree on 2026-09-12:
`WpfUiDispatcherTests.cs:19`, `QfcItemController.SeamFactoryTests.cs:27`,
`QfcFormControllerUndoHandoffTests.cs:29`, `QfcItemController.UiThreadDispatcherFixtureTests.cs:31`,
`QfcHomeControllerRunAsyncTests.cs:24` (partial, also declared in three sibling files), and
`QfcItemController.InitializationTests.cs:30` (partial, also declared in `.Part2.cs:29` and `.Part3.cs:27`).

### N1 — `PumpTimeoutMs` declarations and usages in `QuickFiler.Test`

- **Complete Family:** every declaration of, and every reference to, an identifier named
  `PumpTimeoutMs` anywhere in `QuickFiler.Test`, together with the syntactic position of each
  reference (attribute-argument vs. expression).
- **Exhaustive Search Scope:** the entire `QuickFiler.Test` tree, all `.cs` files plus the `.csproj`.
  The classification must distinguish attribute positions from wait positions, because the whole
  disposition of the finding turns on whether any reference is a wait duration.
- **Inclusion Rules:** any lexical occurrence of the identifier `PumpTimeoutMs`.
- **Exclusion Rules:** none — the family is deliberately total.
- **Primary Search Strategy or Query Expression:**
  `Grep pattern="PumpTimeoutMs" path="QuickFiler.Test" output_mode=content -n head_limit=0`
  (identifier census, unlimited results).
- **Primary Member Set:** Declarations (4) — `Viewers/WebView2BreadcrumbHostTests.cs:25`,
  `Controllers/QfcItemController.InitializationTests.cs:38`,
  `Controllers/QfcItemController.SeamFactoryTests.cs:327`,
  `Controllers/QfcItemController.ViewerSetupTests.cs:34`. Usages (19), every one of the literal form
  `[Timeout(PumpTimeoutMs)]` — `WebView2BreadcrumbHostTests.cs` 32, 82, 135, 181, 226, 257, 302, 348;
  `QfcItemController.InitializationTests.Part3.cs` 39, 82, 130, 174, 244, 352, 400, 455;
  `QfcItemController.SeamFactoryTests.cs` 338, 409; `QfcItemController.ViewerSetupTests.cs` 425.
- **Primary Count:** 4 declarations, 19 usages, **0** in a wait/expression position.
- **Cross-check Search Strategy or Query Expression:** a *different family* — a wait/blocking-API
  census over the pump harness itself rather than an identifier census. `Read` of
  `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` **in full** (483 lines), enumerating every
  blocking or waiting construct in the file and inspecting whether any accepts a timeout argument.
  If `PumpTimeoutMs` were used as a wait duration, it would have to be passed to one of these.
- **Cross-check Member Set:** `_ready.Wait()` (`:60`, no timeout); `_thread.Join()` (`:65`, `:264`,
  no timeout); `await _stopped.Task.ConfigureAwait(false)` (`:263`, untimed);
  `StopAsync().GetAwaiter().GetResult()` (`:240`, untimed);
  `new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously)`
  (`:30-32`, `:364-365`, no timeout parameter); `_syncContext.Post(work, null)` (`:388`, no timeout);
  `Application.Run(applicationContext)` (`:326`, no timeout). Zero `Thread.Sleep`, `Task.Delay`,
  `Stopwatch`, `WaitOne(int)`, or `Wait(int)` anywhere in the file.
- **Cross-check Count:** **7** distinct waiting constructs in the harness, **0** of which takes any
  timeout argument; therefore 0 wait-position usages of `PumpTimeoutMs`.
- **Member-set Comparison:** the primary census's wait-position subset is the empty set. The
  cross-check census independently enumerates every construct that *could* consume a timeout value
  and finds none that does. Normalised sets: primary wait-position set = ∅; cross-check
  timeout-accepting set = ∅. **Agreement.** Assertion admitted: *`PumpTimeoutMs` is declared 4 times
  and used 19 times in `QuickFiler.Test`, exclusively as the argument of an MSTest `[Timeout]`
  attribute and never as an in-test wait, and the pump harness contains no timed wait of any kind, so
  `[Timeout]` is the sole bound on every pump-hosted test.* This re-derivation matches the #729
  census at identical line numbers.

### N2 — `QuickFiler.Test` test methods that acquire `UiThreadDispatcherFixture.TransactionGate`

- **Complete Family:** every `[TestMethod]` compiled into `QuickFiler.Test` whose execution reaches
  `UiThreadDispatcherFixture.BeginTransactionAsync()` — directly in its own body, or transitively
  through any helper it calls — together with the `[TestClass]` that declares it and the timeout
  attribute (if any) it carries.
- **Exhaustive Search Scope:** all `.cs` files under `QuickFiler.Test/`, cross-referenced with
  `QuickFiler.Test/QuickFiler.Test.csproj`'s `<Compile Include>` list. The scope must include the
  *transitive* case, because the largest group of contenders (`...InitializationTests.Part3.cs`)
  never names `BeginTransactionAsync` in its own text — it calls `BuildPumpHarnessAsync`.
- **Inclusion Rules:** the method carries `[TestMethod]`, its file is in the csproj, and its call
  graph reaches `BeginTransactionAsync`.
- **Exclusion Rules:** the helper `BuildPumpHarnessAsync` itself (`Part2.cs:46`) and
  `BuildPumpHarnessCoreAsync` (`:68`), which are not tests; `UiThreadDispatcherFixture.EnsureDispatcher`
  callers, which the fixture's own doc (`:23-27`) states "deliberately never acquires
  `TransactionGate`"; `UiThreadDispatcherFixture.Current` readers
  (`Helper Classes/EmailMoveMonitorTests.cs:52`, `:61`), which take only `FieldLock`.
- **Primary Search Strategy or Query Expression:** call-graph closure from an identifier census.
  `Grep pattern="BeginTransactionAsync|UiThreadDispatcherFixture\.|EnsureUiThreadDispatcher"
  path="QuickFiler.Test"`, then, for the one helper that appears (`BuildPumpHarnessAsync`), a second
  census `Grep pattern="BuildPumpHarnessAsync|new WinFormsPumpHost"` to enumerate its callers, then
  attribution of each call site to its enclosing `[TestMethod]` by reading the surrounding lines.
- **Primary Member Set:**
  - Direct (11): `QfcFormControllerUndoHandoffTests.cs` `:228`, `:279`, `:335`;
    `QfcHomeControllerRunAsyncTests.cs` `:325`; `WpfUiDispatcherTests.cs` `:50`;
    `QfcItemController.UiThreadDispatcherFixtureTests.cs` `:42`, `:105`, `:155`, `:204`, `:271`, `:318`.
  - Transitive via `BuildPumpHarnessAsync` (10): `QfcItemController.InitializationTests.Part3.cs`
    call sites `:47, 90, 138, 183, 252, 360, 408, 463` → 8 test methods declared at `:39-40, 82-83,
    130-131, 174-175, 244-245, 352-353, 400-401, 455-456`; `QfcItemController.SeamFactoryTests.cs`
    call sites `:347`, `:418` → 2 test methods at `:339`, `:410`.
  - Observed and excluded: `QfcItemController.ViewerSetupTests.cs:429` constructs a `WinFormsPumpHost`
    but takes no transaction; the eight `new WinFormsPumpHost()` sites in
    `Viewers/WebView2BreadcrumbHostTests.cs` (`:36, 86, 139, 185, 230, 261, 306, 352`) likewise take
    no transaction; the 14 `new WinFormsPumpHost()` sites in `TestSupport/WinFormsPumpHostTests.cs`
    likewise.
- **Primary Count:** **21** test methods, in **6** `[TestClass]` types.
- **Cross-check Search Strategy or Query Expression:** a structural, *release-side* census using a
  disjoint token set — the transaction's own API and the harness restore call, not the acquire call:
  `Grep pattern="harness\.Restore\(\)|UiThreadDispatcherTransaction |transaction\.Dispose\(\)|transaction\.Install\(|public (sealed )?(partial )?class \w+"
  path="QuickFiler.Test" glob="*{UndoHandoff,RunAsyncTests,WpfUiDispatcher,UiThreadDispatcherFixtureTests,SeamFactoryTests,InitializationTests*}*.cs"`.
  Because `UiThreadDispatcherTransaction.Dispose` is the *only* caller of `ReleaseTransactionGate`
  (`UiThreadDispatcherFixture.cs:85-91`, "Called only by `UiThreadDispatcherTransaction.Dispose`, and
  only once per transaction"), every acquirer must have exactly one matching release site.
- **Cross-check Member Set:** `harness.Restore()` — `InitializationTests.Part3.cs` `:67, 111, 159,
  226, 283, 383, 432, 491` (**8**) and `SeamFactoryTests.cs` `:392, :463` (**2**);
  `transaction.Install(` — `QfcFormControllerUndoHandoffTests.cs` `:236, 287, 343` (**3**),
  `QfcHomeControllerRunAsyncTests.cs` `:355` (**1**), `WpfUiDispatcherTests.cs` `:63` (**1**);
  `UiThreadDispatcherTransaction` local declarations in
  `QfcItemController.UiThreadDispatcherFixtureTests.cs` `:48, 108, 158, 210, 223, 277, 294, 324`,
  which resolve to **6** distinct `[TestMethod]` bodies (the method at `:204` holds two transactions
  by design — `transactionA` at `:210` and `transactionB` at `:223` — and the method at `:271` takes
  two sequentially at `:277` and `:294`). Declaring classes recovered by the same query:
  `QfcFormControllerUndoHandoffTests` `:29`, `WpfUiDispatcherTests` `:19`,
  `QfcHomeControllerRunAsyncTests` `:24`, `QfcItemController_InitializationTests` `:27/:29/:30`
  (one partial type over three files), `QfcItemController_UiThreadDispatcherFixtureTests` `:31`,
  `QfcItemController_SeamFactoryTests` `:27`.
- **Cross-check Count:** 8 + 2 + 3 + 1 + 1 + 6 = **21** test methods, in **6** `[TestClass]` types.
- **Member-set Comparison:** normalised primary set
  `{InitializationTests.Part3 ×8, SeamFactoryTests ×2, UndoHandoff ×3, RunAsyncTests ×1,
  WpfUiDispatcherTests ×1, UiThreadDispatcherFixtureTests ×6}` is identical to the normalised
  cross-check set, method for method and class for class. The two strategies share no token
  (`BeginTransactionAsync`/`BuildPumpHarnessAsync` vs. `harness.Restore`/`transaction.Install`/
  `UiThreadDispatcherTransaction`) and reach the same closure from opposite ends of the transaction
  lifetime. **Agreement.** Assertion admitted: *exactly 21 `QuickFiler.Test` test methods, across
  exactly 6 `[TestClass]` types, contend for the single `UiThreadDispatcherFixture.TransactionGate`
  permit; 17 of them carry a 60,000 ms `[Timeout]` and 4 carry no timeout attribute at all.*

### N3 — `QfcItemController` production sites that marshal through the raw viewer rather than the injected seam

- **Complete Family:** every statement in the `QfcItemController` partial class that obtains a UI
  marshalling primitive directly from `_itemViewer` — that is, every read of
  `IItemViewer.UiSyncContext` and every read of `IItemViewer.UiDispatcher` — in any syntactic position
  (await operand, method receiver, argument).
- **Exhaustive Search Scope:** all `.cs` files under `QuickFiler/Controllers/`. Both member names must
  be searched, because a search for only the `await` form would miss the argument-position reads, and
  a search for only `UiSyncContext` would miss the `UiDispatcher` channel entirely.
- **Inclusion Rules:** the site is executable code in a `QfcItemController` partial and reads
  `UiSyncContext` or `UiDispatcher` off `_itemViewer`/`itemViewer`.
- **Exclusion Rules:** comments and commented-out code (`ViewerSetup.cs:36`, `:273`, `:365`,
  `QfcItemController.cs:316`); `EfcItemController` (a different controller: `:191`, `:846`, `:855`,
  `:1104`); `_formViewer.UiSyncContext` sites in `QfcFormController`/`QfcHomeController`/
  `QfcCollectionController`/`EfcFormController` (different viewer, different controller).
- **Primary Search Strategy or Query Expression:** member-name census, repo-wide then narrowed —
  `Grep pattern="UiSyncContext" glob="*.cs"` (105 hits across the repository) filtered to
  `QuickFiler/Controllers/QfcItemController*`, plus
  `Grep pattern="_itemViewer\.UiDispatcher|itemViewer\.UiDispatcher" path="QuickFiler"`.
- **Primary Member Set:** `UiSyncContext` — `ViewerSetup.cs:64` (await), `:282` (argument), `:287`
  (await), `:298` (argument), `:303` (argument) = **5**. `UiDispatcher` —
  `Initialization.cs:200`, `FolderHandling.cs:188`, `ViewerSetup.cs:371` = **3**.
- **Primary Count:** **8**.
- **Cross-check Search Strategy or Query Expression:** a *declaration-side* strategy instead of a
  usage-side one — enumerate the marshalling members the interface actually publishes, then confirm
  the census is complete against that published surface rather than against a guessed name list.
  `Read` of `QuickFiler/Viewers/IItemViewer.cs` in full (200 lines) plus
  `Grep pattern="InvokeRequired|IAsyncResult BeginInvoke|object Invoke|Dispatcher UiDispatcher"` over
  that file, then, for each published marshalling member, a targeted census of `QfcItemController`
  call sites.
- **Cross-check Member Set:** `IItemViewer` publishes exactly five marshalling-capable members —
  `Dispatcher UiDispatcher` (`:36`), `SynchronizationContext UiSyncContext` (`:37`),
  `bool InvokeRequired` (`:192`), `object Invoke(Delegate)` (`:193`),
  `IAsyncResult BeginInvoke(Delegate)` (`:194`). Call-site counts in `QfcItemController`:
  `UiSyncContext` 5, `UiDispatcher` 3, and the `InvokeRequired`/`Invoke`/`BeginInvoke` trio is reached
  only through `QfcItemController.InvokeBeginInvoke` (`FocusAndTheme.cs`), which is a *guarded
  wrapper*, not a raw bypass, and is excluded by the inclusion rule (it is not a read of a marshalling
  primitive off the viewer; it is a delegated marshal). Bypass set = {`ViewerSetup.cs:64, 282, 287,
  298, 303`, `Initialization.cs:200`, `FolderHandling.cs:188`, `ViewerSetup.cs:371`}.
- **Cross-check Count:** **8**.
- **Member-set Comparison:** the normalised primary set and the normalised cross-check set are
  identical, element for element. The two strategies differ in direction (usage-name census vs.
  interface-declaration enumeration followed by per-member call-site counting), and the cross-check
  additionally proves *exhaustiveness over the published marshalling surface* — that no sixth
  marshalling member exists on `IItemViewer` that the usage census could have missed. **Agreement.**
  Assertion admitted: *exactly 8 executable sites in the `QfcItemController` partial class marshal
  through a primitive read directly off `_itemViewer` rather than through the injected
  `IUiDispatcher` seam: 5 reads of `UiSyncContext` and 3 of `UiDispatcher`.*

### N4 — Run count N required for acceptance criterion 3

This is a closed-form derivation, not an enumeration, so it is presented with two independent
computational routes rather than two member sets.

**Base rate, and where it comes from.**
`docs/features/epics/quickfiler-suite-determinism-foundation/epic-status.md:153`:

> "Seven expiries at the 60,000 ms PumpTimeoutMs under machine load. ... Pre-fix run-level failure
> rate is about **1 in 21**, so thirty consecutive clean runs has probability about **0.23** under the
> null hypothesis of no effect."

The underlying observation is a single event:
`.../511/evidence/regression-testing/determinism-ten-runs.2026-08-21T18-10.md:81-85` — one failing
run ("the second P0-T16 coverage invocation reported 6430 / 6437 with seven 60,000 ms `PumpTimeoutMs`
expiries") against a 20-run pre-fix table
(`.../prefix-baseline.2026-08-21T18-10.md:59`: "The measured pre-fix failure rate across these twenty
runs is **zero**"). Hence p̂ = 1/21 = 0.0476190, and 1 − p̂ = 20/21 = 0.9523810.

**Verification of the stated 0.23.**
ln(20/21) = −0.04879016. 30 × (−0.04879016) = −1.4637049. e^(−1.4637049) = **0.23138**.
Route 2 (repeated squaring, no logarithms): 0.952381² = 0.907029; ⁴ = 0.822702; ⁸ = 0.676839;
¹⁶ = 0.458111; 0.952381³⁰ = ⁱ⁶ × ⁸ × ⁴ × ² = 0.458111 × 0.676839 × 0.822702 × 0.907029 = **0.23138**.
Both routes agree to five significant figures. **The stated p ≈ 0.23 is correct**, and thirty clean
runs is therefore not significant at any conventional level — it would occur by chance roughly once
in every four attempts even if the fix did nothing.

**Formula.**
Under H₀ (the fix has no effect, so each run independently fails with probability p = 1/21), the
observation "N runs, 0 failures" has exact one-sided p-value

    p-value(N) = (1 − p)^N = (20/21)^N

Requiring p-value(N) ≤ α gives

    N ≥ ln(α) / ln(1 − p) = ln(α) / ln(20/21) = ln(α) / (−0.04879016)

**Results.**

| α | ln(α) | N ≥ | Smallest integer N | (20/21)^N | (20/21)^(N−1) |
|---|---|---|---|---|---|
| 0.05 | −2.9957323 | 61.40 | **62** | 0.048558 ≤ 0.05 ✓ | 0.050986 > 0.05 ✓ |
| 0.01 | −4.6051702 | 94.39 | **95** | 0.0097053 ≤ 0.01 ✓ | 0.0101907 > 0.01 ✓ |

Each boundary was checked in both directions (N and N−1), which is the second independent route: the
inequality is verified by direct exponentiation rather than only by the logarithmic solve.

**Recommended AC value: N = 62 consecutive clean runs at α = 0.05.**

**Three caveats that must accompany the number in the spec, or it will not survive review.**

1. **The base rate is a point estimate from a single observed failure.** The exact (Clopper–Pearson)
   95 % interval for 1 success in 21 trials is approximately [0.0012, 0.2382]. At the lower end
   (p = 0.0012), N would need to exceed 2,400 runs for α = 0.05; at the upper end (p = 0.238), N = 11
   would suffice. **N = 62 is defensible as derived from the recorded point estimate, and only as
   that.** State it that way.
2. **The runs must be performed under the condition that produced the failure.** The single observed
   failure's "distinguishing condition was 17 idle MSBuild node-reuse processes"
   (`determinism-ten-runs.md:83-84`). Sixty-two runs on an idle machine test nothing, because the 30
   post-fix runs already recorded in that evidence were green and the failure did not occur there
   either.
3. **Sixty-two full-suite runs is a very large time budget.** From §4.2, a clean QuickFiler.Test pass
   is ~1287 tests; the #511 evidence records ten full-suite runs "totalling roughly two and a half
   hours" under load (`determinism-ten-runs.md:77`). Sixty-two runs under load therefore projects to
   roughly 15 hours. **Consider replacing the run-count AC with a mechanism AC** — for example,
   asserting that the specific member under test no longer constructs a real `ItemViewer`, which is a
   deterministic, single-run, structurally checkable property — and keep the statistical criterion
   only as a supporting signal. A mechanism AC is both cheaper and stronger evidence than a green
   streak.

---

## 9. Behaviour semantics

| # | Behaviour | Success | Failure | Ordering / edge cases |
|---|---|---|---|---|
| 1 | A member currently marshalling through `_itemViewer.UiSyncContext` marshals through the injected `IUiDispatcher` | The member completes when driven with `BuildSyncDispatcher()` and no `WinFormsPumpHost` | The member hangs, or resolves the context off the viewer | `_uiDispatcher` is `null` until `SaveParameters` applies `??= new WpfUiDispatcher()` (`Initialization.cs:391`); the `MailActions.cs:33-38` comment records that `_uiDispatcher` is already observed null in `SeamFactoryTests`. Every new call site needs the same null tolerance. |
| 2 | Production behaviour is unchanged | `WpfUiDispatcher` forwards 1:1 to the WPF `Dispatcher` (`IUiDispatcher.cs:9-13`); a live run marshals to the same thread as before | Any observable ordering change on the UI thread | `await ctx` resumes on the captured context; `IUiDispatcher.InvokeAsync` posts to the dispatcher. These are **not** the same primitive — a WinForms `SynchronizationContext` and a WPF `Dispatcher` are different queues on the same thread. **This is the highest-risk aspect of Part A and must be established before, not after, the edit.** |
| 3 | De-exemption coverage is preserved | Every `#230` justification comment still names a test that exists and covers the member | A comment names a deleted or renamed test | `#511 spec.md:265-267`. Comments to re-check: `Initialization.cs` 135/164/196/259/291/403/447 and `ViewerSetup.cs:272-275`, all pre-drift. |
| 4 | The gate is released even when a test times out | No subsequent gate-taker blocks after an expiry | One expiry cascades into up to 20 more, and hangs 4 un-timed tests | §2.5. A defensible remedy is an `[AssemblyCleanup]`- or `[TestCleanup]`-anchored release, or moving the acquire/release out of the abandoned async path. |
| 5 | `ItemViewer` coverage ACs | AC is phrased over a measured type | AC is phrased over `ItemViewer` | §6.1 — the type emits no Cobertura element at all. |

---

## 10. Test strategy (no test code written)

- **Framework/libraries:** MSTest, Moq, FluentAssertions, per `CLAUDE.md` CUT1/CUT2. No new
  dependency is required; `IUiDispatcher`, `BuildSyncDispatcher()`, `WinFormsPumpHost`, and
  `UiThreadDispatcherFixture` all already exist.
- **Fail-before:** for Part A the red test is structural and deterministic — assert that the member
  under test completes when driven with a synchronous `IUiDispatcher` and **no** `WinFormsPumpHost`.
  Before the change it hangs (so it must itself carry a `[Timeout]` as a deadlock bound); after, it
  passes. That is a genuine red→green transition with no timing tolerance in the assertion.
- **Retain, do not replace, the pump-hosted tests** for at least one member per de-exemption block,
  so constraint 3 is satisfied by construction rather than by argument.
- **For the gate-leak hypothesis (§2.5):** a deterministic test is possible without any timing
  dependence — assert that after a transaction's owner is abandoned, a subsequent
  `BeginTransactionAsync` still completes. Whether this is in scope for #743 is a scoping decision,
  not a research finding; it is a separate defect from the seam.
- **Determinism:** no `Thread.Sleep`, `Task.Delay`, `Stopwatch`, or wall-clock wait in any new test
  (`.claude/rules/general-unit-test.md`, Determinism Infrastructure). `[Timeout]` as a deadlock bound
  is the established in-repo precedent (`QfcItemController.InitializationTests.cs:33-37`).
- **No temporary files** (General Unit Test Policy UT4).

---

## 11. Non-SDK project files that must be edited if any `.cs` file is added or deleted

Both projects are legacy, non-SDK-style MSBuild projects with fully enumerated `<Compile Include>`
lists. **Adding or deleting any `.cs` file requires an explicit edit to the corresponding `.csproj`.**

| Role | Repository-relative path | Evidence it is non-SDK |
|---|---|---|
| QuickFiler production | `QuickFiler/QuickFiler.csproj` | `:2` — `<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">` (no `Sdk` attribute); `:4` imports `Microsoft.Common.props` |
| QuickFiler tests | `QuickFiler.Test/QuickFiler.Test.csproj` | `:2` — `<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`; `:18` `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` |

Existing entry shapes, quoted verbatim.

Simple form (the shape to use for a new plain `.cs` file) — `QuickFiler.Test/QuickFiler.Test.csproj:194`:

```xml
    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />
```

and `QuickFiler/QuickFiler.csproj:335`:

```xml
    <Compile Include="Controllers\QfcItemController.ViewerSetup.cs" />
```

Designer-partial form, which carries child metadata — `QuickFiler/QuickFiler.csproj:427-433`:

```xml
    <Compile Include="Viewers\ItemViewer.cs">
      <SubType>UserControl</SubType>
    </Compile>
    <Compile Include="Viewers\ItemViewer.DisplayState.cs">
      <DependentUpon>ItemViewer.cs</DependentUpon>
      <SubType>UserControl</SubType>
    </Compile>
```

Note the path separator is a backslash and the path is relative to the project directory.

---

## 12. File-size headroom for the files in the blast radius

The 500-line cap (`.claude/rules/general-code-change.md`, "File Size Limit") applies to every file
listed below.

| File | Lines | Headroom | Note |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 467 | 33 | **Contended** — a sibling item is adding `CultureInfo.InvariantCulture` here |
| `QuickFiler/Viewers/ItemViewer.cs` | 400 | 100 | |
| `QuickFiler/Viewers/IItemViewer.cs` | 200 | 300 | Preferred home for a Part B intent member |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 460 | 40 | |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | 6223 | n/a | Designer-generated |
| `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` | 483 | 17 | |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 278 | 222 | |

Not measured: `QfcItemController.Initialization.cs`, `QfcItemController.FolderHandling.cs`,
and the four pump-hosted test files. Measure them at Phase 0 before planning any insertion.

---

## 13. Contention awareness (no coordination attempted)

- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — a sibling item is adding
  `CultureInfo.InvariantCulture`. This file is **also** the primary Part A edit target (5 of the 8
  bypass sites) and has only 33 lines of headroom. This is the highest-probability merge conflict in
  the item.
- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` — a sibling item is editing the WebView2 breadcrumb
  host. §5.4's observations about its constructor should be treated as possibly stale by merge time.

---

## 14. Open questions for the orchestrator

1. **The #592 acceptance criteria consolidated onto #743 on 2026-09-11 were not read** (no `gh`).
   Every AC-shaped recommendation above is derived from repository evidence, not from that comment.
   Reconcile before planning.
2. **Is issue #489 (`IItemViewer` surface) still open?** Part B of the recommendation overlaps its
   scope per `epic.md:75-77`. Could not be determined without `gh`.
3. **Which command is the authoritative repro** — #743's (`no /Settings:`) or #711's
   (`/Settings:TaskMaster.runsettings`)? The two enable different parallelization and therefore test
   different hypotheses. §2.4(b) turns on this.
4. **Is AC 3 to remain a run-count criterion?** §8/N4 recommends N = 62 if so, with three caveats,
   and recommends a mechanism AC instead if the 15-hour projection is unacceptable.
