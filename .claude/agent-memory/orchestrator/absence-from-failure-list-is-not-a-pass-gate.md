---
name: absence-from-failure-list-is-not-a-pass-gate
description: "Test X is absent from the failed-test lines" is satisfied identically by "X passed" and by "X never ran"; pair every absence-based gate with a test-discovery count control
metadata:
  type: feedback
---

An acceptance condition of the form *"name X does not appear among the failed-test lines"* cannot
distinguish a passing test from a test that was never discovered. `vstest.console.exe` at default
verbosity prints failed tests and a run summary; it does **not** print a line per passing test. So a
misspelled method name, a missing `[TestMethod]` attribute, or an assembly that failed to load all
produce a **passing** verdict on every such gate.

**Why it matters here.** On issue #287 six acceptance conditions were written this way, covering
AC3, AC4, AC5, AC12 and half of AC8 and AC9. Every one of them would have been satisfied by a plan
whose new tests silently never ran. Executor preflight caught it; the orchestrator's own mirror
review did not.

**The fix is cheap and the plan usually already has the input.** Capture the baseline run's
`Total tests` value, then assert the post-change run's total equals baseline plus the exact number
of new `[TestMethod]` declarations the plan adds. Derive that number from the plan's own test-name
register and state which register entries are *added* versus *extended* — an extended existing test
adds no declaration and must not be counted. On #287 the register held 14 entries of which 12 were
new (1 through 9, plus 12, 13, 14) and 2 were extensions of existing tests.

Word the control so it also explains the consequence, for example: a smaller total means at least
one new test was not discovered, and every absence-based acceptance condition downstream is then
unsupported.

**Generalization.** This is one instance of the `atomic-plan-contract` rule *prefer a named test
over a phrase search, and assert its pass count*. Where the runner will not give you a per-test pass
line, a discovery-count delta is the nearest falsifiable substitute. Reach for it whenever a gate is
phrased as absence from an error list rather than presence of a success signal — the same reasoning
applies to "no warnings named X" and "no diagnostic mentions Y".

Related: [[preflight-catches-vacuous-gates]],
[[expect-fail-tests-break-substring-scoped-run-gates]],
[[vstest-aggregate-crash-isolate-per-assembly]].
