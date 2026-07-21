# Regression Fail-Before Exception Dossier (Issue #283)

Timestamp: 2026-07-08T17-56

## Defect (pre-fix statement)
The developer-only harness `LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`
(`TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`) classified Outlook
unavailability with a narrow three-HRESULT whitelist (`IsOutlookUnavailableHResult`:
`0x80040154`, `0x80040112`, `0x80080005`). A construction-phase `COMException` with HRESULT
`0x80010100` (RPC_E_SYS_CALL_FAILED) thrown by `new Outlook.Application()` was NOT in that set,
so it fell through the `when` guard to the generic `catch (Exception ex) { captured = ex; }` and
the test FAILED — even though no code-under-test had run (a pure environment/launch failure).

## WhyFailingRunImpossible
The fix extracts the classification into a NEW host-neutral seam
(`LiveOutlookHarnessRunner.Run<T>`). There is no pre-existing extracted seam to author a
"fails-before" unit test against: the regression tests (`LiveOutlookHarnessRunnerTests`) target a
type that did not exist before this change. A literal red-before run of these tests is therefore
structurally impossible — the type under test is introduced by the same change that makes them
green. Additionally, the live harness path cannot be executed here (it constructs a live Outlook;
it is `[TestCategory("LiveOutlook")]` and excluded from this environment by
`/TestCaseFilter:TestCategory!=LiveOutlook`).

## Alternative proof (behavior-diff against the removed whitelist)
The removed whitelist logic `hr == 0x80040154 || hr == 0x80040112 || hr == 0x80080005` returns
`false` for `0x80010100`. Under the OLD code that `false` routed a construction `0x80010100`
COMException to the generic capture branch → test failure. Under the NEW seam, a construction
COMException is classified as a skip regardless of HRESULT.

## Post-fix green proof (deterministic, standard suite)
From P2-T4 (`csharp-test-coverage-final.md`): 230/230 tests pass, including the 7 new
`LiveOutlookHarnessRunnerTests`:
1. Construction COMException `0x80010100` -> `SkipReason` non-null containing "80010100", `Captured` null. (Directly proves the old defect is fixed: the exact defect HRESULT now skips instead of failing.)
2. Construction COMException `0x80004005` (E_FAIL, arbitrary) -> skip, `Captured` null. (Proves "regardless of HRESULT".)
3. Exercise-phase `InvalidOperationException` -> `Captured` = that exception, `SkipReason` null. (Strict exercise failure retained.)
4. Exercise-phase `COMException` -> `Captured` = that COMException, `SkipReason` null. (COMExceptions in the exercise phase are NOT swallowed as skips.)
5. Success path -> `Captured` null, `SkipReason` null, exercise side-effect flag true.
6. Null `construct` -> `ArgumentNullException`.
7. Null `exercise` -> `ArgumentNullException`.

Net: the construction-phase COMException (including `0x80010100`) now yields a skip; exercise-phase
exceptions (including COMExceptions) still fail. Defect resolved with strict failure semantics
preserved for code-under-test.
