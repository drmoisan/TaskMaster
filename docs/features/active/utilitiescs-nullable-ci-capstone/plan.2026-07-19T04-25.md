# Atomic Plan — utilitiescs-nullable-ci-capstone (Issue #376)

- Feature folder: `docs/features/active/utilitiescs-nullable-ci-capstone/`
- Work Mode: full-feature (AC source: `spec.md` + `user-story.md`, both present)
- Requirements source precedence: `issue.md` AC1–AC7, elaborated in `spec.md` and `user-story.md`
- Research: `docs/features/active/utilitiescs-nullable-ci-capstone/research/2026-07-19T00-30-ci-capstone-research.md`
- Scope: this plan authors and preflight-clears atomic execution steps; atomic execution itself is
  performed later by epic-orchestrator. No implementation is performed by this planning pass.
- Evidence root (canonical, non-overridable): `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/`

## Revision note (this plan revision)

This revision replaces the prior Phase 0 assumption that the pragma-driven nullable gate
(`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
/p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) is currently clean on this branch tip. It
is not. The research doc cited above was written against an earlier, pre-fan-in worktree tip
(`dd17719a`) where only 25+3 files were opted into `#nullable`; the actual current branch tip
(`bfcdb394`) has all 12 epic children's file remediation merged, producing real cross-child fan-in
debt that was never previously measured (`ci.yml` only triggers on PRs to `main`/`development`,
never on the integration branch — this is the first-ever measurement of the fanned-in tip).

Verified this session, on the `bfcdb394` tip, after `nuget restore TaskMaster.sln`:

1. **BUILD DEBT 1** (blocks the `/t:Rebuild` dependency chain before other projects are even
   reached): `SVGControl/SvgImageSelector.cs` lines 56–57 — fields `_relativeImagePath` and
   `_absoluteImagePath` (both `string?`) are CS0649 ("never assigned to, will always have its
   default value null"). Root cause: the `ImagePath` property's `set` accessor body (lines
   ~91–110) is entirely commented out (a pre-existing, deliberate dead no-op per #368's
   already-documented judgment call in
   `docs/features/active/utilitiescs-nullable-svgcontrol/evidence/other/imagepath-judgment-call-decision.md`),
   so both fields are never written anywhere in the class. This plan's Phase 1 fixes this with a
   narrow `#pragma warning disable CS0649` / `restore CS0649` bracket, not by resurrecting the
   dead setter logic (out of scope per AC7 — no behavior change).
2. **An unrelated, pre-existing build-environment issue (flagged only, not fixed)**: all 16
   first-party `.csproj` files hardcode `<Analyzer Include>` paths to stale nuget package versions
   (Meziantou.Analyzer.3.0.101, SonarAnalyzer.CSharp.10.27.0.140913,
   Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4) that no longer match each project's
   `packages.config` (3.0.123 / 10.29.0.143774 / 5.6.0). Confirmed identically present on
   `origin/main` (predates this epic, not introduced by any of the 12 children). A clean `nuget
   restore` + rebuild fails with CS0006 unless the old package versions are also present in the
   `packages/` folder (CI stays green today only because its GitHub Actions cache's
   `restore-keys` prefix fallback happens to retain stale cached package directories from before
   the version bump). This is recorded as a maintainer-flag task in Phase 5 (P5-T5); it is not
   fixed by this feature — fixing it would touch all 16 csproj files, a separate, unrelated
   maintenance task.
3. **BUILD DEBT 2** (the real fan-in debt): measured by excluding CS0649 from
   `TreatWarningsAsErrors` purely to see past BUILD DEBT 1 —
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
   /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649` — 296 CS86xx-range diagnostics
   (CS8604: 110, CS8602: 100, CS8601: 32, CS8625: 18, CS8603: 18, CS8619: 8, CS8620: 6, CS8600: 4)
   plus 28 CS0618 (obsolete-API usage) and 2 CS0168 (unused variable) — both also promoted to
   build errors by the same `/p:TreatWarningsAsErrors=true` flag even though they are not
   nullable-related — across 62 distinct already-`#nullable enable`-opted-in `.cs` files, all under
   `UtilitiesCS/EmailIntelligence/**` (subdirectories: Bayesian, ClassifierGroups,
   EmailParsingSorting, Evaluation, Flags, IntelligenceConfig, OlFolderTools, People, SubjectMap)
   and `UtilitiesCS/OutlookObjects/Folder/**`. This is the first measurement of this debt taken
   against the fully-fanned-in integration tip; no prior on-disk estimate exists to compare
   against (296+28+2 = 326 total measured this session). **This 62-file/326-diagnostic figure is a
   session snapshot, not hardcoded plan truth — Phase 2 requires a fresh re-grep + re-build at
   execution time before any remediation batch begins.**
4. **Item 7** (`#nullable disable`/`#nullable enable` island in
   `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNew.cs` lines 29/30/32, added because
   `#366`'s base type was then nullable-oblivious): since `#366` has merged (delivering `where
   TKey : notnull` plus nullable annotations on the ReusableTypeClasses dictionary bases), Phase 2
   includes a task to evaluate removing this island now that its original justification may no
   longer hold.

**Scope-expansion note (flag for escalation, not a unilateral decision by this planning pass):**
`spec.md`'s "Overview"/"Implementation Strategy" sections currently state this feature's default
execution is limited to a CI workflow YAML edit, feature documents, and revertible verification
evidence, explicitly excluding production `.cs` file edits. The build-debt remediation phases below
(Phase 1, Phase 2) require production `.cs` file edits (a CS0649 pragma bracket in
`SvgImageSelector.cs`, and nullable-annotation-only fixes across ~62 files) as a hard prerequisite
for the pragma-driven gate to pass at all — otherwise Phase 3's edited gate step is untestable
(AC1/AC2 verification cannot distinguish "gate correctly enforces per-file opt-in" from "gate
fails because of unrelated pre-existing fan-in debt"). This is a real conflict between `spec.md`'s
documented default-execution scope and what is now required to complete AC1/AC2 on the actual
branch tip. This plan proceeds with the remediation phases because they are a verified, necessary
precondition, but this conflict is called out explicitly in this planning pass's final report for
the orchestrator's decision — it is not resolved unilaterally here, and `spec.md`'s Overview/
Implementation Strategy text is not rewritten by any task in this plan.

## Scope reconciliation (2026-07-19)

`spec.md`'s original Overview/Implementation Strategy language (written before the 12 sibling
children merged) described this feature's default execution as limited to a CI workflow edit,
feature documents, and revertible verification evidence, excluding production `.cs` edits. Phase 1
(SVGControl CS0649) and Phase 2 (UtilitiesCS nullable fan-in) are production `.cs` edits that were
not knowable at spec-authoring time because the fan-in debt only exists once all 12 children are
fanned in. This expansion was authorized by this capstone's child orchestrator's own 2026-07-19
session decision, made directly at delegation time when the fan-in debt was first measured on the
fully-fanned-in integration tip (`bfcdb394`) — it is not a pre-existing line item that was already
recorded in `docs/features/epics/utilitiescs-nullable-remediation/epic.md` before this session.
Both build-debt items are this capstone's own responsibility, not any sibling child's: no
scope-locked child could remediate the fan-in debt under its own per-cluster lock (cross-child
annotation propagation, not any single child's own files), and the SVGControl CS0649 defect blocks
the gate this capstone exists to finalize. A durable record of this decision is added as a
"Capstone scope addendum (2026-07-19)" immediately below the Wave 2 table in `epic.md`, and
`spec.md`'s own "Scope reconciliation (2026-07-19)" section has been updated to match this
attribution. AC7's "no behavior change to production C# code" is unaffected — annotation-only and
narrow-pragma-only fixes are non-behavioral by construction.

## Revision note 2 (2026-07-20 — P2-T17 blocking-finding escalation)

During execution, Phase 2's original final gate task (`P2-T17`, the solution-wide `/t:Rebuild`
command) surfaced pre-existing warnings-as-errors debt in three projects entirely outside this
plan's originally-declared Phase 1/Phase 2 scope trees (`SVGControl/**` and
`UtilitiesCS/EmailIntelligence/**` + `UtilitiesCS/OutlookObjects/Folder/**`): `ToDoModel.csproj`
(CS0618, already resolved), `TaskVisualization.csproj` (CS4014), and `ToDoModel.Test.csproj`
(CS0169). Full detail is recorded in
`docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/p2t17-blocking-finding.2026-07-20T01-30.md`.
The executor correctly stopped and escalated rather than self-resolving CS4014/CS0169
unilaterally, since neither is a mechanical, zero-judgment fix under the three
previously-established patterns.

Root cause (orchestrator-confirmed): commit `20d163ac` (the `/t:Rebuild` fix, PR #361) is not yet
an ancestor of `origin/main` — only of the epic integration branch — so `main`'s currently-green
CI has never run a genuine full-solution rebuild under `TreatWarningsAsErrors=true`; it previously
silently no-op'd via MSBuild's incremental up-to-date check. `TaskController.Actions.cs` and
`PeopleScoDictionaryNewTests.cs` are confirmed byte-identical to `origin/main` (zero commits
touching them between `main` and this branch) — these are genuine pre-existing repo warnings
never before reached by any CI run, not defects introduced by any of the 12 epic children.

Decision (orchestrator, recorded in this session's `artifacts/orchestration/orchestrator-state.json`
under `delegation_receipts.orchestrator_decision_on_P2-T17_blocking_finding`): expand this plan's
scope to remediate CS4014/CS0169 — and any further-discovered layers across the remaining
16-project solution — using only the same three minimal, non-behavior-changing patterns already
established this session (nullable annotation/null-forgiving/guard-clause; narrow pragma-bracket
with rationale; dead-code deletion after grep-confirmed zero live references). P2-T17's original
acceptance requirement (`EXIT_CODE: 0` on the full `TaskMaster.sln` rebuild) is unchanged; only the
remediation steps needed to reach it were added. The rejected alternative — narrowing the gate to
the two originally-declared scope trees only — was rejected because it would not make the actual
CI step (unchanged, whole-solution `msbuild TaskMaster.sln`) genuinely pass on the real GitHub
Actions runner, and `issue.md`'s "Test Conditions to Consider" explicitly requires the
whole-solution pragma-driven gate to pass with all twelve children fanned in.

New tasks P2-T17 through P2-T23 (below) replace the original single P2-T17 gate task; P2-T1
through P2-T16 are unchanged. Every file touched outside the originally-declared scope trees is
recorded in P5-T7's Maintainer Decision Summary addition. This addendum satisfies the audit-trail
requirement without modifying Phase 0/1 or Phase 2's completed T1–T16 tasks.

## Notes for the executing agent

- The gate-step edit target is `.github/workflows/ci.yml`, step "Build with nullable warnings
  treated as errors" (lines 103–115 as of research date 2026-07-19; re-confirm line numbers before
  editing since intervening commits, including this plan's own Phase 1/Phase 2 remediation, may
  shift them — the remediation phases touch `.cs` files only, not `.github/workflows/ci.yml`, but
  line-number drift from any other intervening commit must still be re-confirmed).
- AC2's opted-in/non-opted-in candidate files MUST be re-selected at execution time via a fresh
  `#nullable enable` grep (Phase 4, P4-T1, run only after Phase 2's remediation batches complete) —
  the research's `PercentageFormatter.cs` / `ActionButton.cs` candidates are illustrative defaults
  only.
- No `.claude/rules/*` file is edited by any task in this plan (AC4). No csproj `<Nullable>`
  element is added by any task in this plan (AC5). No `.csproj` `<Analyzer Include>` path is
  edited by any task in this plan (unrelated pre-existing issue, flagged in Phase 5, P5-T5/P5-T6).
  All three are enforced as explicit verification tasks in Phase 5.
- `modified-workflow-needs-green-run` is recorded, not satisfied, by this plan (Phase 6) — the
  green CI run against the branch head is an execution/merge-time obligation for epic-orchestrator.
- Phase 1 and Phase 2 (build-debt remediation) MUST complete, and Phase 2's final full-solution
  rebuild (P2-T23) MUST show `EXIT_CODE: 0`, before Phase 3 (gate-step edit) or Phase 4
  (genuine-enforcement verification) begin. Phase 4's verification is meaningless against an
  unremediated tree because a failing "clean baseline" run (P4-T2) cannot be distinguished from a
  correctly-triggered deliberate defect.
- Do not perform a destructive `nuget` package cache clean as part of any task in this plan. If a
  fresh `nuget restore` (P0-T8) surfaces CS0006 due to the unrelated analyzer-package-version-drift
  issue (see Revision note item 2 and Phase 5, P5-T5), record the occurrence and continue — do not
  attempt to fix the csproj `<Analyzer Include>` paths.

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read `docs/features/active/utilitiescs-nullable-ci-capstone/issue.md` in full and confirm AC1–AC7 are present and unchanged from the versions quoted in this plan.
  - Acceptance: read completed; AC1–AC7 text confirmed present; no discrepancy noted (or discrepancy recorded and plan halted for reconciliation).
- [x] [P0-T2] Read `docs/features/active/utilitiescs-nullable-ci-capstone/spec.md` in full, including the "Maintainer Decision Summary", "Rules-vs-convention conflict detail (AC4)", and "Optional project-level flip detail (AC5)" sections.
  - Acceptance: read completed; all three named sections confirmed present.
- [x] [P0-T3] Read `docs/features/active/utilitiescs-nullable-ci-capstone/user-story.md` in full.
  - Acceptance: read completed; Acceptance Criteria and Non-Goals sections confirmed present.
- [x] [P0-T4] Read `docs/features/active/utilitiescs-nullable-ci-capstone/research/2026-07-19T00-30-ci-capstone-research.md` in full, including sections (a) gate edit, (b) verification method/candidates, (c) rules conflict, (d) optional flip, (e) maintainer-decision inventory, (f) CI-workflow rule applicability, and note explicitly that section 0's "children are PREPARED, not yet EXECUTED" framing is superseded by this plan's Revision note (the children's file remediation IS now merged on the actual branch tip).
  - Acceptance: read completed; all six lettered sections confirmed present; the supersession note is explicitly acknowledged in the reading task's own completion (no separate artifact required for this acknowledgment).
- [x] [P0-T5] Read policy files in compliance order — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/ci-workflows.md`, `.claude/rules/benchmark-baselines.md` — and write `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/phase0-instructions-read.md`.
  - Acceptance: artifact exists with `Timestamp:`, `Policy Order:` (the six files listed in the order read), and an explicit list of files read. Task remains unchecked if the artifact is absent or any field is missing.
- [x] [P0-T6] Run `dotnet csharpier check .` from the repository root and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-csharpier.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of unformatted files, expected zero since no `.cs` file is touched by this task).
- [x] [P0-T7] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-analyzers-build.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (exact command above), `EXIT_CODE:`, `Output Summary:` stating the analyzer/code-style error count on the unmodified branch head (record the actual value observed; do not assume zero).
- [x] [P0-T8] Run `nuget restore TaskMaster.sln` from the repository root and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-nuget-restore.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If this restore surfaces CS0006 on a subsequent build due to the pre-existing csproj `<Analyzer Include>` version drift (Revision note item 2; flagged in Phase 5, P5-T5), record the occurrence in `Output Summary:` and do not attempt to fix the csproj Analyzer Include paths — that is out of scope for this feature.
- [x] [P0-T9] Run the pragma-driven nullable gate `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) against the current branch tip and record the actual, observed result — not an assumed clean result — to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-nullable-gate.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (exact command above), the actual `EXIT_CODE:` observed (a non-zero value is the expected, correct outcome for this task — this is the fail-before/red baseline that Phase 1 and Phase 2 remediate, not a passing gate), and `Output Summary:` recording the diagnostic-code breakdown (at minimum: CS0649 count in `SVGControl`, and CS8604/CS8602/CS8601/CS8625/CS8603/CS8619/CS8620/CS8600/CS0618/CS0168 counts) plus the count of distinct affected first-party `.cs` files. Include the sentence: "This baseline was revised from an earlier plan draft that assumed EXIT_CODE 0 based on research written against an earlier pre-fan-in worktree tip (dd17719a); the actual tip (bfcdb394) has all 12 children's file-scope remediation merged, producing real cross-child fan-in debt (BUILD DEBT 1: SVGControl/SvgImageSelector.cs CS0649; BUILD DEBT 2: ~296+28+2 diagnostics across UtilitiesCS/EmailIntelligence/** and UtilitiesCS/OutlookObjects/Folder/**) that was never previously measured, because ci.yml only triggers on PRs to main/development, never the integration branch." This artifact is the fail-before evidence baseline referenced by Phase 1, Phase 2, and Phase 7's coverage-delta comparison.
- [x] [P0-T10] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (or `vstest.console.exe <resolved UtilitiesCS.Test.dll and SVGControl.Test.dll paths> /EnableCodeCoverage`) and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/baseline/baseline-tests-coverage.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric passed/failed test counts and the numeric coverage headline (line-rate and branch-rate from the emitted coverage report). This is the pre-edit, pre-remediation coverage reference for the AC7 no-regression comparison in Phase 7.

### Phase 1 — Build-Debt Remediation: SVGControl CS0649 (BUILD DEBT 1)

- [x] [P1-T1] Read `docs/features/active/utilitiescs-nullable-svgcontrol/evidence/other/imagepath-judgment-call-decision.md` in full and confirm it documents the `ImagePath` setter dead-code judgment call referenced in this plan's Revision note item 1.
  - Acceptance: read completed; the document's judgment-call rationale (the `set` accessor body is a deliberate, already-accepted dead no-op) is confirmed present.
- [x] [P1-T2] Edit `SVGControl/SvgImageSelector.cs` lines 56–57 (the `_relativeImagePath` and `_absoluteImagePath` field declarations) to bracket them with a narrow `#pragma warning disable CS0649` / `#pragma warning restore CS0649` pair, with an inline comment citing `docs/features/active/utilitiescs-nullable-svgcontrol/evidence/other/imagepath-judgment-call-decision.md` as the rationale, without modifying the commented-out `ImagePath` setter body (~lines 91–110) or any other line in the file.
  - Acceptance: `git diff SVGControl/SvgImageSelector.cs` shows only the pragma bracket and its inline comment added around lines 56–57; the commented-out setter body and every other line in the file are byte-identical to the pre-edit state.
- [x] [P1-T3] Rebuild `SVGControl/SVGControl.csproj` in isolation via `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt1-svgcontrol-rebuild.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming CS0649 no longer appears for `SvgImageSelector.cs` and no new diagnostic is introduced by the pragma edit.
- [x] [P1-T4] Record the before/after CS0649 diagnostic count for `SVGControl/SvgImageSelector.cs` to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt1-diagnostic-count.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, a before-count of 2 (CS0649 x2, from P0-T9's baseline), an after-count of 0 (from P1-T3), and an explicit statement that no other diagnostic code's count changed for this file as a result of this edit.

### Phase 2 — Build-Debt Remediation: UtilitiesCS Nullable Fan-In (BUILD DEBT 2) and Cross-Project Warnings-as-Errors Debt (Layers 1–3)

- [x] [P2-T1] Re-grep `#nullable enable` (and bare `#nullable`) across `UtilitiesCS/EmailIntelligence/**/*.cs` and `UtilitiesCS/OutlookObjects/Folder/**/*.cs` at current execution time (after Phase 1 completes), rebuild the solution via `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, and record the full current file/diagnostic-code list to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-fanin-full-regrep.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with the current opted-in file count and per-diagnostic-code counts (CS8604, CS8602, CS8601, CS8625, CS8603, CS8619, CS8620, CS8600, CS0618, CS0168), plus an explicit statement that this session's 62-file/296+28+2 figure from the plan's Revision note is a snapshot and this artifact is the authoritative current measurement superseding it.
- [x] [P2-T2] Re-grep and rebuild-diagnostic-scan the current file list under `UtilitiesCS/EmailIntelligence/Bayesian/**/*.cs` (including any `Performance` subfolder confirmed present under this tree at re-grep time) to establish this batch's authoritative current diagnostic list, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-bayesian-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list for this batch, and per-diagnostic-code counts for those files, filtered from P2-T1's full output.
- [x] [P2-T3] Apply minimal, behavior-preserving nullable-annotation-only fixes (nullable parameter/return annotations, null-forgiving `!` only where behavior must be preserved exactly as today, guard clauses only where genuinely bug-safe) to the CS86xx-range diagnostics identified in P2-T2 for files under `UtilitiesCS/EmailIntelligence/Bayesian/**`, following the same remediation conventions already used by this epic's 12 sibling children (annotation-only, no behavior change, per AC7); opportunistically fix any CS0618/CS0168 diagnostics in the same files only if trivial (documented obsolete-API replacement or removal of a genuinely-unused variable); rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record before/after diagnostic counts to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-bayesian-remediated.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, before-count and after-count per diagnostic code for this batch's files, confirms the after-count is zero for every CS86xx/CS0618/CS0168 code in this batch's file set, and confirms `git diff` for these files shows only annotation/null-forgiving/guard-clause-level changes (no removed or altered method signatures beyond nullable annotations, no altered control flow beyond added guard clauses).
- [x] [P2-T4] Re-grep and rebuild-diagnostic-scan the current file list under `UtilitiesCS/EmailIntelligence/ClassifierGroups/**/*.cs` to establish this batch's authoritative current diagnostic list, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-classifiergroups-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list for this batch, and per-diagnostic-code counts for those files, filtered from P2-T1's full output.
- [x] [P2-T5] Apply minimal, behavior-preserving nullable-annotation-only fixes to the CS86xx-range diagnostics identified in P2-T4 for files under `UtilitiesCS/EmailIntelligence/ClassifierGroups/**`, following the same conventions as P2-T3; opportunistically fix any CS0618/CS0168 diagnostics in the same files only if trivial; rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via the P2-T3 rebuild command and record before/after diagnostic counts to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-classifiergroups-remediated.<timestamp>.md`.
  - Acceptance: same acceptance shape as P2-T3, scoped to this batch's files.
- [x] [P2-T6] Re-grep and rebuild-diagnostic-scan the current file list under `UtilitiesCS/EmailIntelligence/EmailParsingSorting/**/*.cs` to establish this batch's authoritative current diagnostic list, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-emailparsingsorting-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list for this batch, and per-diagnostic-code counts for those files, filtered from P2-T1's full output.
- [x] [P2-T7] Apply minimal, behavior-preserving nullable-annotation-only fixes to the CS86xx-range diagnostics identified in P2-T6 for files under `UtilitiesCS/EmailIntelligence/EmailParsingSorting/**`, following the same conventions as P2-T3; opportunistically fix any CS0618/CS0168 diagnostics in the same files only if trivial; rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via the P2-T3 rebuild command and record before/after diagnostic counts to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-emailparsingsorting-remediated.<timestamp>.md`.
  - Acceptance: same acceptance shape as P2-T3, scoped to this batch's files.
- [x] [P2-T8] Re-grep and rebuild-diagnostic-scan the current file list under `UtilitiesCS/EmailIntelligence/Evaluation/**/*.cs`, `UtilitiesCS/EmailIntelligence/Flags/**/*.cs`, `UtilitiesCS/EmailIntelligence/IntelligenceConfig/**/*.cs`, `UtilitiesCS/EmailIntelligence/SubjectMap/**/*.cs`, and `UtilitiesCS/EmailIntelligence/Extensions/**/*.cs` to establish this combined batch's authoritative current diagnostic list, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-evalflagsconfigmap-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list for this combined batch (broken out per subdirectory), and per-diagnostic-code counts for those files, filtered from P2-T1's full output.
- [x] [P2-T9] Apply minimal, behavior-preserving nullable-annotation-only fixes to the CS86xx-range diagnostics identified in P2-T8 for files under the five subdirectories listed there, following the same conventions as P2-T3; opportunistically fix any CS0618/CS0168 diagnostics in the same files only if trivial; rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via the P2-T3 rebuild command and record before/after diagnostic counts to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-evalflagsconfigmap-remediated.<timestamp>.md`.
  - Acceptance: same acceptance shape as P2-T3, scoped to this batch's files.
- [x] [P2-T10] Re-grep and rebuild-diagnostic-scan the current file list under `UtilitiesCS/EmailIntelligence/OlFolderTools/**/*.cs` (excluding files already documented as intentionally null-oblivious Designer-generated halves per the Maintainer Decision Summary) to establish this batch's authoritative current diagnostic list, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-olfoldertools-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list for this batch (with the excluded Designer-file set named explicitly), and per-diagnostic-code counts for the non-excluded files, filtered from P2-T1's full output.
- [x] [P2-T11] Apply minimal, behavior-preserving nullable-annotation-only fixes to the CS86xx-range diagnostics identified in P2-T10 for the non-excluded files under `UtilitiesCS/EmailIntelligence/OlFolderTools/**`, following the same conventions as P2-T3; opportunistically fix any CS0618/CS0168 diagnostics in the same files only if trivial; rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via the P2-T3 rebuild command and record before/after diagnostic counts to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-olfoldertools-remediated.<timestamp>.md`.
  - Acceptance: same acceptance shape as P2-T3, scoped to this batch's non-excluded files; confirms the 6 Designer-generated files remain untouched (`git diff` empty for those 6 files).
- [x] [P2-T12] Re-grep and rebuild-diagnostic-scan the current file list under `UtilitiesCS/EmailIntelligence/People/**/*.cs` to establish this batch's authoritative current diagnostic list, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-people-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list for this batch, and per-diagnostic-code counts for those files, filtered from P2-T1's full output.
- [x] [P2-T13] Apply minimal, behavior-preserving nullable-annotation-only fixes to the CS86xx-range diagnostics identified in P2-T12 for files under `UtilitiesCS/EmailIntelligence/People/**` (excluding the `PeopleScoDictionaryNew.cs` island decision, handled separately in P2-T14), following the same conventions as P2-T3; opportunistically fix any CS0618/CS0168 diagnostics in the same files only if trivial; rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via the P2-T3 rebuild command and record before/after diagnostic counts to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-people-remediated.<timestamp>.md`.
  - Acceptance: same acceptance shape as P2-T3, scoped to this batch's files excluding `PeopleScoDictionaryNew.cs`'s island lines (29, 30, 32).
- [x] [P2-T14] For `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNew.cs`, remove the `#nullable disable` (line 29) / `#nullable enable` (line 32) island wrapping the class declaration line (line 30), rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via the P2-T3 rebuild command, and check whether CS8644 (interface-nullability mismatch) or any other new diagnostic reappears on the class declaration line.
  - Acceptance: exactly one of the following two outcomes is recorded to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-people-island-decision.<timestamp>.md`, and the outcome field is not blank: (a) no CS8644 or other new diagnostic appears — the island removal is kept (file becomes uniform `#nullable enable`), the rebuild's `EXIT_CODE: 0` is recorded, and `git diff` confirms only lines 29/32 were removed; or (b) CS8644 or another diagnostic reappears — the island is reverted (lines 29 and 32 re-inserted, `git diff` for the file confirms it is byte-identical to its pre-task state), and the retained rationale is recorded in this artifact with an explicit note that `#366`'s fix was insufficient for this interface-implementation shape, cross-referenced for inclusion in Phase 5's maintainer decision summary (P5-T3, AC6).
- [x] [P2-T15] Re-grep and rebuild-diagnostic-scan the current file list under `UtilitiesCS/OutlookObjects/Folder/**/*.cs` to establish this batch's authoritative current diagnostic list, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-outlookobjectsfolder-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list for this batch, and per-diagnostic-code counts for those files, filtered from P2-T1's full output.
- [x] [P2-T16] Apply minimal, behavior-preserving nullable-annotation-only fixes to the CS86xx-range diagnostics identified in P2-T15 for files under `UtilitiesCS/OutlookObjects/Folder/**`, following the same conventions as P2-T3; opportunistically fix any CS0618/CS0168 diagnostics in the same files only if trivial; rebuild `UtilitiesCS/UtilitiesCS.csproj` in isolation via the P2-T3 rebuild command and record before/after diagnostic counts to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-batch-outlookobjectsfolder-remediated.<timestamp>.md`.
  - Acceptance: same acceptance shape as P2-T3, scoped to this batch's files.
- [x] [P2-T17] Confirm and record the already-applied `ToDoModel.csproj` CS0618 remediation (Layer 1, applied this session during the P2-T17-blocking-finding investigation) for `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs` line 85 (the `AsyncEnumerable.ForEachAwaitAsync` obsolete-API call), bracketed with a narrow `#pragma warning disable CS0618` / `#pragma warning restore CS0618` pair, and write the evidence artifact `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-layer1-todomodel-cs0618.<timestamp>.md`.
  - Acceptance: `git diff "ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs"` shows only the pragma bracket present around line 85; rebuild `ToDoModel/ToDoModel.csproj` in isolation via `msbuild ToDoModel/ToDoModel.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` reaches `EXIT_CODE: 0`; artifact records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming CS0618 no longer appears for this file, the pattern applied (narrow pragma bracket), and a one-line rationale.
- [x] [P2-T18] Apply the CS4014 pragma-bracket fix to `TaskVisualization/TaskController.Actions.cs` line 203 (the `_viewer.Invoke((System.Action)(() => OK_Action()));` call inside the `if (_viewer.InvokeRequired) { ...; return; }` block), bracketing only that line with `#pragma warning disable CS4014` / `#pragma warning restore CS4014` and an inline comment stating this preserves pre-existing fire-and-forget UI-thread-marshal behavior unchanged (no `await` added, per AC7's no-behavior-change constraint), and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-layer2-taskvisualization-cs4014.<timestamp>.md`.
  - Acceptance: `git diff TaskVisualization/TaskController.Actions.cs` shows only the pragma bracket and inline comment added around line 203; no other line in the file differs from its pre-edit state; rebuild `TaskVisualization/TaskVisualization.csproj` in isolation via `msbuild TaskVisualization/TaskVisualization.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` reaches `EXIT_CODE: 0`; artifact records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming CS4014 no longer appears for this file, the pattern applied (narrow pragma bracket), and the one-line rationale.
- [x] [P2-T19] Grep-confirm zero live (non-commented) references to the three unused private fields `mockApplication`, `_mockPrefix`, and `_peopleScoDictionaryNew` in `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` lines 22–24, then delete the three field declarations, leaving the surrounding commented-out test method bodies untouched, and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-layer2-todomodeltest-cs0169.<timestamp>.md`.
  - Acceptance: the grep output recorded in the artifact shows zero live references to any of the three field names outside comments; `git diff "ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs"` shows only lines 22–24 removed, with every commented-out test body byte-identical to its pre-edit state; rebuild `ToDoModel.Test/ToDoModel.Test.csproj` in isolation via `msbuild "ToDoModel.Test/ToDoModel.Test.csproj" /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` reaches `EXIT_CODE: 0`; artifact records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming CS0169 no longer appears for this file, the pattern applied (dead-code deletion), and the one-line rationale (zero live references; deletion changes no observable behavior).
- [x] [P2-T20] Re-run the full solution-wide rebuild gate `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` after P2-T17 through P2-T19's Layer 1/Layer 2 fixes, and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-layer2-post-fix-rebuild.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` stating either (a) `EXIT_CODE: 0` — the solution-wide rebuild is clean, proceed directly to P2-T23; or (b) a non-zero `EXIT_CODE:` with the newly-surfaced project(s) and diagnostic code(s) listed — proceed to P2-T21.
- [x] [P2-T21] Only if P2-T20 recorded outcome (b): re-grep and rebuild-diagnostic-scan the current file list under the remaining not-yet-reached first-party projects (`TaskMaster.csproj`, `QuickFiler.csproj`, `Tags.csproj`, `VBFunctions.csproj`, `TaskTree.csproj`, and their `.Test` counterparts, plus `TaskVisualization.Test.csproj` and `UtilitiesCS.Test.csproj` — neither has received an isolated-project rebuild in Phase 1 or Phase 2, since P2-T18 and the P2-T3/5/7/9/11/13/16 batches rebuilt only their respective main projects) named in P2-T20's output, to establish the authoritative current diagnostic list for this layer, and record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-layer3-remaining-projects-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, the current file list and per-diagnostic-code counts for the specific project(s) P2-T20 reported as newly-surfaced. If P2-T20 recorded outcome (a) (`EXIT_CODE: 0`), this task is marked not-required and left unchecked with an explicit "not required — P2-T20 already reached EXIT_CODE: 0" note rather than executed.
- [x] [P2-T22] Only if P2-T21 produced findings: apply remediation to every diagnostic identified in P2-T21 using only the three authorized patterns — (1) nullable annotation / null-forgiving `!` / guard clause, (2) narrow `#pragma warning disable <CODE>` / `#pragma warning restore <CODE>` bracket with an in-code rationale comment, (3) deletion of compiler-confirmed-dead code after grep-confirming zero live references — rebuild each affected project in isolation, and record before/after diagnostic counts per project and per pattern applied to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt2-layer3-remaining-projects-remediated.<timestamp>.md`.
  - Acceptance: for every diagnostic resolvable via one of the three patterns without a behavior change, the artifact records the file, diagnostic code, pattern applied, and one-line rationale, and the affected project's isolated rebuild reaches `EXIT_CODE: 0`. Explicit stop condition (mandatory): if any diagnostic cannot be resolved via one of these three patterns without an actual behavior change, this task halts, applies no change for that diagnostic, and instead records an escalation artifact at `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/p2-t22-blocking-finding.<timestamp>.md` describing the diagnostic and why none of the three patterns apply without a behavior change; this task remains unchecked pending a further orchestrator decision and is not self-resolved past that point. If P2-T21 was marked not-required, this task is likewise marked not-required and left unchecked.
- [x] [P2-T23] Run the full pragma nullable gate `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (no exclusions, no `/p:Nullable=enable`) across the entire solution after Phase 1, all Phase 2 batches (P2-T2 through P2-T16), and Phase 2's Layer 1/Layer 2/Layer 3 remediation (P2-T17 through P2-T22), and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/remediation-baseline/debt-remediation-final-rebuild.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming zero build errors solution-wide across all 16 first-party projects. If `EXIT_CODE:` is non-zero and every newly-surfaced diagnostic is resolvable via one of the three authorized patterns without a behavior change, repeat the P2-T21/P2-T22/P2-T23 loop (re-grep the newly-surfaced project(s), remediate, re-run this gate) until `EXIT_CODE: 0` is reached, recording each additional iteration's artifacts under the same `debt2-layer3-*` / `debt-remediation-final-rebuild` naming convention with a distinguishing timestamp. If any newly-surfaced diagnostic cannot be resolved via one of the three patterns without a behavior change, halt per P2-T22's explicit stop condition instead of looping further. This is the green checkpoint that Phase 3 (gate-step edit, AC1) and Phase 4 (genuine-enforcement verification, AC2) build upon; Phase 4 MUST NOT begin until this task's `EXIT_CODE: 0` is recorded.

### Phase 3 — Gate-Step Edit (AC1)

- [x] [P3-T1] Edit `.github/workflows/ci.yml`, step "Build with nullable warnings treated as errors", removing `/p:Nullable=enable` from the `msbuild` invocation and replacing the explanatory comment with the pragma-driven rationale, producing exactly:
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
  - Acceptance: `git diff .github/workflows/ci.yml` shows only this step's `run:` block changed (comment text and the one `msbuild` line with `/p:Nullable=enable` removed); no other line in the file differs from the pre-edit state.
- [x] [P3-T2] Confirm the exit-code handling line `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` is present verbatim (unchanged) in the edited step (AC3).
  - Acceptance: grep of the edited step shows the exact line unchanged from the pre-edit version; no new deliberately-failing nested command is introduced in this `run:` block, so `.claude/rules/ci-workflows.md`'s reset/exit-0 requirement is not triggered by this edit.
- [x] [P3-T3] Confirm no other line in the step — step name, `shell: pwsh` directive, or any line outside the `run:` block's comment and `msbuild` invocation — differs from the pre-edit state.
  - Acceptance: line-by-line diff review confirms only the comment block and the single `msbuild` command line changed; step name and `shell: pwsh` are byte-identical to the pre-edit version.

### Phase 4 — Genuine-Enforcement Verification (AC2)

- [x] [P4-T1] Re-grep `#nullable enable` (and bare `#nullable`) across `UtilitiesCS/**/*.cs` and `SVGControl/**/*.cs` at current execution time (after Phase 2's remediation batches complete), and select one still-opted-in candidate file and one still-non-opted-in candidate file, re-confirming (not assuming) `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` and `UtilitiesCS/Dialogs/ActionButton.cs` are still representative or substituting alternates if either has changed opt-in status. Record the full current opted-in file count and both selected candidates to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/nullable-opt-in-regrep.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, the full re-grep file count and file list, and the two selected candidate file paths with an explicit statement of each candidate's opt-in status as of this grep.
- [x] [P4-T2] Run the pragma nullable gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) with no defect present and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-1-clean-baseline.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming a clean rebuild immediately before defect introduction (this must be `EXIT_CODE: 0` because Phase 1/Phase 2's remediation and P2-T23's final rebuild have already cleared the fan-in debt; if this run is not `EXIT_CODE: 0`, Phase 2 is incomplete and this task must halt and report back rather than proceed).
- [x] [P4-T3] [expect-fail] Introduce a deliberate CS8602-class null-dereference defect in the selected opted-in candidate file (e.g. `string? maybeNull = null; int len = maybeNull.Length;` inside a method body), run the pragma nullable gate, and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-2-optedin-defect-fail.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` including the literal CS86xx diagnostic line from the MSBuild error output, confirming the gate fails on the opted-in file's defect.
- [x] [P4-T4] Revert the opted-in-candidate defect introduced in P4-T3 and confirm `git diff <opted-in candidate file path>` is empty.
  - Acceptance: `git diff` for the specific file returns no output; the file is byte-identical to its pre-defect state.
- [x] [P4-T5] Introduce the same defect class (a null-literal assignment dereferenced through a non-nullable local, e.g. `string local = null; Console.WriteLine(local.Length);`) in the selected non-opted-in candidate file, run the pragma nullable gate, and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-3-nonoptedin-defect-pass.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming the gate does not fail and no CS86xx diagnostic is emitted for the non-opted-in file (non-cross-block evidence).
- [x] [P4-T6] Revert the non-opted-in-candidate defect introduced in P4-T5 and confirm `git diff <non-opted-in candidate file path>` is empty.
  - Acceptance: `git diff` for the specific file returns no output; the file is byte-identical to its pre-defect state.
- [x] [P4-T7] Run the pragma nullable gate once more on the fully-reverted tree and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/verification-4-restored-clean.<timestamp>.md`, then confirm `git status --porcelain` shows only the Phase 1/Phase 2 remediation and Phase 3 gate-edit changes (no residual defect).
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming a clean restored-state rebuild; `git status --porcelain` output shows only the intentional, already-committed-or-staged Phase 1/Phase 2/Phase 3 changes, confirming no defect or failing-gate state remains on the branch (AC2 completion requirement).

### Phase 5 — Documentation Verification (AC4, AC5, AC6) and Maintainer Flags

- [x] [P5-T1] Verify AC4: confirm `docs/features/active/utilitiescs-nullable-ci-capstone/spec.md`'s "Rules-vs-convention conflict detail (AC4)" section quotes `.claude/rules/csharp.md` lines 16 and 81–83 verbatim and presents both maintainer options (accept-as-documentation-debt vs. explicit ratified exception) without choosing between them.
  - Acceptance: section confirmed present with both verbatim quotations and both options listed; no resolution/choice is asserted in the section text.
- [x] [P5-T2] Verify AC5: confirm `spec.md`'s "Optional project-level flip detail (AC5)" section documents the project-level `<Nullable>enable</Nullable>` flip for `UtilitiesCS.csproj` and `SVGControl.csproj` as a separately-gated OPTIONAL step with an explicit maintainer decision gate, and confirm neither csproj currently contains a `<Nullable>` element (grep both files for `Nullable`, expect zero matches).
  - Acceptance: section confirmed present describing the flip as optional/not-performed-by-default; grep of both `UtilitiesCS/UtilitiesCS.csproj` and `SVGControl/SVGControl.csproj` returns zero `<Nullable>` matches.
- [x] [P5-T3] Verify AC6: confirm `spec.md`'s "Maintainer Decision Summary" table includes all required rows — `Interfaces/**` (~62 files) exclusion, `Resources.Designer.cs`/`Settings.Designer.cs` exclusion, `PeopleScoDictionaryNewBackup.cs` exclude/delete decision, 6 `OlFolderTools` Designer files, the three pre-existing >500-line files, `MSDemoConv.cs`, `To Depricate/*`, `MailResolution_ToRemove`, the analyzer-package-version-drift flag added in P5-T5, and — if P2-T14 recorded outcome (b) — the retained `PeopleScoDictionaryNew.cs` island rationale.
  - Acceptance: all named rows (or their equivalent single-consolidated-list entries) are confirmed present in the table with their source citations, including the new P5-T5 row and, if applicable, the P2-T14 outcome-(b) row.
- [x] [P5-T4] Confirm no `.claude/rules/*` file appears in `git diff --name-only origin/epic/utilitiescs-nullable-remediation-integration...HEAD` for this feature branch.
  - Acceptance: the diff name-only listing contains zero paths under `.claude/rules/`, confirming AC4's "no `.claude/rules/*` file is edited" constraint holds across the entire branch, not only the files touched by this plan.
- [x] [P5-T5] Add a new row to `docs/features/active/utilitiescs-nullable-ci-capstone/spec.md`'s "Maintainer Decision Summary" table recording the pre-existing analyzer-package-version-drift finding: 16 first-party `.csproj` files hardcode `<Analyzer Include>` paths to stale nuget package versions (Meziantou.Analyzer.3.0.101, SonarAnalyzer.CSharp.10.27.0.140913, Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4) that no longer match each project's `packages.config` (3.0.123 / 10.29.0.143774 / 5.6.0); confirmed identically present on `origin/main`, predating this epic; CI stays green today only via GitHub Actions cache restore-key fallback retaining stale cached package directories. Cite this session's findings (this plan's Revision note item 2) and state the item is flagged only, not fixed by this feature (fixing it would touch all 16 csproj files, a separate maintenance task, out of scope).
  - Acceptance: `git diff docs/features/active/utilitiescs-nullable-ci-capstone/spec.md` shows exactly one new row added to the Maintainer Decision Summary table (no other row modified or removed) with the citations above.
- [x] [P5-T6] Confirm no `.csproj` file's `<Analyzer Include>` path is modified by this feature: run `git diff --name-only origin/epic/utilitiescs-nullable-remediation-integration...HEAD -- "**/*.csproj"` and, for any returned file, confirm the diff contains no changed `<Analyzer Include>` line.
  - Acceptance: for every `.csproj` path returned by the diff (if any), a line-level review confirms zero changed `<Analyzer Include>` lines, confirming the analyzer-version-drift issue is flagged (P5-T5) but not touched by this feature.
- [x] [P5-T7] Add a new row to `docs/features/active/utilitiescs-nullable-ci-capstone/spec.md`'s "Maintainer Decision Summary" table recording that Phase 2's build-debt remediation scope grew beyond the two originally-declared trees (`SVGControl/**`; `UtilitiesCS/EmailIntelligence/**` + `UtilitiesCS/OutlookObjects/Folder/**`) to include `ToDoModel.csproj` (CS0618), `TaskVisualization.csproj` (CS4014), `ToDoModel.Test.csproj` (CS0169), and any further layers remediated under P2-T21/P2-T22; cite the root cause (commit `20d163ac`/PR #361, the `/t:Rebuild` fix, is not yet an ancestor of `origin/main` — only of the epic integration branch — so this is the first genuine full-solution rebuild ever attempted, and these are pre-existing repo warnings, not defects introduced by any epic child) and list every file/diagnostic-code/pattern-applied/rationale tuple recorded across P2-T17 through P2-T23's evidence artifacts.
  - Acceptance: `git diff docs/features/active/utilitiescs-nullable-ci-capstone/spec.md` shows exactly one new row added to the Maintainer Decision Summary table (no other row modified or removed), listing every file/diagnostic-code/pattern-applied/rationale tuple recorded across P2-T17–P2-T23's evidence artifacts, with the root-cause citation above.

### Phase 6 — Modified-Workflow Green-Run Recording

- [x] [P6-T1] Record that `.github/workflows/ci.yml` is modified by this feature, which triggers the `modified-workflow-needs-green-run` policy rule (`.claude/skills/feature-review-workflow/SKILL.md`), and that a green CI run against the branch head is a required, unsatisfied-by-this-plan, execution/merge-time obligation carried out by epic-orchestrator's fan-in. Write this record to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/other/green-run-requirement-recorded.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, the exact modified-workflow path (`.github/workflows/ci.yml`), the rule citation, and an explicit `NOT SATISFIED BY THIS PLAN` statement naming epic-orchestrator as the responsible party for capturing the green run before merge.

### Phase 7 — Final QC

- [x] [P7-T1] Run `dotnet csharpier format .` from the repository root and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-csharpier.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero files reformatted. If this step reformats any file, restart the Final QC loop from P7-T1.
- [x] [P7-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-analyzers-build.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming zero analyzer/code-style errors. If this step fails or changes files, restart the Final QC loop from P7-T1.
- [x] [P7-T3] Run the pragma nullable gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) on the fully-remediated, fully-edited tree and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-nullable-gate.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (exact command above), `EXIT_CODE: 0`, `Output Summary:` confirming a clean full rebuild with no CS0649/CS86xx/CS0618/CS0168/CS4014/CS0169 build errors, matching Phase 2's P2-T23 checkpoint (not the Phase 0 pre-remediation baseline, which was intentionally non-clean). If this step fails or changes files, restart the Final QC loop from P7-T1.
- [x] [P7-T4] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (or `vstest.console.exe <resolved UtilitiesCS.Test.dll and SVGControl.Test.dll paths> /EnableCodeCoverage`) and record the result to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-tests-coverage.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric passed/failed test counts and the numeric post-change coverage headline (line-rate and branch-rate). If this step fails or changes files, restart the Final QC loop from P7-T1.
- [x] [P7-T5] Compare the P0-T10 baseline coverage figures (the honest, pre-remediation baseline — not a fictitious clean one) against the P7-T4 post-change coverage figures and record the delta verification to `docs/features/active/utilitiescs-nullable-ci-capstone/evidence/qa-gates/qc-coverage-delta.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, the P0-T10 baseline line-rate/branch-rate numbers, the P7-T4 post-change line-rate/branch-rate numbers, and an explicit statement that post-change coverage is not lower than the P0-T10 baseline (AC7 no-regression). If a reduction is found, this plan's outcome is remediation-required, not PASS.
- [x] [P7-T6] Confirm `git status --porcelain` is empty at the repository root, repo-wide, after all Phase 7 steps complete, except for the intentional, already-reviewed changes from Phase 1 (SvgImageSelector.cs pragma), Phase 2 (nullable-annotation remediation batches), Phase 3 (ci.yml gate edit), and Phase 5 (spec.md maintainer-decision row).
  - Acceptance: `git status --porcelain` returns no output beyond the enumerated intentional changes, confirming no deliberately-introduced verification defect, no stray reformat, and no untracked file remains on the branch at hand-off.
