# [P5-T1] AC-U4 non-vacuity mutation: notifier call dropped from the default boundary sink

- Issue: #792
- Timestamp: 2026-09-17T20-21
- Command: CMD-OUTLOOK (`Get-Process -Name OUTLOOK`, printed `OUTLOOK-CLOSED: true` before each build), then for the mutated tree and again for the restored tree: CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST (vswhere resolved `vstest.console.exe`), CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~EfcFormControllerIssue792Tests.PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink|FullyQualifiedName~EfcFormControllerTests.PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` and `<task>` = `p5-t1-mutation` (mutated) and `p5-t1-restored` (restored); restoration by `git checkout -- QuickFiler/Controllers/EfcFormController.cs`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory (branch asserted by the helper); build and test consoles in the gitignored `coverage/p5-t1-mutation-build.log`, `coverage/p5-t1-mutation-scoped.log`, `coverage/p5-t1-restore-build.log`, `coverage/p5-t1-restored-scoped.log`; TRX under the gitignored `coverage/test-results/p5-t1-mutation/` and `coverage/test-results/p5-t1-restored/`
- EXIT_CODE: 0
- Output Summary: mutated run `Test Run Failed.`, `Total tests: 2`, `Passed: 1`, `Failed: 1` (exit 1): the new default-sink test failed on the pre-predicted `ContainSingle` assertion and the pre-existing sink-substituting test PASSED; restored run `Test Run Successful.`, `Total tests: 2`, `Passed: 2` (exit 0); file restored byte-identical (SHA-256 equal before mutation and after restoration), BOM preserved, `git diff --numstat HEAD -- QuickFiler/Controllers/EfcFormController.cs` prints nothing.

## Mutation (temporary; restored)

MUTATION: in `QuickFiler/Controllers/EfcFormController.cs` the statement line `UserFaultNotifier?.Invoke(message);` (line 140, inside `DefaultBoundaryErrorSink` at lines 137-141, re-derived before the edit) was deleted. Needle matched exactly once before the edit and zero times after; the file went from 266 to 265 lines; `git diff --numstat HEAD` on the mutated file read `0 1` and the only hunk was `-            UserFaultNotifier?.Invoke(message);`. The file carries a UTF-8 BOM at HEAD; the BOM was present before the edit, after the edit and after restoration (`BOM-BEFORE-MUTATION: True`, `BOM-AFTER-MUTATION: True`, `BOM-AFTER-RESTORE: True`).

PREDICTED-FAILING-ASSERTION: `EfcFormControllerIssue792Tests.PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink` fails on `captured.Should().ContainSingle("the default boundary sink must surface the contained fault to the user exactly once")` (`EfcFormControllerIssue792Tests.cs:101-106`): expected one captured notification, actual none (the `NotThrowAsync` assertion before it still passes because the fault is still contained by the sink's logger call). `EfcFormControllerTests.PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` (`EfcFormControllerTests.cs:299-328`) PASSES because it substitutes `BoundaryErrorSink` with a counting lambda, so the default sink's body never runs for it.

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 21 `CoreCompile:` lines (unanchored count), 10 `csc.exe` lines; `QuickFiler/bin/Debug/QuickFiler.dll` rewritten at 20:21:59 and `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` at 20:22:01, both after the 20:21:56 edit (the helper's first invocation compiled the mutation and then stopped on a helper-side reporting defect before the test run; the second invocation's build was incremental, 18 `CoreCompile:` lines, 0 `csc.exe`, DLL mtimes unchanged, and ran the tests against those mutated assemblies).

OBSERVED (first `Error Message` line, verbatim from the console): `Expected captured to contain a single item because the default boundary sink must surface the contained fault to the user exactly once, but the collection is empty.`

Observed run: `Passed PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault [51 ms]`; `Failed PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink [144 ms]`; `Test Run Failed.`; `Total tests: 2`; `Passed: 1`; `Failed: 1`; exit 1.

PREDICTION-MATCHES-OBSERVATION: true (new test red on the notifier-count assertion; pre-existing test green under the same mutation, which is the recorded proof that the pre-existing test alone could not pin the user surface).

## Restoration proof

- SHA256-BEFORE-MUTATION: `0B122F9A1ABB9C20C252612A4F616456ADCBEBF27E3603CBFCC2F7EA68316320` (also the SHA-256 of the pre-mutation snapshot `coverage/p5-t1-snapshot.cs`, gitignored)
- SHA256-AFTER-RESTORE: `0B122F9A1ABB9C20C252612A4F616456ADCBEBF27E3603CBFCC2F7EA68316320`
- RESTORED-IDENTICAL: true
- BOM-AFTER-RESTORE: true
- Needle count after restore: 1
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Controllers/EfcFormController.cs` prints nothing
- `git diff --no-index --numstat` between the pre-mutation snapshot and the restored file: exit 0 (identical)
- Scoped porcelain (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`) after restoration: prints nothing

Restored build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 25 `CoreCompile:` lines, 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:21:59 to 20:23:02, test DLL 20:22:01 to 20:23:05.

Restored run: `Passed PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault [66 ms]`; `Passed PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink [72 ms]`; `Test Run Successful.`; `Total tests: 2`; `Passed: 2`; `Failed: 0 (omitted category)`; exit 0.
