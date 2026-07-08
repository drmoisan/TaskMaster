# taskmaster-ribbon-tab - Plan (Issue #185)

- **Issue:** #185
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/185
- **Feature folder:** docs/features/active/2026-06-12-taskmaster-ribbon-tab-185
- **Owner:** drmoisan
- **Last Updated:** 2026-06-12T10-32
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit

## Scope

Single-file ribbon XML change plus its regression test:

- Production: `TaskMaster/Ribbon/RibbonExplorer.xml`
- Test: `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`

No other files are in scope. `TabFolder` and `TabTasks` are out of scope and must not be touched.

## Requirements Source (minor-audit)

- Sole requirements source: `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/issue.md`
- Acceptance Criteria source: the explicit `## Acceptance Criteria (early draft)` section of `issue.md` (AC1–AC5).
- No `spec.md` or `user-story.md` is required or expected for this mode. If either file appears in the active folder, execution/validation/audit must fail closed.

### Acceptance Criteria (mirrored from issue.md)

- AC1: A new custom tab declared with an `id` attribute and `label="Taskmaster"` exists in `RibbonExplorer.xml`.
- AC2: The four groups `SpamBayesGroup`, `Group2`, `TriageGroup`, and `UtilitiesGroup` are children of the new Taskmaster tab.
- AC3: The `<tab idMso="TabMail">` element no longer contains any custom group (removed or emptied so no custom group remains on the Mail tab).
- AC4: Every control id, `onAction`/`getPressed`/`getText`/`getLabel` callback, `imageMso`, `label`, `keytip`, and menu nesting is preserved unchanged from the original groups.
- AC5: `RibbonExplorer.xml` remains well-formed and schema-valid; existing `RibbonExplorerXmlTests` pass and a new regression test asserts the Taskmaster tab placement.

## Required References

All work must comply with these repository policies (do not duplicate their content here):

- `CLAUDE.md` (standing instructions)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`

## Evidence Locations (canonical, non-overridable)

All evidence artifacts use `<FEATURE>/evidence/<kind>/` under
`docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/`:

- Baseline: `evidence/baseline/`
- Regression / targeted verification: `evidence/regression-testing/`
- Final QA gates: `evidence/qa-gates/`

Non-canonical paths (e.g., `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) are forbidden.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture

- [x] [P0-T1] Record Phase 0 policy-read evidence to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/baseline/phase0-instructions-read.md` after reading, in order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md`.
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and an explicit list of the four files read.
- [x] [P0-T2] Capture baseline CSharpier format check by running `dotnet tool run csharpier --check .` and writing results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/baseline/baseline-csharpier.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (pass/fail and any unformatted file count).
- [x] [P0-T3] Capture baseline analyzer build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and writing results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/baseline/baseline-analyzers.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (build pass/fail and warning/error counts).
- [x] [P0-T4] Capture baseline nullable/type-check build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` and writing results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/baseline/baseline-nullable.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (build pass/fail).
- [x] [P0-T5] Capture baseline test + coverage by running `vstest.console.exe` against the `TaskMaster.Test` assembly with `/EnableCodeCoverage` and writing results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/baseline/baseline-tests.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including baseline pass/fail counts and the numeric coverage headline (repository line-coverage percent).

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] In `TaskMaster/Ribbon/RibbonExplorer.xml`, add a new custom tab element `<tab id="TabTaskMaster" label="Taskmaster" insertAfterMso="TabMail">` inside `<tabs>` (exact insert position is an implementation detail; the binding requirement is a custom id+label tab named "Taskmaster").
  - Acceptance: a `<tab>` with an `id` attribute and `label="Taskmaster"` (and no `idMso`) exists in the document. Satisfies AC1.
- [x] [P1-T2] Move the four groups `SpamBayesGroup`, `Group2`, `TriageGroup`, and `UtilitiesGroup` verbatim from inside `<tab idMso="TabMail">` into the new `<tab id="TabTaskMaster">`, preserving every child element, control `id`, `onAction`/`getPressed`/`getText`/`getLabel`/`onChange` callback, `imageMso`, `label`, `keytip`, `size`, `itemSize`, comments, and menu nesting unchanged.
  - Acceptance: the four group elements are children of the Taskmaster tab with byte-equivalent inner content versus the pre-change `TabMail` groups (no control id, callback, image, label, keytip, or nesting altered). Satisfies AC2 and AC4.
- [x] [P1-T3] Remove the now-empty `<tab idMso="TabMail">` element from `TaskMaster/Ribbon/RibbonExplorer.xml`.
  - Acceptance: no `<tab idMso="TabMail">` element remains, and no custom group is present on the Mail tab. Satisfies AC3.
- [x] [P1-T4] In `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, add an MSTest `[TestMethod]` using FluentAssertions that asserts each of `SpamBayesGroup`, `Group2`, `TriageGroup`, and `UtilitiesGroup` resolves as a descendant `group` of a `tab` whose `label` attribute equals "Taskmaster".
  - Acceptance: new test method exists, uses FluentAssertions, follows Arrange–Act–Assert, and passes against the changed XML. Contributes to AC5.
- [x] [P1-T5] In `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, add an MSTest `[TestMethod]` using FluentAssertions that asserts `<tab idMso="TabMail">` carries no custom `group` (assert TabMail is absent, or present with zero `group` children).
  - Acceptance: new test method exists, uses FluentAssertions, follows Arrange–Act–Assert, and passes against the changed XML. Contributes to AC5.
- [x] [P1-T6] Record targeted verification evidence to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/regression-testing/targeted-verification.md` by running `vstest.console.exe` filtered to `RibbonExplorerXmlTests` (the two new tests plus `RibbonExplorerXml_IsWellFormedXml` and `RibbonExplorerXml_MenusContainOnlyMenuLegalControls`).
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` confirming the two new tests and the two pre-existing `RibbonExplorerXmlTests` all pass. Confirms AC5.

### Phase 2 — Final QC Loop

Run the full C# toolchain in order. If any step changes files or fails, fix and restart from P2-T1.

- [x] [P2-T1] Run `dotnet tool run csharpier .` and write results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/final-csharpier.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; formatting clean with no residual diffs.
- [x] [P2-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/final-analyzers.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; build passes with no analyzer errors.
- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/final-nullable.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; build passes with warnings-as-errors enabled.
- [x] [P2-T4] Run `vstest.console.exe` against the `TaskMaster.Test` assembly with `/EnableCodeCoverage` and write results to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/final-tests.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including post-change pass/fail counts and the numeric coverage headline.
- [x] [P2-T5] Record coverage delta to `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/coverage-delta.md` comparing baseline (P0-T5) and post-change (P2-T4) coverage.
  - Acceptance: artifact reports baseline coverage percent, post-change coverage percent, and changed-code coverage; repository line coverage remains `>= 80%` and changed lines show no coverage regression. If required coverage values are unavailable, outcome is remediation-required, not PASS.

## Test Plan

- Unit: two new MSTest methods in `RibbonExplorerXmlTests.cs` — (1) the four groups resolve under the Taskmaster custom tab; (2) `TabMail` carries no custom groups.
- Regression: existing `RibbonExplorerXml_IsWellFormedXml` and `RibbonExplorerXml_MenusContainOnlyMenuLegalControls` must pass unchanged.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-tests.md`
  - Post-change: `evidence/qa-gates/final-tests.md`
  - Comparison: `evidence/qa-gates/coverage-delta.md`

## Open Questions / Notes

- The exact `insertAfterMso`/insert position of the new tab is an executor implementation detail; only the custom id+label "Taskmaster" tab requirement is binding.
- Control `id` values must remain globally unique across `customUI`; the verbatim move preserves existing ids and introduces no new control ids.
