# Test-edit census (issue #968, tasks P3-T1 to P3-T9)

Timestamp: 2026-10-03T03-02
Command: pwsh -NoProfile -Command '<CMD-LINECOUNT payload>' with FILES = CS5 (the first of nine payloads; then CMD-TOKEN-COUNT on FAT, on TS with the P0-T12 TS list extended by "Issue #480 shared arrange helper", and on FT; CMD-SPAN-TOKEN-COUNT on R4SPAN, R4HEAD and R4TAIL; CMD-HUNKS on QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs and on QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; each the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted), followed by two git calls
Canonical command: CMD-LINECOUNT, CMD-TOKEN-COUNT, CMD-SPAN-TOKEN-COUNT and CMD-HUNKS as listed; git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test; git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (every payload)
- LINES: FIX 375, FAT 482, TS 442, FT 472, PC 248 (PC at most 500)
- FAT tokens (`EnsureUiThreadDispatcher`, `private static Mock<IItemViewer> BuildExecutingViewer`, `QfcItemControllerTestSupport.BuildExecutingViewer()`, `BuildExecutingViewer`, `absorbs the delegate without running it`, `shared UiThread static is irrelevant`, `absorbs the queued application`, `[TestMethod]`): 0, 0, 8, 8, 1, 1, 1, 17
- TS tokens (`Becomes moot`, `leaks exactly`, `still delegate to a callee`, `not reachable from another test file`, `remaining legitimate`, `QfcItemController_UiThreadDispatcherPinCountTests`, `internal static void EnsureSynchronizationContext()`, `UiThreadDispatcherFixture.EnsureDispatcher();`, `Issue #480 shared arrange helper`): 0, 0, 0, 0, 1, 1, 1, 1, 1
- FT tokens (`no other class may dispose`, `removed that pin: the fixture now counts pins`, `(W5) must not latch`, `[Timeout(GateTimeoutMs)]`, `private const int GateTimeoutMs = 60000;`, `EnsureUiThreadDispatcher()`, `issue #230 lost update`, `the waiter cannot observe the pre-restore value`, `[TestMethod]`): 0, 1, 1, 8, 1, 3, 1, 1, 8
- R4SPAN: SPAN: 212-286; `EnsureUiThreadDispatcher()` 0, `using (` 1, `transactionA.Dispose();` 2, `finally` 3, `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1
- R4HEAD: SPAN: 212-222; `EnsureUiThreadDispatcher()` 0, `using (` 0, `try` 2
- R4TAIL: SPAN: 265-275; `}` 4, `finally` 2, `transactionA.Dispose();` 1
- CMD-HUNKS TestSupport: GIT_DIFF_EXIT_CODE: 0; `HUNK @@ -214,25 +214,27 @@ namespace QuickFiler.Controllers.Tests`; `HUNK @@ -282,9 +284,9 @@ namespace QuickFiler.Controllers.Tests`; HUNK_COUNT: 2 (every old-range start at or above 200; the EnsureSynchronizationContext region 85 to 96 is untouched, AC17)
- CMD-HUNKS fixture tests: GIT_DIFF_EXIT_CODE: 0; `HUNK @@ -194,17 +194,17 @@`; `HUNK @@ -218,9 +218,7 @@`; `HUNK @@ -268,6 +266,10 @@`; HUNK_COUNT: 3 (old ranges 194-210, 218-226, 268-273: each starts at or above 190 and ends at or below 285, so every hunk lies in R4's doc and body, AC10)
- numstat (exit 0): `20	35	QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`; `20	18	QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`; `48	15	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`; `16	14	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`; `1	0	QuickFiler.Test/QuickFiler.Test.csproj`
- porcelain (exit 0), exactly the five #968 .cs paths (four ` M`, one `??`) and the project file:
  - ` M QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
  - ` M QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
  - ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
  - ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
  - ` M QuickFiler.Test/QuickFiler.Test.csproj`
  - `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`

Per-task acceptance readings: P3-T1 private helper count 0 and the line after `BuildFocusController`'s closing brace and one blank line is the `/// <summary>` of `EnableHandlelessThemeInvoke`; P3-T2 prefixed helper 8 and bare helper 8; P3-T3 `absorbs the delegate without running it` 1 and `shared UiThread static is irrelevant` 1 with `var controller = new FocusController();` as the first statement of SetThemeDark_FromNormal_SelectsDarkNormalTheme; P3-T4 `absorbs the queued application` 1 and FAT `EnsureUiThreadDispatcher` 0; P3-T5 and P3-T6 as the TS values above; P3-T7 as the FT values above; P3-T8 as the R4 span values above, with `[Timeout(GateTimeoutMs)]` 8 and the constant 1 unchanged.
