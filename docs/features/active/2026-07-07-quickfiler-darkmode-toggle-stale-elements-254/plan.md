# quickfiler-darkmode-toggle-stale-elements — Atomic Plan (Issue #254)

- **Issue:** #254
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/254
- **Owner:** drmoisan
- **Work Mode:** minor-audit (small-path, minimal-audit)
- **Last Updated:** 2026-07-07
- **Status:** Draft (pending preflight)

## Scope & Requirements Source

- Sole requirements source: `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/issue.md`, `## Acceptance Criteria` section only (AC1–AC4). `spec.md` / `user-story.md` are NOT required and MUST NOT be referenced; if either is unexpectedly present in the active folder, execution fails closed.
- Root-cause design context: `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/research/root-cause-darkmode-toggle-254.md`.

## Confirmed Root Cause (from research)

`_lblSender`/`_lblSubject` are recolored only inside `Theme.SetMailRead()` / `Theme.SetMailUnread()` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`). `Theme.SetQfcTheme()` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:33-41`) reaches them only through a branch guarded by the injected `MailRead()` `Func<bool>`. When `MailRead()` throws on a stale/moved Outlook `MailItem`, the private renderer aborts before recoloring the labels, so they retain prior-theme colors while the rest of the row re-themes. Minimal fix: evaluate `MailRead()` inside a narrow `try/catch` that defaults to a deterministic branch so the labels always re-theme.

## Constraints Encoded in This Plan

- MINIMAL, targeted fix: change only `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` in production, plus the test file and its csproj registration. No opportunistic refactor. A second production file may be touched only if execution proves it unavoidable, and must be recorded as a scope note.
- Error handling per General Code Change Policy: the catch MUST be NARROW — catch `System.Runtime.InteropServices.COMException` (the documented failure of reading `MailItem.UnRead` on a disconnected item), not broad `Exception`. If execution proves `Mail` can be null on this path, add `System.NullReferenceException` explicitly rather than widening to `Exception`. The catch MUST carry a `// why` comment explaining the UI-boundary rationale and MUST NOT silently swallow unrelated exceptions.
- Regression test deterministic and seam-based, reusing the handle-less big-constructor doubles pattern proven in `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.DispatcherTests.cs` (`Constructor_BigOverload_WithNullUiDispatcher_DefaultsToWpfUiDispatcher`, lines 87-146). No live Outlook/COM/WinForms objects; no temp files. `MSTest` + `FluentAssertions`.
- Coverage: changed lines are in the non-exempt `UtilitiesCS` assembly. Baseline and final-QC coverage capture tasks record numeric values; verify no regression on changed lines and `>= 90%` on new/changed code.

## Evidence Location Invariant

All evidence resolves to `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/<kind>/` (`baseline/`, `regression-testing/`, `qa-gates/`). No `artifacts/` evidence path is used. Timestamps use `yyyy-MM-ddTHH-mm` (denoted `<TS>` below).

## Acceptance Criteria Traceability

- **AC1** (every item re-themes; no stale prior-theme colors) → P1-T5 fix + P1-T6 pass-after. Evidence: `evidence/regression-testing/pass-after.2026-07-07T13-18.md` (labels re-theme even when the probe throws).
- **AC2** (minimal, targeted change; no refactor) → single production file at P1-T5. Evidence: `git status` shows only `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` changed in production; narrow COMException catch with `// why` comment.
- **AC3** (deterministic seam-based regression; fails before, passes after) → P1-T1/T2/T4/T6. Evidence: `evidence/regression-testing/fail-before.2026-07-07T13-16.md` (fail-before) and `evidence/regression-testing/pass-after.2026-07-07T13-18.md` (pass-after); handle-less doubles, no live Outlook/COM/WinForms, no temp files.
- **AC4** (#251 no-regression + full C# toolchain + no coverage regression on changed lines) → P2-T1..T5. Evidence: `evidence/qa-gates/qc-csharpier.2026-07-07T13-18.md`, `qc-analyzers.2026-07-07T13-18.md`, `qc-nullable.2026-07-07T13-18.md`, `qc-tests-coverage.2026-07-07T13-28.md` (#251 `QfcCollectionControllerDarkModeTests` green, incl. Cleanup/CleanupAsync unsubscribe tests), `coverage-comparison.2026-07-07T13-28.md` (100% changed-line coverage, no regression).

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in `policy-compliance-order` sequence: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and an explicit list of files read. Acceptance: artifact exists with all three fields present.
- [x] [P0-T2] Capture baseline formatting state. Command: `dotnet tool run csharpier --check .`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/baseline/baseline-csharpier.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact recorded with all four fields.
- [x] [P0-T3] Capture baseline analyzer build. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/baseline/baseline-analyzers.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact recorded with all four fields.
- [x] [P0-T4] Capture baseline nullable/type-check build. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/baseline/baseline-nullable.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact recorded with all four fields.
- [x] [P0-T5] Capture baseline test + coverage. Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/baseline/baseline-tests-coverage.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. `Output Summary:` MUST include numeric baseline coverage headline (overall percent and the `UtilitiesCS` / `Theme` module percent). Acceptance: artifact recorded with numeric coverage values (no placeholders).

---

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] [expect-fail] Create regression test file `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs` with an `[TestClass]` and one regression `[TestMethod]`: construct a `Theme` via the big constructor using handle-less WinForms doubles (pattern from `Theme.DispatcherTests.cs:87-146`), with distinct `mailUnreadBackColor`/`mailReadBackColor` sentinels, `_lblSender`/`_lblSubject` pre-set to a distinct "previous-theme" sentinel color, and `mailRead: () => throw new System.Runtime.InteropServices.COMException()`. Act via `Action act = () => theme.SetQfcTheme(async: false);`. Assert `act.Should().NotThrow()` AND both label `BackColor` values equal the theme unread color (re-themed, not the prior sentinel). Acceptance: file exists with the described test; no live Outlook/COM/WinForms handle and no temp file used.
- [x] [P1-T2] Add two positive branch test cases to the same file: (a) `mailRead: () => false` asserts both labels adopt `mailUnread*` colors; (b) `mailRead: () => true` asserts both labels adopt `mailRead*` colors. Acceptance: both `[TestMethod]` cases present, giving full branch coverage (try-success-read, try-success-unread, catch-default) of the changed block.
- [x] [P1-T3] Register the new test file in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` by adding `<Compile Include="HelperClasses\ThemeHelpers\Theme.MailLabelThemingTests.cs" />` in the existing `ThemeHelpers` `<Compile>` item group (legacy `packages.config` project has no glob include). Acceptance: the exact `<Compile Include>` line is present next to the existing `Theme.DispatcherTests.cs` entry.
- [x] [P1-T4] [expect-fail] Build `UtilitiesCS.Test` and run only the new regression test. Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:<regression-test-name>`. Confirm it FAILS before the fix. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/regression-testing/fail-before.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` (failing assertion / thrown COMException). Acceptance: artifact records a failing run tied to the regression test.
- [x] [P1-T5] Apply the minimal fix in `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` mail branch (currently lines 33-41): evaluate `MailRead()` inside a narrow `try { isRead = MailRead(); } catch (System.Runtime.InteropServices.COMException) { isRead = false; }` and then `if (!isRead) { SetMailUnread(); } else { SetMailRead(); }`, preserving a `// why` comment referencing issue #254 and the stale-`MailItem` UI-boundary rationale. If and only if execution proves `Mail` can be null on this path, add `System.NullReferenceException` explicitly to the catch list; do NOT widen to broad `Exception`. Acceptance: only `Theme.Rendering.cs` production file changed; catch is narrow; `// why` comment present.
- [x] [P1-T6] Re-run the regression test and the two positive cases and confirm all PASS after the fix. Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:Theme_MailLabelTheming`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/regression-testing/pass-after.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (0), `Output Summary:` (3 tests passed). Acceptance: artifact records all three tests passing.

---

### Phase 2 — Final QC Loop

Run the full C# toolchain in order (format → analyzers → nullable → test-with-coverage). If any step changes files or fails, restart from P2-T1. All command tasks are unconditional; `SKIPPED` is not a valid completion state.

- [x] [P2-T1] Format. Command: `dotnet tool run csharpier .`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/qa-gates/qc-csharpier.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact recorded; if files changed, loop restarts.
- [x] [P2-T2] Analyzers. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/qa-gates/qc-analyzers.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: exit code 0, artifact recorded.
- [x] [P2-T3] Nullable/type-check. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/qa-gates/qc-nullable.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: exit code 0, artifact recorded.
- [x] [P2-T4] Test with coverage. Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`. Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/qa-gates/qc-tests-coverage.<TS>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. `Output Summary:` MUST include numeric post-change coverage headline (overall percent and `UtilitiesCS` / `Theme` module percent) and confirm the #251 `QfcCollectionControllerDarkModeTests` suite passed (AC4). Acceptance: exit code 0, numeric coverage recorded, #251 suite green.
- [x] [P2-T5] Coverage delta/threshold verification. Compare baseline (P0-T5) vs post-change (P2-T4). Write `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/qa-gates/coverage-comparison.<TS>.md` recording: baseline coverage, post-change coverage, and new/changed-code coverage for the modified `Theme.Rendering.cs` mail branch. Acceptance: new/changed-code coverage `>= 90%` and no regression on changed lines; if either threshold is unmet, outcome is remediation-required (NOT PASS).
- [x] [P2-T6] AC closeout. Confirm AC1–AC4 satisfied and cite the backing evidence artifact for each in this plan's traceability section. Acceptance: each of AC1–AC4 maps to at least one recorded evidence artifact and all mapped artifacts exist on disk.
