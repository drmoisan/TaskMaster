# [P6-T1] Per-file line coverage of `UtilitiesCS/Threading/UiThread.cs` against the Phase 0 baseline

Timestamp: 2026-09-08T02-58

Command: the pinned per-file lookup applied to `coverage\809-p5-final.cobertura.xml`, the document [P5-T5] wrote. The lookup selects `<class>` elements whose `filename` attribute ends with the backslash suffix `UtilitiesCS\Threading\UiThread.cs`, takes `.//line` under each, counts each line number once, and treats a line number as covered when any matching element carries `hits` greater than zero. A line number that matches no element is not executable and is excluded from both numerator and denominator.

EXIT_CODE: 0

## Figures

FINAL_UITHREAD_LINES_COVERED: 121
FINAL_UITHREAD_LINES_VALID: 126
FINAL_UITHREAD_LINE_PCT: 96.03
FINAL_UITHREAD_UNCOVERED_LINES: 38,39,40,177,178

The baseline value, quoted from `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/baseline/p0-t14-uithread-file-coverage.md`:

BASELINE_UITHREAD_LINE_PCT: 76.83

| Quantity | Baseline | Final |
|---|---|---|
| Lines covered | 63 | 121 |
| Lines valid | 82 | 126 |
| Line % | 76.83 | 96.03 |
| Uncovered line count | 19 | 5 |

## Both gate conditions

1. `FINAL_UITHREAD_LINE_PCT` is `96.03`, which is at least `80.00`. That floor is the `CLAUDE.md` figure, which takes precedence over the `.claude/rules/general-unit-test.md` figure per the precedence order in `.claude/skills/policy-compliance-order/SKILL.md`. **Met**, with 16.03 percentage points of margin.
2. `FINAL_UITHREAD_LINE_PCT` `96.03` is strictly greater than `BASELINE_UITHREAD_LINE_PCT` `76.83`. **Met**, an increase of 19.20 percentage points.

## Each baseline-uncovered line number, evaluated against the final document

The file grew from 195 to 293 physical lines, so a baseline line **number** does not address the same source construct in the final document. The table below is the literal per-number lookup the task asks for; the construct-level reconciliation that follows it is what carries the meaning.

| Baseline uncovered line number | Status in the final document | Text now at that number |
|---|---|---|
| 28 | not executable | `// call regardless of the latch, so a non-STA caller would otherwise poison them even` |
| 29 | not executable | `// when Initialize() never runs.` |
| 30 | covered | `ApartmentState apartment = Thread.CurrentThread.GetApartmentState();` |
| 32 | covered | `{` |
| 33 | covered | `throw new InvalidOperationException(NonStaInitMessage(apartment));` |
| 34 | not executable | `}` |
| 67 | not executable | `private static bool _initialized;` |
| 68 | not executable | (blank) |
| 69 | not executable | `private static void Initialize()` |
| 70 | covered | `{` |
| 71 | not executable | `// Create a hidden form to initialize the synchronization context` |
| 72 | covered | `_syncContextForm = SyncContextFormFactory();` |
| 73 | covered | `_syncContextForm.ShowInTaskbar = false;` |
| 74 | covered | `_syncContextForm.WindowState = FormWindowState.Minimized;` |
| 75 | covered | `_syncContextForm.Show();` |
| 76 | not executable | (blank) |
| 118 | not executable | `/// drives initialization through a failure would otherwise change the premise of every` |
| 119 | not executable | `/// later test in that process. This method is not thread-safe; serialization is provided` |
| 120 | not executable | `/// by <c>[DoNotParallelize]</c> on every consuming test class.` |

No baseline line number is uncovered in the final document.

## Construct-level reconciliation, with the covering test named

| Baseline uncovered construct | Baseline lines | Final lines | Covered now | Test that covers it |
|---|---|---|---|---|
| `if (onLockupDetected is not null) { _onLockupDetected = onLockupDetected; }` body | 28, 29, 30 | 38, 39, 40 | **No** | none; see below |
| `if (timeProvider is not null) { _monitorTimeProvider = timeProvider; }` body | 32, 33, 34 | 42, 43, 44 | Yes | `UiThreadInitRetryContract_Tests.Init_WithMonitorUiThreadEnabled_ConstructsAndRunsTheThreadMonitorWithTheInjectedTimeProvider`, which passes `timeProvider: clock` |
| The `if (_monitorUiThread)` `ThreadMonitor` construction and `Run()` | 67 through 76 | 87 through 97 | Yes | the same test, which passes `monitorUiThread: true` and asserts `ThreadMonitorField` is non-null |
| The lazy `if (_uiSyncContext is null) { Init(); }` block | 118, 119, 120 | 207 through 210 | Yes | `UiThreadInitRetryContract_Tests.UiSyncContext_ReadWithNullBackingFieldFromStaThread_InitializesThroughTheLazyPath` |

Seventeen of the nineteen baseline-uncovered source lines are covered by this delivery. The residual three, the body of the `onLockupDetected` guard now at lines 38 through 40, remain uncovered because no test in this delivery passes a non-null `onLockupDetected` argument on a path that reaches the assignment: the one test that supplies a callback, `Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged`, supplies it precisely to assert that a rejected `Init()` does **not** perform that assignment, so the AC1 precondition throws before line 37 is evaluated.

## The two remaining uncovered lines

| Line | Text | Reason |
|---|---|---|
| 38 | `{` | Body of the `onLockupDetected` guard, as above. |
| 39 | `_onLockupDetected = onLockupDetected;` | Same. |
| 40 | `}` | Same. |
| 177 | `{` | Body of the `ReferenceEquals(_context, _uiSyncContext)` clause of the new predicate, at line 176. |
| 178 | `return true;` | Same. |

The predicate clause at 176 through 178 is reached only when the caller stands on the owning UI thread with a non-null ambient context that is not the captured context, and the captured context is the persistent `_uiSyncContext`. The AC3 case that installs `_uiThreadId` for the host thread, `IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue`, captures a `DispatcherSynchronizationContext` and so falls through to the dispatcher clause instead; no case in this delivery installs `_uiSyncContext` and then awaits that same instance from the owning thread while a different context is ambient. This is a two-line gap in a five-line residual and is recorded rather than closed, because closing it is a further test rather than a defect in the fix. It is carried to [P6-T13].

The five uncovered lines are 3.97% of the 126 executable lines, against a floor that permits 20%.
