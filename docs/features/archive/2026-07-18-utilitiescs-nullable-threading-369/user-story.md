# `utilitiescs-nullable-threading` — User Story

- Issue: #369
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T22-45
- Epic: utilitiescs-nullable-remediation (child, Wave 0)

## Story Statement

- As the repository maintainer, I want the `UtilitiesCS/Threading/` nullable-reference-type debt
  remediated under a per-file `#nullable enable` opt-in, so that the repaired CI nullable gate can
  be enforced on these concurrency/ordering files without cross-blocking non-opted-in files
  elsewhere in the epic.
- As a downstream consumer of the Threading contracts (`UiThread`/`IUiDispatcher`, the progress
  trackers, `LockupAttribution`, `TimeOutTask`, `AsyncMultiTasker`), I want those public members
  annotated to reflect their actual null behavior, so that I consume accurate nullability contracts
  and do not inherit incorrect null assumptions when I remediate my own cluster.

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/Threading/` directory (approximately 25 hand-written `.cs` files plus 4 WinForms
Designer files and 4 `.resx` resources) carries such pre-existing nullable debt. This is the
concurrency/ordering module: it hosts async multi-tasking, idle-time action queues, UI-thread
dispatch, thread monitors, single-shot guards, timeout tasks, and the store-lockup watchdog. Its
public members are consumed across module boundaries, so their nullability annotations become
contracts.

Because .NET Framework 4.8.1 reference assemblies ship no nullable metadata, the BCL surface is
oblivious and the real debt is dominated by compiler-internal diagnostics: CS8618 (uninitialized
non-null fields), CS8625 (`= null` defaults), CS8603/CS8600 (`default(T)`/`as`), plus self-induced
CS8602. A global force-enable of nullable would make no epic child independently mergeable until all
~234 files (~2131 diagnostics) were fixed at once. The per-file opt-in lets this child be remediated
and merged on its own while non-opted-in files stay oblivious and non-cross-blocking.

## Personas & Scenarios

- Persona: Repository maintainer (drmoisan)
  - Who: owner of the nullable-remediation epic and the CI nullable gate.
  - Cares about: a genuinely enforceable nullable gate that does not permanently block future PRs;
    a per-file opt-in architecture that keeps each epic child independently mergeable; concurrency
    semantics that remain exactly as they are today.
  - Constraints: annotation and null-safety only — no behavior changes, no refactors, no API
    redesign; no change to locking, ordering, scheduling, single-shot-guard, `SynchronizationContext`,
    or store-lockup-watchdog concurrency semantics; no project- or solution-level `<Nullable>`
    element; no editing of `.claude/rules/*`.
  - Goals and frustrations: wants the Threading debt cleared under the confirmed architecture, and
    wants scope conflicts (Designer files, `TimeOutTask.cs` line limit, rules-vs-convention) surfaced
    as flags rather than silently resolved.
  - Context: Threading is a Wave-0 concurrency/ordering cluster; its annotations gate the quality of
    the cross-module contracts (~50 consumers of `UiThread`/`IUiDispatcher` alone) that depend on it.

- Persona: Downstream contract consumer
  - Who: an agent or developer in `TaskMaster`, `QuickFiler`, `EmailIntelligence`, or elsewhere in
    `UtilitiesCS` that consumes the Threading public surface, or a later epic child that opts its own
    cluster in.
  - Cares about: consuming Threading public members with nullability annotations that match the
    actual runtime behavior, so their own null-flow analysis is correct — including the
    `IAppAutoFileObjects.ProgressTracker` return, the `LockupAttribution.StoreIdentity` chain, and the
    `TimeOutTask` `Task<TResult>` return contract.
  - Constraints: must not have to re-derive or work around inaccurate Threading contracts; must not
    receive a silently widened `TimeOutTask` return type (`Task<TResult?>`) or a perturbed watchdog
    null-branch.
  - Goals and frustrations: an incorrect annotation on a shared concurrency member (for example a
    member marked nullable that in fact throws, or vice versa) would propagate an incorrect assumption
    into every dependent cluster when it is later opted in.

- Scenario: Remediating and verifying a Threading batch
  - Who is acting: the executor delivering issue #369, batch by batch, in the research's
    foundational -> concurrency-core -> watchdog -> highest-contract/`TimeOutTask`-last order.
  - Trigger: the repaired nullable gate now surfaces pre-existing CS86xx in Threading.
  - Steps: opt each batch's files in with `#nullable enable`; apply annotation/null-safety edits
    (nullable `?`, guards, justified `!`, null-flow corrections); build with the pragma-only command
    (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`) to capture a per-batch
    baseline and drive the opted-in files to zero CS86xx; run the batch's MSTest tests and require
    them green and behavior-identical.
  - Obstacles/decisions: annotate `LockupAttribution.StoreIdentity` to `string?` before the watchdog
    batch consumes it; at `StoreLockupResponder`, annotate around the documented null-store-model
    guards and never alter a null-branch; prefer `AsyncMultiTasker` `timer!` over `timer?.` to
    preserve current behavior; keep `TimeOutTask` returns as `Task<TResult>` with `!` rather than
    widening; leave `*.Designer.cs` and `.resx` non-opted-in and flag where a hand-written form partial
    needs `?`/`!` on its own fields; flag the `TimeOutTask.cs` 500-line pre-existing violation without
    splitting it; do not add `/p:Nullable=enable` to the verification command; do not introduce
    `System.Diagnostics.CodeAnalysis` post-condition attributes (unavailable on net481).
  - Expected outcome: every in-scope Threading file that emitted CS86xx is opted-in and clean under
    the pragma-only gate, with no behavior change, no concurrency-semantics change, and no coverage
    regression on changed lines, and all flagged conflicts documented for the maintainer.

## Acceptance Criteria

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

## Non-Goals

- No behavior changes, refactors, API redesign, or feature work of any kind. Nullable annotation and
  null-safety only.
- No change to locking, ordering, scheduling, the single-shot guard, `SynchronizationContext`
  handling, or the store-lockup-watchdog concurrency semantics.
- No project-level or solution-level `<Nullable>` element as an enforcement mechanism.
- No use of `/p:Nullable=enable` in the verification command, and no editing of `.claude/rules/*` to
  resolve the rules-vs-convention conflict (it is flagged at the epic level, Wave-2 capstone child).
- No introduction of `System.Diagnostics.CodeAnalysis` post-condition attributes (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`) — unavailable/unpolyfilled on net481.
- No widening of `TimeOutTask` returns to `Task<TResult?>` (a silent downstream contract change to
  be escalated to the maintainer, not made here).
- No splitting of `TimeOutTask.cs` to meet the 500-line limit (pre-existing condition, flagged not
  fixed).
- No hand-editing of `*.Designer.cs` or `.resx` files, and no changes to files outside
  `UtilitiesCS/Threading/`.
