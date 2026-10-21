# test-determinism-and-hygiene-debt (Spec)

- **Issue:** #729
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T21-00
- **Status:** Ready for Planning
- **Version:** 1.0
- **Work Mode:** full-bug — this spec is the sole acceptance-criteria source. No user-story.md is produced for this item.

## Context
Four consolidated findings across the C# test suite, all in the same theme: tests that depend on real wall-clock time, real WinForms UI construction, unparallelized-but-unguarded execution, or environmental load — rather than deterministic seams — violating this repo's own determinism-infrastructure policy (.claude/rules/general-unit-test.md: controllable clock, no real wall-clock waits, no live UI in unit tests). Consolidated into one issue rather than four since all four are variations of the same root problem (missing determinism seams) and fixing them is one coherent test-infra effort.

Environment:
- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C# MSTest suite
- Command/flags used: n/a — findings are from direct test-source inspection
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: none of these cause incorrect production behavior — they're test-suite reliability/determinism debt that risks intermittent CI failures and slower feedback loops, consistent with this repo's own stated rationale for the determinism-infrastructure policy these findings violate.


## Repro & Evidence
Steps to Reproduce:
Not applicable — each sub-finding is a static test-source inspection. See "Actual Behavior."

Expected:
Per this repo's own determinism-infrastructure policy: tests use an injected `TimeProvider`/`Clock` seam rather than reading wall-clock time directly; no live UI construction in a unit test; parallelizable tests either tolerate parallel execution or are explicitly excluded with a documented reason; load-sensitive timeouts don't cause spurious CI failures under contention.

Actual:
**1. `NonBlockingDelayTests.cs` awaits real wall-clock time.** Confirmed at `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:38-39`: `var interval = TimeSpan.FromMilliseconds(30); var stopwatch = Stopwatch.StartNew();` then awaits `NonBlockingDelay.WaitAsync(interval)` against that real stopwatch — no fake-timer/`TimeProvider` seam is used. *(Source: #694.)*

**2. `UtilitiesCS.Test/ResourceTests.cs` constructs a live WinForms form in a unit test.** Confirmed at line 20: `Form1 frm = new Form1();` inside `TestMethod1`. No structural guard against this exists for `UtilitiesCS.Test` — only QuickFiler.Test has an equivalent "no live Form in test assembly" structural test. *(Source: #586.)*

**3. Two duplicate `DASLFilterParser*Tests.cs` classes lack `[DoNotParallelize]`**, while the test assembly runs `[assembly: Parallelize(Workers=0, Scope=ClassLevel)]` and no console-lock/serialization mechanism exists to make concurrent execution of these two classes safe. *(Source: #520.)*

**4. Pump-hosted `QfcItemController` tests expire at the 60s `PumpTimeoutMs` under CPU contention.** Load-sensitive flakiness rather than a straightforward logic defect — no simple code fix, but worth tracking as one line item in this consolidated test-infra debt issue rather than its own standalone tracker, since the underlying cause (no environment-aware timeout scaling, or no mocked pump) is the same class of missing-determinism-seam problem as findings 1-3. *(Source: #711.)*

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations above, each confirmed directly against `origin/main` on 2026-09-02.

### Verified corrections to the issue text above

The original issue text is retained verbatim for provenance. Re-verification against the working tree on 2026-09-02 (research artifact docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md) found that **three of the four premises above are materially wrong**. The corrected facts below, not the issue text, govern this spec.

| # | Issue premise | Verified state | Research citation |
|---|---|---|---|
| 1 | Add an optional `TimeProvider?` parameter to `NonBlockingDelay.WaitAsync` | Correct that no seam exists, but the proposed *shape* would not compile. `WaitAsync` is consumed as a method group at TaskMaster/AppGlobals/StoreRehookCoordinator.cs line 102 (`_delay = delay ?? NonBlockingDelay.WaitAsync;` binding to `Func<TimeSpan, Task>`); C# forbids a method-group conversion when an optional parameter has no corresponding delegate parameter, producing CS0123. An explicit overload pair is required. | §1.2, §5 N1 |
| 2 | `Form1`/`Form2` are compiled into `UtilitiesCS.Test` via `ResourceTests.cs` | **False.** `ResourceTests.cs`, `Form1.cs`, `Form2.cs`, `Form3.cs` and their Designer/`.resx` companions are **not** listed in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (explicit `<Compile Include>` list, no wildcard globbing). They are orphan files on disk that never enter the assembly, so no live `Form` is reachable from `UtilitiesCS.Test` today. The real live violation is `SVGControl.Test`, which does compile `Form1.cs` and `Form2.cs`. | §2.1, §2.2, §5 N2, §5 N3 |
| 3 | Two duplicate `DASLFilterParser*Tests.cs` classes conflict with **each other** | **False.** `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` is also absent from the csproj; only `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` is compiled, so there is no two-class conflict. The real hazard is that the compiled class mutates process-global `Console.Out` while roughly thirty sibling classes do the same under the class-level parallel scope. A second unprotected class with the identical hazard exists: `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`. | §3.1, §3.2, §3.4, §5 N4 |
| 4 | The 60 s `PumpTimeoutMs` is a load-sensitive timing dependency that may be removable test-side | Confirmed non-fixable from the test side. `PumpTimeoutMs` is used exclusively as an MSTest `[Timeout(...)]` harness bound and never as an in-test wait, so there is no wall-clock wait to seam out. Scoped out of #729 and promoted as issue #743. | §4.1, §4.2, §5 N5 |

Additional verified facts established while preparing this spec (2026-09-02):

- `SVGControl.Test/SVGControl.Test.csproj` already references FluentAssertions 8.10.0 (line 133) and MSTest.TestFramework 4.3.3 (line 233), so porting the structural guard there needs no package addition. This closes the open question in research §2.3.
- Both `StoreRehookCoordinator` constructions in TaskMaster.Test/AppGlobals/StoreRehookCoordinatorTests.cs (lines 60 and 259) supply `_ => Task.CompletedTask` for the trailing `delay` parameter, so the `delay ?? NonBlockingDelay.WaitAsync` fallback at StoreRehookCoordinator.cs line 102 is **never** taken by those tests. The 1-arg `WaitAsync` overload therefore has no transitive coverage from that suite, and a direct test of it is required to avoid a coverage regression on changed production lines (research §7 raised this as conditional; the condition is now resolved as "test required").

## Scope & Non-Goals

### In scope

**Finding 1 — `TimeProvider` seam for `NonBlockingDelay`.**
- `TaskMaster/AppGlobals/NonBlockingDelay.cs` gains an explicit overload pair: `WaitAsync(TimeSpan delay)` (unchanged signature, delegates to the 2-arg overload with `TimeProvider.System`) and a new `WaitAsync(TimeSpan delay, TimeProvider timeProvider)` whose body replaces `new Timer(...)` with `timeProvider.CreateTimer(...)` returning `ITimer`.
- `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` is rewritten in place to remove `Stopwatch` and inject `FakeTimeProvider`.
- `TaskMaster.Test/TaskMaster.Test.csproj` and `TaskMaster.Test/packages.config` gain two package references each (`Microsoft.Bcl.TimeProvider` 10.0.11, `Microsoft.Extensions.TimeProvider.Testing` 10.9.0), mirroring UtilitiesCS.Test's existing entries verbatim (research §1.5 supplies the exact insertion text).

**Finding 2 — live `Form` types in unit-test assemblies (expanded beyond the issue's literal citation).**
- `UtilitiesCS.Test`: delete the orphan files `UtilitiesCS.Test/ResourceTests.cs`, `UtilitiesCS.Test/Form1.cs`, `UtilitiesCS.Test/Form1.Designer.cs`, `UtilitiesCS.Test/Form1.resx`, `UtilitiesCS.Test/Form2.cs`, `UtilitiesCS.Test/Form2.Designer.cs`, `UtilitiesCS.Test/Form2.resx`, `UtilitiesCS.Test/Form3.cs`, `UtilitiesCS.Test/Form3.Designer.cs`, `UtilitiesCS.Test/Form3.resx` (research §2.4 item 1; `Form3` is not named in the issue but is part of the same stranded set).
- `UtilitiesCS.Test`: add `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`, ported from QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs with namespace `UtilitiesCS.Test` and the `GetLoadableTypes` `ReflectionTypeLoadException` fallback carried over unchanged, plus a `<Compile Include>` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- `SVGControl.Test`: delete `SVGControl.Test/Form1.cs`, `SVGControl.Test/Form1.Designer.cs`, `SVGControl.Test/Form1.resx`, `SVGControl.Test/Form2.cs`, `SVGControl.Test/Form2.Designer.cs`, `SVGControl.Test/Form2.resx`; remove their `<Compile>` and `<EmbeddedResource>` entries from `SVGControl.Test/SVGControl.Test.csproj`; add the same ported guard as `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs`.

*Deviation notice (deliberate and justified).* Issue #729 names only `UtilitiesCS.Test/ResourceTests.cs:20`. That file is an orphan that is never compiled (research §2.1, §5 N2), so acting on the issue's literal citation alone would satisfy the letter of the issue while leaving the actual defect — compiled, unreferenced live `Form` types — untouched in a sibling assembly. `SVGControl.Test` is the only site in the repository where this defect is live and where fail-before evidence exists (research §2.2, §5 N3), and it is in the same solution and the same CI assembly sweep. It is therefore included. Research §2.4 item 3 explicitly deferred this call to the orchestrator; the orchestrator's decision is "include".

**Finding 3 — parallel-execution hazard on `Console.Out` (corrected hazard).**
- Add `[DoNotParallelize]` plus a hazard comment to `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` and `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`.
- Delete the orphan duplicate `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs`.

### Out of scope / non-goals

- **Finding 4 — pump-hosted `QfcItemController` timeout — is explicitly and entirely OUT of scope for issue #729.** Research §4 verified that no test-only change removes its load sensitivity (four independent reasons, §4.2). It has been promoted as its own follow-up issue **#743** (docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md). This promotion is load-bearing: the prior standalone tracker for this same finding, **#711**, was already closed as "superseded by #729", so #743 exists specifically so that closing #729 does not silently drop the finding a second time.
  Finding 4 — reasons no test-only fix exists:
  1. The production code reads the context off the control, not from an injected seam.
  2. The fixture's cost is the real WinForms control tree, not the pump.
  3. `[DoNotParallelize]` would be a no-op.
  4. Removing `[Timeout]` trades a bounded failure for an unbounded hang.
- **All QuickFiler/ production sources.** These are owned by a different parallel work item in this run. No file under QuickFiler/ is modified by #729.
- **No production API seam for `DASLFilterParser.PrintTree`.** Giving `PrintTree` a `TextWriter` parameter would be an unrelated `UtilitiesCS` production API change with no defect behind it, outside the bugfix minimal-change rule (research §3.5).
- **No `[DoNotParallelize]` on UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs.** It captures and restores `Console.Out` but asserts through Moq rather than on captured text, so it has no failing mode of its own; marking it would serialize a class for no benefit (research §3.4).
- **No `PumpTimeoutMs` constant-hoisting hygiene in QuickFiler.Test.** Research §4.3 lists this as permitted but non-remedial; it is excluded to keep the change minimal and to avoid touching a parallel work item's assembly.
- **No app.config change in TaskMaster.Test.** The `Microsoft.Bcl.TimeProvider` binding redirect is already present (TaskMaster.Test/app.config lines 265-271) and no redirect exists or is needed for `Microsoft.Extensions.TimeProvider.Testing` (research §1.5).

### Explicitly excluded systems, integrations, or datasets

- No file under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json is modified. These are push-down-owned from an upstream repository and any defect in them must be fixed upstream.
- No Outlook/COM interaction, no network, no filesystem temporary files. All changes are compile-time or reflection-over-metadata only.

## Root Cause Analysis

Each finding traces to a specific issue, cited inline above. All four share the same root class: a test depends on real time, real UI, or real environmental load instead of an injected, controllable seam — exactly what .claude/rules/general-unit-test.md's "Determinism Infrastructure" section already mandates repo-wide but which these four tests predate or were missed by. The per-finding root causes below are the **corrected** ones established by research on 2026-09-02.

**Finding 1 — no seam exists, and the obvious seam shape is blocked by a method-group conversion constraint.** `NonBlockingDelay.WaitAsync` builds its one-shot timer with `new System.Threading.Timer(...)` directly (NonBlockingDelay.cs lines 55-64), so a test has no way to control when the callback fires and must fall back on a real `Stopwatch`. The reason this was not already fixed with the repository's standard optional-`TimeProvider?` pattern (the precedent at UtilitiesCS/Threading/ThreadMonitor.cs lines 64-82) is a language constraint: that precedent is a *constructor*, and constructors are never converted to delegates. `WaitAsync` is converted to a delegate, at StoreRehookCoordinator.cs line 102. Per the C# standard's method-group-conversion rules, candidate methods are ignored if one or more of their optional parameters has no corresponding parameter in the target delegate type, so adding `TimeProvider? timeProvider = null` removes the only candidate and yields CS0123 at that line. Two explicit overloads restore an unambiguous 1-parameter candidate. (Research §1.1, §1.2, §1.6; numeric derivation §5 N1 established that there are exactly two production call sites and exactly one of them is a method-group conversion.)

**Finding 2 — the reported site is orphan source, and the defect's real location is a sibling assembly (orphan-vs-compiled distinction).** `UtilitiesCS.Test.csproj` uses an explicit `<Compile Include>` list with no wildcard include and no SDK-style implicit globbing, so presence on disk does not imply presence in the assembly. `ResourceTests.cs` and the three `Form` sources are on disk but absent from the csproj — a historical file move left the root copies stranded (the same pattern is visible for `SerializableListTest.cs` and `DeedleTests.cs`, which are compiled from their moved copies). The `ShowDialog()` calls the issue flags therefore cannot execute. The identical hygiene defect *is* live in `SVGControl.Test`, whose csproj compiles `Form1.cs` and `Form2.cs` (with `<SubType>Form</SubType>`) and embeds their `.resx` files, while no test in that assembly references either type. (Research §2.1, §2.2; numeric derivation §5 N2 and §5 N3.)

**Finding 3 — the same orphan-vs-compiled distinction, plus a mis-identified shared resource.** `DASLFilterParser_Tests.cs` is byte-identical to `DASLFilterParserTests.cs` apart from namespace and class name, but only the latter appears in the csproj, so the "two classes conflicting" framing describes a conflict that cannot occur. The genuine hazard is that `DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` captures `Console.Out`, installs a `StringWriter`, and restores the original — a mutation of process-global state — because the production method writes directly to the console with no injectable `TextWriter`. Under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]` (UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21, which research §0 confirmed is live in CI because the CI vstest invocation passes no `/Settings:` argument), this class overlaps roughly thirty sibling classes that also call `Console.SetOut`. Two documented failure modes follow: (a) a sibling's redirect lands between capture and act, the `StringWriter` stays empty, and the assertion fails; (b) a sibling captures `Console.Out` while this test's `StringWriter` is installed and reinstalls it after the `using` has disposed it, making every later `Console.Write` in the process throw `ObjectDisposedException` — one interleaving cascading into unrelated failures. The repository already recognises this exact hazard and its remedy in two classes. (Research §3.1, §3.2, §3.3; numeric derivation §5 N4.)

**Finding 4 — a harness timeout bound, not a wait (timeout-vs-wait distinction).** `PumpTimeoutMs` appears only as the argument of MSTest `[Timeout(...)]` attributes and never in a wait or expression position; the pump-hosted tests contain no `Thread.Sleep`, no `Task.Delay`, no `Stopwatch`, and no polling loop, so their logic already satisfies the determinism rule. The failure mode is narrower than non-determinism: under contention the *real elapsed cost of the work under test* — constructing real WinForms and WebView2 controls to the handle-created state — exceeds the 60 s bound and MSTest aborts an otherwise-correct test. Because the cost is real UI construction serviced by a real Win32 message loop, and because the production code reads its `SynchronizationContext` off the control it constructs rather than from an injected seam, no test-side substitution can remove it. (Research §4.1, §4.2, §4.3; numeric derivation §5 N5.)

## Proposed Fix

### Design summary (what changes where):

Three independent, small changes, all test-side except one production seam:

1. Split `WaitAsync` in `TaskMaster/AppGlobals/NonBlockingDelay.cs` into a 1-arg overload and a 2-arg `TimeProvider`-accepting overload, and rewrite `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` around `FakeTimeProvider`.
2. Remove every `Form`-derived type from the two affected unit-test assemblies and install the existing structural guard in each so the condition cannot return.
3. Move the two remaining `Console.Out`-capturing `UtilitiesCS.Test` classes into MSTest's serial partition with `[DoNotParallelize]`, and delete the orphan duplicate that would reintroduce the hazard if a contributor added it to the csproj.

**`TaskMaster/AppGlobals/NonBlockingDelay.cs` is the only production (non-test) file this work touches.** It is touched because no seam exists yet and one cannot be created from the test side, which is precisely the exception the item brief allows: *test-only wherever possible, production seam only where a seam genuinely does not exist*. Every other change in this item is confined to test projects, test sources, or test project files. In particular, no production API is broadened, renamed, or removed: the 1-arg `WaitAsync` signature that existing callers bind to is unchanged.

### Boundaries and invariants to preserve:

- **`NonBlockingDelay` stays `internal static`.** No class-shape change; the `[assembly: InternalsVisibleTo("TaskMaster.Test")]` declarations already make it reachable from the test project.
- **The 1-arg `WaitAsync(TimeSpan)` overload must remain the unique 1-parameter candidate** so the method-group conversion at StoreRehookCoordinator.cs line 102 continues to bind to `Func<TimeSpan, Task>` (no CS0123). No optional parameter may be introduced on either overload.
- **Preserve the `#nullable enable annotations` / `#nullable restore annotations` pragma pair at `NonBlockingDelay.cs:52-54`** (surrounding `Timer? timer = null`, together with the explanatory comment at lines 47-51). Removing or widening it re-emits CS8632 under the `TreatWarningsAsErrors` gate. The 2-arg overload's `ITimer? timer = null` local needs the same narrowly-scoped treatment.
- **Timer disposal and completion semantics are unchanged.** `ITimer` is `IDisposable`, so the existing `timer?.Dispose(); tcs.TrySetResult(true);` callback body carries over verbatim. `TaskCreationOptions.RunContinuationsAsynchronously` on the `TaskCompletionSource` must be retained — it is what prevents an `await` continuation from running inline on the thread calling `FakeTimeProvider.Advance`.
- **The structural guard is metadata-only.** It reflects over `Assembly.GetExecutingAssembly()` and never instantiates a type; the `ReflectionTypeLoadException` fallback must be carried over so the guard degrades to the loadable subset rather than turning permanently red for an unrelated load failure.
- **`[DoNotParallelize]` is additive and order-independent.** No test body, assertion, or test name changes for finding 3.
- **No production API seam for `DASLFilterParser.PrintTree`**; the console redirect stays at the test level.
- **No file under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json is touched. No file under QuickFiler/ production sources is touched.**

### Dependencies or blocked work:

- The `packages/` directory is absent from this worktree, so `nuget restore` (or a restore-on-build) is a prerequisite before any build following the packages.config / `.csproj` edits (research §0).
- Issue #743 tracks Finding 4. It is a follow-up, not a blocker: #729 can be completed and merged independently.
- QuickFiler/ production sources are held by a different parallel work item in this run; this item must not write into that footprint.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

Reproduced from research §7 "Requirements Mapping", with the `SVGControl.Test` rows added per the in-scope decision recorded above.

| File | Change |
|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | Split `WaitAsync` into a 1-arg overload delegating to a new 2-arg `WaitAsync(TimeSpan, TimeProvider)`; replace `new Timer(...)` with `timeProvider.CreateTimer(...)` returning `ITimer`; preserve the lines 52-54 `#nullable ... annotations` pragma pair. **Only production file changed.** |
| `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` | Remove `Stopwatch` and the `System.Diagnostics` using; inject `FakeTimeProvider`; add the not-completed-before-`Advance` assertion; add one test covering the 1-arg overload |
| `TaskMaster.Test/TaskMaster.Test.csproj` | Two `Reference` blocks (research §1.5 supplies the verbatim text mirrored from UtilitiesCS.Test.csproj) |
| `TaskMaster.Test/packages.config` | Two `package` entries (research §1.5) |
| `UtilitiesCS.Test/ResourceTests.cs`, `UtilitiesCS.Test/Form1.cs`, `UtilitiesCS.Test/Form1.Designer.cs`, `UtilitiesCS.Test/Form1.resx`, `UtilitiesCS.Test/Form2.cs`, `UtilitiesCS.Test/Form2.Designer.cs`, `UtilitiesCS.Test/Form2.resx`, `UtilitiesCS.Test/Form3.cs`, `UtilitiesCS.Test/Form3.Designer.cs`, `UtilitiesCS.Test/Form3.resx` | Delete (orphan files; no csproj edit needed because none is referenced) |
| `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` | New — ported guard, namespace `UtilitiesCS.Test` |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Add the `<Compile Include>` entry for the new guard |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | Add `[DoNotParallelize]` + hazard comment |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | Add `[DoNotParallelize]` + hazard comment |
| `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` | Delete (orphan duplicate) |
| `SVGControl.Test/Form1.cs`, `SVGControl.Test/Form1.Designer.cs`, `SVGControl.Test/Form1.resx`, `SVGControl.Test/Form2.cs`, `SVGControl.Test/Form2.Designer.cs`, `SVGControl.Test/Form2.resx` | Delete (compiled, unreferenced live `Form` types — the only live violation of Finding 2) |
| `SVGControl.Test/SVGControl.Test.csproj` | Remove the `<Compile>` entries for `Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs` and the `<EmbeddedResource>` entries for `Form1.resx`, `Form2.resx`; add the `<Compile Include>` entry for the new guard |
| `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` | New — ported guard, namespace `SVGControl.Test`. FluentAssertions and MSTest references confirmed present (csproj lines 133 and 233), so no package addition is required |
| `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/fail-before-exception.<timestamp>.md` | New — fail-before exception dossier for Finding 3 |

#### Functions/classes/CLI commands impacted:

- `TaskMaster.NonBlockingDelay.WaitAsync` — one new overload; existing signature unchanged.
- `TaskMaster.Test.AppGlobals.NonBlockingDelayTests` — both existing test methods rewritten in place; one method added for the 1-arg overload.
- `UtilitiesCS.Test.OutlookObjects.DASLFilterParserTests`, `UtilitiesCS.Test.ReusableTypeClasses.StackGeek_Tests` — class-level attribute added only.
- `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests`, `SVGControl.Test.NoLiveFormInTestAssemblyTests` — new structural guard classes.
- No CLI command surface changes.

#### Data flow and validation changes:

None. The 2-arg overload changes only *which* timer abstraction schedules the completion callback; the returned `Task`'s observable contract (completes once, after `delay`, on a thread-pool continuation) is identical.

#### Error handling and logging updates:

None. No new exception type, no new catch, no logging call is added or removed. The guard's `ReflectionTypeLoadException` fallback is carried over from the existing QuickFiler.Test source unchanged.

#### Rollback/feature-flag considerations (if applicable):

Not applicable. There is no runtime flag and no staged rollout. Rollback is a plain revert of the branch; the only production file involved retains a backward-compatible signature, so a revert cannot strand a caller.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `public static Task WaitAsync(TimeSpan delay)` — unchanged contract: returns a task completing after `delay`, without blocking the caller and without requiring a running `Dispatcher`. Delegates to the 2-arg overload with `TimeProvider.System`.
- `public static Task WaitAsync(TimeSpan delay, TimeProvider timeProvider)` — same contract, with scheduling supplied by the caller's `TimeProvider`. `timeProvider.CreateTimer(callback, null, delay, Timeout.InfiniteTimeSpan)` returns an `ITimer` disposed inside the callback.
- The structural guard produces an ordered `string[]` of `Form`-assignable type full names from the executing assembly and asserts it is empty.

#### Required configuration keys and defaults:

None. `TimeProvider.System` is the default supplied by the 1-arg overload; no configuration key, environment variable, or runsettings entry is introduced.

#### Backward-compatibility expectations:

- No breaking change. Both production call sites of `WaitAsync` (research §5 N1 established there are exactly two: the method-group conversion at StoreRehookCoordinator.cs line 102 and the direct invocation at TaskMaster/AppGlobals/AppEvents.cs line 456) compile unchanged.
- No `.csproj` in the solution sets `DocumentationFile` or `GenerateDocumentationFile`, so CS0419 (ambiguous cref) cannot be emitted and the existing `<see cref="NonBlockingDelay.WaitAsync"/>` references remain valid despite the new overload (research §0).
- `ITimer` and `TimeProvider` are proven available on net481 in this repository by UtilitiesCS/Threading/ThreadMonitor.cs (lines 43 and 96-101).

#### Performance constraints (latency/throughput/memory):

- Production runtime cost is unchanged in practice: `TimeProvider.System.CreateTimer` wraps the same `System.Threading.Timer`, adding one small wrapper allocation per call. The helper is invoked on a retry path, not a hot path.
- Test-suite runtime improves: the rewritten `NonBlockingDelayTests` no longer spends real milliseconds waiting. `[DoNotParallelize]` on two classes moves them to MSTest's serial partition; the two classes are short and the effect on total assembly time is negligible relative to the flake risk removed.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - `nuget restore` can populate `packages/` in this worktree; the two new packages resolve at the pinned versions used by UtilitiesCS.Test.
  - **Confirmed by execution, no longer an assumption.** Zero-due-time behaviour was the single point in the change designated for confirmation by executing the test rather than by reading source. It has now been executed, and the result is the opposite direction of error from the one anticipated: `FakeTimeProvider.CreateTimer` invokes a zero-due-time one-shot callback during creation, so the task returned for `TimeSpan.Zero` is already completed when control returns and no `Advance` call is required at all. The prior expectation — that `Advance(TimeSpan.Zero)` would wake a zero-due-time waiter (research §1.4 read the upstream `WakeWaiters` implementation and found an inclusive comparison), with `Advance(TimeSpan.FromTicks(1))` as the fallback if that comparison turned out to be strict — addressed the other direction, so the `FromTicks(1)` fallback is not needed and is withdrawn. Recorded in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md`.
  - The MSTest host remains pump-less, so the existing `SynchronizationContext.Current.Should().BeNull()` arrangement assertion stays valid.
- Constraints (budget, performance, compatibility):
  - Bugfix minimal-change rule: no opportunistic refactors, no production API widening beyond the single seam.
  - Test-only wherever possible; the one production file is justified above.
  - net481 / legacy (non-SDK) project format: every new source file needs an explicit `<Compile Include>` entry.
  - File ownership: QuickFiler/ production sources and the push-down-owned .claude/.codex/.agents/config paths are off-limits.
- External dependencies (services, libraries, releases):
  - `Microsoft.Bcl.TimeProvider` 10.0.11 and `Microsoft.Extensions.TimeProvider.Testing` 10.9.0, both already in use by UtilitiesCS.Test. The testing package declares exactly one net462 dependency (`Microsoft.Bcl.TimeProvider >= 8.0.1`), satisfied by the 10.0.11 pin, so two package entries suffice (research §1.5).

## Data / API / Config Impact
- User-facing or API changes: none. `NonBlockingDelay` is `internal`; the added overload is not part of any public surface.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): no CLI flag, runsettings, or config-schema change. CI's vstest invocation is unchanged, including its absence of a `/Settings:` argument — which is why `[DoNotParallelize]` (an in-source attribute) is the correct remedy rather than a runsettings edit.

## Test Strategy

Derived from research §8.

**Finding 1 — rewrite in place, plus one added test.** Rewrite the two existing methods in `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`; do not leave a third variant of the old wall-clock test behind. Ordering is mandatory for a non-zero due time: at a non-zero due time `FakeTimeProvider.CreateTimer` does not invoke the callback at creation, so such a test must **start the task, assert it has not completed, `Advance`, then `await`** (research §1.4). A zero due time is the exception, confirmed by execution: there the callback is invoked during `CreateTimer`, so the task is already completed when the overload returns and no advance step exists (see the Assumptions bullet above). For `WaitAsync_WithNoDispatcher_CompletesAfterInterval` this is a strictly stronger assertion than the current `Stopwatch` check, because it proves the task cannot complete *early*, which elapsed-time assertion never did. For `WaitAsync_ZeroDelay_CompletesWithoutPump` no `Advance` call is used at all: the executed run recorded in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md` showed the task already completed on return, so that test asserts completion immediately after the call and then awaits it; the previously documented `Advance(TimeSpan.FromTicks(1))` fallback addressed the opposite direction of error and is withdrawn. Keep the existing `[Timeout(5000)]` attributes: after the fix they are a harness deadlock bound, not a wait.

**Finding 1 — coverage of both overloads (condition from research §7 now resolved).** Research §7 flagged that a direct test of the 1-arg overload is needed *only if* `StoreRehookCoordinator`'s existing tests do not already exercise it transitively. That condition was checked on 2026-09-02: TaskMaster.Test/AppGlobals/StoreRehookCoordinatorTests.cs constructs the coordinator at lines 60 and 259 and **both call sites pass `_ => Task.CompletedTask` for the trailing `delay` parameter**, so the `delay ?? NonBlockingDelay.WaitAsync` fallback is never taken and the 1-arg body would be left uncovered. **A direct test of the 1-arg overload is therefore required**, otherwise coverage on changed production lines regresses. It should assert completion under the existing `[Timeout]` bound without asserting on elapsed time. `TimeSpan.Zero` is the interval used, so the real-clock one-shot timer is due immediately and the test adds no measurable wait (a completion assertion, not a duration assertion, so no wall-clock dependency is reintroduced).

**Finding 2 — one guard class per test assembly, metadata-only, no instantiation.** The status of the two guards differs and must be documented rather than conflated:
- `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` is **green from birth**. Because the `Form` sources were never compiled into that assembly (research §2.1, §5 N2), it is regression *prevention*, not a fail-before/pass-after regression test. No red run exists or can be produced there, and no reviewer should expect one. Deleting the orphan sources rather than gutting the `[Ignore]`d method bodies is deliberate: gutting would leave `Form` sources on disk one csproj line away from re-entering the assembly.
- `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` is a **genuine red-before / green-after regression test** (research §2.2, §5 N3). The guard must be observed failing against the current csproj — naming the two `Form`-derived types — before the deletions, and passing after. That failing run is the fail-before evidence and is recorded under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/`.

**Finding 3 — attribute-only change, no new test, with a fail-before exception dossier.** The hazard is a race requiring a specific interleaving of `Console.SetOut` across two threads, so a deterministic red run is not producible. Record a fail-before exception dossier at `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/fail-before-exception.<timestamp>.md` with a `WhyFailingRunImpossible` section stating that constraint, and cite the two in-repo precedent classes as the alternative proof that the hazard is real and was previously observed: UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs lines 14-20 and UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs lines 17-21. The new hazard comments should reuse that precedent wording but cite UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21 as the live source of the parallel scope, because the precedent comments' reference to TaskMaster.runsettings is stale — CI passes no `/Settings:` argument, so the assembly-level attribute is what actually takes effect (research §0).

**Finding 4 — no test change.** Out of scope; tracked by #743.

- Regression tests to add or update: `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` (rewrite two, add one); `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` (new, red-before/green-after); `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` (new, green-from-birth prevention guard).
- Unit tests for the fixed behavior and boundaries: `FakeTimeProvider`-driven completion for the 2-arg overload; direct completion test for the 1-arg overload; empty-`Form`-set assertion in both guard assemblies.
- Edge cases and negative scenarios: zero-length delay (`TimeSpan.Zero`); the not-completed-before-`Advance` negative assertion; `ReflectionTypeLoadException` degradation in the guard; the compile-time negative case that the method-group conversion at StoreRehookCoordinator.cs line 102 must not regress to CS0123.
- Error handling and logging verification: not applicable — no error-handling or logging behavior changes.
- Coverage impact and targets for changed lines/modules: `TaskMaster/AppGlobals/NonBlockingDelay.cs` is the only production file with changed covered lines; both overloads must be directly exercised so coverage on changed lines does not regress. Test-only files do not enter the production coverage denominator.
- Toolchain commands to run (format → lint → type-check → test):
  1. `nuget restore` (the `packages/` directory is absent from this worktree)
  2. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`) — note that CSharpier 1.2.6 formats packages.config, so this step will reformat the edited file and the loop must restart from step 1 when it does
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  5. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation`
- Manual validation steps (if required): for the local test run, exclude \.claude\ worktree copies from the discovered assembly list and pass `/InIsolation` to match CI; an empty failure message with sub-millisecond duration indicates an assembly-load problem, not a regression.

## Acceptance Criteria

- [x] AC1 — `TaskMaster/AppGlobals/NonBlockingDelay.cs` declares both `public static Task WaitAsync(TimeSpan delay)` and `public static Task WaitAsync(TimeSpan delay, TimeProvider timeProvider)`, neither declaring an optional parameter, and the 1-arg overload delegates to the 2-arg overload passing `TimeProvider.System`.
- [x] AC2 — The 2-arg overload schedules its one-shot completion via `timeProvider.CreateTimer(callback, null, delay, Timeout.InfiniteTimeSpan)` returning `ITimer` instead of `new System.Threading.Timer(...)`; the callback still disposes the timer and completes the `TaskCompletionSource`, and `TaskCreationOptions.RunContinuationsAsynchronously` is retained.
- [x] AC3 — The `#nullable enable annotations` / `#nullable restore annotations` pragma pair around the nullable timer local in `TaskMaster/AppGlobals/NonBlockingDelay.cs` (at lines 52-54 before the change) is preserved, and the nullable-rebuild gate emits no CS8632 for that file.
- [x] AC4 — The method-group conversion `_delay = delay ?? NonBlockingDelay.WaitAsync;` at TaskMaster/AppGlobals/StoreRehookCoordinator.cs line 102 and the direct invocation at TaskMaster/AppGlobals/AppEvents.cs line 456 both still compile with no CS0123 and no source change to either file. (These are the complete set of production call sites per research §5 N1: exactly two, exactly one of which is a method-group conversion.)
- [x] AC5 — `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` contains no `Stopwatch` reference and no `System.Diagnostics` using directive. The rewritten `WaitAsync_WithNoDispatcher_CompletesAfterInterval` injects a `FakeTimeProvider`, asserts the returned task is not completed before `Advance`, and completes after it. The rewritten `WaitAsync_ZeroDelay_CompletesWithoutPump` injects a `FakeTimeProvider` and asserts the returned task is already completed when control returns from the 2-arg overload, because a zero-due-time one-shot timer is invoked during `CreateTimer`, then awaits it and asserts `RanToCompletion`. Both retain `[Timeout(5000)]` as a deadlock bound.
- [x] AC6 — Both `WaitAsync` overloads are directly covered by `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`: the 2-arg overload via `FakeTimeProvider` and the 1-arg overload via a dedicated test asserting completion (not elapsed duration). Required because StoreRehookCoordinatorTests supplies an explicit `delay` at both construction sites and never reaches the `NonBlockingDelay.WaitAsync` fallback.
- [x] AC7 — `TaskMaster.Test/TaskMaster.Test.csproj` and `TaskMaster.Test/packages.config` each reference `Microsoft.Bcl.TimeProvider` 10.0.11 and `Microsoft.Extensions.TimeProvider.Testing` 10.9.0, with `HintPath`/`targetFramework` values mirroring the UtilitiesCS.Test entries, and TaskMaster.Test/app.config is unmodified.
- [x] AC8 — The orphan files `UtilitiesCS.Test/ResourceTests.cs`, `UtilitiesCS.Test/Form1.cs`, `UtilitiesCS.Test/Form1.Designer.cs`, `UtilitiesCS.Test/Form1.resx`, `UtilitiesCS.Test/Form2.cs`, `UtilitiesCS.Test/Form2.Designer.cs`, `UtilitiesCS.Test/Form2.resx`, `UtilitiesCS.Test/Form3.cs`, `UtilitiesCS.Test/Form3.Designer.cs`, and `UtilitiesCS.Test/Form3.resx` no longer exist on disk.
- [x] AC9 — `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` exists in namespace `UtilitiesCS.Test`, ports the QuickFiler.Test guard including its `GetLoadableTypes` `ReflectionTypeLoadException` fallback, is registered by a `<Compile Include>` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and passes. Its green-from-birth status (regression prevention, not fail-before/pass-after) is stated in the delivery record so no reviewer expects a red run for it.
- [x] AC10 — `SVGControl.Test/Form1.cs`, `SVGControl.Test/Form1.Designer.cs`, `SVGControl.Test/Form1.resx`, `SVGControl.Test/Form2.cs`, `SVGControl.Test/Form2.Designer.cs`, and `SVGControl.Test/Form2.resx` no longer exist on disk, and `SVGControl.Test/SVGControl.Test.csproj` no longer contains `<Compile>` entries for `Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs` or `<EmbeddedResource>` entries for `Form1.resx`, `Form2.resx`. (These are the complete set of `Form`-derived types compiled into that assembly per research §5 N3.)
- [x] AC11 — `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` exists in namespace `SVGControl.Test`, is registered by a `<Compile Include>` entry, and passes; a failing (red-before) run of that guard against the pre-deletion csproj — naming the two `Form`-derived types — is recorded under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/`.
- [x] AC12 — No `System.Windows.Forms.Form`-derived type is compiled into either `UtilitiesCS.Test` or `SVGControl.Test`, demonstrated by both guard tests passing in the final full test run.
- [x] AC13 — `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` and `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` each carry a class-level `[DoNotParallelize]` attribute preceded by a hazard comment matching the precedent wording in PrettyPrint_Tests.cs and OlTableExtensions_Tests.cs, citing UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21 as the live source of the class-level parallel scope. No test body, assertion, or test-method name in either file is otherwise changed. (Per research §5 N4 these are the only two compiled `UtilitiesCS.Test` classes that capture, restore, and assert on `Console.Out` without the attribute.)
- [x] AC14 — `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` no longer exists on disk, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains no reference to it.
- [x] AC15 — A fail-before exception dossier exists at `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/fail-before-exception.<timestamp>.md` for Finding 3, containing a `WhyFailingRunImpossible` section explaining that the failure requires a specific `Console.SetOut` interleaving across two threads, and citing the two in-repo precedent classes as the alternative proof of the hazard.
- [x] AC16 — Finding 4 (pump-hosted `QfcItemController` / `PumpTimeoutMs` load sensitivity) is recorded in this spec as explicitly out of scope, with the four verified reasons no test-only fix exists, and is linked to follow-up issue **#743** (docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md), including the note that the prior standalone tracker #711 was closed as superseded by #729.
- [x] AC17 — UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs is unmodified: `PrintTree` gains no `TextWriter` parameter and no other production seam is added for Finding 3.
- [x] AC18 — `TaskMaster/AppGlobals/NonBlockingDelay.cs` is the only non-test production source file modified by this change; every other modified file belongs to a test project (source, project file, or packages.config) or to this feature's documentation and evidence folder.
- [x] AC19 — No file under QuickFiler/ production sources is added, modified, or deleted.
- [x] AC20 — No file under .claude/**, .codex/**, .agents/**, config/blast-radius.json, or config/orchestration-routing.json is added, modified, or deleted.
- [x] AC21 — The full C# toolchain passes clean in a single final pass, in order: `dotnet tool run csharpier check .` reports no unformatted files; the analyzer rebuild (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) succeeds with no new diagnostics; the nullable rebuild (`/p:TreatWarningsAsErrors=true`) succeeds; and `vstest.console.exe ... /EnableCodeCoverage /InIsolation` reports zero failed tests. The commands run and their results are stated in the completion report.

## Risks & Mitigations
- Technical or operational risks:
  - **CS0123 regression.** If the seam is implemented as an optional parameter rather than an overload pair, the solution stops compiling at StoreRehookCoordinator.cs line 102. Mitigated by AC1 (no optional parameter on either overload) and AC4 (both call sites verified compiling), and detected by the mandatory `/t:Rebuild` gates.
  - **`FakeTimeProvider` zero-delay semantics — resolved by execution, no longer open.** This was the one behavior held on source reading rather than on an executed run. The run recorded in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md` executed it twice with the same result: `CreateTimer` invokes a zero-due-time one-shot callback during creation, so the returned task is already completed on return and `Advance` is not required. The anticipated `Advance(TimeSpan.FromTicks(1))` fallback addressed the opposite direction of error and is withdrawn. The risk is closed; `[Timeout(5000)]` remains on both tests as a deadlock bound.
  - **CS8632 from the nullable gate.** The 2-arg overload introduces a second nullable local. Mitigated by AC3's requirement to scope the `annotations`-only pragma pair the same way the existing code does.
  - **`packages/` absent from the worktree.** A build attempted before `nuget restore` fails on the `<Error Condition="!Exists(...)">` package-restore guards rather than on the change itself. Mitigated by making `nuget restore` the first toolchain step.
  - **CSharpier reformats packages.config.** The formatter will rewrite the edited file, which under the repository's restart rule requires restarting the toolchain loop. Mitigated by expecting the restart rather than treating it as a failure.
  - **Deleting `SVGControl.Test` forms breaks its build.** If the six file deletions and the csproj entry removals are not applied together, the project fails to compile on a missing source file. Mitigated by treating the deletion and the csproj edit as one indivisible change.
  - **Coverage regression on changed production lines.** Mitigated by AC6's requirement that both overloads be directly covered.
- Mitigations and rollbacks: the change is a plain revert away from the pre-change state; no data, schema, or configuration migration is involved and the one production signature that existing callers bind to is unchanged.

## Rollout & Follow-up
- Release/rollout steps: merge to main through the standard PR gate. No deployment, feature flag, or staged rollout is involved — the production change is an internal helper overload and everything else is test-side.
- Post-fix monitoring or clean-up tasks:
  - Watch CI for any recurrence of an empty-captured-output or `ObjectDisposedException` failure in `UtilitiesCS.Test`, which would indicate a third unprotected `Console.Out`-capturing class was introduced after this change.
  - UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs remains a potential stale-writer-leak source (failure mode 2 only, no failing mode of its own). It is intentionally left unmarked; revisit only if an `ObjectDisposedException` cascade is actually observed.
  - Issue #743 carries Finding 4 forward. Do not close it as part of #729.
- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/729
  - Follow-up issue (Finding 4): https://github.com/drmoisan/TaskMaster/issues/743 — supersedes the previously closed #711
  - Source issues consolidated here: #694 (Finding 1), #586 (Finding 2), #520 (Finding 3), #711 (Finding 4, now re-promoted as #743)
  - Research artifact: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md
  - Governing policy: .claude/rules/general-unit-test.md § Determinism Infrastructure

## Write Set

Every file this plan's diff creates, modifies, or deletes, reproduced from the plan's "Complete file-write inventory" section. Nothing else belongs in this list: no scope exclusion, no model reference, and no context reference is a write-set entry.

- `TaskMaster/AppGlobals/NonBlockingDelay.cs`
- `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` (contains a space)
- `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`
- `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`
- `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs`
- `TaskMaster.Test/TaskMaster.Test.csproj`
- `TaskMaster.Test/packages.config`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `SVGControl.Test/SVGControl.Test.csproj`
- `UtilitiesCS.Test/ResourceTests.cs`
- `UtilitiesCS.Test/Form1.cs`
- `UtilitiesCS.Test/Form1.Designer.cs`
- `UtilitiesCS.Test/Form1.resx`
- `UtilitiesCS.Test/Form2.cs`
- `UtilitiesCS.Test/Form2.Designer.cs`
- `UtilitiesCS.Test/Form2.resx`
- `UtilitiesCS.Test/Form3.cs`
- `UtilitiesCS.Test/Form3.Designer.cs`
- `UtilitiesCS.Test/Form3.resx`
- `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs`
- `SVGControl.Test/Form1.cs`
- `SVGControl.Test/Form1.Designer.cs`
- `SVGControl.Test/Form1.resx`
- `SVGControl.Test/Form2.cs`
- `SVGControl.Test/Form2.Designer.cs`
- `SVGControl.Test/Form2.resx`
