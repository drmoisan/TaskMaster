# AC-3 Check-Off (P5-T3)

Timestamp: 2026-08-27T11-53
Task: [P5-T3]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-3 ("A bounded regression test demonstrates the #230 deadlock scenario is
unreachable") is verified on all three of its clauses and is checked off in `spec.md`. `PairsN: 3`,
`PairsNMinus1: 2`, so exactly one further checkbox changed state.

PairsN: 3
PairsNMinus1: 2

`pairs(3) - pairs(2) == 1`. `pairs(2)` is the value recorded by `P5-T2` in
`<FEATURE>/evidence/other/ac-checkoff-ac2.2026-08-27T11-51.md`.

## Cited artifacts, resolved per § Conventions

| Stem | Resolved filename |
| --- | --- |
| `regression-tests-created` | `<FEATURE>/evidence/other/regression-tests-created.2026-08-27T10-40.md` |
| `regression-tests-pass` | `<FEATURE>/evidence/regression-testing/regression-tests-pass.2026-08-27T11-01.md` |

## Verification — three clauses

### Clause 1: the tests exist in the named file

`regression-tests-created` records that
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` exists and lists
all six test names. AC-3's text names R1-R5; this plan delivers a sixth, R6, per § Decisions Record
D1, which discharges the spec's own § Rollout & Follow-up item 4 request that the planner choose
between a sixth test and folding the assertion into R5 and state the choice.

### Clause 2: each carries `[Timeout(GateTimeoutMs)]` with `GateTimeoutMs = 60000`

`regression-tests-created` records the six-attribute count:

```
| `[TestMethod]` match count | exactly 6 | 6 |
| `[Timeout(GateTimeoutMs)]` match count | exactly 6 | 6 |
```

Both counts were re-verified after the `P3-T1` formatter pass and remain 6, recorded in
`<FEATURE>/evidence/qa-gates/csharpier-format.2026-08-27T11-08.md`. The class hosts
`private const int GateTimeoutMs = 60000;`. Six `[TestMethod]` attributes and six
`[Timeout(GateTimeoutMs)]` attributes means every test is bounded, so a regression fails rather than
hangs.

`regression-tests-pass` records all six as passing, with the slowest at 71 ms — far inside the
60 000 ms bound.

### Clause 3: R1 documented as primary deterministic, R4 as supporting probabilistic

This is the clause the other two artifacts do not cover, so it is discharged by the
`PrimaryAssertionDoc:` field of `regression-tests-created`, which is non-empty and opens:

`R1 is the primary deterministic regression assertion and R4 is the supporting probabilistic one.`

The field continues with the reasoning — that R1 reproduces the clobber precondition with no
concurrency and proves the clobber unreachable, that the clobber rather than the scheduling is the
actual #230 mechanism, and that R4 fails only probabilistically under a broken implementation because
no deterministic way exists to prove the second caller is currently blocked without a timed wait.

The sentence is recorded as a quoted field rather than asserted with a line-oriented search because
it is prose CSharpier may rewrap across lines, which would make such a search return zero matches
whatever the executor wrote.

## Result

`- [ ] **AC-3 …` changed to `- [x] **AC-3 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed.
