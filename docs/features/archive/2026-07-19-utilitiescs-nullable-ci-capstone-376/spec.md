# utilitiescs-nullable-ci-capstone — Spec

- **Issue:** #376
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 2 capstone)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-19
- **Status:** Draft
- **Version:** 0.1

## Overview

What need or gap does this idea address?

PR #361 repaired the CI nullable gate so `.github/workflows/ci.yml`'s "Build with nullable
warnings treated as errors" step (lines 103–115) runs `msbuild /t:Rebuild ...
/p:Nullable=enable /p:TreatWarningsAsErrors=true`, performing a genuine full recompile instead
of a silently-skipped incremental build. The twelve Wave-0/Wave-1 remediation children of the
`utilitiescs-nullable-remediation` epic (issues #363–#375) remediate the pre-existing CS86xx
nullable debt (~2131 diagnostics across ~234 files) under a per-file `#nullable enable` opt-in
architecture — `UtilitiesCS.csproj` and `SVGControl.csproj` carry no project-level `<Nullable>`
element.

The repaired gate step, as it stands, still passes `/p:Nullable=enable` globally. That flag
forces the nullable annotation/warning context on for every file in both projects, which would
surface the full pre-existing CS86xx debt across every not-yet-remediated file — the opposite of
the per-file opt-in convention the twelve children are built against. This capstone is the
Wave-2 child that finalizes the CI enforcement mechanism to match that convention: opted-in
files are enforced, non-opted-in files do not cross-block. It also verifies the finalized gate
genuinely enforces (rather than merely permits), flags a rules-vs-convention policy conflict for
the maintainer, evaluates (without executing) an optional project-level `<Nullable>` flip, and
consolidates the epic's accumulated maintainer-decision items into one place.

This feature's default execution is limited to: (1) a single-line CI workflow YAML edit, (2)
these feature documents, (3) revertible verification evidence, and (4) build-debt remediation of
pre-existing/fan-in nullable (CS86xx) and CS0649 debt on already-opted-in files, surfaced only
now that all twelve sibling children have merged into the integration branch (see "Scope
reconciliation (2026-07-19)" below). No project-level `<Nullable>` flip is performed by default.
"No production C# behavior changes" means no observable-behavior change: item (4)'s edits are
nullable-annotation-only or a narrow, documented warning suppression, not a logic change (see AC7
clarification below).

### Scope reconciliation (2026-07-19)

This section's original wording (drafted before the twelve sibling children merged) stated this
feature's default execution excludes production `.cs` edits entirely. That statement is revised
here because the fan-in debt it did not anticipate only becomes measurable once all twelve
children are fanned in — the research pass underlying this spec explicitly examined a pre-fan-in
worktree tip (`dd17719a`) where only 25+3 files were opted into `#nullable`; ci.yml never runs on
the integration branch (its triggers are scoped to `[main, development]` only), so this debt was
never previously surfaced by any CI run.

Once this capstone's atomic plan was revised against the actual, fully-fanned-in integration
branch tip (`bfcdb394`), two concrete build-blocking defects were confirmed:

- **SVGControl/SvgImageSelector.cs CS0649** (`_relativeImagePath`, `_absoluteImagePath`): both
  fields are never assigned because the `ImagePath` setter body is entirely commented out (a
  pre-existing, already-documented dead no-op per #368's judgment-call decision). This blocks the
  `/t:Rebuild` dependency chain before other projects are even reached.
- **UtilitiesCS nullable fan-in debt**: 296 CS86xx-range diagnostics plus 28 CS0618 and 2 CS0168
  (also promoted to errors by the same `/p:TreatWarningsAsErrors=true` flag) across 62 already
  `#nullable enable`-opted-in files under `UtilitiesCS/EmailIntelligence/**` and
  `UtilitiesCS/OutlookObjects/Folder/**` — this is the first measurement of this debt taken
  against the fully-fanned-in integration tip; no prior on-disk estimate exists to compare
  against (`ci.yml` never triggers on the integration branch, so no CI run had previously
  surfaced it).

This expansion was authorized by this capstone's child orchestrator's own 2026-07-19 session
decision, made directly at delegation time when the fan-in debt was first measured on the
fully-fanned-in integration tip (`bfcdb394`) -- it is not a pre-existing line item that was already
recorded in `docs/features/epics/utilitiescs-nullable-remediation/epic.md` before this session.
Both build-debt items are this capstone's own responsibility, not any sibling child's: no
scope-locked child could remediate the fan-in debt under its own per-cluster lock, since it is
cross-child annotation propagation rather than any single child's own files, and the SVGControl
CS0649 defect blocks the very gate this capstone exists to finalize. A durable record of this
scope-expansion decision is added at the epic layer as a "Capstone scope addendum (2026-07-19)"
immediately below the Wave 2 table in `epic.md`, so a later feature-review or epic fan-in audit
can verify the expansion without relying on this document alone. The remediation itself is
annotation-only (nullable annotations, null-forgiving operators, guard clauses) or a narrow,
documented `#pragma warning disable`/`restore` bracket — non-behavioral by construction, so it
does not conflict with AC7's "no behavior change to production C# code."

## Behavior

What should the feature do at a high level?

1. **Gate-step edit (AC1).** Remove `/p:Nullable=enable` from the `msbuild` invocation in
   `.github/workflows/ci.yml` lines 103–115, keeping `/t:Rebuild` and
   `/p:TreatWarningsAsErrors=true` unchanged. Enforcement then relies entirely on each file's own
   `#nullable enable` pragma: an opted-in file's CS86xx diagnostics are promoted to build errors
   by `TreatWarningsAsErrors`; a non-opted-in file compiles under the oblivious nullable context
   and cannot emit CS86xx at all, so it has nothing for `TreatWarningsAsErrors` to promote.
2. **Genuine-enforcement verification (AC2).** Using a still-current re-grep of `#nullable
   enable` at execution time (not the file counts recorded in this spec, which are a snapshot),
   select one currently-opted-in file and one currently-non-opted-in file. Introduce a
   deliberate null defect in each, one at a time, run the finalized gate command, and capture
   `EXIT_CODE` plus a short output summary as an evidence artifact for each of four runs
   (clean baseline, opted-in-defect fail, non-opted-in-defect pass, restored-clean). Revert both
   defects before completion; leave `git status` clean.
3. **Workflow-authoring-rule compliance (AC3).** Confirm the edited step's existing exit-code
   handling (`if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`) is preserved verbatim and that no
   new deliberately-failing nested command is introduced into the workflow `run:` block itself.
4. **Rules-vs-convention conflict flag (AC4).** Record, without editing, the conflict between
   `.claude/rules/csharp.md`'s documented toolchain (which specifies `/p:Nullable=enable`
   globally) and the per-file opt-in convention this capstone's AC1 finalizes in the live CI
   workflow. Present two maintainer options without choosing between them.
5. **Optional project-level flip evaluation (AC5).** Document the trade-offs and current
   infeasibility of an optional project-level `<Nullable>enable</Nullable>` flip for
   `UtilitiesCS.csproj` and `SVGControl.csproj`, as a separately-gated, maintainer-decision step.
   This feature does not perform the flip.
6. **Maintainer-decision consolidation (AC6).** Reproduce the epic's full source-cited
   maintainer-decision inventory in one place so the maintainer is not required to read every
   child spec.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none new. The gate step continues to consume the existing
  `$env:SOLUTION_PATH` environment variable and the existing `msbuild`/`TaskMaster.sln` inputs;
  no new CLI flag, environment variable, or configuration input is introduced.
- Outputs (artifacts, logs, telemetry): four verification-run evidence artifacts under
  `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/evidence/other/` (fail-before,
  pass-after, clean-baseline, restored-clean), per `evidence-and-timestamp-conventions`. No
  production logging or telemetry is added.
- Config keys and defaults: none. No new `.editorconfig`, `.globalconfig`, or MSBuild property is
  introduced.
- Versioning or backward-compatibility constraints: the workflow YAML edit is not a public API
  and has no versioning implication. `TreatWarningsAsErrors=true` continues to promote all
  warnings (not only CS86xx) to build errors, unchanged from the PR #361 behavior; only the
  nullable-context source changes (per-file pragma vs. a global force-enable), not the
  warnings-as-errors scope.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

There is no CLI surface and no new API. The only "surface" is the `msbuild` invocation inside the
CI workflow step.

- Example invocations with expected outputs (concise):
  - Before (current, PR #361 state — `.github/workflows/ci.yml` lines 103–115):
    ```yaml
          - name: Build with nullable warnings treated as errors
            shell: pwsh
            run: |
              # Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
              # recompile under /p:Nullable=enable. The preceding "Build with analyzers"
              # step already compiled everything under the projects' own Nullable settings;
              # MSBuild's incremental up-to-date check does not invalidate on a changed
              # -p:Nullable command-line property alone, so a plain /t:Build here would
              # silently skip recompilation and never actually enforce this gate.
              & msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug `
                  "/p:Platform=Any CPU" `
                  /p:Nullable=enable /p:TreatWarningsAsErrors=true
              if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    ```
  - After (proposed, this capstone's AC1 edit):
    ```yaml
          - name: Build with nullable warnings treated as errors
            shell: pwsh
            run: |
              # Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
              # recompile. Enforcement now relies entirely on each file's own #nullable
              # enable pragma (the repo's per-file opt-in convention; UtilitiesCS.csproj and
              # SVGControl.csproj carry no project-level <Nullable> element) plus
              # /p:TreatWarningsAsErrors=true. MSBuild's incremental up-to-date check does
              # not invalidate on this command-line property change alone, so a plain
              # /t:Build would silently skip recompilation and never enforce this gate.
              & msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug `
                  "/p:Platform=Any CPU" `
                  /p:TreatWarningsAsErrors=true
              if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    ```
  - Only the `msbuild` command line changes (`/p:Nullable=enable` removed) and the explanatory
    comment is updated to describe the pragma-driven rationale. No other line — step name,
    `shell: pwsh`, exit-code handling — changes.
- Contracts and validation rules:
  - **Opted-in files are enforced.** `#nullable enable` sets a file's nullable annotation and
    warning context independent of the project default. CS86xx diagnostics are ordinary compiler
    warnings once a file's context is enabled; `/t:Rebuild` guarantees the file is actually
    recompiled, and `/p:TreatWarningsAsErrors=true` promotes any warning emitted for that
    compilation — including CS86xx — to a build error, exactly as it already does for
    `EnableNETAnalyzers`/`EnforceCodeStyleInBuild` diagnostics in the preceding "Build with
    analyzers" step. This requires no global flag: it already happens today with
    `/p:Nullable=enable` present, because the global flag is a strict superset (it also forces
    nullable on for oblivious files) — removing it removes only the superset behavior, not the
    per-file behavior.
  - **Non-opted-in files do not cross-block.** A file with no `#nullable` pragma and no
    project-level `<Nullable>` element compiles under the oblivious nullable context, in which the
    compiler does not evaluate nullable-flow rules and cannot emit CS86xx-series diagnostics for
    that file. `TreatWarningsAsErrors` has nothing in the CS86xx range to promote for such a file.
    This is independently confirmed by every already-prepared child's own gate-verification
    command (residuals #375 spec.md line 336; svgcontrol #368 spec.md line 263), all of which
    specify the identical `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`
    (no `/p:Nullable=enable`) command and state the same rationale — the twelve children's plans
    were authored against this exact gate-step contract, so this capstone edit finalizes the same
    gate all twelve siblings already assumed, not a new design.
  - `/t:Rebuild` (not `/t:Build`) remains required after the edit for the same reason it was
    required under PR #361: MSBuild's incremental up-to-date check does not invalidate on a
    changed `-p:` command-line property alone. This justification was never specific to the
    `Nullable` property — it is a general statement about MSBuild's up-to-date check — and it
    applies verbatim to `TreatWarningsAsErrors` after the edit.

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants: none. This feature changes one MSBuild command-line
  property in one CI workflow step; it introduces no data transformation, storage, or runtime
  state change.
- Caching or persistence details: none.
- Migration or backfill requirements (if any): none. No project-level `<Nullable>` element is
  added to `UtilitiesCS.csproj` or `SVGControl.csproj` by this feature's default execution (see
  Constraints & Risks and the Maintainer Decision Summary for the optional AC5 flip).

## Constraints & Risks

List notable constraints (performance, compatibility, scope) or risks.

- **Policy prohibits editing `.claude/rules/*`.** The `.claude/rules/csharp.md` conflict (below)
  is flagged for the maintainer, not resolved by editing rule text. No `.claude/rules/*` file is
  touched by this feature.
- **`TreatWarningsAsErrors=true` promotes all warnings, not only CS86xx.** This matches the
  existing PR #361 step behavior and is unchanged by this edit; dropping `/p:Nullable=enable`
  only changes the nullable-context source (per-file pragma vs. global force), not the
  warnings-as-errors scope.
- **The opted-in file set is a moving target.** As of this research pass, `#nullable enable`
  appears in only 25 files under `UtilitiesCS/` and 3 files under `SVGControl/` (28 total) — all
  pre-existing, organic opt-ins predating the epic, not epic output. Every prepared child
  (`docs/features/active/utilitiescs-nullable-*/`) has an unchecked plan and no execution
  evidence yet: the wave-0/wave-1 remediation itself has not been performed anywhere in this
  worktree. By the time this capstone's atomic plan actually executes (after the twelve children
  land), the opted-in count will be substantially larger. **The atomic plan and its execution
  must re-grep for `#nullable enable` immediately before the AC2 verification step and re-select
  a still-opted-in and a still-non-opted-in candidate at that time** — they must not assume the
  candidates named in this spec are still representative.
  - Illustrative-only candidates identified in research (subject to re-confirmation at execution
    time): `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` (opted-in; file-scope
    `#nullable enable`, 35-line static formatter, zero external consumers found) and
    `UtilitiesCS/Dialogs/ActionButton.cs` (non-opted-in; confirmed absent from the 25-file
    opted-in grep). `ActionButton.cs` is itself in-scope for the already-prepared `dialogs-misc`
    child's batch, so it may already be opted in by execution time and must not be used without
    re-confirming it is still non-opted-in.
- **The genuine-enforcement verification must fully revert.** Neither deliberately-introduced
  defect may remain on the branch; `git status` must be clean at hand-off and each of the four
  runs (clean baseline, opted-in-defect fail, non-opted-in-defect pass, restored-clean) is a
  separate evidence artifact, not a single combined record.
- **The optional project-level flip (AC5) is explicitly out of scope for default execution.**
  The epic's Non-Goals restrict a project-level `<Nullable>` flip to an optional, separately
  maintainer-gated step; this feature documents the option and its infeasibility today but does
  not perform it.
- **`modified-workflow-needs-green-run` applies at execution/merge time, not at planning time.**
  The proposed edit is a diff under `.github/workflows/**`, which is exactly the trigger path
  this feature-review policy rule matches (`.claude/skills/feature-review-workflow/SKILL.md`
  lines 68–75). It requires evidence of a green workflow run against the branch head in the
  remediation inputs before the change can merge — a PR-context run or a `workflow_dispatch` run
  whose head SHA matches the branch head is acceptable. This is not something research or
  planning can satisfy in advance; atomic execution, and later the epic-orchestrator's fan-in to
  `main`, must capture a green CI run against the capstone branch head before merge, consistent
  with how PR #361 itself was subject to the same rule.
- **`.claude/rules/ci-workflows.md`'s deliberately-failing-nested-command rule does not apply to
  the AC1 edit itself.** That rule governs a workflow step whose `run:` block intentionally
  invokes a failing nested command (e.g., a negative-path self-validation). The AC1 edit is a
  narrow, single-line removal from an already-compliant step; it adds no deliberately-failing
  nested command and does not change the step's existing exit-code handling — the step already
  ends with the compliant pattern `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` (line 115),
  preserved verbatim. The rule would apply to the AC2 verification's fail-before step only if that
  verification were ever wired into a workflow `run:` block rather than run locally as evidence
  capture; this feature runs the AC2 verification locally, as evidence capture, not as a new
  workflow step.
- **`.claude/rules/benchmark-baselines.md` does not apply.** This feature touches no
  `scripts/benchmarks/**` path and introduces no baseline artifact.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): one line removed
  (`/p:Nullable=enable`) and one explanatory comment updated in the `msbuild` invocation of
  `.github/workflows/ci.yml`'s "Build with nullable warnings treated as errors" step (lines
  103–115); these feature documents (`spec.md`, `user-story.md`); revertible
  genuine-enforcement verification evidence; and, per the "Scope reconciliation (2026-07-19)"
  section above, build-debt remediation of `SVGControl/SvgImageSelector.cs` (CS0649, narrow
  pragma suppression) and the UtilitiesCS nullable fan-in debt (62 files, annotation-only fixes).
  No csproj `<Nullable>` element and no `.claude/rules/*` file is edited by default.
- New classes/functions/commands to add or update: none. No new type, method, script, or command
  is added. The build-debt remediation edits existing method bodies with nullable annotations,
  null-forgiving operators, and guard clauses only; it does not add new public surface.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable to a CI workflow
  YAML edit. The change takes effect on the next workflow run against the branch; a green
  workflow run against the branch head is required before merge per
  `modified-workflow-needs-green-run` (see Constraints & Risks).

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

Acceptance criteria (from `issue.md`, mapped here for traceability):

- [x] AC1: The CI nullable-gate step no longer passes `/p:Nullable=enable` globally; it runs the
  gate under `/t:Rebuild /p:TreatWarningsAsErrors=true` and relies on each file's own `#nullable
  enable` pragma. Opted-in files are enforced; non-opted-in files do not cross-block.
- [x] AC2: A genuine-enforcement verification is defined and executed: a deliberately-introduced
  null defect in an opted-in file fails the gate, and the same class of defect in a non-opted-in
  file does not fail the gate. The introduced defect is reverted before completion; the
  verification is evidenced.
- [x] AC3: Any pwsh step or workflow YAML that is added or modified complies with
  `.claude/rules/ci-workflows.md` (deliberately-failing nested command exit-code handling) and,
  where applicable, `.claude/rules/benchmark-baselines.md`. No leaked `$LASTEXITCODE` on the
  success path.
- [x] AC4: The `.claude/rules/csharp.md` rules-vs-convention conflict (the rule documents forcing
  `/p:Nullable=enable` globally, which conflicts with the per-file opt-in convention) is surfaced
  as an explicit maintainer-decision item in `spec.md`. No `.claude/rules/*` file is edited.
- [x] AC5: The optional project-level `<Nullable>enable</Nullable>` capstone decision for
  `UtilitiesCS.csproj` and `SVGControl.csproj` is documented as a separately-gated OPTIONAL step
  with an explicit maintainer decision gate, not performed by default in this feature.
- [x] AC6: A single consolidated maintainer-decision summary is present in `spec.md`, folding in
  the epic-wide exclusions (`Interfaces/**` ~62 files where CS8618 cannot fire;
  `Properties/Resources.Designer.cs` and `Settings.Designer.cs` left null-oblivious) and the
  child flags: `PeopleScoDictionaryNewBackup.cs` (dead uncompiled duplicate, exclude/delete
  decision), 6 `OlFolderTools` Designer files left oblivious, three pre-existing >500-line files
  not split, and `MSDemoConv.cs` / `To Depricate/*` / `MailResolution_ToRemove` maintainer
  decisions.
- [x] AC7: No **behavior** change to production C# code and no reduction in coverage on changed
  lines. Clarification (per "Scope reconciliation (2026-07-19)" above): this feature's changes
  include CI workflow YAML, feature documents, and the build-debt remediation described in that
  section (a narrow CS0649 pragma suppression in `SVGControl/SvgImageSelector.cs` and
  nullable-annotation-only fixes across the 62-file UtilitiesCS fan-in set). "No behavior change"
  means these edits must not alter observable runtime behavior — they add/adjust nullable
  annotations, null-forgiving operators, and guard clauses, or bracket one already-dead,
  already-documented no-op field pair with a suppression — not that no `.cs` file is touched. A
  csproj `<Nullable>` element is out of scope for this feature's default execution regardless
  (only if the optional AC5 gate is later separately approved).

## Maintainer Decision Summary

This section consolidates the epic-wide maintainer-decision inventory (research section (e)) so
the maintainer does not need to read every child `spec.md` individually. Rows are reproduced
with their original source citations; this capstone does not resolve any of them — it collects
them (AC6).

| Item | Source (child + file) | Decision needed |
|---|---|---|
| `UtilitiesCS/Interfaces/**` (~62 `.cs`) | Epic manifest `epic.md` "Epic-wide exclusions" (lines 230–235); independently confirmed by `dialogs-misc` spec.md "Ownership Gaps" table (`Interfaces/**` row, ~62 files, "CS8618 cannot fire") | Formal epic-wide exclusion from all children (extends existing `Interfaces/IHelperClasses/` precedent) — recorded, not a live blocker. |
| `UtilitiesCS/Properties/Resources.Designer.cs` + `Settings.Designer.cs` (2 `.cs`) | Epic manifest `epic.md` lines 236–238; `dialogs-misc` spec.md "Ownership Gaps" table | Leave null-oblivious (no pragma); `AssemblyInfo.cs` already in `dialogs-misc` scope as verify-only. |
| `PeopleScoDictionaryNewBackup.cs` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 1 (lines 224–232); epic manifest "residuals (#375) execution-time findings" (lines 249–253) | Dead, uncompiled duplicate (CS0101 conflict with live `PeopleScoDictionaryNew.cs`); not in the csproj `<Compile Include>` set. Exclude from opt-in set or delete the file — maintainer choice. |
| 6 `OlFolderTools` Designer-generated files | `utilitiescs-nullable-residuals` spec.md lines 160–164, 210; epic manifest lines 249–253 | Left null-oblivious (no pragma), consistent with the epic-wide Designer-file exclusion; generated halves of WinForms partial classes. |
| Three pre-existing >500-line files in the residual set: `OutlookObjects/AppointmentItem/MeetingItemHelper.cs` (847 lines), `OutlookObjects/Recipient/RecipientStatic.cs` (773 lines), `OutlookObjects/Fields/UserDefinedFields.cs` (722 lines) | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 6 (lines 258–268); epic manifest "residuals (#375) execution-time findings" (lines 259–262) | Flagged, not split — same precedent as Wave-0 `threading` (#369) applying to `TimeOutTask.cs` (975 lines). Splitting is a refactor, out of scope for annotation-only remediation. |
| `Examples/MSDemoConv.cs` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 2 (lines 233–238); originally surfaced by `dialogs-misc` spec.md "Ownership Gaps" table (`Examples/MSDemoConv.cs` row) | Default: remediate annotation-only; alternatives (exclude via `[ExcludeFromCodeCoverage]`/pragma omission, or delete) surfaced for maintainer decision — demo/sample code, not production surface. |
| `To Depricate/FileIO2.cs` and `To Depricate/StringManipulation.cs` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 3 (lines 239–244); originally surfaced by `dialogs-misc` spec.md "Ownership Gaps" table (`To Depricate/*` row) | Real production helpers explicitly named for future deprecation. Annotation-only is feasible but may be wasted effort; maintainer chooses remediate vs. exclude vs. schedule deletion. Flagged; not deleted within `residuals`. |
| `OutlookObjects/MailResolution.cs` class `MailResolution_ToRemove` | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 4 (lines 245–248) | `_ToRemove` suffix signals a deletion candidate. Default: remediate in place (annotation-only is trivial); flag as a deletion candidate; do not delete within `residuals`. |
| `SvgImageSelector.ImagePath` dead-setter / `_relativeImagePath!` judgment call | `utilitiescs-nullable-svgcontrol` spec.md lines 58, 145–153 | The `set` accessor body is entirely commented out (functional no-op), so `_relativeImagePath` is never assigned on any live path, yet the `get` fallback dereferences it. Default: null-forgiving `_relativeImagePath!` with an in-code comment; described as "the single highest-consequence judgment call in the cluster" requiring explicit maintainer acceptance. |
| `SVGControl/RelativePath.cs` (1678 lines) | `utilitiescs-nullable-svgcontrol` spec.md lines 126, 161 | Already exceeds the repo's 500-line limit; is one of 3 already-clean "verify-only" files in this cluster (not newly remediated, but flagged as a pre-existing oversized file consistent with the same no-split precedent as the residuals files above). |
| `dialogs-misc` → `helperclasses` (#364) `depends_on` edge | `utilitiescs-nullable-dialogs-misc` spec.md lines 164–170, 259–260 (Constraints & Risks item 4) | Grep-unconfirmed by source (zero `HelperClasses/` type references under `Dialogs/`). Retained (harmless — both Wave-0 upstreams are prepared) and flagged, not dropped. |
| `residuals` → `reusabletypes` (#366) undeclared dependency edge | `utilitiescs-nullable-residuals` spec.md, Maintainer Decisions item 5 (lines 249–257); epic manifest "residuals (#375) execution-time findings" (lines 254–258) | Six in-scope files consume `#366` types (`TreeNode<T>`, `SmartSerializableLoader`, `ScoDictionaryNew<,>`) not declared in `depends_on`. Harmless for ordering (Wave 0 precedes Wave 1); flagged for epic-planner to add the edge or confirm annotated null-neutrality. |
| Rules-vs-convention conflict (`.claude/rules/csharp.md` forcing global `/p:Nullable=enable`) | Epic manifest lines 148–153; independently re-stated (not resolved) in `residuals` spec.md lines 340–344 and `svgcontrol` spec.md lines 270–277 | See below (AC4) — this capstone's own responsibility to consolidate and surface, not any other child's. |
| `PeopleScoDictionaryNew.cs` `#nullable disable`/`#nullable enable` island (retained) | This capstone's own P2-T14 execution-time finding, `evidence/remediation-baseline/debt2-people-island-decision.2026-07-20T00-52.md` | Island removal was tested and reverted (outcome (b)): removing it reintroduces 12 CS8644 interface-nullability-mismatch errors, because neither base type (`ScoDictionaryNew<,>` nor `ConcurrentObservableDictionary<,>`) carries a `#nullable enable` pragma despite `#366`'s merged `where TKey : notnull` constraint. A full resolution requires opting one or both base types into an enabled nullable context — shared `ReusableTypeClasses` infrastructure out of scope for this capstone's file-scoped remediation. Island retained as-is. |
| Analyzer-package-version-drift (16 first-party `.csproj` files) | This capstone's own session findings, this plan's Revision note item 2 (`docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/plan.2026-07-19T04-25.md`) | All 16 first-party `.csproj` files hardcode `<Analyzer Include>` paths to stale nuget package versions (Meziantou.Analyzer.3.0.101, SonarAnalyzer.CSharp.10.27.0.140913, Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4) that no longer match each project's `packages.config` (3.0.123 / 10.29.0.143774 / 5.6.0). Confirmed identically present on `origin/main`, predating this epic. CI stays green today only because its GitHub Actions cache's `restore-keys` prefix fallback happens to retain stale cached package directories from before the version bump. Flagged only, not fixed by this feature — fixing it would touch all 16 csproj files, a separate, unrelated maintenance task. |
| Scope expansion beyond the two originally-declared Phase 2 trees (`SVGControl/**`; `UtilitiesCS/EmailIntelligence/**` + `UtilitiesCS/OutlookObjects/Folder/**`) to 8 further first-party projects | This capstone's own P2-T17 through P2-T23 execution-time findings; root cause: commit `20d163ac` (the `/t:Rebuild` fix, PR #361) is not yet an ancestor of `origin/main` — only of the epic integration branch — so `main`'s currently-green CI has never run a genuine full-solution rebuild under `TreatWarningsAsErrors=true`; it previously silently no-op'd via MSBuild's incremental up-to-date check. These are pre-existing repo warnings never before reached by any CI run, not defects introduced by any of the 12 epic children (confirmed: `TaskController.Actions.cs` and `PeopleScoDictionaryNewTests.cs` are byte-identical to `origin/main`). | Orchestrator decision (recorded in this session's `artifacts/orchestration/orchestrator-state.json`): expand scope to remediate using only the three previously-established minimal, non-behavior-changing patterns (nullable annotation/null-forgiving/guard-clause; narrow pragma-bracket with rationale; dead-code deletion after grep-confirmed zero live references). Full file/diagnostic-code/pattern/rationale tuple list, by project: <br><br>**`ToDoModel.csproj`** (`evidence/remediation-baseline/debt2-layer1-todomodel-cs0618.2026-07-20T02-00.md`): `Data Model/ToDo/ToDoEvents.Filtering.cs` — CS0618 (`ForEachAwaitAsync`) — narrow pragma bracket — replacing with `await foreach` is a control-flow change, out of scope. <br><br>**`TaskVisualization.csproj`** (`evidence/remediation-baseline/debt2-layer2-taskvisualization-cs4014.2026-07-20T02-05.md`): `TaskController.Actions.cs` — CS4014 — narrow pragma bracket — adding `await` would change WinForms message-pump re-entrancy behavior, out of scope. <br><br>**`ToDoModel.Test.csproj`** (`evidence/remediation-baseline/debt2-layer2-todomodeltest-cs0169.2026-07-20T02-10.md`): `Data Model/People/PeopleScoDictionaryNewTests.cs` — CS0169 x3 (`mockApplication`, `_mockPrefix`, `_peopleScoDictionaryNew`) — dead-code deletion (grep-confirmed zero live references; all remaining references are inside already-commented-out test bodies). <br><br>**`QuickFiler.csproj`** (`evidence/remediation-baseline/debt2-layer3-remaining-projects-remediated.2026-07-20T02-45.md`): `Viewers/IItemViewer.cs` — CS0108 x4 (`InvokeRequired`/`Invoke`/`BeginInvoke`/`Height` hiding `ISynchronizeInvoke`/`IControl` members) — narrow pragma bracket — deliberate mockability re-declaration, adding `new`/restructuring is an API-shape change; `Controllers/QfcQueue.cs`, `Helper Classes/ConversationResolver.cs`, `Controllers/QfcItemController.ViewerSetup.cs` (x2), `Controllers/QfcDatamodel.cs`, `Controllers/QfcCollectionController.cs` (x3) — CS0618 x8 (`SelectAwait`/`SelectAwaitWithCancellation`/`ForEachAwaitAsync`/`ForEachAsync`) — narrow pragma bracket — call-shape/control-flow changes, out of scope; `Controllers/BreadcrumbBridgeRouter.cs` — CS8600 x2 — nullable annotation (`FolderTreeNodeKey` -> `FolderTreeNodeKey?`) — local is already null-checked immediately afterward. <br><br>**`TaskMaster.csproj`** (`evidence/remediation-baseline/debt2-layer3-remaining-projects-remediated.2026-07-20T03-05-iteration2.md`): `AppGlobals/EngineInitTimingProbe.cs`, `AppGlobals/ApplicationGlobals.cs`, `AppGlobals/NonBlockingDelay.cs` — CS8632 x4 — `#nullable enable annotations`/`restore annotations` narrow bracket around pre-existing `?` annotations; `AppGlobals/StoreRehookCoordinator.cs` — CS8767 x1 — same annotations-context bracket, parameter changed to match the `IOutlookReadinessGate.IsReady(Store?)` interface; `AppGlobals/AppItemEngines.cs`, `AppGlobals/AppEvents.cs` (x2), `Ribbon/RibbonController.Intelligence.cs` — CS0618 x4 — narrow pragma bracket. <br><br>**`QuickFiler.Test.csproj`** (same iteration-2 artifact): `Controllers/QfcFormControllerTests.cs` — MSTEST0032 x1 (tautological `Assert.IsTrue(true)` placeholder) — narrow pragma bracket — replacing the placeholder assertion is a test-behavior change, out of scope. <br><br>**`TaskMaster.Test.csproj`** (`evidence/remediation-baseline/debt2-layer3-remaining-projects-remediated.2026-07-20T03-40-iteration3.md`): 6 files (`ApplicationGlobalsStartupTimingTests.cs`, `AppToDoObjectsTests.cs`, `EngineInitTimingProbeTests.cs`, `StoreRehookCoordinatorTests.cs`, `TestableApplicationGlobals.cs`, `StoresWrapperTests.cs`) — CS8632 x13 — `#nullable enable annotations`/`restore annotations` narrow brackets. <br><br>**`UtilitiesCS.Test.csproj`** (same iteration-3 artifact): 10 files (`ManualFireTimerWrapper.cs`, `OlTableExtensions_Tests.cs`, `ConversationHelper_ExtendedTests.cs`, `ProgressTracker_Tests.cs`) — CS8632 x16 — same annotations-context bracket pattern; `EmailTokenizer_Tests.cs`, `SubjectMapEntry_Tests.cs`, `AsyncSerialization_Tests.cs` — CS8625 x3 — null-forgiving operator (`null` -> `null!`) at deliberate-null guard-clause/defensive-null-check test call sites; `StoreWrapperControllerTests.cs`, `SmartSerializable_Tests.cs`, `SmartSerializableBase_Tests.cs` — CS0067 x3 (`PropertyChanged` events required by `INotifyPropertyChanged`-derived interfaces, never raised in the test stub) — narrow pragma bracket — deletion not possible, the interface requires the event. <br><br>Final solution-wide rebuild reaches `EXIT_CODE: 0` (`evidence/remediation-baseline/debt-remediation-final-rebuild.2026-07-20T04-00.md`). The explicit stop condition (halt and escalate if any diagnostic cannot be resolved via the three patterns without a behavior change) was never triggered across any of the three loop iterations. |

### Rules-vs-convention conflict detail (AC4)

`.claude/rules/csharp.md` is not edited; the two conflicting citations are quoted verbatim below
for the flag:

- Toolchain section, item 3 (line 16):
  > 3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on
  > warnings. Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug
  > /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

- "Severity-first ordering invariant" section (lines 81–83):
  > All new analyzer rule severities are configured in `.editorconfig` at `severity =
  > suggestion` (never `warning`/`error`) BEFORE any `<Analyzer Include>` item is wired into a
  > project. This is required because the type-check toolchain step runs `msbuild ...
  > /p:Nullable=enable /p:TreatWarningsAsErrors=true`, which promotes any `warning`-severity
  > analyzer diagnostic to a build error. Keeping new analyzer diagnostics at `suggestion`
  > (message level) prevents the analyzer adoption from breaking the protected nullable gate.

Both citations document the toolchain's type-check step as forcing `/p:Nullable=enable`
globally (also present in the identically-worded root `CLAUDE.md` "C# Toolchain" section and the
`csharp-code-change-policy`/`C# Unit Test Policy` sections of `CLAUDE.md`). This conflicts with
the per-file opt-in convention the epic adopts and that this capstone's AC1 finalizes in the
actual CI workflow. Policy prohibits editing any `.claude/rules/*` file, so this is a flag, not a
resolution.

A prior instance of a policy conflict being surfaced to the maintainer rather than resolved by
editing rules exists at
`docs/features/archive/2026-07-06-appevents-loadasync-inbox-gating-243/runbooks/coverage-threshold-exception.runbook.md`
— a dedicated runbook artifact recording a coverage-threshold exception decision, referenced
from that feature's `remediation-plan.2026-07-06T12-29.md`, rather than any edit to the
coverage-floor rule text itself. `.claude/rules/general-unit-test.md`'s "COM/VSTO/WinForms
coverage exemption" section similarly documents that such exemptions require ratification "by
the project maintainer" and are "tracked" in a feature branch, not resolved by silently changing
the numeric floor.

Applying the same pattern here, the maintainer has two options, presented without a choice made
between them:

1. Accept the workflow's per-file-pragma gate as the authoritative implementation and treat
   `csharp.md`'s wording as documentation debt to be corrected in a future, maintainer-approved
   rules edit.
2. Explicitly ratify an exception analogous to the coverage-threshold runbook precedent above.

### Optional project-level flip detail (AC5)

An optional project-level `<Nullable>enable</Nullable>` flip for `UtilitiesCS.csproj` and
`SVGControl.csproj` is evaluated here as a **separately-gated OPTIONAL step requiring an
explicit maintainer decision**. It is **not performed by default in this feature.**

- **What it would add over per-file pragmas:** applies the enabled nullable context to every
  file compiled by the project by default, including any file a future contributor adds without
  remembering the pragma; removes the possibility of a file silently regressing to oblivious
  status by having its pragma accidentally deleted; is the more conventional/idiomatic .NET
  steady-state configuration (per-file pragma is normally a migration technique, not an end
  state).
- **Risk:** it would immediately surface CS86xx debt in every file not yet opted in at flip
  time, including generated/Designer files and any file legitimately excluded (Interfaces,
  Designer files) unless those are separately suppressed — no such suppression (per-project
  `<Nullable>disable</Nullable>` override, per-file `#nullable disable`, or `NoWarn`/exclusion
  glob) currently exists in either csproj. A flip performed before every file is genuinely clean
  would re-create the exact "silently-masked, then suddenly-blocking" failure mode PR #361 was
  written to fix, moved from the CI-flag layer to the project-config layer.
- **How it would be gated:** per the epic Non-Goals and AC5, only as a separate,
  maintainer-approved step, executed after every remaining in-scope file is opted in and clean,
  with its own dedicated verification pass (full solution `/t:Rebuild` with the flip in place; at
  that point per-file pragmas become redundant since the project defaults to enabled).
- **Current feasibility: not feasible today.** Only 25 of `UtilitiesCS/`'s ~485 `.cs` files and
  3 of `SVGControl/`'s `.cs` files currently carry `#nullable enable`. Even after all twelve
  children execute, exclusions remain by design:
  - `UtilitiesCS/Interfaces/**` (~62 files) — CS8618 cannot fire in interface-only files, but a
    project-level flip does not skip these files; they compile under the enabled context
    regardless of pragma and are expected to stay warning-free, but are unverified as a group
    under an enabled context.
  - `UtilitiesCS/Properties/Resources.Designer.cs` and `Settings.Designer.cs` — a project-level
    flip would force these generated files into an enabled context; generated Designer files are
    a known source of CS86xx noise (implicit non-nullable fields assigned by
    designer-generated `InitializeComponent()` patterns), the single largest concrete risk the
    flip introduces beyond what per-file pragmas ever exposed.
  - Six `OlFolderTools` Designer-generated files — same Designer-file risk, smaller scale.
  - `PeopleScoDictionaryNewBackup.cs` — dead, uncompiled duplicate; irrelevant to the flip since
    it is outside the csproj's `<Compile Include>` set regardless of `<Nullable>`.
  - Any file still pending a maintainer decision to remediate vs. exclude vs. delete
    (`MSDemoConv.cs`, `To Depricate/*`, `MailResolution_ToRemove`) is, by definition, not yet in
    a known-clean state, so the flip cannot safely happen until those decisions resolve.
  - **Conclusion:** the concrete blocking condition is "Designer-generated files
    (Resources/Settings + 6 OlFolderTools) and the Interfaces tree would enter an enabled
    nullable context under a project-level flip without ever having been individually verified
    clean," a materially different (and currently unverified) risk surface from the per-file
    pragma approach's exhaustively-tested surface. This feature's default execution does not
    perform the flip.

## Seeded Test Conditions (from potential)

- [ ] Pragma-driven gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug
  /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) passes on the
  integration branch head with all twelve children fanned in.
- [ ] Deliberate null defect in a currently-opted-in file (re-grep-confirmed at execution time)
  fails the gate (fail-before evidence).
- [ ] Deliberate null defect in a currently-non-opted-in file (re-grep-confirmed at execution
  time) does not fail the gate (non-cross-block evidence).
- [ ] The existing MSTest suite and coverage gate are unaffected by the gate-step change.
- [ ] Each of the four verification runs (clean baseline, opted-in-defect fail,
  non-opted-in-defect pass, restored-clean) is captured as a separate evidence artifact under
  `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/evidence/other/`, and `git status` is
  clean at hand-off.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order. Because this feature's only in-scope source change
is one workflow YAML line, most toolchain stages have nothing to act on; they are still run to
confirm no other file was inadvertently touched:

1. `csharpier .` — no `.cs` file is modified by this feature's default scope; run to confirm no
   incidental drift.
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — unaffected by a workflow-only
   change; run to confirm the toolchain baseline is unaffected.
3. Nullable verification via the finalized per-file pragma gate (this feature's actual subject):
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true` — no `/p:Nullable=enable`. Under `TreatWarningsAsErrors`, any
   CS86xx in a pragma-enabled file becomes a build error while non-opted-in files (Designer,
   generated, and any not-yet-remediated hand-authored files) stay silent. This is the gate this
   capstone finalizes.
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — unaffected by a workflow-only
   change; run to confirm no coverage regression, per AC7.

Do NOT pass `/p:Nullable=enable` globally in the finalized gate command (step 3). The global
flag forces nullable project-wide and, applied to a solution where not every file is yet
remediated, would surface the full pre-existing CS86xx debt across every not-yet-remediated file
instead of isolating the per-file opt-in signal — precisely the behavior this capstone removes
from the CI gate.
