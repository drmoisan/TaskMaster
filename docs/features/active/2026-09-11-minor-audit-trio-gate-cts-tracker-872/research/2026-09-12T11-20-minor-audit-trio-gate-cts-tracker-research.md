# Research — Issue #872, minor-audit trio (gate log assertion, CTS disposal, dormant tracker)

- Timestamp: 2026-09-12T11-20
- Feature folder: `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872`
- Scope: research only. No production file was modified.
- Repository state: branch `TaskMaster-wt-2026-09-12T10-15`, clean at session start, HEAD `2405a829d`.

All line references below are to the working tree at that commit.

---

## Headline scope findings (read first)

Two findings contradict or qualify the `issue.md` Write Set. Both are stated in full in
section 5.

1. **SF-1 (blocking for AC3 as literally worded).** Making `ProgressPackage` implement
   `IDisposable` does **not** close the leak on the two static tuple-factory paths or on
   `BayesianPerformanceMeasurement.LoadIfNullAsync`. Those paths construct an owned source and
   then either discard the package or hand the package to a caller that never disposes it.
   Closing them requires edits to seven call sites in four files that are **outside** the
   Write Set. Recommended resolution: keep the Write Set as-is, implement the ownership
   mechanism, and re-word / interpret AC3 as a capability criterion, then promote the
   residual call-site leak as a follow-up issue.
2. **SF-2 (favourable).** No change to `UtilitiesCS/Threading/ProgressTracker.cs` or
   `UtilitiesCS/Threading/ProgressTrackerPane.cs` is required. Both downstream viewers already
   tolerate a disposed source explicitly. The Write Set holds for Defect B on that axis.

---

## 1. CancellationTokenSource ownership in `UtilitiesCS/Threading/ProgressPackage.cs`

### 1.a — Enumeration of escape paths

**Direct answer.** A source constructed inside either `InitializeAsync` overload escapes the
`ProgressPackage` instance by seven distinct routes. Five of them produce a reference that
outlives the package under at least one current caller; two of them produce a reference that
*always* outlives the package.

Construction sites (the only two places the class creates a source):

- `UtilitiesCS/Threading/ProgressPackage.cs:25` — `_cancelSource = cancelSource ?? new CancellationTokenSource();` (tracker overload)
- `UtilitiesCS/Threading/ProgressPackage.cs:40` — same expression (pane overload)

| # | Escape route | Evidence | Does the escaping reference outlive the package? |
|---|---|---|---|
| E1 | Into a `ProgressTracker` the package constructs | `ProgressPackage.cs:27-28` -> `ProgressTracker.cs:22` (field `_cancelSource`) -> `ProgressTracker.cs:40` (`_progressViewer.CancelSource = _cancelSource`) | Yes on the tuple-factory paths (E6), because the tracker is returned in the tuple while the package is discarded. No on the `InitializeAsync` paths, where the package holds the tracker. |
| E2 | Into a `ProgressTrackerPane` the package constructs | `ProgressPackage.cs:42` -> `ProgressTrackerPane.cs:11-18` -> `ProgressPane.SetCancellationTokenSource` at `ProgressTrackerPane.cs:17` and `ProgressPane.cs:50-52` | Same as E1. Note: in every production pane call site a non-null `progressTrackerPane` is injected, so this constructor does not run and the source never reaches a `ProgressPane`. |
| E3 | Public `CancelSource` getter | `ProgressPackage.cs:83-87` | Depends on the reader. Read at `MulticlassEngine.cs:132,150`, `CategoryClassifierGroup.cs:111,128`, `OlFolderClassifierGroup.cs:282`, `BayesianPerformanceMeasurement.cs:216`. |
| E4 | `SpawnChild` copies the reference into a new package | `ProgressPackage.cs:122` | Yes. Children are passed as method arguments and outlive the expression. Production sites: `BayesianPerformanceMeasurement.cs:72, 74, 79, 121, 123, 128, 182, 241, 346, 1064`. |
| E5 | `ToTuple()` / `ToTuplePane()` | `ProgressPackage.cs:137`, `ProgressPackage.cs:147` | Yes when called by E6. Directly callable by any holder. |
| E6 | `CreateAsTupleAsync` / `CreateAsTuplePaneAsync` construct the package locally, return the tuple, discard the package | `ProgressPackage.cs:60-63`, `ProgressPackage.cs:77-79` | **Always.** The package is unreachable the moment the method returns; the source is not. |
| E7 | `BayesianPerformanceMeasurement.LoadIfNullAsync` constructs an *owned* package and returns it to its caller inside a tuple | `BayesianPerformanceMeasurement.cs:1351-1353` and `:1403-1405`; consumed at `:65-69` and `:160-165` | **Always.** The package and its owned source both outlive the constructing method and no caller disposes either. |

Complete enumeration of the fourteen `.cs` files that mention `ProgressPackage` or
`CreateAsTuple`, with the ownership classification of each construction:

Search scope: whole working tree. Pattern: `ProgressPackage|CreateAsTuple`, glob `*.cs`,
no head limit. Result: 14 distinct files.

Production (7):

1. `UtilitiesCS/Threading/ProgressPackage.cs` — the class itself.
2. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs:38-40`,
   `:196-198`, `:320-322` — `CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker)`.
   No source supplied, so a source is **constructed and owned**, and it is then **discarded
   into a `_` discard** while the token derived from it is used. Leak, three sites.
3. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs:129` —
   `CreateAsTupleAsync()` with no arguments. Source constructed and owned; captured as
   `tokenSource` and never disposed. Leak. This is also the only site that reaches E1 with a
   real `ProgressTracker.Initialize()`.
4. `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs:237-239`
   (tuple factory, leak) and `:280-287` (`InitializeAsync` with `ppkg.CancelSource` injected —
   not owned).
5. `UtilitiesCS/EmailIntelligence/ClassifierGroups/MulticlassEngine.cs:249-251` (tuple factory,
   leak) and `:130-137`, `:148-155` (injected, not owned).
6. `UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs:180-182`
   (tuple factory, leak) and `:109-116`, `:126-133` (injected, not owned).
7. `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianPerformanceMeasurement.cs:215-220`
   (injected, not owned), `:1351-1353` and `:1403-1405` (**owned, escapes via E7**).
8. `UtilitiesCS/EmailIntelligence/ClassifierGroups/Actionable/ActionableClassifierGroup.cs:61` —
   parameter only, no construction.
9. `QuickFiler/Controllers/BayesianPerformanceController.cs:58-62` — `InitializeAsync` with
   `cancelSource: _globals.AF.CancelSource`. **Injected, must never be disposed**: that property
   is add-in-lifetime state declared at `TaskMaster/AppGlobals/AppAutoFileObjects.cs:643`
   (`public CancellationTokenSource CancelSource { get; private set; } = new();`) and surfaced by
   `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs:62`. This is the strongest argument
   for a per-instance ownership flag rather than blanket disposal.

(The count of 9 above collapses the tally to 9 production files because
`ActionableClassifierGroup.cs` and `BayesianPerformanceController.cs` are counted separately;
the remaining five of the fourteen are test files.)

Test (5): `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`,
`UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/MulticlassEngine_Tests.cs`,
`UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs`,
`UtilitiesCS.Test/EmailIntelligence/ActionableClassifierGroup_Tests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs`.
Every test construction uses an object initializer (`new ProgressPackage { CancelSource = ... }`)
or supplies a caller source, so none of them creates an owned source today.

### 1.b — Does `IDisposable` on `ProgressPackage` actually close the defect?

**Direct answer: no, not on the tuple-factory paths, and not on E7.** State this plainly in the
plan.

- `CreateAsTupleAsync` (`ProgressPackage.cs:60-63`) and `CreateAsTuplePaneAsync`
  (`ProgressPackage.cs:77-79`) construct `var package = new ProgressPackage();`, initialize it,
  return `package.ToTuple()` / `package.ToTuplePane()`, and never retain the package. Adding
  `IDisposable` gives that local package a `Dispose` method that nothing calls, and the source it
  owns is deliberately in the returned tuple. Nothing is closed.
- `LoadIfNullAsync` (`BayesianPerformanceMeasurement.cs:1351`, `:1403`) is the same shape at one
  remove: the *package* escapes with its owned source, and the consumers at `:65-69` and
  `:160-165` use it and discard it.
- Consequence: after the change, the seven leaking sites remain
  (`EmailDataMiner.Transform.cs:38, 196, 320`, `EmailDataMiner.FolderExtraction.cs:129`,
  `OlFolderClassifierGroup.cs:237`, `MulticlassEngine.cs:249`, `CategoryClassifierGroup.cs:180`)
  plus the two `LoadIfNullAsync` sites. None of the four owning files is in the Write Set.

**Testing the potential record's "flag set in the constructor branch" suggestion against the
escape paths.** The suggestion is *necessary and correct as far as it goes*, and it is the only
mechanism that is safe against E3/E4:

- It correctly refuses to dispose `_globals.AF.CancelSource`
  (`BayesianPerformanceController.cs:59`) and every `ppkg.CancelSource` hand-down
  (`MulticlassEngine.cs:132,150`, `CategoryClassifierGroup.cs:111,128`,
  `OlFolderClassifierGroup.cs:282`, `BayesianPerformanceMeasurement.cs:216`).
- It is **insufficient by itself** because ownership state without a release trigger releases
  nothing. The flag must be paired with a `Dispose()` and with a caller that invokes it.
- One implementation constraint the suggestion does not state: the flag must be assigned **only**
  at the `?? new CancellationTokenSource()` site inside `InitializeAsync`, and **never** in the
  public `CancelSource` setter (`ProgressPackage.cs:86`). `SpawnChild` (`ProgressPackage.cs:120-127`)
  assigns through that setter; if the setter set the flag, every child would claim ownership of
  its parent's source. See 1.c.
- Second constraint: `_ownsCancelSource = cancelSource is null;` (an assignment, not a
  conditional set) is required so that a second `InitializeAsync` call with an injected source
  clears a previously-claimed ownership. Double initialization is not reachable in production
  today — every production site is `new ProgressPackage().InitializeAsync(...)` — but the
  assignment form is free and makes AC3's word "every" literally true for the first source only.
  A guard that disposes a previously-owned source before overwriting it would make "every"
  literally true in all cases; it adds one branch that AC12 then has to cover, so it is optional.

**Recommended mechanism (single recommendation).**

1. Add `private bool _ownsCancelSource;` to `ProgressPackage`.
2. At `ProgressPackage.cs:25` and `:40`, immediately alongside the null-coalescing construction,
   set `_ownsCancelSource = cancelSource is null;`.
3. Implement `public void Dispose()` that releases only when `_ownsCancelSource` is true, clears
   the flag, and does **not** null `_cancelSource` (nulling would change the observable behaviour
   of the public `CancelSource` getter for existing callers and would also remove the only
   deterministic probe the AC4 tests can use).
4. Declare `ProgressPackage : IDisposable`.
5. Add an XML doc comment on both static tuple factories stating that the returned source is
   transferred to the caller and that the caller owns its release. This is the only in-Write-Set
   way to record the residual.

Rejected alternatives, briefly:

- *Dispose the package inside the tuple factories.* Rejected: it would dispose the source the
  factory is contractually returning, and `issue.md` lines 43-44 forbid disturbing that
  behaviour.
- *Suppress construction of a source when the caller supplies none.* Rejected: `ProgressTracker`'s
  and `ProgressTrackerPane`'s constructors take a non-nullable source
  (`ProgressTracker.cs:20`, `ProgressTrackerPane.cs:11`) and `_cancel` is derived from it at
  `ProgressPackage.cs:26`/`:41`. This is a behaviour change with unbounded blast radius.
- *A finalizer on `ProgressPackage` that disposes the owned source.* Rejected: it makes the
  release GC-timing-dependent, it resurrects the same class of non-determinism the repo has
  previously rejected, and it cannot be asserted deterministically in a unit test.

### 1.c — What `SpawnChild` must do

**Direct answer.** `SpawnChild` must do nothing new. The rule is: *a package owns a source only
if it constructed that source inside its own `InitializeAsync`; ownership is never transferred by
assignment.* That rule is expressible with exactly the same per-instance boolean as the parent,
and it is satisfied by construction provided the flag is set only at the two `InitializeAsync`
construction sites and not in the `CancelSource` property setter.

Evidence. `SpawnChild` at `ProgressPackage.cs:118-128` builds the child with an object
initializer:

```
CancelSource = this.CancelSource,
```

That runs the public setter at `ProgressPackage.cs:86`, not `InitializeAsync`. A field-initialized
`bool` defaults to `false`, so the child's `_ownsCancelSource` is `false` and a child `Dispose()`
is a no-op on the source. The parent alone can release it.

A child's own `_progressTracker` / `_progressTrackerPane` are fresh objects produced by
`this.ProgressTracker?.SpawnChild(allocation)` / `this.ProgressTrackerPane?.SpawnChild(allocation)`
(`ProgressPackage.cs:125-126`). Those child trackers do not receive the source at all:
`ProgressTracker(ProgressTracker parent, int, int)` at `ProgressTracker.cs:60-69` never assigns
`_cancelSource`, and `ProgressTrackerPane(ProgressTrackerPane parent, int, int)` at
`ProgressTrackerPane.cs:39-48` has no source parameter. So `SpawnChild` adds no new escape of the
source beyond the copied reference itself.

Production `SpawnChild` call sites on `ProgressPackage` (as opposed to on the trackers) are all in
one file: `BayesianPerformanceMeasurement.cs:72, 74, 79, 121, 123, 128, 182, 241, 346, 1064`.

### 1.d — Must `ProgressTracker` or `ProgressTrackerPane` change?

**Direct answer: no. Neither file needs to change, and the Write Set is correct as written on
this point.** This is favourable scope finding SF-2.

Does either hold the source past the package's lifetime?

- `ProgressTracker` holds it in a field: `ProgressTracker.cs:80` (`private CancellationTokenSource? _cancelSource;`),
  assigned at `:22` and `:27`. On the tuple-factory paths the tracker does outlive the package.
- `ProgressTrackerPane` does **not** hold it at all. The constructor parameter at
  `ProgressTrackerPane.cs:11` is forwarded to the pane at `:17` and then goes out of scope. There
  is no field.

Would either observe an `ObjectDisposedException` if the package released the source?

- `ProgressTracker` itself: no. `_cancelSource` is read at exactly one place after assignment —
  `ProgressTracker.cs:40`, inside `Initialize()` — and never again anywhere else in the file. A
  disposal after `Initialize()` is unobservable to the tracker.
- `ProgressViewer` (the real holder on the tracker path): already handles it.
  `UtilitiesCS/Threading/ProgressViewer.cs:82-101` wraps `source.Cancel()` in
  `try { ... } catch (ObjectDisposedException) { logger.Debug(...); }`, and the comment at
  `:76-81` states the contract explicitly: *"This viewer borrows the source and never disposes
  it; a different holder owns disposal... A disposed source is a lifecycle race rather than a
  defect."*
- `ProgressPane` (the real holder on the pane path): identical treatment at
  `UtilitiesCS/Threading/ProgressPane.cs:66-84`, same `catch (ObjectDisposedException)` and the
  same comment at `:63-65`.

Residual hazard to record in the plan, not to fix here: a `CancellationToken` captured from a
source that is later disposed remains safe for `IsCancellationRequested` and
`ThrowIfCancellationRequested`, but `token.Register(...)` and `token.WaitHandle` throw
`ObjectDisposedException`, and framework APIs that accept a token (for example `Task.Delay`)
register internally. Because no production caller invokes `ProgressPackage.Dispose()` after this
change, the hazard is latent and is not triggered by this delivery. Any future work that adds a
`Dispose` call at one of the seven leaking call sites must first prove the token is no longer in
flight.

### 1.e — Language version and available disposal syntaxes

**Direct answer.** Both projects target `net481` and both compile at a C# version that supports
`using` declarations, `await using`, ref-struct-free `IAsyncDisposable` patterns, and the
null-coalescing forms already used in the file.

- `UtilitiesCS/UtilitiesCS.csproj:10` — `<LangVersion>12.0</LangVersion>`;
  `UtilitiesCS.csproj:16` — `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj:18` — `<LangVersion>Latest</LangVersion>`;
  `UtilitiesCS.Test.csproj:17` — `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`.
- `Directory.Build.props` exists at the repository root but sets only
  `RxUseUnsupportedPackagesConfig` (`Directory.Build.props:15-17`). It does not set
  `LangVersion`, `Nullable`, or `GenerateDocumentationFile`.

**`using` declarations are available in the production project as well as the test project.**
The test file's existing use at `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs:17, 44, 68, 93`
is not a test-project-only capability: the production project already uses the same syntax at
`UtilitiesCS/Threading/TimeOutTask.cs:52, 55, 119, 120, 199, 202, 274, 275, 358, 359, 436, 437`.

Framework caveat for `net481`: `init` accessors, `record` and `record struct` are unavailable
(no `IsExternalInit`), which is why `QfcGateBatch` is a `readonly struct`
(`QfcStreamingDequeueConfidenceGate.cs:14-17`). That restriction does not affect any disposal
syntax.

Analyzer environment relevant to adding `IDisposable`:

- Neither project references `Microsoft.CodeAnalysis.NetAnalyzers`. The analyzer sets present are
  Meziantou, Roslynator, AsyncFixer, BannedApiAnalyzers, SonarAnalyzer, and (test project only)
  MSTest.Analyzers — `UtilitiesCS.csproj:1308-1317`, `UtilitiesCS.Test.csproj:953-985`.
- `.editorconfig:27` sets `dotnet_analyzer_diagnostic.severity = suggestion` as a catch-all, and
  `.editorconfig:29` (`dotnet_diagnostic.MSTEST0032.severity = warning`) is the **only** rule
  raised above suggestion. Search scope: `.editorconfig`, pattern `severity = (error|warning)`,
  one hit.
- Therefore `CA1063`, `CA2000` and the Sonar disposal rules cannot break the
  `/p:TreatWarningsAsErrors=true` gate. Existing test code that creates a `ProgressPackage`
  without disposing it will not start failing the build.
- Neither project sets `GenerateDocumentationFile` or `DocumentationFile` (search scope:
  `UtilitiesCS*/*.csproj`, pattern `GenerateDocumentationFile|DocumentationFile`, zero hits), so
  XML doc comment diagnostics such as `CS1574` are not emitted at all.

### 1.f — Shape the AC4 tests must take (headless constraint)

This is not a numbered question but the tests cannot be written without it.

`InitializeAsync` will only *construct* a source when `cancelSource` is null. Passing null
`cancelSource` on the tracker overload while leaving `progressTracker` null would run
`new ProgressTracker(_cancelSource, screen).Initialize()` (`ProgressPackage.cs:27-28`), and
`ProgressTracker.Initialize()` touches `UiThread.Dispatcher` and constructs a WinForms
`ProgressViewer` (`ProgressTracker.cs:33-56`). That is host-bound.

The headless-safe arrangement is to pass `cancelSource: null` **and** a non-null
`progressTracker`, so the `??` at `:27` short-circuits:

- `new ProgressTracker(someCts)` alone performs no UI work — the constructor at
  `ProgressTracker.cs:20-23` only assigns a field. The existing passing test at
  `ProgressPackage_Tests.cs:18` already relies on this.
- Naming `progressTracker:` disambiguates the two `InitializeAsync` overloads, which would
  otherwise be ambiguous for a null third argument.
- Supply `stopWatch:` as well to avoid the `await Task.Run(...)` at `:29`.
- Do **not** use the pane overload for these tests: `new ProgressTrackerPane(cts)` calls
  `UiThread.Dispatcher.Invoke` and shows a `ProgressPane` (`ProgressTrackerPane.cs:13-36`).

Deterministic assertions available, requiring no timer inspection:

- Released: the captured source's `Token` getter throws `ObjectDisposedException` after
  `Dispose()`.
- Left usable (injected case): the injected source's `Token` is still readable and `Cancel()`
  still sets `IsCancellationRequested`.
- Child case: `parent` owns; `parent.SpawnChild(25).Dispose()`; the parent's source `Token` is
  still readable.

---

## 2. Disposal in `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`

### 2.a — Last use of the token and of the tracker that holds the source

**Direct answer.** The token's last use is line 235 (the `token` argument to
`Task.Factory.StartNew`). The tracker's last use is line 240 (`progress.Report(100)`). Release is
therefore safe at any point after line 240 and must not be earlier than line 240.

Evidence — `RebuildAsync`, `SubjectMapSco.Orchestration.cs:221-241`:

- `:228` `var tokenSource = new CancellationTokenSource();`
- `:229` `var token = tokenSource.Token;`
- `:230` `var progress = new ProgressTracker(tokenSource).Initialize();` — the tracker takes the
  source (`ProgressTracker.cs:22`) and `Initialize()` hands it to the `ProgressViewer`
  (`ProgressTracker.cs:40`).
- `:232-238` `await Task.Factory.StartNew(RebuildCore, Tuple.Create(appGlobals, progress), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);`
- `:240` `progress.Report(100);`

`RebuildCore` (`:243-262`) receives only `Tuple<IApplicationGlobals, ProgressTracker>` and never
touches the token or the source. Confirmed by reading the whole method: no `token`, no
`tokenSource`, no `CancellationToken` reference appears between `:243` and `:262`.

`progress.Report(100)` at `:240` matters because `ProgressTracker.Report(double)` at
`ProgressTracker.cs:141-178` closes the root `ProgressViewer` when the value reaches 100
(`:163-175`). After that call the viewer that holds the source has been closed, so disposal of the
source afterwards is not merely safe but is the natural point.

### 2.b — Correct release shape across the faulting path

**Direct answer.** Convert line 228 to a `using` declaration:

`using var tokenSource = new CancellationTokenSource();`

That is the correct shape for this method, and it is the recommendation.

Rationale:

- A `using` declaration in an `async` method disposes at the end of the enclosing scope on the
  normal path **and** when an exception propagates out of `RebuildAsync`, which covers the faulting
  `await Task.Factory.StartNew(...)` at `:232-238`.
- It does not swallow the exception: a `using` declaration lowers to `try/finally` with no `catch`,
  so `TaskCanceledException`, `AggregateException` unwrapping, or any fault from `RebuildCore`
  propagates unchanged to the caller.
- Because the declaration is at `:228` and the method body ends after `:240`, the release point is
  strictly after the last use of both the token and the tracker, satisfying AC5's ordering clause.
- A `try { ... } finally { tokenSource.Dispose(); }` block is behaviourally identical and would
  also satisfy AC5, but it adds two levels of brace nesting over a 13-line method with no
  compensating benefit. The `using` declaration also matches existing production style in this
  assembly (`UtilitiesCS/Threading/TimeOutTask.cs:52, 119, 199, 274, 358, 436`).

What must **not** be done: `tokenSource.Dispose()` placed between `:238` and `:240`, or any
placement that precedes `progress.Report(100)`. That would dispose the source while the root
`ProgressViewer` is still open and still exposes an enabled Cancel button. It would not crash
(`ProgressViewer.cs:95` catches `ObjectDisposedException`) but it would silently make the Cancel
button inert, which is a behaviour regression.

Language availability: `using` declarations are available in `UtilitiesCS` (LangVersion 12.0 —
`UtilitiesCS.csproj:10`), see 1.e.

### 2.c — The `ExcludeFromCodeCoverage` attribute and its consequence for verification

**Confirmed from the file.** `[ExcludeFromCodeCoverage]` sits on `RebuildAsync` at
`SubjectMapSco.Orchestration.cs:221`, immediately above the signature at `:222`. The attribute is
imported via `using System.Diagnostics.CodeAnalysis;` at `:5`. The same attribute is also on
`CreateFolderHandleResolver` (`:54`), `ShowSummaryMetrics()` (`:130`) and `RebuildCore` (`:243`).

Consequence for verification:

- An excluded member emits **no** `<method>` element in the Cobertura report. It does not appear
  at 0% — it is absent from the file's method list entirely. A per-file coverage delta on
  `SubjectMapSco.Orchestration.cs` therefore cannot discriminate whether the AC5 change landed.
  `issue.md:132-136` already states this; this research confirms it from the source.
- AC5 must be verified structurally (the `using` declaration is present at the construction site,
  and `progress.Report(100)` still precedes the end of scope) and by the two `msbuild /t:Rebuild`
  gates. It carries no test obligation, consistent with `issue.md:137-138`: `RebuildAsync` installs
  a `WindowsFormsSynchronizationContext` at `:224-227` and starts a `LongRunning` task, so it is
  not unit-testable without a host.
- Practical note for the executor: the change is a single-token edit on line 228. A `git diff`
  hunk of exactly that line is the strongest available structural evidence.

---

## 3. Deleting the dormant type

### 3.a — Exhaustive reference search for `ProgressTrackerAsync`

**Direct answer.** There is no reference to `ProgressTrackerAsync` in compiled code outside
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, the type's own declaration file, and
the two `<Compile Include>` items. There is no `see cref` or `seealso cref` reference to it
anywhere in the tree.

Search 1 — compiled C# sources.
- Scope: whole working tree. Glob: `*.cs`. Pattern: `ProgressTrackerAsync`. Head limit: 0.
- Result — every hit, with file and line:

| File | Line | Nature |
|---|---|---|
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 14 | `public class ProgressTrackerAsync` (declaration) |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 20 | constructor `(CancellationTokenSource)` |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 25 | constructor `(CancellationTokenSource, Screen?)` |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 31 | `public async Task<ProgressTrackerAsync> InitializeAsync()` |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 14 | test class name |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 21, 32, 42, 52, 71, 89, 112, 115, 145, 165 | construction sites |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 136 | `<c>async Task&lt;ProgressTrackerAsync&gt;</c>` inside an XML doc comment |
| `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | 208 | `asynchronous sibling on <c>ProgressTrackerAsync</c>` — prose inside a `<c>` tag, **not** a `cref`, **not** compiled |

Search 2 — cref-form doc references.
- Scope: whole working tree, all file types. Pattern: `cref\s*=\s*"[^"]*ProgressTrackerAsync`.
- Result: **no matches**. There is no `see cref` or `seealso cref` naming the type, so the
  `TreatWarningsAsErrors` concern raised in the research brief does not materialise. As an
  independent second line of defence, neither owning project sets `GenerateDocumentationFile` or
  `DocumentationFile` (search scope `UtilitiesCS*/*.csproj`, pattern
  `GenerateDocumentationFile|DocumentationFile`, zero hits), so `CS1574` cannot be emitted in the
  first place.

Search 3 — non-source carriers (project files, config, generated reports).
- Scope: whole working tree excluding `docs/**`. Pattern: `ProgressTrackerAsync`. Mode:
  files_with_matches, head limit 0.
- Result — 9 files:
  1. `UtilitiesCS/UtilitiesCS.csproj` (Compile item, line 971)
  2. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (Compile item, line 496)
  3. `UtilitiesCS/Threading/ProgressTrackerAsync.cs`
  4. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
  5. `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` (the `<c>` prose at :208)
  6. `coverage_output.txt` (repo-root generated coverage summary; the line reads
     `UtilitiesCS\Threading\ProgressTrackerAsync.cs | 0.5`) — not compiled, not in the Write Set.
     It will become stale after the deletion. Flag it as an informational note only.
  7. `.claude/agent-memory/feature-review/project_584-review-residuals.md`
  8. `.claude/agent-memory/feature-review/project_449-review-residuals.md`
  9. `.claude/agent-memory/atomic-executor/project_preflight_moving_base_two_dot_diff_inertness_test.md`

  Items 7-9 are agent-memory markdown; per `CLAUDE.md` and the push-down ownership convention
  they are not edited by this delivery.

Conclusion: the deletion is safe. Nothing in compiled code outside the test file references the
type, and the `issue.md:49-52` claim about the `<c>` tag in
`ProgressTracker_ReportAndViewerTests.cs` is confirmed exactly as written.

### 3.b — Verbatim Compile-item lines

`UtilitiesCS/UtilitiesCS.csproj` line 971 (four leading spaces, self-closing, backslash path
separator):

```
    <Compile Include="Threading\ProgressTrackerAsync.cs" />
```

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 496 (four leading spaces, self-closing):

```
    <Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />
```

Neighbouring context, to make the removal unambiguous for the executor:

- `UtilitiesCS.csproj:970` is `    <Compile Include="NewtonsoftHelpers\AppGlobalsConverter.cs" />`
  and `:972` is `    <Compile Include="Threading\ProgressTrackerPane.cs" />`. Neither may be
  touched.
- `UtilitiesCS.Test.csproj:495` is `    <Compile Include="Threading\ProgressTrackerPane_Tests.cs" />`
  and `:497` is `    <Compile Include="Threading\TaskPriority_Tests.cs" />`. Neither may be touched.

### 3.c — Compile-item baseline counts for the AC7 check

**Direct answer.**

| Project file | Compile items at base commit | Expected after AC7 |
|---|---|---|
| `UtilitiesCS/UtilitiesCS.csproj` | **491** | 490 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | **476** | 475 |

Primary search strategy — literal element-plus-attribute prefix, counted per matching line:
- Pattern: `<Compile Include=`
- Scope: the single project file, `output_mode: count`.
- Result: `UtilitiesCS/UtilitiesCS.csproj` = 491; `UtilitiesCS.Test/UtilitiesCS.Test.csproj` = 476.

Cross-check search strategy — bare element-name prefix (deliberately wider, so it would also catch
`<Compile Update=`, `<Compile Remove=`, or any Compile element whose attribute is not `Include`):
- Pattern: `<Compile`
- Scope: the same two files, `output_mode: count`.
- Result: `UtilitiesCS/UtilitiesCS.csproj` = 491; `UtilitiesCS.Test/UtilitiesCS.Test.csproj` = 476.

Member-set comparison. The two strategies return identical counts for both files, which
establishes that every `Compile` element in both files uses the `Include` attribute and that no
`Update`/`Remove` form exists. `</Compile>` closing tags do not contain the substring `<Compile`
and so are not double-counted by either pattern.

Supplementary shape check (not a count source, recorded so the executor is not surprised by the
file's structure): the self-closing single-line form `<Compile Include="..." />` accounts for 448
of the 491 items in `UtilitiesCS.csproj` and 475 of the 476 items in `UtilitiesCS.Test.csproj`
(pattern `<Compile Include="[^"]*"\s*/>`). The remaining 43 and 1 items respectively are
multi-line elements carrying a child such as `<DependentUpon>` or `<SubType>` — for example
`UtilitiesCS.csproj:967-969`. **Both items to be deleted are of the single-line self-closing
form**, so each removal is a one-line deletion.

Reproduction command for the executor (repository-relative, using the same grep semantics):
count lines matching the literal `<Compile Include=` in each of the two project files, before and
after the edit; the difference must be exactly 1 in each file. Repeat with the bare `<Compile`
pattern as the cross-check.

### 3.d — Test-method count in the deleted test file

**Direct answer: 9.** The UtilitiesCS test assembly's total test count must fall by exactly 9
for AC11.

Primary count: pattern `^\s*\[TestMethod\]` (anchored, whitespace-tolerant) against
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` — 9 matches.
Cross-check: unanchored pattern `TestMethod` against the same file — 9 matches, which also
confirms there is no `[DataTestMethod]`, no `[TestMethod(...)]` with arguments, and no prose
occurrence of the word inflating the count.

The nine methods, by line:

| Line | Method |
|---|---|
| 17 | `Constructor_WithTokenSource_ShouldSetDefaultProperties` |
| 30 | `Allocation_ShouldBeSettable` |
| 40 | `StartingAt_ShouldBeSettable` |
| 50 | `JobName_ShouldBeSettable` |
| 65 | `Constructor_WithScreenOverload_HasSameDefaultsAsBasicConstructor` |
| 86 | `Tracker_SetAllocationAndJobName_BothPropertiesReflectUpdatedValues` |
| 107 | `ChildTracker_ConfiguredWithSubRange_AllocationAndStartingAtArePreserved` |
| 141 | `InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` |
| 158 | `InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker` |

There are no `[DataRow]` attributes in the file, so the executed-test count equals the
method count: 9 executed tests, not more.

One class-level attribute is also removed with the file: `[DoNotParallelize]` at line 13. It is
scoped to this class only and has no effect on any other test class.

---

## 4. Asserting the scan-bound log line

### 4.a — Exact signature of the reflection-based `CreateGate` helper

**Direct answer.** There are two overloads, both `private static object`, both declared in the base
partial file `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`. Both are
visible from Part4 because all four files declare
`public partial class QfcStreamingDequeueConfidenceGateTests` in namespace
`QuickFiler.Controllers.Tests`.

Overload A — the delegate-first form, at `QfcStreamingDequeueConfidenceGateTests.cs:27-43`:

```
private static object CreateGate(
    Func<MailItem> tryTakeNext,
    Func<MailItem, CancellationToken, Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>> scoreLoader,
    double threshold,
    TimeProvider timeProvider = null,
    Action<string> debugLog = null,
    Func<bool> sourceActive = null,
    TimeSpan? firstBatchDeadline = null,
    Action<int, int, int> progressCallback = null,
    Action<MailItem> onRejected = null,
    int? maxScanWithoutAcceptance = null,
    TimeSpan? zeroAcceptanceCeiling = null
)
```

Overload B — the queue-and-dictionary convenience form, at
`QfcStreamingDequeueConfidenceGateTests.cs:100-112`:

```
private static object CreateGate(
    Queue<MailItem> source,
    IDictionary<MailItem, long> scores,
    double threshold = 0.90,
    TimeProvider timeProvider = null,
    Action<string> debugLog = null,
    Func<bool> sourceActive = null,
    TimeSpan? firstBatchDeadline = null,
    Action<int, int, int> progressCallback = null,
    Action<MailItem> onRejected = null,
    int? maxScanWithoutAcceptance = null,
    TimeSpan? zeroAcceptanceCeiling = null
)
```

Overload B forwards to overload A at `:114-130`. Overload A resolves the internal type by name
(`:45-47`), performs one exact eleven-type constructor lookup (`:55-77`), asserts the constructor
is non-null (`:78-80`) so the helper fails closed, and invokes it (`:82-97`). The production
constructor it binds to is `QfcStreamingDequeueConfidenceGate.cs:136-152`; the parameter order and
names match one-for-one.

Supporting helpers the new tests will need, all already present:

- `DequeueBatchAsync(object gate, int quantity, int timeOut, CancellationToken token)` returning
  `Task<QfcGateBatch>` — `QfcStreamingDequeueConfidenceGateTests.cs:148-168`.
- `Scored(long score, string topFolder = "", IFolderSearchHandler handler = null)` returning
  `Task<(long, string, IFolderSearchHandler)>` — `QfcStreamingDequeueConfidenceGateTests.Part3.cs:32-36`.
- `BuildCandidates(int count)` returning `List<MailItem>` —
  `QfcStreamingDequeueConfidenceGateTests.Part4.cs:33-37`.
- `CreateMailItem(string subject, string entryId)` —
  `QfcStreamingDequeueConfidenceGateTests.cs:19-25`.

### 4.b — Exact literal text of the scan-bound line, and the substrings to assert

**Direct answer.** The line is produced by `LogScanBoundReached` at
`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:349-361`. The bound token is chosen at
`:351-352`:

```
string bound = scannedCount >= MaxScanWithoutAcceptance ? "scan-cap" : "zero-acceptance-ceiling";
```

and the message is composed at `:353-357`:

```
$"Zero-acceptance scan bound reached [QfcStreamingDequeueConfidenceGate.DequeueAsync] "
+ $"Accepted={acceptedCount} Scanned={scannedCount} Cutoff={_cutoff} "
+ $"Elapsed={elapsed} ScanCap={MaxScanWithoutAcceptance} "
+ $"Ceiling={ZeroAcceptanceCeiling} Bound={bound} Decision=stop"
```

It is emitted to the injected delegate at `:359` (`_debugLog?.Invoke(message)`) and to log4net at
`:360`. The delegate is the nullable field assigned from the constructor's `debugLog` parameter
(`:86`, `:158`), so passing no delegate produces no observable output — which is exactly why the
two existing bound tests observe nothing.

Concrete rendered lines for the two bounds, under the parameter values given in 4.c:

Item-cap bound (`maxScanWithoutAcceptance: 4`, threshold 0.90, `FakeTimeProvider` never advanced):

```
Zero-acceptance scan bound reached [QfcStreamingDequeueConfidenceGate.DequeueAsync] Accepted=0 Scanned=4 Cutoff=900 Elapsed=00:00:00 ScanCap=4 Ceiling=00:02:00 Bound=scan-cap Decision=stop
```

Time-ceiling bound (`zeroAcceptanceCeiling: TimeSpan.FromSeconds(120)`, clock advanced 121 s,
`MaxScanWithoutAcceptance` left at its default 250):

```
Zero-acceptance scan bound reached [QfcStreamingDequeueConfidenceGate.DequeueAsync] Accepted=0 Scanned=0 Cutoff=900 Elapsed=00:02:01 ScanCap=250 Ceiling=00:02:00 Bound=zero-acceptance-ceiling Decision=stop
```

Short, single-line, non-interpolated assertion tokens:

| Purpose | Token |
|---|---|
| Select the scan-bound line out of the captured log list | `Zero-acceptance scan bound reached` |
| AC1 — accepted count | `Accepted=0` |
| AC1 — scanned count (cap test) | `Scanned=4` |
| AC1 — cutoff in force | `Cutoff=900` |
| AC1 — item-cap bound token | `Bound=scan-cap` |
| AC1 — stop decision | `Decision=stop` |
| AC2 — time-ceiling bound token | `Bound=zero-acceptance-ceiling` |
| AC2 — negative assertion that the two bounds did not collapse | `Bound=scan-cap` asserted absent |

Two selection hazards the plan must respect:

1. `LogLaunch` (`:310-320`) also emits `ScanCap=` and `Ceiling=` on every run, and
   `LogZeroAcceptanceCheckpoint` (`:327-342`) also emits `Accepted=`, `Scanned=` and `Cutoff=`.
   The new tests must therefore filter the captured list on
   `Zero-acceptance scan bound reached` **before** asserting the field substrings, following the
   pattern the existing `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` test uses at
   `Part4.cs:242-249`.
2. `Bound=` appears only in the scan-bound line, and FluentAssertions' `Contain` for strings is
   case-sensitive, so `ScanCap=250` in the launch line cannot be confused with `Bound=scan-cap`.
   Asserting `Bound=scan-cap` absent in the ceiling test is therefore safe and is the assertion
   that makes AC2's "collapsed the two bounds to one value" regression detectable.

### 4.c — Are the two bounds separately drivable through the helper?

**Direct answer: yes, and the two existing tests already prove it.** Both bounds are evaluated at
the same site, `QfcStreamingDequeueConfidenceGate.cs:230`:

```
if (scanned >= MaxScanWithoutAcceptance || elapsed >= ZeroAcceptanceCeiling)
```

and both reach `LogScanBoundReached(accepted.Count, scanned, elapsed)` at `:232`. Which disjunct
fires determines the `bound` token at `:352`.

**Item-cap bound.** Reproduce `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached`
(`Part4.cs:132-167`):

- `tryTakeNext`: a counting lambda over `new Queue<MailItem>(BuildCandidates(10))`
- `scoreLoader`: `(mail, token) => Scored(100L)` — below the 900 cutoff, and it does **not**
  advance the clock
- `threshold: 0.90`
- `timeProvider: new FakeTimeProvider()`
- `sourceActive: () => true`
- `maxScanWithoutAcceptance: 4`
- `zeroAcceptanceCeiling`: omitted, so the default 120 s applies and `elapsed` (0) never reaches it
- Act: `DequeueBatchAsync(gate, 5, 0, CancellationToken.None)`
- Outcome: `scanned` reaches 4, `4 >= 4` is true, `0 >= 00:02:00` is false, so
  `bound == "scan-cap"`.

**Time-ceiling bound.** Reproduce
`DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling`
(`Part4.cs:175-207`):

- `tryTakeNext`: `() => null` — the source never yields, so the loop parks on the empty-source
  delay at `:253-255` and `scanned` stays 0
- `scoreLoader`: `(mail, token) => Scored(950L)` (never invoked)
- `threshold: 0.90`
- `timeProvider: new FakeTimeProvider()`
- `sourceActive: () => true` — required, otherwise the loop returns `SourceExhausted` at `:249`
  before the bound is evaluated
- `zeroAcceptanceCeiling: TimeSpan.FromSeconds(120)`
- `maxScanWithoutAcceptance`: omitted, so the default 250 applies and `0 >= 250` is false
- Act: start `DequeueBatchAsync(gate, 1, 200, CancellationToken.None)` without awaiting, assert it
  is not complete, then `fakeTime.Advance(TimeSpan.FromSeconds(121))`, then await
- Outcome: `0 >= 250` false, `00:02:01 >= 00:02:00` true, so
  `bound == "zero-acceptance-ceiling"`.

**What changes when a `debugLog` delegate is passed.** Only observability. The delegate is invoked
at three sites — `LogLaunch:318`, `LogZeroAcceptanceCheckpoint:340`, `LogScanBoundReached:359` —
and additionally by `LogScore` (`:363` onward) once per scored candidate. It is never consulted in
any control-flow decision, so the two existing tests' outcome assertions (stop reason, take count,
scanned count, residual queue length) are unaffected. Consequences for the new tests:

- The captured list will contain more than the scan-bound line. Filter first.
- In the cap test the list will contain one launch line, four per-score lines, and one scan-bound
  line. In the ceiling test it will contain one launch line and one scan-bound line (no candidate
  is ever scored). No checkpoint line appears in the ceiling test because the default 12 s
  checkpoint interval is reached and logged at `:236-240` — verify this at execution time rather
  than asserting an exact total count; asserting `ContainSingle` on the scan-bound filter is the
  robust form.
- Recommendation: write the new tests as new, separately named methods rather than mutating the
  two existing bound tests. The existing tests are the #791 stop-reason and take-count pins; AC1
  and AC2 are log-content criteria. Mixing them would make one failure ambiguous.

### 4.d — Part4 file size and whether two more tests fit

**Direct answer: yes, with roughly 100 lines of margin.**

- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` is **348 lines**
  at the base commit (closing brace at `:347`, file ends `:348`). This matches `issue.md:141`.
- The repository limit is 500 lines (`CLAUDE.md`, General Code Change Policy section 4;
  `.claude/rules/general-code-change.md`, File Size Limit).
- Size of the two nearest analogues, measured including their XML doc comments:
  `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` is 41 lines
  (`:127-167`); `DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling` is
  39 lines (`:169-207`). The log-asserting analogue
  `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` is 49 lines (`:209-258`).
- A realistic estimate for the two new tests is 50-55 lines each, because each is the arrangement
  of an existing bound test plus a `debugLog: logs.Add` argument, a `var logs = new List<string>();`
  declaration, a filter line, and four to five `.And.Contain(...)` assertions, plus a doc comment.
- Projected total: 348 + 100 to 348 + 110 = **448 to 458 lines**. That is under 500 with 42 to 52
  lines of margin, so no new part file and no edit to `QuickFiler.Test/QuickFiler.Test.csproj` is
  required. The Write Set holds.
- Contingency, recorded for completeness only: if the executor's tests overshoot,
  `QfcStreamingDequeueConfidenceGateTests.Part3.cs` is 290 lines and could absorb them, but that
  file is not in the Write Set and moving tests there would be a scope change. The other two parts
  cannot absorb anything: `QfcStreamingDequeueConfidenceGateTests.Part2.cs` is 499 lines and
  `QfcStreamingDequeueConfidenceGateTests.cs` is 488 lines.
- CSharpier will reformat the new code; line estimates above assume CSharpier's output shape,
  which the surrounding tests already exhibit.

---

## 5. Scope findings

### SF-1 — AC3 cannot be literally satisfied inside the Write Set (blocking, needs an orchestrator decision)

AC3 reads: *"`UtilitiesCS/Threading/ProgressPackage.cs` releases every `CancellationTokenSource` it
constructs itself."*

The class constructs a source at `ProgressPackage.cs:25` and `:40`. On the following nine
production paths that source is constructed and then leaves the class with no holder that will ever
call a release:

| # | Site | Why the release cannot happen inside the Write Set |
|---|---|---|
| 1 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs:38-40` | Package discarded by the static factory; source discarded into `_` by the caller |
| 2 | `.../EmailDataMiner.Transform.cs:196-198` | Same |
| 3 | `.../EmailDataMiner.Transform.cs:320-322` | Same |
| 4 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs:129` | Package discarded; source captured as `tokenSource`, never disposed |
| 5 | `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs:237-239` | Package discarded; source flows into a local tuple |
| 6 | `UtilitiesCS/EmailIntelligence/ClassifierGroups/MulticlassEngine.cs:249-251` | Same |
| 7 | `UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs:180-182` | Same |
| 8 | `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianPerformanceMeasurement.cs:1351-1353` | Owned package returned to the caller; caller at `:65-69` never disposes |
| 9 | `.../BayesianPerformanceMeasurement.cs:1403-1405` | Owned package returned to the caller; caller at `:160-165` never disposes |

The four owning files —
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs`,
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs`,
`UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`,
`UtilitiesCS/EmailIntelligence/ClassifierGroups/MulticlassEngine.cs`,
`UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs`,
`UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianPerformanceMeasurement.cs`
— are **not** in the `issue.md` Write Set (lines 72-79).

Recommended resolution, in preference order:

1. **Preferred.** Keep the Write Set unchanged. Implement the ownership flag plus `IDisposable`
   plus the tuple-factory ownership-transfer doc comment as described in 1.b. Interpret AC3 as a
   capability criterion: *the class records ownership at the construction site and releases only an
   owned source when disposed; it never releases an injected one*. Open a follow-up issue covering
   the nine sites above so the residual is tracked rather than buried in feature-folder prose.
   This is consistent with the repository convention of promoting out-of-scope defects to real
   issues.
2. Widen the Write Set to include the four to six consumer files. Not recommended for a
   minor-audit item: three of the call sites are in `[ExcludeFromCodeCoverage]` host-bound methods,
   disposal ordering at each site requires proving the token is no longer in flight, and
   `BayesianPerformanceMeasurement.cs` is a 1500-plus-line file whose `LoadIfNullAsync` contract
   would have to change shape.
3. Do nothing for Defect B. Not recommended; the mechanism in option 1 is a genuine improvement
   and is cheap.

### SF-2 — `ProgressTracker.cs` and `ProgressTrackerPane.cs` do not need to change (favourable)

Recorded here so the orchestrator does not pre-emptively widen the Write Set. See 1.d for the
evidence. Both `ProgressViewer.RequestCancel` (`UtilitiesCS/Threading/ProgressViewer.cs:82-101`) and
`ProgressPane.RequestCancel` (`UtilitiesCS/Threading/ProgressPane.cs:66-84`) already catch
`ObjectDisposedException` and document the borrowed-source contract. `ProgressTracker` reads
`_cancelSource` exactly once after assignment, inside `Initialize()`. `ProgressTrackerPane` does
not retain the source at all.

### SF-3 — `coverage_output.txt` becomes stale (informational only)

`coverage_output.txt` at the repository root contains
`UtilitiesCS\Threading\ProgressTrackerAsync.cs | 0.5` and is the only non-`docs/`, non-`.claude/`
artifact outside the Write Set that names the deleted type. It is a generated coverage summary, not
compiled, and not referenced by any build step found in this search. No action proposed; recorded
so a reviewer who greps for the type name after the deletion is not surprised.

---

## 6. Testing implications (no test code written)

Defect A — two new `[TestMethod]`s in
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`:

- Both reuse the existing reflection helpers and `FakeTimeProvider`. No sleep, no wall-clock wait,
  no COM, no temporary file. Deterministic by construction, matching the determinism requirements
  in `.claude/rules/general-unit-test.md`.
- Both must filter the captured delegate output on `Zero-acceptance scan bound reached` before
  asserting fields (see 4.b).
- AC2's discriminating assertion is the negative one: the ceiling test must assert
  `Bound=scan-cap` is absent, not merely that `Bound=zero-acceptance-ceiling` is present.
  Presence alone would still pass under a regression that emitted both.
- The ceiling test's `pending.IsCompleted.Should().BeFalse()` step is an established pattern in
  this file (`Part4.cs:193`); reuse it rather than inventing a different synchronization shape.

Defect B — three new `[TestMethod]`s in `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`:

- Owned-source-released, injected-source-left-usable, child-does-not-release-parent's-source.
- All three must use the tracker overload with a non-null injected `ProgressTracker` and an
  explicit `stopWatch`, so no UI thread is touched (see 1.f). `new ProgressTracker(cts)` is already
  proven headless-safe by the existing test at `ProgressPackage_Tests.cs:18`.
- The deterministic disposal probe is that the source's `Token` getter throws
  `ObjectDisposedException` after release. Do not assert on timers or finalization.
- The existing four tests in that file must keep passing unchanged; none of them calls
  `InitializeAsync` with a null `cancelSource`, so none acquires ownership.
- File size after the change: the file is 121 lines today; three tests at roughly 25 lines each
  brings it to about 200 lines, well under the limit.

Defect C:

- Deleting `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` removes exactly 9 executed
  tests from the UtilitiesCS test assembly (see 3.d). AC11's expected delta is **-9**, with the
  net delta for the whole delivery being `-9 + 3 = -6` for that assembly if the three Defect B
  tests land in the same run, and `+2` for the QuickFiler test assembly.
- Capture the per-assembly test counts in Phase 0 so the AC11 comparison is against a measured
  baseline rather than an inferred one.

Coverage (AC12):

- The changed lines in `ProgressPackage.cs` are the two flag assignments at the construction sites
  and the new `Dispose` body. All are exercised by the three Defect B tests, so the changed-line
  coverage should be 100 percent and cannot regress against a Phase 0 baseline of the same lines.
- `SubjectMapSco.Orchestration.cs` is excluded from any meaningful coverage comparison by the
  `[ExcludeFromCodeCoverage]` attribute at `:221` (see 2.c); AC12 does not name that file and
  should not be extended to it.
- Deleting `ProgressTrackerAsync.cs` removes a 109-line production file from the denominator. Its
  committed per-file figure in `coverage_output.txt` is 0.5, so the repository-wide percentage will
  move upward slightly. That movement is expected and is the stated purpose of issue #841
  (`issue.md:139-140`).
