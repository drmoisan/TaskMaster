# Remediation Plan — qfc-item-controller-testability (#227), Cycle 1

- **Cycle entry timestamp:** 2026-06-29T13-20
- **Feature folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227`
- **Base branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
- **Head:** `TaskMaster-wt-2026-06-29-09-38` (`bcc7d7e32a12693b732d5c5e133a681890bec412`)
- **Work Mode:** `full-feature` (resolved from `issue.md` metadata block: `- Work Mode: full-feature`)
- **Plan author:** atomic-planner
- **Remediation inputs (authoritative):** `remediation-inputs.2026-06-29T13-20.md` (cycle entry), `../2026-06-29T13-15-audit/remediation-inputs.2026-06-29T13-15.md`, `../2026-06-29T13-15-audit/policy-audit.2026-06-29T13-15.md`
- **Reference procedure precedent:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/remediation-plan.2026-06-28T21-30.md` (#223 cycle-1 canonical-coverage generation)

## Scope Statement (single root cause, R1 only)

The cycle-0 feature-review returned a Conditional Go with two blocking findings and one deferred
residual. This plan addresses ONLY **R1**: the workflow-mandated canonical Cobertura C# coverage
artifact `artifacts/csharp/coverage.xml` is absent. Coverage was recorded only in feature-folder
evidence files (`evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`,
`evidence/qa-gates/p8-tests-coverage.2026-06-29T12-40.md`), so the coverage-artifact-presence gate
fails. This is an evidence-artifact generation task. No production-code or test-code change is
required or permitted; the implementation already passed all four toolchain gates (233/233 tests)
and the affected testable non-exempt denominator (82.74%, 484/585) is met. This plan generates the
canonical Cobertura artifact from the QuickFiler.Test coverage run, validates that it is well-formed
and consistent with the existing evidence, and confirms the four-step toolchain remains green.

**Explicitly out of scope for this cycle (not in this plan):**

- **R2 — Exemption-boundary maintainer ratification.** Governance action escalated to the project
  maintainer (Dan Moisan). NOT routed to atomic-planner / atomic-executor. No task in this plan.
- **R3 — AC5 ≥90% new/extracted sub-target residual.** Deferred to #197 (injectable `Dispatcher`
  seam + EventWiring lambda extraction). No task in this plan.

## Guardrails (encoded from the do-not-do list)

- G1. Do NOT edit, add, or delete any production or test `.cs` file. This plan generates an
  evidence artifact only; it changes no source.
- G2. Do NOT modify any file under `.claude/rules/**` or `CLAUDE.md`, and do NOT weaken any
  coverage threshold or `[ExcludeFromCodeCoverage]` exemption.
- G3. Do NOT alter, weaken, delete, or add tests to move coverage numbers. The test count must
  remain 233/233.
- G4. Do NOT narrow scope or mark C# coverage "informational only." Do NOT touch R2 or R3.
- G5. The ONLY non-`<FEATURE>/evidence/<kind>/` output path permitted by this plan is the canonical
  coverage artifact `artifacts/csharp/coverage.xml` (the workflow-mandated machine-readable C#
  coverage artifact, the single permitted exception, per the #223 precedent and the orchestrate
  skill evidence rules). Every other artifact this plan produces is written under
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/`.

## Evidence Location Invariant

All evidence artifacts resolve to
`docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/`
(`remediation-baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`). The
single canonical-but-non-evidence path `artifacts/csharp/coverage.xml` is mandated by the
coverage-verification contract and is explicitly permitted; it is the only such exception. No
`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
`artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, or
`artifacts/post-change/` path is used. No non-canonical evidence path was supplied by the caller, so
no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entry is required.

## Contingency Model (deterministic, no open holes)

Coverage `.coverage` acquisition has two convergent sources and one decision task that selects
exactly one:

- **SOURCE-REUSE** — if a valid prior `.coverage` binary from the QuickFiler.Test run already
  exists (the same run that produced the 233/233 and 82.74% evidence), reuse it directly for the
  Cobertura conversion.
- **SOURCE-FRESH** — otherwise run
  `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`
  to produce a fresh `.coverage` binary.

Both sources converge on the same Phase 1 conversion (`dotnet-coverage merge ... -f cobertura -o
artifacts/csharp/coverage.xml`) and the same Phase 2 validation. A bounded attempt means a single
run (no retries, sleeps, or timing hacks per policy).

---

### Phase 0 — Policy Reads and Remediation Baseline Capture

- [x] [P0-T1] Read the policy files in the required order (`CLAUDE.md`,
  `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
  `.claude/rules/csharp.md`, and the skills `atomic-plan-contract`,
  `evidence-and-timestamp-conventions`, `remediation-handoff-atomic-planner`) and write
  `evidence/remediation-baseline/phase0-instructions-read.2026-06-29T13-20.md`.
  **Acceptance:** artifact exists and contains `Timestamp:`, `Policy Order:`, and an explicit list
  of every file read.

- [x] [P0-T2] Confirm the canonical coverage artifact is absent at cycle entry. Run
  `Test-Path artifacts/csharp/coverage.xml` and record the result in
  `evidence/remediation-baseline/baseline-canonical-artifact.2026-06-29T13-20.md`.
  **Acceptance:** artifact exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
  recording `artifacts/csharp/coverage.xml` = ABSENT (the defect baseline).

- [x] [P0-T3] Confirm no production or test code change is intended this cycle. Run
  `git status --porcelain` and confirm there is no pending edit to any `.cs` or `.csproj` file, and
  confirm `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` is present (the assembly whose
  coverage is being captured). Write
  `evidence/remediation-baseline/baseline-no-code-change.2026-06-29T13-20.md`.
  **Acceptance:** artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
  stating no `.cs`/`.csproj` change is pending or intended (R1 is artifact-generation only) and that
  `QuickFiler.Test.dll` exists.

- [x] [P0-T4] Capture coverage-tooling availability and the prior-cycle numeric coverage headline
  baseline. Record presence of `dotnet-coverage` (`Get-Command dotnet-coverage`) and
  `vstest.console.exe` (via `vswhere`), and record the known numeric coverage values carried from
  the prior cycle (233/233 tests pass; affected testable non-exempt denominator 484/585 = 82.74%;
  per-cluster figures from `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`). Write
  `evidence/remediation-baseline/baseline-coverage-tooling.2026-06-29T13-20.md`.
  **Acceptance:** artifact records tool availability (present/absent) and the numeric coverage
  headline values above, with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 1 — Canonical Coverage Artifact Acquisition

- [x] [P1-T1] Ensure a current Debug build so coverage instrumentation has a fresh QuickFiler.Test
  assembly. Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` and
  write `evidence/qa-gates/p1-build.2026-06-29T13-20.md`.
  **Acceptance:** build `EXIT_CODE: 0` recorded with `Timestamp:`, `Command:`, and `Output Summary:`;
  no source `.cs` file is modified by this step.

- [x] [P1-T2] Acquisition decision task: select exactly one `.coverage` source. If a valid prior
  QuickFiler.Test `.coverage` binary from the 12-40/12-50 run is present and readable, select
  **SOURCE-REUSE** and skip P1-T3. Otherwise select **SOURCE-FRESH** and execute P1-T3. Record the
  selection in `evidence/qa-gates/p1-acquisition-decision.2026-06-29T13-20.md`.
  **Acceptance:** artifact states `SELECTED_SOURCE: SOURCE-REUSE` or `SELECTED_SOURCE: SOURCE-FRESH`
  with the deciding observation (prior `.coverage` present+readable, or absent), the resolved
  `.coverage` path, plus `Timestamp:` and `Output Summary:`.

- [x] [P1-T3] SOURCE-FRESH only — produce a fresh `.coverage` binary. Run
  `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`
  (single attempt; no retries) and write
  `evidence/qa-gates/p1-coverage-collect.2026-06-29T13-20.md`.
  **Acceptance:** artifact records `Timestamp:`, exact `Command:`, `EXIT_CODE:`, and `Output Summary:`
  capturing the 233/233 test pass result and the produced `.coverage` path. If `SOURCE-REUSE` was
  selected in P1-T2, this task is marked `EXIT_CODE: SKIPPED` per its explicit SOURCE-FRESH skip
  branch.

- [x] [P1-T4] Convert the selected `.coverage` binary to Cobertura at the canonical path. Run
  `dotnet-coverage merge <selected .coverage path> -f cobertura -o artifacts/csharp/coverage.xml`
  following the documented #223 cycle-1 procedure. Write
  `evidence/qa-gates/p1-coverage-convert.2026-06-29T13-20.md`.
  **Acceptance:** `artifacts/csharp/coverage.xml` exists; artifact records `Timestamp:`, exact
  `Command:`, `EXIT_CODE:`, and `Output Summary:` confirming the conversion produced the file.

- [x] [P1-T5] Verify the canonical artifact. Confirm `artifacts/csharp/coverage.xml` exists, parses
  as well-formed Cobertura XML, and exposes a readable repo/package `line-rate` attribute and the
  QuickFiler package/classes. Write
  `evidence/qa-gates/p1-canonical-artifact-verified.2026-06-29T13-20.md`.
  **Acceptance:** artifact records the parse result confirming the file exists, is well-formed
  Cobertura, and the `line-rate` is readable, with `Timestamp:`, `Command:`, `EXIT_CODE:`, and
  `Output Summary:` (R1 artifact-existence sub-claim resolved here).

### Phase 2 — Cobertura Validation and Coverage Consistency

- [x] [P2-T1] Parse the numeric coverage figures from `artifacts/csharp/coverage.xml`
  (`line-rate`, `lines-covered`, `lines-valid` at the relevant package/class scope) and write
  `evidence/regression-testing/coverage-xml-parse.2026-06-29T13-20.md`.
  **Acceptance:** artifact records the parsed numeric covered/valid line counts and percentage from
  the produced XML, with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P2-T2] Confirm the produced artifact is consistent with the existing feature evidence:
  233/233 QuickFiler.Test tests pass and the affected testable non-exempt denominator is 82.74%
  (484/585) as recorded in `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md` and
  `evidence/qa-gates/p8-tests-coverage.2026-06-29T12-40.md`. Record the generation command,
  `EXIT_CODE`, and the resulting `line-rate` in
  `evidence/qa-gates/canonical-coverage-consistency.2026-06-29T13-20.md`.
  **Acceptance:** artifact records the generation command, `EXIT_CODE:`, the resulting Cobertura
  `line-rate`, and an explicit `CONSISTENT: YES`/`CONSISTENT: NO` determination against the existing
  233/233 and 82.74% evidence, with `Timestamp:` and `Output Summary:`. A `CONSISTENT: NO` outcome is
  recorded as remediation-required, not PASS.

### Phase 3 — Final QA Verification Loop and Cycle Close

- [x] [P3-T1] Run formatting confirmation: `dotnet tool run csharpier check .`. Write
  `evidence/qa-gates/final-csharpier.2026-06-29T13-20.md`.
  **Acceptance:** `EXIT_CODE: 0` (no source `.cs` was modified, so format must be clean) with
  `Timestamp:`, `Command:`, and `Output Summary:`. If any file changes, restart the loop from P3-T1.

- [x] [P3-T2] Run analyzer build:
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Write `evidence/qa-gates/final-analyzers.2026-06-29T13-20.md`.
  **Acceptance:** `EXIT_CODE: 0` with `Timestamp:`, `Command:`, and `Output Summary:`.

- [x] [P3-T3] Run nullable/type-check build:
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  Write `evidence/qa-gates/final-nullable.2026-06-29T13-20.md`.
  **Acceptance:** `EXIT_CODE: 0` with `Timestamp:`, `Command:`, and `Output Summary:`.

- [x] [P3-T4] Run the coverage-enabled test gate:
  `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`.
  Confirm 233/233 tests pass and the affected testable non-exempt denominator remains 82.74%
  (484/585), unchanged by the artifact-generation cycle. Write
  `evidence/qa-gates/final-tests-coverage.2026-06-29T13-20.md`.
  **Acceptance:** artifact records the test pass/fail counts (expected 233/233, no test
  removed/weakened per G3), the numeric affected testable non-exempt coverage value (82.74%),
  `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`.

- [x] [P3-T5] Cycle-close verification: confirm no evidence artifact was written to a forbidden
  `artifacts/` evidence path (only `artifacts/csharp/coverage.xml` is permitted), confirm the
  worktree contains exactly the expected new artifacts (the canonical coverage XML plus the evidence
  markdown under the feature folder), confirm no `.cs`/`.csproj` file changed (G1/G3), and record the
  finding-to-task traceability summary. Write
  `evidence/qa-gates/final-cycle-close.2026-06-29T13-20.md`.
  **Acceptance:** artifact confirms no forbidden evidence path was used, no source change occurred,
  lists the produced artifacts, and includes the traceability map (below) with `Timestamp:` and
  `Output Summary:`.

---

## Finding-to-Task Traceability

| Source finding | Description | Remediating tasks |
|---|---|---|
| R1 (Blocking, FAIL) | Canonical `artifacts/csharp/coverage.xml` absent for a changed C# language | P0-T2 (defect baseline); P1-T1 → P1-T5 (build, source decision, collect, convert, verify); P2-T1 → P2-T2 (parse + consistency) |
| R1 no-regression sub-claim | Artifact generation must not change source or test outcome | P0-T3 (no-code-change baseline); P3-T1 → P3-T4 (green four-step toolchain, 233/233); P3-T5 (cycle close) |
| R2 (Blocking, governance) | Exemption-boundary maintainer ratification | NOT in this plan — escalated to maintainer (Dan Moisan) |
| R3 (deferred) | AC5 ≥90% new/extracted sub-target residual | NOT in this plan — deferred to #197 |

## Coverage Evidence Contract Compliance

- Baseline numeric coverage headline captured: P0-T4 (233/233 tests; affected testable non-exempt
  484/585 = 82.74%; per-cluster figures).
- Post-remediation numeric figure captured: P2-T1, P2-T2, P3-T4 (Cobertura `line-rate` parsed and
  reconciled against the existing 82.74% evidence).
- Consistency determination task with explicit YES/NO outcome: P2-T2 (CONSISTENT: YES/NO).
- Canonical machine-readable artifact produced: P1 (`artifacts/csharp/coverage.xml`).
- If the produced coverage is inconsistent with the existing evidence or the artifact cannot be
  produced, the cycle outcome is remediation-required, never a silent PASS.

## Preflight and Validation Status

- This plan must pass `mcp__drm-copilot__validate_orchestration_artifacts`
  (`artifact_type: "plan"`, `artifact_path:` this file) before `atomic-executor` runs preflight.
- Validator: NOT RUN by the planner (authoring step). Structural self-check performed: canonical
  `### Phase N — <Title>` headings (no token between `Phase N` and the em-dash), sequential
  `[P#-T#]` IDs per phase, all evidence paths under
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/` with the
  single permitted exception `artifacts/csharp/coverage.xml`, no forbidden `artifacts/` evidence
  paths.
- Preflight directive for handoff: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`. Expected signal:
  `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED`. The planner does not self-approve.
- Plan-path continuity: this exact file (`remediation-plan.2026-06-29T13-20.md`) is updated in place
  across any preflight revision iterations; no timestamped sibling plan files are created this cycle.
