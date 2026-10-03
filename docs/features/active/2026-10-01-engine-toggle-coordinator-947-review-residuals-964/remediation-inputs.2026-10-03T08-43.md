# Remediation Inputs - Issue 964 (cycle 1)

Timestamp: 2026-10-03T08-43
Issue: 964
Branch: bug/engine-toggle-coordinator-947-review-residuals-964
Opened by: orchestrator, under the maintainer related-defect remediation directive
Source review: code-review.2026-10-03T08-50.md (verdict PASS, 0 blocking findings)
Base for this cycle: HEAD 96d2975d4 (review artifacts committed)

## Why this cycle exists

The reduced audit returned PASS with no blocking findings. The maintainer directive for this run states that a defect related to the item's work (same files, same component, same root cause or a sibling call site, including comment drift and test-quality nits in touched files) is remediated inside the item through the normal plan, test and review flow. Two findings in the source review meet that definition. Both are classified `autonomous`. The remaining findings (CR-2, CR-3) record a "No change" recommendation and the observations (O-1 to O-5) are informational or already tracked; they are out of scope for this cycle.

## Findings

### R-1 (from CR-1): untested null-or-empty engine-name arm

- Severity: Non-blocking (Minor); remediated under the related-defect directive
- Remediability: autonomous
- File: `TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs`, line 20 (`RenderEngineName`), reached from `BuildUnavailableMessage` and `BuildUnmappedKeyMessage`.
- Defect: the true arm of `string.IsNullOrEmpty(engineName) ? NullEngineNameToken : engineName` is executed by no test. The final Cobertura class node for `EngineToggleStateCoordinator.Messages.cs` reads branch-rate 0.5 (the type aggregate is 43/44 branches).
- Required outcome: a deterministic MSTest test in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (not the primary fixture, which is at 481/500 lines) covering both a null and an empty engine name on the refusal path (engines accessor returns null): `HandleToggleClickAsync` does not throw; exactly one notification is delivered and it contains the `NullEngineNameToken` rendering `(null)`; no error is logged; no engine member is invoked; no control is invalidated. A `[DataRow(null)]` / `[DataRow("")]` pair is acceptable.
- Invariant to hold: the Messages partial's class node reaches branch-rate 1 in the final coverage document; no production file changes (this is a test-only remediation).

### R-2 (from CR-4): asymmetric assertions in the both-sinks-throw test

- Severity: Non-blocking (Informational); remediated under the related-defect directive as a test-quality nit in a touched file
- Remediability: autonomous
- File: `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`, lines 88-112 (`HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow`).
- Defect: unlike its sibling test (lines 52-79), it does not assert `Engines.VerifyNoOtherCalls()` or that `Invalidations` is empty.
- Required outcome: add both assertions with FluentAssertions or Moq verification as the sibling does; the test continues to pass.

## Constraints

- Test-only change set: `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` only; no production file, csproj or other test file changes. The SinkGuard partial must stay at or below 500 lines.
- MSTest, Moq and FluentAssertions; no temporary files; no `Thread.Sleep` / `Task.Delay`; deterministic.
- Full CLAUDE.md C# toolchain in order (csharpier format and check, analyzer `/t:Rebuild`, `TreatWarningsAsErrors` `/t:Rebuild`, `Invoke-MSTestWithCoverage.ps1`) with numeric coverage evidence, restart from step 1 on any failure or file change.
- Evidence under `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/<kind>/` only.
- Do not weaken or edit any acceptance criterion in `issue.md`.
