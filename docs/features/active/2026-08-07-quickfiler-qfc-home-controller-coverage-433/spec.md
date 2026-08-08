# quickfiler-qfc-home-controller-coverage — Spec

- **Issue:** #433
- **Parent:** Epic `quickfiler-per-file-coverage` (issue #136) — child **F7**, wave 1, complexity band C3
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T21-15
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** `full-feature` (this file and `user-story.md` are together the authoritative acceptance-criteria source)
- **Base commit at authoring time:** `74be1964`
- **Depends on:** F1 `quickfiler-coverage-denominator-and-exemption-ledger` (wave 0) — **not present on disk at authoring time**

## Overview

This is an **enabler** feature. It carries the QuickFiler `QfcHomeController` file set to the epic's
per-file coverage standard so that the session-lifecycle, metrics, and queue-refill code can be
changed by a maintainer or an autonomous agent without silent regression escapes.

`QfcHomeController` is the top-level lifecycle coordinator for a QuickFiler filing session: it wires
the data model, explorer controller, form viewer, keyboard handler, and UI queue; starts the session
in either normal or high-confidence mode; drains the background refill queue; and accumulates session
metrics. Its three implementation partials carry the ordering and state-transition invariants of the
whole feature, and today several of their most consequential paths — the error branch of the
background-worker completion handler, the entire metrics drain method, all three exception handlers of
the background refill, and every null-guard short-circuit in the mode selection — are executed by no
test at all.

Epic context and the specific epic obligations this child inherits:

- Per-file line coverage is the unit of success, not assembly aggregate
  (`epic.md` § Shared Design 6). Aggregate coverage alone satisfies nothing here.
- Seam hierarchy is strict: interface seam > injectable delegate > adapter
  (`epic.md` § Shared Design 2, `.claude/rules/csharp.md` § DI Seams).
- No behavior change to end-user QuickFiler flows; no production file over 500 lines; tests
  deterministic, isolated, free of temporary files, live forms, and external services
  (`epic.md` front matter `nfrs`).
- The file set of every wave-1 child is **disjoint** from every sibling's
  (`epic.md` § Decomposition Rationale). Editing a sibling-owned file is a scope violation, not a
  convenience.

## Scope

### In scope — five production files, with F1 ledger classification

| # | File | Lines at `74be1964` | Expected F1 classification | Work required by this child |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | 487 | `testable` | Mandatory partial split, seams S1/S2, new tests |
| 2 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 234 | `testable` | Seams M1/M2/M3/M4, new tests |
| 3 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 86 | `testable` | New tests only — **zero production edits** |
| 4 | `QuickFiler/Controllers/IQfcHomeController.cs` | 20 | `interface-only / not-measured` | **None** beyond recording the ledger row and harness output |
| 5 | `QuickFiler/Interfaces/IFilerHomeController.cs` | 45 | `interface-only / not-measured` | **None** beyond recording the ledger row and harness output |

One new production file is created (see § Mandatory Partial Split):
`QuickFiler/Controllers/QfcHomeController.Properties.cs`, expected classification `testable`.

New test files are created under `QuickFiler.Test/Controllers/`, mirroring the production tree per
`.claude/rules/general-unit-test.md` § Test File Location.

The interface classification is not an assumption of convenience. Both interface research artifacts
establish zero executable IL-producing content three independent ways:

1. Exhaustive source construct check — every member declaration terminates in `;`; there is no member
   body, no `static` member, no `const`, no attribute of any kind, no nested type
   (`IQfcHomeController.cs.research` § 2.1; `IFilerHomeController.cs.research` § 2.1).
2. Target framework — `QuickFiler/QuickFiler.csproj:13` is `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`.
   Default interface implementations require CLR support the .NET Framework runtime does not provide,
   so Roslyn rejects them regardless of `LangVersion=preview` (`csproj:14`). These files cannot
   silently acquire executable content by a later edit
   (`IQfcHomeController.cs.research` § 2.2; `IFilerHomeController.cs.research` § 2.2).
3. Committed instrumented Cobertura artifact — no `<class>` element exists for either file, and no
   `QuickFiler.*I<Uppercase>` class element exists anywhere in the report, while the concrete class
   `QuickFiler.Interfaces.MailItemActionsAdapter` **is** present as a positive control proving the
   instrumenter reaches the `QuickFiler\Interfaces\` folder
   (`IFilerHomeController.cs.research` § 2.3; `IQfcHomeController.cs.research` § 2.3).

Both files match the `.claude/rules/general-unit-test.md` § Coverage Requirements carve-out for
"C# interface-only files" verbatim. The COM/VSTO exemption is **not** the correct instrument for
them — that exemption governs lines that are executable but hard to reach, and these files have no
executable lines at all.

### Out of scope

- **Any edit to `IQfcHomeController.cs` or `IFilerHomeController.cs`.** See § Cross-Child Contract
  Notes for the verified reasons this is both unnecessary and prohibited.
- **Shape-assertion and reflection-shape tests** against either interface. Explicitly rejected on the
  record (`IQfcHomeController.cs.research` § 4.1; `IFilerHomeController.cs.research` § 4.1): they buy
  zero coverage because the files emit no IL and test-assembly lines are excluded from measurement by
  policy; they duplicate a check the compiler already performs more strongly; they have no unit of
  behavior under test; and they would convert a legitimate future coordinated interface change into a
  spurious failure in this child's suite. A test asserting that
  `EfcHomeController.QuickFileMetrics_WRITE` throws `NotImplementedException` is rejected for the
  additional reason that `EfcHomeController.Metrics.cs` is sibling **F8**-owned.
- **Removal of the dead `Iterate()` / `Iterate2()` methods.** See § Decision: Dead Iteration Methods.
- **Fixes for the latent defects listed in § Out-of-Scope Defects.**
- **Removal of unused `using` directives, of the commented-out members of `IFilerHomeController`
  (lines 29, 34, 40), or of the orphan uncompiled file `QuickFiler/Interfaces/IQfcHomeController.cs`.**
  All are zero-coverage-value diffs on files that several siblings read.
- **`coverage.config`, any shared build property file, and any sibling-owned file** (enumerated in
  § Sibling-Owned Files).
- **Any change to repository coverage thresholds.**

## Upstream Contract: What F1 Delivers and How This Child Consumes It

F1 is a genuine dependency, not stylistic ordering. This child consumes exactly two F1 deliverables
and produces evidence against both.

### 1. The classification ledger (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`)

F1's ledger is the **sole authority** on whether each of the five in-scope files is `testable` or
`interface-only / not-measured` or `ratified-exempt`. This child's coverage acceptance criteria apply
**only to files F1 classifies `testable`**.

Consumption is a Phase-0 **read**, never an edit — F1 owns the ledger file.

Halt conditions, to be checked before any production or test edit:

- If any of files 1-3 is not classified `testable`, the seam and test recommendations for that file
  are void and this spec must be revised before work proceeds.
- If either interface file is classified `testable`, that classification is unsatisfiable on the
  present content (a zero-line denominator has no line rate to raise); halt and escalate to the epic
  orchestrator rather than inventing coverage work.
- If F1's shared seam convention mandates interface seams uniformly and disallows the
  `.claude/rules/csharp.md` § DI Seams delegate carve-out, the seam set in § Required Seams must be
  re-formed and this spec revised.

Two open mechanism questions belong to F1 and must be answered in Phase 0 rather than decided here:

- **Ledger row format for zero-line files** — does the harness emit a row for a file the Cobertura
  report omits entirely, and does F1 want `0/0` or a `not measured` label? This determines the exact
  shape of the evidence artifact for files 4 and 5.
- **Ledger rows for files created mid-wave (CC-A2 / CC-B2)** — F1's ledger is authored against the
  121 files compiled today, so `QfcHomeController.Properties.cs` will have no row. F9, F11 and F13 are
  all expected to create partial files in the same wave, so this is a shared mechanism question. The
  capstone F16 verifies that every compiled file is either at target or on the ledger; a file with no
  row would fail that check.

### 2. The per-file coverage harness

F1 delivers the repeatable per-file line-coverage report derived from the Cobertura output of
`Invoke-MSTestWithCoverage.ps1`. It is the **only accepted evidence mechanism** for every per-file
figure in this spec. No substitute harness may be constructed by this child, and no aggregate
assembly figure substitutes for a per-file figure.

Two runs are required:

- **Baseline** — F1's harness at the child's merge base, recorded under
  `<FEATURE>/evidence/baseline/`, capturing line rate and branch rate for each `testable` file plus
  the repository-wide line rate, with the exact command and `EXIT_CODE`.
- **Final** — F1's harness after all work, recorded under `<FEATURE>/evidence/qa-gates/`, in the same
  shape. This is the artifact the acceptance criteria are evaluated against.

### 3. Indicative figures are planning inputs only

The following figures come from a Cobertura artifact committed by a **different feature** (#424) at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.
They were produced against substantially the current file content and are sound for gap sizing. They
are **not** F1 harness output and must not be cited as acceptance evidence.

| File | Indicative line rate | Indicative branch rate | Source |
| --- | --- | --- | --- |
| `QfcHomeController.cs` | ~71.39% | ~51% | Cobertura line 21643 |
| `QfcHomeController.Metrics.cs` | ~65.09% | ~62.5% | Cobertura line 22314 |
| `QfcHomeController.Iteration.cs` | ~86.25% (estimate) | ~66.67% | Cobertura line 22612 |

The `Iteration.cs` figure is load-bearing for how this child is scoped: the file is plausibly
**already above** the epic's 80% per-file line floor
(`QfcHomeController.Iteration.cs.research` § 3.4). Its value in this child is therefore **branch
coverage, error-path coverage, and behavior pinning**, not headline line coverage. Acceptance for that
file is worded around the specific named uncovered branches, not around a line-rate target that may be
vacuously satisfied.

The 51% branch rate on `QfcHomeController.cs` has a single dominant cause: seven lambda-cache
conditions (source lines 163, 172, 181, 189, 197, 210, 243) sit at 0% purely because the private
parameterless constructor at line 30 is never invoked. One test lifts all seven
(`QfcHomeController.cs.research` § 3.1).

## Behavior

No end-user-observable behavior changes. This child delivers:

1. **Test coverage** — new MSTest test classes in new files under `QuickFiler.Test/Controllers/`
   covering the currently-unexecuted paths of the three testable implementation files.
2. **Testability seams** — a minimal set of `internal` members on the `QfcHomeController` class, each
   with a production default that reproduces today's behavior exactly.
3. **One mechanical partial split** — relocation of an existing `#region` into a new partial file to
   stay under the 500-line limit once the seams are added.
4. **Evidence** — per-file coverage figures, file-size measurements, frozen-file hashes, and toolchain
   results committed under `<FEATURE>/evidence/`.

Every production change is behavior-preserving by construction: each seam's default is the exact
expression it replaces, the pure-function extraction reproduces current semantics including its
defects, and the partial split moves source text without altering any declaration.

## Required Seams

Ranked per `.claude/rules/csharp.md` § DI Seams, with the epic's addition that a **pure-function
extraction that removes the need for any seam outranks all three rungs** (rank 0). Each entry states
why the next-lower-cost rung does not suffice.

All seams are `internal` members declared on the `QfcHomeController` class. The enabling mechanism is
already present: `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at
`QuickFiler/Controllers/QfcHomeController.cs:18` (verified on disk), plus
`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` at
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11` so Moq can proxy internal QuickFiler types
where needed. Seven existing loader delegates
(`QfcDataModelLoader`, `QfcAsyncDataModelLoader`, `QfcExplorerControllerLoader`,
`QfcKeyboardHandlerLoader`, `QfcQueueLoader`, `QfcFormControllerLoader`,
`HighConfidencePreFilterLoader`, at `QfcHomeController.cs:159-244`) plus the injectable `TimeProvider`
(`QfcHomeController.Metrics.cs:17`) are the ratified in-repo precedent, and **none of them appears on
either interface**.

### Required set

| ID | Seam | Declared in | Rung | Enables | Why a lower rung does not suffice |
| --- | --- | --- | --- | --- | --- |
| **S1** | `internal Action<string> ShowUserMessage { get; set; } = msg => MessageBox.Show(msg);` | `QfcHomeController.cs` | 2 — injectable delegate | The `e.Error != null` arm of `Worker_RunWorkerCompleted` (source lines 335, 337-339, currently 0%) | No lower rung exists. `MessageBox.Show` is static, modal, and blocks on human interaction — an outright unit-test-policy violation, so without a seam the branch is permanently untestable. Rung 1 (interface) is declined under the rule's own carve-out for "a single call path when a full interface is excessive" (`csharp.md` § DI Seams): one call path, one `string` parameter, no expected second implementation. `Tags/IUserPrompt.cs` exists but **QuickFiler does not reference the Tags project** (verified: `QuickFiler.csproj` references only SVGControl, TaskVisualization, ToDoModel, UtilitiesCS), so there is no interface to reuse and introducing one would add a compiled file to the epic denominator for a single void method. |
| **S2** | `internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;` | `QfcHomeController.cs` | 2 — injectable delegate | `TimedConsumerAsync` (source lines 363, 365-384, 386 — **22 lines, the largest single uncovered block in the file**) | The alternative is writing a real file, which `.claude/rules/general-unit-test.md` prohibits outright, and `FileIO2`'s API cannot be redirected to a memory stream. Rung 1 would require introducing an interface into `UtilitiesCS`, widening the change into another project outside F7's file set. A method-group default means there is no default lambda body left uncovered. |
| **M1** | `internal static (double duration, string durationText, string durationMinutesText) BuildDurationTexts(TimeSpan elapsed, int emailsLoaded, IFormatProvider formatProvider = null)` | `QfcHomeController.Metrics.cs` | **0 — pure-function extraction, no seam** | The duration arithmetic and formatting shared by source lines 42/48-56 and 121/127-135, including the `emailsLoaded > 0` FALSE branch and all numeric formatting, none of which is exercised with a non-zero input today | There is no lower-cost option and **no seam of any rung can work**: `System.Diagnostics.Stopwatch.Elapsed` is a non-virtual property on a concrete class, so no interface, delegate, or adapter can control it without *replacing* the stopwatch field — and replacing it is blocked by two live pins (`QfcHomeControllerRunAsyncTests.RunAsync_ExecutesCorrectly:303` asserts `StopWatch.IsRunning`; `QfcHomeControllerPropertyTests.StopWatch_PropertyWorksCorrectly:232` asserts identity). Extraction leaves one unavoidable wiring line and moves 100% of the decision logic into a testable pure function. Ratified shape precedent in the same assembly: `EfcHomeController.BuildQuickFileMetricLines` (`EfcHomeController.Metrics.cs:55-85`), covered by `EfcHomeControllerMetricsTests.cs:20-61`. |
| **M2** | `internal Action<string, string[], string> MetricsLineWriter { get; set; } = FileIO2.WriteTextFile;` | `QfcHomeController.Metrics.cs` | 2 — injectable delegate | The disk write in `QuickFileMetrics_WRITE` (source lines 84-87) with real content | `FileIO2.WriteTextFile` is a `static` method that opens a `StreamWriter` per output line. Statics are not mockable and temporary files are prohibited. Today the write is only survivable because every existing test returns `Array.Empty<string>()` from `GetMoveDiagnostics`, so the loop iterates zero times — **the moment any test returns a non-empty diagnostics array, a real file is written**. Rung 1 declined: one call site, one three-argument operation, no expected second implementation. This exact delegate shape is already ratified in this assembly as `EfcHomeControllerDependencies.MetricsLineWriter`. |
| **M3** | `internal Func<string, int, CancellationToken, bool> MetricsAdder { get; set; }`, defaulted to `(line, timeoutMs, ct) => _metrics.TryAdd(line, timeoutMs, ct)` | `QfcHomeController.Metrics.cs` | 2 — injectable delegate | The `TryAdd`-returns-`false` retry loop (source lines 205-212, 225) and the uncancelled-`OperationCanceledException` back-off (219-223) | The `else` branch at 219-223 is **unreachable through the real `BlockingCollection<T>`**: `TryAdd(T,int,CancellationToken)` only lets an `OperationCanceledException` escape when the caller's own token is cancelled, so `catch (OperationCanceledException)` with `ct.IsCancellationRequested == false` cannot be produced by any arrangement of the concrete type. `TryAdd` is also non-virtual, so subclassing cannot intercept it. Rung 3 (adapter) would add a type and a file for one method at higher cost than the delegate. |
| **M4** | Visibility widening: both `NonBlockingProducer` overloads (`QfcHomeController.Metrics.cs:190`, `:201`) `private` → `internal` | `QfcHomeController.Metrics.cs` | **not a seam — lowest cost of all** | Direct compile-checked invocation of both producer overloads by five proposed tests | The alternative is `MethodInfo.Invoke`, which for `async Task` methods requires unwrapping a `Task` from `object` and converts every argument-shape mistake into a run-time `TargetInvocationException` instead of a compile error. `.claude/rules/csharp.md` prefers `internal` for non-public APIs; `InternalsVisibleTo` is already declared, so the public surface is unchanged. |

`QfcHomeController.Iteration.cs` requires **no seam of any kind**. Every dependency it reaches
already resolves at rung 1: `IQfcDatamodel` declares `Complete`, both `DequeueNextItemGroupAsync`
overloads, and `DequeueNextItemGroup`; `IQfcQueue` declares `EnqueueAsync`, `CompleteAddingAsync`, and
`Dequeue`; `IQfcFormController` declares `ItemsPerIteration`, `Groups`, and both `LoadItems`
overloads. `Globals` has an `internal` setter, `CreateCancellationToken()` is `internal` with a public
`TokenSource` getter, and the stopwatch fields are seeded by reflection following the pattern all six
existing home-controller suites use (`QfcHomeController.Iteration.cs.research` § 6). This file's diff
in this child is **one new test file and nothing else**.

### Two distinct writer seams — do not conflate

S2 (`MetricsFileWriter`) and M2 (`MetricsLineWriter`) are different seams for different call sites and
both are required:

- S2 is **async, four arguments** (`filename, string[], folderpath, CancellationToken`), matching
  `FileIO2.WriteTextFileAsync`, consumed by `TimedConsumerAsync` in `QfcHomeController.cs`.
- M2 is **synchronous, three arguments** (`filename, string[], folderRoot`), matching
  `FileIO2.WriteTextFile`, consumed by `QuickFileMetrics_WRITE` in `QfcHomeController.Metrics.cs`.

Both are declared locally on `QfcHomeController`. Neither reuses nor extends
`EfcHomeControllerDependencies`, which is sibling **F8**-owned and cited as precedent only.

### Conditional items — require explicit justification in the atomic plan, not settled here

| ID | Item | Gate |
| --- | --- | --- |
| **S3** | `internal IUiDispatcher UiDispatcher { get; set; } = new WpfUiDispatcher();` — a genuine rung-1 interface seam over `UiThread.Dispatcher.Invoke` (`QfcHomeController.cs:343`) | **Coverage gain: zero** (lines 343-347 are already hit). It is a policy-quality improvement only: it removes the existing suite's dependency on the process-global `UiThread.Init(false)` (`QfcHomeControllerRunAsyncTests.cs:329`), a UT4 mutable-global-state concern, but requires modifying an existing test. Adopt only if the plan justifies the modification and the line budget is comfortable. |
| **S4 / S5a / S5b** | `LaunchCoreAsync` extraction plus `QfcFormViewerLoader` and `UiSchedulerLoader` (Tier C), unlocking ~22 of the 34 uncovered `LaunchAsync` lines, plus a second partial split (`QfcHomeController.Lifecycle.cs`) | Gated on F1's disposition of `LaunchAsync`'s residual lines as irreducible host wiring. `ProgressTracker.Initialize()` constructs and **shows** a `ProgressViewer` form, and the controller is constructed *inside* the static method so no instance seam can be pre-assigned. **S5a and S5b must be adopted as a pair**: source line 136 `TaskScheduler.FromCurrentSynchronizationContext()` succeeds today only as a side effect of line 133 constructing a WinForms `Form` that auto-installs a `WindowsFormsSynchronizationContext`; injecting a mock viewer without also seaming the scheduler turns `InitAsync_InitializesCorrectly` into an `InvalidOperationException`. This coupling is load-bearing and is the single most likely way to break a passing test in this child. Tier C leaves ~8 uncovered lines in the thin wrapper as the honest irreducible remainder. |
| **D7** | Removing the duplicated `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)` at `QfcHomeController.Metrics.cs:84` by reusing `folderRoot` from line 33 | Behavior-identical, and it removes a **permanently-partial branch** from this child's own coverage number (the `false` arm of line 84 cannot be taken because line 33 already returned on that condition). A judgment call for the planner: it is a production edit inside a coverage child, justified only by the uncoverable-branch removal. |
| **TC18** | A test covering the default `QfcFormControllerLoader` body (source lines 220-229, 10 lines) | Depends on four early-return null guards inside sibling **F6**'s `QfcFormController.SetupDisposal.cs` (lines 24-30, 50-57, 77-80, 151-154). Treat as **buffer, not baseline** — the coverage target must hold without it, and it does. If F6 removes or narrows those guards mid-wave, drop this task rather than editing F6. |

## Mandatory Partial Split

**This is not optional.** The minimum required seam set (S1 + S2) adds ~15 physical lines to a file
that is 487 lines against a hard 500-line limit — projecting **502 lines**, a Blocking finding. Any
plan that adds S1 and S2 without the split violates `.claude/rules/general-code-change.md`
§ File Size Limit.

### The chosen split

Relocate the entire `#region Public Properties` block — source lines **406-485** of
`QfcHomeController.cs` at base commit `74be1964`, 80 physical lines — into a new partial file
`QuickFiler/Controllers/QfcHomeController.Properties.cs`. The moved members are the backing fields and
accessors for `ExplorerController`, `FormController`, `KeyboardHandler`, `DataModel`, `FilerQueue`,
`UiScheduler`, `StopWatch`, the `_formViewer` field, `CreateCancellationToken()`, `TokenSource`,
`Token`, `WorkerComplete`, and `UiSyncContext`.

Rationale:

- It is a **pure mechanical move** of a single `#region` with no cross-references outside the type.
- Line budget: main file `487 − 80 = 407`, `+15` for S1 and S2 = **422 / 500**, leaving 78 lines of
  headroom that absorbs Tier C later without a second split. The new file is ~95 physical lines.
- The new file arrives at high coverage on creation: 22 coverable lines, 18 covered today, reaching
  **100%** once `CreateCancellationToken()` is covered. This matters — a split chosen on cohesion
  grounds alone can create a file that fails the epic's own bar on creation. A
  `QfcHomeController.Seams.cs` holding source lines 159-245 was rejected for exactly that reason:
  measured against the indicative hit map it is 16 covered / 13 uncovered = **55%**.
- `[assembly: InternalsVisibleTo("QuickFiler.Test")]` (source line 18) **stays in
  `QfcHomeController.cs`**. The whole child's test access depends on that attribute remaining in a
  compiled file.

### Verified: the split has no effect on either interface contract

This is stated explicitly because eight of the twelve live members of `IFilerHomeController` and two
of the eight members of `IQfcHomeController` have their implementation inside the moved block:

1. **A partial class is one type.** The C# specification treats the parts of a partial declaration as
   a single declaration; the member set and the base/interface list are the union of all parts.
   Compilation emits exactly one `TypeDef` for `QuickFiler.Controllers.QfcHomeController` regardless
   of how many files declare parts of it.
2. **Interface implementation mapping is per-type, not per-file.** The CLR interface map binds, for
   example, `IFilerHomeController.get_UiSyncContext` to `QfcHomeController.get_UiSyncContext` by type
   and member; source file has no representation in that metadata. Relocating source text changes only
   the sequence points recorded in the PDB — that is, which file the coverage report attributes lines
   to — not the emitted contract, not the interface map, not the vtable layout.
3. **`EfcHomeController` (sibling F8) is entirely unaffected.** The split touches only
   `QfcHomeController`. **The split is therefore not a cross-child change**, which is the specific
   reassurance the `IFilerHomeController` contract note requires.
4. **Consumers are unaffected.** Every consumer binds through an interface or a concrete type name;
   none references a file.
5. **`InternalsVisibleTo` is assembly-scoped**, so the `internal` setter on `DataModel` and the
   `private` setter on `WorkerComplete` keep their semantics after the move.
6. **The existing reflection-based tests are unaffected.** They resolve members via
   `_controller.GetType()`, which returns the single `QfcHomeController` type object regardless of
   source partitioning.

### csproj implication — a known merge-conflict hotspot

The new partial requires a `<Compile Include="Controllers\QfcHomeController.Properties.cs" />` entry
in `QuickFiler/QuickFiler.csproj`. The three existing `QfcHomeController*` entries are at lines
**325-327** (verified on disk).

`QuickFiler.csproj` is **not** on the sibling-owned prohibited list, but this `ItemGroup` is a known
wave-1 merge-conflict hotspot: siblings **F9, F11 and F13** are all expected to add `<Compile>`
entries during the same wave. Mitigation, which the plan must encode as a task constraint:

- Add the entry as a **single-line diff**, inserted adjacent to lines 325-327 so the conflict region
  is minimal and mechanically resolvable.
- Make **no other change** to the csproj — no reordering, no whitespace normalization, no property
  edits.

### Alternative recorded and not adopted

Relocating the metrics block (source lines 353-386 of `QfcHomeController.cs`: `_metrics`,
`_metricsConsumers`, `_lockObject`, `_fileName`, `TimedConsumerAsync`) into
`QfcHomeController.Metrics.cs` is more cohesive and frees 34 lines. It is **not adopted**: it leaves
only 32 lines of headroom versus 78, does not accommodate Tier C, and moves 22 *uncovered* lines into
a partial that is itself only at an indicative 65.09% — safe only if the covering tests land in the
same change, which couples two file plans and makes intermediate states non-compliant.

## Test Design Requirements

### Framework and structure

- **MSTest** (`[TestClass]`, `[TestMethod]`, `Microsoft.VisualStudio.TestTools.UnitTesting`), **Moq**
  for mocks and stubs, **FluentAssertions** for assertions.
- **Arrange–Act–Assert**, with the three phases distinguishable.
- Every test carries a summary comment or XML doc stating the scenario and the expected outcome
  (`.claude/rules/general-unit-test.md` § Documentation).
- Test files live in `QuickFiler.Test/Controllers/`, mirroring the production tree. No test file may
  exceed 500 lines.

### Determinism — prohibited constructs

Prohibited in every new or modified test in this child:

- `Thread.Sleep`, `Task.Delay`, and any real wall-clock wait, poll loop, or timing hack.
- `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`. Time comes from the injected
  `TimeProvider` seam, supplied as `FakeTimeProvider` in tests.
- Unseeded randomness.
- Temporary files, real filesystem writes, and any external service, database, network, or process.
- Live Outlook COM objects. Interop types are reached only through Moq-able interop **interfaces**
  (`NameSpace`, `Folders`, `Folder`, `Items`, `AppointmentItem`, `MailItem`), following the ratified
  pattern in `UtilitiesCS.Test/OutlookObjects/Calendar/CalendarTests.cs:58-79`.
- Live WinForms `Form` construction, any `Show()`, any `MessageBox`, and any dependency on the UI
  thread or on `UiThread.Init`.
- Mutable process-global state that leaks between tests, including
  `SynchronizationContext.SetSynchronizationContext` in test Arrange.

Permitted with justification:

- `[Timeout(...)]` **strictly as a hang guard**, where the passing path consumes zero wall-clock time.
  The in-test doc comment must say so. It is not a substitute for a deterministic gate.
- Reflection to seed private fields and invoke private members, following the pattern all eight
  existing home-controller suites use. This is accepted as consistent with established practice, with
  the recorded liability that it couples tests to member names and breaks at run time rather than
  compile time on a rename.
- `[STATestMethod]` / `*.StaTests.cs` under the epic's STA last-resort clause is **not needed for any
  file in this child** — every remaining gap is reachable by seam, interface mock, or reflection.
  Introducing an STA-bound test here would be unjustified.

### Deterministic ordering for the fire-and-forget refill

`QfcHomeController.Iteration.cs:76` is `_ = IterateQueueAsync();`. The refill's first yielding point is
`await _datamodel.DequeueNextItemGroupAsync(...)`; everything before it executes synchronously on the
caller's thread. The required mechanism is therefore **antecedent gating with a
`TaskCompletionSource`**:

1. Arrange the mocked `DequeueNextItemGroupAsync` to return a `TaskCompletionSource<IList<MailItem>>`
   task the test owns and has not completed.
2. Call `Iterate2()`; it returns as soon as the refill suspends.
3. Assert synchronously, before completing the source, that the UI load ran and the enqueue did not.
   This is race-free by construction — the continuation is provably not runnable while its antecedent
   is incomplete. This is the ordering proof and costs zero wall-clock time.
4. Complete the source with `SetResult`; because it is created without
   `TaskCreationOptions.RunContinuationsAsynchronously`, the continuation runs inline.
5. Observe completion through a second `TaskCompletionSource<bool>` set in a mock `Callback`.

`Task.Yield()`, scheduler drains, and polling `Mock.Invocations.Count` in a loop are rejected as
non-deterministic under load.

### Proposed test set (planning input; the atomic plan is authoritative on task decomposition)

Each research artifact enumerates one row per test case with an Arrange/Act/Assert sketch and the
lines or branches it closes. Every proposed name was checked against all eight existing suites and
none duplicates an existing `[TestMethod]`.

| Target file | Proposed cases | New test files | Research section |
| --- | --- | --- | --- |
| `QfcHomeController.cs` | 15 required (TC1, TC2, TC4-TC10, TC12-TC17) plus TC3/TC11/TC18 as buffer | `QfcHomeControllerLifecycleTests.cs`, `QfcHomeControllerWorkerCompletionTests.cs`, `QfcHomeControllerMetricsConsumerTests.cs`, `QfcHomeControllerModeGuardTests.cs` | `QfcHomeController.cs.research` § 4 |
| `QfcHomeController.Metrics.cs` | 17 (A1-A9, B1-B2, C1, D1-D5) | `QfcHomeControllerMetricsCoverageTests.cs`, split by group if it approaches 500 lines | `QfcHomeController.Metrics.cs.research` § 4 |
| `QfcHomeController.Iteration.cs` | 12 (A1-A4, B1-B2, C1-C3, D1-D3) | `QfcHomeControllerIterationCoverageTests.cs` | `QfcHomeController.Iteration.cs.research` § 4 |

Duplication is prohibited. Each research artifact carries an explicit duplication guard listing the
assertions that already exist and must not be re-created, and an explicit list of rejected candidates
recorded so the planner does not re-add them. Two proposed cases assert strictly different
post-conditions from an existing test on the same member
(`Iteration.cs.research` C3 versus `QfcHomeControllerIterationTests.SwapStopWatch_ExecutesCorrectly`);
the plan task text must say so explicitly so a reviewer does not read it as re-authoring a frozen test.

## Frozen Test Files — Hard Constraint

Issue #424 has **already merged into this branch's base** (verified: `QfcHomeController.cs:292-305`
carries the `QfcScanProgressBandMapper` wiring, the 200 ms poll, and
`QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`). There is no live edit conflict, only a
set of pins this child must respect. #424 AC 12 requires the following files byte-unmodified:

| File | Obligation | Note |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | **Byte-unmodified.** Carries the exact-argument pin `DequeueNextItemGroupAsync(8, 2000)` at line 268 | **The single largest process risk in this child.** This is F7's primary existing suite for `Iteration.cs`, it sits next to the new file, and the natural instinct when adding an iteration test is to append to the existing class. |
| `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` | Byte-unmodified | Targets `QfcDatamodel` (F5 territory) and pins nothing about this child's files. |
| `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs` | Byte-unmodified | |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | Byte-unmodified | F6 territory in any case. |
| `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` | Diff constrained to its four already-applied overload-shape hunks — i.e. **no further modification**, and no re-shaping of its `Setup`/`Verify` matchers | |

**All new tests go in new test files.** The plan must name the new file explicitly in every test task,
and a Phase-0 task must record the SHA-256 of each frozen file so the final QA gate can prove
byte-identity.

### Additional #424 pins this child must not contradict

- Do **not** assert the retired two-argument `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` at
  the pre-UI call site in `RunAsync`; it contradicts
  `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`.
- Do **not** "harmonize" poll intervals — do not assert `200` at the post-UI site or `2000` at the
  pre-UI site.
- Do **not** duplicate or contradict the 0→30 progress-band assertions in
  `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand`.
- Do **not** assert that the legacy synchronous `Run()` uses the deadline/progress overload; #424
  deliberately left it on the legacy overload.
- Do **not** assert that `HighConfidencePreFilterLoader` is invoked from `RunAsync`. Issue #218
  established that dequeue-time admission owns high-confidence filtering and the pre-filter must not
  run in `RunAsync`. TC17 tests the **delegate in isolation**, never through `RunAsync` — that
  distinction is essential.
- Do **not** introduce any seam that replaces or wraps `_stopWatch`;
  `RunAsync_ExecutesCorrectly:303` asserts `StopWatch.IsRunning`. This is the direct reason M1 is a
  pure extraction rather than a clock seam.
- Do **not** test the provably dead null arm of `progress?.Report(100)` (`QfcHomeController.cs:312`);
  line 277 already dereferences `progress` unconditionally.

### One proposal, not a decision

`GetMoveDiagnostics_NullAppointment_DoesNotThrow` in `QfcHomeControllerMetricsTests.cs` (lines
161-241) is **vacuous**: it arranges an empty `SpecialFolders`, so execution returns at
`QfcHomeController.Metrics.cs:38` and never reaches `GetCalendar`, `GetMoveDiagnostics`, or any
appointment. Its name, summary, and inline comments all describe behavior it does not exercise. The
metrics research **recommends retargeting** it (seed `MyDocuments`, assert `GetMoveDiagnostics` is
called once with a null `ref` appointment) rather than deleting it or leaving it alongside the new
test A2, which asserts the abort path properly.

`QfcHomeControllerMetricsTests.cs` is **not** on the frozen list, so the retarget is permissible. It is
nonetheless a **proposal requiring explicit justification in the atomic plan**, not a settled
decision: it is a deliberate modification of an existing test, and the plan must record the
justification and the updated summary text as part of the task.

## Cross-Child Contract Notes

**Research reports zero required cross-child contract additions.** This is a positive finding, not an
absence of analysis. The specific claims that were verified:

1. **Every proposed seam is `internal` on the class, so no interface can carry it.** C# interfaces
   cannot declare `internal` members that a public implementer must satisfy, and no consumer reaches
   the seams through an interface reference — the tests hold the concrete `QfcHomeController`. The
   seven existing loader delegates plus the injectable `TimeProvider` prove the pattern works without
   an interface edit; none of them appears on `IQfcHomeController` or `IFilerHomeController`.
2. **`IQfcHomeController` has exactly one implementer** — `QfcHomeController` at
   `QfcHomeController.cs:22`, F7-owned. There is no second production implementer and no hand-written
   test double; all test usage is `Mock<IQfcHomeController>`. Every one of its eight members is
   implemented on the F7-owned partial family.
3. **`IFilerHomeController` has two implementers, and one is sibling-owned.**
   `EfcHomeController` at `EfcHomeController.cs:18` is **F8**-owned. Adding any member produces CS0535
   on `EfcHomeController.cs` unless F8 implements it simultaneously; since F7 and F8 execute
   concurrently and fan in at integration, a unilateral addition would either break F8's build after
   rebase or force F7 to edit an F8-owned file. **Three of the twelve live members are already
   satisfied by a throw on the F8 side**: `Loaded` (`EfcHomeController.cs:391`), `FilerQueue`
   (`:417`), and `QuickFileMetrics_WRITE` (`EfcHomeController.Metrics.cs:26-29`). Removal, rename, and
   signature change carry the same prohibition, as does uncommenting any of lines 29, 34 or 40 — each
   of which would break `EfcHomeController` at compile time for a documented CS0535/CS0738 reason.
4. **`IFilerHomeController` is the highest-fan-in file in this child's set.** Eight wave-1 children
   touch the contract (F2, F3, F6, F9, F10, F11 as consumers; F7 and F8 as implementers), plus
   `TaskMaster/Ribbon/RibbonController.cs` outside the epic entirely. An "obvious" one-line edit here
   has the largest blast radius of any file assigned to F7. The control is that no edit is needed.
5. **The partial split imposes no obligation on any sibling** (see § Mandatory Partial Split).
6. **`coverage.config` needs no change.** Verified: it excludes only third-party module paths
   (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, MSTest, Microsoft.Testing). QuickFiler is
   instrumented.
7. **Every sibling-owned member this child's tests touch already exists on an interface a Moq mock can
   satisfy**: `IQfcCollectionController.GetMoveDiagnostics` (F11),
   `IQfcFormController.Groups` / `ItemsPerIteration` / both `LoadItems` overloads (F6),
   `IQfcDatamodel.Complete` / both `DequeueNextItemGroupAsync` overloads / `DequeueNextItemGroup` (F5),
   `IQfcQueue.EnqueueAsync` / `CompleteAddingAsync` / `Dequeue` (F2).

### Advisory notes — no addition requested of any sibling

| # | Surface | Owner | Advisory |
| --- | --- | --- | --- |
| A-1 | `IQfcDatamodel.DequeueNextItemGroupAsync(int, int)` | F5 | Must not be removed or re-signatured: it is byte-pinned at `QfcHomeControllerIterationTests.cs:268` under #424 AC 12. |
| A-2 | `IQfcQueue.Dequeue()` tuple return `(TableLayoutPanel, List<QfcItemGroup>)` | F2 | Must not become non-nullable or eagerly constructed — that shape is what lets `Iterate2` be tested without constructing a WinForms control (a loose mock yields `(null, null)`). |
| A-3 | The four early-return null guards in `QfcFormController.SetupDisposal.cs` (24-30, 50-57, 77-80, 151-154) | F6 | Requested only that they not be removed; buffer test TC18 relies on them. If F6 changes them mid-wave, drop TC18 rather than editing F6. |
| A-4 | Type names `IQfcExplorerController`, `IFilerFormController` (F6), `IQfcKeyboardHandler` (F3), `FilerQueue` (F2), `IQfcDatamodel` (F5) | F2/F3/F5/F6 | Named in `IFilerHomeController` member signatures. F7 asks only that the names remain stable through wave 1. A rename by any of these children breaks this file even though F7 never edits it. This is an integration-merge watch item; the epic orchestrator's pre-wave rebase is the control. |
| A-5 | `QfcDatamodel.LoadAsync` | F5 | Source line 173 (the `QfcAsyncDataModelLoader` default body, 1 line) can only be covered if `LoadAsync` becomes reachable without live Outlook COM. **No addition requested** — this child accepts the 1 line as residual and does not depend on F5. |
| A-6 | `QfcFormController.Iterate` delegate field (`QfcFormController.cs:48`, `:85`, nulled at `SetupDisposal.cs:225`) | F6 | If F6 removes the never-invoked delegate field, `QfcHomeController.Iterate()` loses its last reference and its removal becomes a clean follow-up under issue #447. F7 must not pre-empt that. |

### Sibling-Owned Files — must not be edited

`IQfcDatamodel` / `QfcDatamodel*` (F5); `IQfcQueue` / `QfcQueue*` / `FilerQueue` (F2);
`QfcCollectionController*` (F11); `QfcFormController*` / `QfcExplorerController` (F6);
`KeyboardHandler` / `Kbd*` / `Ka*` (F3); `EfcHomeController*` (F8); `coverage.config` and any shared
build property file.

### File-name collision hazard

Two files named `IQfcHomeController.cs` exist in the working tree. Only
`QuickFiler/Controllers/IQfcHomeController.cs` is compiled (`QuickFiler.csproj:304`, verified). The
second, `QuickFiler/Interfaces/IQfcHomeController.cs`, declares an unrelated
`QuickFiler.Interfaces.IQfcHomeController` with different members and survives only in the stale
`QuickFiler.csproj.bak:244`; it is outside the 121-file denominator and outside this child's file set.
**Every plan task that names this file must use the full path.** An agent that greps by file name and
edits the wrong one would either modify a dead file and produce a silently-green toolchain with no
effect, or edit the live one believing it dead.

## Decision: Dead Iteration Methods Are Covered, Not Removed

`Iterate()` and `Iterate2()` (`QfcHomeController.Iteration.cs:55-68` and `:70-77`, 23 of the file's 86
lines) are **dead production code**, verified by repository-wide search:

- `Iterate` is bound into `QfcFormController`'s private `IterateDelegate Iterate` field
  (`QfcFormController.cs:48`, declared `:85`), which is **never invoked** anywhere; its only other
  appearance is `Iterate = null` at `SetupDisposal.cs:225`.
- `Iterate2` appears at exactly four places repository-wide: its declaration
  (`Controllers/IQfcHomeController.cs:14`), its definition, and two lines of one test. **No production
  call site exists.**

They are **covered by this child, not removed.** Rationale:

1. Removing production code inside a coverage child breaches the epic NFR of no behavior change.
2. Removal is a breaking change to the public `IQfcHomeController` contract and would require editing
   sibling **F6**-owned files (`QfcFormController.cs`, `QfcFormController.SetupDisposal.cs`), which
   the epic's disjoint-file-set decomposition forbids.
3. They are compiled production code and therefore in the coverage denominator today.

Removal is tracked separately as **GitHub issue #447**, sequenced after F6 removes the never-invoked
delegate field.

## Out-of-Scope Defects — Do Not Fix

The following latent production defects were found during research. Each is **promoted to its own
GitHub issue and is out of scope for this child.** The child must not fix them. This is stated
explicitly so that a reviewer does not read a characterization test as endorsing a defect.

| Issue | Defect | Evidence |
| --- | --- | --- |
| **#442** | **Metrics are never flushed to disk.** `_metricsConsumers` (`QfcHomeController.cs:356`) is initialized to `0` and only ever *decremented* (`:366`, `Metrics.cs:228`) — there is no increment anywhere in the repository, so `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` (`Metrics.cs:226`) can never be true and `TimedConsumerAsync` is never subscribed. Even if it were, `Metrics.cs:229-230` constructs a `System.Timers.Timer(2000)` into a local, subscribes the handler, and never calls `Start()`. `_metrics` accumulates lines that are never written. `_fileName` (`:358`) is assigned at `Metrics.cs:153` and never read. Additionally, `_metrics.GetConsumingEnumerable().ToArray()` (`:367`) would block indefinitely if the handler *were* invoked, because `CompleteAdding()` is never called on `_metrics`. | `QfcHomeController.cs.research` § 9 R3; `Metrics.cs.research` § 9 D3-D5 |
| **#443** | **Metrics duration is misread.** `WriteMetricsAsync` reads `StopWatch.Elapsed` (`_stopWatch`, `Metrics.cs:121`) while the sibling `QuickFileMetrics_WRITE` reads `_stopWatchMoved` (`:42`); production calls `SwapStopWatch()` *before* the write, so `_stopWatch` is the freshly restarted instance and the recorded duration is ≈0 s while the true value sits unread. Both sites use `TimeSpan.Seconds` (the 0-59 component) rather than `TotalSeconds`, so a 90-second session records 30, and line 44 compounds it by deriving `startTime` from the full `Elapsed`. All CSV formatting (`"MM/dd/yyyy"`, `"hh:mm"`, `"##0"`, `"##0.00"`) is culture-sensitive, so a non-invariant culture emits a comma decimal separator into a comma-delimited file. | `Metrics.cs.research` § 9 D1, D2, D8, D9 |
| **#446** | **`IterateQueueAsync`'s empty-batch inference closes the UI queue irreversibly.** Line 32 treats `listObjects.Count == 0` as end-of-source and calls `CompleteAddingAsync`, reaching `_queue.CompleteAdding()` — an irreversible close. Post-#424 the two-argument `DequeueNextItemGroupAsync` delegates to the four-argument overload with `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` (12 s), so the post-UI background refill **silently inherited a 12-second deadline**. A slow high-confidence scan that accepts nothing within 12 s now returns empty while unscanned items remain, and the UI queue is closed for the rest of the session. Before #424 the overload was unbounded and the inference was sound. | `Iteration.cs.research` § 8.3, § 9 LD3 |
| **#447** | **Dead `Iterate` / `Iterate2` removal.** See § Decision above. | `Iteration.cs.research` § 9 LD1 |

### Characterization-test labelling requirement

Where a test in this child pins current defective behavior, it **must** be labelled a
**CHARACTERIZATION** test in its own summary comment, naming the tracking issue, and must assert what
the code does today without asserting that it is correct. Known instances:

| Test | Pins | Label must name |
| --- | --- | --- |
| Metrics A8 (`NonBlockingProducer` with `_metricsConsumers == 2`, documenting the 2 → 0 → −1 transition; must not assert the timer fires) | #442 | #442 |
| `QfcHomeController.cs` TC7-TC11 (`TimedConsumerAsync`, unreachable in production) | #442 | #442 |
| Metrics D3 (`BuildDurationTexts` with 90 s elapsed → 30) | #443 | #443 |
| `Iteration.cs` A-group tests of the empty-batch routing — no new test may assert that the routing is *correct*; the existing `QfcHomeControllerIterationTests.cs:124` already characterizes it | #446 | #446 |
| `Iteration.cs` D3 (fire-and-forget fault discarded and never surfaced) | Unobserved-fault discard (`Iteration.cs.research` § 9 LD2 — **no issue number assigned**) | Cite the research artifact section and flag the finding to the epic orchestrator for promotion |

Additional report-only findings recorded in the research artifacts and **not** promoted at authoring
time — dead locals and a dead `out` parameter, the write-only `_fileName`, the near-duplicate
appointment-creation logic, the conditional missing null guard on the dequeue result, the swallowed
cancellation with no log line, and the interface-segregation narrowing of `IFilerHomeController` — are
likewise out of scope. This child must not fix any of them. Their promotion is the epic orchestrator's
decision.

## Inputs / Outputs

- **Inputs:** F1's ledger rows for the five in-scope files; F1's per-file coverage harness (derived
  from `Invoke-MSTestWithCoverage.ps1`); the merge-base baseline run.
- **Outputs:** per-file and repository coverage figures under `<FEATURE>/evidence/baseline/` and
  `<FEATURE>/evidence/qa-gates/`; frozen-file SHA-256 manifests; measured file-line-count table;
  toolchain command transcripts with `EXIT_CODE`.
- **Evidence location:** canonical `<FEATURE>/evidence/<kind>/` per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Timestamps use `yyyy-MM-ddTHH-mm`.
  Writing evidence to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other
  non-canonical path is a policy violation.
- **Config keys and defaults:** none added.
- **Versioning / backward compatibility:** no public API change. Every new member is `internal`. Both
  interface files are byte-unmodified.

## API / CLI Surface

No CLI surface and no public API change. The new members are `internal` on `QfcHomeController` and are
reachable from `QuickFiler.Test` via the existing `InternalsVisibleTo` attribute.

Toolchain commands, in the order the policy requires:

```
csharpier .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
```

## Data & State

No data-flow, storage, persistence, caching, migration, or backfill change. The seams redirect
existing writes to injectable delegates whose production defaults are the exact expressions they
replace, so the session CSV and the "Email Time" calendar appointment behave identically. The partial
split moves source text and alters no declaration.

## Constraints & Risks

| # | Risk | Mitigation |
| --- | --- | --- |
| R1 | **F1 has not landed.** The ledger, the shared seam convention, the exemption disposition for `LaunchAsync`'s residual lines, and the harness that produces acceptance evidence are all upstream. | Gate the plan's Phase 0 on reading the ledger. Halt conditions are enumerated in § Upstream Contract. |
| R2 | **The 500-line limit is breached by the minimum recommendation** (487 + 15 = 502). | The partial split is mandatory, not optional. Encode it as a task that precedes the seam tasks. |
| R3 | **Accidental edit of the frozen `QfcHomeControllerIterationTests.cs`** — the single largest process risk in this child. | Name the new test file explicitly in every test task; record SHA-256 in Phase 0 and re-verify in the final QA gate. |
| R4 | **`Iteration.cs` may already exceed the 80% line floor**, making a "reach 80%" criterion vacuous and the work look optional. | Take F1's numeric baseline before fixing the AC, and word the AC around the named uncovered branches (see AC7). |
| R5 | **Behavior-preservation discipline.** The defects in § Out-of-Scope Defects are exactly the kind an implementer fixes while writing an adjacent test. | Explicit no-fix criterion (AC19) plus the characterization-labelling requirement. |
| R6 | **`QuickFiler.csproj` `<Compile>` merge conflict** with F9, F11, F13 in the same wave. | Single-line diff adjacent to lines 325-327; no other csproj change. |
| R7 | **S5a/S5b coupling.** Source lines 133 and 136 are invisibly coupled — `TaskScheduler.FromCurrentSynchronizationContext()` succeeds only because the preceding line constructs a live `QfcFormViewer` that installs a `WindowsFormsSynchronizationContext`. A viewer seam without a scheduler seam turns `InitAsync_InitializesCorrectly` red. | Tier C is conditional; if adopted, S5a and S5b ship as one task. |
| R8 | **Pre-existing policy debt: the existing `Init*` tests construct a live WinForms form** (`QfcHomeController.cs:93` and `:133`), contradicting `epic.md` § Shared Design 2. S5a/S5b would resolve it but only under Tier C. | If the plan stops short of Tier C, record this as known, unresolved policy debt rather than leaving it silent. Feature review may flag it either way. |
| R9 | **Reflection-heavy tests** couple to member names and break at run time, not compile time, on a rename. | Accepted as consistent with all eight existing suites. Prefer the M4 visibility widening over reflection wherever it is available. |
| R10 | **Framework-behavior assumptions.** Three arrangements rest on unverified framework or Moq behavior: `BlockingCollection<T>.TryAdd(item, 20, ct)` throwing for an already-cancelled token; the interop `Items.Add` optional-`object Type` parameter's Moq setup shape; and Moq's inability to match a specific instance for a `ref` parameter. | Confirm each at implementation time. Documented fallbacks: route the cancelled case through the M3 `MetricsAdder` seam; use the existing `Mock<Items>` usages in `TaskMaster.Test` / `UtilitiesCS.Test` as the reference; capture the `ref` argument in a `Callback` rather than an argument matcher. |
| R11 | **Indirect exposure through F6.** F6 is refactoring `QfcFormController`, the heaviest consumer of `IQfcHomeController`. Behavior this child pins could be affected at integration-merge time even though neither child edits the interface. | Merge-time watch item, not a planning blocker; the epic orchestrator rebases before each wave. |
| R12 | **Repository coverage-floor divergence.** `CLAUDE.md` § UT2 states `>= 80%` repository-wide with `>= 90%` for new modules; `.claude/rules/general-unit-test.md` states `>= 85%` line and `>= 75%` branch; `epic.md` measures `>= 80%` **per file**. The #424 evidence recorded a merge-base repository line rate of 70.19%, so an absolute repository-wide floor is not satisfiable by this child. | This child's binding obligation is the epic's per-file target plus retain-or-improve on the repository figure (AC22). It does not re-scope any threshold. The 90% new-module expectation is applied to the new partial file (AC8) and is an open question to F1 for the new *members* added to existing files. |

## Implementation Strategy

- **Scope of change:** two production files gain `internal` seams (`QfcHomeController.cs`,
  `QfcHomeController.Metrics.cs`); one new production partial file is created
  (`QfcHomeController.Properties.cs`); one csproj line is added; one production file
  (`QfcHomeController.Iteration.cs`) and both interface files are untouched; six or more new test
  files are added.
- **New members:** `ShowUserMessage`, `MetricsFileWriter` (on `QfcHomeController.cs`);
  `BuildDurationTexts`, `MetricsLineWriter`, `MetricsAdder` (on `QfcHomeController.Metrics.cs`);
  visibility widening on both `NonBlockingProducer` overloads.
- **Dependency changes:** none. `Microsoft.Extensions.TimeProvider.Testing` (`FakeTimeProvider`) is
  already in use by `QfcHomeControllerMetricsTests.cs`.
- **Logging/telemetry:** no addition. Restoring the two commented-out `logger.Debug` lines in the
  `IterateQueueAsync` catch handlers is a production change and belongs to a separate issue.
- **Sequencing constraint:** the partial split must precede the seam additions so the 500-line limit
  is never breached in an intermediate state. The Phase-0 ledger read and baseline capture must
  precede everything.
- **Rollout:** no feature flag, no staged deploy, no fallback path. The change is inert at run time.

## Acceptance Criteria

Upstream contract and baseline:

- [ ] **AC1** — F1's ledger row for each of the five in-scope files is read from
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` and recorded verbatim in
      `<FEATURE>/evidence/qa-gates/`. `QfcHomeController.cs`, `QfcHomeController.Metrics.cs`, and
      `QfcHomeController.Iteration.cs` are recorded as `testable`; `IQfcHomeController.cs` and
      `IFilerHomeController.cs` are recorded as `interface-only / not-measured`. Any other
      classification halts work and requires this spec to be revised before further edits. The ledger
      file is not modified by this child.
- [ ] **AC2** — A merge-base baseline run of F1's per-file harness is recorded under
      `<FEATURE>/evidence/baseline/` with the exact command, `EXIT_CODE`, an output summary, and the
      line rate and branch rate for each `testable` file plus the repository-wide line rate. AC3-AC7
      and AC22 are evaluated against this baseline.

Per-file coverage, measured by F1's harness and recorded under `<FEATURE>/evidence/qa-gates/`:

- [ ] **AC3** — `QuickFiler/Controllers/QfcHomeController.cs`: post-change per-file line coverage is
      `>= 80%` **and** `>= ` its AC2 baseline; branch rate is recorded and is `>= ` its AC2 baseline.
      Numeric figures cited, not asserted.
- [ ] **AC4** — `QuickFiler/Controllers/QfcHomeController.Metrics.cs`: post-change per-file line
      coverage is `>= 80%` **and** `>= ` its AC2 baseline; branch rate recorded and `>= ` baseline.
- [ ] **AC5** — `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: post-change per-file line
      coverage is `>= 80%` **and** `>= ` its AC2 baseline; branch rate recorded and `>= ` baseline.
- [ ] **AC6** — `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: the following previously
      unexecuted paths are executed by a new test and shown covered in the final harness output — the
      throwing arm of the entry `Token.ThrowIfCancellationRequested()` (source line 13), the
      `catch (OperationCanceledException)` handler (38-41), the `Token.IsCancellationRequested == true`
      swallow (42-47), the `throw;` rethrow (48-51), and both null-conditional short-circuits of
      `Globals?.QfSettings?.HighConfidenceModeEnabled` in `Iterate()` (line 60).
- [ ] **AC7** — `QuickFiler/Controllers/QfcHomeController.Properties.cs` (new): per-file line coverage
      is `>= 90%`, satisfying the new-module rule in `CLAUDE.md` § UT2 for a newly created file.
- [ ] **AC8** — For `QuickFiler/Controllers/IQfcHomeController.cs` and
      `QuickFiler/Interfaces/IFilerHomeController.cs`, the final harness output demonstrating zero
      measurable lines (or the ledger's chosen zero-line row format) is recorded as evidence. No test
      is authored against either file, and **no interface shape-assertion or reflection-shape test
      appears anywhere in this child's diff.**

File size and structure:

- [ ] **AC9** — Every production file in scope, including the new
      `QfcHomeController.Properties.cs`, is at or under **500 lines**, with a measured per-file line
      count recorded as evidence.
- [ ] **AC10** — Every new or modified test file is at or under **500 lines**, with measured counts
      recorded.
- [ ] **AC11** — The `#region Public Properties` block (source lines 406-485 of
      `QfcHomeController.cs` at base `74be1964`) is relocated into
      `QuickFiler/Controllers/QfcHomeController.Properties.cs` with no declaration changed;
      `[assembly: InternalsVisibleTo("QuickFiler.Test")]` remains in `QfcHomeController.cs`; and the
      `QuickFiler/QuickFiler.csproj` diff consists of exactly one added
      `<Compile Include="Controllers\QfcHomeController.Properties.cs" />` line adjacent to lines
      325-327, with no other csproj change of any kind.

Seams and interfaces:

- [ ] **AC12** — The production seam set is limited to the ratified required set (S1, S2, M1, M2, M3,
      M4) plus any conditional item (S3, S4/S5a/S5b, D7) whose adoption is explicitly justified in the
      approved atomic plan. Every seam is an `internal` member on `QfcHomeController`. No seam is added
      to, and no member is added to, removed from, renamed in, or re-signatured on,
      `IQfcHomeController.cs` or `IFilerHomeController.cs`; the diff for both files is **empty**.
- [ ] **AC13** — Each new delegate seam has a production default that reproduces the expression it
      replaces exactly, and a test pins that the default is non-null without invoking a prohibited
      side effect: `ShowUserMessage`, `MetricsFileWriter`, `MetricsLineWriter`, `MetricsAdder`, and
      `TimeProvider` (which must be shown to default to `TimeProvider.System`).
- [ ] **AC14** — `BuildDurationTexts` reproduces current semantics exactly — `elapsed.Seconds` (not
      `TotalSeconds`), the `emailsLoaded > 0` guard, the `"##0"` and `"##0.00"` formats, and a default
      format provider of `CultureInfo.CurrentCulture` — and both former inline call sites delegate to
      it with no change in emitted values.

Frozen and existing tests:

- [ ] **AC15** — All five frozen test files are byte-identical to base commit `74be1964`, proven by a
      SHA-256 manifest recorded before the first edit and re-verified in the final QA gate:
      `QfcHomeControllerIterationTests.cs`, `QfcInitEmailQueueZeroBatchTests.cs`,
      `QfcHighConfidencePreFilterTests.cs`, `QfcFormControllerTests.cs`,
      `QfcHomeControllerIssue218Tests.cs`.
- [ ] **AC16** — No existing test file is modified other than changes enumerated **and justified** in
      the approved atomic plan. If the `GetMoveDiagnostics_NullAppointment_DoesNotThrow` retarget is
      taken, the plan records its justification and the test's summary is updated to describe the
      behavior it then actually exercises.

Scope containment and behavior preservation:

- [ ] **AC17** — `git diff --name-only` against the merge base is recorded as evidence and contains
      only: the two seam-bearing production files, the new production partial, the one csproj line's
      file, new test files, this feature folder's documents and evidence, and any existing test file
      permitted by AC16. **No sibling-owned file appears** — specifically none of
      `IQfcDatamodel`/`QfcDatamodel*`, `IQfcQueue`/`QfcQueue*`/`FilerQueue`,
      `QfcCollectionController*`, `QfcFormController*`/`QfcExplorerController`,
      `KeyboardHandler`/`Kbd*`/`Ka*`, `EfcHomeController*`, `coverage.config`, or any shared build
      property file.
- [ ] **AC18** — No behavior change to observable QuickFiler flows: every test that passed in the AC2
      baseline run still passes in the final run, with no assertion weakened, no expectation relaxed,
      no `[Ignore]` added, and no test deleted. The pass/fail delta is recorded.
- [ ] **AC19** — No fix for #442, #443, #446, or #447 appears in the diff. `Iterate()` and
      `Iterate2()` remain present in `QfcHomeController.Iteration.cs` and on
      `IQfcHomeController`, and are covered rather than removed. No unpromoted report-only research
      finding is fixed either.
- [ ] **AC20** — Every test that pins current defective behavior carries `CHARACTERIZATION` in its own
      summary comment, names its tracking issue (#442, #443, #446) or, where no issue exists, cites the
      research artifact section, and asserts current behavior without asserting that it is correct. No
      new test asserts that the empty-batch → `CompleteAddingAsync` routing is correct.

Test quality:

- [ ] **AC21** — No new or modified test contains `Thread.Sleep`, `Task.Delay`, a real wall-clock wait
      or poll loop, `DateTime.Now`/`DateTime.UtcNow`/`DateTimeOffset.Now`, unseeded randomness, a
      temporary or real file write, an external service or process, a live Outlook COM object, a live
      WinForms `Form` or `Show()`, a `MessageBox`, a `UiThread.Init` call, or a
      `SynchronizationContext.SetSynchronizationContext` in Arrange. Verified by a recorded search over
      the child's new and modified test files. `[Timeout]` appears only as a hang guard with in-test
      justification.
- [ ] **AC22** — Every new test uses MSTest attributes, Moq, and FluentAssertions; follows
      Arrange–Act–Assert; and carries a summary comment stating its scenario and expected outcome. New
      tests live only in new files under `QuickFiler.Test/Controllers/`, mirroring the production tree.
- [ ] **AC23** — For each of the three `testable` files, the delivered tests cover positive flows with
      valid inputs, negative flows for invalid or missing inputs, boundary conditions, and
      error-handling behavior, plus state transitions where the member is stateful. A per-file scenario
      mapping is recorded as evidence.
- [ ] **AC24** — No test duplicates an assertion listed in a research artifact's duplication guard, and
      no test listed as a rejected candidate is present. Where a new test targets the same member as an
      existing test, the plan task text states which distinct post-condition it asserts.

Toolchain and aggregate coverage:

- [ ] **AC25** — The full C# toolchain passes in a single final pass, in order — `csharpier .`; the
      analyzer msbuild (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`); the nullable
      msbuild (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`); and
      `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — with the exact commands and
      `EXIT_CODE` for each stage recorded under `<FEATURE>/evidence/qa-gates/`. No analyzer diagnostic
      is suppressed to achieve this.
- [ ] **AC26** — The repository-wide line coverage figure from the same final run is recorded and is
      `>= ` the AC2 baseline figure, satisfying the epic's retain-or-improve leading indicator. This is
      a no-regression criterion against the measured baseline, not an absolute floor.
- [ ] **AC27** — All evidence artifacts are written under `<FEATURE>/evidence/<kind>/` per
      `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, with `yyyy-MM-ddTHH-mm` timestamps.
      No evidence is written to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any
      other non-canonical path.

## Definition of Done

- [ ] Acceptance criteria above documented and each mapped to a test, a measurement, or a recorded
      evidence artifact
- [ ] Behavior matches acceptance criteria in the documented environment (.NET Framework 4.8.1,
      `QuickFiler.Test` via `vstest.console.exe`)
- [ ] Tests added in new files only, with the frozen-file manifest re-verified
- [ ] Edge cases and error handling covered per AC23
- [ ] `spec.md` and `user-story.md` acceptance criteria checked off per
      `.claude/skills/acceptance-criteria-tracking/SKILL.md`, with an AC status summary reported
- [ ] Any deviation from the ratified seam set, or adoption of a conditional item, recorded in this
      spec with its justification
- [ ] Toolchain pass completed in order (format → analyze → type-check → test with coverage)

## Seeded Test Conditions (from potential)

- [ ] Unit coverage areas: `Worker_RunWorkerCompleted` cancelled and error arms; `TimedConsumerAsync`
      drain, empty, unresolvable-folder, and throwing-writer paths; `Run`/`RunAsync` null-`Globals` and
      null-`QfSettings` mode guards; the private parameterless constructor and its seven lambda-cache
      conditions; `CreateCancellationToken`; the default `QfcExplorerControllerLoader` and
      `HighConfidencePreFilterLoader` bodies; `QuickFileMetrics_WRITE` and `WriteMetricsAsync`
      calendar-found branches and `MyDocuments`-absent guards; both `NonBlockingProducer` overloads;
      `BuildDurationTexts` across zero, positive, negative, boundary, and over-one-minute inputs;
      `IterateQueueAsync` entry-guard, cancellation, swallow, and rethrow paths; `Iterate` and
      `Iterate2` stopwatch rotation; `SwapStopWatch` full post-condition set; `Iterate2`
      fire-and-forget ordering and fault discard.
- [ ] Integration scenarios: none. Every path in scope is reachable by unit test with interface mocks,
      injectable delegates, or reflection.
- [ ] CLI/API examples: not applicable — no CLI surface and no public API change.
