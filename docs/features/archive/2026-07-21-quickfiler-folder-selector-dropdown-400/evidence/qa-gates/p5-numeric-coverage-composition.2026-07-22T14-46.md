# P5 numeric-coverage composition — NON-AUTHORITATIVE (P5-T172 NOT SATISFIED)

Timestamp: `2026-07-22T14-46`

Command: `$suffix=[DateTimeOffset]::UtcNow.ToString('yyyy-MM-ddTHH-mm'); $evidence=(Resolve-Path 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates').Path; $coverageOutput=Join-Path $evidence "coverage-p5-numeric-correction.$suffix.cobertura.xml"; $coverageConfig=(Resolve-Path 'coverage.config').Path; $cliRunSettings=(Resolve-Path 'scripts\vscode\TaskMaster.cli.runsettings').Path; $quickFilerTestAssembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $p5Filter='FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests'; $preHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; $coverageVersion=(& dotnet-coverage --version | Out-String).Trim(); $coverageArgs = @('collect','--output',$coverageOutput,'--output-format','cobertura','--settings',$coverageConfig,'--',$vstestPath,$quickFilerTestAssembly,"/Settings:$cliRunSettings",'/InIsolation',"/TestCaseFilter:$p5Filter"); & dotnet-coverage @coverageArgs; $code=$LASTEXITCODE; $postHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; "SUFFIX=$suffix"; "DOTNET_COVERAGE_VERSION=$coverageVersion"; "COVERAGE_OUTPUT=$coverageOutput"; "PRE_HASH=$preHash"; "POST_HASH=$postHash"; if(Test-Path $coverageOutput){$xml=[xml](Get-Content -Raw $coverageOutput); "XML_ROOT=$($xml.DocumentElement.Name)"; "XML_COMPLETE=$([bool]$xml.DocumentElement)"; "XML_BYTES=$((Get-Item $coverageOutput).Length)"; "XML_SHA256=$((Get-FileHash -Algorithm SHA256 $coverageOutput).Hash)"}; exit $code`

EXIT_CODE: `1`

Output Summary: `FAIL — P5-T172 is NOT satisfied and remains unchecked. The exact P5-T171 17-class filter executed under the P5-T99 direct dotnet-coverage command shape reached natural completion but returned exit code 1 with 160 discovered, 159 passed, 1 failed, 0 skipped. The single failure is QuickFiler.Test.Viewers.BreadcrumbUiThreadDispatchTests.SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext, asserting "Expected context.PostCount to be greater than 0 because worker completion must cross the captured UI dispatcher, but found 0" at QuickFiler.Test\Viewers\BreadcrumbUiThreadDispatchTests.cs:55. The failure reproduced on two consecutive full-set instrumented invocations. The emitted Cobertura document is structurally complete and coverage.config is byte-identical before and after, but the run does not meet the P5-T172 160/160 requirement, so neither this artifact nor coverage-p5-numeric-correction.2026-07-22T14-44.cobertura.xml is authoritative and P5-T173 must not parse it as an authoritative source.`

## Verdict

`P5-T172: NOT SATISFIED.` The task requires natural exit zero with 160 discovered and 160 passed. This run produced 159 passed and 1 failed. Per fixed execution rule 44 and the task's own "no partial/stale artifact qualifies" clause, the checklist item remains unchecked, and P5-T173 and P5-T174 remain unexecuted and unchecked.

## Execution facts

- `dotnet-coverage` version: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`.
- VSTest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
- Test assembly: only `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- Selected classes: exactly `17` (the exact P5-T171 filter, unmodified).
- Discovered: `160`.
- Passed: `159`.
- Failed: `1`.
- Skipped: `0`.
- Total test time: `4.1` seconds (second invocation).
- Process result: natural completion. No stall, no timeout, no termination, no incomplete discovery, no partial XML.

## Failing case

- Fully qualified name: `QuickFiler.Test.Viewers.BreadcrumbUiThreadDispatchTests.SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext`.
- Assertion: `Expected context.PostCount to be greater than 0 because worker completion must cross the captured UI dispatcher, but found 0.`
- Source location: `QuickFiler.Test\Viewers\BreadcrumbUiThreadDispatchTests.cs` line `55`, immediately after `await Task.WhenAny(population, context.FirstPost)`.
- Mechanical implication: `RecordingSynchronizationContext.Post` increments `PostCount` inside the lock before `_firstPost.TrySetResult(true)`, so a completed `FirstPost` can never observe `PostCount == 0`. The observed value of `0` therefore means the `population` task returned by `BreadcrumbBridgeCoordinator.SetSuggestionsAsync` completed before any post was made to the captured context.

## Reproduction and control matrix

| Invocation | Instrumentation | Filter scope | Result |
|---|---|---|---|
| Full-set run 1 (`2026-07-22T14-42`) | `dotnet-coverage collect` | exact 17-class P5 filter | `160` discovered, `159` passed, `1` failed — same case |
| Full-set run 2 (`2026-07-22T14-44`) | `dotnet-coverage collect` | exact 17-class P5 filter | `160` discovered, `159` passed, `1` failed — same case |
| Isolation control x3 | `dotnet-coverage collect` | `FullyQualifiedName~BreadcrumbUiThreadDispatchTests` | `9` discovered, `9` passed, `0` failed on all three runs |
| Uninstrumented control | none (plain VSTest) | exact 17-class P5 filter | `160` discovered, `160` passed, `0` failed, exit `0` |

The uninstrumented control reproduces the already-recorded P5-T171 result (`p5-numeric-coverage-17-class-pass-after.2026-07-22T13-07.md`) against the current unchanged worktree, confirming that P5-T171 remains valid and that the tree has not regressed. The differentiating condition is the combination of `dotnet-coverage` instrumentation with the full 17-class parallel composition; instrumentation alone (isolation control) does not reproduce it.

## Configuration integrity

- Pre-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- Post-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- Result: unchanged. `coverage.config`, `scripts/vscode/TaskMaster.cli.runsettings`, thresholds, filters, exclusions, packages, and `QuickFiler/Viewers/ItemViewer.Designer.cs` were not modified by this task.

## Emitted Cobertura artifact (non-authoritative)

- File: `coverage-p5-numeric-correction.2026-07-22T14-44.cobertura.xml`.
- SHA-256: `3772E492EB66B46E5AAB0C7A23E1C7AA71B8B3A98ED650241C99CA29C0D77669`.
- Bytes: `17,329,488`.
- XML root: `coverage`; structural result: complete and parseable.
- Root headline: `8,623` covered lines of `85,785` valid lines.
- First-party class entries present for every measurable P5 type, including the new coordinator: `BreadcrumbUiDispatcher` (3), `BreadcrumbWebViewSurfaceFactory` (3), `BreadcrumbPopupUiOperations` (22), `BreadcrumbDropDownHost` (10), `BreadcrumbDropDownOpenLifetime` (20), `BreadcrumbDropDownOpenCoordinator` (7), `BreadcrumbMessengerHub` (4), `BreadcrumbCollapsedAttachment` (2), `BreadcrumbCollapsedSurfaceController` (3).
- Status: **non-authoritative**. It was produced by a run that failed one of the 160 required cases, so it must not be used as the P5-T173 numeric source.

## Discarded artifact disclosure

The first invocation (`2026-07-22T14-42`) was launched through `powershell.exe`, in which `Get-FileHash` was unavailable, so neither the `coverage.config` integrity hashes nor the artifact hash could be captured for that run. Its Cobertura output `coverage-p5-numeric-correction.2026-07-22T14-42.cobertura.xml` was therefore explicitly deleted rather than retained, because it carried no integrity verification and duplicated the reproduced result. This deletion is disclosed here rather than performed silently; the run's test outcome is preserved in the reproduction matrix above. The second invocation was re-run under `pwsh` 7.6.0 with full hash capture.

## Downstream state

- `P5-T172`: unchecked.
- `P5-T173`: not executed, unchecked. No authoritative P5-T172 artifact exists to parse, and the task forbids parsing anything else.
- `P5-T174`: not executed, unchecked. It is gated on passing P5-T102 through P5-T173 evidence.
- Phase 5 is therefore not complete.
- No production, test, project, runsettings, or configuration file was modified. Correcting the failing case requires an in-place plan revision authorizing an edit to `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` and/or `BreadcrumbBridgeCoordinator`, which is outside this executor's approved scope.
