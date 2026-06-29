# Remediation Plan — qfc-form-viewer-testability (#223), Cycle 1

- **Cycle entry timestamp:** 2026-06-28T21-30
- **Feature folder:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
- **Base branch:** `main` (merge-base `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
- **Head:** `e91927105abde2ceadd10a7011bc17d714108afd`
- **Work Mode:** `full-feature` (resolved from `issue.md` metadata block)
- **Plan author:** atomic-planner
- **Remediation inputs (authoritative):** `remediation-inputs.2026-06-28T21-30.md`
- **Source audits:** `policy-audit.2026-06-28T21-30.md`, `code-review.2026-06-28T21-30.md`, `feature-audit.2026-06-28T21-30.md`

## Scope Statement (single root cause)

Two blocking findings share one cause: the canonical Cobertura C# coverage artifact
`artifacts/csharp/coverage.xml` was never generated, so the repo-wide first-party
(testable-denominator) `>= 80%` floor is unmeasured. No production-code or test change is
required or permitted by these findings — the refactor itself already passed all four
toolchain gates (196/196 tests) and 6 of 7 ACs. This plan generates the canonical artifact,
measures and records the repo-wide first-party testable-denominator figure, confirms it
against the `>= 80%` floor, and re-checks AC5 on confirmation.

## Guardrails (encoded from the do-not-do list)

- G1. Do NOT split `QuickFiler/Controllers/QfcCollectionController.cs` or
  `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`.
- G2. Do NOT modify any file under `.claude/rules/**` or `CLAUDE.md`, and do NOT weaken any
  coverage threshold or `[ExcludeFromCodeCoverage]` exemption to make the floor pass.
- G3. Do NOT alter, weaken, delete, or add tests to move coverage numbers. No production or
  test `.cs` file is edited by this plan.
- G4. Do NOT narrow scope or mark C# coverage "informational only."
- G5. The ONLY non-`<FEATURE>/evidence/<kind>/` output path permitted by this plan is the
  canonical coverage artifact `artifacts/csharp/coverage.xml`. Every other artifact this plan
  produces is written under `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/<kind>/`.

## Evidence Location Invariant

All evidence artifacts resolve to
`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/<kind>/`
(`remediation-baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`).
The single canonical-but-non-evidence path `artifacts/csharp/coverage.xml` is mandated by the
coverage-verification contract and is explicitly permitted; it is the only such exception.
No `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`
path is used. No non-canonical evidence path was supplied by the caller, so no
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` entry is required.

## Contingency Model (deterministic, no open holes)

Coverage artifact acquisition has a known environment risk: local full-assembly C# coverage
previously failed on a Moq binding redirect. The plan defines two convergent paths and a
single decision task that selects exactly one:

- **PATH-LOCAL** — the bounded local run of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
  produces `artifacts/csharp/coverage.xml` directly.
- **PATH-CI** — if the bounded local attempt fails (e.g., the Moq binding redirect), the
  authoritative measurement is the PR CI coverage run. The CI `quality-gates` job already
  runs every first-party `*.Test.dll` with `/EnableCodeCoverage /InIsolation` and uploads the
  `.coverage` attachments as the `test-results` artifact. PATH-CI downloads that attachment
  and converts it to Cobertura at `artifacts/csharp/coverage.xml`. Instrumentation happens on
  the CI runner, so the local binding-redirect failure does not block measurement.

Both paths converge on the same Phase 2 measurement and Phase 4 final gate. A bounded attempt
means a single local run (no retries, sleeps, or timing hacks per policy); failure routes to
PATH-CI rather than reattempting.

The floor comparison in Phase 2 has two explicit outcomes:

- **FLOOR-PASS** (`>= 80%`) — AC5 is fully satisfied; Phase 3 re-checks AC5.
- **FLOOR-BELOW** (`< 80%` due to PRE-EXISTING first-party shortfall not introduced by this
  change) — Phase 2 records a precise, scoped finding for orchestrator escalation
  (authority-scoped exception decision). AC5 stays unchecked. The plan does not silently pass
  and does not weaken the gate.

---

### Phase 0 — Policy Reads and Remediation Baseline Capture

- [x] [P0-T1] Read the policy files in the required order (`CLAUDE.md`,
  `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
  `.claude/rules/csharp.md`, and the coverage skills `atomic-plan-contract`,
  `evidence-and-timestamp-conventions`, `remediation-handoff-atomic-planner`) and write
  `evidence/remediation-baseline/phase0-instructions-read.2026-06-28T21-30.md`.
  **Acceptance:** artifact exists and contains `Timestamp:`, `Policy Order:`, and an explicit
  list of every file read.

- [x] [P0-T2] Confirm the canonical coverage artifact is absent at cycle entry. Run
  `Test-Path artifacts/csharp/coverage.xml` and record the result in
  `evidence/remediation-baseline/baseline-canonical-artifact.2026-06-28T21-30.md`.
  **Acceptance:** artifact exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and
  `Output Summary:` recording `artifacts/csharp/coverage.xml` = ABSENT (the defect baseline).

- [x] [P0-T3] Confirm a Debug build of `TaskMaster.sln` exists so every first-party
  `*.Test.dll` is present under `**/bin/Debug/`. Enumerate the discovered first-party test
  assemblies (e.g., `UtilitiesCS.Test`, `QuickFiler.Test`, `ToDoModel.Test`,
  `TaskVisualization.Test`, `Tags.Test`, `TaskMaster.Test`, `VBFunctions.Test`) and write
  `evidence/remediation-baseline/baseline-test-assemblies.2026-06-28T21-30.md`.
  **Acceptance:** artifact lists each discovered first-party `*.Test.dll` path with
  `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T4] Capture coverage-tooling availability and the prior-cycle numeric coverage
  headline baseline. Record presence of `dotnet-coverage` (`Get-Command dotnet-coverage`) and
  `vstest.console.exe` (via `vswhere`), and record the known numeric coverage values carried
  from the prior cycle (QfcFormController changed-type 51.86%, new-code `QfcFormKeyHandler`
  100%, disclaimed single-assembly process-wide 12.86%, and the repo-wide first-party
  testable-denominator figure = UNMEASURED, which is the target of this remediation). Write
  `evidence/remediation-baseline/baseline-coverage-tooling.2026-06-28T21-30.md`.
  **Acceptance:** artifact records tool availability (present/absent) and the numeric coverage
  headline values above, with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T5] Record the local-vs-CI feasibility precondition and the evidence-location
  invariant for this cycle in
  `evidence/remediation-baseline/baseline-contingency-precondition.2026-06-28T21-30.md`.
  **Acceptance:** artifact states the bounded-local-attempt rule (single run), the PATH-CI
  fallback trigger (local coverage run fails, e.g., Moq binding redirect), and the single
  permitted non-evidence path `artifacts/csharp/coverage.xml`; includes `Timestamp:` and
  `Output Summary:`.

### Phase 1 — Canonical Coverage Artifact Acquisition

- [x] [P1-T1] Ensure a current Debug build so coverage instrumentation has fresh first-party
  `*.Test.dll` inputs. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` and write
  `evidence/qa-gates/p1-build.2026-06-28T21-30.md`.
  **Acceptance:** build `EXIT_CODE: 0` recorded with `Timestamp:`, `Command:`, and
  `Output Summary:`; no source `.cs` file is modified by this step.

- [x] [P1-T2] PATH-LOCAL bounded attempt: generate the canonical Cobertura artifact using the
  repo's established conversion path. Run
  `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput artifacts/csharp/coverage.xml`
  (single attempt; no retries). This auto-discovers all first-party `*.Test.dll`, runs
  `dotnet-coverage collect --output-format cobertura --settings coverage.config`, and
  post-processes to Koverage-compatible Cobertura. Write
  `evidence/qa-gates/p1-local-coverage-attempt.2026-06-28T21-30.md`.
  **Acceptance:** artifact records `Timestamp:`, exact `Command:`, `EXIT_CODE:`, and
  `Output Summary:` capturing pass/fail. On failure, the summary records the failure reason
  (e.g., Moq binding redirect) verbatim.

- [x] [P1-T3] Decision/branch task: read the P1-T2 outcome and select exactly one path. If
  `artifacts/csharp/coverage.xml` exists and parses as well-formed Cobertura with a readable
  repo-wide `line-rate`, select **PATH-LOCAL** and skip P1-T4/P1-T5. Otherwise select
  **PATH-CI** and execute P1-T4/P1-T5. Record the selection in
  `evidence/qa-gates/p1-acquisition-decision.2026-06-28T21-30.md`.
  **Acceptance:** artifact states `SELECTED_PATH: PATH-LOCAL` or `SELECTED_PATH: PATH-CI` with
  the deciding observation (artifact present+parseable, or the recorded local failure), plus
  `Timestamp:` and `Output Summary:`.

- [x] [P1-T4] PATH-CI only — confirm the PR is open and the CI `quality-gates` job has a green
  run on head commit `e91927105abde2ceadd10a7011bc17d714108afd`, then download the
  `test-results` artifact containing the `.coverage` attachment(s). Write
  `evidence/qa-gates/p1-ci-coverage-source.2026-06-28T21-30.md` with the CI run URL and the
  downloaded artifact path(s).
  **Acceptance (PATH-CI):** artifact records the green CI run URL, the `.coverage` file
  path(s) obtained, `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. If
  PATH-LOCAL was selected, this task is marked `EXIT_CODE: SKIPPED` per its explicit PATH-CI
  skip branch.

- [x] [P1-T5] PATH-CI only — convert the downloaded CI `.coverage` attachment to Cobertura at
  the canonical path. Run
  `dotnet-coverage merge -f cobertura -o artifacts/csharp/coverage.xml <downloaded .coverage path(s)>`,
  then apply the repo's Koverage post-processing (strip third-party packages, inject
  `<sources>`, rewrite to workspace-relative paths) consistent with
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Write
  `evidence/qa-gates/p1-ci-coverage-convert.2026-06-28T21-30.md`.
  **Acceptance (PATH-CI):** `artifacts/csharp/coverage.xml` exists and is well-formed
  Cobertura; artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If
  PATH-LOCAL was selected, this task is marked `EXIT_CODE: SKIPPED` per its explicit PATH-CI
  skip branch.

- [x] [P1-T6] Verify the canonical artifact regardless of path. Confirm
  `artifacts/csharp/coverage.xml` exists, parses as Cobertura, exposes a repo-wide
  `line-rate`, and contains first-party packages (third-party stripped). Write
  `evidence/qa-gates/p1-canonical-artifact-verified.2026-06-28T21-30.md`.
  **Acceptance:** artifact records `ls`/parse result confirming the file exists and the
  repo-wide `line-rate` is readable, with `Timestamp:`, `Command:`, `EXIT_CODE:`, and
  `Output Summary:` (Finding 1 artifact-existence sub-claim resolved here).

### Phase 2 — Repo-Wide First-Party Testable-Denominator Measurement and Floor Decision

- [x] [P2-T1] Parse the repo-wide first-party `line-rate`, `lines-covered`, and `lines-valid`
  from `artifacts/csharp/coverage.xml` across all first-party packages. Write
  `evidence/regression-testing/repo-wide-coverage-raw.2026-06-28T21-30.md`.
  **Acceptance:** artifact records the numeric repo-wide first-party covered/valid line counts
  and percentage, with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P2-T2] Apply the documented COM/VSTO/WinForms `[ExcludeFromCodeCoverage]` exemptions to
  the denominator and compute the testable-denominator figure. Confirm that
  `[ExcludeFromCodeCoverage]`-marked Form-derived/Designer/COM-host-bound classes are absent
  from the instrumented denominator (the collector honors the attribute), and document the
  testable-denominator covered/valid counts. Write
  `evidence/regression-testing/repo-wide-coverage-testable-denominator.2026-06-28T21-30.md`.
  **Acceptance:** artifact records the testable-denominator numeric figure and confirms the
  exemption boundary was applied (not weakened, per G2), with `Timestamp:` and
  `Output Summary:`.

- [x] [P2-T3] Compare the testable-denominator figure to the `>= 80%` floor and record the
  decision. Write the decision token to
  `evidence/qa-gates/repo-wide-floor-decision.2026-06-28T21-30.md`.
  **Acceptance:** artifact states `FLOOR_DECISION: FLOOR-PASS` (figure `>= 80%`) or
  `FLOOR_DECISION: FLOOR-BELOW` (figure `< 80%`), with the numeric figure, the `>= 80%`
  threshold, `Timestamp:`, and `Output Summary:`.

- [x] [P2-T4] Write the consolidated repo-wide coverage measurement evidence artifact at
  `evidence/qa-gates/repo-wide-coverage-measurement.2026-06-28T21-30.md`, referencing the
  canonical artifact `artifacts/csharp/coverage.xml`, the acquisition path (PATH-LOCAL or
  PATH-CI), the testable-denominator figure, and the floor decision.
  **Acceptance:** artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`
  with the numeric repo-wide figure and `FLOOR-PASS`/`FLOOR-BELOW` (Finding 1 measurement
  sub-claim resolved here).

- [x] [P2-T5] FLOOR-BELOW only — record a precise, scoped finding for orchestrator escalation
  (authority-scoped exception decision). Document the measured figure, the gap to 80%, the
  evidence that the shortfall is PRE-EXISTING (the changed/new lines meet their thresholds:
  new code 100%, changed type +12.62pp), and that this remediation introduced no regression.
  Write `evidence/other/repo-wide-floor-escalation-finding.2026-06-28T21-30.md`.
  **Acceptance (FLOOR-BELOW):** artifact records the scoped finding with numeric figures and a
  clear statement that the gate is NOT weakened and the decision is routed to the orchestrator;
  includes `Timestamp:` and `Output Summary:`. If `FLOOR-PASS`, this task is marked
  `EXIT_CODE: SKIPPED` per its explicit FLOOR-BELOW skip branch.

### Phase 3 — AC5 Re-Check and Issue Update

- [x] [P3-T1] FLOOR-PASS only — re-check AC5 in
  `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/issue.md` per the
  `acceptance-criteria-tracking` protocol, changing the AC5 checkbox from `[ ]` to `[x]` now
  that the repo-wide `>= 80%` sub-claim is verified.
  **Acceptance (FLOOR-PASS):** AC5 line in `issue.md` is `[x]`; the other six ACs are
  unchanged. If `FLOOR-BELOW`, this task is `EXIT_CODE: SKIPPED` per its explicit FLOOR-PASS
  skip branch and AC5 remains `[ ]`.

- [x] [P3-T2] FLOOR-PASS only — mirror the AC5 issue update under
  `evidence/issue-updates/issue-223.2026-06-28T21-30.md`.
  **Acceptance (FLOOR-PASS):** mirror artifact records the exact AC5 text now checked,
  `PostedAs:` (body or comment), the GitHub URL if posted, `Timestamp:`, and
  `IssueUpdatedAt:`. If `FLOOR-BELOW`, this task is `EXIT_CODE: SKIPPED`.

- [x] [P3-T3] FLOOR-BELOW only — leave AC5 unchecked and record the disposition referencing
  the escalation finding in
  `evidence/issue-updates/issue-223-ac5-deferred.2026-06-28T21-30.md`.
  **Acceptance (FLOOR-BELOW):** artifact records that AC5 stays `[ ]` pending the
  orchestrator's authority-scoped exception decision, references
  `evidence/other/repo-wide-floor-escalation-finding.2026-06-28T21-30.md`, and includes
  `Timestamp:` and `Output Summary:`. If `FLOOR-PASS`, this task is `EXIT_CODE: SKIPPED`.

### Phase 4 — Final QA Verification Loop and Cycle Close

- [x] [P4-T1] Run formatting confirmation: `dotnet tool run csharpier check .`. Write
  `evidence/qa-gates/final-csharpier.2026-06-28T21-30.md`.
  **Acceptance:** `EXIT_CODE: 0` (no source `.cs` was modified, so format must be clean) with
  `Timestamp:`, `Command:`, and `Output Summary:`. If any file changes, restart the loop from
  P4-T1.

- [x] [P4-T2] Run analyzer build:
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Write `evidence/qa-gates/final-analyzers.2026-06-28T21-30.md`.
  **Acceptance:** `EXIT_CODE: 0` with `Timestamp:`, `Command:`, and `Output Summary:`.

- [x] [P4-T3] Run nullable/type-check build:
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  Write `evidence/qa-gates/final-nullable.2026-06-28T21-30.md`.
  **Acceptance:** `EXIT_CODE: 0` with `Timestamp:`, `Command:`, and `Output Summary:`.

- [x] [P4-T4] Record the final coverage-enabled test gate. The authoritative coverage run is
  the artifact produced in Phase 1 (PATH-LOCAL local run, or PATH-CI runner run). Confirm the
  196/196 first-party test pass result and the numeric repo-wide first-party
  testable-denominator figure from `artifacts/csharp/coverage.xml`. Write
  `evidence/qa-gates/final-tests-coverage.2026-06-28T21-30.md`.
  **Acceptance:** artifact records the test pass/fail counts (expected 196/196, no test
  removed/weakened per G3), the numeric repo-wide testable-denominator coverage value, the
  `FLOOR-PASS`/`FLOOR-BELOW` decision, the acquisition path, `Timestamp:`, `Command:`,
  `EXIT_CODE:`, and `Output Summary:`.

- [x] [P4-T5] Cycle-close verification: confirm no evidence artifact was written to a forbidden
  `artifacts/` evidence path (only `artifacts/csharp/coverage.xml` is permitted), confirm the
  worktree contains exactly the expected new artifacts (canonical coverage XML, evidence
  markdown, and — on FLOOR-PASS — the AC5 `issue.md` re-check), and record the
  finding-to-task traceability summary. Write
  `evidence/qa-gates/final-cycle-close.2026-06-28T21-30.md`.
  **Acceptance:** artifact confirms no forbidden evidence path was used, lists the produced
  artifacts, and includes the traceability map (below) with `Timestamp:` and `Output Summary:`.

---

## Finding-to-Task Traceability

| Source finding | Description | Remediating tasks |
|---|---|---|
| Finding 1 (FAIL) | Canonical `artifacts/csharp/coverage.xml` absent; repo-wide first-party `>= 80%` floor unmeasured | P1-T1 → P1-T6 (artifact acquisition + verification); P2-T1 → P2-T4 (measurement + floor decision) |
| Finding 2 (blocking PARTIAL) | AC5 repo-wide coverage sub-claim unverified | P2-T3/P2-T4 (floor confirmation); P3-T1 (AC5 re-check on FLOOR-PASS); P2-T5/P3-T3 (escalation route on FLOOR-BELOW) |
| AC5 re-check | Re-check AC5 in `issue.md` per acceptance-criteria-tracking | P3-T1 (FLOOR-PASS) + P3-T2 (mirror); P3-T3 (FLOOR-BELOW deferral) |

## Coverage Evidence Contract Compliance

- Baseline numeric coverage headline captured: P0-T4 (prior-cycle 51.86%/100%/12.86% + repo-wide UNMEASURED target).
- Post-remediation numeric repo-wide testable-denominator figure captured: P2-T1 → P2-T4, P4-T4.
- Floor/threshold decision task with explicit PASS/BELOW outcomes: P2-T3 (and escalation P2-T5).
- Canonical machine-readable artifact produced: P1 (`artifacts/csharp/coverage.xml`).
- If the repo-wide figure is unavailable or below floor, the cycle outcome is
  remediation-required / escalation (P2-T5), never a silent PASS.

## Preflight and Validation Status

- This plan must pass `mcp__drm-copilot__validate_orchestration_artifacts`
  (`artifact_type: "plan"`, `artifact_path:` this file) before `atomic-executor` runs preflight.
- Validator: NOT RUN by the planner (authoring step). Structural self-check performed: canonical
  `### Phase N — <Title>` headings (no parenthetical qualifiers), sequential `[P#-T#]` IDs per
  phase, all evidence paths under
  `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/<kind>/` with the
  single permitted exception `artifacts/csharp/coverage.xml`, no forbidden `artifacts/` evidence
  paths.
- Preflight directive for handoff: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`. Expected signal:
  `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED`. The planner does not self-approve.
- Plan-path continuity: this exact file
  (`remediation-plan.2026-06-28T21-30.md`) is updated in place across any preflight revision
  iterations; no timestamped sibling plan files are created this cycle.
