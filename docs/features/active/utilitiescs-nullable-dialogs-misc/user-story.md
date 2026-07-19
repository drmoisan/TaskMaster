# `utilitiescs-nullable-dialogs-misc` — User Story

- Issue: #374
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18
- Work Mode: full-feature

## Story Statement

- As the maintainer enforcing the repaired CI nullable gate, I want the `UtilitiesCS/Dialogs/`
  cluster (`ActionButton`, `DelegateButton`, `FunctionButton`, `InputBox`, `MyBox`,
  `MyBoxModeless`, `NotImplementedDialog`, `YesNoToAll`, and their viewer/code-behind
  companions) opted into per-file `#nullable enable` and brought to zero CS86xx diagnostics, so
  that this cluster is independently mergeable under the per-file pragma architecture without
  waiting for or cross-blocking any other Wave-1 or Wave-2 child.
- As the maintainer sequencing the epic's waves, I want this cluster's annotations to consume
  the Wave-0 `utilitiescs-nullable-extensions` `WinFormsExtensions.Clone<T>()` contract
  correctly, so that the Wave-2 capstone can later finalize gate enforcement across all
  remediated clusters without discovering an inconsistent annotation at this cluster's
  boundary.

## Problem / Why

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` (a genuine recompile
rather than a silently-skipped incremental build), cannot be enforced against new code until
the pre-existing nullable-reference-type debt (CS86xx diagnostics) is remediated under a
per-file `#nullable enable` opt-in architecture. This feature is the Wave-1 child that
remediates `UtilitiesCS/Dialogs/` (12 of 16 `.cs` files; 4 Designer-generated files excluded)
plus the smallest defensible "misc" component named by the epic's `dialogs-misc` label:
`UtilitiesCS/WindowsAPI/ExtraDeclarations.cs` and `UtilitiesCS/Properties/AssemblyInfo.cs`,
both verify-only. Fourteen files total receive the `#nullable enable` pragma.

These files consume exactly one cross-module contract from the Wave-0
`utilitiescs-nullable-extensions` child (issue #363): `WinFormsExtensions.Clone<T>()`, called
by `ActionButton`, `DelegateButton`, `FunctionButton`, and `MyBox`. No `HelperClasses/` (#364)
type is referenced anywhere in `Dialogs/`, which is flagged (not silently corrected) against
the epic manifest's declared `depends_on: [extensions, helperclasses]` edge for this child.

## Personas & Scenarios

- Persona: the maintainer (`drmoisan`) acting as CI-gate owner and epic sequencer.
  - Cares about: the repaired nullable gate becoming genuinely enforceable without a
    solution-wide "big bang" remediation blocking all other in-flight work.
  - Constraints: no behavior change permitted (dialog display and button-wrapper logic must
    remain identical); no project-level `<Nullable>` element may be introduced; net481/C# 12
    constraints preclude nullable post-condition attributes and `record`/`init` conversions
    (though this cluster carries no `record`/`struct` CS0518 risk); Designer-generated files
    must stay untouched and oblivious.
  - Goals: each Wave-1 child, including this one, merges independently and leaves
    non-remediated files elsewhere in the repository unaffected.
  - Frustrations addressed: previously the CI nullable step silently no-op'd on an incremental
    build (PR #361 fixed the mechanism); this feature addresses the debt the fixed mechanism
    now surfaces for the `Dialogs/` cluster specifically.
- Persona: the epic-planner reconciling the epic manifest's Wave-1 ownership map against actual
  file counts.
  - Cares about: the epic's "~16 files" estimate for `dialogs-misc` not silently ballooning if
    the "remaining small subdirs (catch-all)" wording is interpreted literally.
  - Constraints: several residual `UtilitiesCS` subdirectories (`Interfaces/**`,
    `OutlookObjects/`, `EmailIntelligence/` residual, `OneDriveHelpers/`, `Examples/`,
    `To Depricate/`) are thematically or dependency-wise unrelated to `Dialogs/`.
  - Goals: receive an explicit, evidence-backed table of what was excluded and why, rather than
    a silent scope decision buried in code changes.
  - Frustrations addressed: without this flag, a future reviewer might assume `dialogs-misc`
    was supposed to absorb ~110 additional residual files (an ~8x scope increase) that were
    never actually verified for genuine CS86xx risk.
- Scenario: a contributor opens a pull request that edits one of the 14 cluster files after this
  feature merges.
  - Trigger: a code change to, for example, `MyBox.cs`.
  - Steps: CI runs `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`; because
    `MyBox.cs` now carries `#nullable enable`, any newly introduced CS86xx diagnostic in that
    file fails the build; files elsewhere in the repository that remain non-opted-in (including
    the four Designer-generated siblings in this same cluster) are unaffected by the same run.
  - Obstacles/decisions: the contributor must resolve the nullable diagnostic using the same
    conventions this feature established (prefer `?` annotation and justified `!` over new
    runtime guards) rather than reintroducing debt or suppressing the warning.
  - Expected outcome: the gate enforces nullable correctness on the files this feature already
    remediated, while contributors touching non-opted-in files elsewhere are not blocked by
    unrelated debt.

## Acceptance Criteria

- [x] AC1: Every one of the 14 in-scope files (12 `Dialogs/` remediation targets +
  `ExtraDeclarations.cs` + `AssemblyInfo.cs`) carries `#nullable enable` and compiles with zero
  nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors`, so that the
  repaired gate can enforce this cluster without a global `/p:Nullable=enable` flag.
- [x] AC2: No project-level or solution-level `<Nullable>` element is introduced into
  `UtilitiesCS.csproj`, so that the per-file opt-in architecture required by the epic is
  preserved.
- [x] AC3: No behavior change to dialog display, button-wrapper, or MyBox logic; existing
  `UtilitiesCS.Test/Dialogs/` tests still pass, so that the remediation is verifiably
  annotation-only.
- [x] AC4: No coverage regression on changed lines, so that the annotation work does not
  introduce untested executable paths (for example new runtime null guards).
- [x] AC5: Public signatures of the remediated types remain behavior-compatible; nullability
  annotations reflect actual null behavior and are consistent with the consumed
  `WinFormsExtensions.Clone<T>()` contract from `utilitiescs-nullable-extensions` (#363), so that
  this cluster's contracts do not propagate an incorrect null-state assumption to any other
  cluster or to the Wave-2 capstone.
- [x] AC6: Non-remediated files (the 4 Designer-generated files and every other file outside
  this cluster) remain non-opted-in and are not cross-blocked; the change is independently
  mergeable under the per-file pragma architecture, so that this feature can merge without
  waiting on any other Wave-1/Wave-2 sibling beyond the confirmed #363 Batch D ordering
  precondition.

## Non-Goals

- No behavior changes, refactors, or feature work to dialog display, button-wrapper, or MyBox
  logic. This is null-annotation and null-safety remediation only.
- No API redesign; public method and property signatures remain behavior-compatible.
- No project-level `<Nullable>enable</Nullable>` flip in `UtilitiesCS.csproj`.
- No hand-editing of the four Designer-generated files
  (`DelegateButtonTemplate.Designer.cs`, `FolderNotFoundViewer.Designer.cs`,
  `InputBoxViewer.Designer.cs`, `MyBoxViewer.Designer.cs`); they stay non-opted-in and
  oblivious.
- No absorption of the larger residual `UtilitiesCS` subdirectories into this child's scope:
  `Interfaces/**` (~62 files, recommended for a repo-wide exclusion), `OutlookObjects/` root and
  8 leaf dirs (~13 files, recommended for an existing Outlook Wave-1 child),
  `EmailIntelligence/` root + `Evaluation/` + `OlFolderTools/` + `People/` (~26 files,
  recommended for a dedicated child), `Examples/MSDemoConv.cs` (demo code, maintainer
  decision), `To Depricate/*` (deprecation-candidate, maintainer decision), and
  `OneDriveHelpers/*` (undeclared dependency on the Threading Wave-0 child). These are flagged
  in the spec's "Ownership Gaps Flagged for Epic-Planner / Maintainer" section, not silently
  folded in.
- No resolution of the rules-vs-convention conflict between `.claude/rules/csharp.md`'s
  documented global `/p:Nullable=enable` toolchain step and this epic's per-file pragma
  convention; that conflict is flagged for the maintainer and deferred to the Wave-2 CI
  capstone child (`utilitiescs-nullable-ci-capstone`).
- No resolution of the unconfirmed `helperclasses` (#364) dependency edge declared by the epic
  manifest for `dialogs-misc`; this feature's scope finds no direct `HelperClasses/` reference
  within `Dialogs/` and flags the edge as unconfirmed-by-source rather than removing it.
