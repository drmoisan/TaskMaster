# Pre-change census of the folded-scope files and the project file (issue #968, task P0-T13)

Timestamp: 2026-10-03T02-49
Command: pwsh -NoProfile -Command '<CMD-LINECOUNT payload>' with FILES = FOLD6 (the first of thirteen separate payloads listed below; each is the named Command Reference macro executed verbatim with PREFIX expanded, WORKTREE substituted and the stated FILE, FILES, START, END and TOKENS substituted)
Canonical command: CMD-LINECOUNT, CMD-HASH, CMD-TOKEN-COUNT (LIV, TD, ZB, DMT, QDM, QQP, PROJ) and CMD-SPAN-TOKEN-COUNT (T1-LIVE, HELD, T-SIB, GATE-LAMBDA)
EXIT_CODE: 0
Output Summary: WORKTREE-LEAF agent-a291a7fbabf9d0229 in every payload; every payload exited 0; LINES 312, 244, 232, 371, 495, 413; every token and span value equals the plan's expected pre-change value (no FOLD CENSUS MISMATCH). Details below.

## CMD-LINECOUNT on FOLD6

- LINES QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 312
- LINES QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 244
- LINES QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 232
- LINES QuickFiler.Test\Controllers\QfcDatamodelTests.cs = 371
- LINES QuickFiler\Controllers\QfcDatamodel.cs = 495
- LINES QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs = 413

## CMD-HASH on FOLD6

- BASE-HASH: QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 04A1963C8D577FB6FD43079DD05446600399832EC3438971D08503CB2B11870A
- BASE-HASH: QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 0F076832B8ACEC32BA61D822D19397E7ADF9FECE4424C2EC3F73B4D7D277D3F1
- BASE-HASH: QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 4B3D6D3FD67ABC2F583EEABB6FAFEC22D3561A7B72AEC553556F31C2A30B32EE
- BASE-HASH: QuickFiler.Test\Controllers\QfcDatamodelTests.cs = B9AB3F7B59001008DBA5A7ADB31D85455A2E7F05A8C09715039FE0F2F36D6DB5
- BASE-HASH: QuickFiler\Controllers\QfcDatamodel.cs = B06004A654EB1630B4759D378271187BA252F1FCC44486B9DFF9B4B08C24491F
- BASE-HASH: QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs = B54E4CE3654FC22E572A46D2AD78CBD28C9342E650E5B3D8C1861139990A0DBB

## CMD-TOKEN-COUNT (token lists exactly as in the P0-T13 task text, in that order)

- LIV: 1, 3, 0, 2, 0, 4, 3, 3, 1, 0, 0, 0, 0, 4 (`FakeTimeProvider` 1 is line 114 only; the `using Microsoft.Extensions.Time.Testing;` directive does not contain the token)
- TD: 1, 2, 0, 1, 1, 0, 5
- ZB: 1, 4, 0, 3, 0, 1, 1, 1, 1, 1, 0, 3
- DMT: 2, 0, 0, 0, 0, 1, 0, 1, 4, 5, 0, 9
- QDM: 2, 4, 0, 1, 4, 1, 1, 1, 2, 2, 7, 7, 1, 2, 1, 1, 2, 1, 2, 2, 3
- QQP: 1, 1, 2, 1, 3, 0, 0, 1, 1, 0, 1
- PROJ: `<Compile Include=` 187 (PROJ-COMPILE-ITEMS-BASE: 187), then 0, 0, 0, 1, 1

## CMD-SPAN-TOKEN-COUNT

- T1-LIVE (LIV): SPAN: 110-165; `await` 5, `using (NoSynchronizationContext())` 0, `Task.Yield` 3, `fake.Advance` 3, `for (int i` 1, `clock.ReArm();` 0, `(await pending)` 1
- HELD (LIV): SPAN: 183-217; `new SynchronousBackgroundWorker()` 1, `SynchronousBackgroundWorker worker,` 0, `SynchronousBackgroundWorker.StartSynchronously` 0
- T-SIB (DMT): SPAN: 96-133; `await Task.Yield();` 1, `clock.ReArm();` 0, `await Task.WhenAny(clock.Armed, pending)` 0, `using (var worker = new BackgroundWorker())` 0, `IList<MailItem> result = await pending;` 1
- GATE-LAMBDA (QQP): SPAN: 299-310; `() => _remainingLoadActive,` 1, `() => false,` 0

Each printed span end is the END anchor line minus one. The non-zero counts (three nested worker classes, the old-shape `Task.Yield` and retry loop, the four legacy members, the stale comment tokens) are the positive controls for the zero gates of P4-T9, P5-T5, P5-T12 and P6-T2.
