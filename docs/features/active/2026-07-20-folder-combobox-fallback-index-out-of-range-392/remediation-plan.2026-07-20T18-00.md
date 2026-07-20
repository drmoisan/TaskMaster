# Remediation Plan — folder-combobox-fallback-index-out-of-range (Issue #392), Cycle 1 (R1)

- **Timestamp:** 2026-07-20T18-00
- **Entry cycle:** 1
- **Work Mode:** `minor-audit` (AC source: `issue.md` `## Acceptance Criteria` only; AC-1 through
  AC-5 are already all `[x]` and are not reopened by this cycle — see Phase 2)
- **Trigger:** `remediation-inputs.2026-07-20T18-00.md`, sourced from
  `policy-audit.2026-07-20T18-00.md` Section 5 (coverage findings R1/R2), `code-review.2026-07-20T18-00.md`
  (Findings CR-1/CR-2/CR-3, informational — no action required by this cycle), and
  `feature-audit.2026-07-20T18-00.md` (AC-5 coverage caveat).
- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392`
- **Timestamp token:** every `<TS>` placeholder below MUST be substituted with the real ISO-8601
  timestamp (`yyyy-MM-ddTHH-mm`) at the moment the artifact is written, per
  `evidence-and-timestamp-conventions`.

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QC artifact tasks,
and a coverage delta/threshold task for the in-scope language (C#). If any required baseline, QC,
or coverage-comparison artifact is missing, the remediation verdict must be BLOCKED or INCOMPLETE,
never PASS.

**Evidence accounting rule:** Each evidence-producing task names its exact artifact path under
`docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/<kind>/`.
Do not mark an evidence-backed task complete without the artifact on disk. The sole non-evidence
exception is the repo-wide coverage-gate tooling input at `artifacts/csharp/coverage.xml`
(JaCoCo), which is documented as a non-canonical-evidence tooling path, not a duplicate evidence
location (see the main plan's Correction note).

## Scope-Lock (reiterated, not reopened)

This cycle may modify only:
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (R1: add coverage only where an
  existing, pre-#392 branch is currently uncovered — no new production code path, no behavior
  change).
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` (R1: 1-2 new `[TestMethod]`
  additions only — **no new file**; splitting this file into two is explicitly NOT authorized in
  this cycle even though `code-review` Finding CR-2 notes it is at the 500-line boundary).

No other production file may be changed. `spec.md`/`user-story.md` are confirmed absent from the
feature folder and remain out of scope for this minor-audit AC source. R2 is documentation-only (no
code, no policy edits, no coverage excludes, no package-wide test additions).

## Do-Not-Do List (from `remediation-inputs.2026-07-20T18-00.md`, reiterated)

- Do not modify `.claude/rules/*` or any policy document to weaken the 85%/75% coverage floor.
- Do not add a `coverage.config` or `.csproj` coverage-exclude for `QuickFiler` or any of its
  classes to artificially raise the measured percentage.
- Do not expand the Scope-Lock beyond the two named files to chase the package-wide `QuickFiler`
  floor (that is R2's disposition, not R1's code task).
- Do not silently mark AC-5 or any policy-audit coverage row as PASS without the corresponding
  remediation task closing the gap (R1) or a recorded maintainer disposition (R2).
- Do not weaken or delete the two Phase-1 (original cycle) regression tests, the six re-verified
  pre-existing tests, or `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection` (the test
  that documents the pre-existing, unrelated, out-of-scope empty-array gap per code-review Finding
  CR-3/Info).

## R1/R2 Disposition Map (scope decisions already made by the orchestrator — encoded, not reopened)

- **R1** (`QfcItemController.FolderHandling.cs` class-level branch coverage 73.81% vs 75% floor,
  `policy-audit.2026-07-20T18-00.md` Section 5.2/5.5): IN SCOPE. Closed by Phase 1 (P1-T2..P1-T5)
  and verified by Phase 2's coverage delta (P2-T6).
- **R2** (`QuickFiler` package-wide 73.68%/64.62% and canonical repo-wide artifact 16.25%/13.60%,
  Section 5.1/5.4/5.6): RESOLVED AS `SCOPE_CHANGE`, no code work. Tracked in open GitHub issue #136
  (*Feature: quickfiler-80-per-file-coverage*). The orchestrator has already recorded a
  `human_interaction` `scope_change` disposition in `orchestrator-state`, citing issue #136,
  `remediation-inputs.2026-07-20T18-00.md` option (b), and CLAUDE.md's COM/VSTO testable-denominator
  exemption language. This plan's only obligation is the single documentation task P1-T1, which
  records that disposition verbatim as feature-folder evidence.

---

### Phase 0 — Remediation Baseline Capture

- [x] [P0-T1] Read `CLAUDE.md` in full (policy reading order position 1).
  - Acceptance: file read in this session; its Policy Compliance Order section is quoted verbatim
    in the P0-T5 evidence artifact.

- [x] [P0-T2] Read `.claude/rules/general-code-change.md` (policy reading order position 2).
  - Acceptance: file read; its Mandatory Toolchain Loop / File Size Limit sections quoted in the
    P0-T5 evidence artifact.

- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` (policy reading order position 3).
  - Acceptance: file read; its Coverage Requirements section quoted in the P0-T5 evidence artifact.

- [x] [P0-T4] Read `.claude/rules/csharp.md` and `.claude/rules/quality-tiers.md` (policy reading
  order positions 4-5, plus the uniform 85%/75% coverage-tier rule that triggered this cycle).
  - Acceptance: both files read; the Toolchain/Testing Standards section of `csharp.md` and the
    Uniform-vs-Tier-Dependent Gate Matrix of `quality-tiers.md` are quoted in the P0-T5 evidence
    artifact.

- [x] [P0-T5] Write the Phase 0 policy-read evidence artifact to
  `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/remediation-baseline/phase0-instructions-read.<TS>.md`.
  - Acceptance: file exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of
    the four files read in P0-T1 through P0-T4, in order.

- [x] [P0-T6] Verify the minor-audit Scope-Lock boundary for this remediation cycle.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/remediation-baseline/minor-audit-scope.<TS>.md`.
  - Acceptance: artifact confirms `spec.md`/`user-story.md` remain absent from the feature folder,
    and confirms the Scope-Lock file list above (`QfcItemController.FolderHandling.cs` and
    `QfcItemController.FolderHandlingTests.cs` only, no new files) is unchanged from the original
    cycle's fix scope.

- [x] [P0-T7] Record remediation baseline git state (current branch, `HEAD` short SHA via
  `git rev-parse --abbrev-ref HEAD` and `git rev-parse --short HEAD`).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/remediation-baseline/git-baseline-state.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
    stating the branch name and SHA.

- [x] [P0-T8] Re-run the C# coverage collection for `QuickFiler.Test` and record the class-level
  `QfcItemController.FolderHandling.cs` line/branch rates as this cycle's re-baseline.
  - Command: `dotnet-coverage collect -f cobertura -s coverage-exclude-deedle.xml -o <TS>-coverage.cobertura.xml -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/remediation-baseline/coverage-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
    the numeric class-level line/branch rate for `QfcItemController.FolderHandling.cs`. Expected
    starting point (per `policy-audit.2026-07-20T18-00.md` Section 5.2): 91.89% line / 73.81%
    branch. Do not assume these figures are unchanged without re-measuring — this re-baseline may
    run at a later point in time than the original audit.

---

### Phase 1 — Constrained Remediation Implementation (R1 Code + R2 Documentation)

- [x] [P1-T1] Record the R2 maintainer-disposition evidence artifact (documentation only; no code
  change; not gated on P0-T8).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/coverage-disposition-decision.<TS>.md`.
  - Acceptance: artifact records, verbatim, the disposition already made by the orchestrator: the
    `QuickFiler` package-wide (73.68%/64.62%) and canonical repo-wide artifact (16.25%/13.60% raw
    six-package aggregate) coverage gaps are `SCOPE_CHANGE`, tracked in open GitHub issue #136
    (*Feature: quickfiler-80-per-file-coverage*), citing (a) the recorded `human_interaction`
    `scope_change` entry in `orchestrator-state`, (b) the `#328` `StoreWrapper` branch-floor
    exception precedent as the analogous ratification pattern, (c) CLAUDE.md's COM/VSTO
    testable-denominator exemption language, and (d) that the true all-first-party repo-wide figure
    is measured by the PR CI full-suite run (not this single-project local collection). This
    satisfies R2 in full; no further R2 task exists in this plan.

- [x] [P1-T2] Identify, from the class-level Cobertura per-line data captured in P0-T8, the
  specific existing (pre-#392) branch(es) in `QfcItemController.FolderHandling.cs` that remain
  uncovered and are the smallest, most self-contained addition to close the 73.81% -> >= 75% gap.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/other/branch-gap-analysis.<TS>.md`.
  - Acceptance: artifact records the identified line(s)/branch(es) with file:line citations and the
    rationale for why exercising them is expected to close the gap, with no production behavior
    change implied.

- [x] [P1-T3] Add 1-2 new `[TestMethod]`s to
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` exercising the branch(es)
  identified in P1-T2, using MSTest + Moq (for any collaborator mocking) + FluentAssertions,
  consistent with the file's existing test style.
  - Precondition: P1-T2 complete.
  - Acceptance: no existing test's assertions, names, or behavior are modified (the six
    re-verified pre-existing tests and `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection`
    are untouched); no new production code path is introduced in `QfcItemController.FolderHandling.cs`
    (this task adds tests only, not production code); no new file is created.

- [x] [P1-T4] Verify the new test(s) from P1-T3 pass and do not weaken any existing assertion.
  - Precondition: P1-T3 complete.
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~<new-test-name(s), substituted from P1-T3>"`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/regression-testing/new-branch-test-pass.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    stating the new test(s) passed.

- [x] [P1-T5] Confirm `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` remains
  within the 500-line limit after the P1-T3 addition.
  - Command: `wc -l "QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs"` (baseline
    500 lines exactly per `policy-audit.2026-07-20T18-00.md` Section 4, zero headroom).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/other/file-size-check.<TS>.md`.
  - Acceptance: file is <= 500 lines. Per this cycle's Scope-Lock, splitting the file into two
    (code-review Finding CR-2's suggestion) is NOT authorized in this cycle: if the P1-T3 addition
    would exceed 500 lines, the addition must be made to fit within the existing file (e.g., by
    trimming redundant inline comments/blank lines elsewhere in the file, not by shortening
    assertions or weakening any test) rather than creating a new file. If 500 lines cannot be met
    without a new file or without weakening a test, halt this task and escalate to the coordinator
    for an explicit Scope-Lock amendment before proceeding — do not silently split the file or
    silently exceed the limit.

---

### Phase 2 — Final QC Loop, Coverage Delta, and Reaudit Handoff

Unconditional full C# toolchain, run in order. If any step fails or changes files, restart this
phase from P2-T1. No `SKIPPED` outcomes; no IN_SCOPE/OUT_OF_SCOPE branches.

- [x] [P2-T1] Run the final C# formatting command.
  - Command: `dotnet tool run csharpier .` then `dotnet tool run csharpier --check .`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-csharpier-final.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; the
    two Scope-Lock files must be format-clean. Pre-existing, unrelated config-noise failures on
    other files (unchanged from the original cycle's baseline) are non-blocking; any NEW
    format failure on a file outside that pre-existing noise set is blocking and requires a
    restart from P2-T1.

- [x] [P2-T2] Run the final C# analyzer build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-analyzer-final.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`;
    zero analyzer diagnostics attributable to the two Scope-Lock files. If this command fails, fix
    and restart Phase 2 from P2-T1.

- [x] [P2-T3] Run the final C# nullable build command.
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Rationale for `/t:Rebuild` (not `/t:Build`): established in the main plan's P2-T3 — MSBuild's
    incremental up-to-date check otherwise skips `CoreCompile`, which would vacuously pass the
    error-set comparison below.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-nullable-final.<TS>.md`.
  - Acceptance: the command must run and the artifact must record `Timestamp:`, `Command:`,
    `EXIT_CODE:`, and `Output Summary:` including an error-set comparison against the original
    cycle's `evidence/baseline/nullable-baseline.<TS>.md` (P0-T11 of `plan.2026-07-20T12-59.md`,
    34 errors, all in vendored `SVGControl.csproj`). Acceptance = zero NEW errors relative to that
    baseline AND zero errors attributable to first-party in-scope files, per the amended AC-5 scope
    note in `issue.md` and `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`.
    If any NEW error appears, or any error is attributable to a first-party project, fix and restart
    Phase 2 from P2-T1.

- [x] [P2-T4] Run the final full-suite MSTest coverage command for `QuickFiler.Test`.
  - Command: `dotnet-coverage collect -f cobertura -s coverage-exclude-deedle.xml -o <TS>-coverage.cobertura.xml -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-vstest-coverage-final.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    with total tests, pass/fail counts (baseline 541 plus any new tests from P1-T3, all passing),
    and the numeric post-change class-level line/branch rate for `QfcItemController.FolderHandling.cs`.
    If this command fails, fix and restart Phase 2 from P2-T1.

- [x] [P2-T5] Regenerate the JaCoCo-format canonical coverage-gate input at
  `artifacts/csharp/coverage.xml` from the P2-T4 Cobertura output, using the established conversion
  pattern recorded in `evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md` (from the
  original cycle).
  - Evidence: `artifacts/csharp/coverage.xml` (JaCoCo, tooling-input path, not an evidence
    duplicate) and
    `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-coverage-conversion.<TS>.md`.
  - Acceptance: `artifacts/csharp/coverage.xml` exists in JaCoCo format with `Timestamp:`,
    `Command:`, `EXIT_CODE:`, and `Output Summary:` recorded in the evidence mirror, including the
    report-level, `QuickFiler`-package-level, and `QfcItemController.FolderHandling.cs`
    class-level LINE/BRANCH counter totals.

- [x] [P2-T6] Compute the coverage delta: the P0-T8 baseline (91.89% line / 73.81% branch) versus
  this phase's final class-level rate for `QfcItemController.FolderHandling.cs` (from P2-T4/P2-T5).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-coverage-delta.<TS>.md`.
  - Acceptance: artifact contains an explicit statement: **class-level branch coverage >= 75%:
    PASS/FAIL**; **no regression on any previously-covered line/branch: PASS/FAIL**. If either
    reads FAIL, this is remediation-required, not PASS: restart Phase 1 from P1-T2 with a
    different branch target rather than proceeding to P2-T7.

- [x] [P2-T7] Verify no other test regressed by comparing the original cycle's baseline
  (`evidence/baseline/vstest-coverage-baseline.<TS>.md`, 541 tests) and this cycle's final (P2-T4)
  full-suite results by test name/class.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-regression-check.<TS>.md`.
  - Acceptance: artifact confirms every test that passed at the original baseline still passes, and
    the total pass count did not decrease (it may only increase, by the P1-T3 addition).

- [x] [P2-T8] Update `issue.md` with a remediation-cycle closure note.
  - Files: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/issue.md`.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/issue-updates/remediation-cycle1-note.<TS>.md`.
  - Acceptance: no AC checkbox changes are made (all five ACs are already `[x]` and are unaffected
    by this coverage-only remediation cycle); the evidence note summarizes the R1 branch-coverage
    closure (P2-T6 PASS) and the R2 scope-change disposition (P1-T1), each cited to its backing
    artifact path.

- [x] [P2-T9] Hand off to `feature-review` for reaudit.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/remediation-reaudit-handoff.<TS>.md`.
  - Acceptance: `feature-review` writes `policy-audit.<exit-ts>.md`, `code-review.<exit-ts>.md`, and
    `feature-audit.<exit-ts>.md` (and, if any finding remains, a new
    `remediation-inputs.<exit-ts>.md` opening cycle 2) using an exit timestamp distinct from this
    cycle's entry timestamp (`2026-07-20T18-00`); the handoff evidence records that the reaudit is
    required to independently re-verify: (a) the class-level branch-coverage floor is met (P2-T6),
    (b) the P1-T1 R2 maintainer-disposition record is present and adequate, and (c) all five
    original acceptance criteria (AC-1 through AC-5) remain PASS with no regression introduced by
    this remediation cycle's own changes (P2-T7).

---

## R1/R2 Traceability Map (for preflight cross-check)

- R1 (class-level branch coverage 73.81% -> >= 75%) → P0-T8 (re-baseline), P1-T2 (gap analysis),
  P1-T3 (new tests), P1-T4 (pass verification), P1-T5 (file-size gate), P2-T4/P2-T5 (final
  measurement), P2-T6 (explicit PASS/FAIL delta gate).
- R2 (`QuickFiler` package-wide and canonical repo-wide gaps, `SCOPE_CHANGE`) → P1-T1 (sole
  documentation task), P2-T8 (closure note cites P1-T1), P2-T9 (reaudit re-verifies P1-T1 is
  adequate).
- No-regression obligation (all five original ACs stay PASS) → P2-T2, P2-T3, P2-T7, P2-T9.

## Preflight Note

This plan has not yet been preflighted by `atomic-executor` under
`DIRECTIVE: PREFLIGHT VALIDATION ONLY`. Per `remediation-handoff-atomic-planner`, execution must
not begin until `PREFLIGHT: ALL CLEAR` is returned; if `PREFLIGHT: REVISIONS REQUIRED` is returned,
`atomic-planner` (not `atomic-executor` or `feature-review`) revises this same file in place.
