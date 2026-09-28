# [P5-T6] AC-U3 non-vacuity mutation: pop-out carry deposit removed from the EfcHomeController constructor

- Issue: #792
- Timestamp: 2026-09-17T20-43
- Command: CMD-OUTLOOK (`OUTLOOK-CLOSED: true` before each build), then for the mutated tree and again for the restored tree: CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST, CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~QfcCollectionControllerIssue792PopOutTests.EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController` and `<task>` = `p5-t6-mutation` / `p5-t6-restored`; restoration by `git checkout -- QuickFiler/Controllers/EfcHomeController.cs`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; consoles in the gitignored `coverage/p5-t6-mutation-build.log`, `coverage/p5-t6-mutation-scoped.log`, `coverage/p5-t6-restore-build.log`, `coverage/p5-t6-restored-scoped.log`; TRX under the gitignored `coverage/test-results/p5-t6-mutation/` and `coverage/test-results/p5-t6-restored/`
- EXIT_CODE: 0
- Output Summary: mutated run `Test Run Failed.`, `Total tests: 1`, `Failed: 1` (exit 1) on the pre-predicted captured-handler reference assertion (`but found <null>`); restored run `Test Run Successful.`, `Total tests: 1`, `Passed: 1` (exit 0); file restored byte-identical (SHA-256 equal before mutation and after restoration), UTF-8 BOM present at HEAD and preserved through the edit and the restoration, `git diff --numstat HEAD -- QuickFiler/Controllers/EfcHomeController.cs` prints nothing.

## Mutation (temporary; restored)

MUTATION: in `QuickFiler/Controllers/EfcHomeController.cs` lines 87-88 (re-derived before the edit; inside the constructor, after the `DataModelFactory` call at `:77-82` and before the `DataModel.Mail is not null` branch at `:90`) the two statements `DataModel.CarriedFolderHandler = carriedFolderHandler;` and `DataModel.CarriedMailHelper = carriedMailHelper;` are deleted. The file carries a UTF-8 BOM at HEAD.

PREDICTED-FAILING-ASSERTION (written before the mutated run): `QfcCollectionControllerIssue792PopOutTests.EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController` fails on `capturedHandler.Should().BeSameAs(handler, "the carried handler must be on the data model at factory time")` (`QfcCollectionControllerIssue792PopOutTests.cs:202-204`): the form-controller factory (`:171-185`) is still invoked, so the preceding `formFactoryCalled.Should().BeTrue(...)` (`:199-201`) passes, but the factory reads `dataModel.CarriedFolderHandler` (`:182`) from a data model that was never deposited into, so `capturedHandler` is null and the reference assertion reports a null actual. FluentAssertions raises on the first failing assertion, so the `capturedHelper` (`:205-207`) and persisted-deposit (`:208-210`) assertions are not reached.

Needle (the two lines plus their CRLF terminators) matched once before and zero after; 464 lines before, 462 after; `git diff --numstat HEAD` read `0 2`; the hunk was the deletion of lines 87-88 only. `BOM-BEFORE-MUTATION: True`, `BOM-AFTER-MUTATION: True`, `BOM-AFTER-RESTORE: True`.

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 26 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:41:46 to 20:43:10, test DLL 20:41:49 to 20:43:12.

OBSERVED (first `Error Message` line, verbatim): `Expected capturedHandler to refer to Mock<IFolderSearchHandler:1>.Object because the carried handler must be on the data model at factory time, but found <null>.`

Observed run: `Failed EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController [292 ms]`; `Test Run Failed.`; `Total tests: 1`; `Failed: 1`; exit 1.

PREDICTION-MATCHES-OBSERVATION: true (captured-handler reference assertion at `:202-204`, actual null; the `formFactoryCalled` assertion passed).

## Restoration proof

- SHA256-BEFORE-MUTATION: `20FC90DB68FB1CA5199311B2494AD1FB33EF43FEDDB2764BB72359BEF14D64DE` (equal to the gitignored snapshot `coverage/p5-t6-snapshot.cs`)
- SHA256-AFTER-RESTORE: `20FC90DB68FB1CA5199311B2494AD1FB33EF43FEDDB2764BB72359BEF14D64DE`
- RESTORED-IDENTICAL: true; BOM-AFTER-RESTORE: true; needle count after restore 1
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Controllers/EfcHomeController.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0
- Scoped porcelain (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`) after restoration: prints nothing

Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 23 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:43:10 to 20:43:29, test DLL 20:43:12 to 20:43:32.

Restored run: `Passed EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController [250 ms]`; `Test Run Successful.`; `Total tests: 1`; `Passed: 1`; `Failed: 0 (omitted category)`; exit 0.
