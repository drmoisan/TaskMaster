# outlook-store-exclusion — Remediation Plan (Issue #328)

- **Issue:** #328
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/328
- **Owner:** drmoisan
- **Work Mode:** full-feature (AC sources: `spec.md` + `user-story.md`)
- **Last Updated:** 2026-07-16T02-30
- **Status:** DRAFT — remediation of the three enumerated items in
  `remediation-inputs.2026-07-15T21-22.md`. The feature implementation (original plan
  `plan.2026-07-15T18-45.md`, v1.2, all 42 tasks P0-T1..P4-T8 checked off) is delivered and its
  toolchain/coverage evidence stands. This plan does NOT re-open any Phase 0–4 task or Scope-Lock
  entry from that plan except as R1/R2/R3 require.
- **Version:** 1.0
- **Language/tooling:** C# / MSTest / Moq / FluentAssertions (no C# source or test file is modified by
  this remediation; see the No-Code-Change QA Statement below).

## Authoritative Inputs (do not re-derive)

- `docs/features/active/2026-07-15-outlook-store-exclusion-328/remediation-inputs.2026-07-15T21-22.md`
  (the trigger; enumerates R1, R2, R3 and the do-not-do list).
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/policy-audit.2026-07-15T21-22.md`
  (Section 5 coverage verdicts; §5.1 canonical-artifact absence; §5.2 per-class table).
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/code-review.2026-07-15T21-22.md`
  (Low finding: dead-method deletion vs documented non-goal).
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/feature-audit.2026-07-15T21-22.md`
  (AC12 / US-AC4 graded PARTIAL on the two coverage items).
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/plan.2026-07-15T18-45.md`
  (delivered plan-of-record; P4-T7 AppToDoObjects.cs 503-line pre-existing-exception precedent).
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml`
  (the verified feature Cobertura; source data for R1).
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/coverage-delta.2026-07-15T18-45.md`
  (StoreWrapper baseline branch 65.38% → post 64.81%; line 95.31%; source data for R2).
- `artifacts/orchestration/orchestrator-state.json` `human_interaction_history[0]`
  (`response: scope_change`, `resolved_at: 2026-07-15T23:35:00Z`; source citation for R3).

All policy authority order is per `.claude/skills/policy-compliance-order`; do not duplicate policy
content here. Evidence-path authority is `.claude/skills/evidence-and-timestamp-conventions`.

## Evidence Location Statement

- All EVIDENCE artifacts produced by this plan resolve to
  `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/<kind>/`
  (`baseline/`, `remediation-baseline/`, `qa-gates/`, `issue-updates/`). No forbidden `artifacts/`
  evidence sub-path is used. The delegation prompt supplied the canonical scheme; no
  `EVIDENCE_LOCATION_OVERRIDE_REJECTED` line is required.
- `artifacts/csharp/coverage.xml` (R1) is NOT an evidence artifact. It is the hard-coded
  coverage-gate INPUT path that `.claude/hooks/validate-feature-review-coverage.ps1` reads
  (`Get-JacocoRepoCoverage -Path 'artifacts/csharp/coverage.xml'` and
  `Get-JacocoBranchCoverage -Path 'artifacts/csharp/coverage.xml'`). `enforce-evidence-locations.ps1`
  explicitly lists `artifacts/csharp/` as an ALLOWED path (it is not in the forbidden prefix set).
  Writing this file is required by the tooling and is not an evidence-location violation. The R1
  evidence NOTE that documents the conversion still resolves to `evidence/qa-gates/`.

## Coverage-Validator Format Determination (R1 pre-work, completed during planning)

- `.claude/hooks/validate-feature-review-coverage.ps1` parses `artifacts/csharp/coverage.xml` as
  **JaCoCo XML**: `Get-JacocoRepoCoverage` sums `//counter[@type="LINE"]` `missed`/`covered`;
  `Get-JacocoBranchCoverage` sums `//counter[@type="BRANCH"]` `missed`/`covered`.
- The verified feature evidence (`final-coverage.2026-07-15T18-45.cobertura.xml`) is **Cobertura**
  (`line-rate`/`branch-rate` attributes), which the hook cannot read.
- Decision: R1 requires a **Cobertura → JaCoCo conversion** (not Cobertura-as-is), scoped to
  first-party production packages, written to `artifacts/csharp/coverage.xml`.

## Scope-Lock — files this plan authorizes changing

Documentation (modify — R3 only):
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/spec.md`
  (§2.2 out-of-scope dead-method bullet; §6.2 dead-method requirement/deletion-out-of-scope text;
  §11 required-file-changes map line for `ToDoEvents.cs`; AC6 wording in §12; and the §12 AC Status
  narrative for AC12).
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/user-story.md`
  (Non-Goals dead-method bullet; and the AC Status narrative for US-AC4).

Coverage-gate tooling input (new — R1 only; hook-mandated path, not an evidence-kind path):
- `artifacts/csharp/coverage.xml` (JaCoCo-format, converted from the verified feature Cobertura).

Evidence artifacts (new — canonical `<FEATURE>/evidence/<kind>/`):
- `evidence/baseline/phase0-instructions-read.remediation.2026-07-16T02-30.md`
- `evidence/remediation-baseline/baseline-carryover.2026-07-16T02-30.md`
- `evidence/qa-gates/csharp-coverage-canonical.2026-07-16T02-30.md` (R1)
- `evidence/qa-gates/storewrapper-branch-coverage-disposition.2026-07-16T02-30.md` (R2)
- `evidence/qa-gates/remediation-verification.2026-07-16T02-30.md` (final)
- `evidence/issue-updates/ac-checkoff.remediation.2026-07-16T02-30.md` (final)

NOT changed by this remediation (explicit): no production `.cs`, no test `.cs`, no `.csproj`,
no `.props`/`.targets`, no coverage configuration (`coverage.config`/`*.runsettings`), and no
coverage threshold. The original plan's Scope-Lock and its Phase 0–4 tasks are not re-opened.

## No-Code-Change QA Statement (authority: atomic-plan-contract Final QA Loop; remediation-inputs do-not-do list)

- This remediation modifies only Markdown documents (R3), one Markdown disposition note (R2), one
  Markdown evidence note plus the JaCoCo coverage-gate input file (R1), and evidence artifacts. No
  C# source or test file compiles differently; no `.csproj` changes.
- Therefore the four-stage C# toolchain loop (csharpier → analyzer build → nullable/TWAE build →
  vstest) is NOT triggered and is NOT re-run. The remediation-inputs do-not-do list states: "Do not
  re-run or alter the passing toolchain stages except as needed to regenerate coverage." R1 does not
  regenerate coverage by re-running vstest; it converts the already-verified Cobertura, which is
  deterministic and avoids re-introducing the documented `dotnet-coverage` vendored-module
  denominator nondeterminism (policy-audit §5.4).
- The authoritative toolchain and coverage baseline is the original plan's executed Phase 4 evidence,
  carried over in Phase 0 (P0-T2). The final QC gate for this remediation is the artifact-and-AC
  verification in Phase 4, not a toolchain re-run.

## Coverage Floor (authority: CLAUDE.md / `.claude/rules/general-unit-test.md`)

- Repository-wide first-party line coverage floor: >= 85% (uniform) — read from the canonical
  `artifacts/csharp/coverage.xml` after R1.
- Branch-coverage floor: >= 75% (uniform). `StoreWrapper.cs` is below this floor at 64.81%
  (pre-existing; baseline 65.38%); R2 records the ratified pre-existing-exception disposition rather
  than commissioning open-ended new branch tests. No threshold is weakened and no production-source
  `exclude` entry is added.
- New/changed-line coverage on the touched non-exempt first-party classes is already verified >= 95%
  line with both arms of every new branch covered (policy-audit §5.2/§5.3; coverage-delta evidence);
  this remediation does not alter that.

## Remediation Plan (Atomic Tasks)

### Phase 0 — Policy Reads and Baseline Carryover

- [x] [P0-T1] Read the policy set in required order and record the read in `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/baseline/phase0-instructions-read.remediation.2026-07-16T02-30.md`
  - Files to read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/policy-compliance-order/SKILL.md`
  - Acceptance: artifact exists with `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Record the baseline-or-carryover statement in `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/remediation-baseline/baseline-carryover.2026-07-16T02-30.md`, citing that this remediation modifies no C# source/test/`.csproj` file so the original plan's executed Phase 4 toolchain and coverage evidence carries over unchanged
  - Cite carryover artifacts by path: `evidence/qa-gates/final-csharpier.2026-07-15T18-45.md`, `final-analyzer-build.2026-07-15T18-45.md`, `final-nullable-build.2026-07-15T18-45.md`, `final-vstest.2026-07-15T18-45.md`, `final-coverage.2026-07-15T18-45.cobertura.xml`, `coverage-delta.2026-07-15T18-45.md`, `file-size-check.2026-07-15T18-45.md`.
  - Acceptance: artifact records `Timestamp:`, `Command:` (`N/A — carryover, no toolchain re-run`), `EXIT_CODE:` (`0` carryover), and `Output Summary:` containing the numeric carryover coverage headline values (StoreFilterAttribution line 100.00% / branch 96.88%; StoresWrapper line 98.42% / branch 89.13%; StoreWrapper line 95.31% / branch 64.81%; StoreWrapperController line 95.89% / branch 85.38%) and the statement that csharpier/analyzers/nullable/vstest are not re-run because no compilable file changes.

### Phase 1 — R1 Emit Canonical C# Coverage Artifact (JaCoCo)

- [x] [P1-T1] Convert the verified feature Cobertura `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml` to JaCoCo XML at `artifacts/csharp/coverage.xml`, scoped to first-party production packages (include the first-party production assemblies present in the Cobertura report; exclude the vendored modules Deedle / FSharp.Core / Swordfish / SVGControl and all `*.Test` assemblies), emitting `<counter type="LINE" missed=".." covered=".."/>` and `<counter type="BRANCH" missed=".." covered=".."/>` elements
  - Acceptance: `artifacts/csharp/coverage.xml` exists; it is well-formed XML containing at least one `//counter[@type="LINE"]` and at least one `//counter[@type="BRANCH"]`; dot-sourcing `.claude/hooks/validate-feature-review-coverage.ps1` and calling `Get-JacocoRepoCoverage -Path 'artifacts/csharp/coverage.xml'` and `Get-JacocoBranchCoverage -Path 'artifacts/csharp/coverage.xml'` each return a non-null numeric value.
- [x] [P1-T2] Verify and record the parsed canonical-artifact coverage in `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/csharp-coverage-canonical.2026-07-16T02-30.md`, including the parsed first-party line% and branch%, and the exact included-package / excluded-(vendored+test)-package manifest so the number is auditable
  - Acceptance: artifact records `Timestamp:`, `Command:` (the `Get-JacocoRepoCoverage`/`Get-JacocoBranchCoverage` invocations), `EXIT_CODE:`, and `Output Summary:` with the numeric first-party line% and branch%. The recorded first-party line coverage is `>= 85%` AND `Test-Path artifacts/csharp/coverage.xml` is `True`. Explicitly-authorized alternative (only if the deterministic first-party aggregate cannot clear 85% because pre-existing out-of-scope low-coverage first-party files inflate the denominator beyond issue #328's incremental contribution): record the PR CI coverage run as the authoritative repo-wide C# coverage gate per policy-audit §5.4, citing the CI workflow-run URL; the canonical `artifacts/csharp/coverage.xml` remains present and hook-parseable in either branch.

### Phase 2 — R2 Disposition StoreWrapper Branch-Coverage Floor

- [x] [P2-T1] Author the ratified pre-existing branch-coverage disposition for `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` in `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/storewrapper-branch-coverage-disposition.2026-07-16T02-30.md`, mirroring the AppToDoObjects.cs 503-line pre-existing-exception precedent (original plan P4-T7 / `file-size-check.2026-07-15T18-45.md`)
  - The note MUST state, verifiably: (a) file path `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`; (b) pre-existing baseline branch coverage `65.38%` (already below the 75% floor before #328, per `coverage-delta.2026-07-15T18-45.md`); (c) post-change branch coverage `64.81%`; (d) the 0.57-point movement is a denominator effect from newly-added, fully-covered `StoreId`-capture branches (both true/false arms exercised by `StoreWrapperTests` StoreId cases), NOT a regression on any pre-existing changed line; (e) line coverage `95.31%` clears the 85% line floor; (f) an explicit statement that this is a recorded acknowledgment of a pre-existing condition #328 did not introduce or worsen, is NOT a threshold weakening, and adds NO production-source `exclude` entry; (g) a maintainer-ratification line.
  - Acceptance: the artifact exists at the canonical `evidence/qa-gates/` path with all seven enumerated fields present and the two numeric branch values (65.38% pre / 64.81% post) recorded; a check confirms no coverage configuration file (`coverage.config`/`*.runsettings`/csproj coverage excludes) and no coverage threshold was modified by this remediation.

### Phase 3 — R3 Reconcile Dead-Method Deletion Documentation

- [x] [P3-T1] Reconcile every `spec.md` passage that describes threading or deferring the two dead `ToDoEvents` methods to match the delivered deletion — the §2.2 "Deleting the two dead `ToDoEvents` methods" out-of-scope bullet, the §6.2 "Dead-method requirement" / "Deletion is out of scope" text, and the §11 required-file-changes map line for `ToDoModel/Data Model/ToDo/ToDoEvents.cs` — stating that `GetListOfToDoItemsInView` and `GetToDoItemsInView` were DELETED as part of #328 (not threaded, not deferred to a separate issue) under the user-approved scope change (`artifacts/orchestration/orchestrator-state.json` `human_interaction_history`, `response: scope_change`, `resolved_at: 2026-07-15T23:35:00Z`)
  - Acceptance: the §2.2, §6.2, and §11 passages in `docs/features/active/2026-07-15-outlook-store-exclusion-328/spec.md` state deletion-as-delivered and cite the approved scope change; no remaining spec text asserts the two methods are "threaded" or that deletion is "out of scope" / "a separate issue"; no code change is made.
- [x] [P3-T2] Rewrite spec.md AC6 (§12) so its sub-clause no longer says the two methods "are threaded with the same filter for consistency"; instead state that the two dead methods (`GetListOfToDoItemsInView`, `GetToDoItemsInView`) were deleted under the approved scope change, removing the bypass entirely, while preserving AC6's substantive requirement that the live bypass sites route through the shared predicate with no parallel filtering logic
  - Acceptance: AC6 text in `docs/features/active/2026-07-15-outlook-store-exclusion-328/spec.md` describes deletion-as-delivered and no longer contradicts it; the "bypass sites route through the filter; no parallel filtering logic" requirement is retained; no code change.
- [x] [P3-T3] Reconcile the `user-story.md` Non-Goals bullet "Deleting the two apparently-dead `ToDoEvents` methods ... deferred to the atomic plan and, if pursued, a separate issue" to state the two methods were deleted as part of #328 under the approved scope change (cite `resolved_at: 2026-07-15T23:35:00Z`)
  - Acceptance: the Non-Goals bullet in `docs/features/active/2026-07-15-outlook-store-exclusion-328/user-story.md` states deletion-as-delivered and no longer describes threading/deferral; no code change.

### Phase 4 — Final Verification and Acceptance-Criteria Re-Checkoff

- [x] [P4-T1] Verify the three remediation items and record `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/remediation-verification.2026-07-16T02-30.md`: (R1) `Test-Path artifacts/csharp/coverage.xml` is `True` and the hook parser returns a first-party line% >= 85% (or the P1-T2 PR-CI-authoritative alternative is recorded); (R2) the StoreWrapper disposition note exists with all seven required fields; (R3) a grep of `spec.md` and `user-story.md` for the dead-method threading/deferral wording returns only the reconciled deletion/scope-change text
  - Acceptance: artifact records `Timestamp:`, `Command:` (the `Test-Path`, hook-parse, and grep invocations), `EXIT_CODE:`, and `Output Summary:` confirming all three checks pass; any failing check makes the outcome remediation-required, not PASS.
- [x] [P4-T2] Re-run the acceptance-criteria checkoff and record `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/issue-updates/ac-checkoff.remediation.2026-07-16T02-30.md`, resolving spec.md AC12 and user-story.md US-AC4 from PARTIAL to PASS on the basis of R1 (canonical `artifacts/csharp/coverage.xml` emitted and readable) and R2 (StoreWrapper branch-floor ratified pre-existing disposition), and confirming AC6 wording is reconciled by R3; update the AC Status narrative lines in `spec.md` (§12) and `user-story.md` for AC12/US-AC4 to cite this resolution
  - Acceptance: the ac-checkoff artifact shows spec.md 12/12 and user-story.md 4/4 PASS with AC12 and US-AC4 explicitly resolved to PASS (citing R1 + R2) and AC6 noted as reconciled (citing R3); no AC remains PARTIAL; the spec.md/user-story.md AC Status narratives reference the remediation resolution. This task runs only after P4-T1 reports all three checks passing.

## Acceptance Criteria Traceability (remediation item → tasks)

- R1 (canonical C# coverage artifact at `artifacts/csharp/coverage.xml`, JaCoCo, readable >= 85% line) → P1-T1, P1-T2; verified P4-T1; resolves spec AC12 / US-AC4 → P4-T2.
- R2 (ratified pre-existing StoreWrapper branch-floor disposition; no threshold weakening, no exclude) → P2-T1; verified P4-T1; resolves spec AC12 / US-AC4 → P4-T2.
- R3 (spec §2.2/§6.2/§11 + AC6 + user-story Non-Goals reconciled to deletion-as-delivered) → P3-T1, P3-T2, P3-T3; verified P4-T1; AC6 reconciliation confirmed → P4-T2.

## Out-of-Scope Guardrails (from remediation-inputs do-not-do list)

- Do NOT weaken any coverage threshold or add a production-source `exclude` entry (R2 is a documented
  disposition note only).
- Do NOT refactor the accepted filter-predicate duplication (`ShouldIncludeStore` / `Decide` /
  `StoreIsIncluded`).
- Do NOT re-run or alter the passing toolchain stages except as needed to regenerate coverage; R1
  converts existing verified coverage rather than re-running vstest.
- Do NOT narrow scope or re-open delivered behavior; no `.cs`/`.csproj` file is modified.
- Do NOT add temporary files or non-deterministic APIs (no new tests are written).

## Preflight

- Preflight signal is reported in the planner's final message per `atomic-plan-contract`
  (`DIRECTIVE: PREFLIGHT VALIDATION ONLY`).
