# `utilitiescs-nullable-residuals` — User Story

- Issue: #375
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T23-13
- Work Mode: full-feature

## Story Statement

- As the repository maintainer, I want the residual, previously-unowned CS86xx-risk trees under
  `UtilitiesCS/` (Examples, To Depricate, OneDriveHelpers, the OutlookObjects residual, and the
  EmailIntelligence residual — 44 files) opted in per-file and brought clean, so that the CI
  nullable gate repaired by PR #361 can be genuinely enforced against new code without permanently
  blocking future PRs.
- As a developer opening a future PR that touches these trees, I want each residual file already
  annotated to reflect its actual runtime null behavior, so that the pragma-driven gate gives me an
  accurate signal on my own change rather than surfacing pre-existing debt I did not introduce.

## Problem / Why

PR #361 repaired the CI nullable gate to use `msbuild /t:Rebuild` so it performs a genuine
recompile instead of a silently-skipped incremental build. That repair means the gate now surfaces
pre-existing CS86xx nullable-reference-type diagnostics that were previously masked. The epic
`utilitiescs-nullable-remediation` remediates this ~2131-diagnostic backlog under a per-file
`#nullable enable` opt-in so that opted-in files are enforced while non-opted-in files remain
oblivious and are not cross-blocking.

When the epic's Wave-1 children were assigned, the `dialogs-misc` (#374) child narrowed its scope
to `UtilitiesCS/Dialogs/` and flagged a set of residual, unowned trees that no other Wave-0 or
Wave-1 child claimed. The epic's Residual-Scope Decision reconciled those flags against the
definition-of-done inventory by exact `.cs` file count and assigned the 44 residual files with
genuine CS86xx risk to this child. Without this child, those residual trees would remain a
permanent source of gate failures once the gate is enforced — exactly the "permanently blocking
future PRs" outcome the epic exists to prevent.

## Personas & Scenarios

- Persona: repository maintainer (drmoisan)
  - who the user is: owner of the TaskMaster repository and the nullable-remediation epic.
  - what they care about: turning on genuine nullable enforcement without creating a wall of
    pre-existing failures that block unrelated PRs.
  - their constraints: net481 target (no post-condition attributes, no `record`/`init`); no
    project-level `<Nullable>` flip; no behavior changes; no refactors; policy prohibits editing
    `.claude/rules/*`; the 500-line file-size limit.
  - their goals and frustrations: wants the residual unowned trees cleaned so the epic's
    definition of done is met; does not want annotation effort spent on dead or deprecation-marked
    code without an explicit decision.
  - their context and motivations: this is the last remediation child (with the CI capstone) that
    stands between the epic and enforceable per-file nullable checking.
- Scenario: bringing the residual trees to zero CS86xx under the pragma
  - who is acting: the executor implementing this child's atomic plan.
  - what triggered the action: the residuals child was promoted to an active feature (issue #375)
    after its three Wave-0 upstreams (extensions #363, helperclasses #364, threading #369) were
    prepared.
  - what steps they take: capture a clean baseline `vstest.console.exe` run for `UtilitiesCS.Test`;
    then, batch by batch (leaf-first, Designer files never opted in), add `#nullable enable` to
    each in-scope hand-written file and annotate to zero CS86xx under the pragma-only
    `/t:Rebuild /p:TreatWarningsAsErrors=true` build; preserve existing guards; prefer annotation
    plus justified `!` over new runtime guards; keep annotations consistent with the upstream
    extensions/helperclasses/threading contracts.
  - what obstacles or decisions occur: three pre-existing >500-line files are flagged, not split;
    a dead uncompiled duplicate (`PeopleScoDictionaryNewBackup.cs`), a demo file
    (`MSDemoConv.cs`), two deprecation-marked files, and a `_ToRemove`-suffixed type are surfaced
    as maintainer decisions rather than silently resolved; an undeclared `ReusableTypeClasses`
    (#366) edge is flagged for the epic-planner.
  - what outcome they expect: each compiled opted-in file reaches zero CS86xx under the pragma-only
    gate, tests stay green with no coverage regression on changed lines, and the six maintainer
    decisions are recorded in `spec.md`.

## Acceptance Criteria

- [x] AC1: Every compiled in-scope hand-written file carries `#nullable enable` and produces zero
  CS86xx diagnostics under the pragma-only build
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true`.
- [x] AC2: No `<Nullable>` element is added to `UtilitiesCS.csproj` or the solution; verification
  uses the pragma-only command with no global `/p:Nullable=enable`.
- [x] AC3: The 6 `*.Designer.cs` files under `OlFolderTools` are left oblivious (no pragma) and are
  not cross-blocked.
- [x] AC4: No behavior change — no new types, no post-condition attributes, no `record`/`record
  struct`/`init`, existing guards preserved, no new runtime guard beyond what reaching zero CS86xx
  strictly requires.
- [x] AC5: Annotations are consistent with the upstream extensions/helperclasses/threading
  annotated signatures (for example `TimeOutTask.RunWithTimeout` returns non-null `Task<TResult>`;
  `StreamExtensions.TryCopyToAsyncWithTimeout` returns `Task<bool>`).
- [x] AC6: A clean baseline test run is captured before edits, and no test regressions or
  changed-line coverage regressions are attributable to this child.
- [x] AC7: The six Maintainer Decisions and Flags (dead duplicate exclusion, `MSDemoConv.cs`
  decision, deprecation-marked files, `MailResolution_ToRemove`, the `ReusableTypeClasses` #366
  edge, and the three 500-line breaches) are recorded in `spec.md`.
- [x] AC8: No in-scope file exceeds 500 lines as a result of edits; the three pre-existing
  >500-line files are flagged, not split.

## Non-Goals

- No behavior change. This is null-annotation and null-safety remediation only; observable runtime
  behavior before and after is identical.
- No refactor. No file is split, no method is restructured, and no API is redesigned — including
  the three pre-existing >500-line files, which are flagged rather than split.
- No deletion of the flagged files within this scope. The dead duplicate
  (`PeopleScoDictionaryNewBackup.cs`), the deprecation-marked `To Depricate/*` files, and the
  `MailResolution_ToRemove` type are flagged for a maintainer decision and are not deleted by this
  child.
- No project-level or solution-level `<Nullable>` flip. Enforcement stays per-file pragma only; a
  global `<Nullable>enable</Nullable>` is out of scope and reserved for the epic's Wave-2 CI
  capstone consideration.
- No editing of `.claude/rules/*`. The rules-versus-convention conflict (the stock toolchain forces
  `/p:Nullable=enable` globally) is flagged for the maintainer, not resolved here.
- No post-condition attributes, `record`/`record struct`/`init`, or other net481-unavailable
  constructs are introduced.
