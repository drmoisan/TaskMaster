# P8-T5 — Final QC: Full Suite with Coverage

Issue: #230
Task: [P8-T5]
Phase 8 loop iteration: 2 (the iteration-1 run failed; see "Iteration 1" below)

## Iteration 2 — the clean pass

- Timestamp: 2026-08-08T00-05
- Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/qa-gates/coverage-final.cobertura.xml`
- EXIT_CODE: 0
- Output Summary: **Total tests: 6293 — Passed: 6293, Failed: 0, Skipped: 0.**
  9 test assemblies discovered. Cobertura root `<coverage>` element reports
  **`line-rate="0.858333"` (85.8333%)** and **`branch-rate="0.792226"`
  (79.2226%)**, with `lines-covered="95293"`, `lines-valid="111021"`,
  `branches-covered="22073"`, `branches-valid="27862"`. Coverage artifact written to
  `evidence/qa-gates/coverage-final.cobertura.xml`.

This is the coverage-enabled test stage required by CUT3: the script wraps
`vstest.console.exe ... /InIsolation` inside `dotnet-coverage collect`.

### Repo-wide executed counts (audit trail, not a wiring gate)

The run discovers every first-party `*.Test.dll`, so 6293 is a repository-wide
figure, not an assembly-scoped one. Wiring is proven separately in P7-T8 by csproj
enumeration plus `/ListTests` discovery. Baseline was 6272 tests; the delta of +21
matches exactly the 21 test methods this feature adds (P7-T8 static enumeration).

| Metric | Baseline (P0-T6) | Post-change (P8-T5) | Delta |
|---|---:|---:|---:|
| Total tests | 6272 | 6293 | +21 |
| Passed | 6272 | 6293 | +21 |
| Failed | 0 | 0 | 0 |

### U-AC5 evidence

The run completed unattended with **no hang, no dialog, and no external process**:
no live Outlook, and no WebView2 runtime initialization (every
`IWebViewCoreInitializer` is mocked and faults at the seam). Every one of the 21
new tests completed inside its `[Timeout]` bound.

## Iteration 1 — failed run that forced the loop restart

- Timestamp: 2026-08-07T23-55
- Command: identical to the above
- EXIT_CODE: 1
- Output Summary: **Total tests: 6293 — Passed: 6291, Failed: 2.** Both failures
  were `[Timeout]` expiries after 60000 ms, not assertion failures:
  - `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`
  - `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController`

### Root cause and fix

One failure came from each of the two test classes that swap the process-wide
static `UtilitiesCS.UiThread.Dispatcher`
(`QfcItemController_InitializationTests` and `QfcItemController_SeamFactoryTests`).
MSTest class-level parallelization runs those classes concurrently, so their
fixtures could interleave: class B's `PumpHarness.Restore()` reverted the static
to the value it had captured — the deliberately **parked** dispatcher seeded by
`QfcItemControllerTestSupport.EnsureUiThreadDispatcher` — while class A's member
under test was still awaiting a `QfcTipsDetails.ToggleAsync` dispatcher operation.
The parked dispatcher never runs a frame, so that await could never complete and
the test failed on its `[Timeout]` instead of on an assertion. The other five
pump-hosted tests, which do not sit inside that window when the swap occurs,
passed in the same run.

This is a genuine test-isolation defect (UT1 Independence; plan D11 "no static or
shared host state"), not a flake to be retried. The fix adds a static
`SemaphoreSlim(1, 1)` gate held from `BuildPumpHarnessAsync` through
`PumpHarness.Restore()`, so exactly one pump fixture owns the static at a time
across every test class in the assembly. `Restore()` is now idempotent so it
cannot over-release, and `BuildPumpHarnessAsync` releases the gate if construction
throws. The gate is a deterministic completion signal — released by the preceding
test's `Restore`, never by elapsed time — so it introduces no wall-clock wait and
does not violate D10.

Per the plan's restart rule, Phase 8 was restarted from P8-T1 after this fix; the
iteration-2 results above are the clean pass.
