# Code Review — Issue #283 (liveoutlook-harness-construction-scoped-skip)

- Timestamp: 2026-07-08T14-34
- Base: `main` @ `930467f4`; Head @ `143da1ed`
- Scope: full branch diff (feature-vs-base)

## Executive Summary

The change is a focused, well-structured defect fix. It introduces
`LiveOutlookHarnessRunner`, a host-neutral seam that classifies a live-Outlook harness outcome by
the phase in which an exception is thrown (construction COMException -> skip regardless of HRESULT;
any exercise-phase exception -> captured failure). This is a correct and minimal fix for the
reported defect (a construction-phase `0x80010100` RPC_E_SYS_CALL_FAILED previously fell through a
narrow HRESULT whitelist and failed the test). The integration test is refactored to route both
phases through the seam and the whitelist is removed; the timing/completion assertions are
preserved. Seven deterministic MSTest tests exercise both phases, both null guards, and the success
path, using `Func<T>`/`Action<T>` delegate seams rather than Moq (appropriate — the delegates are
the injection point). Naming, XML docs, net481 constraints, and FluentAssertions usage all conform
to repo standards.

No Blocking code-quality defects were found in the changed code itself. The Blocking items for this
review are policy/process gaps (CI workflow green-run evidence, absent canonical coverage
artifacts) documented in the policy audit, not code defects. The findings below are code-level
observations only.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | docs/.../evidence/qa-gates/ac-verification-summary.md | AC4 row | Seam coverage recorded as 90.0% here, but `coverage-delta.md` and `csharp-test-coverage-final.md` record 100.0% (30/30) after the coordinator-directed non-COM catch test was added. | Reconcile to the final 100.0% figure so the evidence set is internally consistent. | Conflicting evidence undermines machine-unverifiable coverage claims. | `ac-verification-summary.md` L11 vs `coverage-delta.md` L9. |
| Low | TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunner.cs | `HarnessOutcome` struct L48-71 | The "exactly one of `Captured`/`SkipReason` is meaningful" invariant is a documented convention, not enforced by the type (both are independently settable via the constructor). | Optional: add a small factory (`Skip(...)`, `Capture(...)`, `Success()`) or a `Debug.Assert` guarding mutual exclusivity. | Encoding the invariant in the API prevents future misuse; low risk since the only caller is the seam itself. | Constructor `HarnessOutcome(Exception?, string?)` L55-59. |
| Info | TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunnerTests.cs | Whole file | Tests are deterministic, isolated, AAA-structured, use FluentAssertions with descriptive because-reasons, no live Outlook, no temp files, no banned timing APIs. | None. | Confirms UT policy conformance for the new tests. | Direct inspection; `powershell`-independent C# suite 231/231 per evidence. |
| Info | TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs | L27-95 | Whitelist (`IsOutlookUnavailableHResult` + three HRESULT constants) removed; both phases routed through the seam; `maxTickBlockMs`, `completed`, and latency assertions preserved unchanged. | None. | Confirms the fix does not weaken the live-path timing/completion assertions. | Diff `930467f4..143da1ed`. |
| Info | .github/workflows/ci.yml; scripts/vscode/Invoke-MSTest.ps1; scripts/vscode/Invoke-MSTestWithCoverage.ps1 | vstest invocation lines | `/TestCaseFilter:TestCategory!=LiveOutlook` appended consistently in CI and both local QC arg builders; the CI line remains guarded by the existing `if ($LASTEXITCODE -ne 0) { throw }`. | None (code-level); green-run evidence is required per policy audit. | Confirms AC5 code changes are correct and consistent across CI and local runners. | Diff `930467f4..143da1ed`. |

## Notes

- No banned APIs (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`)
  are introduced in the new C# code. The live harness continues to use `Application.DoEvents()` /
  `Thread.Yield()` in its STA pump loop; this is pre-existing code in the excluded LiveOutlook path
  and is unchanged by this fix.
- Change budget: PowerShell edits touch 2 production scripts + 1 test file, within the PowerShell
  direct-mode budget.
