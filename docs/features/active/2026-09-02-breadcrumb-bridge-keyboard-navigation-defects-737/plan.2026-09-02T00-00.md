# Plan: breadcrumb-bridge-keyboard-navigation-defects (Issue #737)

- Issue: #737
- Feature folder: `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737`
- Work Mode: full-bug
- AC source (sole): `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`'s `## Acceptance Criteria` section (7 items, spec.md lines 90-96)
- Write Set (exactly 3 files, per spec.md `## Write Set`):
  - `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
  - `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`
  - `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`

**Fail-closed evidence rule:** every baseline and final-QC command task writes its own evidence artifact under `<FEATURE>/evidence/<kind>/` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. If any required artifact is missing or incomplete, the corresponding plan checklist item MUST remain unchecked.

**Toolchain restart rule (applies to every Phase 5 toolchain task):** Format -> Lint (analyzer) -> Type-check (nullable) -> Test is one pass. Format (P5-T2) is expected to change files; that alone does not trigger a restart. If check (P5-T3), the analyzer rebuild (P5-T4), the nullable rebuild (P5-T5), or either test task (P5-T6, P5-T7) reports a non-zero `EXIT_CODE`, compare the failing file/diagnostic set against the matching Phase 0 baseline artifact. If the failing set is identical to baseline (pre-existing, not introduced by this fix), record that comparison in the task's evidence and proceed without restarting. If the failing set contains a Write Set file or any diagnostic absent from baseline, restart the loop from P5-T1.

**MSBuild terminal-summary observation basis:** `msbuild`/`Invoke-VSBuild.ps1` always prints a build-ending summary line reading either `Build succeeded.` or `Build FAILED.`, followed by counts of the form `<N> Warning(s)` and `<N> Error(s)`. This is MSBuild's version-stable terminal summary format (present in every MSBuild release used by this repo's own CI workflow) and is the literal text every analyzer/nullable rebuild task below must record; it is not a prediction about this specific build's content.

**`msbuild` is not on PATH in this environment.** Every rebuild task below uses `scripts\vscode\Invoke-VSBuild.ps1`, which resolves `MSBuild.exe` via `vswhere` internally. The Write Set contains no `.csproj`, so the wrapper's `Sync-PackageReferences.ps1` HintPath rewrite (which would itself violate a Write-Set-only scope on a feature that forbids a `.csproj`) is not a concern here. The wrapper throws on a non-zero MSBuild exit code, so `EXIT_CODE:` in every rebuild evidence artifact is the invoking `pwsh` process's own exit code, not a raw MSBuild code.

### Phase 0 — Policy Reads & Baseline Capture

- [ ] [P0-T1] Read the repository-root policy file `./CLAUDE.md` in full.
- [ ] [P0-T2] Read `.claude/rules/general-code-change.md` in full.
- [ ] [P0-T3] Read `.claude/rules/general-unit-test.md` in full.
- [ ] [P0-T4] Read `.claude/rules/csharp.md` in full.
- [ ] [P0-T5] Read `.claude/rules/quality-tiers.md` in full.
- [ ] [P0-T6] Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/phase0-instructions-read.2026-09-02T00-00.md` containing `Timestamp:`, `Policy Order:` (1. CLAUDE.md, 2. .claude/rules/general-code-change.md, 3. .claude/rules/general-unit-test.md, 4. .claude/rules/csharp.md, 5. .claude/rules/quality-tiers.md), and the explicit list of the five files read in P0-T1 through P0-T5. Acceptance: the file exists and contains all five listed paths.
- [ ] [P0-T7] Bootstrap the pinned .NET SDK: run `pwsh -File scripts\vscode\Install-RepoDotNetSdk.ps1`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-sdk-bootstrap.2026-09-02T00-00.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (the `pwsh` process exit code), and `Output Summary:` recording the resolved SDK version line the script prints. Acceptance: `EXIT_CODE: 0`.
- [ ] [P0-T8] Restore the CSharpier tool pinned by the repo-root `dotnet-tools.json` (csharpier 1.2.6): run `dotnet tool restore` at the repo root. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-tool-restore.2026-09-02T00-00.md` with the four required fields. Acceptance: `EXIT_CODE: 0`.
- [ ] [P0-T9] Restore NuGet packages: run `pwsh -File scripts\vscode\Invoke-Restore.ps1`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-nuget-restore.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P0-T10] Baseline CSharpier check (read-only, repo-wide, per CLAUDE.md's C#1 approved command): run `dotnet tool run csharpier check .`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-csharpier-check.2026-09-02T00-00.md` with `Output Summary:` recording the tool's printed pass/fail summary line and, if non-zero, the list of files it reports as unformatted (this list is the baseline comparison set for the Phase 5 restart rule). Acceptance: artifact exists with all four fields populated (an `EXIT_CODE` other than 0 is a valid, recorded baseline outcome, not a task failure).
- [ ] [P0-T11] Baseline analyzer rebuild: run `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-analyzer-rebuild.2026-09-02T00-00.md` with `Output Summary:` recording the `Build succeeded.`/`Build FAILED.` line and the trailing Warning(s)/Error(s) counts. Acceptance: `EXIT_CODE: 0`.
- [ ] [P0-T12] Baseline nullable rebuild: run `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-nullable-rebuild.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P0-T13] Baseline scoped vstest run against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` (the Write Set's single shared test assembly), scoped to the two touched test classes:
  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbHtmlRendererTests" "/Logger:trx;LogFileName=baseline-scoped.trx" /ResultsDirectory:docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737\evidence\baseline\testresults
  ```
  Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-vstest-scoped.2026-09-02T00-00.md` with `Output Summary:` recording vstest's printed `Passed!`/`Failed!` line with its Failed/Passed/Skipped/Total counts. Acceptance: `EXIT_CODE: 0` and the printed summary line reads `Failed: 0`. `FolderBreadcrumbBridgeRouterTests` is a `partial class` split across two files: `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (14 `[TestMethod]` items) and the sibling partial file `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` (12 more `[TestMethod]` items declared on the same `public sealed partial class FolderBreadcrumbBridgeRouterTests`), and both sets share the class's `FullyQualifiedName`, so both are matched by the `FullyQualifiedName~FolderBreadcrumbBridgeRouterTests` filter above. Combined with `BreadcrumbHtmlRendererTests`'s 14 `[TestMethod]` items (all counts verified by direct read of the current tree), `Total: 40` is the expected baseline count.
- [ ] [P0-T14] Baseline full-repository coverage capture via `scripts\vscode\Invoke-MSTestWithCoverage.ps1`, which has a documented `.Count` StrictMode defect on any `-SearchRoot` narrower than the repo root, so `-SearchRoot .` is mandatory:
  ```
  pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\breadcrumb-737-baseline.cobertura.xml
  [xml]$cov = Get-Content coverage\breadcrumb-737-baseline.cobertura.xml -Raw
  $cov.coverage.'line-rate'; $cov.coverage.'lines-covered'; $cov.coverage.'lines-valid'
  ```
  Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/baseline/baseline-coverage-fullrepo.2026-09-02T00-00.md` with `Output Summary:` recording the numeric `line-rate` (as a percentage), `lines-covered`, and `lines-valid` values read from the emitted Cobertura XML's `/coverage` element. Acceptance: `EXIT_CODE: 0` and all three numeric values recorded.

### Phase 1 — Finding 1 (#640): Scroll-Into-View in `BridgeJs`

- [ ] [P1-T1] Edit `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`. Insert exactly the following two new lines immediately after the line `+ "      }\n"` that closes the `render`/`subfolderResult` `if`/`else if` block (currently file line 133, the closing brace matching the `+ "      if (msg.type === 'render') {\n"` opening at file line 114) and immediately before the line `+ "    });\n"` that closes the `addEventListener('message', ...)` callback (currently file line 134):
  ```
              + "      var scrollTarget = document.querySelector('.rowwrap.selected');\n"
              + "      if (scrollTarget) { scrollTarget.scrollIntoView({ block: 'nearest' }); }\n"
  ```
  No other line in the file changes. Acceptance: the two lines above are present at that position and every other line of the file is byte-identical to the pre-edit tree.
- [ ] [P1-T2] Verify the Finding 1 addition with a fixed-string search: `Select-String -Path UtilitiesCS\OutlookObjects\Folder\BreadcrumbDocumentAssets.cs -SimpleMatch -Pattern "scrollIntoView({ block: 'nearest' })"`. Acceptance: exactly one match.
- [ ] [P1-T3] Check off AC1 in `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`: change line 90 from `- [ ] In \`UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs\`'s \`BridgeJs\` constant, the inbound message listener scrolls the current \`.rowwrap.selected\` element into view (\`scrollIntoView({ block: 'nearest' })\`) after a \`render\` or \`subfolderResult\` DOM update, addressing Finding 1 (#640).` to the identical text with `- [x]`. Acceptance: only the checkbox marker changes; the criterion text is unchanged.

### Phase 2 — Finding 2 (#641): Enter-Key Binding in `BridgeJs`

- [ ] [P2-T1] Edit `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`. Insert exactly the following seven new lines immediately after the line `+ "  document.addEventListener('keydown', function (e) {\n"` (file line 101, unchanged by Phase 1) and immediately before the line `+ "    var map = { ArrowLeft: 'Left', ArrowRight: 'Right', ArrowUp: 'Up', ArrowDown: 'Down' };\n"` (file line 102, unchanged by Phase 1):
  ```
              + "    if (e.key === 'Enter') {\n"
              + "      var selected = document.querySelector('.rowwrap.selected');\n"
              + "      var id = selected ? selected.getAttribute('data-row-id') : '';\n"
              + "      post({ type: 'rowSelected', rowId: id });\n"
              + "      e.preventDefault();\n"
              + "      return;\n"
              + "    }\n"
  ```
  This task must not touch the Phase 1 insertion, must not add any new `BreadcrumbMessageTypes` constant, `IsKnownInboundType` branch, or `ProcessInboundAsync` case in any other file, and must not modify any file other than `BreadcrumbDocumentAssets.cs`. Acceptance: the seven lines above are present at that position and every other line of the file (including the Phase 1 insertion) is unchanged.
- [ ] [P2-T2] Verify the Finding 2 addition with two fixed-string searches: (a) `Select-String -Path UtilitiesCS\OutlookObjects\Folder\BreadcrumbDocumentAssets.cs -SimpleMatch -Pattern "e.key === 'Enter'"` — exactly one match; (b) `Select-String -Path UtilitiesCS\OutlookObjects\Folder\BreadcrumbDocumentAssets.cs -SimpleMatch -Pattern "post({ type: 'rowSelected', rowId: id });"` — exactly one match (the pre-existing click handler posts `rowId: rowId`, a different literal, so this token is unique to the new Enter branch). Acceptance: both searches return exactly one match each.
- [ ] [P2-T3] Check off AC2 in `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`: change line 91 from `- [ ] In the same \`BridgeJs\` constant, the \`keydown\` listener includes an \`Enter\` branch that posts \`{ type: 'rowSelected', rowId: id }\` using the same \`.rowwrap.selected\` lookup the arrow-key handler already uses, addressing Finding 2 (#641), and requires no new C#-side message type, codec branch, or router case.` to the identical text with `- [x]`.

### Phase 3 — `BreadcrumbHtmlRendererTests.cs`: New Test for Findings 1 and 2

- [ ] [P3-T1] Edit `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`. Insert the following new test method immediately after the closing `}` of `Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation` (file line 94) and immediately before the blank line preceding the `[TestMethod]` attribute of `RenderRowFragment_EveryRowKind_EmitsTrailingPctFlexItem` (file line 96):
  ```

          /// <summary>
          /// Issue #737 Findings 1 and 2: string-containment assertion against the public
          /// <see cref="BreadcrumbDocumentAssets.BridgeJs"/> constant, following the
          /// <see cref="Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation"/>
          /// precedent. This repository's test suite has no headless-browser or JS-engine
          /// dependency, so this test verifies the JS text is present and correctly shaped --
          /// not that it executes correctly in a real WebView2/Chromium document.
          /// </summary>
          [TestMethod]
          public void Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView()
          {
              // Act
              string bridgeJs = BreadcrumbDocumentAssets.BridgeJs;

              // Assert
              bridgeJs.Should().Contain("e.key === 'Enter'");
              bridgeJs.Should().Contain("post({ type: 'rowSelected', rowId: id });");
              bridgeJs.Should().Contain("scrollIntoView({ block: 'nearest' })");
          }
  ```
  No existing test method in the file is modified. Acceptance: the new method is present verbatim at that position and every other line of the file is unchanged. `UtilitiesCS.OutlookObjects.Folder` (carrying `BreadcrumbDocumentAssets`) is already `using`-imported at file line 6, so no new `using` is required.
- [ ] [P3-T2] Rebuild the solution so the new test method and the Phase 1/2 JS edits are compiled into `UtilitiesCS.Test.dll`: run `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-build-phase3.2026-09-02T00-00.md` recording the `Build succeeded.`/`Build FAILED.` line. Acceptance: `EXIT_CODE: 0`.
- [ ] [P3-T3] Run the new test in isolation: vswhere-resolved vstest (same resolution pattern as P0-T13) against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` with `/TestCaseFilter:"FullyQualifiedName~Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView"` `/InIsolation` `/Settings:scripts\vscode\TaskMaster.cli.runsettings` `"/Logger:trx;LogFileName=phase3-new-test.trx"` `/ResultsDirectory:docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737\evidence\qa-gates\testresults`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-vstest-phase3-new-test.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0` and the printed summary line reads `Passed: 1, Failed: 0`.
- [ ] [P3-T4] Check off AC3 in `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`: change line 92 from `- [ ] A new MSTest test method in \`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs\`, following the existing \`Issue439...\` string-containment precedent, asserts the rendered document (or the \`BreadcrumbDocumentAssets.BridgeJs\` constant directly) contains the Enter-triggered \`rowSelected\` post and the \`scrollIntoView\` call, with the JS-execution-harness limitation documented in the test's own comment or docstring.` to the identical text with `- [x]`.

### Phase 4 — Finding 3 (#693): Router Test Assertion Fix

- [ ] [P4-T1] Edit `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`. Replace the Arrange/Act/Assert body of `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` (currently file lines 373-384, between the `// Arrange:` comment block ending at line 372 and the method's closing `}` at line 385) exactly as follows.

  OLD (lines 373-384):
  ```
              var router = await PopulatedRouterAsync(ProviderMock());
              await ArrowAsync(router, "left");
              await ArrowAsync(router, "left");

              // Act
              var outputs = await ArrowAsync(router, "left");

              // Assert
              outputs.Should().ContainSingle();
              ((UnhandledArrowMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
                  .Direction.Should()
                  .Be(BreadcrumbArrowDirection.Left);
  ```

  NEW:
  ```
              var router = await PopulatedRouterAsync(ProviderMock());
              var firstPress = await ArrowAsync(router, "left");
              var secondPress = await ArrowAsync(router, "left");

              // Act
              var outputs = await ArrowAsync(router, "left");

              // Assert
              firstPress.Should().ContainSingle();
              BreadcrumbBridgeSerializer.Parse(firstPress[0]).Should().BeOfType<RenderMessage>();
              secondPress.Should().ContainSingle();
              BreadcrumbBridgeSerializer.Parse(secondPress[0]).Should().BeOfType<RenderMessage>();
              outputs.Should().ContainSingle();
              ((UnhandledArrowMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
                  .Direction.Should()
                  .Be(BreadcrumbArrowDirection.Left);
  ```
  This task must not modify the `ArrowAsync` helper (file lines 429-436), `PopulatedRouterAsync` (file lines 78-91), or any provider-mock factory (`ProviderMock`, `StemProviderMock`, `ParentSubfolderProviderMock`) in this file, and must not modify any other test method. Acceptance: only the quoted block changes; every other line of the file is unchanged.
- [ ] [P4-T2] Rebuild the solution so the Finding 3 edit is compiled: run `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-build-phase4.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P4-T3] Run the modified test in isolation: vswhere-resolved vstest against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` with `/TestCaseFilter:"FullyQualifiedName~Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft"` `/InIsolation` `/Settings:scripts\vscode\TaskMaster.cli.runsettings` `"/Logger:trx;LogFileName=phase4-modified-test.trx"` `/ResultsDirectory:docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737\evidence\qa-gates\testresults`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-vstest-phase4-modified-test.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0` and the printed summary line reads `Passed: 1, Failed: 0`.
- [ ] [P4-T4] Run the sibling #440 regression test to confirm the Finding 3 fix is consistent with it: vswhere-resolved vstest with `/TestCaseFilter:"FullyQualifiedName~ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition"` `/InIsolation` `/Settings:scripts\vscode\TaskMaster.cli.runsettings` `"/Logger:trx;LogFileName=phase4-sibling-test.trx"` `/ResultsDirectory:docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737\evidence\qa-gates\testresults`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-vstest-phase4-sibling-test.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0` and the printed summary line reads `Passed: 1, Failed: 0`.
- [ ] [P4-T5] Check off AC4 in `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`: change line 93 from `- [ ] In \`UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs\`, \`Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft\` captures both previously-discarded \`ArrowAsync(router, "left")\` results and asserts each parses to a \`RenderMessage\`, addressing Finding 3 (#693), without modifying the \`ArrowAsync\` helper signature or any shared provider-mock/router factory in the file.` to the identical text with `- [x]`.
- [ ] [P4-T6] Check off AC5 in `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`: change line 94 from `- [ ] The fix for Finding 3 preserves the #440 ancestor-walk contract already documented in the test's in-code comment (two presses to reach the root on the three-segment fixture; \`UnhandledArrowMessage\` only on the third press), and is consistent with the sibling test \`ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition\`.` to the identical text with `- [x]`.

### Phase 5 — Final QC: Full C# Toolchain, Scope Verification, Coverage Delta, AC Closeout

- [ ] [P5-T1] Capture pre-format SHA-256 hashes of the three Write Set files: `Get-FileHash -Algorithm SHA256 UtilitiesCS\OutlookObjects\Folder\BreadcrumbDocumentAssets.cs, UtilitiesCS.Test\OutlookObjects\Folder\FolderBreadcrumbBridgeRouterTests.cs, UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbHtmlRendererTests.cs`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-csharpier-pre-format-hashes.2026-09-02T00-00.md` recording all three hashes. Acceptance: three hashes recorded.
- [ ] [P5-T2] Run scoped CSharpier format (write-mode; scoped to the Write Set only, per repo convention that a repo-wide format pass can break a zero-diff-outside-Write-Set acceptance condition): `dotnet tool run csharpier format UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`. Then re-run `Get-FileHash -Algorithm SHA256` on the same three files and compare against P5-T1. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-csharpier-format.2026-09-02T00-00.md` with `Output Summary:` recording the tool's printed `Formatted N files` line (explicitly labeled as a processed-file count, not a rewritten-file count, per this tool's documented behavior) and the per-file changed/unchanged determination from the hash comparison. Acceptance: `EXIT_CODE: 0`.
- [ ] [P5-T3] Repo-wide CSharpier check (read-only verification): `dotnet tool run csharpier check .`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-csharpier-check.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0`, applying the Toolchain Restart Rule above if not (compare the failing file list against P0-T10's baseline).
- [ ] [P5-T4] Analyzer rebuild (repo-wide): `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-analyzer-rebuild.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P5-T5] Nullable rebuild (repo-wide): `pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-nullable-rebuild.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P5-T6] Scoped vstest run covering every touched or added test (same `TestCaseFilter` shape as P0-T13): `/TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbHtmlRendererTests"` `/InIsolation` `/Settings:scripts\vscode\TaskMaster.cli.runsettings` `"/Logger:trx;LogFileName=qa-scoped.trx"` `/ResultsDirectory:docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737\evidence\qa-gates\testresults`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-vstest-scoped.2026-09-02T00-00.md`. Acceptance: `EXIT_CODE: 0` and the printed summary line reads `Failed: 0` with `Total: 41` (the 40-test P0-T13 baseline -- 14 in `FolderBreadcrumbBridgeRouterTests.cs`, 12 in the sibling partial `FolderBreadcrumbBridgeRouterInFlightTests.cs`, 14 in `BreadcrumbHtmlRendererTests.cs` -- plus exactly the one new `[TestMethod]` added in P3-T1; Phase 4 modified an existing test's body without adding or removing a method).
- [ ] [P5-T7] Final full-repository coverage capture via `scripts\vscode\Invoke-MSTestWithCoverage.ps1`:
  ```
  pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\breadcrumb-737-final.cobertura.xml
  [xml]$cov = Get-Content coverage\breadcrumb-737-final.cobertura.xml -Raw
  $cov.coverage.'line-rate'; $cov.coverage.'lines-covered'; $cov.coverage.'lines-valid'
  ```
  Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-coverage-fullrepo.2026-09-02T00-00.md` with `Output Summary:` recording the numeric `line-rate` (as a percentage), `lines-covered`, and `lines-valid` read from the emitted XML. Acceptance: `EXIT_CODE: 0` and all three numeric values recorded.
- [ ] [P5-T8] Coverage delta verification: compare P0-T14's baseline `line-rate`/`lines-covered`/`lines-valid` against P5-T7's final values. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-coverage-delta.2026-09-02T00-00.md` recording both sets of numbers and stating explicitly that `BreadcrumbDocumentAssets.cs`'s Phase 1/2 edits add only `const string` literal content (no new executable IL line is emitted for a compile-time constant initializer), so the sole production file in the Write Set introduces zero new coverable lines; the other two Write Set files are test files excluded from the coverage denominator. Acceptance: final `lines-covered` / `lines-valid` ratio is not lower than baseline's, and the artifact states the zero-new-coverable-line basis above.
- [ ] [P5-T9] Check off AC7 in `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`: change line 96 from `- [ ] The full C# toolchain (csharpier format/check, analyzer rebuild, nullable rebuild, vstest with coverage) passes cleanly in a single pass, per CLAUDE.md and \`.claude/rules/general-code-change.md\`, with no reduction in coverage on changed lines.` to the identical text with `- [x]`. Only perform this task after P5-T2 through P5-T8 have all completed with their stated acceptance conditions met in one uninterrupted pass (no restart triggered).
- [ ] [P5-T10] Pre-commit scope verification against `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/` (working-tree-inclusive, so no commit is required first):
  ```
  $mergeBase = git merge-base origin/main HEAD
  git diff --name-only $mergeBase | Where-Object { $_ -notmatch '^docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/' -and $_ -notmatch '^\.claude/agent-memory/' } | Sort-Object
  git status --porcelain
  ```
  Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-scope-check.2026-09-02T00-00.md` recording both command outputs. `.claude/agent-memory/atomic-planner/MEMORY.md` is excluded from this filter because it already carries an uncommitted, pre-existing modification unrelated to this feature's Write Set, confirmed via `git diff --name-only $(git merge-base origin/main HEAD)` returning exactly that one path before any plan task in this cycle ran; the exclusion is scoped to the `.claude/agent-memory/` tree only. Acceptance: the filtered `git diff --name-only` output is exactly these three lines, sorted:
  ```
  UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs
  UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
  UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs
  ```
- [ ] [P5-T11] Check off AC6 in `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`: change line 95 from `- [ ] No file outside the Write Set is modified. In particular, no Qfc-pipeline file (QuickFiler/Resources/FolderBreadcrumb.html, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs, QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs) and no #440 production logic in QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs is altered.` to the identical text with `- [x]`. Only perform this task after P5-T10 confirms the exact three-file scope.
- [ ] [P5-T12] Update the `### Acceptance Criteria Status` section at the bottom of `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md` (current lines 100-104): change `Checked off (delivered): 0` to `Checked off (delivered): 7`, change `Remaining (unchecked): 7` to `Remaining (unchecked): 0`, and change `Items remaining: all seven items above (no implementation has occurred yet; this spec is a planning artifact only)` to `Items remaining: none`. Acceptance: all three lines updated as stated and no other line in that section changes.
- [ ] [P5-T13] Commit the Write Set and `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/`. Before staging, remove any stray `plan.*.md` sibling file in the feature folder other than the canonical `plan.2026-09-02T00-00.md`: this is a defensive general-form guard, not a one-off cleanup of a specific filename, against active-folder-creation tooling (or any other process) re-creating a bootstrap-template stub plan file before this task's `git add` runs; the subsequent `git add` of the feature-folder directory would otherwise silently sweep such a stray file into this feature's commit. This guard must never remove the canonical plan file itself.
  ```
  Get-ChildItem -Path docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737 -Filter 'plan.*.md' | Where-Object { $_.Name -ne 'plan.2026-09-02T00-00.md' } | Remove-Item -ErrorAction Stop
  git add UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/
  git commit -m "fix(#737): breadcrumb bridge scroll-into-view, Enter key binding, and #440 router-test assertion fix

  Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01LTjXvNFHVh7Fo7kYGgWsx2"
  ```
  Acceptance: after the `Get-ChildItem`/`Remove-Item` step, `Get-ChildItem -Path docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737 -Filter 'plan.*.md'` returns exactly one item (`plan.2026-09-02T00-00.md`), and `git commit` exits 0.
- [ ] [P5-T14] Post-commit verification: `git diff --name-only origin/main...HEAD | Where-Object { $_ -notmatch '^docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/' -and $_ -notmatch '^\.claude/agent-memory/' } | Sort-Object` and `git status --porcelain | Where-Object { $_ -notmatch '\.claude/agent-memory/' }`. Write `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/evidence/qa-gates/qa-post-commit-verification.2026-09-02T00-00.md` recording both raw outputs and the filtered porcelain output. The same `.claude/agent-memory/` exclusion applies to both commands here as it does to the diff in P5-T10, for the identical reason: `.claude/agent-memory/atomic-planner/MEMORY.md`'s pre-existing uncommitted modification is not part of this task's `git add` in P5-T13 and remains dirty after the commit, so an unfiltered porcelain check would report it and falsely fail this task. Acceptance: the filtered diff output is identical to P5-T10's corrected three-line list and the filtered `git status --porcelain` output prints nothing.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`
- Total AC items: 7
- Checked off (delivered): 0 (updated by executor as P1-T3, P2-T3, P3-T4, P4-T5, P4-T6, P5-T9, P5-T11 complete)
- Remaining (unchecked): 7

## Planner Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

This is a round-3 revision pass responding to one atomic-executor preflight defect (Defect D: a stray sibling `plan.*.md` bootstrap-template stub in the feature folder, already deleted from the working tree by the orchestrator, that P5-T13's `git add` of the feature-folder directory would otherwise have swept into the feature's commit). Round 1 and round 2 defects (test-count arithmetic, the scope-verification gate exclusion, and self-embedded signal lines) were independently re-verified as correctly fixed by a separate round-2 atomic-executor preflight pass and are not re-derived here, per the contract's instruction that this pass's obligation is to re-derive the citations this pass's own edit touches and to re-check the sibling region of those citations. Every citation below was re-derived directly against the current tree in this pass:

- `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/` (feature folder listing) | Re-derived this pass via a `plan.*.md` glob against the live feature folder: exactly one match, `plan.2026-09-02T00-00.md` (the canonical plan path). No stray `plan.*.md` sibling file is present in the current tree; this confirms the orchestrator's stated deletion took effect and that no different or additional stray file exists. This is the direct precondition citation for Defect D's fix.
- `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/plan.2026-09-02T00-00.md` P5-T13 (this file, this pass's edit) | Re-derived this pass: the task now opens with a `Get-ChildItem -Path <feature-folder> -Filter 'plan.*.md' | Where-Object { $_.Name -ne 'plan.2026-09-02T00-00.md' } | Remove-Item -ErrorAction Stop` step, preceding the existing `git add`/`git commit` span, with explanatory prose stating the general-form defensive purpose and the constraint that the canonical file must never be removed; the acceptance condition now requires the same `Get-ChildItem -Filter 'plan.*.md'` query to return exactly one item after the removal step, in addition to the pre-existing `git commit` exit-0 clause. The command uses the same `Get-ChildItem`/`Where-Object`/pipe idiom already established by P5-T1 (`Get-FileHash ...`) and P5-T10 (`Where-Object { ... }`) in this plan, so it matches this plan's established PowerShell style rather than introducing a new one.
- P5-T10 (sibling task, re-checked this pass, not edited) | Re-derived: P5-T10's `git diff --name-only $mergeBase` scope-check runs before P5-T13 in task order and inspects only tracked-file diffs against `$mergeBase`; an untracked stray `plan.*.md` file would not appear in that `git diff --name-only` output regardless of the new removal step's position, and P5-T10's acceptance condition gates only on the filtered `git diff --name-only` three-line list, not on the recorded `git status --porcelain` output. No change to P5-T10 is required: the new removal step in P5-T13 runs after P5-T10 already completed and does not alter what P5-T10 observes or gates on.
- P5-T11 (sibling task, re-checked this pass, not edited) | Re-derived: P5-T11 checks off AC6 based on P5-T10's confirmed three-file scope and has no dependency on P5-T13's commit mechanics or the new removal step. No interaction found; no change required.
- P5-T14 (sibling task, re-checked this pass, not edited) | Re-derived: P5-T14's post-commit `git status --porcelain` (filtered for `.claude/agent-memory/`) runs after P5-T13. Because the new removal step deletes any stray `plan.*.md` file from disk (via `Remove-Item`) before P5-T13's `git add`/`git commit`, no such file can exist as an untracked or staged artifact by the time P5-T14 runs, so P5-T14's existing acceptance clause (filtered porcelain output prints nothing) is unaffected and remains satisfiable. No change to P5-T14 is required.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`, `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`, `.claude/agent-memory/atomic-planner/MEMORY.md` | Not touched by this pass's edit and not in the same file/region as the P5-T13 citation (this pass's sole edit is the removal step inserted into P5-T13's command block and acceptance text); round 2's re-derivation of these stands unchanged and is not superseded by this pass.

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS

CITATION-TO-TREE: PASS

CITATION: UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs | lines 101-109 (keydown listener), lines 110-136 (render/subfolderResult listener), line 133 (render/subfolderResult closing brace), line 134 (addEventListener closing), line 98 (pre-existing rowSelected literal)
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs | lines 367-385 (Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft), lines 373-384 (Arrange/Act/Assert body replaced), lines 429-436 (ArrowAsync helper, unmodified), lines 78-91 (PopulatedRouterAsync, unmodified), lines 442-467 (sibling #440 test, unmodified), line 24 (public sealed partial class declaration), 14 [TestMethod] items re-counted this pass via grep
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs | partial-class sibling of FolderBreadcrumbBridgeRouterTests, 12 [TestMethod] items re-counted this pass via grep, line 21 (public sealed partial class declaration)
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs | lines 6 (using import), lines 66-94 (Issue439... precedent test), lines 94-96 (insertion point), 14 [TestMethod] items re-counted this pass via grep
CITATION: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md | lines 90-96 (7 AC bullets), lines 98-104 (AC Status section)
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | AssemblyName=UtilitiesCS.Test, OutputPath=bin\Debug\
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | full-repo-only SearchRoot requirement (no test-filter parameter)
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | line-rate/lines-covered/lines-valid attribute emission in Assert-CoberturaLineCoverageThreshold and ConvertTo-KoverageCoberturaXml
CITATION: scripts/vscode/Invoke-VSBuild.ps1 | -Target Rebuild/-EnableNETAnalyzers/-EnforceCodeStyleInBuild/-TreatWarningsAsErrors switches, internal vswhere MSBuild resolution
CITATION: .claude/agent-memory/atomic-planner/agent-memory-is-tracked-scope-git-gates.md | documented repo-layout property that .claude/agent-memory/** is tracked and routinely already modified at branch head, the basis for the P5-T10/P5-T14 diff-and-porcelain exclusion
CITATION: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/plan.2026-09-02T00-00.md | re-derived this pass: zero line-anchored ^PREFLIGHT:/^CONVERGENCE: matches remain in the plan body after the "## Preflight Handoff" section was reworded to "## Handoff Note"
CITATION: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/ (feature folder) | re-derived this pass via a plan.*.md glob against the live tree: exactly one match, plan.2026-09-02T00-00.md, confirming no stray sibling plan file currently exists (the basis for the P5-T13 defensive-removal addition)
CITATION: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/plan.2026-09-02T00-00.md | P5-T13 (this pass's edit): Get-ChildItem -Filter 'plan.*.md' / Where-Object Name -ne 'plan.2026-09-02T00-00.md' / Remove-Item -ErrorAction Stop precedes the existing git add/git commit span; acceptance now additionally requires a single-item Get-ChildItem -Filter 'plan.*.md' result after removal

AC-TRACEABILITY: PASS

SCOPE-BOUNDARY: PASS
Basis: every implementation task is confined to the 3 Write Set files; every command task either reads state or is scoped to the Write Set; the sole repo-wide write-mode command, P5-T2, is explicitly restricted to the 3 Write Set file arguments rather than `.`.

AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7

AC-MAPPING: AC1 | IMPLEMENTATION: P1-T1 | TESTS: P1-T2, P5-T6 | EVIDENCE: qa-gates/qa-vstest-scoped.2026-09-02T00-00.md
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T1 | TESTS: P2-T2, P5-T6 | EVIDENCE: qa-gates/qa-vstest-scoped.2026-09-02T00-00.md
AC-MAPPING: AC3 | IMPLEMENTATION: P3-T1 | TESTS: P3-T3, P5-T6 | EVIDENCE: qa-gates/qa-vstest-phase3-new-test.2026-09-02T00-00.md
AC-MAPPING: AC4 | IMPLEMENTATION: P4-T1 | TESTS: P4-T3, P5-T6 | EVIDENCE: qa-gates/qa-vstest-phase4-modified-test.2026-09-02T00-00.md
AC-MAPPING: AC5 | IMPLEMENTATION: P4-T1 | TESTS: P4-T3, P4-T4 | EVIDENCE: qa-gates/qa-vstest-phase4-sibling-test.2026-09-02T00-00.md
AC-MAPPING: AC6 | IMPLEMENTATION: P1-T1, P2-T1, P3-T1, P4-T1 (scope-limited by construction) | TESTS: P5-T10 | EVIDENCE: qa-gates/qa-scope-check.2026-09-02T00-00.md, qa-gates/qa-post-commit-verification.2026-09-02T00-00.md
AC-MAPPING: AC7 | IMPLEMENTATION: P5-T2 through P5-T8 | TESTS: P5-T3, P5-T4, P5-T5, P5-T6, P5-T7 | EVIDENCE: qa-gates/qa-csharpier-check.2026-09-02T00-00.md, qa-gates/qa-analyzer-rebuild.2026-09-02T00-00.md, qa-gates/qa-nullable-rebuild.2026-09-02T00-00.md, qa-gates/qa-vstest-scoped.2026-09-02T00-00.md, qa-gates/qa-coverage-delta.2026-09-02T00-00.md

UNRESOLVED-GAPS: NONE

## Handoff Note

This planner's tool surface for this session is file-only (`Read`, `Grep`, `Glob`, `Edit`, `Write`) with no Bash and no `mcp__drm-copilot__*` tools, so this planner cannot itself invoke `mcp__drm-copilot__validate_orchestration_artifacts` and cannot itself produce a genuine `atomic-executor` preflight clearance decision. Outstanding-clearance note (not a formatted signal line): this plan has been through two real atomic-executor preflight passes. Round 1 returned three defects (wrong test-count totals, an unsatisfiable scope-verification gate, and self-embedded literal signal lines), all addressed in the round-2 revision. Round 2 independently re-verified all three round-1 defects as correctly fixed and found exactly one new defect (Defect D: a stray sibling `plan.*.md` bootstrap-template stub in the feature folder, already removed from the working tree by the orchestrator), addressed in this round-3 revision via a defensive removal step added to P5-T13, as recorded in this pass's `SELF-REVIEW: RE-DERIVED THIS PASS` enumeration and `PLANNER-INTERNAL-REVIEW: PASS` record above. This revision round is itself part of the real preflight interaction, conducted through the actual `DIRECTIVE: PREFLIGHT VALIDATION ONLY` delegation channel; the calling agent must route this revised plan through a fresh atomic-executor preflight delegation to obtain the next genuine clearance decision before treating this plan as approved for execution, and must separately run `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` and `artifact_path: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/plan.2026-09-02T00-00.md`. This file (`plan.2026-09-02T00-00.md`) is the single target path for every revision in this cycle; no sibling timestamped plan file will be created.

plan-path: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/plan.2026-09-02T00-00.md
