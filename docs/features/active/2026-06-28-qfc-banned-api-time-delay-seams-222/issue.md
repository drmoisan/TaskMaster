# qfc-banned-api-time-delay-seams (Issue #222)

- Date captured: 2026-06-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-banned-api-time-delay-seams/ (Issue #222)

- Issue: #222
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/222
- Last Updated: 2026-06-28
- Work Mode: full-bug

## Problem / Why

Eight pre-existing banned-API usages remain in the Quick Filer controllers. These were
flagged and verified pre-existing on `main` (not introduced) during issue #218
remediation, and logged as a deferred follow-up in PR #221. Banned APIs per repo policy
are `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, and `Task.Delay`.
Their presence makes the affected code non-deterministic and not unit-testable without
wall-clock dependence.

Exact active (non-commented) sites verified on HEAD:

- `Task.Delay`:
  1. `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:43` -> `await Task.Delay(5)`
  2. `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:142` -> `await Task.Delay(200)`
  3. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:214` -> `await Task.Delay(20)`
- `DateTime.Now`:
  4. `QuickFiler/Controllers/QfcHomeController.cs:75` -> `DateTime.Now.ToString("mm:ss.fff")` in a log string
  5. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:20` -> `var now = DateTime.Now`
  6. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:100` -> `curDateText = DateTime.Now.ToString("MM/dd/yyyy")`
  7. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:102` -> `curTimeText = DateTime.Now.ToString("hh:mm")`
  8. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:114` -> `OlEndTime = DateTime.Now`

## Proposed Behavior

Route all eight sites through injectable time/delay seams so the source of current time
and async delay becomes injectable. Production defaults must match today's behavior
exactly (same 5/200/20 ms delays, same timestamp semantics). Either reuse/extend an
existing UtilitiesCS abstraction, add a minimal hand-rolled `IClock` + `IAsyncDelay`
seam, or add `Microsoft.Bcl.TimeProvider` (dependency approval required). Prefer the
simplest behavior-preserving design.

## Acceptance Criteria (early draft)

- [ ] All 8 active banned-API sites in the four target files are removed and replaced with injected seams.
- [ ] No new banned-API usages introduced; RS0030 not suppressed globally and policy files not weakened.
- [ ] Production behavior preserved: delays remain 5/200/20 ms; timestamp formats and semantics unchanged.
- [ ] Seams injected through `QfcHomeController` and `QfcDatamodel` construction paths without breaking public `IQfcDatamodel` / home-controller surfaces.
- [ ] Every touched file remains <= 500 lines.
- [ ] Focused MSTest+Moq+FluentAssertions tests prove time-dependent output uses the injected clock and delayed paths await the injected delay (Moq-verifiable), with no live Outlook COM and no temp files.
- [ ] New/changed code targets >= 90% coverage; coverage on changed lines not reduced; repo-wide floor (>= 80%) maintained.
- [ ] C# toolchain passes in order: csharpier -> analyzer build -> nullable build (TreatWarningsAsErrors) -> vstest with coverage.

## Constraints & Risks

- Solution targets .NET Framework VSTO; `System.TimeProvider` requires the
  `Microsoft.Bcl.TimeProvider` package (dependency approval required if chosen).
- Must not break `IQfcDatamodel` / home-controller public surfaces; use internal seams /
  defaults consistent with the issue #218 injection approach.
- Behavior preservation is mandatory.

## Test Conditions to Consider

- [ ] Injected clock controls timestamp-producing output (mm:ss.fff, MM/dd/yyyy, hh:mm).
- [ ] Delayed paths await the injected delay seam (Moq verify) instead of wall-clock `Task.Delay`.
- [ ] Production default seam yields current behavior.
- [ ] No live Outlook COM, no temp files, deterministic.

## Next Step

- [ ] Promote to GitHub issue (refactor type)
- [ ] Create active feature folder from the template

## Reference

- Originating PR / context: PR #221 (issue #218); follow-up note in `artifacts/pr_body_218.md` "Follow-ups".
- Banned-API sweep evidence: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/banned-api-sweep-cycle2-218.md`
