# quickfiler-darkmode-unread-labels-stay-blue - Minor-Audit Plan

- **Issue:** #269
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/269
- **Requirements Source:** `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`
- **Plan Path:** `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/plan.md`
- **Feature Folder:** `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269`
- **Work Mode:** minor-audit
- **Language:** C#
- **Last Updated:** 2026-07-08T09-15
- **Status:** Draft — pending preflight validation

## Requirements Boundary

This minor-audit plan uses only `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md` as the requirements source. Acceptance criteria are limited to the checkbox items (AC1-AC5) under that file's explicit `## Acceptance Criteria` section (confirmed present at lines 68-74). `spec.md` and `user-story.md` are not required by minor-audit mode; their absence from the feature folder is not a blocker.

Implementation is constrained to the confirmed root cause (see `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/research/mechanism-unread-labels-blue-254.md`). Expected touched files are limited to:

- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` (production file 1 of 2)
- `QuickFiler/Helper Classes/QfcThemeHelper.cs` (production file 2 of 2)
- `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs` (extend existing file; no new file, no csproj wiring required)
- `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` (extend existing file; no new file, no csproj wiring required)
- `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md` (AC checkbox status updates only)

Both production files are within a 1-3 file small-path budget. No new source files are created, so no `<Compile Include>` wiring task is required in either `UtilitiesCS.Test.csproj` or `QuickFiler.Test.csproj`.

All evidence must be written under `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/<kind>/`.

## Confirmed Root Cause (not re-litigated; see mechanism doc for full derivation)

`Theme.SetQfcTheme()` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:8-121`) paints the row's `TableLayoutPanel`s dark (lines 15-18) before reaching the mail-label branch (lines 42-59). That branch wraps the read-state probe call (`isRead = MailRead();`, line 45) in a `try/catch` that only catches `System.Runtime.InteropServices.COMException` (lines 43-50). The injected probe `() => !controller.Mail.UnRead` (`QuickFiler/Helper Classes/QfcThemeHelper.cs:89`) throws `NullReferenceException` when `controller.Mail` is `null` — a state the class already anticipates elsewhere (`QfcItemController.Initialization.cs:392-394`, `_mailActions ??= mailItem is null ? null : ...`). An uncaught `NullReferenceException` at line 45 aborts `SetQfcTheme()` before the label branch (lines 52-58) and the button loop (lines 61-72) run, leaving `_lblSender`/`_lblSubject` at their last successfully-applied color (light-theme unread `Color.MediumBlue`, `QfcThemeHelper.cs:119`) while the rest of the row appears dark through ordinary WinForms ambient `BackColor` inheritance from the already-recolored panel.

## Chosen Fix Shape (exact, minimal, targeted — no opportunistic refactor)

Two complementary changes, both within the probe construction site and the mail-label guard named by AC3, applied together for defense in depth:

1. **`QuickFiler/Helper Classes/QfcThemeHelper.cs:89`** — null-guard the probe at its construction site. Change `() => !controller.Mail.UnRead` to `() => controller.Mail is not null && !controller.Mail.UnRead`. This removes the `NullReferenceException` trigger at its source. The guard defaults to `false` (i.e., "not read") when `controller.Mail` is `null`, deliberately matching the existing default-to-unread convention already documented at `Theme.Rendering.cs:34-41` and already exercised by the `COMException` case (`isRead = false` on line 49). This intentionally departs from the research document's originally-sketched `controller.Mail is null || !controller.Mail.UnRead` form, which would default to `true` ("is read", black-on-read colors) — the opposite of the established convention and inconsistent with the required test's expectation that a probe fault yields the theme's *unread* colors.
2. **`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:42-50`** — broaden the existing narrow `catch (System.Runtime.InteropServices.COMException)` to add a second, equally narrow catch clause for `System.NullReferenceException`, also setting `isRead = false;`. This is not a broad `catch (Exception)`; it names two specific exception types and preserves propagation for every other exception type, consistent with the file's own documented rationale.

Rationale against the fail-fast/no-broad-catch policy: change 1 eliminates the confirmed root cause at its source (the preferred, root-cause fix per `.claude/rules/general-code-change.md` Bugfix Workflow). Change 2 is a narrowly-scoped defense-in-depth boundary guard — it does not widen to `catch (Exception)`, and it ensures the label/button branches cannot be skipped by this specific, already-anticipated fault class even if a future caller constructs the probe without the guard in change 1. Together they satisfy AC2's "regardless of the probe outcome" requirement without weakening the existing `COMException` handling (AC5, no regression to issue #254).

---

### Phase 0 — Policy and Baseline Evidence

- [x] [P0-T1] Record policy-read evidence for issue #269 before implementation begins.
  - Files read (in order): `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/research/mechanism-unread-labels-blue-254.md`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: Evidence file exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read above, in order.

- [x] [P0-T2] Verify the minor-audit requirements boundary for issue #269.
  - Files: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md` (and confirm absence of `spec.md`, `user-story.md` in the same folder)
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/minor-audit-scope.2026-07-08T09-15.md`
  - Acceptance: Evidence confirms `issue.md` contains `- Work Mode: minor-audit`, contains an explicit `## Acceptance Criteria` section listing AC1-AC5, treats only that section as the AC source, and confirms `spec.md` and `user-story.md` are absent from the feature folder.

- [x] [P0-T3] Record investigation evidence confirming the confirmed-mechanism citations needed to implement and test the fix.
  - Files: `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`, `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`, `QuickFiler/Helper Classes/QfcThemeHelper.cs`, `QuickFiler/Controllers/QfcItemController.Initialization.cs`, `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/investigation-notes.2026-07-08T09-15.md`
  - Acceptance: Evidence records, with file:line citations: (a) the exact statement order in `SetQfcTheme()` (panel recolor at lines 15-18, mail-label branch at lines 42-59, button loop at lines 61-72); (b) the existing narrow `catch (COMException)` block (lines 43-50); (c) the probe construction site `() => !controller.Mail.UnRead` (`QfcThemeHelper.cs:89`); (d) confirmation that `IQfcItemController.Mail` is a nullable `Outlook.MailItem` property and that a null `Mail` is an anticipated state elsewhere in the class; (e) the existing three test cases in `Theme.MailLabelThemingTests.cs` (probe throws `COMException`, probe returns `false`, probe returns `true`) and the existing `BuildProductionControlSet_MapsControllerAndViewerInputs` test in `QfcThemeHelperTests.cs`.

- [x] [P0-T4] Run the baseline C# formatting command.
  - Files: `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`, `QuickFiler/Helper Classes/QfcThemeHelper.cs`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/csharpier-baseline.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:` stating whether any files were changed.

- [x] [P0-T5] Run the baseline C# analyzer build command.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/csharp-analyzers-baseline.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T6] Run the baseline C# nullable build command.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/csharp-nullable-baseline.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T7] Run the baseline MSTest coverage command for the two impacted test assemblies.
  - Files: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`, `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with total tests, pass/fail counts, and the numeric baseline coverage headline percentage for both assemblies.

---

### Phase 1 — Constrained Implementation (Red → Green)

- [x] [P1-T1] Delegate constrained C# implementation to the small-path implementation engineer for issue #269.
  - Files: `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`, `QuickFiler/Helper Classes/QfcThemeHelper.cs`, `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`
  - Acceptance: The implementation handoff references issue #269, the feature folder, the requirements source, the `csharp.md` policy rule, the "Chosen Fix Shape" section of this plan, and the constraint that production changes are limited to `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` and `QuickFiler/Helper Classes/QfcThemeHelper.cs` only.

- [x] [P1-T2] [expect-fail] Add regression test `Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread` to `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, run it against the pre-fix code, and confirm it fails.
  - Precondition: Phase 0 complete.
  - Test scenario: Reuse the existing `BuildTheme` helper with `mailRead: () => throw new NullReferenceException("simulated null Mail")`. Act via `Action act = () => theme.SetQfcTheme(async: false);`. Assert `act.Should().NotThrow();`, `lblSender.BackColor.Should().Be(UnreadBack);`, `lblSubject.BackColor.Should().Be(UnreadBack);`, and both labels `Should().NotBe(PreviousThemeSentinel)`.
  - Acceptance: Test added with `[TestMethod]` inside the existing `[TestClass]` fixture, MSTest + FluentAssertions per `csharp.md`. Evidence artifact `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/fail-before-theme-nre-probe.2026-07-08T09-15.md` records `Timestamp:`, `Command:` (the targeted `vstest.console.exe` filter run against `UtilitiesCS.Test.dll` for this test), `EXIT_CODE:` (non-zero / failing), and `Output Summary:` confirming the pre-fix `NullReferenceException` propagates out of `SetQfcTheme(async: false)`. Satisfies AC4 (fail-before half).

- [x] [P1-T3] [expect-fail] Add regression test `BuildProductionControlSet_WithNullMail_MailReadReturnsFalseWithoutThrowing` to `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`, run it against the pre-fix code, and confirm it fails.
  - Precondition: Phase 0 complete.
  - Test scenario: Build a `FakeQfcItemController` via the existing `CreateController` helper, set `Mail = null`, build a `QfcThemeControlSet` via `QfcThemeHelper.BuildProductionControlSet`, then `Action act = () => controlSet.MailRead();` and assert `act.Should().NotThrow();` followed by asserting the invoked result is `false`.
  - Acceptance: Test added with `[TestMethod]` inside the existing `[TestClass]` fixture. Evidence artifact `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/fail-before-qfcthemehelper-null-mail.2026-07-08T09-15.md` records `Timestamp:`, `Command:` (the targeted `vstest.console.exe` filter run against `QuickFiler.Test.dll` for this test), `EXIT_CODE:` (non-zero / failing), and `Output Summary:` confirming the pre-fix `NullReferenceException` is thrown when the probe is invoked with a null `Mail`. Satisfies AC4 (fail-before half, probe construction site).

- [x] [P1-T4] Fix the probe construction site in `QuickFiler/Helper Classes/QfcThemeHelper.cs`.
  - Precondition: P1-T2 and P1-T3 complete and confirmed failing.
  - Fix: In `BuildProductionControlSet` (line 89), change `() => !controller.Mail.UnRead` to `() => controller.Mail is not null && !controller.Mail.UnRead`.
  - Acceptance: The probe never dereferences a null `controller.Mail`; it returns `false` when `controller.Mail` is `null`. No other line in `QfcThemeHelper.cs` is modified. Satisfies AC3 (probe construction site component).

- [x] [P1-T5] Fix the mail-label guard in `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`.
  - Precondition: P1-T4 complete.
  - Fix: In `SetQfcTheme()` (lines 42-50), add a second catch clause `catch (System.NullReferenceException) { isRead = false; }` alongside the existing `catch (System.Runtime.InteropServices.COMException) { isRead = false; }`, and update the preceding "why" comment (lines 34-41) to note that the probe fault surface now also includes `NullReferenceException` from a null `Mail` (issue #269), while keeping both catch clauses narrow (no `catch (Exception)`).
  - Acceptance: `SetQfcTheme()` no longer aborts when the probe throws `COMException` or `NullReferenceException`; both cases default `isRead = false` and proceed to the mail-label branch and button loop. No other statement in `Theme.Rendering.cs` is modified. Satisfies AC2 and AC3 (mail-label guard component).

- [x] [P1-T6] Record implementation-scope evidence confirming only the two named production files were changed.
  - Files: `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`, `QuickFiler/Helper Classes/QfcThemeHelper.cs`, `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/implementation-scope.2026-07-08T09-15.md`
  - Acceptance: Evidence lists every changed file (via `git diff --stat`) and confirms the only production files changed are `Theme.Rendering.cs` and `QfcThemeHelper.cs`, satisfying AC3.

- [x] [P1-T7] Run the targeted issue #269 regression tests in `UtilitiesCS.Test` with coverage and confirm all four `Theme_MailLabelThemingTests` methods pass post-fix.
  - Files: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~Theme_MailLabelThemingTests"`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/targeted-vstest-utilitiescs.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming all four tests pass, including the new `Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread` and the pre-existing `Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread` (`COMException` case, issue #254 non-regression). Satisfies AC1, AC4 (pass-after half), and AC5 (`COMException` non-regression).

- [x] [P1-T8] Run the targeted issue #269 regression test in `QuickFiler.Test` with coverage and confirm it passes post-fix.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~QfcThemeHelperTests"`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/targeted-vstest-quickfiler.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming `BuildProductionControlSet_WithNullMail_MailReadReturnsFalseWithoutThrowing` passes and every pre-existing `QfcThemeHelperTests` test still passes. Satisfies AC1, AC3, and AC4 (pass-after half, probe construction site).

---

### Phase 2 — Final C# QA Loop

- [x] [P2-T1] Run the final C# formatting command.
  - Files: `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`, `QuickFiler/Helper Classes/QfcThemeHelper.cs`, `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/csharpier-final.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:`; if this command changes files, restart Phase 2 from P2-T1 after preserving the evidence.

- [x] [P2-T2] Run the final C# analyzer build command.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/csharp-analyzers-final.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T3] Run the final C# nullable build command.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/csharp-nullable-final.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T4] Run the final full-suite MSTest coverage command for both impacted test assemblies.
  - Files: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`, `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-08T09-15.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with total tests, pass/fail counts, and the numeric post-change coverage headline percentage for both assemblies; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T5] Record C# coverage comparison evidence for issue #269.
  - Files: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-08T09-15.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/targeted-vstest-utilitiescs.2026-07-08T09-15.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/targeted-vstest-quickfiler.2026-07-08T09-15.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-08T09-15.md`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/csharp-coverage-comparison.2026-07-08T09-15.md`
  - Acceptance: Evidence records baseline coverage, targeted-test coverage, post-change coverage for both `UtilitiesCS.Test` and `QuickFiler.Test`, confirms no repository-wide regression, and confirms the changed lines in `Theme.Rendering.cs` and `QfcThemeHelper.cs` are covered by the new tests added in P1-T2 and P1-T3. Satisfies AC5 (coverage portion, no regression on changed lines).

- [x] [P2-T6] Update issue #269 acceptance-criteria status after verified completion.
  - Files: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/issue-updates/ac-status.2026-07-08T09-15.md`
  - Acceptance: Only verified acceptance criteria (AC1-AC5) under `## Acceptance Criteria` in `issue.md` are changed from `[ ]` to `[x]`; unchanged text is preserved. Evidence records total AC items, checked items, remaining items, and the verification evidence used for each checked item, per `acceptance-criteria-tracking`.

- [x] [P2-T7] Verify required CI checks pass green on the PR head SHA once the PR is opened for issue #269.
  - Files: PR created from this branch against `main`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/ci-check-verification.<pr-timestamp>.md`
  - Acceptance: Evidence records the PR URL, head SHA, the required check names, and their pass/fail status (`gh pr checks <PR>` or equivalent), confirming all required checks are green. If no PR has been opened yet at plan-execution time, record that explicit deferral reason in the evidence artifact rather than a numeric `EXIT_CODE` (the only authorized non-command completion path in this task); this task must be re-run to completion once a PR exists.

- [x] [P2-T8] Record final minor-audit readiness evidence for issue #269.
  - Files: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/plan.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/baseline/phase0-instructions-read.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/regression-testing/implementation-scope.2026-07-08T09-15.md`, `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/csharp-coverage-comparison.2026-07-08T09-15.md`
  - Evidence: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/evidence/qa-gates/minor-audit-readiness.2026-07-08T09-15.md`
  - Acceptance: Evidence confirms Phase 0 artifacts exist, Phase 1 scope and regression-test evidence exist, Phase 2 C# QA artifacts exist, every command-bearing task has an executed numeric `EXIT_CODE`, AC1-AC5 are checked off in `issue.md`, and the P2-T7 CI-check disposition (green or explicitly deferred pending PR) is recorded.
