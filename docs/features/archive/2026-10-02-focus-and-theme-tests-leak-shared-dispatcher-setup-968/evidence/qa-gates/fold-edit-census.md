# Fold-edit census (issue #968, tasks P4-T2 to P4-T9)

Timestamp: 2026-10-03T03-07
Command: pwsh -NoProfile -Command '<CMD-EOL payload>' with FILE = `QuickFiler.Test\TestSupport\SynchronousBackgroundWorker.cs` (SBW), the first of eleven payloads (then CMD-LINECOUNT on FOLD6 plus SBW; CMD-TOKEN-COUNT on LIV, TD, ZB, DMT, QDM and PROJ with the P0-T13 lists and on SBW with the P4-T9 list; CMD-SPAN-TOKEN-COUNT on HELD; CMD-HUNKS on QuickFiler/Controllers/QfcDatamodel.cs; each the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted), followed by two git calls
Canonical command: as listed; git -C WORKTREE diff --numstat HEAD -- QuickFiler QuickFiler.Test; git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (every payload)
- P4-T2 CMD-EOL on SBW: BARE_LF: 0; CRLF_COUNT: 27; LINES: 27 (CRLF_COUNT equals LINES; at most 500 and at least 20)
- LINES: LIV 311, TD 229, ZB 224, DMT 374, QDM 367, QQP 413, SBW 27 (every value at most 500; QDM at most 400)
- LIV tokens (P0-T13 order): 0, 2, 2, 4, 3, 4, 3, 3, 1, 1, 0, 0, 0, 4 (`class SynchronousBackgroundWorker` 0, `StartSynchronously` 2, `SynchronousBackgroundWorker.StartSynchronously` 2, `new SynchronousBackgroundWorker()` 4, `using (var worker = new SynchronousBackgroundWorker())` 3, `StartHeldOpenLoader(` 4, `Task.Yield` 3, `fake.Advance` 3, `FakeTimeProvider` 1, `using QuickFiler.Test.TestSupport;` 1, `NoSynchronizationContext` 0, `Duplicated per file` 0, `new ArmingFakeTimeProvider()` 0, `[TestMethod]` 4)
- TD tokens: 0, 1, 1, 1, 0, 1, 5
- ZB tokens: 0, 3, 3, 3, 3, 0, 0, 0, 0, 0, 1, 3
- DMT tokens: 2, 1, 0, 0, 0, 1, 0, 1, 4, 5, 1, 9 (`new BackgroundWorker()` 2, `using (var worker = new BackgroundWorker())` 1, `using QuickFiler.Test.TestSupport;` 1, `[TestMethod]` 9)
- QDM tokens: 0, 0, 1, 0, 2, 0, 0, 1, 0, 0, 6, 6, 1, 0, 0, 0, 0, 1, 2, 2, 3
- PROJ tokens: `<Compile Include=` 189 (PROJ-COMPILE-ITEMS-BASE 187 plus 2), then 1, 1, 0, 1, 1
- SBW tokens (`class SynchronousBackgroundWorker`, `internal sealed class SynchronousBackgroundWorker : BackgroundWorker`, `internal static void StartSynchronously(BackgroundWorker worker)`, `Dispose`, `namespace QuickFiler.Test.TestSupport`): 1, 1, 1, 1, 1
- HELD: SPAN: 170-204; `new SynchronousBackgroundWorker()` 0, `SynchronousBackgroundWorker worker,` 1, `SynchronousBackgroundWorker.StartSynchronously` 1
- CMD-HUNKS QfcDatamodel.cs: GIT_DIFF_EXIT_CODE: 0; HUNK_COUNT: 7 (recorded, not gated); hunks `@@ -96,9 +96,6 @@`, `@@ -106,9 +103,6 @@`, `@@ -191,7 +185,6 @@`, `@@ -206,8 +199,6 @@`, `@@ -240,30 +231,6 @@`, `@@ -360,13 +327,12 @@`, `@@ -375,102 +341,8 @@`
- numstat (exit 0):
  - `65	66	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
  - `2	17	QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
  - `25	22	QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
  - `41	49	QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
  - `20	35	QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
  - `20	18	QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
  - `48	15	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
  - `16	14	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
  - `2	0	QuickFiler.Test/QuickFiler.Test.csproj`
  - `1	129	QuickFiler/Controllers/QfcDatamodel.cs` (128 removed lines plus the replaced line at old 369, whose replacement is the only added line)
- porcelain (exit 0), twelve lines:
  - ` M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
  - ` M QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
  - ` M QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
  - ` M QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
  - ` M QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
  - ` M QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
  - ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
  - ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
  - ` M QuickFiler.Test/QuickFiler.Test.csproj`
  - ` M QuickFiler/Controllers/QfcDatamodel.cs`
  - `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`
  - `?? QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`
  (QfcDatamodel.QueueProcessing.cs is untouched until P5-T11.)
- P4-T3: exactly one project-file line contains `TestSupport\SynchronousBackgroundWorker.cs`; the line before it contains `TestSupport\DedicatedWorkerThread.cs` and the line after it contains `TestSupport\WinFormsPumpHostTests.cs` (in-place Edit immediately after the DedicatedWorkerThread item, which was immediately followed by the WinFormsPumpHostTests item at fact 8).
