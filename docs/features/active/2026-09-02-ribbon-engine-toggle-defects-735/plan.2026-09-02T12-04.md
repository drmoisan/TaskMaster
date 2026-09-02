# ribbon-engine-toggle-defects (Plan)

- **Issue:** #735
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** full-bug (acceptance criteria come from `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md` only; no user story exists for this item and none is to be authored)
- **Last Updated:** 2026-09-02T12-04
- **Status:** Ready for preflight
- **Version:** 1.0
- **Plan path continuity:** this file is updated in place for every preflight revision round. No timestamped sibling plan file is created for this cycle.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** the artifact path is named in the task text. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** all artifacts live under `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/`, `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied; no `artifacts/` evidence path appears in this plan.

## Requirement sources

- Acceptance criteria: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, section `## Acceptance Criteria`. Twenty-five checkbox items, given the stable IDs F1-AC1 through F1-AC7, F2-AC1 through F2-AC8, F3-AC1 through F3-AC6, and X-AC1 through X-AC4 in the AC identity table below.
- Design record: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/research/2026-09-02T09-15-ribbon-engine-toggle-defects-research.md`.
- Issue metadata: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/issue.md` carries `- Work Mode: full-bug` and no acceptance-criteria section. It is not an acceptance-criteria source for this cycle.

## Write set (the only files this plan may create or modify outside the feature folder)

- `TaskMaster/Ribbon/RibbonExplorer.xml`
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs`
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`
- `TaskMaster/Ribbon/SpamManagerResetGate.cs`
- `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`
- `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`
- `TaskMaster/TaskMaster.csproj`
- `TaskMaster.Test/TaskMaster.Test.csproj`

Files this plan must not touch, stated so the executor can fail closed rather than infer: TaskMaster/AppGlobals/AppOlObjects.cs and TaskMaster/AppGlobals/NonBlockingDelay.cs are owned by a different concurrent work item in the same parallel run. TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs is deliberately not modified; the two new XML-consistency tests go in the XML fixture instead, at the cost of one duplicated type-name constant. TaskMaster/Ribbon/RibbonViewer.cs is read-only for this change: no callback method is added, renamed, or removed on that type.

## Toolchain resolution (used by every command task)

Neither vstest.console.exe nor vswhere.exe is on PATH in this environment. Every task that builds or runs tests resolves them first:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
$msbuild = & $vswhere -latest -products * -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
```

Scoped test runs use the pinned form below, substituting the task's own filter, TRX name, and results directory. `/InIsolation` is mandatory for the Moq-based assemblies, and vstest 18.x rejects `OR` inside a filter, so clauses are joined with a vertical bar. The whole `/Logger` switch is double-quoted because an unquoted semicolon terminates the argument in pwsh and silently degrades the switch to a bare `/Logger:trx`, which names the TRX after the account and machine.

```powershell
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~<clauses>" `
  "/Logger:trx;LogFileName=<taskid>.trx" `
  /ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\<kind>\<taskid>
```

Intermediate compile gates in Phases 1 through 3 use `/t:Build`, which is correct there because those tasks change source files, so `CoreCompile` is not up to date and does run. The authoritative analyzer and nullable gates in Phase 4 use `/t:Rebuild`, because MSBuild's up-to-date check does not invalidate on a command-line property change and a warm `/t:Build` would exit 0 with the analyzers never loaded.

## Design decisions carried from the spec and research (do not redesign)

1. Finding 1 edits the CustomUI document, not the viewer type. Four `onAction` values lose the "ed" suffix; the `BtnMigrateIDs` button element is deleted whole. CSharpier formats this document, so the edit is followed by a format pass and any reflow is accepted.
2. Finding 2 introduces one host-neutral `internal sealed class SpamManagerResetGate` in namespace `TaskMaster`, following the constructor-guard shape of EngineReadinessGate.cs and the deferred-invocation shape of EngineGatedCommandRunner.cs. It carries no coverage-exemption attribute and an XML-doc paragraph recording that the omission is deliberate. `ClearSpamManagerAsync` keeps its synchronization-context preamble and confirmation dialog and moves only its engine-touching statements into a deferred lambda.
3. Finding 3 adds a monotonic sequence ticket plus a compare-and-apply cache write on the toggle state coordinator, retypes the pressed-state cache to a private nested reference type so the concurrent dictionary's conditional update compares by reference identity, and restructures prime completion so a canceled prime is treated as a failure.
4. Coverage disposition: the roughly ten residual lines inside `ClearSpamManagerAsync` remain inside the ribbon controller's pre-existing type-level exemption. This plan asserts no coverage credit for them anywhere and adds no new exemption attribute. They are validated by the manual-verification dossier of P2-T13, which is a documentation and acceptance task, not a toolchain gate.
5. Contingency: if `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` exceeds 500 lines after the final format pass, the versioned cache is extracted into its own class rather than trimming documentation. P4-T3 carries that branch explicitly; it is not a silent assumption.

## Literals this plan creates, quoted here so acceptance searches for them are exonerated

New test method names: `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod`, `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters`, `Constructor_WithNullAutoFileAccessor_ThrowsArgumentNullException`, `Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException`, `Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException`, `RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors`, `RunAsync_WhenAutoFileAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset`, `RunAsync_WhenManagerIsNull_NotifiesOnceAndDoesNotInvokeReset`, `RunAsync_WhenEnginesAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset`, `RunAsync_WhenAllDependenciesAvailable_InvokesResetWithResolvedManagerAndEngines`, `RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify`, `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult`, `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult`, `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce`, `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine`, `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker`, `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked`.

New production identifiers: `SpamManagerResetGate`, `RunAsync`, `BuildNotReadyMessage`, `_autoFileAccessor`, `_enginesAccessor`, `_notifyNotReady`, `_spamManagerResetGate`, `SpamManagerReset`, `PressedState`, `_stateSequence`, `NextSequence`, `TryApplyState`, `Active`, `Sequence`.

New test-fixture identifiers: `SpamManagerResetGateTests`, `RibbonControlTypeName`.

XML values after the edit: `MoveEntireConversation_Click`, `SaveAttachments_Click`, `SaveEmailCopy_Click`, `SavePictures_Click`. XML values and elements removed by the edit: `MoveEntireConversation_Clicked`, `SaveAttachments_Clicked`, `SaveEmailCopy_Clicked`, `SavePictures_Clicked`, `BtnMigrateIDs_Click`, and the element whose id attribute is `BtnMigrateIDs`.

## AC identity table

Each ID names one checkbox in the spec's `## Acceptance Criteria` section, in document order.

| ID | Spec heading | Opening words of the criterion |
|---|---|---|
| F1-AC1 | Finding 1 | The Explorer CustomUI document declared five callback names |
| F1-AC2 | Finding 1 | Exactly four action-callback attribute values are renamed |
| F1-AC3 | Finding 1 | Exactly one element is deleted |
| F1-AC4 | Finding 1 | The rename-versus-removal partition is exactly four renames plus one removal |
| F1-AC5 | Finding 1 | A test named RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod exists and passes |
| F1-AC6 | Finding 1 | A test named RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters exists and passes |
| F1-AC7 | Finding 1 | Both new tests are demonstrated to fail against the pre-fix tree |
| F2-AC1 | Finding 2 | A new internal sealed class SpamManagerResetGate exists |
| F2-AC2 | Finding 2 | The gate's RunAsync throws ArgumentNullException for a null reset delegate |
| F2-AC3 | Finding 2 | The gate carries no ExcludeFromCodeCoverage attribute |
| F2-AC4 | Finding 2 | ClearSpamManagerAsync retains its synchronization-context preamble |
| F2-AC5 | Finding 2 | All nine tests in the new gate fixture pass |
| F2-AC6 | Finding 2 | Line coverage for the new gate class is at least 90% |
| F2-AC7 | Finding 2 | No new ExcludeFromCodeCoverage attribute is introduced anywhere in the diff |
| F2-AC8 | Finding 2 | The change description records the manual verification |
| F3-AC1 | Finding 3 | The pressed-state cache is a concurrent dictionary of a private nested reference type |
| F3-AC2 | Finding 3 | Both writers capture a ticket immediately before invoking the activation read |
| F3-AC3 | Finding 3 | The pressed-state reader keeps its bool return type |
| F3-AC4 | Finding 3 | Prime completion treats any outcome other than ran-to-completion as a failure |
| F3-AC5 | Finding 3 | All six new tests in the new coordinator race file pass |
| F3-AC6 | Finding 3 | The existing coordinator test class declaration changes by exactly one added partial keyword |
| X-AC1 | Cross-cutting | All three new source files are registered as compile items |
| X-AC2 | Cross-cutting | Every file created or modified by this change is under the 500-line ceiling |
| X-AC3 | Cross-cutting | The full toolchain passes in order in a single pass |
| X-AC4 | Cross-cutting | No behavior outside the three findings changes |

### Phase 0 — Policy Reads and Baseline Capture

- [ ] [P0-T1] Read the four policy documents in the mandatory order — CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/csharp.md — and record the read in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/phase0-instructions-read.2026-09-02T12-04.md`.
  - Acceptance: the artifact exists and carries `Timestamp:`, a `Policy Order:` line naming the four documents in that order, and one line per document recording its heading count. No policy document is modified.
- [ ] [P0-T2] Read the acceptance-criteria source `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md` and the design record `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/research/2026-09-02T09-15-ribbon-engine-toggle-defects-research.md` in full, and record the write set and the two prohibited AppGlobals paths in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/scope-and-write-set.2026-09-02T12-04.md`.
  - Acceptance: the artifact lists exactly the ten write-set paths and names TaskMaster/AppGlobals/AppOlObjects.cs and TaskMaster/AppGlobals/NonBlockingDelay.cs as prohibited, and records that the spec's acceptance section holds 25 unchecked checkbox items.
- [ ] [P0-T3] Bootstrap the .NET SDK with scripts/vscode/Install-RepoDotNetSdk.ps1 and then run `dotnet tool restore`, recording both invocations in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/toolchain-bootstrap.2026-09-02T12-04.md`.
  - Acceptance: both commands report `EXIT_CODE: 0`, and the artifact records the CSharpier version resolved by `dotnet tool run csharpier --version` as `1.2.6`. Rationale: global.json pins the SDK under a directory that is absent in a fresh worktree, so every `dotnet` command fails until the bootstrap script has run.
- [ ] [P0-T4] Restore NuGet packages with scripts/vscode/Invoke-Restore.ps1 and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/nuget-restore.2026-09-02T12-04.md`.
  - Acceptance: `EXIT_CODE: 0`, and the artifact records that the repository-root `packages` directory exists after the run. Rationale: without it the analyzer and nullable baselines fail with CS0006 rather than measuring anything.
- [ ] [P0-T5] Capture the read-only formatter baseline with `dotnet tool run csharpier check .` and record the verbatim unformatted-file set in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/csharpier-check.2026-09-02T12-04.md`.
  - Acceptance: the artifact records the exit code and, when non-zero, every path CSharpier reported as unformatted, verbatim and one per line. This set is the comparison basis for P4-T4; it is not a pass/fail gate here. The artifact also records whether any of the eight formatter-visible write-set paths appears in that set.
- [ ] [P0-T6] Capture the analyzer baseline by running the CLAUDE.md analyzer rebuild on TaskMaster.sln and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/msbuild-analyzer.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: the artifact records `EXIT_CODE:` and the trailing warning and error counts printed by MSBuild. A non-zero baseline is recorded as the baseline, not treated as a plan failure; P4-T5 is compared against it.
- [ ] [P0-T7] Capture the nullable baseline by running the CLAUDE.md nullable rebuild on TaskMaster.sln and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/msbuild-nullable.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: the artifact records `EXIT_CODE:` and the trailing warning and error counts. Do not add `/p:Nullable=enable`; this repository opts into nullable per file and the solution-wide property produces hundreds of errors that CI does not see.
- [ ] [P0-T8] Capture the scoped ribbon-fixture test baseline by running every test whose fully qualified name contains `TaskMaster.Test.Ribbon` and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/ribbon-tests.2026-09-02T12-04.md`, with the TRX written to `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/p0-t8`.
  - Command: the pinned scoped form with `"/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon"`, `"/Logger:trx;LogFileName=p0-t8.trx"`, and `/ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\baseline\p0-t8`.
  - Acceptance: that results directory holds exactly one TRX file and no others; the artifact records the total, passed, failed and skipped counts read from that TRX. This is the population P1-T8 and P3-T12 are compared against.
- [ ] [P0-T9] Capture the coverage baseline for the whole first-party suite with scripts/vscode/Invoke-MSTestWithCoverage.ps1 and record the numeric headline in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/coverage-baseline.2026-09-02T12-04.md`, with the Cobertura document at `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml`.
  - Command: run scripts/vscode/Invoke-MSTestWithCoverage.ps1 through pwsh with the search root set to the current directory, the configuration set to Debug, and the coverage output set to `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml`.
  - Acceptance, part 1 — discovery scope: the artifact lists every discovered test assembly path, and every one of them is under the workspace root recorded in P0-T11. The workspace root itself sits beneath a `.claude` segment, so a "contains no `.claude`" filter is meaningless here; the check is that no discovered path contains a further `worktrees` segment relative to that root.
  - Acceptance, part 2 — numeric headline: the artifact records the root `coverage` element's `line-rate` and `branch-rate` attributes as numbers.
  - Acceptance, part 3 — per-file figures: the artifact records the aggregated line coverage for `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, summed over every `class` element whose `filename` attribute ends with that path, including the compiler-generated async state-machine and display classes, because an async body lands in its own `class` element and reading the named element alone measures only constructors and field initializers. It records the same aggregation for `TaskMaster/Ribbon/RibbonController.Intelligence.cs`, and if no `class` element matches that file it records `ABSENT — pre-existing type-level ExcludeFromCodeCoverage on the containing type` rather than a number. It records `NOT APPLICABLE — file does not exist at baseline` for `TaskMaster/Ribbon/SpamManagerResetGate.cs`; no baseline figure is invented for it.
- [ ] [P0-T10] Record the pre-change line counts of the six existing in-scope source files in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/file-line-counts.2026-09-02T12-04.md`.
  - Files measured: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `TaskMaster/Ribbon/RibbonController.Intelligence.cs`, `TaskMaster/Ribbon/RibbonExplorer.xml`, `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`, `TaskMaster.Test/TaskMaster.Test.csproj`.
  - Acceptance: the artifact records one count per file, measured with `Get-Content -LiteralPath` piped to a count, and records the headroom to 500 for each of the five source files. These counts are advisory; the authoritative audit is P4-T2, taken after the final format pass.
- [ ] [P0-T11] Record the workspace root, the current branch, the merge base against origin/main, and the pre-change `git status --porcelain` output in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/baseline/base-ref.2026-09-02T12-04.md`.
  - Command: `$base = git merge-base origin/main HEAD; git rev-parse --show-toplevel; git rev-parse --abbrev-ref HEAD; git status --porcelain`
  - Acceptance: the artifact records the resolved merge-base commit id, and records whether the pre-change tree is clean. Every later diff gate re-derives the same base with `git merge-base origin/main HEAD` rather than pinning the recorded id, so a rebase cannot make a later gate compare against a stale commit.

### Phase 1 — Finding 1: Explorer CustomUI callback bindings

Both tests added here compile against the pre-fix tree, because they reference only the embedded resource and existing public metadata on the viewer type. No declaration-only seam task is needed for this phase.

- [ ] [P1-T1] Add the two new test methods and one private constant to `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, inside a new region for this issue placed after the existing issue #503 region.
  - Content: a private `const string RibbonControlTypeName = "Microsoft.Office.Core.IRibbonControl";` hoisting the literal that already appears inline in the existing getEnabled signature test; a method `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod` that enumerates descendant element nodes only, treats an attribute as a callback when its local name is `onAction`, `onChange` or `onLoad` or begins with `get`, includes the root element's load callback, and asserts every distinct value matches the name of some public instance method on the viewer type, failing with the full unresolved list in one message; and a method `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters` that resolves every check-box action callback and asserts the method returns void and takes the ribbon-control interface followed by a bool, comparing the first parameter by full type name.
  - Acceptance: both `[TestMethod]` names are present in the file, the constant is present, and the file still compiles as part of P1-T2's run. No production file is modified by this task.
- [ ] [P1-T2] [expect-fail] Run only the two new tests against the pre-fix tree and record the failing run in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/fail-before-finding1.2026-09-02T12-04.md`, with the TRX in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/p1-t2`.
  - Command: build first with `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then the pinned scoped run with `"/TestCaseFilter:FullyQualifiedName~RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod|FullyQualifiedName~RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters"`, `"/Logger:trx;LogFileName=p1-t2.trx"`, `/ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p1-t2`.
  - Acceptance: the build exits 0; that results directory holds exactly one TRX and no others; the TRX records total 2, passed 0, failed 2. The artifact carries `ExpectedExitCode: 1`, quotes the first failure message showing all five unresolved names, and quotes the second failure message showing the four unresolvable check-box callbacks.
- [ ] [P1-T3] Rename the four check-box action-callback values in `TaskMaster/Ribbon/RibbonExplorer.xml` from the `_Clicked` spelling to the `_Click` spelling on the move-entire-conversation, save-attachments, save-email-copy and save-pictures check boxes inside the Item Sort Settings menu.
  - Acceptance: the file contains zero occurrences of each of `MoveEntireConversation_Clicked`, `SaveAttachments_Clicked`, `SaveEmailCopy_Clicked`, `SavePictures_Clicked`, and exactly one occurrence of each of `MoveEntireConversation_Click`, `SaveAttachments_Click`, `SaveEmailCopy_Click`, `SavePictures_Click`. No `getPressed` value and no element id changes.
- [ ] [P1-T4] Delete the entire button element whose id attribute is `BtnMigrateIDs` from `TaskMaster/Ribbon/RibbonExplorer.xml`.
  - Acceptance: the file contains zero occurrences of `BtnMigrateIDs` and zero occurrences of `MigrateToDoIDs`, and the document still parses as well-formed XML. The removal, rather than an implementation, is the spec's decision: no method of that name exists anywhere in the solution and no design document proposes a MigrateToDoIDs behavior.
- [ ] [P1-T5] Run the formatter over `TaskMaster/Ribbon/RibbonExplorer.xml` and record the before-and-after SHA-256 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/csharpier-xml-format.2026-09-02T12-04.md`.
  - Command: capture `Get-FileHash -Algorithm SHA256` for the file, run `dotnet tool run csharpier format TaskMaster/Ribbon/RibbonExplorer.xml`, capture the hash again.
  - Acceptance: the artifact records both hashes and the exit code. Do not use the console line `Formatted N files` as a rewrite signal: CSharpier reports the number of files it processed, not the number it changed, so a one-file run always prints 1. Any reflow the formatter produces is accepted; the semantic check is P1-T9.
- [ ] [P1-T6] Rebuild so the edited CustomUI document is re-embedded into the TaskMaster assembly and copied to the test output directory, recording the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/build-after-finding1.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
  - Acceptance: `EXIT_CODE: 0`. The rebuild is load-bearing: the tests read the document through `GetManifestResourceStream`, so a stale assembly would answer P1-T7 from the pre-fix resource.
- [ ] [P1-T7] Re-run the same two tests and record the passing run in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/pass-after-finding1.2026-09-02T12-04.md`, with the TRX in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/p1-t7`.
  - Acceptance: that results directory holds exactly one TRX and no others; the TRX records total 2, passed 2, failed 0; `EXIT_CODE: 0`.
- [ ] [P1-T8] Run the whole ribbon test fixture set and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/ribbon-fixtures-after-finding1.2026-09-02T12-04.md`, with the TRX in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/p1-t8`.
  - Command: the pinned scoped form with `"/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon"`, `"/Logger:trx;LogFileName=p1-t8.trx"`, `/ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p1-t8`.
  - Acceptance: that results directory holds exactly one TRX and no others; failed is 0; total equals the P0-T8 baseline total plus 2. The pre-existing set-equality test asserting that getEnabled is declared only on engine-backed controls is among the passing tests, which proves the deletion did not disturb the engine-command control set.
- [ ] [P1-T9] Verify the CustomUI edit is exactly four attribute-value renames plus one element removal, reflow-independently, and record the comparison in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/xml-edit-scope.2026-09-02T12-04.md`.
  - Method: load the pre-change document with `$base = git merge-base origin/main HEAD` followed by `git show "${base}:TaskMaster/Ribbon/RibbonExplorer.xml"`, reusing the same two-statement merge-base convention P0-T11 establishes, and load the current file; for each, build the sorted multiset of element-name plus id pairs and the sorted multiset of callback attribute name plus value pairs; report both symmetric differences.
  - Acceptance: the element symmetric difference is exactly one entry, the button whose id is `BtnMigrateIDs`; the callback symmetric difference is exactly nine entries, the five removed values and the four added values. A line-count diff is deliberately not used, because the formatter may reflow attributes and would make a numstat expectation unsatisfiable.
- [ ] [P1-T10] Check off F1-AC1 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md` and cite `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/fail-before-finding1.2026-09-02T12-04.md` and the P1-T7 artifact as its evidence.
  - Acceptance: exactly one checkbox changes from `- [ ]` to `- [x]`; the criterion text is unmodified; the pre-fix count of five and the post-fix count of zero are both quoted from the two artifacts.
- [ ] [P1-T11] Check off F1-AC2 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the callback symmetric difference recorded by P1-T9 and the unchanged state of TaskMaster/Ribbon/RibbonViewer.cs.
  - Acceptance: exactly one checkbox flips; a numstat diff against the merge base, restricted to the viewer source file TaskMaster/Ribbon/RibbonViewer.cs, produces no output line, proving no method was added, renamed or removed on that type to satisfy the bindings.
- [ ] [P1-T12] Check off F1-AC3 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the single-entry element symmetric difference recorded by P1-T9.
  - Acceptance: exactly one checkbox flips.
- [ ] [P1-T13] Check off F1-AC4 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the four-plus-one partition recorded by P1-T9 against the five unresolved names recorded by P1-T2.
  - Acceptance: exactly one checkbox flips; the artifact citation shows the four renamed names each had a correctly signatured twin and the removed name had none.
- [ ] [P1-T14] Check off F1-AC5 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the P1-T7 TRX entry for `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod`.
  - Acceptance: exactly one checkbox flips; the named test is recorded as passed in that TRX.
- [ ] [P1-T15] Check off F1-AC6 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the P1-T7 TRX entry for `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters`.
  - Acceptance: exactly one checkbox flips; the named test is recorded as passed in that TRX.
- [ ] [P1-T16] Check off F1-AC7 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the two quoted failure messages in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/fail-before-finding1.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips; the artifact shows five unresolved names for the first test and four unresolvable check-box callbacks for the second.

### Phase 2 — Finding 2: the Spam Manager reset gate

No `[expect-fail]` run is possible for this finding. The defect lives inside a method that shows a message box, installs a WinForms synchronization context, and reaches disk-backed classifier creation, and it sits inside the ribbon controller's pre-existing type-level coverage exemption. P2-T12 records that impossibility as a schema-valid fail-before exception dossier instead of fabricating a failing run.

- [ ] [P2-T1] Create `TaskMaster/Ribbon/SpamManagerResetGate.cs` containing the `internal sealed class SpamManagerResetGate` in namespace `TaskMaster`.
  - Shape: three readonly fields `_autoFileAccessor`, `_enginesAccessor` and `_notifyNotReady`; a constructor taking an auto-file-objects accessor, an engines accessor and a not-ready notification delegate, each validated with the `?? throw new ArgumentNullException(nameof(x))` form; one method `internal Task RunAsync(Func<ManagerAsyncLazy, IAppItemEngines, Task> reset)` that throws for a null reset delegate before touching any accessor, then resolves the auto-file objects, takes the manager through a null-conditional, resolves the engines, emits the not-ready message exactly once and returns a completed task when either the manager or the engines facade is null, and otherwise returns the reset invocation directly with no await and no catch; and one `private static string BuildNotReadyMessage()` producing a current-culture-formatted notice that names no control id.
  - Usings: System, System.Globalization, System.Threading.Tasks, UtilitiesCS, and nothing else. The classifier manager type and both dependency interfaces all live in the UtilitiesCS namespace, so no additional using and no new project reference is required.
  - Acceptance: the file exists, declares exactly one type, and contains an XML-doc paragraph stating that the absence of a coverage-exemption attribute is deliberate, mirroring the equivalent paragraph on EngineReadinessGate.cs.
- [ ] [P2-T2] Register the new production file as a compile item in `TaskMaster/TaskMaster.csproj`, inserted in the existing ribbon item group in alphabetical position, immediately after the RibbonViewer.EngineCommands.cs compile entry and immediately before the TryFunctionalityInConstruction.cs compile entry.
  - Acceptance: the project file contains exactly one line reading `<Compile Include="Ribbon\SpamManagerResetGate.cs" />`. This edit is unavoidable: the project is a legacy non-SDK project that lists every source file explicitly, so an unlisted file is not compiled. The project file is excluded from the formatter by .csharpierignore, so no format pass is required for it.
- [ ] [P2-T3] Build the solution and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/build-after-gate-class.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
  - Acceptance: `EXIT_CODE: 0`, and the artifact records that the build log contains a compile line for the new file, which proves the item-group registration took effect rather than being silently ignored.
- [ ] [P2-T4] Verify the host-neutrality constraints on `TaskMaster/Ribbon/SpamManagerResetGate.cs` and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/gate-class-constraints.2026-09-02T12-04.md`.
  - Acceptance: the file contains zero lines matching `^\s*\[ExcludeFromCodeCoverage\]`, zero lines matching `^using Microsoft\.Office`, zero lines matching `^using System\.Windows\.Forms`, and zero occurrences of `log4net`. The attribute check is anchored to the attribute form on its own line, so the XML-doc sentence that names the attribute in prose while explaining its deliberate absence does not defeat the check; the artifact quotes that sentence to show the distinction.
- [ ] [P2-T5] Create `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` as a `[TestClass] public class SpamManagerResetGateTests` in namespace `TaskMaster.Test.Ribbon`, holding the nine named tests, using MSTest, Moq and FluentAssertions.
  - Cases: three constructor null-argument tests each asserting the offending parameter name; a null-reset test whose accessors are strict delegates that fail the test if invoked; three not-ready tests covering a null auto-file-objects result, a mocked auto-file-objects whose manager is unset, and a null engines result, each asserting exactly one notification and that the reset delegate was never invoked; a success test that arranges a real classifier manager over a mocked globals object and asserts both lambda arguments are the same instances that were resolved and that no notification was emitted; and a faulting-reset test asserting the fault propagates unchanged with no notification.
  - Acceptance: all nine `[TestMethod]` names listed in the literals block are present in the file. No test sleeps, polls, reads the wall clock, touches the filesystem, or creates a temporary file; the manager's constructor performs only an async-lazy assignment and never executes its factory, so constructing it reaches no disk and no COM.
- [ ] [P2-T6] Register the new test file as a compile item in `TaskMaster.Test/TaskMaster.Test.csproj`, in the existing ribbon item group.
  - Acceptance: the project file contains exactly one line reading `<Compile Include="Ribbon\SpamManagerResetGateTests.cs" />`.
- [ ] [P2-T7] Build the solution and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/build-after-gate-tests.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
  - Acceptance: `EXIT_CODE: 0`, and the artifact records a compile line for the new test file.
- [ ] [P2-T8] Run the nine gate tests and record the run in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/gate-tests.2026-09-02T12-04.md`, with the TRX in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/p2-t8`.
  - Command: the pinned scoped form with `"/TestCaseFilter:FullyQualifiedName~SpamManagerResetGateTests"`, `"/Logger:trx;LogFileName=p2-t8.trx"`, `/ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p2-t8`.
  - Acceptance: that results directory holds exactly one TRX and no others; the TRX records total 9, passed 9, failed 0; `EXIT_CODE: 0`.
- [ ] [P2-T9] Rewrite the Clear Spam Manager call site in `TaskMaster/Ribbon/RibbonController.Intelligence.cs`: add a private backing field and a lazily built gate property inside the existing Spam Manager region immediately above the method, and move the four engine-touching statements into an async lambda passed to the gate.
  - Shape: the property builds the gate from an auto-file accessor and an engines accessor that read the globals object through a null-conditional with the null-forgiving operator, plus the existing private not-ready notifier declared on the sibling engine-commands partial, carrying the same explanatory comment used by the two existing lazily built ribbon collaborators: a null accessor result is a supported input the gate treats as "not ready", not a defect.
  - Body change: invert the confirmation result to an early return, then await the gate's deferred invocation, using the resolved manager and engines in place of the globals chain inside the lambda. The synchronization-context preamble and the confirmation dialog stay exactly where they are and in that order.
  - Acceptance: the method body contains zero occurrences of `Globals.AF` and zero occurrences of `Globals.Engines`, and no inline null-conditional or `is null` guard is added inside the method. That approach was explicitly disrecommended by the maintainer on the predecessor issue and would place the guard permanently inside the coverage-exempt region.
- [ ] [P2-T10] Build the solution and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/build-after-callsite.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
  - Acceptance: `EXIT_CODE: 0`.
- [ ] [P2-T11] Verify the call-site edit is confined to the Spam Manager region of `TaskMaster/Ribbon/RibbonController.Intelligence.cs` and record the comparison in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/callsite-edit-scope.2026-09-02T12-04.md`.
  - Command: `git diff --unified=0 (git merge-base origin/main HEAD) -- TaskMaster/Ribbon/RibbonController.Intelligence.cs`
  - Acceptance: every hunk in the output lies between the Spam Manager region markers; the preamble statement installing the synchronization context and the confirmation dialog call both appear unchanged and in their original order in the post-change file; the eight QuickFiler-settings members and the three not-implemented members are untouched by any hunk.
- [ ] [P2-T12] Write the fail-before exception dossier for finding 2 at `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/fail-before-exception.2026-09-02T12-04.md`.
  - Required contents: `Timestamp:`; a `WhyFailingRunImpossible:` paragraph stating that the defective statements show a message box, install a WinForms synchronization context, and call classifier creation and serialization paths that touch disk, so no deterministic unit test can execute them; an alternative-proof section pointing at the nine gate tests, which cover the whole decision the fix extracts; and the negative-claim fields `SearchScope:`, `SearchPatterns:` and `SearchResult:` recording that no failing run for this finding exists anywhere under the feature's evidence tree.
  - Acceptance: the dossier exists at that exact path and carries all of those fields.
- [ ] [P2-T13] Write the manual-verification dossier at `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md`, recording the required operator procedure and its outcome field.
  - Required contents: the two-step procedure from the spec — launch Outlook with add-in user-interface errors shown, click Clear Spam Manager before add-in initialization completes and confirm the prompt, observe the not-ready notice instead of a null-reference exception; then repeat after initialization completes and confirm the reset still runs end to end — plus a single field `ManualVerificationStatus:` whose value is either `PERFORMED` with both observed outcomes recorded, or `OPERATOR-ACTION-REQUIRED` with the reason.
  - Acceptance: the dossier exists with that field present. This task is a documentation and acceptance task, not a toolchain gate: it must not be reported as a passing automated check, and no coverage credit is claimed for the residual lines it covers.
- [ ] [P2-T14] Check off F2-AC1 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/gate-tests.2026-09-02T12-04.md` for the three constructor cases.
  - Acceptance: exactly one checkbox flips; the three constructor tests are recorded as passed in the P2-T8 TRX.
- [ ] [P2-T15] Check off F2-AC2 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the null-reset, three not-ready, success and faulting-reset entries of the P2-T8 TRX.
  - Acceptance: exactly one checkbox flips; all six of those tests are recorded as passed.
- [ ] [P2-T16] Check off F2-AC3 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/gate-class-constraints.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips; the artifact shows all four zero counts and quotes the XML-doc sentence recording the deliberate omission.
- [ ] [P2-T17] Check off F2-AC4 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/callsite-edit-scope.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips; the artifact shows the preamble and dialog unchanged and no inline guard added.
- [ ] [P2-T18] Check off F2-AC5 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the total-9 passed-9 line of the P2-T8 TRX.
  - Acceptance: exactly one checkbox flips.

### Phase 3 — Finding 3: toggle-state race, canceled prime, and the untested guard

The six new tests reference only members that already exist on the coordinator and the existing private harness, so they compile against the pre-fix tree once the partial keyword is added. The partial keyword must land first, because two files declaring the same class without it is a compile error that would redden the whole test assembly and destroy the assertion-time fail-before evidence.

- [ ] [P3-T1] Add the `partial` keyword to the test class declaration in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`. This is a one-word edit and the only change to that file in this entire plan.
  - Acceptance: the declaration line reads `public partial class EngineToggleStateCoordinatorTests`; the file's line count is unchanged from the P0-T10 baseline. The two-file partial pattern is already established in this same directory by the ribbon controller fixture and its engines partial.
- [ ] [P3-T2] Create `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` as a second partial of the same class, holding the six named tests, reusing the existing private nested harness and logged-error record with no duplication.
  - Cases: the prime-after-toggle reproduction, in which the activation read returns a held completion source on the first call and true on the second, the prime is started by a cache-miss read, the toggle is awaited to completion, then the prime is released with the stale value and awaited through the prime handle, asserting the cached read is true, exactly one invalidation was issued, and no error was logged; the toggle-versus-toggle case, in which two toggles dequeue two held completion sources and the later-started one completes first, asserting the cached read is true and exactly one invalidation; the uncontended case guarding against over-suppression by the new conditional invalidation; the CR-3 case calling the toggle path directly with the harness engines flag off, asserting the invalid-operation exception message names the engine key and that the engine toggle was never invoked; the canceled-prime logging case; and its companion asserting the cache stays unset after a canceled prime.
  - Ordering constraint inside the canceled-prime logging test: assert the single logged error and its message first, then perform the second cache-miss read and await the second prime handle, then assert the second prime handle is not the same instance as the first. The harness engines mock is strict and each test supplies its own setups; a re-primed read re-enters the same setup, so an error-count assertion taken after the re-prime would be unsatisfiable by construction. The prime-handle identity assertion is the deterministic signal that the in-flight marker was cleared.
  - Acceptance: all six `[TestMethod]` names listed in the literals block are present in the file, and no test sleeps, polls, reads the wall clock, touches the filesystem, or starts a message pump.
- [ ] [P3-T3] Register the new race test file as a compile item in `TaskMaster.Test/TaskMaster.Test.csproj`, in the existing ribbon item group.
  - Acceptance: the project file contains exactly one line reading `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.Race.cs" />`.
- [ ] [P3-T4] Build the solution and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/build-before-race-fix.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
  - Acceptance: `EXIT_CODE: 0` and a compile line for the new race file. A green build here is what makes the next task's failures genuine assertion failures rather than compile errors.
- [ ] [P3-T5] [expect-fail] Run the six new race tests against the pre-fix coordinator and record the run in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/fail-before-finding3.2026-09-02T12-04.md`, with the TRX in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/p3-t5`.
  - Command: the pinned scoped form with `"/TestCaseFilter:FullyQualifiedName~ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult|FullyQualifiedName~ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult|FullyQualifiedName~ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce|FullyQualifiedName~ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine|FullyQualifiedName~GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker|FullyQualifiedName~GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked"`, `"/Logger:trx;LogFileName=p3-t5.trx"`, `/ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p3-t5`.
  - Acceptance: that results directory holds exactly one TRX and no others; the TRX records total 6, passed 3, failed 3, and the three failures are exactly the prime-after-toggle reproduction, the toggle-versus-toggle case, and the canceled-prime logging case. The artifact carries `ExpectedExitCode: 1` and states why the other three pass before the fix: the uncontended case and the CR-3 guard case already hold on the pre-fix code, and the pre-fix canceled prime already leaves the cache unset.
- [ ] [P3-T6] Introduce the versioned pressed-state cache in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`: add a private long sequence field read and written only through interlocked operations, a private nested sealed reference type carrying an activation flag and a sequence ticket, a `NextSequence()` interlocked increment, a `TryApplyState(...)` explicit compare-and-swap loop that stores an observation only when no newer observation is already cached for the key and returns whether the write was applied, retype the cache to the nested type with ordinal comparison, and update the synchronous reader to unwrap the cached observation.
  - Acceptance: the file contains zero occurrences of `ConcurrentDictionary<string, bool>`, contains the `PressedState`, `_stateSequence`, `NextSequence` and `TryApplyState` identifiers, and the synchronous reader still returns `bool` and still contains no `await`, no blocking call and no `throw`. A reference type is required rather than a value tuple, because the conditional update then compares by reference identity, which is the compare-and-swap semantic needed; a value tuple would degrade the comparison to structural equality.
  - Sequencing note: the tree is expected to be compile-red between this task and P3-T9, because the two writers still assign a bool into the retyped cache. No build or test gate runs in that window by design; the next build gate is P3-T10.
- [ ] [P3-T7] Update the toggle writer in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` so it takes a ticket after the engine toggle completes and before the activation read, applies the observation through the compare-and-apply helper, and invalidates the control only when the write was applied.
  - Acceptance: within that method the ticket capture appears after the toggle await and before the activation-read await, and the invalidation call is inside the conditional. Update-before-invalidate ordering is preserved, so the existing ordering test continues to pass unmodified.
- [ ] [P3-T8] Update the prime writer in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` so it takes a ticket immediately before the activation read, applies the observation through the compare-and-apply helper, and invalidates the control only when the write was applied.
  - Acceptance: within that method the ticket capture appears before the activation-read await and the invalidation call is inside the conditional. Conditional invalidation is correct: a rejected write means a newer writer already stored its value and already invalidated.
- [ ] [P3-T9] Restructure prime completion in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` so it returns early only when the task ran to completion, and otherwise clears the in-flight marker and logs the failure, synthesizing a cancellation exception when there is no exception to unwrap.
  - Acceptance: the method tests the completed task's status against ran-to-completion; the marker removal and the log call are on the non-completed path; the faulted path still unwraps the base exception, which an existing test asserts by reference, and the existing prime-failed message builder is reused unchanged.
- [ ] [P3-T10] Build the solution and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/build-after-race-fix.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
  - Acceptance: `EXIT_CODE: 0`. This is the first build gate after P3-T6, which is the point at which the compile-red window opened by the retyped cache closes.
- [ ] [P3-T11] Re-run the six race tests and record the passing run in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/pass-after-finding3.2026-09-02T12-04.md`, with the TRX in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/p3-t11`.
  - Acceptance: that results directory holds exactly one TRX and no others; the TRX records total 6, passed 6, failed 0; `EXIT_CODE: 0`.
- [ ] [P3-T12] Run the whole coordinator fixture across both partial files and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/coordinator-fixture-after-fix.2026-09-02T12-04.md`, with the TRX in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/p3-t12`.
  - Command: the pinned scoped form with `"/TestCaseFilter:FullyQualifiedName~EngineToggleStateCoordinatorTests"`, `"/Logger:trx;LogFileName=p3-t12.trx"`, `/ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p3-t12`.
  - Acceptance: that results directory holds exactly one TRX and no others; failed is 0; the pre-existing update-before-invalidate ordering test and the pre-existing faulted-prime test are both recorded as passed, unmodified.
- [ ] [P3-T13] Verify the one-word edit to `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/partial-keyword-edit.2026-09-02T12-04.md`.
  - Command: `git diff --numstat (git merge-base origin/main HEAD) -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`
  - Acceptance: the single output line reports one insertion and one deletion for that path, and the unified diff shows the added and removed lines differ only by the added keyword.
- [ ] [P3-T14] Check off F3-AC1 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the identifier checks recorded for P3-T6.
  - Acceptance: exactly one checkbox flips.
- [ ] [P3-T15] Check off F3-AC2 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the P3-T7 and P3-T8 ordering checks and the passing prime-after-toggle and toggle-versus-toggle tests in the P3-T11 TRX.
  - Acceptance: exactly one checkbox flips.
- [ ] [P3-T16] Check off F3-AC3 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the reader signature check from P3-T6 and the unmodified ordering test recorded in the P3-T12 TRX.
  - Acceptance: exactly one checkbox flips.
- [ ] [P3-T17] Check off F3-AC4 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the two canceled-prime tests in the P3-T11 TRX and the still-passing faulted-prime test in the P3-T12 TRX.
  - Acceptance: exactly one checkbox flips.
- [ ] [P3-T18] Check off F3-AC5 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the total-6 passed-6 line of the P3-T11 TRX together with the three named failures in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/regression-testing/fail-before-finding3.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips.
- [ ] [P3-T19] Check off F3-AC6 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/partial-keyword-edit.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips.

### Phase 4 — Final QA Toolchain Loop

The loop is format, then lint, then type-check, then test with coverage. If any step fails or rewrites a file, restart the loop at P4-T1. P4-T11 records that a single pass completed clean.

- [ ] [P4-T1] Run the formatter over this change's own source paths only, recording a SHA-256 for each file immediately before and immediately after the run in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/csharpier-format.2026-09-02T12-04.md`.
  - Scope: `TaskMaster/Ribbon/RibbonExplorer.xml`, `TaskMaster/Ribbon/RibbonController.Intelligence.cs`, `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `TaskMaster/Ribbon/SpamManagerResetGate.cs`, `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`, `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`. The two project files are excluded from the formatter by .csharpierignore and are not passed to it.
  - Acceptance: the artifact records sixteen hashes, eight before and eight after, and defines the rewritten-file count as the number of paths whose two hashes differ. The console line `Formatted N files` must not be used as the rewritten count: CSharpier reports files processed, not files changed, so an eight-path run always prints 8 and a restart rule keyed on it would never terminate. If the rewritten count is greater than zero, continue to P4-T2; the restart obligation is triggered by a later failing step, not by this rewrite.
  - Acceptance, sibling invalidation: for every path whose two hashes differ, the earlier scope gate that measured that path must be re-run and its artifact replaced. Concretely, a changed hash on `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` requires re-running the P3-T13 numstat check and re-confirming F3-AC6 before Phase 5; a changed hash on `TaskMaster/Ribbon/RibbonController.Intelligence.cs` requires re-running the P2-T11 region check and re-confirming F2-AC4; a changed hash on `TaskMaster/Ribbon/RibbonExplorer.xml` requires no re-run, because the P1-T9 check compares element and attribute sets rather than lines and is therefore reflow-independent. The artifact records which of these re-runs was required and its outcome, or records that no hash changed.
- [ ] [P4-T2] Audit the post-format line counts of every file this change created or modified against the 500-line ceiling and record them in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/file-line-counts.2026-09-02T12-04.md`.
  - Files measured: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `TaskMaster/Ribbon/RibbonController.Intelligence.cs`, `TaskMaster/Ribbon/SpamManagerResetGate.cs`, `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`, `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`.
  - Acceptance: the artifact records one count per file measured after P4-T1, and records for each whether it is at or below 500. This is the authoritative audit; the P0-T10 counts were advisory because the formatter reflows to its print width and can push a hand-written file past the ceiling. The CustomUI document is not measured against this ceiling: it is a resource document that already exceeded 500 lines before this change and this change only removes a line from it.
- [ ] [P4-T3] Resolve the coordinator size contingency for `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` and record the outcome in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/coordinator-size-contingency.2026-09-02T12-04.md`.
  - Branch A, at or below 500: record `CONTINGENCY: NOT APPLICABLE` together with the measured count taken from P4-T2. No file is created.
  - Branch B, above 500: extract the versioned cache — the nested state type, the sequence field, the next-sequence helper, the compare-and-apply helper and the dictionary — into a new internal sealed class in a new file under the ribbon directory, register it as a compile item in `TaskMaster/TaskMaster.csproj`, add a matching test file registered in `TaskMaster.Test/TaskMaster.Test.csproj`, then re-run P4-T1 and P4-T2 and record the re-measured count. Documentation must not be trimmed to fit. This branch extends the write set and must be reported to the orchestrator as a scope amendment in the same artifact.
  - Acceptance: exactly one branch is recorded, the measured count is stated numerically in either branch, and the final count for that file is at or below 500. Research projects roughly 455 to 465 lines after formatting, so branch A is expected but is not assumed.
- [ ] [P4-T4] Verify formatting repository-wide, read-only, with `dotnet tool run csharpier check .` and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/csharpier-check-final.2026-09-02T12-04.md`.
  - Acceptance: either the exit code is 0, or the reported unformatted set is exactly the set captured in P0-T5 and contains none of the eight formatter-visible paths listed in P4-T1. The artifact records the reported set verbatim in either case.
- [ ] [P4-T5] Run the analyzer gate and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/msbuild-analyzer.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: `EXIT_CODE: 0`, and the trailing warning and error counts are no worse than the P0-T6 baseline counts. `/t:Rebuild` is required: a warm `/t:Build` exits 0 with `CoreCompile` skipped on every project and the analyzers never loaded.
- [ ] [P4-T6] Run the nullable type-check gate and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/msbuild-nullable.2026-09-02T12-04.md`.
  - Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: `EXIT_CODE: 0`, and the trailing warning and error counts are no worse than the P0-T7 baseline counts. Do not add `/p:Nullable=enable`.
- [ ] [P4-T7] Run the coverage-enabled test gate over the whole first-party suite with scripts/vscode/Invoke-MSTestWithCoverage.ps1 and record the numeric result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md`, with the Cobertura document at `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml`.
  - Command: run scripts/vscode/Invoke-MSTestWithCoverage.ps1 through pwsh with the search root set to the current directory, the configuration set to Debug, and the coverage output set to `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml`.
  - Acceptance: `EXIT_CODE: 0` with zero failed tests; the artifact records the root `line-rate` and `branch-rate` as numbers and the discovered-assembly list, checked against the workspace root exactly as in P0-T9. The script always excludes the live-Outlook category, so this run starts no external process and is compared against the P0-T9 population on equal terms.
- [ ] [P4-T8] Compute the coverage delta and the new-code figures from the two Cobertura documents and record them in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/coverage-delta.2026-09-02T12-04.md`.
  - Method: for each file, sum the covered and total line entries over every `class` element whose `filename` attribute ends with that file's path, including compiler-generated async state-machine and display classes, then derive the rate from the sums.
  - Required rows: baseline and final line coverage for `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, with the final value at or above the baseline value; final line coverage for `TaskMaster/Ribbon/SpamManagerResetGate.cs` at or above 90%, meeting the new-module rule; coverage of the lines this change added to the coordinator, derived by intersecting the added line numbers from `git diff --unified=0 (git merge-base origin/main HEAD) -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` with the covered line set, reported as a covered-over-total pair with every uncovered added line enumerated by number; and a row for `TaskMaster/Ribbon/RibbonController.Intelligence.cs` recording `ABSENT — pre-existing type-level ExcludeFromCodeCoverage on the containing type` if no `class` element matches it in either document, with no coverage credit claimed for it in that case.
  - Acceptance: every required row is present with a numeric value or the explicit absence marker, and no row carries a placeholder such as unverified.
- [ ] [P4-T9] Verify that no new coverage-exemption attribute was introduced anywhere in the change and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/no-new-exemption.2026-09-02T12-04.md`.
  - Command: `git add -N .; git diff (git merge-base origin/main HEAD) -- TaskMaster TaskMaster.Test`
  - Acceptance: the diff contains zero added lines matching `^\+\s*\[ExcludeFromCodeCoverage\]` and zero removed lines matching the same pattern, so no exemption is added and none is widened. The check is anchored to the attribute form so the XML-doc prose on the new gate class, which names the attribute while recording its deliberate absence, does not defeat it; the artifact quotes that prose line to show why an unanchored search would have been wrong. The `git add -N` span is required because two of the three new files would otherwise be invisible to a diff against the merge base.
- [ ] [P4-T10] Verify the change footprint and record it in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/footprint-scope.2026-09-02T12-04.md`.
  - Command: `git add -N .; git diff --name-status (git merge-base origin/main HEAD); git status --porcelain`
  - Acceptance: every changed path is either one of the ten write-set paths, one of the paths created by the P4-T3 branch B if that branch was taken, or a path under `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/`. TaskMaster/AppGlobals/AppOlObjects.cs and TaskMaster/AppGlobals/NonBlockingDelay.cs are absent from the changed set, as is TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs and TaskMaster/Ribbon/RibbonViewer.cs. The porcelain status span is required alongside the name-listing diff because the diff enumerates tracked changes only.
- [ ] [P4-T11] Record in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/toolchain-loop-closure.2026-09-02T12-04.md` that P4-T1 through P4-T10 completed in a single pass with no failing step and no file rewritten after the format step.
  - Acceptance: the artifact lists each of the ten steps with its exit code and states the pass number. If any step failed or rewrote a tracked file after P4-T1, the loop restarts at P4-T1 and the artifact records both the failed pass and the clean pass.

### Phase 5 — Coverage, Scope and Acceptance Reconciliation

The tasks here close the acceptance criteria that could only be judged after the Phase 4 coverage and toolchain gates, then reconcile the whole acceptance set and hand off the reduced audit.

- [ ] [P5-T1] Check off F2-AC6 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the new-module row of `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/coverage-delta.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips, and only if that row reports a numeric value at or above 90%. If the value is below 90%, leave the box unchecked and record the shortfall with the uncovered lines enumerated.
- [ ] [P5-T2] Check off F2-AC7 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/no-new-exemption.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips; the artifact shows both zero counts.
- [ ] [P5-T3] Resolve F2-AC8 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md` against the `ManualVerificationStatus:` field of `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md`.
  - Acceptance: if that field reads `PERFORMED` with both observed outcomes recorded, flip exactly one checkbox. If it reads `OPERATOR-ACTION-REQUIRED`, leave the checkbox unchecked and record `OPERATOR-ACTION-REQUIRED` for this criterion in the P5-T8 summary. An executor without a live Outlook host must take the second branch; recording it unchecked is the correct outcome, not a plan failure.
- [ ] [P5-T4] Check off X-AC1 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing the three compile-item registrations in `TaskMaster/TaskMaster.csproj` and `TaskMaster.Test/TaskMaster.Test.csproj` together with the passing analyzer and nullable rebuilds.
  - Acceptance: exactly one checkbox flips; the two project files hold the three expected compile-item lines and P4-T5 and P4-T6 both recorded exit code 0.
- [ ] [P5-T5] Check off X-AC2 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/file-line-counts.2026-09-02T12-04.md` and the contingency outcome from P4-T3.
  - Acceptance: exactly one checkbox flips; every measured count is at or below 500.
- [ ] [P5-T6] Check off X-AC3 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/toolchain-loop-closure.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips; the artifact records one clean pass with all steps at exit code 0.
- [ ] [P5-T7] Check off X-AC4 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`, citing `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/footprint-scope.2026-09-02T12-04.md` and `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/callsite-edit-scope.2026-09-02T12-04.md`.
  - Acceptance: exactly one checkbox flips; the artifacts show the eight QuickFiler-settings members, the orphaned folder-classifier handler and the three not-implemented bound handlers untouched, and the spec's rollout section already records all three as separate follow-ups.
- [ ] [P5-T8] Write the acceptance-criteria status summary to `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/issue-updates/ac-status.2026-09-02T12-04.md`.
  - Required contents: the source file path, the total of 25, the checked count, the remaining count, and the text of every remaining unchecked criterion.
  - Acceptance: the summary's checked count equals the number of `- [x]` items actually present in the spec's acceptance section, verified by counting them in the file rather than by summing the plan's own claims.
- [ ] [P5-T9] Rewrite the local account token and the machine-name token out of the contents of every committed evidence document under `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/`, and record the per-file substitution counts in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/evidence-sanitization.2026-09-02T12-04.md`.
  - Tokens, named by role and derived at run time so this plan file does not itself carry either value: the local account token is the value of `Split-Path -Leaf $env:USERPROFILE`; the machine-name token is the value of `$env:COMPUTERNAME`. Neither value may be written into this plan, into the artifact this task produces, or into the artifact P5-T10 produces; both artifacts record only the derivation expression, the per-file counts, and the replacement tokens.
  - Scope: every file under that evidence tree whose extension is `.trx`, `.cobertura.xml` or `.md`, enumerated recursively. Both TRX documents and Cobertura documents are in scope because each carries the tokens inside its content rather than only in its name: a TRX carries the account token in the `runUser=` attribute of its `TestRun` element and the machine-name token in the `computerName=` attribute of every `UnitTestResult` element, and a Cobertura document carries both inside the absolute source paths it records. The already-committed repository artifact docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r2-full-diagnostic.trx is the verified precedent for this: it carries one `runUser=` occurrence of the account token and 6476 `computerName=` occurrences of the machine-name token in its content, none of which a name-only check would ever see.
  - Method: for each in-scope file, read the content, replace every case-insensitive occurrence of the account token with the literal `REDACTED-ACCOUNT` and every case-insensitive occurrence of the machine-name token with the literal `REDACTED-MACHINE`, count the substitutions of each kind before writing, and write the file back only when at least one substitution was made.
  - Acceptance: the artifact exists at that path and records, for every in-scope file, the file path relative to the feature folder together with two integers, the account-token substitution count and the machine-name-token substitution count. The count rows, not the exit code, are the required observation: this task rewrites tracked files and exits 0 whether it substituted anything or nothing, so an exit code alone records no outcome. The artifact also carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, and its `Output Summary:` states the total number of files rewritten and the two total substitution counts.
- [ ] [P5-T10] Audit evidence completeness across `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/` and record the result in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/evidence-completeness.2026-09-02T12-04.md`.
  - Acceptance, part 1 — artifact completeness: every artifact path named by a task from P0-T1 through P5-T9 exists. The bound stops at P5-T9 because the reduced-audit handoff artifact is written by P5-T11, after this gate runs, so demanding it here would be unsatisfiable. Every command-bearing artifact within that bound carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`; the two artifacts recording an intentionally failing run carry `ExpectedExitCode: 1`.
  - Acceptance, part 2 — sanitization completeness: the count of files and directories anywhere under the evidence tree whose name contains either the local account token or the machine-name token, compared case-insensitively, is zero; and the count of case-insensitive occurrences of either token in the contents of every file under the evidence tree whose extension is `.trx`, `.cobertura.xml` or `.md` is also zero. Both tokens are re-derived here by the same run-time expressions P5-T9 uses, and neither value is written into this artifact. The content check is the load-bearing half: a name-only check passes on a TRX whose `runUser=` and `computerName=` attributes still carry both tokens, which is exactly the state the verified precedent artifact is in.
  - Acceptance, part 3 — verdict: if any check in part 1 or part 2 fails the verdict is BLOCKED, never PASS.
- [ ] [P5-T11] Record the reduced-audit handoff and the follow-up promotion list in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/other/reduced-audit-handoff.2026-09-02T12-04.md`.
  - Required contents: pointers to the coverage delta, footprint scope, toolchain closure and acceptance status artifacts; the manual-verification status; and the three follow-ups the spec defers, namely the eight QuickFiler-settings unguarded-globals sites, the orphaned folder-classifier handler, and the three not-implemented bound handlers.
  - Acceptance: the artifact exists and names all three follow-ups. Promotion of those follow-ups into their own issues is out of this plan's scope and is the orchestrator's action.

## Planner Adversarial Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

### Revision round 1 — citations re-derived in this pass

This round applied a preflight delta touching the Phase 5 evidence-sanitization gate, the Phase 2 project-file registration task, the Phase 1 merge-base convention, and every backticked non-write-set configuration path. Each citation those edits touch was re-read directly against the working tree in this same pass, together with its sibling region.

- `TaskMaster/TaskMaster.csproj` — re-derived for P2-T2: the ribbon compile-item group spans 458-470 and is alphabetically ordered. `Ribbon\EngineToggleStateCoordinator.cs` is at 463 and `Ribbon\RibbonController.cs` at 464, so those two entries are adjacent and nothing sorts between them; `Ribbon\RibbonViewer.EngineCommands.cs` is at 469 and `Ribbon\TryFunctionalityInConstruction.cs` at 470, and `Ribbon\SpamManagerResetGate.cs` sorts between exactly those two. The previous placement clause was therefore self-contradictory and is replaced.
- `TaskMaster.Test/TaskMaster.Test.csproj` — re-derived as the Phase 2 sibling: the ribbon compile-item group spans 314-324 and is not alphabetically ordered, with `Ribbon\RibbonExplorerXmlTests.cs` at 324 following `Ribbon\TryFunctionalityInConstructionTests.cs` at 323. P2-T6 and P3-T3 name only the group and assert no ordering, so both remain satisfiable and are unchanged.
- docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r2-full-diagnostic.trx — re-derived for P5-T9: this committed artifact carries one `runUser=` occurrence holding the account token and 6476 `computerName=` occurrences holding the machine-name token, all in file content and none in the file name. This is the measured precedent that makes the name-only gate insufficient.
- `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/plan.2026-09-02T12-04.md` — re-derived for P1-T9: P0-T11 at plan line 141 assigns the merge base with `$base = git merge-base origin/main HEAD`, so P1-T9 now reuses that same two-statement convention instead of a parenthesized subexpression concatenated to a path suffix.
- `TaskMaster/Ribbon/RibbonExplorer.xml` — re-derived as the P1-T9 sibling region: the button element carrying the unimplemented callback is at line 82 with id `BtnMigrateIDs`, callback `BtnMigrateIDs_Click` and label `MigrateToDoIDs`; the four check-box action callbacks with the "ed" suffix are at 268, 274, 280 and 286. P1-T3, P1-T4 and the P1-T9 acceptance clause remain exactly satisfiable against that state.
- .csharpierignore — re-derived for the backtick sweep and for P5-T9: line 4 excludes the whole evidence tree and lines 5-8 exclude Cobertura, coverage and TRX artifacts, so the files P5-T9 rewrites are outside the formatter and cannot trigger a P4-T1 restart; lines 12-14 exclude project, props and targets files only.
- coverage.config — re-derived for the backtick sweep: the module excludes at 14-20 name third-party and test-framework assemblies only, and no first-party production path is excluded.
- .gitignore — re-derived for the backtick sweep and for P5-T9: lines 140-141 ignore `*.coverage` and `*.coveragexml`, and line 144 ignores the repository-root coverage directory. Neither `*.trx` nor `*.cobertura.xml` is ignored, so the TRX and Cobertura documents this plan writes into the evidence tree are committed, which is what makes the P5-T9 content rewrite load-bearing rather than cosmetic.
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs` — re-derived as the Phase 2 sibling region: file length 412; the Spam Manager region opens at 188; the engines accessor is already null-conditional at 204; the target method spans 206-233, with the synchronization-context preamble at 208-211, the confirmation dialog at 212-216, the first unguarded globals dereference in the condition at 219-224, and the five statements of the guarded block at 226-230, of which 229 and 230 are the two further unguarded globals dereferences; the three not-implemented members begin at 235 and lie outside the edit region.

### Authoring pass — citations re-derived when the plan was first written

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` — re-derived: file length 389; cache declaration and ordinal comparer at 68-69; synchronous reader body 142-145; toggle writer awaits at 223-224 with the unconditional store at 226 and invalidation at 227; prime writer at 303-312 with the unconditional store at 310; prime completion at 319-329 reading only the task's exception; the `System.Threading` using at line 4, so interlocked operations need no new using; the header remark at 36-43 recording that the type is deliberately not coverage-exempt.
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs` — re-derived: the UtilitiesCS using at line 12. The line spans for this file are restated by the revision-round entry above, which supersedes the authoring-pass span for the engine-touching statements.
- `TaskMaster/Ribbon/RibbonExplorer.xml` — re-derived: the button element with the unimplemented callback at line 82; the four check-box action callbacks with the "ed" suffix at 268, 274, 280 and 286, each inside the Item Sort Settings menu at 258-288, each already declaring a correctly resolving pressed-state callback.
- `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` — re-derived: file length 323; class declaration at 21; the embedded-resource name constant at 23; the document loader at 48-60; the precedent signature test declared at 294 with the ribbon-control type name literal inline at 316; the region terminator at 321.
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` — re-derived: file length 459; class declaration at 23; the faulted-prime test at 213-243 asserting the unwrapped exception by reference at 233; the update-before-invalidate ordering test at 250; the private nested harness at 403-441 with its strict engines mock at 419-420 and its engines-available flag at 428; the logged-error record at 446-457.
- TaskMaster/Ribbon/EngineReadinessGate.cs (precedent, not modified) — re-derived: the deliberate-omission XML-doc paragraph at 24-28 and the constructor guard form at 45-49.
- TaskMaster/Ribbon/EngineGatedCommandRunner.cs (precedent, not modified) — re-derived: the suppresses-invocation-never-errors paragraph at 21-25; the two constructor guards at 60-65; the null-argument-before-evaluation ordering at 97-111.
- TaskMaster/Ribbon/RibbonController.EngineCommands.cs (precedent, not modified) — re-derived: the two lazily built collaborators at 39-46 and 67-77, both using the null-conditional accessor with the null-forgiving operator and the same explanatory comment; the private not-ready notifier at 158-162.
- `TaskMaster/TaskMaster.csproj` — re-derived: the ribbon compile-item group at 458-470, alphabetically ordered, with the toggle coordinator entry at 463 and the ribbon controller entry at 464.
- `TaskMaster.Test/TaskMaster.Test.csproj` — re-derived: the ribbon compile-item group at 314-324; the project reference to the utilities project at 343-346; the platform default at 11 and the debug output path at 35.
- UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs (read-only) — re-derived: namespace at 26, type at 28, the single-argument constructor at 37-42 whose only work is a field assignment and an async-lazy assignment at 94 that does not execute its factory.
- UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs (read-only) — re-derived: namespace at 11, interface at 13, the manager member at 37.
- UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs (read-only) — re-derived: namespace at 5, interface at 7, the restart member at 14.
- TaskMaster/Properties/AssemblyInfo.cs (read-only) — re-derived: the internals-visible declaration for the test assembly at 38, which is what lets the test project reach an internal gate class.
- .csharpierignore — re-derived: lines 4-8 exclude the evidence tree and coverage and test-result artifacts; lines 12-14 exclude project, props and targets files only, so the CustomUI document is formatted and both project files are not.
- coverage.config — re-derived: module excludes cover third-party and test-framework assemblies only; no first-party production path is excluded.
- .gitignore — re-derived: line 144 ignores the repository-root coverage directory, which is why this plan writes both Cobertura documents into the feature's evidence tree instead.
- .claude/rules/csharp.md (policy, read-only) — re-derived: lines 14-19 pin the four toolchain commands, the rebuild requirement for both msbuild gates, and the prohibition on the solution-wide nullable property; lines 39-41 pin the repository floor, the new-module target, and the changed-line no-regression rule.
- .claude/rules/general-code-change.md (policy, read-only) — re-derived: the 500-line ceiling and its exception list, which does not exempt test code.
- .claude/rules/plan-acceptance-gates.md (policy, read-only) — re-derived: the G1 through G9 rule table at 31-44 and the attribution window at 52-54, applied to every acceptance condition in this plan.
- scripts/vscode/Invoke-MSTestWithCoverage.ps1 (tooling, read-only) — re-derived: the parameter block at 1-11 confirming the search-root, configuration and coverage-output parameters and the default output path.
- `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md` — re-derived: the work-mode marker at line 6, the write set at 131-142, and the twenty-five acceptance checkboxes at 146-181.
- `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/issue.md` — re-derived: the work-mode marker at line 12 and the absence of any acceptance-criteria section.

Sibling-region findings that changed this plan relative to a naive reading of the research record:

1. The existing coordinator harness wires a strict engines mock. A canceled-prime test that asserts an error count after triggering a re-prime would re-enter the same setup and observe a second logged error, so P3-T2 fixes the assertion order and uses prime-handle identity as the marker-cleared signal.
2. The logged-error record in the existing fixture is named for logging, not loading; the research record's spelling of it is a typo and this plan refers to it by role rather than by that spelling.
3. The workspace root for this cycle sits beneath a `.claude` segment, so the usual "no `.claude` path in the discovered assembly list" coverage check would reject every legitimate assembly. P0-T9 and P4-T7 assert a workspace-root prefix instead.
4. The ribbon controller's containing type carries a pre-existing type-level coverage exemption, so its partial is expected to have no class element in either Cobertura document. P0-T9 and P4-T8 record an explicit absence marker rather than demanding a number that will never be printed.

Sibling-region findings added in revision round 1:

5. The Phase 5 evidence gate checked file and directory names only. This plan commits TRX and Cobertura documents, and both carry the account token and the machine-name token inside their content, so a name-only gate cannot fail on the exact leak it exists to catch. P5-T9 now rewrites both tokens out of file content and P5-T10 asserts zero content occurrences case-insensitively as well as zero name occurrences.
6. The completeness gate's artifact-existence check is bounded at P5-T9. The reduced-audit handoff artifact is written by P5-T11, which runs after the gate, so an unbounded "every artifact named in this plan exists" clause would be unsatisfiable at the moment the gate runs.
7. The ribbon compile-item group in `TaskMaster/TaskMaster.csproj` is alphabetically ordered, and the two entries the previous P2-T2 placement clause named are adjacent to each other, so no insertion point exists between them. The replacement clause names the two entries that actually bracket the alphabetical position.
8. The Spam Manager method holds a third unguarded globals dereference at line 219-224, inside the condition that opens the guarded block, ahead of the two at 229 and 230. P2-T9's acceptance clause already binds it, because that clause demands zero occurrences of both globals tokens anywhere in the method body rather than in the guarded block alone, so the extraction must carry the condition's read into the deferred lambda as well.

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS

CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS

CITATION: TaskMaster/Ribbon/EngineToggleStateCoordinator.cs | length 389; lines 4, 36-43, 68-69, 142-145, 223-227, 303-312, 319-329
CITATION: TaskMaster/Ribbon/RibbonController.Intelligence.cs | length 412; lines 12, 188, 204, 206-233, 208-211, 212-216, 219-224, 226-230, 235
CITATION: TaskMaster/Ribbon/RibbonExplorer.xml | lines 82, 258-288, 268, 274, 280, 286
CITATION: TaskMaster/Ribbon/EngineReadinessGate.cs | lines 24-28, 45-49
CITATION: TaskMaster/Ribbon/EngineGatedCommandRunner.cs | lines 21-25, 60-65, 97-111
CITATION: TaskMaster/Ribbon/RibbonController.EngineCommands.cs | lines 39-46, 67-77, 158-162
CITATION: TaskMaster/Properties/AssemblyInfo.cs | line 38 internals-visible-to the test assembly
CITATION: TaskMaster/TaskMaster.csproj | ribbon compile-item group lines 458-470; RibbonViewer.EngineCommands.cs entry at 469; TryFunctionalityInConstruction.cs entry at 470
CITATION: TaskMaster.Test/TaskMaster.Test.csproj | lines 11, 35, 314-324, 343-346
CITATION: .csharpierignore | lines 4-8 evidence and artifact excludes; lines 12-14 project-file excludes
CITATION: coverage.config | module-exclude list lines 14-20
CITATION: .gitignore | lines 140-141, 144
CITATION: docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r2-full-diagnostic.trx | 1 runUser= occurrence and 6476 computerName= occurrences in file content
CITATION: TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs | length 323; lines 21, 23, 48-60, 294, 316, 321
CITATION: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs | length 459; lines 23, 213-243, 233, 250, 403-441, 419-420, 428, 446-457
CITATION: UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs | lines 26, 28, 37-42, 94
CITATION: UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs | lines 11, 13, 37
CITATION: UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs | lines 5, 7, 14
CITATION: .claude/rules/csharp.md | lines 14-19, 39-41
CITATION: .claude/rules/general-code-change.md | 500-line file size limit section
CITATION: .claude/rules/plan-acceptance-gates.md | lines 31-44, 52-54
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | parameter block lines 1-11
CITATION: docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md | line 6; lines 131-142; lines 146-181
CITATION: docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/issue.md | line 12
CITATION: docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/research/2026-09-02T09-15-ribbon-engine-toggle-defects-research.md | sections 2.1-2.4, 3.3-3.5, 4.1-4.5, 5.1-5.5, 7.1-7.3, 9

AC-INVENTORY: F1-AC1, F1-AC2, F1-AC3, F1-AC4, F1-AC5, F1-AC6, F1-AC7, F2-AC1, F2-AC2, F2-AC3, F2-AC4, F2-AC5, F2-AC6, F2-AC7, F2-AC8, F3-AC1, F3-AC2, F3-AC3, F3-AC4, F3-AC5, F3-AC6, X-AC1, X-AC2, X-AC3, X-AC4

AC-MAPPING: F1-AC1 | IMPLEMENTATION: P1-T3, P1-T4 | TESTS: P1-T2, P1-T7 | EVIDENCE: evidence/regression-testing/fail-before-finding1.2026-09-02T12-04.md, evidence/regression-testing/pass-after-finding1.2026-09-02T12-04.md
AC-MAPPING: F1-AC2 | IMPLEMENTATION: P1-T3 | TESTS: P1-T7, P1-T9 | EVIDENCE: evidence/qa-gates/xml-edit-scope.2026-09-02T12-04.md
AC-MAPPING: F1-AC3 | IMPLEMENTATION: P1-T4 | TESTS: P1-T9 | EVIDENCE: evidence/qa-gates/xml-edit-scope.2026-09-02T12-04.md
AC-MAPPING: F1-AC4 | IMPLEMENTATION: P1-T3, P1-T4 | TESTS: P1-T9 | EVIDENCE: evidence/qa-gates/xml-edit-scope.2026-09-02T12-04.md
AC-MAPPING: F1-AC5 | IMPLEMENTATION: P1-T1 | TESTS: P1-T7 | EVIDENCE: evidence/regression-testing/pass-after-finding1.2026-09-02T12-04.md
AC-MAPPING: F1-AC6 | IMPLEMENTATION: P1-T1 | TESTS: P1-T7 | EVIDENCE: evidence/regression-testing/pass-after-finding1.2026-09-02T12-04.md
AC-MAPPING: F1-AC7 | IMPLEMENTATION: P1-T1 | TESTS: P1-T2 | EVIDENCE: evidence/regression-testing/fail-before-finding1.2026-09-02T12-04.md
AC-MAPPING: F2-AC1 | IMPLEMENTATION: P2-T1, P2-T2 | TESTS: P2-T8 | EVIDENCE: evidence/regression-testing/gate-tests.2026-09-02T12-04.md
AC-MAPPING: F2-AC2 | IMPLEMENTATION: P2-T1 | TESTS: P2-T8 | EVIDENCE: evidence/regression-testing/gate-tests.2026-09-02T12-04.md
AC-MAPPING: F2-AC3 | IMPLEMENTATION: P2-T1 | TESTS: P2-T4 | EVIDENCE: evidence/qa-gates/gate-class-constraints.2026-09-02T12-04.md
AC-MAPPING: F2-AC4 | IMPLEMENTATION: P2-T9 | TESTS: P2-T11 | EVIDENCE: evidence/qa-gates/callsite-edit-scope.2026-09-02T12-04.md
AC-MAPPING: F2-AC5 | IMPLEMENTATION: P2-T5, P2-T6 | TESTS: P2-T8 | EVIDENCE: evidence/regression-testing/gate-tests.2026-09-02T12-04.md
AC-MAPPING: F2-AC6 | IMPLEMENTATION: P2-T1 | TESTS: P4-T7, P4-T8 | EVIDENCE: evidence/qa-gates/coverage-delta.2026-09-02T12-04.md
AC-MAPPING: F2-AC7 | IMPLEMENTATION: N/A no-change requirement | TESTS: P4-T9 | EVIDENCE: evidence/qa-gates/no-new-exemption.2026-09-02T12-04.md
AC-MAPPING: F2-AC8 | IMPLEMENTATION: P2-T13 | TESTS: P5-T3 manual referral, not an automated gate | EVIDENCE: evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md, evidence/regression-testing/fail-before-exception.2026-09-02T12-04.md
AC-MAPPING: F3-AC1 | IMPLEMENTATION: P3-T6 | TESTS: P3-T11 | EVIDENCE: evidence/regression-testing/pass-after-finding3.2026-09-02T12-04.md
AC-MAPPING: F3-AC2 | IMPLEMENTATION: P3-T7, P3-T8 | TESTS: P3-T11 | EVIDENCE: evidence/regression-testing/pass-after-finding3.2026-09-02T12-04.md
AC-MAPPING: F3-AC3 | IMPLEMENTATION: P3-T6 | TESTS: P3-T12 | EVIDENCE: evidence/regression-testing/coordinator-fixture-after-fix.2026-09-02T12-04.md
AC-MAPPING: F3-AC4 | IMPLEMENTATION: P3-T9 | TESTS: P3-T11, P3-T12 | EVIDENCE: evidence/regression-testing/pass-after-finding3.2026-09-02T12-04.md
AC-MAPPING: F3-AC5 | IMPLEMENTATION: P3-T2 | TESTS: P3-T5, P3-T11 | EVIDENCE: evidence/regression-testing/fail-before-finding3.2026-09-02T12-04.md, evidence/regression-testing/pass-after-finding3.2026-09-02T12-04.md
AC-MAPPING: F3-AC6 | IMPLEMENTATION: P3-T1 | TESTS: P3-T13 | EVIDENCE: evidence/qa-gates/partial-keyword-edit.2026-09-02T12-04.md
AC-MAPPING: X-AC1 | IMPLEMENTATION: P2-T2, P2-T6, P3-T3 | TESTS: P4-T5, P4-T6 | EVIDENCE: evidence/qa-gates/msbuild-analyzer.2026-09-02T12-04.md, evidence/qa-gates/msbuild-nullable.2026-09-02T12-04.md
AC-MAPPING: X-AC2 | IMPLEMENTATION: P4-T3 | TESTS: P4-T2 | EVIDENCE: evidence/qa-gates/file-line-counts.2026-09-02T12-04.md, evidence/qa-gates/coordinator-size-contingency.2026-09-02T12-04.md
AC-MAPPING: X-AC3 | IMPLEMENTATION: N/A toolchain requirement | TESTS: P4-T1, P4-T4, P4-T5, P4-T6, P4-T7 | EVIDENCE: evidence/qa-gates/toolchain-loop-closure.2026-09-02T12-04.md
AC-MAPPING: X-AC4 | IMPLEMENTATION: N/A scope-boundary requirement | TESTS: P4-T10, P2-T11 | EVIDENCE: evidence/qa-gates/footprint-scope.2026-09-02T12-04.md

UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
