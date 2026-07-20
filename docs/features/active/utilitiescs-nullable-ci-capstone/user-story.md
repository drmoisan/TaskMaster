# `utilitiescs-nullable-ci-capstone` — User Story

- Issue: #376
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-19

## Story Statement

- As the repository maintainer / CI owner, I want the nullable gate to enforce per-file
  `#nullable enable` opt-in genuinely — opted-in files fail on real null defects, non-opted-in
  files never cross-block — so that the gate PR #361 repaired can be relied on going forward
  without permanently blocking PRs that touch not-yet-remediated code.
- As the maintainer closing out the `utilitiescs-nullable-remediation` epic, I want a single
  consolidated view of every outstanding maintainer decision the twelve remediation children
  accumulated, so that I do not have to read twelve child specs individually to know what still
  needs my judgment.

## Problem / Why

What need or gap does this idea address?

PR #361 repaired the CI nullable gate to run a genuine `/t:Rebuild` recompile instead of a
silently-skipped incremental build, using `/p:Nullable=enable /p:TreatWarningsAsErrors=true`.
That repair is correct as a build-freshness fix, but the `/p:Nullable=enable` flag forces the
nullable context on for every file solution-wide. Under the epic's chosen per-file `#nullable
enable` opt-in architecture — chosen so each of the twelve remediation children can merge
independently rather than requiring all ~234 files fixed at once — that global flag would defeat
the point: it would surface the full pre-existing CS86xx debt (~2131 diagnostics) across every
not-yet-remediated file the moment any child's PR runs the gate.

This capstone is the Wave-2 child that removes the global override so the gate matches the
convention it is meant to enforce, proves the resulting gate is a real enforcement mechanism
(not merely a mechanism that happens to pass), flags a policy-document conflict it cannot itself
resolve, evaluates but does not perform an optional stronger alternative, and gathers the
maintainer decisions the other eleven children left open.

## Personas & Scenarios

- Persona: Repository maintainer / CI owner
  - who the user is: owns the CI nullable gate, the epic's per-file opt-in architecture, and the
    final merge decision for both this capstone and the epic's integration-to-`main` PR.
  - what they care about: a gate that catches real null defects in opted-in files without
    cross-blocking unrelated, not-yet-remediated PRs; no production behavior change; no
    reduction in coverage on changed lines; a small number of clearly-surfaced decisions rather
    than decisions buried across twelve child specs.
  - their constraints: cannot approve edits to `.claude/rules/*`; must decide, separately from
    this feature, whether to ratify the optional project-level `<Nullable>` flip and how to
    reconcile the `csharp.md` conflict; needs the capstone's verification evidence to trust the
    finalized gate before merging.
  - their goals and frustrations: wants confidence that "opted-in enforced, non-opted-in silent"
    is proven, not asserted; frustrated by the prospect of re-deriving the same maintainer
    decisions independently from each of twelve child specs.
  - their context and motivations: this is the last child in the epic (Wave 2); every other
    child depends on this one to actually flip the CI gate over to the per-file model it was
    built against.
  - Scenario: Reviewing the finalized gate and its verification evidence
    - who is acting: the maintainer, reviewing the capstone's PR.
    - what triggered the action: the twelve Wave-0/Wave-1 children have landed (or are landing)
      under the per-file opt-in convention; the gate step still carries the global
      `/p:Nullable=enable` flag left over from PR #361.
    - what steps they take: confirm the single-line workflow diff drops
      `/p:Nullable=enable` and keeps `/t:Rebuild /p:TreatWarningsAsErrors=true`; review the four
      verification evidence artifacts (clean baseline, opted-in-defect fail, non-opted-in-defect
      pass, restored-clean) and confirm `git status` is clean; confirm a green workflow run
      exists against the branch head (required by `modified-workflow-needs-green-run` before
      merge); review the Maintainer Decision Summary in `spec.md` in place of reading every
      child's own maintainer-decision notes.
    - what obstacles or decisions occur: the maintainer must separately decide (not as part of
      this feature) how to resolve the `.claude/rules/csharp.md` conflict and whether to later
      ratify the optional project-level `<Nullable>` flip; both are presented as options, not
      resolved here.
    - what outcome they expect: the gate enforces opted-in files and does not cross-block
      non-opted-in files, proven by evidence rather than asserted; the maintainer has one place
      to review outstanding decisions instead of twelve.

- Persona: Wave-0/Wave-1 remediation-child developer (any of the twelve prior children)
  - who the user is: a developer who already remediated their cluster under the per-file pragma
    convention and is relying on this capstone to make the CI gate actually match that
    convention.
  - what they care about: that their already-merged (or merging) child's files are not
    cross-blocked by other clusters' not-yet-remediated debt once this capstone lands; that the
    gate command their own child's spec already assumed (`/t:Rebuild
    /p:TreatWarningsAsErrors=true`, no global `/p:Nullable=enable`) is the one actually running
    in CI.
    - their constraints: their own child's spec and plan were written against the finalized gate
      contract before this capstone executed; if the capstone's edit diverged from that
      assumption, their child's own verification would be invalidated.
  - their goals and frustrations: wants the capstone to confirm, not silently change, the gate
    contract every other child was built against.
  - their context and motivations: depends on this capstone (their `feature_folder` is listed in
    the capstone's `depends_on` in the epic manifest), but is not blocked by it in the other
    direction — their own remediation work does not wait on the capstone to execute.
  - Scenario: Confirming the gate contract did not change underneath an already-remediated file
    - who is acting: a developer (or the epic-orchestrator, on their behalf) checking that the
      capstone's edit matches what their own child's `spec.md` already stated as the gate
      command.
    - what triggered the action: the capstone's PR is ready to merge into the integration branch.
    - what steps they take: compare the capstone's after-text gate command against the identical
      command already recorded in their own child's spec (e.g., residuals #375 spec.md line 336,
      svgcontrol #368 spec.md line 263).
    - what obstacles or decisions occur: none expected — the capstone's research confirmed all
      twelve children's plans were authored against this exact gate-step contract, so the edit is
      a finalization, not a new design.
    - what outcome they expect: no surprise; the running gate matches what their own remediation
      was already verified against.

## Acceptance Criteria

- [x] AC1: The CI nullable-gate step no longer passes `/p:Nullable=enable` globally; it runs the
  gate under `/t:Rebuild /p:TreatWarningsAsErrors=true` and relies on each file's own `#nullable
  enable` pragma. Opted-in files are enforced; non-opted-in files do not cross-block. So that:
  the maintainer can trust that merging a not-yet-remediated cluster's PR will not be blocked by
  unrelated pre-existing debt, while an opted-in file's real null defects still fail the build.
- [x] AC2: A genuine-enforcement verification is defined and executed: a deliberately-introduced
  null defect in an opted-in file fails the gate, and the same class of defect in a non-opted-in
  file does not fail the gate. The introduced defect is reverted before completion; the
  verification is evidenced. So that: the maintainer has proof, not an assertion, that the gate
  distinguishes opted-in from non-opted-in files correctly, and the branch is left exactly as
  clean as it started.
- [x] AC3: Any pwsh step or workflow YAML that is added or modified complies with
  `.claude/rules/ci-workflows.md` (deliberately-failing nested command exit-code handling) and,
  where applicable, `.claude/rules/benchmark-baselines.md`. No leaked `$LASTEXITCODE` on the
  success path. So that: the workflow edit cannot silently leak a stale non-zero exit code and
  report a false failure (or false success) to GitHub Actions.
- [x] AC4: The `.claude/rules/csharp.md` rules-vs-convention conflict (the rule documents
  forcing `/p:Nullable=enable` globally, which conflicts with the per-file opt-in convention) is
  surfaced as an explicit maintainer-decision item in `spec.md`. No `.claude/rules/*` file is
  edited. So that: the maintainer is aware of the documentation/policy inconsistency and can
  choose how to resolve it, without this feature overstepping the policy prohibition on editing
  rule files.
- [x] AC5: The optional project-level `<Nullable>enable</Nullable>` capstone decision for
  `UtilitiesCS.csproj` and `SVGControl.csproj` is documented as a separately-gated OPTIONAL step
  with an explicit maintainer decision gate, not performed by default in this feature. So that:
  the maintainer can later choose a stronger, more conventional enforcement posture once the
  remaining Designer/Interfaces risk surface is verified clean, without this feature forcing that
  decision or its risk prematurely.
- [x] AC6: A single consolidated maintainer-decision summary is present in `spec.md`, folding in
  the epic-wide exclusions (`Interfaces/**` ~62 files where CS8618 cannot fire;
  `Properties/Resources.Designer.cs` and `Settings.Designer.cs` left null-oblivious) and the
  child flags: `PeopleScoDictionaryNewBackup.cs` (dead uncompiled duplicate, exclude/delete
  decision), 6 `OlFolderTools` Designer files left oblivious, three pre-existing >500-line files
  not split, and `MSDemoConv.cs` / `To Depricate/*` / `MailResolution_ToRemove` maintainer
  decisions. So that: the maintainer reviews one artifact instead of reading every child spec to
  find open decisions.
- [x] AC7: No behavior change to production C# code and no reduction in coverage on changed
  lines (the feature's changes are limited to CI workflow YAML, feature documents, and — only if
  the optional AC5 gate is later approved — csproj `<Nullable>` elements, which is out of scope
  for this feature's default execution). So that: the capstone, like every other epic child,
  remains strictly an annotation/enforcement change with no production risk.

## Non-Goals

Call out what is explicitly excluded from this feature.

- No project-level or solution-level `<Nullable>` flip is performed by default. `UtilitiesCS.csproj`
  and `SVGControl.csproj` keep no `<Nullable>` element as part of this feature's execution; the
  flip is documented as an optional, separately maintainer-gated future step only (AC5).
- No editing of `.claude/rules/*`. The `.claude/rules/csharp.md` rules-vs-convention conflict is
  flagged in `spec.md`, not resolved by changing rule text (AC4).
- No production C# behavior change and no new/modified test logic. The only source-adjacent
  change is the deliberately-introduced-and-reverted defects used for the AC2 verification, which
  must leave `git status` clean at completion — they are not a lasting code change.
- No change to the exit-code handling, step name, `shell: pwsh` directive, or any other line in
  the gate step beyond the single `/p:Nullable=enable` removal and its explanatory comment (AC1,
  AC3).
- No resolution of the other eleven children's individual maintainer decisions (dead-code
  deletions, Designer-file exclusions, oversized-file splits, dependency-edge corrections). This
  feature consolidates them into one summary (AC6) but does not decide them.
- No workflow-run execution guarantee from this document alone: the
  `modified-workflow-needs-green-run` requirement (a green CI run against the branch head before
  merge) is an execution/merge-time obligation carried out by atomic execution and the
  epic-orchestrator, not satisfied by authoring these feature documents.
