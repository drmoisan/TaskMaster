# `2026-07-04-coverage-gaps-test-seams-236` - User Story

- Issue: #236
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-04T13-15

## Story Statement

- As a TaskMaster maintainer, I want the remaining QuickFiler coverage gaps in `EfcViewerQueue`, `ItemViewerQueue`, `QfcThemeHelper`, `EfcHomeController`, and `TlpCellStates` closed through narrow COM/WinForms seams or isolated pure logic, so that these components can be verified with deterministic MSTest coverage instead of coverage exemptions.
- As a QuickFiler user, I want these refactors to preserve existing Outlook and QuickFiler behavior, so that filing, viewer creation, theming, and controller initialization continue to work while maintainers improve regression protection.

## Problem / Why

The five issue #236 targets contain logic that is not currently covered because it is coupled to static queues, live viewer construction, WinForms controls, Outlook COM traversal, or controller construction. The repository policy requires coverage to be improved through testable design rather than exemptions. This work should make the logic observable through local seams while preserving production adapters and current caller behavior.

## Personas & Scenarios

- Persona: QuickFiler maintainer
  - Maintains C# QuickFiler controller, helper, queue, and theme code.
  - Needs deterministic tests that run without live Outlook, live COM selection, external services, temporary files, or full QuickFiler windows.
  - Must keep public/static surfaces compatible because existing controllers, queues, ribbon actions, and tests depend on them.
  - Wants narrow local seams that match existing repository patterns such as `IUiDispatcher`, viewer interfaces, internal helpers, and factory delegates.

- Persona: QuickFiler user
  - Uses the Outlook add-in for ordinary filing workflows.
  - Expects queue prebuild, viewer loading, theming, and home-controller startup behavior to remain unchanged.
  - Should not see a new setting, CLI, dialog, theme change, or workflow change from issue #236.

- Scenario: Queue behavior becomes testable without live viewers
  - The maintainer introduces instance-owned queue cores or equivalent internal services behind the existing static queue APIs.
  - Tests inject fake viewer factories and synchronous dispatcher seams to exercise queue creation, dequeue, replacement scheduling, cancellation boundaries, chunk behavior, and reset/disposal behavior.
  - Production callers continue to use the existing static methods, which construct real viewers through the production dispatcher path.

- Scenario: Theme construction becomes testable without full forms
  - The maintainer keeps `QfcThemeHelper` production overloads but adds a pure internal input model or overload for controls and delegates.
  - Tests use handleless controls or adapters to verify theme keys, representative colors, control groups, `SetupFormThemes`, and `SetTheme` behavior.
  - Production theme visuals and caller contracts remain unchanged.

- Scenario: Controller construction becomes testable without Outlook COM
  - The maintainer keeps `EfcHomeController` public construction and factory methods as production adapters.
  - Tests inject seams for Outlook selection traversal, data model creation, viewer dequeue, keyboard handler creation, explorer-controller creation, and form-controller creation.
  - Controller decision logic is covered without dereferencing live Outlook selection objects or constructing live viewers.

- Scenario: State conversion receives direct coverage
  - The maintainer adds direct tests for `TlpCellStates` constructors, raw-list conversion, duplicate keys, `TryAddState`, empty inputs, and null-input behavior.
  - If null behavior changes, the implementation fails fast with a specific exception and the tests document the contract.

## Acceptance Criteria

- [x] AC1 - `EfcViewerQueue` queue creation, cached dequeue, empty dequeue, replacement scheduling, cancellation boundaries, and disposal/reset behavior are covered by deterministic MSTest tests through a narrow factory/dispatcher seam without constructing a live `EfcViewer` in unit tests.
- [x] AC2 - `ItemViewerQueue` queue creation, synchronous and dispatched build paths, cached dequeue, empty dequeue, chunk dequeue, cancellation boundaries, and disposal/reset behavior are covered by deterministic MSTest tests through a narrow factory/dispatcher seam without constructing a live `ItemViewer` in unit tests.
- [x] AC3 - `QfcThemeHelper` theme construction, theme key selection, representative color/control-group mapping, `SetupFormThemes`, and direct `SetTheme` extension behavior are covered without requiring live QuickFiler form instances beyond test-controlled controls or adapters.
- [x] AC4 - `EfcHomeController` construction and initialization decision logic is covered through explicit seams for Outlook selection traversal, data model creation, viewer dequeue/construction, keyboard handler creation, explorer-controller creation, and form-controller creation.
- [x] AC5 - `TlpCellStates` constructors, raw-list conversion behavior, duplicate-key behavior, `TryAddState` outcomes, empty input handling, and null-input behavior are covered directly. If production behavior changes, null inputs fail fast with a specific exception.
- [x] AC6 - Existing public/static production entry points remain source-compatible for current callers, including queue APIs, `QfcThemeHelper` production overloads, `EfcHomeController` public constructor and factory methods, and `TlpCellStates` constructors.
- [x] AC7 - No coverage exemptions are added for `EfcViewerQueue`, `ItemViewerQueue`, `QfcThemeHelper`, `EfcHomeController`, or `TlpCellStates`, and no coverage configuration is weakened.
- [ ] AC8 - Repository-wide line coverage remains at or above 80%, and changed or newly introduced non-exempt code for issue #236 meets the repository policy target of at least 90% coverage.
- [x] AC9 - All implementation evidence, including baselines, QA gates, regression results, and coverage artifacts, is stored under `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/<kind>/`.
- [x] AC10 - The final C# toolchain passes in order: CSharpier, .NET analyzers, nullable analysis with warnings as errors, and MSTest with code coverage.

## Acceptance Evidence

| AC | Status | Evidence |
| --- | --- | --- |
| AC1 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/queue-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC2 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/queue-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC3 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/theme-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC4 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/efc-home-controller-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC5 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/tlp-cell-states-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC6 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-analyzer-build.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-nullable-build.2026-07-04T13-15.md` |
| AC7 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-no-coverage-exemptions.2026-07-04T13-15.md` |
| AC8 | FAIL | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-thresholds.2026-07-04T13-15.md` reports repository coverage 45.12% against the 80.00% threshold and changed/new-code coverage 71.19% against the 90.00% threshold. |
| AC9 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-evidence-location-audit.2026-07-04T13-15.md` |
| AC10 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-toolchain-loop.2026-07-04T13-15.md` |

## Verification Expectations

- Planning must include baseline coverage capture for the issue #236 targets before implementation and post-change coverage comparison after implementation.
- Unit tests must use MSTest, Moq, and FluentAssertions and must avoid live Outlook, external services, temporary files, and uncontrolled live WinForms windows.
- Tests that use static override seams must restore production defaults and must avoid parallel interference.
- Final validation must run the required C# toolchain in order: `csharpier .`, analyzer build, nullable/warnings-as-errors build, and coverage-enabled MSTest.
- Coverage, QA, regression, and baseline artifacts must be written under `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/<kind>/`.

## Non-Goals

- Adding coverage exemptions or weakening coverage policy.
- Changing QuickFiler user-facing behavior, theme visuals, filing behavior, ribbon actions, or workflow settings.
- Introducing a broad dependency-injection container, a new test framework, Microsoft Fakes/Shims, or static interception.
- Replacing existing public/static APIs when additive internal seams or adapters can preserve caller compatibility.
- Adding CLI flags, environment variables, persisted settings, telemetry systems, external services, or data migrations.
