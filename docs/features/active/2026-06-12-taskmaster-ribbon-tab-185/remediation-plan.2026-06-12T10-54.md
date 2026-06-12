# taskmaster-ribbon-tab — Remediation Plan (Issue #185)

- **Issue:** #185
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/185
- **Feature folder:** docs/features/active/2026-06-12-taskmaster-ribbon-tab-185
- **Cycle Entry Timestamp:** 2026-06-12T10-54
- **Cycle-entry inputs artifact:** docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/remediation-inputs.2026-06-12T10-54.md
- **Base Branch:** main (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
- **Head Branch:** TaskMaster-wt-2026-06-12-10-29 (`9db230d50a49bf4831174f2d4aef8bec624b5358`)
- **Owner:** drmoisan
- **Last Updated:** 2026-06-12T10-54
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit

## Scope

This is a remediation cycle that addresses a single BLOCKING finding (R1) plus two non-blocking
findings (R2 MINOR, R3 INFO) from the feature-review at timestamp `2026-06-12T10-54`. The
remediation produces the mandatory repository-wide C# coverage evidence artifact; it does NOT
add production code, modify ribbon content, or change any policy threshold.

In-scope branch diff files (from cycle-entry inputs):
- Production: `TaskMaster/Ribbon/RibbonExplorer.xml` (single-line, non-compiled XML resource edit; no instrumentable IL)
- Test: `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` (added MSTest tests)

Artifact to produce: `artifacts/csharp/coverage.xml` (Cobertura format; root `<coverage line-rate=...>`, per-line `<line number= hits=>`).

## Requirements Source (minor-audit)

- Sole requirements source: `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/issue.md`
- Acceptance Criteria source: the explicit `## Acceptance Criteria` section of `issue.md` (AC1–AC5).
- No `spec.md` or `user-story.md` is required or expected for this mode. If either file appears in
  the active folder, execution/validation/audit must fail closed.

### Remediation Findings (from cycle-entry inputs)

- R1 (BLOCKING): Canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent. The
  branch diff contains two C# files, so a repository-wide C# coverage artifact is mandatory for the
  coverage gate to be evaluable. The artifact must be Cobertura XML.
- R2 (MINOR): `artifacts/pr_context.summary.txt` misclassifies C# scope ("Core logic changes: 0
  files") and omits the two in-scope C# files. Regenerate PR context artifacts.
- R3 (INFO): The nullable build exits 1 with 84 pre-existing vendored errors (SVGControl 68,
  UtilitiesSwordfish 16). No remediation for #185; documented baseline, excluded per
  `.claude/rules/csharp.md`.

### Verified Facts (do not re-litigate)

- The in-scope production change is a non-compiled XML resource edit with no instrumentable IL;
  there is no changed-line coverage regression possible.
- Repo-wide C# coverage is produced by running ALL `*.Test.dll` assemblies (under `bin/Debug`,
  excluding `obj`/`ref`) with vstest `/EnableCodeCoverage /InIsolation`, then converting/merging the
  resulting `.coverage` to Cobertura at the canonical path `artifacts/csharp/coverage.xml`.
- The full-assembly Moq binding-redirect failure (System.Threading.Tasks.Extensions) that breaks
  plain vstest runs does NOT occur under `/EnableCodeCoverage`; the coverage run is expected to
  succeed.
- Repo-wide C# line coverage may sit below 80% due to documented pre-existing COM/VSTO/WinForms
  conditions. The reviewer owns the final PASS/FAIL coverage judgment based on change-scope gates.
  Producing the artifact makes the gate evaluable. This plan does NOT weaken, skip, or reword any
  coverage threshold.

## Required References

All work must comply with these repository policies (do not duplicate their content here):

- `CLAUDE.md` (standing instructions)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/ci-workflows.md`
- `.claude/rules/tonality.md`

## Evidence Location Invariant

All evidence artifacts produced by this plan MUST be written under the canonical tree
`docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/<kind>/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Writing to `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/coverage/`, `artifacts/evidence/`, or any other
non-canonical evidence path is a policy violation. The canonical C# coverage artifact path
`artifacts/csharp/coverage.xml` is a tool output path mandated by the feature-review coverage gate
and is NOT an evidence-tree artifact; the human-readable evidence record of that run is written to
`evidence/qa-gates/`.

---

### Phase 0 — Baseline Capture and Policy Read

- [x] [P0-T1] Read the repository policy files in the required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/ci-workflows.md`, `.claude/rules/tonality.md`) and the cycle-entry inputs artifact `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/remediation-inputs.2026-06-12T10-54.md`; write a Phase 0 read-evidence artifact to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/remediation-baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Normalize the acceptance-criteria heading in `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/issue.md` from `## Acceptance Criteria (early draft)` to the canonical `## Acceptance Criteria` (permitted minor-audit requirements-source normalization; change the heading text only). The AC1–AC5 checkbox item text directly under that heading MUST remain byte-for-byte unchanged. Binary acceptance condition: the file contains exactly the heading line `## Acceptance Criteria` (no trailing parenthetical) AND the AC1–AC5 checkbox item text is unchanged from before the edit. Record the result in `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/remediation-baseline/phase0-ac-heading-normalization.md` with fields `Timestamp:`, `Command/Action:`, `EXIT_CODE:` (or `N/A`), and `Output Summary:` (heading before/after and confirmation that AC item text is unchanged).
- [x] [P0-T3] Confirm minor-audit preconditions: verify `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/issue.md` contains the exact canonical heading `## Acceptance Criteria` (as normalized in P0-T2) with AC1–AC5 present as checkbox items, and that neither `spec.md` nor `user-story.md` exists in the active folder; fail closed only if the exact canonical heading is absent. Record the result (`PASS` or `FAIL-CLOSED` with reason) in `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/remediation-baseline/phase0-mode-precondition.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T4] Capture the baseline state of the canonical coverage artifact by checking whether `artifacts/csharp/coverage.xml` exists at branch head; write `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/remediation-baseline/baseline-coverage-artifact-presence.md` with fields `Timestamp:`, `Command:` (e.g., `Test-Path artifacts/csharp/coverage.xml`), `EXIT_CODE:`, and `Output Summary:` stating present/absent (expected: absent, confirming R1).
- [x] [P0-T5] Enumerate the set of repo-wide test assemblies to instrument by listing all `*.Test.dll` files under `bin/Debug` excluding `obj` and `ref` paths; write the resolved assembly list to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/remediation-baseline/baseline-test-assembly-set.md` with fields `Timestamp:`, `Command:` (the discovery command), `EXIT_CODE:`, and `Output Summary:` listing each resolved assembly path.

### Phase 1 — Produce Repository-Wide C# Coverage Evidence

- [x] [P1-T1] Run the repository-wide C# coverage collection over all `*.Test.dll` assemblies enumerated in P0-T5 using vstest with `/EnableCodeCoverage /InIsolation`, producing the raw `.coverage` output; record the exact command, assembly arguments, and `EXIT_CODE` in `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/repo-wide-coverage-run.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail of the run and path to the produced `.coverage` file).
- [x] [P1-T2] Convert/merge the `.coverage` output from P1-T1 to Cobertura XML at the canonical tool path `artifacts/csharp/coverage.xml` using the repo-standard conversion (e.g., `dotnet-coverage merge -f cobertura -o artifacts/csharp/coverage.xml <coverage files>`); verify the produced file exists and that its root element is `<coverage line-rate=...>` with per-line `<line number= hits=>` entries; record the conversion command and result in `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/repo-wide-coverage-convert.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (Cobertura validity confirmation and resolved root `line-rate`).
- [x] [P1-T3] Extract and record the repository-wide coverage interpretation by reading `artifacts/csharp/coverage.xml`: report the root `line-rate` as a percentage and the per-file coverage for the two in-scope C# files (`TaskMaster/Ribbon/RibbonExplorer.xml` — note as non-instrumentable non-compiled resource with no IL; `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`); write `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/repo-wide-coverage.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` capturing the repo-wide `line-rate` percent and the in-scope changed-file coverage interpretation (no changed-line regression is possible per verified facts; do not adjust any threshold).
- [x] [P1-T4] Regenerate PR context artifacts per `pr-context-artifacts` so the changed-files overview lists `RibbonExplorer.xml` and `RibbonExplorerXmlTests.cs` (addresses R2 MINOR); confirm both files appear in the "Changed files overview" section and record the regeneration command and confirmation in `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/pr-context-regen.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (both C# files present yes/no).

### Phase 2 — Final QA Loop

- [x] [P2-T1] Run formatting: `dotnet tool run csharpier .` (or `csharpier .`); confirm no `*.cs` files were reformatted; record `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/remediation-final-csharpier.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If any file is reformatted, restart the loop from this task.
- [x] [P2-T2] Run analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; record `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/remediation-final-analyzers.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and diagnostic count).
- [x] [P2-T3] Run nullable/type-check: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; record `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/remediation-final-nullable.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The pre-existing vendored failures in `SVGControl` and `UtilitiesSwordfish` are documented baseline (R3 INFO) and are NOT to be remediated; the summary must classify any errors as pre-existing-vendored vs in-scope and confirm zero in-scope nullable errors.
- [x] [P2-T4] Run tests with coverage over the repo-wide `*.Test.dll` assembly set: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation`; record `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/remediation-final-tests.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric post-change repo-wide line-coverage percent and the in-scope changed-file coverage (cross-referencing P1-T3). This command must execute; `SKIPPED` is not a valid outcome.
- [x] [P2-T5] Verify the QA loop completed a single clean pass with no files changed by P2-T1 through P2-T4 (excluding the documented R3 vendored nullable baseline in P2-T3); write a final reconciliation artifact `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/remediation-final-summary.md` with fields `Timestamp:`, `Output Summary:` confirming: R1 resolved (canonical Cobertura `artifacts/csharp/coverage.xml` produced with recorded repo-wide `line-rate`), R2 resolved (PR context lists both C# files), R3 acknowledged as documented baseline (no action), AC4 verbatim preservation untouched (no edit to `RibbonExplorer.xml`), and no coverage threshold was weakened, skipped, or reworded.

---

## Do Not Do (carried from cycle-entry inputs)

- Do not modify `RibbonExplorer.xml` group/control content; AC4 (verbatim preservation) must remain satisfied.
- Do not weaken, skip, or reword any coverage policy threshold to make the gate pass.
- Do not relocate evidence outside the canonical `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/<kind>/` tree.
- Do not expand scope beyond producing the mandatory C# coverage evidence and regenerating PR context.
- Do not touch the vendored projects (`SVGControl`, `UtilitiesSwordfish`) to silence pre-existing nullable errors (R3 INFO).

## Handoff

Per `remediation-handoff-atomic-planner`, this plan was authored by `atomic-planner` from the
cycle-entry inputs artifact `remediation-inputs.2026-06-12T10-54.md`. Control returns to the
orchestrator, which runs `validate_orchestration_artifacts` (`artifact_type: "plan"`) and routes
preflight validation to `atomic-executor`. The planner does not self-validate or run preflight.
