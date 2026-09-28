---
name: pump-timeout-743
description: "#743/#592/#511: the dispatcher-gate lead is stale (symbols deleted by #493); 9 of 19 pump tests never touch the gate; committed TRX timestamps quantify gate blocking; no expiry artifact exists in-repo"
metadata:
  type: project
---

Issue #743 (QuickFiler `ItemViewer` UI-marshalling seam) research, 2026-09-12.

**Fact:** `UiThreadDispatcherGate` / `SwapUiThreadDispatcher` exist in ZERO `.cs` files — issue #493
deleted them and replaced them with `UiThreadDispatcherFixture` / `UiThreadDispatcherTransaction`
(`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, whose doc comment
names #493, not #648; #648 only added `WpfUiDispatcherTests` as a consumer). Any delegation prompt
citing those two symbols is quoting `docs/` or `epic.md:93`, not code.

**Why:** the epic's premise "shared by two test classes" is out of date by 3x — 21 test methods
across 6 `[TestClass]` types now contend for the single `TransactionGate` permit, and 4 of them
(`QfcFormControllerUndoHandoffTests` x3, `QfcHomeControllerRunAsyncTests` x1) carry NO `[Timeout]`,
so a gate leak hangs unboundedly there rather than failing at 60s.

**How to apply:** when a maintainer lead names a synchronisation primitive, grep `.cs` only before
accepting it. Then check the *population* the lead can reach: 9 of the 19 `[Timeout(PumpTimeoutMs)]`
tests never take the gate, and 8 of those 9 are the `WebView2BreadcrumbHostTests` the issue itself
names as failing — which falsifies the lead as a dominant cause without any measurement.

## Reusable techniques discovered

1. **Committed TRX files are a measurement instrument, not just a pass/fail log.** `<UnitTestResult>`
   carries `duration`, `startTime`, `endTime` per test. Overlapping windows prove class-level
   parallelism; a test whose window *starts* before another's gate release *ends* lets you subtract
   and read off the exact blocking time. In `.../662/evidence/qa-gates/p2-t7/quickfiler-postchange.trx`
   this gave 4.10s of pure `TransactionGate` blocking out of a 4.73s duration — i.e. 6.8% of the
   60,000 ms bound, which is how the lead got quantified rather than argued. The `<ResultSummary>`
   `<StdOut>` also prints `Test Parallelization enabled ... (Workers: 24, Scope: ClassLevel)`, so the
   TRX self-reports whether parallelism was on. Grep `duration="00:00:0[5-9]|duration="00:0[1-9]:` to
   find the slow tail fast.
2. **Absence of evidence is itself reportable.** Repo-wide grep for `exceeded execution timeout
   period`, `Test timed out`, `timeout="[1-9]` across `docs/` returns ZERO matches. There is no
   committed artifact of an actual 60s expiry anywhere, because #511 never merged (work preserved on
   `bug/winformspumphost-suite-determinism-511-exec` at `53a2a08f`). State that instead of inferring.
3. **`[ExcludeFromCodeCoverage]` invisibility is directly checkable.** Grep a committed
   `.cobertura.xml` for `filename="<path prefix>` — `QuickFiler\Viewers\ItemViewer.cs` and
   `ItemViewer.Designer.cs` emit NO `<class>` element at all, while the unexempt sibling
   `ItemViewerExpanded.Designer.cs` emits one at line-rate 0.995. The attribute at `ItemViewer.cs:20`
   is type-scoped so it hides the 6223-line Designer partial too.
4. **Two committed coverage reports can disagree for the same file.** `QfcItemController.ViewerSetup.cs`
   reads 0.904762 in the 2026-09-08 report and 0.863014 in the 2026-09-06 one. Never lift a baseline
   from a committed report; measure fresh.

## Traps

- **Two different repro commands are two different experiments.** #743's `issue.md:22` passes no
  `/Settings:`; #711's passes `/Settings:TaskMaster.runsettings`, which declares
  `<Workers>0</Workers><Scope>ClassLevel</Scope>`. CI (`.github/workflows/_mstest-coverage.yml:99`)
  passes no `/Settings:` and `QuickFiler.Test/Properties/AssemblyInfo.cs` has no `[assembly: Parallelize]`,
  so gate contention is structurally impossible under the #743 command.
- **MSTest default is NON-cooperative timeout.** `useCooperativeCancellation` defaults to `false`
  (Microsoft Learn "Configure MSTest", `mstest.timeout` table); the repo has no `testconfig.json` and
  no `CooperativeCancellation` entry in any `.runsettings`. Default `false` means MSTest DOES stop
  observing a timed-out method, so an `async Task` test's `finally` — where the gate is released —
  may never run. `#511 spec.md:132-139` recorded this cascade and deliberately left it unfixed; #493
  did not close it.
- **A marshalling seam alone cannot make these tests cheap.** `QfcItemController` reaches the viewer
  through 14 `(ItemViewer)_itemViewer` concrete casts plus
  `ResolveControlGroupsAsync(ItemViewer itemViewer)`, and `ViewerSetup.cs:288` calls
  `GetAllChildren()`, an extension method on `System.Windows.Forms.Control`
  (`UtilitiesCS/Extensions/WinFormsExtensions.cs:146`) that cannot be invoked through `IItemViewer`.
  So the real 6223-line Designer tree gets built regardless of any `IUiDispatcher` change.
- **`WebView2BreadcrumbHost` already constructor-injects its control** (`:70-71` public 2-arg,
  `:88-92` internal 3-arg with `BreadcrumbUiDispatcher`). What pins the concrete `WebView2` is the
  body: a `ConditionalWeakTable<WebView2, ...>` key at `:46-47` and `+=` on
  `CoreWebView2InitializationCompleted` / `Disposed` at `:112-113`. #743's "introduce an interface
  over the control" bullet is therefore much larger than it sounds.
- **The 1-in-21 base rate rests on a single observed failing run** (20 clean pre-fix runs + 1 event
  with seven expiries). 0.952381^30 = 0.23138 checks out, and N=62 at alpha=0.05 / N=95 at 0.01, but
  the Clopper-Pearson 95% CI for 1/21 is roughly [0.0012, 0.2382], which spans N=11 to N>2400. Quote
  N=62 only as "derived from the recorded point estimate", and prefer a mechanism AC — 62 loaded
  full-suite runs projects to ~15 hours from the #511 timing evidence.
