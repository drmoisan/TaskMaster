# Research — WpfUiDispatcherTests ungated static swap (Issue #648)

- Timestamp: 2026-08-31T20-15
- Work mode: minor-audit
- Severity: Low
- Scope: one file, `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`
- Branch: `bug/wpfuidispatchertests-ungated-static-swap-648`, cut from `origin/main` at `2b85134b42872e405602e6064e02dc9cda6c319b`

This artifact answers the six delegated questions and nothing further. Every claim cites a
repository-relative path and a line or identifier read during this pass.

## 0. Verification of supplied context

All supplied context was re-verified and is accurate. No correction is required.

| Claim | Verification |
| --- | --- |
| Target file is 88 lines (89 with trailing newline) | `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` read in full; closing brace at :88 |
| `typeof(UiThread).GetField(` at :42, literal at :43, `field.SetValue(null, dispatcher);` at :51, unconditional `finally` at :81-85 | Read; `finally` block spans :81-85 with the restore at :83 and shutdown at :84 |
| Literal `"_dispatcher"` on exactly 2 lines under `QuickFiler.Test/`, 5 repo-wide | Confirmed: `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:43`, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136`, plus `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144` |
| `BeginTransactionAsync` / `Install` / `Dispose` shape | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:122-126`, `:242-254`, `:261-276` |
| Both types are `internal` to `QuickFiler.Test` | `UiThreadDispatcherFixture` declared `internal static class` at `:29`; `UiThreadDispatcherTransaction` declared `internal sealed class` at `:220` |
| Consumer exemplar around `QfcItemController.InitializationTests.Part2.cs:53` | Confirmed at `:53-55` |

Two additional facts, not in the supplied context, are relevant and are used below.

- `CLAUDE.md` §C#1.2 names `.globalconfig` as an analyzer-configuration input. No `.globalconfig`
  exists anywhere in the tree (glob `**/.globalconfig` returned no files). `.editorconfig` at the
  repository root is the only analyzer severity configuration.
- The parameterless `WpfUiDispatcher` constructor captures a lazy provider rather than reading the
  static eagerly: `UtilitiesCS/Threading/WpfUiDispatcher.cs:24-25` chains to `:33-35`, and
  `private Dispatcher Dispatcher => _dispatcherProvider();` at `:37` re-reads `UiThread.Dispatcher`
  on every member call (`:40`, `:43`, `:53`). Construction order relative to `Install` is therefore
  not load-bearing, but the static must hold the test dispatcher for the whole assertion region.

---

## 1. Exact target shape of the rewritten test method

### 1.1 Shape the existing consumers use

Every in-repo consumer of the transaction uses **explicit `try` / `finally` with a call to
`transaction.Dispose()`**, never a `using` statement or `using` declaration:

- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:53-65` — awaits the
  transaction, then `try { ... } catch { transaction.Dispose(); throw; }`.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` — six tests, all
  `public async Task`, all disposing in a `finally`: `:48-51/:87-90`, `:108-111/:143-146`,
  `:158-161/:185-187`, `:203-205/:225-226`, `:270-274`, `:317-320/:336-338`.

The closest structural exemplar is test R1 at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:40-96`. It has
exactly the shape #648 needs: an outer `try`/`finally` owning the live dispatcher
(`StartRunningDispatcher()` at `:45`, `ShutdownDispatcher(liveA)` at `:94`) wrapping an inner
`try`/`finally` owning the transaction (`BeginTransactionAsync()` at `:48-50`,
`transaction.Dispose()` at `:89`).

### 1.2 `async Task` is required

`UiThreadDispatcherFixture.BeginTransactionAsync()` returns
`Task<UiThreadDispatcherTransaction>` (`QfcItemController.UiThreadDispatcherFixture.cs:122`). The
only synchronous alternatives are `.GetAwaiter().GetResult()` or `.Result`, neither of which any
consumer uses. The method must become `public async Task`. This matches AC-3, which states the
method "is declared `async Task` because the gate is awaited", and matches all six fixture tests.

A `using` declaration (C# 8) is **not** recommended. `QuickFiler.Test/QuickFiler.Test.csproj` declares
no `<LangVersion>` (read `:1-40`; a repo-wide grep over `*.csproj`/`*.props`/`*.targets` found
`LangVersion` in thirteen projects, none of them `QuickFiler.Test`), so the effective language version
for this assembly is compiler-default rather than pinned. Independently, the block-scoped form is the
uniform convention here: the file under change already uses the block form at
`WpfUiDispatcherTests.cs:70-78`, and `.editorconfig:7` sets
`csharp_prefer_simple_using_statement = true:suggestion` — a suggestion, not an enforced style. Keep
`try`/`finally`.

### 1.3 Proposed rewritten method

```csharp
        private const int GateTimeoutMs = 60000;

        /// <summary>
        /// Cycle-3 P9-T7 (member #39, de-exempted): asserts that <c>Invoke</c>, <c>InvokeAsync</c>, and
        /// <c>BeginInvoke</c> each execute the supplied delegate on the dispatcher's own thread (not the
        /// test thread). <c>BeginInvoke</c> is fire-and-forget, so its completion is observed
        /// deterministically via a <see cref="ManualResetEventSlim"/> signal rather than polling.
        /// <para>
        /// Issue #648: the swap of the process-wide static goes through
        /// <c>UiThreadDispatcherFixture</c>, which holds <c>TransactionGate</c> from acquisition until
        /// <c>Dispose</c> and restores by <c>ReferenceEquals</c> compare-then-write.
        /// </para>
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()
        {
            // Arrange
            Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(dispatcher);
                    WpfUiDispatcher sut = new WpfUiDispatcher();
                    int dispatcherThreadId = dispatcher.Thread.ManagedThreadId;

                    // Act / Assert — Invoke (blocking, synchronous marshal)
                    int invokeThreadId = -1;
                    sut.Invoke(() => invokeThreadId = Thread.CurrentThread.ManagedThreadId);
                    invokeThreadId.Should().Be(dispatcherThreadId);

                    // Act / Assert — InvokeAsync
                    int invokeAsyncThreadId = -1;
                    Task invokeAsyncTask = sut.InvokeAsync(() =>
                        invokeAsyncThreadId = Thread.CurrentThread.ManagedThreadId
                    );
                    invokeAsyncTask.GetAwaiter().GetResult();
                    invokeAsyncThreadId.Should().Be(dispatcherThreadId);

                    // Act / Assert — BeginInvoke (fire-and-forget; observed deterministically via a signal)
                    int beginInvokeThreadId = -1;
                    using (ManualResetEventSlim signal = new ManualResetEventSlim(false))
                    {
                        sut.BeginInvoke(() =>
                        {
                            beginInvokeThreadId = Thread.CurrentThread.ManagedThreadId;
                            signal.Set();
                        });
                        signal.Wait();
                    }
                    beginInvokeThreadId.Should().Be(dispatcherThreadId);
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
            }
        }
```

The three Act/Assert regions are copied verbatim from `WpfUiDispatcherTests.cs:55-79`, which satisfies
AC-4 (behavior preserved). `Construction_YieldsAnIUiDispatcher` at `:23-30` is untouched.

The `field.Should().NotBeNull(...)` guard at `WpfUiDispatcherTests.cs:46` is not reproduced, and does
not need to be: the fixture performs the identical assertion once, in
`UiThreadDispatcherFixture.ResolveDispatcherField()` at
`QfcItemController.UiThreadDispatcherFixture.cs:135-140`, including the same `because:` text.

### 1.4 Using directives after the rewrite

- `using System.Reflection;` (`:1`) must be removed — see §4.1.
- `using UtilitiesCS;` (`:7`) becomes unused. It exists solely for `typeof(UiThread)` at `:42`:
  `UiThread` is declared in namespace `UtilitiesCS` (`UtilitiesCS/Threading/UiThread.cs:15-17`),
  whereas `IUiDispatcher` and `WpfUiDispatcher` are in `UtilitiesCS.Threading`
  (`UtilitiesCS/Threading/IUiDispatcher.cs:15`, `UtilitiesCS/Threading/WpfUiDispatcher.cs:7,17`),
  supplied by `using UtilitiesCS.Threading;` at `:8`. The fully-qualified `<see cref="UtilitiesCS.UiThread.Dispatcher"/>`
  in the class doc comment at `:15` needs no using. Removing `:7` is recommended and is inside the
  single-file scope boundary; it is not mandated by any acceptance criterion, and leaving it raises no
  diagnostic (§4.1).
- `System.Threading` (`:2`), `System.Threading.Tasks` (`:3`), `System.Windows.Threading` (`:4`),
  `FluentAssertions` (`:5`), `Microsoft.VisualStudio.TestTools.UnitTesting` (`:6`) and
  `UtilitiesCS.Threading` (`:8`) all remain in use.

---

## 2. Teardown ordering

**Correct order: `transaction.Dispose()` first, then `QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher)`.**
The nested `try`/`finally` in §1.3 produces exactly that order.

Four supporting reasons, in decreasing weight:

1. **It closes an observability window that the reverse order opens.** `ShutdownDispatcher` calls
   `dispatcher?.InvokeShutdown()` (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:277-280`),
   which ends the message loop started by `Dispatcher.Run()` at `:260`. If shutdown ran first, the
   static would transiently name a dispatcher whose loop has ended. `TransactionGate` does not protect
   against that being observed: `UiThreadDispatcherFixture.EnsureDispatcher()`
   (`QfcItemController.UiThreadDispatcherFixture.cs:99-115`) takes only `FieldLock`, never
   `TransactionGate`, and installs nothing when the field is non-null (`:107-111`). A concurrent class
   calling the `EnsureUiThreadDispatcher` wrapper (`QfcItemController.TestSupport.cs:238-239`) would
   therefore accept the dead dispatcher and post work that never executes. Disposing first eliminates
   the window, because `UiThreadDispatcherTransaction.Dispose()` restores at `:272` strictly before it
   releases the gate at `:275` — the property the class doc states at `:216-218`.
2. **The reverse hazard does not exist.** After `Dispose()`, `CompareExchange(_installedValue, _previous)`
   at `:272` has already removed our dispatcher from the static, so shutting it down afterwards cannot
   be observed by any other reader.
3. **It preserves the pre-change semantic order.** The current `finally` restores at
   `WpfUiDispatcherTests.cs:83` and only then shuts down at `:84`.
4. **It matches the exemplar.** R1 disposes the transaction in the inner `finally`
   (`QfcItemController.UiThreadDispatcherFixtureTests.cs:89`) and shuts the dispatcher down in the
   outer `finally` (`:94`). R4 and R6 use the same nesting (`:225-226`/`:253`, `:337`/`:342`).

`Dispose()` is idempotent (guard at `:262-268`), so the inner `finally` calling it after a successful
inner-body call would be safe; the shape in §1.3 calls it exactly once regardless.

---

## 3. Hang risk and `[Timeout]`

**Require `[Timeout(GateTimeoutMs)]` with `private const int GateTimeoutMs = 60000;`.**

Evidence:

- **Every existing awaiter of `TransactionGate` carries a 60 s timeout.** All six tests in
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` are decorated
  `[Timeout(GateTimeoutMs)]` at `:41`, `:104`, `:154`, `:196`, `:263`, `:310`, against
  `private const int GateTimeoutMs = 60000;` at `:33`. The class doc states the rationale directly at
  `:24-26`: "Every test carries the 60-second MSTest timeout attribute so a genuine deadlock becomes a
  test failure rather than a hung run."
- **The indirect consumers do too.** The pump tests that reach the gate through
  `BuildPumpHarnessAsync` are decorated `[Timeout(PumpTimeoutMs)]`
  (`QfcItemController.InitializationTests.Part3.cs:39`, with
  `internal const int PumpTimeoutMs = 60000;` at `QfcItemController.InitializationTests.cs:38`), as
  are `QfcItemController.SeamFactoryTests.cs:293` and `QfcItemController.ViewerSetupTests.cs:34`
  (both `= 60000`).
- **No repo-wide MSTest timeout exists.** `TaskMaster.runsettings` (30 lines, read in full) contains
  only `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`
  plus coverage `ModulePaths` excludes; `scripts/vscode/TaskMaster.cli.runsettings` (9 lines, read in
  full) contains only the `Parallelize` block. Neither declares `TestTimeout` or `TestSessionTimeout`.
  A repo-wide grep for `TestTimeout|TestSessionTimeout|[Timeout(` over `*.cs`, `*.runsettings`,
  `*.yml`, `*.props` returned matches only in eight `.cs` test files. The only time bound in CI is the
  job-level `timeout-minutes: 30` at `.github/workflows/_mstest-coverage.yml:14`, which kills the whole
  job rather than reporting a failed test.
- **The rewrite introduces a real, if bounded, wait.** `BeginTransactionAsync` calls
  `TransactionGate.WaitAsync()` with no timeout and no cancellation token
  (`QfcItemController.UiThreadDispatcherFixture.cs:124`). Without `[Timeout]`, a transaction leaked
  anywhere in the assembly turns this test into an unbounded hang that only the 30-minute CI job
  ceiling terminates.

Related fixture design note requested by the question: the file-level doc at
`QfcItemController.UiThreadDispatcherFixture.cs:22-27` records why `EnsureDispatcher` deliberately
never acquires `TransactionGate` — "Callers of the `QfcItemControllerTestSupport.EnsureUiThreadDispatcher`
wrapper live in test files that carry no `[Timeout]`, so making them wait on a gate another test class
holds for a whole test body would convert a bounded failure elsewhere into an unbounded hang there."
The rewritten test is on the opposite side of that trade: it becomes a gate *holder*, so it must carry
the timeout, and it is the reason §2 orders `Dispose()` before shutdown.

---

## 4. Analyzer consequences of the rewrite

Governing configuration, read this pass:

- `.editorconfig:27` — `dotnet_analyzer_diagnostic.severity = suggestion`. The in-file comment at
  `:24-25` states the intent: "All new analyzer diagnostics default to suggestion so they cannot be
  promoted to errors under the nullable `/p:TreatWarningsAsErrors=true` build (the protected CI gate)."
- `.editorconfig:29` — `dotnet_diagnostic.MSTEST0032.severity = warning`. A grep of `.editorconfig`
  for `severity = error` returned **no** matches, and for `severity = warning` returned exactly one
  match, line 29. MSTEST0032 is therefore the single rule in the whole configuration above
  `suggestion`.
- `QuickFiler.Test/QuickFiler.Test.csproj` declares no `EnforceCodeStyleInBuild`,
  `EnableNETAnalyzers`, `GenerateDocumentationFile`, `TreatWarningsAsErrors`, `NoWarn` or
  `AnalysisLevel` (grep over the file returned only the comment at `:501`). `WarningLevel` is `5`
  (`:39`). Analyzer/nullable enforcement comes exclusively from the two solution-level `msbuild`
  command lines in `CLAUDE.md` §"C# Toolchain".
- Analyzer packages referenced by this assembly (`QuickFiler.Test/packages.config`): AsyncFixer 2.1.0
  (`:3`), Meziantou.Analyzer 3.0.194 (`:11-16`), Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0
  (`:20-25`), MSTest.Analyzers 4.3.3 (`:113-118`), Roslynator.Analyzers 5.0.0 (`:139-144`),
  SonarAnalyzer.CSharp 10.33.0.1635 (`:145-150`).

### 4.1 Removing `using System.Reflection;`

**Required, but by acceptance criterion rather than by any diagnostic.** AC-2 states the file must
contain "no `using System.Reflection;` directive". After the rewrite `FieldInfo` is not referenced
anywhere in the file — the only three references are `FieldInfo field` at `WpfUiDispatcherTests.cs:42`
and `BindingFlags` at `:44`, both inside the region being replaced.

Leaving it would raise nothing enforceable. The applicable diagnostic is IDE0005 (unnecessary using
directive), and two independent facts neutralize it here: it falls under the `suggestion` catch-all at
`.editorconfig:27`, and it is not reported in a command-line build for a project that sets neither
`GenerateDocumentationFile` nor `EnforceCodeStyleInBuild` — `QuickFiler.Test.csproj` sets neither.
CSharpier does not remove unused usings. The same reasoning applies to the discretionary removal of
`using UtilitiesCS;` (§1.4): no diagnostic either way.

### 4.2 `.ConfigureAwait(false)`

**Not required by any enabled rule; nevertheless write it, to match every existing consumer.**

- No rule forces it. CA2007 appears nowhere in `.editorconfig` (grep returned no match) and is
  therefore governed by the `suggestion` catch-all at `:27`. The only ConfigureAwait-adjacent rule
  named explicitly is CRR0029 ("ConfigureAwait(true) is called implicitly") at `.editorconfig:3-4`,
  also `suggestion`. Meziantou's MA0004 (`use ConfigureAwait when awaiting`) is `suggestion` at
  `:35`, and Roslynator's RCS1090 is `suggestion` at `:376`. A `suggestion` cannot be promoted by
  `/p:TreatWarningsAsErrors=true`, because it is not a warning.
- Every fixture consumer writes it: `QfcItemController.InitializationTests.Part2.cs:53-55` and
  `:58-59`; `QfcItemController.UiThreadDispatcherFixtureTests.cs:50`, `:110`, `:160`, `:205`, `:218`,
  `:232`, `:272`, `:289`, `:319`; `QfcItemController.InitializationTests.Part3.cs:47` and `:50`. The
  fixture itself writes it internally at `QfcItemController.UiThreadDispatcherFixture.cs:124`.
- The assembly is not uniform outside that family — for example
  `QfcItemController.ViewerSetupTests.cs:224` awaits without it — so this is a convention match within
  the fixture-consumer set, not a repository-wide rule.

### 4.3 New diagnostics from an `async Task` MSTest method

No diagnostic can newly fail either msbuild gate, because MSTEST0032 is the only rule above
`suggestion` and the rewrite changes no assertion shape (the file uses FluentAssertions
`Should()` throughout, not MSTest `Assert`). MSTEST0032 is already suppressed narrowly and
deliberately elsewhere in the assembly with `#pragma warning disable MSTEST0032` /
`restore MSTEST0032` at `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:698` and `:700`; the
rewrite touches nothing related.

Regarding AsyncFixer specifically (`.editorconfig:537-543`, all six IDs at `suggestion`): the
rewritten method genuinely awaits, so the "async method without await" family does not apply. The
retained `invokeAsyncTask.GetAwaiter().GetResult()` at the current `:65` is a synchronous block inside
an `async` method and could attract a blocking-call suggestion; it is preserved verbatim because AC-4
requires the assertion behavior to be unchanged, and at `suggestion` severity it cannot fail a gate.
Converting it to `await` would change the `BeginInvoke` observation ordering and is out of scope.

---

## 5. Fail-before feasibility

### 5.1 A deterministic red run is structurally impossible without out-of-scope scaffolding

Four independent reasons:

1. **There is no unit under test.** The defect lives entirely inside a test method body. The
   production type `WpfUiDispatcher` is correct; nothing in `UtilitiesCS` changes. There is no seam a
   second test could observe, because the offending read-modify-write is local to
   `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread`.
2. **The hazard requires an interleaving that cannot be forced.** Reproducing it needs
   `WpfUiDispatcherTests` and a gate-holding class to interleave their writes to the static. Under
   MSTest `Scope=ClassLevel`, `Workers=0` (`TaskMaster.runsettings:4-7`;
   `scripts/vscode/TaskMaster.cli.runsettings:4-7`) that interleaving is possible but not
   controllable. The fixture's own regression suite already documents this limitation for the
   equivalent case: `QfcItemController.UiThreadDispatcherFixtureTests.cs:17-21` records that R4 "fails
   only probabilistically, because nothing can force the second caller to reach its acquisition point
   while the first still holds the gate and there is no deterministic way to prove the second caller is
   currently blocked without a timed wait, which the repository's determinism rules forbid."
3. **CI cannot express it at all.** `.github/workflows/_mstest-coverage.yml:83` invokes
   `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
   with no `/Settings:` argument, so MSTest's default (no parallelization) applies and the race is
   dormant in the only run that gates merge.
4. **The one deterministic alternative is excluded by AC-6.** A source-scanning guard test asserting
   AC-1 is technically feasible in this assembly — the technique already exists at
   `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:49-50` (`ReadControllerSource` →
   `File.ReadAllText(ResolveRepositoryPath(...))` with the walk-up resolver at `:52-57`). Adding it
   would create a second changed `.cs` path, which AC-6 forbids ("changes exactly one path with a
   `.cs` extension"). It is therefore out of scope for this issue, and is at most a follow-up.

**Conclusion: plan for a `fail-before-exception` dossier, not a red run.**

### 5.2 Required dossier contents

Path: `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/regression-testing/fail-before-exception.<timestamp>.md`,
per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:137-140`. Schema-validity per that
skill requires (`:108-111`, `:133-136`): `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`WhyFailingRunImpossible: <1-3 sentences>` plus an alternative proof section. The archived exemplar
`docs/features/archive/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/regression-testing/fail-before-exception.runasync-wiring.2026-08-06T23-37.md`
shows the accepted layout: header block, `WhyFailingRunImpossible:` at `:10`, an
"Absence-of-seam proof" section at `:12`, a "Search performed for an existing failing run" section
carrying `SearchScope:` / `SearchPatterns:` / `SearchResult:` at `:34-38`, and a pointer to the
authoritative substitute evidence at `:47-59`.

The absence proof for #648 is a **no-unit-under-test / uncontrollable-interleaving proof**, not an
absence-of-seam proof. It should state:

- The defect's whole extent is `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:42-85`; no
  production line changes, so no production behavior can be asserted to differ before and after.
- The interleaving argument, quoting `QfcItemController.UiThreadDispatcherFixtureTests.cs:17-21`.
- The CI-dormancy argument, quoting `.github/workflows/_mstest-coverage.yml:83`.
- The AC-6 exclusion of a source-scanning guard test, naming
  `QfcFormControllerSeamTests.cs:49-50` as the technique that exists but is out of scope.
- Negative-claim fields per `SKILL.md:145-153`: `SearchScope:` the feature's
  `evidence/regression-testing/` folder, `SearchPatterns: fail-before-exception.*.md`,
  `SearchResult:` the result.

Non-red evidence that substitutes for the failing run:

1. **Structural before/after counts** (AC-1, AC-2). Record the two-line baseline for `"_dispatcher"`
   beneath `QuickFiler.Test/` and the one-line post-change result, plus the zero-occurrence result for
   `GetField`, `SetValue`, and `using System.Reflection;` in the target file. Use two independent
   search methods, as the issue's own AC preamble requires.
2. **Behavior-preservation runs** (AC-4, AC-5). A scoped `WpfUiDispatcherTests` run and a full
   `QuickFiler.Test.dll` run, both zero-failed, the full run's passed count no lower than the Phase 0
   baseline under `evidence/baseline/`.
3. **A run under the repo runsettings.** The issue's § Proposed Fix / Validation Ideas asks for "a run
   under the repo runsettings (`ClassLevel`, `Workers=0`) to exercise concurrent classes". This is
   non-deterministic evidence and must be labelled as such — a green run does not prove the race is
   gone, only that the gated path is stable under the parallel scope.
4. **Inherited regression coverage.** The six #493 tests R1-R6
   (`QfcItemController.UiThreadDispatcherFixtureTests.cs:40-344`) are the authoritative
   fail-before/pass-after evidence for the underlying clobber mechanism. #648 extends the already
   proven protocol to one more call site; it does not introduce a new mechanism needing its own
   fail-before pair. Cite them the way the #424 dossier cites its authoritative pair at `:47-57`.
5. **Scope-boundary diff** (AC-6). The `git diff --name-only` against `origin/main` at
   `2b85134b42872e405602e6064e02dc9cda6c319b`, showing one `.cs` path and no path under
   `UtilitiesCS.Test/` or `UtilitiesCS/`.

---

## 6. Test-run commands for this assembly

### 6.1 Repo-standard local invocation (no coverage)

```
pwsh -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test -Configuration Debug
```

The script resolves `vstest.console.exe` through `vswhere` (`scripts/vscode/Invoke-MSTest.ps1:97-105`)
and builds the argument list at `:54`:

```
@($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation', '/TestCaseFilter:TestCategory!=LiveOutlook')
```

so all three of `/Settings:`, `/InIsolation`, and `/TestCaseFilter:TestCategory!=LiveOutlook` are used.

**`/Settings:` points at the off-root CLI runsettings, not the repo-root file.**
`Resolve-RunSettingsPath` returns `Join-Path $ScriptRoot 'TaskMaster.cli.runsettings'` (`:29`), i.e.
`scripts/vscode/TaskMaster.cli.runsettings`. The `.DESCRIPTION` for `Get-VsTestArgumentList` at
`:42-44` still says "the repo-root TaskMaster.runsettings"; that docstring is stale relative to `:29`.
The two files carry the same `Workers=0` / `Scope=ClassLevel` block; only the repo-root file also
carries the Code Coverage data collector (`TaskMaster.runsettings:9-29`), which is why the CLI path
exists (`:17-22`).

### 6.2 Local invocation with coverage

```
pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test
```

Argument list at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:70-77`:

```
collect --output <out> --output-format cobertura --settings <derived coverage.config> -- <vstest> <assemblies> /Settings:<cli.runsettings> /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook
```

The outer `dotnet-coverage --settings` is the instrumentation-exclude file derived from the repo-root
`coverage.config` (`:198-231`, resolved at `:320`); the inner `/Settings:` is again the CLI runsettings
(`:33`). Requires the `dotnet-coverage` global tool (guard at `:292-294`).

`CLAUDE.md` §"C# Toolchain" step 4 gives the raw form
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`; the scripts above are the concrete
repo-standard wrappers around it.

### 6.3 CI

`.github/workflows/_mstest-coverage.yml:83`:

```
& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"
```

Differences from local: **no `/Settings:` at all** (so MSTest default sequential execution, which is
why the #648 race is dormant in CI), `/EnableCodeCoverage` instead of the `dotnet-coverage` wrapper,
and `/Logger:trx`. Build is `msbuild ... /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
(`:50-51`); the job ceiling is `timeout-minutes: 30` (`:14`).

### 6.4 Running only `WpfUiDispatcherTests`

`vstest.console.exe` accepts a single `/TestCaseFilter:`, so the class restriction must be combined
with the category exclusion rather than added as a second switch:

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll ^
  /Settings:scripts\vscode\TaskMaster.cli.runsettings ^
  /InIsolation ^
  /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~WpfUiDispatcherTests"
```

That selects both members of the class:
`QuickFiler.Controllers.Tests.WpfUiDispatcherTests.Construction_YieldsAnIUiDispatcher`
(`WpfUiDispatcherTests.cs:24`) and
`...WpfUiDispatcherTests.Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread` (`:39`),
which is what AC-5 requires ("both of that class's tests passing"). Note `FullyQualifiedName~` is a
substring match; `ClassName=WpfUiDispatcherTests` is the exact-match alternative.

### 6.5 Worktree-exclusion caveat

Neither discovery routine excludes worktree paths. `scripts/vscode/Invoke-MSTest.ps1:107-113` and
`.github/workflows/_mstest-coverage.yml:70-76` filter only on `\bin\<Configuration>\`, `\obj\` and
`\ref\`.

For this checkout the risk is contained but conditional:

- `Invoke-MSTest.ps1:88` sets `$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path`, so
  when the script is run from a worktree it roots discovery at that worktree.
- A glob of `.claude/worktrees/*` inside this checkout returned no files, so recursion from this
  worktree root cannot reach a sibling worktree.
- Running the same script from the **main clone** root would recurse into
  `.claude/worktrees/*/**/bin/Debug/*.Test.dll` and sweep in every active worktree's assemblies.

Mitigation for evidence runs: always pass `-SearchRoot QuickFiler.Test` (both scripts accept it —
`Invoke-MSTest.ps1:2-3`, `Invoke-MSTestWithCoverage.ps1:2-3`) so the assembly set is bounded to one
project, or name the single assembly path explicitly as in §6.4. Record the explicit assembly path in
the evidence artifact's `Command:` field so the run is reproducible and auditable.

---

## 7. Summary of design decisions

| # | Decision |
| --- | --- |
| 1 | Method becomes `public async Task`, decorated `[TestMethod]` + `[Timeout(GateTimeoutMs)]`, with nested `try`/`finally` mirroring R1. No `using` declaration. |
| 2 | Teardown: `transaction.Dispose()` in the inner `finally`, `ShutdownDispatcher(dispatcher)` in the outer `finally`. |
| 3 | `[Timeout]` is required; `private const int GateTimeoutMs = 60000;` matches the fixture-test convention. No repo-wide MSTest timeout exists. |
| 4 | Remove `using System.Reflection;` (AC-2) and, discretionarily, `using UtilitiesCS;`. Write `.ConfigureAwait(false)` to match consumers. No enforceable diagnostic is introduced. |
| 5 | No deterministic red run is possible; produce a schema-valid `fail-before-exception` dossier plus the structural and behavior-preservation evidence listed in §5.2. |
| 6 | Use `scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test` locally; scope the class with a single combined `/TestCaseFilter:`; bound discovery so sibling worktrees are never swept in. |

## 8. Open item for the planner (not a scope expansion)

`CLAUDE.md` §C#1.2 lists `.globalconfig` as an analyzer-configuration input, but no `.globalconfig`
exists in the tree. This does not affect #648 — `.editorconfig` alone determines that no diagnostic in
this rewrite can exceed `suggestion` — but the toolchain documentation and the tree disagree. Record
it as an observation; do not act on it inside this issue.
