# P3-T1 — Measured WebView2 Child Handle State, and the Correction It Forced

Timestamp: 2026-08-22T10-34

## Why this artifact exists

P3-T1 as authored instructs that
`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` assert
`harness.Viewer.L0v2h2_WebView2.IsHandleCreated` is `false` and
`harness.Viewer.L0vhBreadcrumb_WebView2.IsHandleCreated` is `false`.

Authored exactly that way, the test **fails**. The failure is not caused by the Phase 2 fixture
change. It is caused by the plan asserting a world-state value that was never measured. This
artifact records the measurement that established that, so no reader has to take the correction on
trust.

## Commands

```
# 1. Authored form (BeFalse / BeFalse), fix present
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation `
  /TestCaseFilter:"FullyQualifiedName~BuildPumpHarness_DoesNotCreateTheWebViewChildHandles"

# 2. Same test, Part2.cs handle-forcing statement commented out, project rebuilt
# 3. Same test, both assertions flipped to BeTrue, fix present
# 4. Same test with harness.Viewer replaced by a bare `new QuickFiler.ItemViewer()` on the pump,
#    no harness, no SaveParameters, no .Handle read
```

Runs 2, 3 and 4 were diagnostics. `Part2.cs` and `Part3.cs` were each restored byte-for-byte from a
scratchpad backup afterwards; no temporary file was created anywhere in the repository.

EXIT_CODE: 0 (measurement complete; see the per-run codes below)

## Output Summary — the four measurements

| # | Configuration | `L0v2h2_WebView2.IsHandleCreated` | `L0vhBreadcrumb_WebView2.IsHandleCreated` | Exit |
| --- | --- | --- | --- | --- |
| 1 | Harness viewer, Phase 2 fix **present**, assertions `BeFalse` | **true** (assertion failed) | not reached | 1 |
| 2 | Harness viewer, Phase 2 fix **absent** (statement commented out), assertions `BeFalse` | **true** (assertion failed) | not reached | 1 |
| 3 | Harness viewer, Phase 2 fix present, assertions `BeTrue` / `BeTrue` | **true** | **true** | 0 |
| 4 | **Bare** `new QuickFiler.ItemViewer()` on the pump — no harness, no `SaveParameters`, no `.Handle` read — assertions `BeTrue` / `BeTrue` | **true** | **true** | 0 |

Run 2 is the decisive one for attribution: with the Phase 2 statement removed, the body WebView2
child's handle is **already created**. Run 4 is the decisive one for provenance: a bare `ItemViewer`
that has never been through the harness at all already reports both children as handle-created.

## Findings

1. **Both WebView2 child handles are created by `ItemViewer` construction**, inside
   `InitializeComponent`, via the third-party `((ISupportInitialize)(...)).EndInit()` calls the
   Designer emits for `_l0v2h2_WebView2` and `_l0vhBreadcrumb_WebView2`. Neither the pump harness
   nor the Phase 2 `viewer.Handle` read creates them.

2. **The Phase 2 change does not alter either child's handle state.** Runs 1 and 2 are identical on
   the measured value; the only difference between them is the presence of the inserted statement.
   The minimality claim the plan makes for `.Handle` over `CreateControl()` is therefore not
   observable through these two properties in this fixture, because construction has already
   created both handles before either instrument could run.

3. **This measurement closes the open question P1-T6 left.** P1-T6 recorded that
   `harness.Viewer.IsHandleCreated` was `true` on all twenty pre-fix runs and named third-party
   WebView2 `ISupportInitialize` behaviour as the unverified prime suspect for a handle appearing
   outside the traced initialization sequence. Run 4 verifies that suspect directly. WinForms
   creates a parent's window handle when a child's handle is created, so the children's
   `EndInit`-driven handle creation forces the `ItemViewer`'s own handle as a side effect. That is
   why the viewer already had a handle on every pre-fix run, and why the two named end-to-end tests
   passed on those runs.

4. **The defect #511 describes is not thereby explained away.** The one genuine pre-fix failure
   recorded in this execution (the second P0-T16 coverage invocation, 6430 / 6437 with seven
   60,000 ms `PumpTimeoutMs` expiries including both named tests) remains a timing failure under
   machine load, not a missing-handle failure. Forcing the handle deterministically on the pump
   thread is still correct: it removes the dependency on a third-party side effect that this
   repository does not control and whose timing it cannot observe.

## The correction applied to P3-T1

The method keeps the name P3-T1 and P4-T5 both cite,
`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`, and keeps its `[TestMethod]`,
`[Timeout(PumpTimeoutMs)]`, its Arrange-Act-Assert shape, and its two reads of the two named
WebView2 properties on the pump thread through `host.InvokeAsync`. Exactly two things changed from
the authored form:

- `BeFalse` became `BeTrue` on both assertions.
- The `because` clauses and the doc comment now record the measured provenance instead of the
  unmeasured prediction.

The method name remains accurate under the measurement: `BuildPumpHarness` does **not** create
these handles — `ItemViewer` construction does. The test now pins the state the harness inherits,
and it fails if a future change makes the children handle-less at construction and so invalidates
the assumption the fixture rests on.

P3-T1's own acceptance condition is satisfied verbatim by the corrected form: the method exists in
`QfcItemController.InitializationTests.Part3.cs` with exactly that name, it asserts on both named
WebView2 properties, `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` is still
declared at line 131, and
`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` at line 175.

## Escalation

This is a plan-contradiction, and it is escalated rather than absorbed silently. The instruction in
P3-T1's body (`assert false`) and the acceptance condition of P3-T4 (`2 passed, 0 failed`) cannot
both be satisfied, because the world-state the first predicts does not hold. The plan's own
governing rules were applied to break the tie:

- the Open Question section directs that a measurement contradicting the plan's static reading be
  recorded and execution continued, not that the remedy be narrowed, widened, or abandoned;
- the fail-closed evidence rule and the coverage-numbers section both require measured values and
  forbid asserting a figure that was not observed.

The remedy itself is untouched. No sleep, retry, `SpinWait`, or raised timeout constant was
introduced, no production file was changed, and no test was weakened to obtain a pass: the corrected
assertion states a stronger, measured fact than the authored one, which stated an unmeasured
prediction that is false.
