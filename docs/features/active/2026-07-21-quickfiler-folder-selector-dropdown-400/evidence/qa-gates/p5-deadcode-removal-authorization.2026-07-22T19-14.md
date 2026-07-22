# P5-T202 — Unreachable-dead-code removal authorization inventory (read-only)

Timestamp: 2026-07-22T19-14Z

Command: `for f in QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs coverage.config scripts/vscode/TaskMaster.cli.runsettings; do sha256sum "$f"; wc -l "$f"; done; grep -n "BreadcrumbDropDownOpenLifetime\|BreadcrumbPopupBoundaryCoverageTests" QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj`

EXIT_CODE: 0

## Scope statement

This task is read-only. No production, test, project, configuration, or evidence-source file was
changed. It gates the single production correction (P5-T203) that closes the ninth below-threshold
unit and executes only after the P5-T201 170/170 composition
(`p5-branch-coverage-composition.2026-07-22T18-58.md`, EXIT_CODE 0) was recorded passing.

## Three-step unreachability proof for `<CompleteOpenAsync>d__16` inner recovery `catch` (lines 153-156)

Citing `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` and the P5-T201 Cobertura
(`coverage-p5-branch-coverage-correction.2026-07-22T18-58.cobertura.xml`) only.

(a) **CompleteOpenAsync lines 147-157.** The outer `catch (Exception exception)` wraps
`await HandleOpenFailureAsync(exception, lease).ConfigureAwait(false)` in an inner `try` whose
`catch (Exception recoveryFailure)` at lines 153-156 calls `_uiOperations.Report(recoveryFailure)`.
The caught `exception` is a non-null caught exception.

(b) **HandleOpenFailureAsync lines 335-359.** The method's only substantive work
(`await _uiOperations.RunAsync(...).ConfigureAwait(false)`, lines 342-353) is entirely inside a
`try` whose `catch (Exception rollbackFailure)` at lines 355-358 routes every rollback failure to
`_uiOperations.Report(rollbackFailure)` at line 357. There is no throwing path outside that
`try`/`catch`, so the returned `Task` can only complete successfully — the method cannot throw.

(c) **BreadcrumbUiDispatcher.Report lines 238-253.** `Report(Exception exception)` throws
`ArgumentNullException` only when `exception == null` (lines 240-243). That branch is impossible here
because the argument is a caught non-null `Exception` (`rollbackFailure` in (b), or `recoveryFailure`
in (a)). Otherwise it wraps the sink call `_errorSink(exception)` in `try`/`catch (Exception
sinkException)` and logs (lines 245-252), so it cannot throw.

**Conclusion.** Since `Report` cannot throw for a non-null argument (c), `HandleOpenFailureAsync`
cannot throw (b), so `await HandleOpenFailureAsync(...)` in the inner `try` (a) can never throw, and
the inner `catch (Exception recoveryFailure)` at lines 153-156 is provably unreachable dead code
introduced on this branch.

## Coverage baseline for the unit

- `QuickFiler.Viewers.BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` is
  `line-rate="0.8571428571428571"` = **24/28 = 85.71%**, branch-rate 1, in the P5-T201 Cobertura,
  with lines 153-156 uncovered per the P5-T185 baseline and the
  `p5-authoritative-focused-coverage-decision.2026-07-22T16-29.md` shortfall inventory (unit 7,
  24/28, lines 153-156).
- Removing those four lines shrinks the denominator to **24/24 = 100%** with no new test case required
  for that unit.

## Anti-masking distinction (per Fixed-execution-rules anti-masking clause)

Removing genuinely-unreachable dead production code to legitimately raise coverage is expressly NOT
one of the anti-masking-prohibited behaviors and is distinguished from them. The removal changes no
assertion, adds no sleep/delay/wall-clock wait/retry loop/timing threshold, adds no
`[DoNotParallelize]`/`[Ignore]`/category skip, narrows no filter, and adds/changes no coverage or test
exclusion, threshold, or `coverage.config` value. Adding a coverage exclusion, documenting a 24/28
carve-out, or making `Report`/`HandleOpenFailureAsync` rethrow are all rejected in favor of removal,
per `.claude/rules/general-unit-test.md` (no production file may be excluded from coverage; untestable
lines are refactored out) and the simplicity-first design principle.

## Pre-edit baselines (SHA-256 + physical line counts)

| File | SHA-256 | `wc -l` |
|---|---|---:|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `e53de9be76cb7ac3f69b43c12088a7b4b6da6f3f2455dcf7c6c10f5a010c53f1` | 437 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | `594d96f2a8f34e6e987d2ad7efeda6fce999152027924d83a15fc22b7f3e63db` | 480 |

The test-file SHA is byte-identical to the P5-T201 gated state recorded in
`p5-branch-coverage-composition.2026-07-22T18-58.md` (`594d96f2…`), confirming no drift.

## Protected baselines (must be hash-identical after the correction)

| Protected artifact | SHA-256 / value |
|---|---|
| `coverage.config` | `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` |
| `scripts/vscode/TaskMaster.cli.runsettings` | `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` |
| `QuickFiler.csproj` OpenLifetime include | line 393 `<Compile Include="Viewers\BreadcrumbDropDownOpenLifetime.cs" />` |
| `QuickFiler.Test.csproj` PopupBoundary includes | line 81 `...BreadcrumbPopupBoundaryCoverageTests.cs`; line 82 `...BreadcrumbPopupBoundaryCoverageTests.Part2.cs` |

### Protected 17-class filter string (byte-identical to P5-T171/P5-T183/P5-T201)

```
FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests
```

## Eight already-closed units (raised to >=90% by tests only in P5-T188/P5-T195)

The nine P5-T185 units minus `<CompleteOpenAsync>d__16`: the eight coordinator/lifetime/host units
closed by the ten test cases added in P5-T188 and P5-T195, per
`p5-branch-coverage-nine-unit-closure.2026-07-22T18-58.md`. `<CompleteOpenAsync>d__16` is the ninth and
sole remaining unit at 24/28, closed by the P5-T203 dead-code removal.

## Seven never-regress passing units (protected pre-correction baseline)

- Dispatcher 144/144
- NavigationReadiness 96/96
- Factory 16/16
- host-neutral Popup operations at least 75/76
- Hub 155/155
- Attachment 80/80
- Release 16/16

## Protected 170-case composition

`70+13+12+5+15+23+12+10+10` = 170, per the P5-T201 composition
(`p5-branch-coverage-composition.2026-07-22T18-58.md`): 17 classes, 170 passed, 0 failed, 0 skipped.

## Output Summary

Read-only authorization inventory recorded with EXIT_CODE 0. The three-step unreachability proof
holds: `Report` cannot throw for a non-null argument, so `HandleOpenFailureAsync` cannot throw, so the
inner `catch` at `BreadcrumbDropDownOpenLifetime.cs` 153-156 is unreachable dead code; `<CompleteOpenAsync>d__16`
is 24/28 = 85.71% and removal shrinks the denominator to 24/24 = 100% with no new test. Pre-edit
baselines (SHA-256 + line counts) and protected baselines (`coverage.config`, runsettings, 17-class
filter, `Compile` includes, seven never-regress units, 170-case composition) are captured. Removal is
distinguished from anti-masking-prohibited behavior. The edit is authorized for P5-T203.
