# ribbon-controller-engines-null-unsafe (Plan)

- **Issue:** #507
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08T15-24
- **Status:** Ready for Preflight
- **Version:** 0.2
- **Work Mode:** minor-audit
- **Directive:** MINIMAL-AUDIT PLAN REQUIRED

## Requirements Source

`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md` is the
sole requirements source. Its `## Acceptance Criteria` section (AC1-AC6) is the only
acceptance-criteria source for this plan. `spec.md`, `user-story.md`, and `research.md` are
intentionally absent and are not required for minor-audit mode.

## Hard Scope Boundary

- In-scope files (the only two files any task may modify):
  - `TaskMaster/Ribbon/RibbonController.Intelligence.cs` (production)
  - `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (test)
- `TaskMaster/Ribbon/RibbonViewer.cs` MUST NOT be modified. Issues #505
  (`ribbon-async-getpressed-signature`) and #506 (`ribbon-toggle-engine-fire-and-forget`) are
  deliberately deferred to a separate feature that lands after `bug/ribbon-engine-readiness-guard-503`
  merges. If either defect is observed during execution, leave it alone and do not touch
  `RibbonViewer.cs`. Phase 2 includes an explicit guard task verifying this.
- `RibbonController` carries `[ExcludeFromCodeCoverage]` under the ratified VSTO/COM ribbon-handler
  coverage exemption (`TaskMaster/Ribbon/RibbonController.cs:36`). This change adds no coverage
  surface; no task may remove, widen, or work around that attribute, and no new-code coverage target
  applies to this class. The coverage obligation in this plan is limited to recording the repo-wide
  coverage headline at baseline (Phase 0) and at final QC (Phase 2) and confirming no regression.

## MSTest Discovery Caveat (apply to every `vstest.console.exe` task)

When globbing for `*.Test.dll`, exclude any path containing `\.claude\`. The repository has
approximately 20 stale `.claude/worktrees/agent-*` worktrees whose old builds are otherwise
discovered and produce bogus `AssemblyInitialize` signature failures. Every task below that runs
`vstest.console.exe` must apply this exclusion and record that it did so in its evidence artifact.

## Evidence Location

All evidence artifacts resolve under
`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/<kind>/`
using `<kind>` in `{baseline, regression-testing, qa-gates, issue-updates, other}`. No task may
write evidence to any `artifacts/...` path. Every command-step artifact must include `Timestamp:`,
`Command:`, `EXIT_CODE:`, and `Output Summary:`.

---

### Phase 0 — Baseline capture

- [x] [P0-T1] Read, in order, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
  `.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md`; write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/other/phase0-instructions-read.md`
  containing `Timestamp:`, `Policy Order:` (the four files in the order read), and the explicit
  list of files read. Acceptance: the artifact exists with all three required fields populated.

- [x] [P0-T2] Run baseline command `csharpier .` from the repo root (workspace
  `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7e887d12b262219`). Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-csharpier.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (state whether any file was
  reformatted). Acceptance: the artifact exists with all four required fields populated.

- [x] [P0-T3] Run baseline command
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-msbuild-analyzers.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (analyzer diagnostic count).
  Acceptance: the artifact exists with all four required fields populated.

- [x] [P0-T4] Run baseline command
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-msbuild-nullable.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (build result, warning/error
  count). Acceptance: the artifact exists with all four required fields populated.

- [x] [P0-T5] Run baseline command `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
  where `<test-assembly-paths>` is every `*.Test.dll` under the workspace excluding any path
  containing `\.claude\` (per the MSTest Discovery Caveat above). Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-vstest-coverage.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` containing the numeric
  repo-wide line-coverage headline percentage and the total pass/fail/skip test counts. Acceptance:
  the artifact exists with all four required fields populated and the coverage headline and
  pass/fail counts are numeric (not placeholders).

- [x] [P0-T6] In
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md`, check off
  `- [ ] baseline` to `- [x] baseline` under `## Evidence Checklist`, citing the P0-T1 through P0-T5
  artifact paths in an adjacent note. Acceptance: the checkbox is `[x]` and the citation is present.

### Phase 1 — Constrained small-path implementation

- [x] [P1-T1] [expect-fail] In `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`, add test method
  `Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing` (AC1, AC3) that constructs a bare
  `new RibbonController()` (does not call `CreateController()` and does not set `Globals`), reads
  `controller.Engines` inside a FluentAssertions non-throwing assertion, and asserts the result is
  `null`. Acceptance: the file compiles and the new test method is present.

- [x] [P1-T2] [expect-fail] Run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
  /TestCaseFilter:"FullyQualifiedName~Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing"`
  against the current pre-fix source, with `<test-assembly-paths>` excluding any path containing
  `\.claude\` (per the MSTest Discovery Caveat above). Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/regression-testing/phase1-expect-fail-engines-unassigned.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` showing the test failed with
  `NullReferenceException`. Acceptance: the artifact exists, all four fields populated, and
  `Output Summary:` documents a failing result.

- [x] [P1-T3] In `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`, add test method
  `Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines` (AC4) that builds a controller via the
  existing `CreateController()` helper, then assigns a distinguishable
  `Mock<IAppItemEngines>.Object` onto the controller's `Globals.Engines`
  (`ApplicationGlobals.Engines` is `public ... { get; private set; }`, so set it through the same
  reflection approach `CreateController()` already uses for `_quickFilerSettings`), and asserts
  `controller.Engines` is reference-equal to that mock instance. The assertion must prove the
  property forwards the assigned value, not merely that both sides are `null`. Acceptance: the file
  compiles, the new test method is present, and it fails if the property stops forwarding.

- [x] [P1-T4] In `TaskMaster/Ribbon/RibbonController.Intelligence.cs` line 204, change
  `internal IAppItemEngines Engines => Globals.Engines;` to
  `internal IAppItemEngines Engines => Globals?.Engines;` (AC1, AC2, AC4). No other line in the
  file changes. Acceptance: the file compiles and line 204 matches the null-conditional form
  exactly.

- [x] [P1-T5] Run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
  /TestCaseFilter:"FullyQualifiedName~Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing|FullyQualifiedName~Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines"`
  post-fix, with `<test-assembly-paths>` excluding any path containing `\.claude\` (per the MSTest
  Discovery Caveat above). Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/regression-testing/phase1-post-fix-engines-tests.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` showing both tests passed.
  Acceptance: the artifact exists, all four fields populated, and `Output Summary:` documents two
  passing tests.

- [x] [P1-T6] In
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md`, check off
  AC1, AC3, and AC4 under `## Acceptance Criteria` and check off `- [ ] targeted verification` to
  `- [x] targeted verification` under `## Evidence Checklist`, citing the P1-T1 through P1-T5
  artifact paths in an adjacent note. Acceptance: all three AC checkboxes and the checklist item
  are `[x]` with the citation present.

### Phase 2 — Final QC loop

- [x] [P2-T1] Run final command `csharpier .`. If it reformats any file, discard this pass and
  restart the Phase 2 loop from this task. Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-final-csharpier.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` confirming `EXIT_CODE: 0` and
  zero files reformatted. Acceptance: the artifact exists, all four fields populated, `EXIT_CODE: 0`.

- [x] [P2-T2] Run final command
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  If it fails, fix and restart the Phase 2 loop from P2-T1. Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-final-msbuild-analyzers.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` confirming `EXIT_CODE: 0`.
  Acceptance: the artifact exists, all four fields populated, `EXIT_CODE: 0`.

- [x] [P2-T3] Run final command
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  If it fails, fix and restart the Phase 2 loop from P2-T1. Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-final-msbuild-nullable.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` confirming `EXIT_CODE: 0`.
  Acceptance: the artifact exists, all four fields populated, `EXIT_CODE: 0`.

- [x] [P2-T4] Run final command `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
  where `<test-assembly-paths>` is every `*.Test.dll` under the workspace excluding any path
  containing `\.claude\` (per the MSTest Discovery Caveat above). If any test fails, fix and
  restart the Phase 2 loop from P2-T1. Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-final-vstest-coverage.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` containing the numeric
  post-change repo-wide line-coverage headline percentage and total pass/fail/skip test counts.
  Acceptance: the artifact exists, all four fields populated, `EXIT_CODE: 0`, and the coverage
  headline and pass/fail counts are numeric (not placeholders).

- [x] [P2-T5] Compare the P0-T5 baseline coverage headline and pass/fail counts against the P2-T4
  post-change values; confirm the coverage headline did not regress and the pass/fail counts are
  no worse than baseline (AC6). Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-coverage-comparison.md`
  with `Timestamp:`, baseline coverage/pass-fail values, post-change coverage/pass-fail values, and
  an explicit no-regression confirmation. Acceptance: the artifact exists with both numeric value
  sets and an explicit no-regression statement.

- [x] [P2-T6] Run `git diff --name-only 003c5715055d7d1933db68a742531332756e30b2...HEAD` and confirm
  `TaskMaster/Ribbon/RibbonViewer.cs` does NOT appear in the output. Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-ribbonviewer-guard.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` listing the diff file names and
  confirming `RibbonViewer.cs` is absent. Acceptance: the artifact exists, all four fields
  populated, and confirms `RibbonViewer.cs` is not in the diff.

- [x] [P2-T7] Run `git status --porcelain` and
  `git diff --name-only 003c5715055d7d1933db68a742531332756e30b2...HEAD` and confirm that the only
  changed files with a `.cs`, `.csproj`, `.props`, `.targets`, or `.sln` extension are
  `TaskMaster/Ribbon/RibbonController.Intelligence.cs` and
  `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (AC2). Changed files under
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/` (issue, plan, and
  evidence artifacts) and under `artifacts/orchestration/` are expected audit-trail output and are
  NOT scope violations; list them separately in the artifact rather than flagging them. Any other
  changed path is a scope violation and must be reverted before this task passes. Write
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-git-status-scope-check.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` listing every changed file and
  confirming no file outside the two in-scope files appears. Acceptance: the artifact exists, all
  four fields populated, and confirms exactly the two in-scope files changed.

- [x] [P2-T8] In
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md`, check off
  AC2, AC5, and AC6 under `## Acceptance Criteria` and check off `- [ ] end-state` to
  `- [x] end-state` under `## Evidence Checklist`, citing the P2-T1 through P2-T7 artifact paths in
  an adjacent note. Acceptance: all three AC checkboxes and the checklist item are `[x]` with the
  citation present, and all six AC1-AC6 checkboxes in the file are now `[x]`.
