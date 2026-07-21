# utilitiescs-nullable-threading (Issue #369)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-nullable-threading/ (Issue #369)
- Epic: utilitiescs-nullable-remediation (child, Wave 0)

- Issue: #369
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/369
- Last Updated: 2026-07-19
- Work Mode: full-feature

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/Threading/` directory (approximately 25 hand-written `.cs` files plus 4 WinForms
Designer files) carries such pre-existing nullable debt. This is the concurrency/ordering module:
it hosts async multi-tasking, idle-time action queues, UI-thread dispatch, thread monitors,
single-shot guards, timeout tasks, and the store-lockup watchdog. Its public members are consumed
across module boundaries, so their nullability annotations become contracts.

## Proposed Behavior

Remediate the pre-existing nullable-reference-type debt across `UtilitiesCS/Threading/` using a
per-file `#nullable enable` opt-in:

- Add a `#nullable enable` pragma to each remediated file and bring that file to zero CS86xx
  diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators (`!`) only where justified, and null-flow corrections. No behavior changes, no
  refactors, no API redesign, no feature work. In particular, NO change to locking, ordering,
  scheduling, or concurrency semantics.
- Keep public signatures behavior-compatible; annotate to reflect actual null behavior so the
  annotations serve as accurate downstream contracts.

## Acceptance Criteria (early draft)

- [ ] Every `.cs` file under `UtilitiesCS/Threading/` that emits CS86xx carries `#nullable enable`
  and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] No project-level `<Nullable>` element is introduced in `UtilitiesCS.csproj`.
- [ ] No behavior change; no change to locking, ordering, or concurrency semantics; existing tests
  still pass; no coverage regression on changed lines.

## Constraints & Risks

- Follow the repo C# toolchain order (csharpier -> msbuild analyzers/codestyle -> nullable build
  with TreatWarningsAsErrors -> vstest with coverage). MSTest + Moq + FluentAssertions for any
  test work.
- This is the concurrency/ordering module (`concurrency_or_ordering` complexity floor). Nullable
  annotation must not alter locking, ordering, single-shot-guard, or scheduling behavior.
- The store-lockup watchdog (`StoreLockupResponder`) has a documented null-store-model hazard;
  annotations must not perturb its null-branch behavior.
- Annotations become cross-module contracts; incorrect annotations could propagate incorrect null
  assumptions to consumers.
- `TimeOutTask.cs` (975 lines) exceeds the repo 500-line limit — PRE-EXISTING; annotation-only
  work cannot bring it under 500 without a refactor. Flag, do not fix.
- WinForms Designer files (`*.Designer.cs`) and `.resx` are generated; default is to leave them
  non-opted-in (oblivious) and not hand-edit them.

## Test Conditions to Consider

- [ ] Existing MSTest suite for UtilitiesCS still passes post-annotation.
- [ ] No coverage regression on changed lines.
- [ ] Nullable gate (`/t:Rebuild /p:TreatWarningsAsErrors=true`) passes for the opted-in files.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/utilitiescs-nullable-threading/` folder from the template
