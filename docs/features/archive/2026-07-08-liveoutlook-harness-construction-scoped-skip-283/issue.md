# liveoutlook-harness-construction-scoped-skip (Issue #283)

- Date captured: 2026-07-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/liveoutlook-harness-construction-scoped-skip/ (Issue #283)
- Type: Bug

- Issue: #283
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/283
- Last Updated: 2026-07-08
- Work Mode: minor-audit

## Problem / Why

The developer-only live-Outlook integration harness
`LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`
(`TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`) false-fails in Visual
Studio with `COMException (0x80010100) RPC_E_SYS_CALL_FAILED`. The exception is thrown by
`new Outlook.Application()` during Arrange, before any code-under-test runs. The harness is
designed to report `Assert.Inconclusive` (skip) when it has no Outlook to exercise, but its
availability decision is a narrow HRESULT whitelist (`IsOutlookUnavailableHResult`) covering
only three HRESULTs. `0x80010100` is not in that set, so the environment/launch failure falls
to the generic catch, is captured, and fails the test.

The HRESULT whitelist is the wrong seam: any COMException from constructing the Outlook
Application is a pure environment/launch failure that can never represent a hookup defect
(no code-under-test has run yet). Separately, CI claims to exclude the harness via
`/TestCaseFilter:"TestCategory!=LiveOutlook"`, but `.github/workflows/ci.yml` runs
`vstest.console.exe` without that filter, so the LiveOutlook category is not actually gated.

## Proposed Behavior

- Scope the "unavailable -> Inconclusive" decision to the `new Outlook.Application()`
  construction call: a COMException there reports Inconclusive regardless of HRESULT.
- Preserve strict failure semantics for the exercise phase (gate construction,
  `coordinator.Tick()`, hookup callback): exceptions there still fail the test.
- Extract the classification into a small, unit-testable seam.
- Add the `/TestCaseFilter:"TestCategory!=LiveOutlook"` gate to CI and the local QC scripts
  that mirror the CI vstest invocation.

## Acceptance Criteria

- [x] AC1 — A construction-phase COMException (any HRESULT, specifically including 0x80010100 RPC_E_SYS_CALL_FAILED) makes the harness report Inconclusive, not fail.
- [x] AC2 — Exercise-phase exceptions (OutlookReadinessGate construction, `coordinator.Tick()`, or the hookup callback), including COMExceptions, still surface as a captured failure.
- [x] AC3 — The Outlook-availability classification is extracted into a small, unit-testable seam (out of the anonymous thread body).
- [x] AC4 — New deterministic regression test(s) added in the standard (non-LiveOutlook) suite, hitting no live Outlook and using no temporary files; coverage does not regress on changed lines.
- [x] AC5 — `.github/workflows/ci.yml` and the mirrored local QC commands apply `/TestCaseFilter:"TestCategory!=LiveOutlook"` to the vstest invocation.
- [x] AC6 — XML doc comments in `LiveOutlookHookupIntegrationTests.cs` updated to match the new skip behavior and the now-accurate CI filter claim.
- [x] AC7 — Full four-step C# toolchain (csharpier → analyzers → nullable → vstest) passes in a single final pass; the PowerShell toolchain (format → analyze → Pester) passes for the changed QC scripts and tests.

## Constraints & Risks

- Minimal, targeted defect fix; no opportunistic refactors. Do not weaken the existing
  timing/completion assertions in the live path.
- MSTest + Moq + FluentAssertions; nullable stays enabled with no new warnings.
- net48 target: no `init`/`record`/`record struct` (CS0518). No temporary files in tests.

## Test Conditions to Consider

- [ ] Construction-phase COMException with HRESULT 0x80010100 -> skip signal.
- [ ] Construction-phase COMException with an arbitrary other HRESULT -> skip signal.
- [ ] Exercise-phase exception (including COMException) -> captured failure.
- [ ] Success path -> neither skip nor capture.
- [ ] Local QC arg builders and CI include the LiveOutlook exclusion filter.

## Next Step

- [ ] Promote to GitHub issue (bug template)
- [ ] Create `docs/features/active/liveoutlook-harness-construction-scoped-skip/` folder from the template
