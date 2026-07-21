# utilitiescs-nullable-threading — Spec

- **Issue:** #369
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (child, Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-45
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/Threading/` directory (approximately 25 hand-written `.cs` files, plus 4 WinForms
Designer files and 4 `.resx` resources) carries such pre-existing nullable debt. This is the
concurrency/ordering module: it hosts async multi-tasking, idle-time action queues, UI-thread
dispatch, thread monitors, single-shot guards, timeout tasks, and the store-lockup watchdog. Its
public members are consumed across module boundaries, so their nullability annotations become
contracts that downstream epic features consume.

This feature remediates that debt for the `Threading/` tree only, using a per-file
`#nullable enable` opt-in. It is annotation and null-safety work exclusively. It introduces no
behavior change and no refactor. In particular it makes no change to locking, ordering, scheduling,
the single-shot guard, `SynchronizationContext` handling, or the store-lockup watchdog concurrency
semantics.

## Behavior

Remediate the pre-existing nullable-reference-type debt across `UtilitiesCS/Threading/` using a
per-file `#nullable enable` opt-in. The following are maintainer-mandated hard constraints, not
options; no alternative architecture is to be proposed or adopted:

- Add a `#nullable enable` pragma to each remediated file and bring that file to zero CS86xx
  diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none. No project-level or solution-level `<Nullable>`
  element may be introduced by this feature.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators (`!`) only where justified, and null-flow corrections. No behavior changes, no
  refactors, no API redesign, no feature work. In particular, NO change to locking, ordering,
  scheduling, single-shot-guard, `SynchronizationContext`, or store-lockup-watchdog concurrency
  semantics.
- Keep public signatures behavior-compatible; annotate to reflect the actual runtime null behavior
  so the annotations serve as accurate downstream contracts.

Files that are not opted-in remain in an oblivious nullable context and are not cross-blocking.
This is the mechanism that lets each epic child merge independently without requiring the entire
epic (~2131 diagnostics across ~234 files) to be fixed first.

## Inputs / Outputs

- Inputs (files): the 25 hand-written `.cs` files under `UtilitiesCS/Threading/`. The 4 WinForms
  Designer files (`*.Designer.cs`) and the 4 `.resx` resources are OUT of scope for opt-in; they
  receive no pragma and remain oblivious (see Constraints & Risks item 3).
- Outputs (source changes): a `#nullable enable` pragma plus annotation/null-safety edits on each
  in-scope hand-written file that emits CS86xx; no new files, no removed files, no project-file
  edits.
- Config keys and defaults: none introduced. `UtilitiesCS.csproj` remains without a `<Nullable>`
  element.
- Versioning or backward-compatibility constraints: public member signatures must remain
  behavior-compatible. Nullability annotations added to public members become cross-module
  contracts consumed outside `Threading/`; they must reflect actual null behavior rather than
  change it.

## API / CLI Surface

This feature exposes no new commands or CLI. The "surface" is the set of nullability annotations
applied to public members of the Threading types. These annotations ARE the contract consumed
outside `Threading/`.

net481 BCL-oblivious reality (governs the debt profile): .NET Framework 4.8.1 reference assemblies
ship no nullable metadata, so BCL/framework surfaces (`System.*`,
`System.Windows.Threading.Dispatcher`, `System.Threading.Tasks.Task`,
`SynchronizationContext.Current`, `string.IsNullOrWhiteSpace`,
`MethodBase.GetCurrentMethod().DeclaringType`, `Microsoft.Office.Interop.Outlook`) are oblivious.
The real Threading debt is therefore dominated by compiler-internal diagnostics that do not depend
on BCL annotations: CS8618 (uninitialized non-null fields/auto-properties/events — the largest
category), CS8625 (`= null` defaults and `null`-literal assignments), CS8603/CS8600
(`default(T)` returns on unconstrained generics and `x as T` results), plus self-induced CS8602
(appearing only after a field is annotated `T?`, resolved with a justified `!` rather than a new
guard).

Top cross-module-contract members (annotate deliberately; annotate last within their batches;
preserve current runtime behavior):

- **`UiThread` / `IUiDispatcher` / `WpfUiDispatcher`** — highest fan-out; approximately 50 non-test
  consumers across `TaskMaster`, `QuickFiler`, and `UtilitiesCS`. The public
  `UiThread.UiSyncContext`/`Dispatcher` and the `IUiDispatcher` member signatures are load-bearing
  contracts.
- **Progress trackers** (`ProgressTracker`, `ProgressTrackerPane`, `ProgressPackage`,
  `IProgressViewer`, `ProgressViewer`) — high fan-out across `QuickFiler`, `TaskMaster`, and
  `UtilitiesCS`, including the formal cross-module contract `IAppAutoFileObjects.ProgressTracker`
  which returns `ProgressTrackerPane`. The shared `IProgress<(int Value, string JobName)>` tuple
  contract must remain consistent across `ProgressTracker`/`ProgressTrackerPane`.
- **`LockupAttribution.StoreIdentity` → `string?` chain** — a high concurrency contract. Identity
  is genuinely null when no per-store scope is open; the `string?` annotation on the ctor param and
  property matches the documented "null = no context" behavior and feeds `StoresWrapper`,
  `StoreWrapper`, `StoreLockupAttribution`, and `TaskMaster` consumers.
- **`TimeOutTask`** (`RunWithTimeout`/`TimeoutAfter`) — high fan-out across `UtilitiesCS` and
  `QuickFiler`. Return-type stability protects all consumers: keep the public return type
  `Task<TResult>` and use `result = default!` / `return result!`; do NOT widen to
  `Task<TResult?>` (a silent downstream contract change to be escalated to the maintainer, not made
  here).
- **`AsyncMultiTasker.AsyncMultiTaskChunker`** — consumed by `EmailIntelligence` classifier and
  data-miner groups.

Narrower / self-documenting nullability: `ThreadSafeFunctions` and `ThreadSafeSingleShotGuard` are
widely consumed but expose no nullable surface, so opting them in imposes no contract.

Contracts and validation rules: annotations must express the null behavior that already occurs at
runtime. Where a member currently dereferences a value that flow analysis cannot prove non-null but
an initialization-order or call-site invariant guarantees, the behavior-preserving annotation is a
justified `!` (with a short `// why` comment), not a nullable contract change or a new runtime
guard.

## Data & State

This feature introduces no data flow, storage, persistence, caching, migration, or backfill
changes. Edits are confined to compile-time nullability annotations and null-flow corrections in
source. Runtime data transformations and invariants are unchanged by design; the "no behavior
change" constraint means observable state transitions before and after remediation are identical.
This holds specifically for the concurrency state machinery: locks, `Interlocked` operations,
`volatile` fields, timer arm/re-arm ordering, single-shot guards, and `Dispatcher.Post`/`Send`
sequencing are untouched.

## Constraints & Risks

The following mechanics flags are carried verbatim in substance from the research findings and
govern execution:

1. **Pragma-only verification command (do NOT use `/p:Nullable=enable`).** Local and CI
   verification of the opted-in files must use the pragma-only build
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`, relying on each file's own `#nullable enable` pragma. It must
   NOT add `/p:Nullable=enable`, which would enable nullable project-wide and surface the whole
   epic's ~2131 CS86xx diagnostics across ~234 files as false failures unrelated to issue #369.
   This is a deliberate, documented deviation from the stock CLAUDE.md / `.claude/rules/csharp.md`
   type-check command for this child only; it must NOT be resolved by editing `.claude/rules/*`.

2. **net481 BCL-oblivious profile shapes the fixes.** The debt is dominated by CS8618, CS8625,
   CS8603/CS8600, and self-induced CS8602. Because net481's `string.IsNullOrWhiteSpace` is NOT
   annotated `[NotNullWhen(false)]`, it does not refine null-state; `StoreLockupResponder` therefore
   requires a justified `!` at a guaranteed-non-null call site rather than reliance on a
   post-condition attribute. `System.Diagnostics.CodeAnalysis` post-condition attributes
   (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`) are not available/polyfilled on net481
   and must NOT be introduced; zero CS86xx is reachable with plain `?`, `= null!`, and justified
   `!`.

3. **`StoreLockupResponder` null-store-model hazard (issue #260) — annotate around the guards, never
   alter them.** The store-lockup watchdog has a documented null-store-model hazard: the watchdog
   thread crashes on a null store model. `OnLockupDetected` reads `attribution.StoreIdentity` (now
   `string?`) and passes through the documented no-context / unresolved-sentinel /
   `<Stores-enumeration>` / already-disabled null branches (issues #260/#264/#292). All edits are
   *around* those guards: `string?` on the identity flowing in and `?` on the two optional ctor
   params. No edit may add, remove, reorder, or alter the content of any null-branch. If a residual
   diagnostic appears to require touching a branch, FLAG it rather than resolve it.

4. **`AsyncMultiTasker` `timer` escalation — prefer `timer!` over `timer?.`.** In the second
   `AsyncMultiTaskChunker` overload, the `ITimerWrapper timer` local starts null and is assigned
   inside `await Task.Run(...)`; the `catch`/`finally` dereference it. Use `timer!.StopTimer()` /
   `timer!.Dispose()` to preserve the current NRE-if-unassigned behavior; do NOT switch to
   `timer?.` (that would swallow the failure and change behavior). This behavior-preserving default
   is recorded for reviewer confirmation.

5. **WinForms Designer handling — leave `*.Designer.cs` non-opted-in and do not hand-edit.**
   `#nullable enable` is lexical/per-file, so a Designer partial that carries no pragma stays
   oblivious even though it is the same class as an opted-in hand-written partial. Fields declared
   in the oblivious Designer partial (`Bar`, `JobName`, `ButtonCancel`) are treated as oblivious
   when referenced from the opted-in partial → they emit no CS86xx and need no `?`. The 4 `.resx`
   resources are left untouched. Do NOT add `#nullable enable` to any `*.Designer.cs`. The
   hand-written form partials (`ProgressPane.cs`, `ProgressViewer.cs`, `SyncContextForm.cs`) annotate
   only their own hand-declared fields/auto-props (for example `_dispatcher`, `_tokenSource`,
   `_cancelSource`, `UiSyncContext`, `UiDispatcher`) — never Designer-declared controls. Where a
   hand-written form partial dereferences a field it declares (for example `_tokenSource.Cancel()`
   after annotating `_tokenSource` as `CancellationTokenSource?`), use a justified `!` at the
   invariant-guaranteed call site (the button is enabled only after `SetCancellationTokenSource`),
   preserving current behavior.

6. **`TimeOutTask.cs` (975 lines) exceeds the repo 500-line limit — PRE-EXISTING; FLAG, do NOT
   fix.** Annotation-only work cannot bring it under 500 without splitting its ~15 overloads into
   multiple files, which is a refactor and out of scope. Adding the pragma makes it 976 lines.
   Record the breach as a known pre-existing policy exception and defer any split to a separate
   issue. `ApplicationIdleTimer.cs` (481 lines) and `AsyncMultiTasker.cs` (465 lines) are
   near-limit; keep annotations in-place (prefer `?` / `= null!` / justified `!` over new
   multi-line guard blocks) and, if csharpier reflow plus annotations push either to 501+ lines,
   FLAG it as an annotation-induced breach to the maintainer rather than split the file.

7. **No-op / clean files receive a pragma only if they emit CS86xx.** Fully commented-out or
   value-type-only files (for example `TaskPriority.cs`, `AsyncIdleQueue1.cs`,
   `ThreadSafeSingleShotGuard.cs`, `ThreadSafeFunctions.cs`, `ProgressMultiStepViewer.cs`) are
   expected to emit no CS86xx; add a pragma to them for cluster consistency only, and do not force
   pragmas on files that emit none. Interface-only files (`IUiDispatcher.cs`, `IProgressViewer.cs`)
   emit no CS86xx but the pragma still fixes the nullability of the declared contract.

8. **Rules-vs-convention conflict (flagged at epic level, not resolved here).**
   `.claude/rules/csharp.md` documents forcing `/p:Nullable=enable` globally, which conflicts with
   the per-file opt-in convention. This is flagged at the epic level (Wave-2 capstone child); it is
   not resolved in this feature and no `.claude/rules/*` file is edited.

Additional constraints and risks:

- Follow the repo C# toolchain order (csharpier -> msbuild analyzers/codestyle -> msbuild
  type-check -> vstest with coverage). For this child the type-check stage uses the pragma-only form
  in item (1), not the stock `/p:Nullable=enable` form. Any test work uses MSTest + Moq +
  FluentAssertions.
- This is the concurrency/ordering module (`concurrency_or_ordering` complexity floor). Nullable
  annotation must not alter locking, ordering, single-shot-guard, `Interlocked`, `volatile`, timer
  arm/re-arm, or scheduling behavior. `CurrentStoreContext._current` moves from
  `volatile string` to `volatile string?` (matching the documented "null = no context" contract)
  with the `volatile` keyword and single-writer/single-reader discipline untouched.
- Annotations become cross-module contracts; incorrect annotations could propagate incorrect null
  assumptions to consumers only when those consumers are later opted in — hence the deliberate,
  last-within-batch treatment of the contract members.
- Prefer `?` / `= null!` / justified `!` over new `if (x is null) …` guards so that no new uncovered
  executable line is introduced and no coverage regression on changed lines occurs.

## Implementation Strategy

- Implementation scope: add `#nullable enable` to each in-scope `Threading/` file that emits CS86xx
  and apply annotation/null-safety edits to reach zero CS86xx per file under the pragma-only build.
  No new classes, functions, or commands; no dependency changes; no logging/telemetry additions; no
  project-file edits.
- Phasing: the research identifies an 8-batch sequence, foundational/low-risk first and
  concurrency-core / highest cross-module contract plus `TimeOutTask` last. Batches are cohesive and
  independently reviewable; each opts in its files and reaches zero CS86xx for those files under the
  pragma-only verification. The batches (scope, not fine-grained sequencing) are:
  1. No-op / confirm-clean: `TaskPriority.cs`, `AsyncIdleQueue1.cs`, `ThreadSafeSingleShotGuard.cs`,
     `ThreadSafeFunctions.cs`, `ProgressMultiStepViewer.cs`.
  2. Interfaces + dispatcher adapter (Contract): `IUiDispatcher.cs`, `WpfUiDispatcher.cs`,
     `IProgressViewer.cs`.
  3. Ambient/value concurrency types (Contract): `CurrentStoreContext.cs`, `LockupStallDecider.cs`
     (including `LockupAttribution`) — settle the `string?` identity chain before the watchdog batch
     consumes it.
  4. Idle scheduling + idle timer: `IdleActionQueue.cs`, `IdleAsyncQueue.cs`,
     `ApplicationIdleTimer.cs` (watch line count).
  5. WinForms hand-partials: `ProgressPane.cs`, `ProgressViewer.cs`, `SyncContextForm.cs` (own-field
     nullability only; Designer/resx left oblivious).
  6. Progress trackers (Contract): `ProgressPackage.cs`, `ProgressTracker.cs`,
     `ProgressTrackerAsync.cs`, `ProgressTrackerPane.cs`.
  7. Dispatch + watchdog core (CRITICAL): `UiThread.cs`, `ThreadMonitor.cs`,
     `StoreLockupResponder.cs` — depends on Batch 3; enforce the "annotate around guards, never alter
     a null-branch" rule; the store-lockup null-store-model hazard lives here.
  8. High-contract parallel + timeout (LAST): `AsyncMultiTasker.cs`, `TimeOutTask.cs` — highest
     consumer count; resolve the `timer!` decision, the `TimeOutTask` return-type-stability decision,
     and the `TimeOutTask` 500-line flag under focused review.
- Verification per batch: build with the pragma-only command to capture a per-batch CS86xx baseline,
  then drive the opted-in files to zero; run that batch's corresponding `UtilitiesCS.Test/Threading/`
  tests (adding `QuickFiler`/`TaskMaster` test assemblies when a batch touches a contract they
  consume) and require them green and behavior-identical.
- Rollout: no feature flags or staged deploys. Each batch is additive; non-opted-in files remain
  oblivious until remediated.

## Definition of Done

- [x] Every `.cs` file under `UtilitiesCS/Threading/` that emits CS86xx carries a
  `#nullable enable` pragma and compiles with zero nullable (CS86xx) diagnostics under the
  per-file pragma with `/p:TreatWarningsAsErrors=true`.
- [x] No project-level or solution-level `<Nullable>` element is introduced; `UtilitiesCS.csproj`
  retains none.
- [x] Changes are annotation/null-safety only: no behavior change, no API/signature semantics
  change, and no change to locking, ordering, scheduling, single-shot-guard, `SynchronizationContext`,
  or store-lockup-watchdog concurrency semantics.
- [x] All existing MSTest tests for UtilitiesCS still pass and are behavior-identical; no coverage
  regression on changed lines.
- [x] The full C# toolchain (csharpier -> analyzer/codestyle build -> type-check build -> vstest
  with coverage) passes on the final pass, using the pragma-only type-check command
  (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`) for this child.
- [x] `StoreLockupResponder` null-branch behavior is preserved exactly: the no-context,
  unresolved-sentinel, `<Stores-enumeration>`, and already-disabled branches are unchanged in order
  and content; the identity chain is annotated around them.
- [x] WinForms Designer files (`*.Designer.cs`) and the 4 `.resx` resources are not hand-edited and
  are left non-opted-in (oblivious); hand-written form partials annotate only their own declared
  fields.
- [x] The `TimeOutTask.cs` 500-line pre-existing violation is flagged (not fixed) in the feature
  docs; any annotation-induced breach of `ApplicationIdleTimer.cs` / `AsyncMultiTasker.cs` past 500
  lines is flagged rather than resolved by splitting.

## Seeded Test Conditions (from potential)

- [x] Existing MSTest suite for UtilitiesCS still passes post-annotation.
- [x] No coverage regression on changed lines.
- [x] Nullable gate passes for the opted-in files using the pragma-only build
  (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`).
