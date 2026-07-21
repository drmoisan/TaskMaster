# people-tag-window-autotag (Plan)

- **Issue:** #322
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/322
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-12T11-36
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md` (`## Acceptance Criteria`, AC1-AC6)
- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-07-12-people-tag-window-autotag-322`
- **Timestamp token:** every `<TS>` placeholder below MUST be substituted with the real ISO-8601
  timestamp (`yyyy-MM-ddTHH-mm`) at the moment the artifact is written, per
  `evidence-and-timestamp-conventions`.

**Fail-closed evidence rule:** This plan includes explicit baseline artifact tasks, final-QA
artifact tasks, and a coverage no-regression task for the single in-scope language (C#). If any
required baseline, QA, or coverage-comparison artifact is missing, or its required fields
(`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`) are incomplete, the audit verdict must
be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task names its exact artifact path under
`docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/<kind>/`. Do not mark an
evidence-backed task complete without the artifact on disk.

## Requirements Boundary

This minor-audit plan uses only
`docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md` as the requirements
source. Acceptance criteria are limited to the checkbox items (AC1-AC6) under that file's explicit
`## Acceptance Criteria` section (confirmed present at lines 61-68). `spec.md` and `user-story.md`
are not required for minor-audit mode; if either is unexpectedly present in the feature folder, that
is a fail-closed condition and must be reported, not silently ignored.

All evidence must be written under
`docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/<kind>/`. No non-canonical
path (e.g. `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`) is
used anywhere in this plan.

## Confirmed Facts (from source inspection, recorded for the Phase 1 diagnosis task)

- `TaskVisualization/TaskController.Actions.cs:25-56` — `AssignPeople()` constructs a
  `TagPromptRequest` whose `objItemObject` argument (line 46) is `_active.OlItem.InnerObject` (the
  raw, unwrapped Outlook COM object).
- `TaskVisualization/TaskController.Actions.cs:61-91, 93-126, 131-161` — the three sibling methods
  `AssignContext` (line 81), `AssignProject` (line 113), and `AssignTopic` (line 151) all pass
  `objItemObject: _active.OlItem` (the `IOutlookItem` wrapper itself, not `.InnerObject`).
- `TaskVisualization/TaskController.cs:311` — `_autoAssign` is a single shared `IAutoAssign` field
  used as the `autoAssigner` for People (`Actions.cs:42`), Context (`Actions.cs:77`), and Topic
  (`Actions.cs:147`); `AssignProject` (`Actions.cs:109`) uses a distinct `ProjectAssign` method.
- `Tags/TagController.cs:100-108` — `ResolveMailItem(object objItem)` returns a non-null `MailItem`
  only when `objItem is not null && objItem is MailItem` (a raw interop-type check).
  `Tags/TagController.cs:50-55` sets `_olMail = ResolveMailItem(_objItem)` and `_isMail = true` only
  when `_olMail is not null`.
- `Tags/TagController.cs:115-128` — `SetAutoAssignState` hides/disables the viewer's auto-assign
  button unless `autoAssigner is not null & _isMail` (line 118).
- `Tags/TagController.cs:287-296` — `ButtonAutoAssign_Action` (line 287) calls
  `_autoAssigner.AutoFindAsync(_objItem)` (line 291); this only runs when the button is enabled.
- `TaskVisualization/AutoAssignPeople.cs:59-87` — `AutoFind(object objItem)` returns `[]`
  immediately for `null` (lines 62-65), and for any type not matching `MailItemHelper` (lines
  66-69), an `IOutlookItem` whose `GetOlItemType() == OlItemType.olMailItem` (lines 70-76), or a raw
  `MailItem` (lines 77-80); the final `else` branch (lines 81-84) silently returns `[]` with no
  logging.
- `UtilitiesCS/Interfaces/IReusableTypeClasses/IOutlookItem.cs:6-59` — confirms `IOutlookItem` is a
  host-neutral wrapper interface exposing `InnerObject` as the underlying raw COM object; it is not
  itself assignable to the interop `MailItem` type.
- `UtilitiesCS/OutlookObjects/Item/OutlookItem.cs:176` — confirms `InnerObject => this._item`,
  i.e. the raw item the wrapper was constructed from.
- `ToDoModel/Data Model/ToDo/ToDoItem.cs:406` — confirms `Active.OlItem` returns `IOutlookItem` (via
  `FlaggableItem`), so `_active.OlItem.InnerObject` in `AssignPeople()` is the raw object, not the
  wrapper that the other three assign methods pass.
- `TaskVisualization.Test/TaskControllerActionsTests.cs:313-336` already exercises
  `AssignPeople()`/`AssignContext()` via the mocked `ITagPromptService` seam but does not currently
  assert on the captured `TagPromptRequest.ObjItemObject`'s identity.
- `TaskVisualization.Test/AutoAssignPeopleTests.cs:82-102` covers `AutoFind`'s raw-`MailItem` branch
  and unknown-type branch but has no test for the `IOutlookItem`-wrapped-mail branch
  (`AutoAssignPeople.cs:70-76`).
- `Tags.Test/TagControllerCoverageExpansionTests.cs:~309-331` already covers
  `ButtonAutoAssign_Action`'s tag-toggle behavior generically via a mocked `IAutoAssign`.
- `TaskVisualization.Test/TaskControllerFixtures.cs:73-79` provides `TagPrompt(bool cancelled,
  string selection)`, a `Mock<ITagPromptService>` whose `Prompt(It.IsAny<TagPromptRequest>())` setup
  can be extended with a `.Callback<TagPromptRequest>(...)` to capture the request passed by
  `AssignPeople()`.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read `CLAUDE.md` in full (policy reading order position 1).
  - Evidence contribution: quoted in P0-T5.
  - Acceptance: file read in this session; its Policy Compliance Order section is quoted verbatim
    in the P0-T5 evidence artifact.

- [x] [P0-T2] Read `.claude/rules/general-code-change.md` (policy reading order position 2).
  - Acceptance: file read; its Mandatory Toolchain Loop section quoted in the P0-T5 evidence
    artifact.

- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` (policy reading order position 3).
  - Acceptance: file read; its Coverage Requirements section quoted in the P0-T5 evidence artifact.

- [x] [P0-T4] Read `.claude/rules/csharp.md` (policy reading order positions 4-5, C# Code Change
  Policy and C# Unit Test Policy consolidated).
  - Acceptance: file read; its Toolchain and Testing Standards sections quoted in the P0-T5
    evidence artifact.

- [x] [P0-T5] Write the Phase 0 policy-read evidence artifact to
  `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/phase0-instructions-read.md`.
  - Acceptance: file exists and contains `Timestamp:`, `Policy Order:` (the exact ordered list
    "CLAUDE.md (all sections) → General Code Change Policy → General Unit Test Policy → C# Code
    Change Policy → C# Unit Test Policy"), and an explicit list of the four files read in P0-T1
    through P0-T4, in order.

- [x] [P0-T6] Verify the minor-audit requirements boundary for issue #322.
  - Files: `docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md` (and confirm
    absence of `spec.md`/`user-story.md` in the same folder).
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/minor-audit-scope.<TS>.md`.
  - Acceptance: evidence confirms `issue.md` contains `- Work Mode: minor-audit`, contains an
    explicit `## Acceptance Criteria` section listing AC1-AC6, treats only that section as the AC
    source, and records whether `spec.md`/`user-story.md` are present or absent in the feature
    folder (fail-closed if unexpectedly present).

- [x] [P0-T7] Record baseline git state (current branch name and `HEAD` short SHA via
  `git rev-parse --abbrev-ref HEAD` and `git rev-parse --short HEAD`).
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/git-baseline-state.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
    stating the branch name and SHA.

- [x] [P0-T8] Record candidate-defect-surface baseline notes citing the Confirmed Facts file:line
  list above (no diagnosis conclusion yet — capture only).
  - Files: `TaskVisualization/TaskController.Actions.cs`, `Tags/TagController.cs`,
    `TaskVisualization/AutoAssignPeople.cs`.
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/candidate-defect-surface.<TS>.md`.
  - Acceptance: artifact lists, verbatim, the three candidate defect-surface file:line citations
    from this plan's Confirmed Facts section, with no conclusion drawn (conclusion is Phase 1
    P1-T1's job).

- [x] [P0-T9] Run the baseline C# formatting command.
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/csharpier-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
    stating whether any files were changed.

- [x] [P0-T10] Run the baseline C# analyzer build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/analyzer-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
    the warning/error count.

- [x] [P0-T11] Run the baseline C# nullable build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/nullable-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
    the warning/error count.

- [x] [P0-T12] Run the baseline MSTest coverage command for `TaskVisualization.Test` and `Tags.Test`.
  - Command: `vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll Tags.Test\bin\Debug\Tags.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/vstest-coverage-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
    total tests, pass/fail counts, and the numeric baseline line-coverage percentage for
    `TaskVisualization.dll` and `Tags.dll`.

---

### Phase 1 — Constrained Small-Path Implementation

Delegated to the C# small-path implementation engineer via `atomic-executor`. Follows the repo
Bugfix Workflow: (a) diagnose and document root cause, (b) author a failing regression test first,
(c) implement the minimal targeted fix, (d) check off satisfied AC items in `issue.md`.

- [x] [P1-T1] Diagnose and document the confirmed root cause.
  - Files: `TaskVisualization/TaskController.Actions.cs`, `Tags/TagController.cs`,
    `TaskVisualization/AutoAssignPeople.cs`, `UtilitiesCS/OutlookObjects/Item/OutlookItem.cs`,
    `UtilitiesCS/Interfaces/IReusableTypeClasses/IOutlookItem.cs`.
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/other/root-cause-322.<TS>.md`.
  - Acceptance: evidence cites file:line for (a) `AssignPeople()` passing
    `_active.OlItem.InnerObject` (`TaskController.Actions.cs:46`) vs. the three sibling methods
    passing `_active.OlItem` (lines 81, 113, 151); (b) which branch of `AutoAssignPeople.AutoFind`
    (`AutoAssignPeople.cs:59-87`) the current People argument reaches versus which branch the
    wrapper argument would reach; (c) an explicit confirmed/ruled-out verdict on whether
    `TagController.ResolveMailItem`'s `is MailItem` check (`TagController.cs:100-108`) and the
    `_isMail` gate in `SetAutoAssignState` (`TagController.cs:115-128`) are also blocking for the
    corrected wrapper argument, with supporting reasoning; (d) a single confirmed primary root-cause
    statement. This satisfies AC1.

- [x] [P1-T2] [expect-fail] Author a failing regression test in
  `TaskVisualization.Test/TaskControllerActionsTests.cs` asserting `AssignPeople()` passes the same
  `IOutlookItem` wrapper object that `AssignContext`/`AssignProject`/`AssignTopic` pass, not
  `.InnerObject`.
  - Test name: `AssignPeople_PassesOutlookItemWrapper_NotInnerObject`. Use MSTest + Moq +
    FluentAssertions: extend the `ITagPromptService` mock (`TaskControllerFixtures.TagPrompt`
    pattern) with `.Callback<TagPromptRequest>(r => captured = r)`, invoke
    `controller.AssignPeople()`, then assert
    `captured.ObjItemObject.Should().BeSameAs(controller.Active.OlItem)`. No live Outlook process,
    no temporary files.
  - Precondition: P1-T1 complete.
  - Run command: `vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignPeople_PassesOutlookItemWrapper_NotInnerObject"`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/regression-testing/fail-before-322.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, a non-zero `EXIT_CODE:`, and
    `Output Summary:` stating `1 failed` for the new test, run before any production-code change.
    This satisfies AC2's fail-before requirement.

- [x] [P1-T3] Add a coverage-confirmation test to
  `TaskVisualization.Test/AutoAssignPeopleTests.cs` proving `AutoAssignPeople.AutoFind` reaches the
  classifier seam for an `IOutlookItem`-wrapped mail item (the corrected argument type), not only
  for a raw `MailItem`.
  - Test name: `AutoFind_OutlookItemMailBranch_RoutesThroughToHelperSeam`. Mirror the existing
    `AutoFind_MailItemBranch_RoutesThroughToHelperSeam` pattern (throwing `_toHelper` stub proves
    seam invocation) but supply a Moq `IOutlookItem` whose `GetOlItemType()` returns
    `OlItemType.olMailItem` and whose `InnerObject` returns a Moq `MailItem`.
  - Precondition: P1-T1 complete.
  - Acceptance: the new test passes without modifying `AutoAssignPeople.cs` (this branch already
    exists in production code); it closes the existing coverage gap for the `IOutlookItem` branch
    (`AutoAssignPeople.cs:70-76`) and documents the destination behavior the fix in P1-T4 restores.

- [x] [P1-T4] Implement the minimal fix: change `AssignPeople()`'s `TagPromptRequest` argument from
  `_active.OlItem.InnerObject` to `_active.OlItem` in `TaskVisualization/TaskController.Actions.cs`
  (line 46), matching `AssignContext`/`AssignProject`/`AssignTopic`.
  - Precondition: P1-T2 confirmed failing, P1-T3 complete.
  - Acceptance: exactly one line changed in exactly one production file; `git diff` for
    `TaskVisualization/TaskController.Actions.cs` shows only this substitution inside
    `AssignPeople()`. Satisfies AC3.

- [x] [P1-T5] Apply or explicitly rule out the secondary `ResolveMailItem`/`_isMail` fix in
  `Tags/TagController.cs`, based on P1-T1's diagnosis verdict.
  - If P1-T1 confirms `ResolveMailItem`'s `is MailItem` check does not recognize the
    `IOutlookItem`-wrapped mail item (blocking `_isMail`/button state for the corrected argument):
    extend `ResolveMailItem` to also accept an `IOutlookItem` whose `GetOlItemType() ==
    OlItemType.olMailItem`, returning its `InnerObject` cast to `MailItem`, mirroring
    `AutoAssignPeople.AutoFind`'s own branch pattern (`AutoAssignPeople.cs:70-76`).
  - If P1-T1 rules this out (confirms `_isMail` already evaluates true for the wrapper, matching
    Context/Project's current behavior): make no code change.
  - Precondition: P1-T1 complete.
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/other/secondary-fix-decision-322.<TS>.md`.
  - Acceptance: evidence file exists and states exactly one of the two outcomes above, citing the
    P1-T1 artifact; if applied, the `Tags/TagController.cs` diff is limited to `ResolveMailItem`.

- [x] [P1-T6] Re-run the regression test from P1-T2 alone and confirm it now passes.
  - Precondition: P1-T4 and P1-T5 complete.
  - Command: `vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignPeople_PassesOutlookItemWrapper_NotInnerObject"`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/regression-testing/pass-after-322.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    stating `1 passed, 0 failed`. Satisfies AC2's pass-after requirement and AC4 (the corrected
    object now reaches the seam the classifier consumes).

- [x] [P1-T7] Run the targeted regression suite for the Context/Project/Topic assign flows plus the
  People/`AutoAssignPeople`/`TagController` auto-assign seam to confirm no regression.
  - Command: `vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll Tags.Test\bin\Debug\Tags.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignContext|FullyQualifiedName~AssignProject|FullyQualifiedName~AssignTopic|FullyQualifiedName~AutoAssignPeople|FullyQualifiedName~TagController"`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/regression-testing/targeted-no-regression-322.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    listing pass counts per named test class, confirming zero new failures relative to P0-T12.
    Satisfies AC5.

- [x] [P1-T8] Check off satisfied AC items (AC1-AC5) in `issue.md`'s `## Acceptance Criteria`
  section per `acceptance-criteria-tracking`, citing the Phase 1 evidence artifacts backing each.
  - Files: `docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md`.
  - Evidence mirror: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/issue-updates/ac-status-phase1-322.<TS>.md`.
  - Acceptance: only AC1-AC5 under `## Acceptance Criteria` are changed from `[ ]` to `[x]`, each
    backed by the corresponding P1 evidence artifact path; AC6 remains unchecked pending Phase 2.

---

### Phase 2 — Final QC Loop

Unconditional full C# toolchain, run in order. If any step fails or changes files, restart this
phase from P2-T1. No `SKIPPED` outcomes; no IN_SCOPE/OUT_OF_SCOPE branches.

- [x] [P2-T1] Run the final C# formatting command.
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/csharpier-final-322.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if
    this command changes files, restart Phase 2 from P2-T1.

- [x] [P2-T2] Run the final C# analyzer build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/analyzer-final-322.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`;
    if this command fails, fix and restart Phase 2 from P2-T1.

- [x] [P2-T3] Run the final C# nullable build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/nullable-final-322.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`;
    if this command fails, fix and restart Phase 2 from P2-T1.

- [x] [P2-T4] Run the final full-suite MSTest coverage command for `TaskVisualization.Test` and
  `Tags.Test`.
  - Command: `vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll Tags.Test\bin\Debug\Tags.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/vstest-coverage-final-322.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    with total tests, pass/fail counts, and the numeric post-change line-coverage percentage for
    `TaskVisualization.dll` and `Tags.dll`; if this command fails, fix and restart Phase 2 from
    P2-T1.

- [x] [P2-T5] Compare the baseline coverage (P0-T12) against the post-change coverage (P2-T4) and
  confirm no regression on changed lines and >= 90% coverage on the new/changed code (the new test
  methods and the one-line/`ResolveMailItem` production change from Phase 1).
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/coverage-delta-322.<TS>.md`.
  - Acceptance: artifact contains baseline coverage %, post-change coverage %, changed-line coverage
    % for the touched production file(s), and an explicit PASS/FAIL statement on "no regression on
    changed lines" and ">= 90% coverage on new/changed code." Satisfies the coverage portion of AC6.

- [x] [P2-T6] Verify no other test class regressed by comparing the baseline (P0-T12) and final
  (P2-T4) full-suite results by test name/class.
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/regression-check-322.<TS>.md`.
  - Acceptance: artifact confirms every test that passed at baseline still passes, and the total
    pass count did not decrease. Satisfies the no-regression portion of AC6.

- [x] [P2-T7] Check off AC6 in `issue.md`'s `## Acceptance Criteria` section and record the final
  AC closure summary.
  - Files: `docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md`.
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/issue-updates/ac-status-final-322.<TS>.md` and
    `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/other/ac-closure-summary-322.<TS>.md`.
  - Acceptance: AC6 is changed from `[ ]` to `[x]`, backed by P2-T2 through P2-T6; the closure
    summary lists AC1-AC6 each mapped to its exact backing evidence artifact path(s) from Phases 1
    and 2.

- [x] [P2-T8] Record final minor-audit readiness evidence for issue #322.
  - Evidence: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/minor-audit-readiness-322.<TS>.md`.
  - Acceptance: evidence confirms Phase 0 artifacts exist, Phase 1 diagnosis/regression-test/fix
    evidence exists, Phase 2 QC artifacts exist, every command-bearing task has an executed numeric
    `EXIT_CODE`, and AC1-AC6 are checked off in `issue.md`.

---

## Acceptance Criteria Coverage Map (for preflight cross-check)

- AC1 (root cause identified and documented) → P1-T1.
- AC2 (failing regression test authored first, passes after fix) → P1-T2 (fail-before), P1-T4
  (fix), P1-T6 (pass-after).
- AC3 (auto-tag function executes the people auto-assign path for the active item) → P1-T3
  (destination-branch coverage), P1-T4 (fix), P1-T6 (verification).
- AC4 (matching auto-found people tags toggled on, verified via `TagController` auto-assign action
  seam) → P1-T6 (People flow now reaches the seam), existing `Tags.Test/TagControllerCoverageExpansionTests.cs`
  toggle coverage (unchanged), confirmed via P1-T7.
- AC5 (Context/Project flows unchanged, no regression in their tests) → P1-T5 (secondary-fix
  scope discipline), P1-T7 (targeted no-regression run).
- AC6 (full C# toolchain passes, no regression on changed lines, >= 90% new/changed-code coverage)
  → P2-T1 through P2-T6.
