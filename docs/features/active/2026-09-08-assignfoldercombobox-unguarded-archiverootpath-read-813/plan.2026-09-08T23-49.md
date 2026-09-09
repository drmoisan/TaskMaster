# 2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read (Plan)

- **Issue:** #813
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09
- **Status:** Ready for execution
- **Version:** 1.0
- **Work Mode:** full-bug (spec.md is the sole Acceptance Criteria source; `user-story.md` is
  intentionally absent from this feature folder — confirmed absent at plan-authoring time)

## Plan-wide conventions

- `<TIMESTAMP>` in any evidence filename below means: substitute the actual ISO-8601 capture
  timestamp (`yyyy-MM-ddTHH-mm`, per `evidence-and-timestamp-conventions`) at the moment that task
  executes. It is a file-naming instruction, not an asserted literal.
- `<FEATURE>` = `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813`.
  All evidence paths below resolve under `<FEATURE>/evidence/<kind>/` only. No task in this plan
  writes to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other
  non-canonical location.
- **Owned files (may be edited by this plan):** `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
  (production fix) and `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
  (new regression test). Confirmed at plan-authoring time (2026-09-09): `FolderHandling.cs` is 296
  lines and `Part2.cs` is 363 lines. The estimated addition to `Part2.cs` (3 `using` lines + one
  ~15-line private helper + one ~55-line test method) brings it to roughly 430–440 lines, well under
  the 500-line cap, so this plan targets `Part2.cs` directly; no `Part3.cs` file is created. Phase 6
  still gates the final line count as a real, non-vacuous check.
- **Files this plan MUST NOT modify:** `TaskMaster/AppGlobals/AppOlObjects.cs`,
  `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`, `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`
  (issue #812's frozen write set), and every sibling-owned file named in spec.md's Scope & Non-Goals
  section, at their verified repository paths (corrected during preflight review to match the exact
  paths already used by the P6-T3 enforcement gate below): `QuickFiler/Controllers/QfcHomeController.cs`,
  `UtilitiesCS/Threading/ProgressViewer.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`
  and `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`,
  `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`,
  the SDIL reader files (`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`,
  `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILInstruction.cs`,
  `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` — confirmed to exist and named
  explicitly in the P6-T3 enforcement gate below),
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions*.cs`, `UtilitiesCS/Threading/TimeOutTask.cs`,
  `UtilitiesCS/Extensions/DfDeedle.cs`, `.editorconfig`, `BannedSymbols.txt`, and
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`. No task in this plan touches
  `.claude/rules/**`, `.github/instructions/**`, or `CLAUDE.md`.
- **Toolchain scope resolution:** `.claude/rules/general-code-change.md`'s cross-language seven-stage
  loop includes "architecture-boundary tests" and "contract/schema compatibility checks" stages that
  have no C#-specific tooling defined anywhere in this repository (no NetArchTest, no schema-diff
  tool is wired for any `.csproj` touched here); `.claude/rules/csharp.md` and CLAUDE.md's "C#
  Toolchain (run in this exact order)" section define the concrete, tool-backed sequence for C# as
  four stages (format → analyze → type-check → test). This plan follows that four-stage C#-specific
  sequence for every toolchain task, per policy-compliance-order's rule that language-specific
  policy layers on top of (and gives concrete form to) the general cross-language policy; it is not a
  skip of general-code-change.md's stages, since no C# tooling exists in this repo for the two
  inapplicable stages.
- **Coverage floor resolution:** `.claude/rules/general-unit-test.md` sets a uniform 85% line / 75%
  branch floor across all tiers; `.claude/rules/csharp.md` and CLAUDE.md's C# Unit Test Policy state
  an 80% repo-wide floor with a 90% floor for new code. Per the "do not weaken any coverage
  threshold" instruction, this plan applies the STRICTER of the two repo-wide floors (85% line, per
  general-unit-test.md, which supersedes the 80% C#-specific figure for the repo-wide gate) together
  with the 90% new/changed-code floor from CLAUDE.md's C# Unit Test Policy (the two are compatible;
  the second applies to the specific lines touched by the fix, the first to the repo-wide total).
  Repo-wide **branch** coverage is a pre-existing, unrelated shortfall against the 75% floor: the most
  recent same-methodology measurement (`docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/policy-audit.2026-09-08T11-30.md`,
  merged to `main` immediately before this branch) records 66.3978% repo-wide branch coverage, a gap
  this repository has repeatedly and explicitly dispositioned as non-blocking in prior sibling audits
  (2026-08-24, 2026-08-26, 2026-09-01) because it predates this fix and this fix's own scope cannot
  close a repo-wide gap. Repo-wide **line** coverage in that same audit is 86.0424%, above the 85%
  floor, so P5-T6's line-coverage acceptance is realistically satisfiable and this plan does not
  weaken it. P5-T6's acceptance for branch coverage is therefore limited to no-further-regression
  (post-change repo-wide branch-rate not more than 0.5 percentage points below the pre-existing
  66.3978% baseline), consistent with the "no regression on changed lines" requirement without
  treating this narrow fix as responsible for closing the pre-existing repo-wide branch-coverage gap.

---

### Phase 0 — Policy Reads & Toolchain/Coverage Baseline

- [ ] [P0-T1] Read, in order, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
  `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`. Record `Timestamp:`,
  `Policy Order: CLAUDE.md, general-code-change.md, general-unit-test.md, csharp.md`, and the
  explicit list of the four file paths read, in `<FEATURE>/evidence/baseline/phase0-instructions-read.<TIMESTAMP>.md`.
  Acceptance: the artifact file exists and lists all four paths in the stated order.

- [ ] [P0-T2] Capture the pre-change repository state: run `git rev-parse HEAD` and
  `git status --porcelain` from the worktree root. Record `Timestamp:`, `Command:`, `EXIT_CODE:`,
  and `Output Summary:` (the HEAD SHA and the porcelain output, or `Output Summary: clean` if empty)
  in `<FEATURE>/evidence/baseline/phase0-branch-state.<TIMESTAMP>.md`. Acceptance: the artifact
  records a 40-character HEAD SHA and the literal porcelain output.

- [ ] [P0-T3] Resolve the toolchain executable paths needed by later phases: run
  `vswhere.exe -latest -find **\vstest.console.exe` to resolve `vstest.console.exe`, and confirm
  `msbuild` is resolvable (via `vswhere.exe -latest -find **\MSBuild.exe` or the `msbuild` on PATH).
  Record both resolved paths, `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` in
  `<FEATURE>/evidence/baseline/phase0-toolchain-paths.<TIMESTAMP>.md`. Acceptance: both paths are
  non-empty and point to files that exist. Later phases that invoke `vstest.console.exe` reference
  the path recorded in this artifact rather than a hardcoded absolute path.

- [ ] [P0-T4] Run `dotnet tool restore` from the worktree root. Record `Timestamp:`, `Command:`,
  `EXIT_CODE:`, `Output Summary:` in `<FEATURE>/evidence/baseline/phase0-dotnet-tool-restore.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0`.

- [ ] [P0-T5] Run `dotnet tool run csharpier check QuickFiler/Controllers/QfcItemController.FolderHandling.cs QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
  to capture the pre-change formatting state of the two owned files. Record `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:` in `<FEATURE>/evidence/baseline/phase0-csharpier-check.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0` (both files are already CSharpier-compliant pre-change).

- [ ] [P0-T6] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  to capture the pre-change analyzer baseline. Record `Timestamp:`, `Command:`, `EXIT_CODE:`,
  `Output Summary:` (must include the literal `Build succeeded` line and the `0 Error(s)` count) in
  `<FEATURE>/evidence/baseline/phase0-analyzer-rebuild.<TIMESTAMP>.md`. Acceptance: `EXIT_CODE: 0`.

- [ ] [P0-T7] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  to capture the pre-change nullable/type-check baseline. Record `Timestamp:`, `Command:`,
  `EXIT_CODE:`, `Output Summary:` (must include `Build succeeded` and `0 Error(s)`) in
  `<FEATURE>/evidence/baseline/phase0-nullable-rebuild.<TIMESTAMP>.md`. Acceptance: `EXIT_CODE: 0`.

- [ ] [P0-T8] Run `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml`
  to capture the pre-change repo-wide coverage baseline and confirm the full suite is green before
  any change is made. Record `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` containing
  the total tests passed/failed count and the repo-wide `line-rate`/`branch-rate` percentages read
  from the produced Cobertura XML, in `<FEATURE>/evidence/baseline/phase0-coverage-baseline.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0`, 0 failed tests, and both percentages are recorded as numeric values
  (not placeholders).

---

### Phase 1 — Scope & File-State Confirmation

- [ ] [P1-T1] Re-read `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` and confirm the
  unguarded read still sits at lines 231-234 exactly as:
  `string predetermined = ProjectPredeterminedFolder(_predeterminedFolder, _globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty));`
  (the exact 4-line expression spanning lines 231-234), and record the confirmed line range and file
  line count (296) in `<FEATURE>/evidence/baseline/phase1-file-size-check.<TIMESTAMP>.md`.
  Acceptance: the recorded text matches this expression verbatim and the file has 296 lines. If it
  does not match, this task fails and the plan requires re-authoring before Phase 2 proceeds.

- [ ] [P1-T2] Re-read `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
  and confirm its current line count. Append the count to
  `<FEATURE>/evidence/baseline/phase1-file-size-check.<TIMESTAMP>.md`. Acceptance: the recorded
  count is 363. (This confirms the plan's Part2.cs-vs-Part3.cs sizing decision from the plan header
  still holds at execution time.)

- [ ] [P1-T3] Confirm `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/spec.md`
  exists and `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/user-story.md`
  does not exist (full-bug mode gate). Record both findings in
  `<FEATURE>/evidence/baseline/phase1-mode-gate-check.<TIMESTAMP>.md`. Acceptance: `spec.md` exists,
  `user-story.md` does not exist.

---

### Phase 2 — Failing Regression Test (must fail first)

- [ ] [P2-T1] [expect-fail] In `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`,
  add exactly three new `using` directives after the existing `using UtilitiesCS;` line (line 8):
  `using System.Collections.Generic;`, `using System.Reflection;`, and
  `using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;`. Then add the
  following private helper method and test method as new members of the
  `QfcItemController_FolderHandlingTests` partial class, placed after the existing
  `LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation` test method
  (currently ending at line 361) and before the closing braces at lines 362-363:

  ```csharp
  /// <summary>
  /// Builds a <see cref="FolderPredictor"/> via the globals-providing constructor so
  /// <c>Suggestions</c> is non-null (matching production initialization), with a known
  /// <c>FolderArray</c> seeded the same way <see cref="BuildFolderHandlerWithArray"/> does.
  /// Used only by the #813 regression test below, which must observe
  /// <c>SetFolderSuggestions</c> being invoked.
  /// </summary>
  private static FolderPredictor BuildFolderHandlerWithSuggestions(
      IApplicationGlobals globals,
      params string[] folders
  )
  {
      var fp = new FolderPredictor(globals);
      typeof(FolderPredictor)
          .GetField("_folderList", BindingFlags.NonPublic | BindingFlags.Instance)
          .SetValue(fp, new List<string>(folders));
      return fp;
  }

  /// <summary>
  /// Issue #813. <c>Ol.ArchiveRootPath</c> throws <see cref="InvalidOperationException"/> when the
  /// archive root is unconfigured or unresolvable. The read at <c>AssignFolderComboBox</c>'s
  /// predetermined-folder projection step must not propagate that exception onto the UI dispatcher
  /// thread; it must degrade to no preselection while leaving the combo box and suggestion rows
  /// populated.
  /// </summary>
  [TestMethod]
  public void AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing()
  {
      // Arrange
      var mock = new Mock<IItemViewer>();
      mock.SetupGet(v => v.InvokeRequired).Returns(false);
      // FolderContains returns false because the predetermined folder text is not present in the
      // populated array; this isolates the assertion to the archive-root fallback behavior
      // (AC3) independent of any containment match.
      mock.Setup(v => v.FolderContains(It.IsAny<string>())).Returns(false);
      mock.Setup(v => v.GetSelectedFolder()).Returns(string.Empty);

      var globals = new Mock<IApplicationGlobals>();
      globals.SetupGet(g => g.Ol.ArchiveRootPath).Throws<InvalidOperationException>();
      globals.SetupGet(g => g.AF.RecentsList).Returns(new SloLinkedList<string>());

      var controller = new FolderController();
      SetPrivate(controller, "_itemViewer", mock.Object);
      SetPrivate(controller, "_globals", globals.Object);
      SetPrivate(controller, "_predeterminedFolder", @"\\A\chosen");
      SetPrivate(
          controller,
          "_folderHandler",
          BuildFolderHandlerWithSuggestions(globals.Object, @"\\A\header", @"\\A\top")
      );

      // Act
      Action act = () => controller.AssignFolderComboBox();

      // Assert
      act.Should()
          .NotThrow<InvalidOperationException>(
              "an unresolvable archive root must degrade to no preselection instead of "
                  + "propagating onto the UI dispatcher thread"
          );
      mock.Verify(
          v => v.AddFolderItems(It.IsAny<string[]>()),
          Times.Once(),
          "the combo box must still populate even though the archive-root read fails"
      );
      mock.Verify(
          v => v.SetFolderSuggestions(It.IsAny<IReadOnlyList<FolderRow>>()),
          Times.Once(),
          "suggestion rows must still populate even though the archive-root read fails"
      );
      mock.Verify(
          v => v.SetFolderSelectedItem(It.IsAny<string>()),
          Times.Never(),
          "no preselection can occur once the archive-root read fails"
      );
      mock.Verify(
          v => v.SetFolderSelectedIndex(It.IsAny<int>()),
          Times.Once(),
          "the index-fallback path must run instead of preselection"
      );
  }
  ```

  Acceptance (AC1, AC2, AC3 — evidence, not yet sign-off): the test method
  `AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing` exists in
  `Part2.cs` with the body above, verbatim.

- [ ] [P2-T2] [expect-fail] Build `QuickFiler.Test.csproj` (Debug|Any CPU) and run, against the
  UNCHANGED (pre-fix) production code, the single new test via the `vstest.console.exe` path
  recorded in `<FEATURE>/evidence/baseline/phase0-toolchain-paths.<TIMESTAMP>.md`:
  `<vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing" /InIsolation /Logger:trx /ResultsDirectory:<FEATURE>/evidence/regression-testing`.
  Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1`, and `Output Summary:` (must
  state 1 failed, 0 passed, and that the failure is an unhandled `InvalidOperationException` thrown
  from `AssignFolderComboBox`) in `<FEATURE>/evidence/regression-testing/phase2-expect-fail-run.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE` equals the declared `ExpectedExitCode: 1` (the test fails pre-fix, proving
  the regression is real and reproducible).

---

### Phase 3 — Minimal Fix

- [ ] [P3-T1] In `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, replace lines 231-234:

  ```csharp
                  string predetermined = ProjectPredeterminedFolder(
                      _predeterminedFolder,
                      _globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)
                  );
  ```

  with:

  ```csharp
                  string archiveRootPath;
                  try
                  {
                      archiveRootPath = _globals is null
                          ? null
                          : (_globals.Ol?.ArchiveRootPath ?? string.Empty);
                  }
                  catch (InvalidOperationException)
                  {
                      // #813: Ol.ArchiveRootPath throws when the archive root is unconfigured or
                      // unresolvable. Degrade to no preselection instead of propagating onto the UI
                      // dispatcher thread; ProjectPredeterminedFolder/ToDisplayStem already treat an
                      // empty archive root as the identity projection.
                      archiveRootPath = string.Empty;
                  }
                  string predetermined = ProjectPredeterminedFolder(
                      _predeterminedFolder,
                      archiveRootPath
                  );
  ```

  No other line in the file changes. The `_globals is null ? null : ...` branch is preserved
  unchanged inside the `try`. Acceptance (AC4 evidence, not yet sign-off): the file contains exactly
  one `catch (InvalidOperationException)` clause and the replaced block reads as above.

---

### Phase 4 — Regression Confirmation & AC1–AC3 Sign-off

- [ ] [P4-T1] Rebuild `QuickFiler.Test.csproj` (Debug|Any CPU) and re-run the exact command from
  P2-T2 (same `TestCaseFilter`, same `vstest.console.exe` path) against the now-fixed production
  code. Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (must state 1 passed, 0
  failed) in `<FEATURE>/evidence/regression-testing/phase4-post-fix-confirm.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0`, 1 passed, 0 failed.

- [ ] [P4-T2] Once P4-T1 passes, check off the first Acceptance Criteria checkbox in
  `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/spec.md`
  (the item beginning "A regression test exists in
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`") by changing its
  `- [ ]` to `- [x]`. Acceptance (AC1 sign-off): that single line reads `- [x]` and no other
  Acceptance Criteria checkbox in spec.md is changed by this task.

- [ ] [P4-T3] Check off the second Acceptance Criteria checkbox in spec.md (the item beginning "The
  same test asserts the folder combo box and suggestion rows are still populated") by changing its
  `- [ ]` to `- [x]`. Acceptance (AC2 sign-off): that single line reads `- [x]` and no other
  Acceptance Criteria checkbox in spec.md is changed by this task.

- [ ] [P4-T4] Check off the third Acceptance Criteria checkbox in spec.md (the item beginning "The
  same test asserts no preselection occurs") by changing its `- [ ]` to `- [x]`. Acceptance (AC3
  sign-off): that single line reads `- [x]` and no other Acceptance Criteria checkbox in spec.md is
  changed by this task.

---

### Phase 5 — Full C# Toolchain QA Loop & Coverage Delta

- [ ] [P5-T1] Run `dotnet tool run csharpier format QuickFiler/Controllers/QfcItemController.FolderHandling.cs QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`.
  Record `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` containing the literal
  `Formatted 2 files in` prefix CSharpier prints on a completed run for two files passed on the
  command line (this is the tool's standard summary line regardless of whether either file needed
  reformatting) in `<FEATURE>/evidence/qa-gates/phase5-csharpier-format.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0` and the literal prefix is present in the recorded output.

- [ ] [P5-T2] Run `dotnet tool run csharpier check QuickFiler/Controllers/QfcItemController.FolderHandling.cs QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`.
  Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
  `<FEATURE>/evidence/qa-gates/phase5-csharpier-check.<TIMESTAMP>.md`. Acceptance: `EXIT_CODE: 0`.
  If `EXIT_CODE` is nonzero, re-run P5-T1 and repeat this task until `EXIT_CODE: 0` (restart-from-
  formatting rule).

- [ ] [P5-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (must include `Build succeeded`
  and `0 Error(s)`) in `<FEATURE>/evidence/qa-gates/phase5-analyzer-rebuild.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0`. If nonzero or if this step changed any tracked file, restart the loop
  from P5-T1.

- [ ] [P5-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.
  Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (must include `Build succeeded`
  and `0 Error(s)`) in `<FEATURE>/evidence/qa-gates/phase5-nullable-rebuild.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0`. If nonzero or if this step changed any tracked file, restart the loop
  from P5-T1.

- [ ] [P5-T5] Run `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <FEATURE>/evidence/qa-gates/coverage-post-change.cobertura.xml`.
  Record `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` containing the total
  tests-passed/failed count (must show 0 failed, and must include the new
  `AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing` test in the
  passed count) and the repo-wide `line-rate`/`branch-rate` percentages read from the produced
  Cobertura XML, in `<FEATURE>/evidence/qa-gates/phase5-coverage-post-change.<TIMESTAMP>.md`.
  Acceptance: `EXIT_CODE: 0`, 0 failed tests, both percentages recorded as numeric values.

- [ ] [P5-T6] Compare `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` (P0-T8) against
  `<FEATURE>/evidence/qa-gates/coverage-post-change.cobertura.xml` (P5-T5). Record in
  `<FEATURE>/evidence/qa-gates/phase5-coverage-delta.<TIMESTAMP>.md`:
  `Baseline: <line-rate>% line / <branch-rate>% branch`,
  `PostChange: <line-rate>% line / <branch-rate>% branch`, and
  `NewCodeCoverage: <hit-count>/<line-count> lines hit` for every source line inside the
  `try`/`catch (InvalidOperationException)` block added in Phase 3, read from the per-line hit data
  for `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` in the post-change Cobertura XML.
  The Koverage post-processing step in `Invoke-MSTestWithCoverage.ps1` rewrites Cobertura `filename`
  attributes to native (backslash) separators, so perform the per-line lookup against the
  backslash-spelled path `QuickFiler\Controllers\QfcItemController.FolderHandling.cs` (or match on the
  bare filename) — a forward-slash lookup against the processed XML matches zero rows and must not be
  misread as 0% new-code coverage. Acceptance: post-change repo-wide line coverage is >= 85% and is
  not more than 0.5 percentage points below the baseline line-rate value (accounting for repo-wide
  Cobertura nondeterminism); post-change repo-wide branch coverage is not more than 0.5 percentage
  points below the baseline branch-rate value (no-further-regression only — per the plan-wide
  "Coverage floor resolution" note, the pre-existing 66.3978% repo-wide branch-coverage shortfall
  against the 75% floor is a dispositioned, unrelated gap this narrow fix is not responsible for
  closing); every line inside the added `try`/`catch (InvalidOperationException)` block shows a hit
  count > 0 (100% new-code coverage, exceeding the 90% new-code floor).

- [ ] [P5-T7] Once P5-T1 through P5-T6 all show a passing acceptance state in the same pass, check
  off the sixth Acceptance Criteria checkbox in spec.md (the item beginning "Full C# toolchain passes
  with no regression") by changing its `- [ ]` to `- [x]`. Acceptance (AC6 sign-off): that single
  line reads `- [x]`.

---

### Phase 6 — Scope-Boundary & Catch-Type Verification (AC4–AC5 Sign-off)

- [ ] [P6-T1] Grep `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` for the literal
  single-line token `catch (InvalidOperationException)`: it must appear exactly once, inside
  `AssignFolderComboBox`. Then extract only the `AssignFolderComboBox` method body (lines 191-250
  pre-fix, confirmed by direct read: the method opens at line 191 and its closing brace is at line
  250; lines 252-296 belong to two unrelated methods, `ProjectPredeterminedFolder` and
  `PopulateAndSelectFolder`, and must not be included in the extracted span. After the Phase 3 edit
  the method's closing brace shifts a few lines later to accommodate the added `try`/`catch` block;
  extract through the method's actual closing brace at execution time, not a fixed line number) and
  grep that extracted span only (not the whole file) for the literal tokens `catch (Exception` and
  `catch (System.Exception`: both must return zero matches within the extracted span. The whole-file
  form of this check is deliberately not used, because
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` contains three pre-existing,
  out-of-scope occurrences of the literal substring `catch (System.Exception` outside
  `AssignFolderComboBox` (a comment at line 74, and real catch clauses at lines 121 and 127 inside
  `LoadFolderHandlerAsync`), none of which this plan's Phase 3 edit touches; a whole-file zero-match
  assertion against that token is unsatisfiable regardless of this plan's change and would not test
  anything about the Phase 3 edit. Record both grep results, the extracted line range used,
  `Timestamp:`, `Command:`, `Output Summary:` in
  `<FEATURE>/evidence/qa-gates/phase6-catch-type-check.<TIMESTAMP>.md`. Acceptance: exactly one match
  for `catch (InvalidOperationException)` in the whole file, and zero matches for the two
  broader-catch tokens within the extracted `AssignFolderComboBox` method-body span only.

- [ ] [P6-T2] Check off the fourth Acceptance Criteria checkbox in spec.md (the item beginning "The
  fix in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` catches only
  `InvalidOperationException`") by changing its `- [ ]` to `- [x]`. Acceptance (AC4 sign-off): that
  single line reads `- [x]`.

- [ ] [P6-T3] Run `git merge-base HEAD main` to resolve the base SHA for this branch (per
  `pr-base-branch-merge-base`), then run
  `git diff --name-only <merge-base-sha> -- TaskMaster/AppGlobals/AppOlObjects.cs TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs TaskMaster/AppGlobals/ArchiveRootPathGuard.cs QuickFiler/Controllers/QfcHomeController.cs UtilitiesCS/Threading/ProgressViewer.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs" "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILInstruction.cs" "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs" UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs UtilitiesCS/OutlookObjects/Table/OlTableExtensions.RowTransforms.cs UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS/Extensions/DfDeedle.cs .editorconfig BannedSymbols.txt UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`,
  then also run `git status --porcelain -- TaskMaster/AppGlobals/AppOlObjects.cs TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs TaskMaster/AppGlobals/ArchiveRootPathGuard.cs QuickFiler/Controllers/QfcHomeController.cs UtilitiesCS/Threading/ProgressViewer.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs" "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILInstruction.cs" "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs" UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs UtilitiesCS/OutlookObjects/Table/OlTableExtensions.RowTransforms.cs UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS/Extensions/DfDeedle.cs .editorconfig BannedSymbols.txt UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`
  as a porcelain-status companion so an untracked file among the sibling-exclusion set (which the
  name-only diff alone cannot see) is also caught
  (all named paths, including the three SDIL reader production files under
  `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/`, confirmed to exist at plan-revision time via
  repository glob; a prior revision of this task incorrectly asserted no `*Sdil*`/`*SDIL*`-matching
  file existed in the tree and left the SDIL reader files uncovered by this explicit check, relying
  solely on the comprehensive allow-list check in P6-T4 as a backstop — that assertion was false and
  is corrected here by naming the three files explicitly). Record `Timestamp:`,
  both `Command:` lines, `EXIT_CODE:`, `Output Summary:` in
  `<FEATURE>/evidence/qa-gates/phase6-scope-boundary-check.<TIMESTAMP>.md`. Acceptance: both the diff
  output and the porcelain-status output are empty (none of the named files were modified, staged, or
  left untracked-and-changed).

- [ ] [P6-T4] Run `git diff --name-only <merge-base-sha> HEAD` (same base SHA from P6-T3) against
  the full working tree, then also run `git status --porcelain` (no pathspec) as a porcelain-status
  companion so any untracked file the commit-to-commit diff cannot see is still captured. Record the
  full file list from both commands, `Timestamp:`, both `Command:` lines, `Output Summary:` in
  the same `<FEATURE>/evidence/qa-gates/phase6-scope-boundary-check.<TIMESTAMP>.md` artifact.
  Acceptance: across both command outputs combined, the only source files listed are
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` and
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` (or the new
  `...Part3.cs`/csproj entry if Phase 2 created one); any remaining entries
  are limited to paths under
  `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/`.

- [ ] [P6-T5] Check off the fifth Acceptance Criteria checkbox in spec.md (the item beginning "No
  files owned by issue #812") by changing its `- [ ]` to `- [x]`. Acceptance (AC5 sign-off): that
  single line reads `- [x]`.

- [ ] [P6-T6] Measure the final line count of
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`. Record the count in
  `<FEATURE>/evidence/other/phase6-file-size-final.<TIMESTAMP>.md`. Acceptance: the count is <= 500.

---

### Phase 7 — Documentation & Spec Status Update

- [ ] [P7-T1] In spec.md, confirm all six Acceptance Criteria checkboxes now read `- [x]` (set by
  P4-T2, P4-T3, P4-T4, P6-T2, P6-T5, P5-T7). Record a summary in
  `<FEATURE>/evidence/other/phase7-spec-status-update.<TIMESTAMP>.md` listing all six checkbox texts
  and confirming each is checked. Acceptance: all six read `- [x]`.

- [ ] [P7-T2] Update spec.md's `- **Status:**` field from `Draft` to `Implemented` and its
  `- **Last Updated:**` field to the current date. Acceptance: both fields reflect the new values.

---

### Phase 8 — PR & Handoff Preparation

- [ ] [P8-T1] Write a PR-notes artifact summarizing the fix (one-line production change: narrow
  `try`/`catch (InvalidOperationException)` around the `Ol.ArchiveRootPath` read in
  `AssignFolderComboBox`; one new regression test in `Part2.cs`), the risk (none — behaviorally
  equivalent to the existing null-`_globals` path per research §2), and links to issue #813 and
  related issues #812 and #797, to `<FEATURE>/evidence/other/pr-notes.<TIMESTAMP>.md`, for use by a
  later `pr-author` skill run. This plan does not create or submit the PR. Acceptance: the artifact
  exists and names issues #813, #812, and #797.

---

### Phase 9 — Rollout & Follow-up Notes

- [ ] [P9-T1] Record spec.md's Rollout & Follow-up content verbatim (standard PR review and merge, no
  phased rollout or feature flag; post-merge verification that the regression test passes in CI;
  links to #813, #812, #797) into `<FEATURE>/evidence/other/rollout-followup.<TIMESTAMP>.md`.
  Acceptance: the artifact exists and reproduces that section's content.

- [ ] [P9-T2] Mirror the completion status of issue #813 into
  `<FEATURE>/evidence/issue-updates/issue-813.<TIMESTAMP>.md` per the Issue Update Mirroring
  convention: `Timestamp:`, the intended text, and `PostedAs: unknown` if the GitHub issue itself is
  not updated as part of this plan's execution (posting the update is a separate, later action not
  performed by this plan). Acceptance: the artifact exists with `Timestamp:` and `PostedAs:` fields.

---

## Acceptance Criteria Traceability Summary

| Spec.md AC | Satisfied by | Signed off by |
|---|---|---|
| AC1 (regression test exists, does not throw) | P2-T1, P4-T1 | P4-T2 |
| AC2 (AddFolderItems/SetFolderSuggestions invoked) | P2-T1, P4-T1 | P4-T3 |
| AC3 (SetFolderSelectedItem never called; index fallback runs) | P2-T1, P4-T1 | P4-T4 |
| AC4 (catches only InvalidOperationException) | P3-T1, P6-T1 | P6-T2 |
| AC5 (no #812/sibling files modified) | P6-T3, P6-T4 | P6-T5 |
| AC6 (full toolchain passes, no regression) | P5-T1–P5-T6 | P5-T7 |

---

## SELF-REVIEW: RE-DERIVED THIS PASS

This plan has been through four preflight revision rounds since initial authoring. Round 1 applied
three deltas (the corrected sibling-file paths in the plan-wide header/P6-T3 list; the added
branch-coverage disposition in the "Coverage floor resolution" note and P5-T6's acceptance; the
added backslash-path guidance for the post-processed Cobertura per-line lookup in P5-T6). Round 2
applied one further delta (P6-T1's broader-catch check rescoped from a whole-file grep to an
`AssignFolderComboBox` method-body span, because the whole-file form was unsatisfiable against three
pre-existing, out-of-scope `catch (System.Exception` occurrences), plus a citation-completeness
backfill for two round-1 facts. Round 3 corrected two further defects: P6-T1's stated method-body
span was wrong (it named "lines 191-296, through EOF," but `AssignFolderComboBox`'s actual
closing brace is at line 250; lines 252-296 belong to the unrelated `ProjectPredeterminedFolder` and
`PopulateAndSelectFolder` methods) and has been corrected to lines 191-250 pre-fix (with guidance to
extract through the method's actual closing brace post-fix, since Phase 3 shifts it a few lines
later); and P1-T1's descriptive prose mislabeled the 4-line replaced expression (lines 231-234) as a
"3-line expression," corrected to "4-line." Round 4 corrected one further defect: the plan-wide
header note, P6-T3, and this section had each asserted "no file matching `*Sdil*`/`*SDIL*` exists in
the current tree" as the reason spec.md's "SDIL Reader files" exclusion was left unenumerated; this
was false. `git ls-files -- "*SDIL*"` returns six tracked files, three of which
(`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, `ILInstruction.cs`, `MethodBodyReader.cs`)
are the production files that phrase names. All three prose locations and P6-T3's explicit
`git diff`/`git status` pathspecs have been corrected to name these three files. Every citation below
that a round's edit touched, added, or removed was re-read directly against the current repository
tree in that same pass; citations untouched by any revision round are unchanged from the initial
pass and are retained below for completeness of the traceability record:

- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` — lines 191 (`AssignFolderComboBox`
  start), 200-249 (guarded block), 206 (`EnsureBreadcrumbPipeline`), 212 (`AddFolderItems`), 219-222
  (`SetFolderSuggestions` guard), 231-234 (the exact unguarded expression this plan replaces),
  235-247 (preselect/fallback branch), 248 (`_selectedFolder` assignment), 262-268
  (`ProjectPredeterminedFolder`), 296 (EOF, confirmed 296 total lines).
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` — lines 1-8 (current
  `using` list), 21 (`partial class QfcItemController_FolderHandlingTests`), 163-204 and 266-308
  (the two existing `Ol.ArchiveRootPath`-stubbing tests this plan's new test follows), 361-363 (EOF,
  confirmed 363 total lines, insertion point for the new members).
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` — lines 21-25
  (`FolderController` private nested class), 100-113 (`BuildFolderHandlerWithArray`, confirmed it
  uses the single-`Outlook.Application`-parameter constructor and leaves `Suggestions` at its default
  `null!`), 115-118 (`SetPrivate` reflection helper).
- `QuickFiler.Test/QuickFiler.Test.csproj` — lines 186-187 (`Compile Include` list already contains
  `FolderHandlingTests.cs` and `FolderHandlingTests.Part2.cs`; confirms no new `Compile Include` is
  needed since this plan targets `Part2.cs`, not a new `Part3.cs`), lines 17 (`AssemblyName`) and 36
  (`OutputPath` = `bin\Debug\`, confirming the `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` test
  assembly path used in Phase 2/4).
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — lines 15-17 (namespace `UtilitiesCS`, not
  `UtilitiesCS.OutlookObjects.Folder`, confirming no new `using` is needed for `FolderPredictor`
  itself), 26-34 (`Application`-only ctor sets `_globals = null!`), 36-41 (`IApplicationGlobals`-only
  ctor sets `_globals` and `Suggestions = new FolderScorer()` — the ctor this plan's new
  `BuildFolderHandlerWithSuggestions` helper uses), 244-259 (`FolderRowArray` getter, confirmed it
  unconditionally reads `_globals.AF.RecentsList.Count` regardless of `Suggestions.Count`, which is
  why the new test's globals mock must configure `AF.RecentsList`), 264-269 (`Suggestions` public
  settable property, default `null!`).
- `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` — line 26 (parameterless public ctor), lines
  39-42 (`Count` property reads only the internal dictionary, no globals dependency, confirming
  `new FolderScorer().Count == 0` is safe with no further setup).
- `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs` — lines 5-17 (namespace `UtilitiesCS`;
  `Ol`, `AF` members confirmed).
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` — lines 9-16 (namespace `UtilitiesCS`; `App` and
  `ArchiveRootPath` members confirmed).
- `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` — lines 11-22 (namespace `UtilitiesCS`;
  `RecentsList` typed `SloLinkedList<string>`).
- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloLinkedList.cs` — lines
  13-29 (namespace `UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable`,
  confirming the new `using` this plan adds to `Part2.cs`; parameterless public ctor confirmed).
- `QuickFiler/Viewers/IItemViewer.cs` — lines 13-15 (namespace `QuickFiler`, confirming no new
  `using` is needed since `Part2.cs`'s enclosing namespace `QuickFiler.Controllers.Tests` nests under
  `QuickFiler`), 87, 93-96, 118, 192 (`AddFolderItems`, `SetFolderSuggestions`, `GetSelectedFolder`,
  `SetFolderSelectedIndex`, `SetFolderSelectedItem`, `FolderContains`, `InvokeRequired` signatures
  confirmed against the new test's Moq setup/verify expressions).
- `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/spec.md`
  — lines 60-82 (Scope & Non-Goals), 211-228 (the six Acceptance Criteria checkboxes this plan
  traces to and checks off), 244-251 (Rollout & Follow-up, mirrored verbatim in Phase 9).
- `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/research/research.2026-09-08T23-58.md`
  — §5 (Option A code shape, adapted in Phase 3 to preserve the existing `_globals is null` ternary
  verbatim per the delegation instruction), §4 (existing Moq pattern at lines 163/266 this plan's new
  test follows).
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — lines 1-13 (`-SearchRoot`, `-Configuration`,
  `-CoverageOutput` parameters confirmed for the Phase 0/Phase 5 coverage commands).
- `.claude/rules/csharp.md` — full file read this pass; confirmed the four-stage toolchain and the
  80%/90% coverage figures referenced in the plan-wide "Coverage floor resolution" note.
- `.claude/rules/general-unit-test.md` — full file read this pass (via prior tool context); confirmed
  the uniform 85%/75% floor referenced in the same note.
- Sibling-owned exclusion paths named in P6-T3 — each resolved via a repository glob in this pass:
  `QuickFiler/Controllers/QfcHomeController.cs`, `UtilitiesCS/Threading/ProgressViewer.cs`,
  `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` and `.Display.cs`,
  `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs`,
  `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions*.cs`,
  `UtilitiesCS/Threading/TimeOutTask.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, and
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` all exist at the paths now named
  in P6-T3. This round's re-derivation also found that a prior revision's claim — "no file matching
  `*Sdil*`/`*SDIL*` exists anywhere in the tree" — was false: `git ls-files -- "*SDIL*"` returns six
  tracked files, three of which are the production files spec.md's Scope & Non-Goals section names
  as "the SDIL reader files" (`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`,
  `ILInstruction.cs`, `MethodBodyReader.cs`; the other three are that production code's own test
  files under `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/`). This round corrects the plan-wide
  header note and P6-T3 to name the three production paths explicitly rather than asserting they are
  unenumerable.
- `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/policy-audit.2026-09-08T11-30.md`
  — re-read this round (round 2 self-review, backfilling a round-1 citation gap): lines 91, 168-169,
  and 180-182 confirmed the exact repo-wide branch-rate (66.3978%) and line-rate (86.0424%) figures
  the "Coverage floor resolution" note (plan-wide conventions) and P5-T6's acceptance condition rely
  on.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` — re-read this round (round 2
  self-review): lines 74 (a comment containing the literal substring `catch (System.Exception`), 121
  and 127 (real `catch (System.Exception ...)` clauses inside `LoadFolderHandlerAsync`, outside
  `AssignFolderComboBox`), confirming P6-T1's whole-file broader-catch form was unsatisfiable and
  motivating this round's rescoping of that task to the `AssignFolderComboBox` method-body span
  (lines 191-250 pre-fix; the method's closing brace is at line 250, not line 296 — lines 252-296
  belong to the unrelated `ProjectPredeterminedFolder` and `PopulateAndSelectFolder` methods).
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — re-read this round (round 2 self-review,
  backfilling a round-1 citation gap): lines 64 and 420 confirmed `ConvertTo-KoverageRelativePath`'s
  `-PathSeparator` parameter defaults to `[System.IO.Path]::DirectorySeparatorChar` (backslash on
  Windows), and line 436 confirmed `ConvertTo-KoverageCoberturaXml` rewrites each class node's
  `filename` attribute through that function, supporting P5-T6's backslash-path lookup guidance.

**Sibling-region re-check:** The two existing `Ol.ArchiveRootPath` tests in `Part2.cs` (lines
163-204, 266-308) were re-read alongside the target lines to confirm they remain unaffected by this
plan's additions (this plan only appends new members after line 361; it does not edit any existing
line in `Part2.cs`). The `AssignFolderComboBox` lines surrounding 231-234 (200-230 and 235-249) were
re-read to confirm no other line in the method changes, and that the `_selectedFolder` assignment at
line 248 and the fallback branch at lines 242-247 remain reachable and unmodified after the Phase 3
edit.

---

## PLANNER-INTERNAL-REVIEW: PASS

CITATION-TO-TREE: PASS
CITATION: QuickFiler/Controllers/QfcItemController.FolderHandling.cs | lines 231-234
CITATION: QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs | lines 1-8, 163, 266, 361-363
CITATION: QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs | lines 21-25, 100-113, 115-118
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | lines 17, 36, 186-187
CITATION: UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs | lines 26-41, 244-269
CITATION: UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs | lines 26, 39-42
CITATION: UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs | lines 5-17
CITATION: UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs | lines 9-16
CITATION: UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs | lines 11-22
CITATION: UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloLinkedList.cs | lines 13-29
CITATION: QuickFiler/Viewers/IItemViewer.cs | lines 13-15, 87, 93-96, 118, 192
CITATION: docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/spec.md | lines 60-82, 211-228, 244-251
CITATION: docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/research/research.2026-09-08T23-58.md | section 5, section 4
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 1-13
CITATION: docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/policy-audit.2026-09-08T11-30.md | lines 91, 168-169, 180-182
CITATION: QuickFiler/Controllers/QfcItemController.FolderHandling.cs | lines 74, 121, 127 (pre-existing catch (System.Exception outside AssignFolderComboBox)
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | lines 64, 420, 436
CITATION: UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs, ILInstruction.cs, MethodBodyReader.cs | existence confirmed via git ls-files -- "*SDIL*"
AC-TRACEABILITY: PASS
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1, P3-T1 | TESTS: P4-T1 | EVIDENCE: phase2-expect-fail-run, phase4-post-fix-confirm
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T1, P3-T1 | TESTS: P4-T1 | EVIDENCE: phase2-expect-fail-run, phase4-post-fix-confirm
AC-MAPPING: AC3 | IMPLEMENTATION: P2-T1, P3-T1 | TESTS: P4-T1 | EVIDENCE: phase2-expect-fail-run, phase4-post-fix-confirm
AC-MAPPING: AC4 | IMPLEMENTATION: P3-T1 | TESTS: P6-T1 | EVIDENCE: phase6-catch-type-check
AC-MAPPING: AC5 | IMPLEMENTATION: (no change to excluded files, by omission) | TESTS: P6-T3, P6-T4 | EVIDENCE: phase6-scope-boundary-check
AC-MAPPING: AC6 | IMPLEMENTATION: P5-T1, P5-T3, P5-T4 | TESTS: P5-T2, P5-T5, P5-T6 | EVIDENCE: phase5-csharpier-check, phase5-analyzer-rebuild, phase5-nullable-rebuild, phase5-coverage-post-change, phase5-coverage-delta
SCOPE-BOUNDARY: PASS
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
