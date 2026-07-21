# `utilitiescs-nullable-email-parsing` — User Story

- Issue: #370
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18

## Story Statement

- As the maintainer enforcing the repaired CI nullable gate, I want the
  `EmailIntelligence` parsing/sorting cluster (`EmailParsingSorting/`, `SubjectMap/`, `Ctf/`)
  opted into per-file `#nullable enable` and brought to zero CS86xx diagnostics, so that this
  cluster is independently mergeable under the per-file pragma architecture without waiting for
  or cross-blocking any other Wave-1 or Wave-2 child.
- As the maintainer sequencing the epic's waves, I want this cluster's annotations to consume
  the Wave-0 `utilitiescs-nullable-extensions` (`NullExtensions.cs`, `StringExtensions.cs`,
  `IEnumerableExtensions.cs`) contracts correctly, so that the Wave-2 capstone can later finalize
  gate enforcement across all remediated clusters without discovering an inconsistent
  annotation at this cluster's boundary.

## Problem / Why

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` (a genuine recompile
rather than a silently-skipped incremental build), cannot be enforced against new code until
the pre-existing nullable-reference-type debt (CS86xx diagnostics) is remediated under a
per-file `#nullable enable` opt-in architecture. This feature is the Wave-1 child that
remediates the `EmailIntelligence` parsing/sorting cluster only:
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/`, `UtilitiesCS/EmailIntelligence/SubjectMap/`,
and `UtilitiesCS/EmailIntelligence/Ctf/` (24 of 25 `.cs` files; `SubjectMapMetrics.Designer.cs`
is excluded as Designer-generated).

These files consume the shared extension-method annotations delivered by the Wave-0
`utilitiescs-nullable-extensions` child (issue #363), whose nullability annotations are the
cross-module contracts this cluster relies on for correct null-flow analysis.

## Personas & Scenarios

- Persona: the maintainer (`drmoisan`) acting as CI-gate owner and epic sequencer.
  - Cares about: the repaired nullable gate becoming genuinely enforceable without a
    solution-wide "big bang" remediation blocking all other in-flight work.
  - Constraints: no behavior change permitted (parsing/sorting logic must remain identical);
    no project-level `<Nullable>` element may be introduced; net481/C# 12 constraints preclude
    nullable post-condition attributes and `record`/`init` conversions.
  - Goals: each Wave-1 child, including this one, merges independently and leaves
    non-remediated files elsewhere in the repository unaffected.
  - Frustrations addressed: previously the CI nullable step silently no-op'd on an incremental
    build (PR #361 fixed the mechanism); this feature addresses the debt the fixed mechanism
    now surfaces for the `EmailIntelligence` parsing/sorting cluster specifically.
- Scenario: a contributor opens a pull request that edits one of the 24 cluster files after
  this feature merges.
  - Trigger: a code change to, for example, `EmailFilerConfig.cs`.
  - Steps: CI runs `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`; because
    `EmailFilerConfig.cs` now carries `#nullable enable`, any newly introduced CS86xx diagnostic
    in that file fails the build; files elsewhere in the repository that remain non-opted-in are
    unaffected by the same run.
  - Obstacles/decisions: the contributor must resolve the nullable diagnostic using the same
    conventions this feature established (prefer `?` annotation and justified `!` over new
    runtime guards) rather than reintroducing debt or suppressing the warning.
  - Expected outcome: the gate enforces nullable correctness on the files this feature already
    remediated, while contributors touching non-opted-in files elsewhere are not blocked by
    unrelated debt.

## Acceptance Criteria

- [x] AC1: Every `.cs` file in the cluster (`EmailParsingSorting/`, `SubjectMap/`, `Ctf/`) that
  emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the
  per-file pragma with `TreatWarningsAsErrors`, so that the repaired gate can enforce this
  cluster without a global `/p:Nullable=enable` flag.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`, so
  that the per-file opt-in architecture required by the epic is preserved.
- [x] AC3: No behavior change to parsing/sorting logic; existing tests still pass, so that the
  remediation is verifiably annotation-only.
- [x] AC4: No coverage regression on changed lines, so that the annotation work does not
  introduce untested executable paths (for example new runtime null guards).
- [x] AC5: Public signatures of the remediated types remain behavior-compatible; nullability
  annotations reflect actual null behavior and are consistent with the upstream
  `utilitiescs-nullable-extensions` annotation contracts they consume, so that this cluster's
  contracts do not propagate an incorrect null-state assumption to any other cluster or to the
  Wave-2 capstone.
- [x] AC6: Non-remediated files remain non-opted-in and are not cross-blocked; the change is
  independently mergeable under the per-file pragma architecture, so that this feature can merge
  without waiting on `utilitiescs-nullable-email-classifier` or any other Wave-1/Wave-2 sibling.

## Non-Goals

- No behavior changes, refactors, or feature work to the parsing/sorting logic. This is
  null-annotation and null-safety remediation only.
- No API redesign; public method and property signatures remain behavior-compatible.
- No splitting of files that exceed the 500-line general file-size limit
  (`SortEmail.cs`, `EmailTokenizer.cs`, `SubjectMapEntry.cs`) — these are pre-existing
  conditions flagged for a future issue, not fixed here.
- No project-level `<Nullable>enable</Nullable>` flip in `UtilitiesCS.csproj`.
- No remediation of `SubjectMapMetrics.Designer.cs` (Designer-generated code, excluded).
- No resolution of the rules-vs-convention conflict between `.claude/rules/csharp.md`'s
  documented global `/p:Nullable=enable` toolchain step and this epic's per-file pragma
  convention; that conflict is flagged for the maintainer and deferred to the Wave-2 CI
  capstone child (`utilitiescs-nullable-ci-capstone`).
- No remediation of any cluster outside `EmailParsingSorting/`, `SubjectMap/`, and `Ctf/`
  (in particular, `EmailIntelligence/Bayesian`/ClassifierGroups/Flags is the separate
  `utilitiescs-nullable-email-classifier` Wave-1 sibling).
