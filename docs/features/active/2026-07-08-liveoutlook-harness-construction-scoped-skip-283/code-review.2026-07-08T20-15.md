# Code Review — Issue #283 (liveoutlook-harness-construction-scoped-skip)

- Timestamp: 2026-07-08T20-15
- Review type: RE-AUDIT (remediation pass 1)
- Base: `main` @ `930467f4`; Head @ `87d223a0`
- Scope: full branch diff `930467f4..87d223a0`

## Executive Summary

The change is a small, well-structured defect fix. It replaces a narrow HRESULT whitelist with a
phase-scoped classification: any COMException during construction of the Outlook Application is
treated as an environment/launch failure and reported as a skip (Inconclusive), while any exception
during the exercise phase — including a COMException — remains a captured failure. The decision is
extracted into a host-neutral, unit-testable seam (`LiveOutlookHarnessRunner.Run<T>`) using
`Func<T>`/`Action<T>` delegate injection, which is the minimal seam appropriate here and avoids a
live Outlook dependency, mocks, or temporary files.

Code quality is good. The seam is cohesive, fail-fast (null-guards both delegates), and thoroughly
XML-documented with the two-phase rationale and the net481 CS0518 constraint on `HarnessOutcome`.
The 8 new MSTest/FluentAssertions tests cover the positive path, both skip-classification branches,
the non-COM construction capture, both exercise-phase captures (including COMException), and both
null-guard paths. Coverage of the new seam is machine-verified at 100% line coverage. The CI and
local QC arg-builder changes are single-token appends that keep the existing seam structure intact.

No blocking code-quality findings. One optional Low invariant-hardening item is carried forward from
the prior review. This is a re-audit; the three prior policy Blocking findings were remediated and
are verified in the companion policy audit.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunner.cs | `HarnessOutcome` ctor (L55-59) | The struct accepts both `captured` and `skipReason` non-null; mutual exclusivity is a convention enforced only by the `Run` call sites, not by the type. | Optionally add a private factory (`Skip`/`Capture`/`Success`) or a `Debug.Assert` guarding mutual exclusivity. | Encoding the invariant in the type would prevent a future caller from constructing a contradictory outcome. Not gating — the seam never violates it and all 8 tests assert it. | Source L55-71; tests L37-143 assert one-of semantics per case. |
| Info | TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunner.cs | `Run<T>` phase-2 catch (L133-135) | Exercise-phase catch is intentionally broad (`catch (Exception)`) and returns a captured outcome rather than rethrowing. | None — retain. | This is a defined boundary that preserves and surfaces the failure to the test via `Captured`; it does not silently swallow (the exception is carried out). Matches the general error-handling policy's "boundary with added context." | Source L126-136; XML doc L23-29. |
| Info | scripts/vscode/Invoke-MSTest.ps1; scripts/vscode/Invoke-MSTestWithCoverage.ps1 | `Get-VsTestArgumentList` / `Get-DotnetCoverageArgumentList` return arrays | Change is a single appended `/TestCaseFilter:TestCategory!=LiveOutlook` token in each arg-builder return array, mirroring the CI filter. | None — retain. | Keeps the two local QC invocations consistent with the corrected `ci.yml`, satisfying AC5. Covered by the added RunSettings tests. | PS diff verified; `Invoke-MSTest.RunSettings.Tests.ps1` filter assertions. |
| Info | TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs | whole-file refactor (+66/-85) | The LiveOutlook integration test now delegates classification to the seam and its XML docs match the new skip behavior and the accurate CI filter claim. | None — retain. | Reduces the harness to thin wiring and centralizes the decision in the tested seam (AC3, AC6). | Diff `--stat` 151 lines changed; behavior verified by the seam's 8 unit tests. |

## Design and Test Quality Notes

- Separation of concerns: pure two-phase classification is isolated from the COM-bound harness; the
  harness becomes thin wiring. Aligns with General Code Change Policy §1 and §4.
- Test discipline: Arrange–Act–Assert throughout; descriptive method names; FluentAssertions with
  explicit because-reasons; deterministic (no clock, no RNG, no sleep, no network, no temp files).
  Conforms to General Unit Test Policy and C# Unit Test Policy.
- net481 constraint honored: `HarnessOutcome` is a plain `readonly struct` (no `init`/`record`),
  documented in-code with the CS0518 rationale.
- HRESULT surfacing: the skip reason embeds `0x{ErrorCode:X8}`, and the tests assert on the specific
  offending value (`80010100`, `80004005`), which makes the classification observable and the
  regression concrete.
</content>
