# [P5-T8] AC-U3 non-vacuity mutation: carried-predictor adoption always refused

- Issue: #792
- Timestamp: 2026-09-17T20-45
- Command: CMD-OUTLOOK (`OUTLOOK-CLOSED: true` before each build), then for the mutated tree and again for the restored tree: CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST, CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts|FullyQualifiedName~EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry` and `<task>` = `p5-t8-mutation` / `p5-t8-restored`; restoration by `git checkout -- QuickFiler/Controllers/EfcDataModel.Carry.cs`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; consoles in the gitignored `coverage/p5-t8-mutation-build.log`, `coverage/p5-t8-mutation-scoped.log`, `coverage/p5-t8-restore-build.log`, `coverage/p5-t8-restored-scoped.log`; TRX under the gitignored `coverage/test-results/p5-t8-mutation/` and `coverage/test-results/p5-t8-restored/`
- EXIT_CODE: 0
- Output Summary: mutated run `Test Run Failed.`, `Total tests: 2`, `Failed: 2` (exit 1): the pure adoption test on the pre-predicted boolean (`adopts to be True ... but found False`) and the `InitFolderHandlerAsync` test on the pre-predicted unguarded construction path exception (`System.NullReferenceException` from `FolderPredictor.cs:39` via `EfcDataModel.Carry.cs:62`); restored run `Test Run Successful.`, `Total tests: 2`, `Passed: 2` (exit 0); file restored byte-identical (SHA-256 equal before mutation and after restoration), no BOM at HEAD and none introduced, `git diff --numstat HEAD -- QuickFiler/Controllers/EfcDataModel.Carry.cs` prints nothing.

## Mutation (temporary; restored)

MUTATION: in `QuickFiler/Controllers/EfcDataModel.Carry.cs` line 34 (re-derived before the edit; inside `TryAdoptCarriedFolderHandler` at lines 25-39, in the adopting branch `adopted = predictor; return true;` at `:33-34`) `return true;` becomes `return false;`, so the method always returns false (the `:38` branch already returns false). The `out` assignment at `:33` is left in place so the change is confined to the boolean.

PREDICTED-FAILING-ASSERTION (written before the mutated run):

- `EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts` fails on `adopts.Should().BeTrue("a concrete predictor with no explicit list must be adopted")` (`EfcDataModelIssue792CarryTests.cs:60`): the `OBSERVED:` line contains `adopts to be true` and `but found False`. The `adopted.Should().BeSameAs(carried)` assertion at `:61` is not reached.
- `EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry` fails on the unguarded construction path's exception rather than on an assertion: with adoption refused, `InitFolderHandlerAsync` (`EfcDataModel.Carry.cs:41-91`) falls into the null-list branch, `scoringInput` is null (the uninitialized model has neither `MailInfo` nor `CarriedMailHelper`), and `new FolderPredictor(Globals)` (`:62`) runs with a null `Globals`, so `FolderPredictor.cs:39` (`_olApp = AppGlobals.Ol.App;`) throws `NullReferenceException` inside `Task.Run`, which the `await` at `:135` of the test rethrows before `model.FolderHelper.Should().BeSameAs(carried)` (`:138`) is reached. The `OBSERVED:` line therefore names `System.NullReferenceException`. Had construction succeeded instead, the failure would land on the `:138` `BeSameAs` assertion (a fresh predictor is not the carried instance); the plan accepts either outcome, and this artifact records which one occurred.
- `Total tests: 2`, `Failed: 2`, `Test Run Failed.`, exit 1.

Needle (`adopted = predictor;` plus `return true;`, each with its CRLF terminator) matched once before and zero after; replacement present once; 100 lines before and after; `git diff --numstat HEAD` read `1 1`; the hunk was the single line 34 change. `BOM-BEFORE-MUTATION: False`, `BOM-AFTER-MUTATION: False`, `BOM-AFTER-RESTORE: False`.

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 28 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:45:02 to 20:46:28, test DLL 20:45:05 to 20:46:31.

OBSERVED (first `Error Message` line of each failed test, verbatim):

- `TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts`: `Expected adopts to be True because a concrete predictor with no explicit list must be adopted, but found False.`
- `InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry`: `Test method QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry threw exception:` followed by `System.NullReferenceException: Object reference not set to an instance of an object.` The stack trace in the gitignored `coverage/p5-t8-mutation-scoped.log` names `UtilitiesCS.FolderPredictor..ctor(IApplicationGlobals AppGlobals)` at `FolderPredictor.cs:line 39`, invoked from the `InitFolderHandlerAsync` lambda at `EfcDataModel.Carry.cs:line 62`, rethrown by the test's `await` at `EfcDataModelIssue792CarryTests.cs:line 135`.

Observed run: `Failed TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts [147 ms]`; `Failed InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry [11 ms]`; `Test Run Failed.`; `Total tests: 2`; `Failed: 2`; exit 1.

PREDICTION-MATCHES-OBSERVATION: true (the first on the boolean at `:60`; the second on the unguarded construction path's `NullReferenceException` from `FolderPredictor.cs:39` via `EfcDataModel.Carry.cs:62`, the exception outcome named as primary in the prediction above; the `:138` `BeSameAs` was not reached).

## Restoration proof

- SHA256-BEFORE-MUTATION: `C0E4085C52DEE82C074BE3EEC0375D7ADB6761E43AF75B90302A823C670B44DB` (equal to the gitignored snapshot `coverage/p5-t8-snapshot.cs`)
- SHA256-AFTER-RESTORE: `C0E4085C52DEE82C074BE3EEC0375D7ADB6761E43AF75B90302A823C670B44DB`
- RESTORED-IDENTICAL: true; BOM-AFTER-RESTORE: false (no BOM at HEAD, none introduced); needle count after restore 1, replacement count 0
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Controllers/EfcDataModel.Carry.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0
- Scoped porcelain (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`) after restoration: prints nothing

Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 25 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:46:28 to 20:46:49, test DLL 20:46:31 to 20:46:51.

Restored run: `Passed TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts [49 ms]`; `Passed InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry [1 ms]`; `Test Run Successful.`; `Total tests: 2`; `Passed: 2`; `Failed: 0 (omitted category)`; exit 0.
