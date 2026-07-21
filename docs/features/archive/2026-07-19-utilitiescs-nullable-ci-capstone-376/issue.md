# utilitiescs-nullable-ci-capstone (Issue #376)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/ (Issue #376)

- Issue: #376
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/376
- Last Updated: 2026-07-18
- Work Mode: full-feature
- Parent: Epic `utilitiescs-nullable-remediation` (Wave 2 capstone)

## Problem / Why

The CI nullable gate was repaired by PR #361 to run
`msbuild TaskMaster.sln /t:Rebuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`, so it
performs a genuine full recompile instead of a silently-skipped incremental build. The twelve
Wave-0/Wave-1 remediation children (issues #363, #364, #365, #366, #367, #368, #369, #370, #371,
#372, #374, #375) remediate the pre-existing CS86xx nullable debt under a per-file `#nullable
enable` opt-in architecture (`UtilitiesCS.csproj` and `SVGControl.csproj` have no project-level
`<Nullable>` element).

The gate step as repaired still passes `/p:Nullable=enable` globally, which forces nullable
project-wide and would surface the full pre-existing CS86xx debt across every not-yet-remediated
file. Under a per-file opt-in convention, a not-yet-opted-in file must not cross-block. This
capstone finalizes the CI enforcement mechanism so it matches the per-file opt-in architecture:
opted-in files are enforced; non-opted-in files are not cross-blocking. It also flags a policy
conflict, evaluates an optional project-level flip, and consolidates the epic's
maintainer-decision items.

## Proposed Behavior

1. Revise/supersede the PR #361 gate step so it DROPS the global `/p:Nullable=enable` override and
   relies on each file's own `#nullable enable` pragma under `/t:Rebuild
   /p:TreatWarningsAsErrors=true`.
2. Provide genuine-enforcement verification: a deliberately-introduced null defect in an opted-in
   file must fail the gate; the same class of defect in a non-opted-in file must not.
3. Preserve the exit-code handling and workflow-authoring rules already in place
   (`.claude/rules/ci-workflows.md`, `.claude/rules/benchmark-baselines.md`) for any pwsh step or
   workflow YAML touched.
4. Flag the `.claude/rules/csharp.md` rules-vs-convention conflict for the maintainer (do not edit
   any `.claude/rules/*` file).
5. Evaluate an OPTIONAL, separately-gated project-level `<Nullable>enable</Nullable>` flip for
   `UtilitiesCS.csproj` and `SVGControl.csproj`, presented as a maintainer decision gate, not a
   default action.
6. Consolidate the epic-wide exclusions and accumulated child maintainer-decision flags into a
   single maintainer-decision summary.

## Acceptance Criteria

- [ ] AC1: The CI nullable-gate step no longer passes `/p:Nullable=enable` globally; it runs the
  gate under `/t:Rebuild /p:TreatWarningsAsErrors=true` and relies on each file's own `#nullable
  enable` pragma. Opted-in files are enforced; non-opted-in files do not cross-block.
- [ ] AC2: A genuine-enforcement verification is defined and executed: a deliberately-introduced
  null defect in an opted-in file fails the gate, and the same class of defect in a non-opted-in
  file does not fail the gate. The introduced defect is reverted before completion; the verification
  is evidenced.
- [ ] AC3: Any pwsh step or workflow YAML that is added or modified complies with
  `.claude/rules/ci-workflows.md` (deliberately-failing nested command exit-code handling) and,
  where applicable, `.claude/rules/benchmark-baselines.md`. No leaked `$LASTEXITCODE` on the
  success path.
- [ ] AC4: The `.claude/rules/csharp.md` rules-vs-convention conflict (the rule documents forcing
  `/p:Nullable=enable` globally, which conflicts with the per-file opt-in convention) is surfaced
  as an explicit maintainer-decision item in `spec.md`. No `.claude/rules/*` file is edited.
- [ ] AC5: The optional project-level `<Nullable>enable</Nullable>` capstone decision for
  `UtilitiesCS.csproj` and `SVGControl.csproj` is documented as a separately-gated OPTIONAL step
  with an explicit maintainer decision gate, not performed by default in this feature.
- [ ] AC6: A single consolidated maintainer-decision summary is present in `spec.md`, folding in
  the epic-wide exclusions (`Interfaces/**` ~62 files where CS8618 cannot fire;
  `Properties/Resources.Designer.cs` and `Settings.Designer.cs` left null-oblivious) and the child
  flags: `PeopleScoDictionaryNewBackup.cs` (dead uncompiled duplicate, exclude/delete decision),
  6 `OlFolderTools` Designer files left oblivious, three pre-existing >500-line files not split,
  and `MSDemoConv.cs` / `To Depricate/*` / `MailResolution_ToRemove` maintainer decisions.
- [ ] AC7: No behavior change to production C# code and no reduction in coverage on changed lines
  (the feature's changes are limited to CI workflow YAML, feature documents, and — only if the
  optional AC5 gate is later approved — csproj `<Nullable>` elements, which is out of scope for
  this feature's default execution).

## Constraints & Risks

- Policy PROHIBITS editing `.claude/rules/*`. The csharp.md conflict is flagged, not resolved.
- `TreatWarningsAsErrors=true` promotes ALL warnings (not only CS86xx) to errors; this matches the
  existing PR #361 step behavior. Dropping `/p:Nullable=enable` only changes the nullable context
  source (per-file pragma vs. global force), not the warnings-as-errors scope.
- The genuine-enforcement verification must revert its deliberately-introduced defect; it must not
  leave a failing gate or a real defect on the branch.
- The optional project-level flip (AC5) is explicitly NOT executed by default; the epic Non-Goals
  restrict a project-level flip to an optional, separately-gated step.

## Test Conditions to Consider

- [ ] Pragma-driven gate (`/t:Rebuild /p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`)
  passes on the integration branch head with all twelve children fanned in.
- [ ] Deliberate null defect in an opted-in file fails the gate (fail-before evidence).
- [ ] Deliberate null defect in a non-opted-in file does not fail the gate (non-cross-block
  evidence).
- [ ] The existing MSTest suite and coverage gate are unaffected by the gate-step change.

## Next Step

- [x] Promote to GitHub issue (feature request template) — Issue #376
- [x] Create `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/` folder from the template
- [ ] Author spec.md and user-story.md (full-feature)
- [ ] Generate and preflight-clear the atomic plan
