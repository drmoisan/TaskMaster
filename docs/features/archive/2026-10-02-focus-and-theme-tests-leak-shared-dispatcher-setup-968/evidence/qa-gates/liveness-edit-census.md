# Liveness-edit census (issue #968, tasks P5-T2 to P5-T5)

Timestamp: 2026-10-03T03-11
Command: pwsh -NoProfile -Command '<CMD-EOL payload>' with FILE = `QuickFiler.Test\TestSupport\ArmingFakeTimeProvider.cs` (AFTP), the first of nine payloads (then CMD-LINECOUNT on LIV, DMT and AFTP; CMD-TOKEN-COUNT on LIV, DMT and PROJ with the P0-T13 lists and on AFTP with the P5-T5 list; CMD-SPAN-TOKEN-COUNT on T1-LIVE, HELD and T-SIB; each the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted), followed by two git calls
Canonical command: as listed; git -C WORKTREE diff --numstat HEAD -- QuickFiler QuickFiler.Test; git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (every payload)
- P5-T2 CMD-EOL on AFTP: BARE_LF: 0; CRLF_COUNT: 49; LINES: 49 (CRLF_COUNT equals LINES; at most 500 and at least 30)
- P5-T2 project item: exactly one project-file line contains `TestSupport\ArmingFakeTimeProvider.cs`; it was inserted immediately after the `TestSupport\SynchronousBackgroundWorker.cs` line, and the line after it is the `TestSupport\WinFormsPumpHostTests.cs` item
- LINES: LIV 348, DMT 392, AFTP 49 (each at most 500)
- LIV tokens (P0-T13 order): 0, 2, 2, 4, 4, 4, 0, 0, 2, 1, 3, 0, 1, 4 (`Task.Yield` 0, `fake.Advance` 0, `FakeTimeProvider` 2 = the two `ArmingFakeTimeProvider` lines of L-T1, `NoSynchronizationContext` 3, `new ArmingFakeTimeProvider()` 1, `new SynchronousBackgroundWorker()` 4, `using (var worker = new SynchronousBackgroundWorker())` 4, `StartSynchronously` 2, `[TestMethod]` 4)
- DMT tokens (P0-T13 order): 2, 2, 1, 1, 1, 1, 1, 0, 2, 6, 1, 9
- PROJ tokens: `<Compile Include=` 190 (PROJ-COMPILE-ITEMS-BASE 187 plus 3), then 1, 1, 1, 1, 1
- AFTP tokens: 1, 1, 1, 1, 1, 1, 1, 1
- T1-LIVE: SPAN: 102-169; `await` 3, `using (NoSynchronizationContext())` 2, `Task.Yield` 0, `fake.Advance` 0, `for (int i` 0, `clock.ReArm();` 1, `(await pending)` 1
- HELD: SPAN: 207-241; `new SynchronousBackgroundWorker()` 0, `SynchronousBackgroundWorker worker,` 1, `SynchronousBackgroundWorker.StartSynchronously` 1
- T-SIB: SPAN: 103-152; `await Task.Yield();` 0, `clock.ReArm();` 1, `await Task.WhenAny(clock.Armed, pending)` 1, `using (var worker = new BackgroundWorker())` 1, `IList<MailItem> result = await pending;` 1
- SCOPE-BODIES-AWAIT-FREE: YES. Reading the T1-LIVE span, the three `await` lines are 116 (`return await loaderRelease.Task;`, inside the loader lambda), 141 (`Task first = await Task.WhenAny(clock.Armed, pending);`) and 164 (`(await pending)`); the two `using (NoSynchronizationContext())` bodies are lines 124 to 132 and 156 to 158, and neither contains an `await` line.
- numstat (exit 0):
  - `137	101	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
  - `2	17	QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
  - `68	47	QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
  - `41	49	QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
  - `20	35	QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
  - `20	18	QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
  - `48	15	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
  - `16	14	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
  - `3	0	QuickFiler.Test/QuickFiler.Test.csproj`
  - `1	129	QuickFiler/Controllers/QfcDatamodel.cs`
- porcelain (exit 0), thirteen lines: the twelve P4-T9 lines plus `?? QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs`:
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
  - `?? QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs`
  - `?? QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`
