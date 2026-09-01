# P3-T7 — Post-Change Cobertura Report and Changed-Line Coverage

Timestamp: 2026-09-01T08-27

## Command

```text
dotnet-coverage collect --output coverage\p3-t7.cobertura.xml --output-format cobertura --settings coverage.config -- <resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\p3-t7 /TestCaseFilter:TestCategory!=LiveOutlook
```

EXIT_CODE: 0

Test run under the collector: `Total tests: 4771`, `Passed: 4771`, `Failed: 0` — consistent with
P3-T5. `coverage\p3-t7.cobertura.xml` was produced.

## Mechanically Derived Line Locations

Derived from the **post-change source**, not from any line number recorded in the plan.

| Symbol | Derivation | Value |
| --- | --- | --- |
| **L_FILTER** | line number of the unique line containing `when (e is TaskCanceledException \|\| e is TimeoutException)` | **217** |
| **L_GUARD** | smallest line number greater than L_FILTER whose text matches `^\s*token\.ThrowIfCancellationRequested\(\);\s*$` | **219** |
| **L_CTOR** | the second line number in ascending order whose text matches `^\s*using var timeoutSource = \(\s*$` | **199** |

The filter literal matched on exactly **1** line, so L_FILTER is unambiguous. The open-paren
constructor form matched on lines **52 and 199**; 52 is the pre-existing `Func<TResult>` sibling seam
and 199 is the construction this change introduced, which is why the rule takes the second match.

Source text at each derived location:

```text
line 217 :             catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)
line 219 :                 token.ThrowIfCancellationRequested();
line 199 :             using var timeoutSource = (
line 200 :                 timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))
line 201 :             )(milliseconds);
```

The closed range L_CTOR through L_CTOR + 2 is lines 199-201, which is exactly the three-line
replaced timeout-source construction.

## Lookup Rule Applied

`RunWithTimeout<T1, TResult>` is `async`, so the compiler emits nested state-machine types and the
report carries more than one `<class>` element with the same `filename` and overlapping
`<line number=...>` entries whose `hits` differ. Every `<line>` element whose `number` equals the
target was read, across all `<class>` elements whose `filename` ends with `TimeOutTask.cs`. The
recorded hit count is the maximum over those elements.

**Matching `<class>` element count: 29** (baseline P0-T13 had 28; the additional element corresponds
to the new lambda/closure introduced by the seam construction).

**Distinct Cobertura `filename` values matched: exactly one.**

```text
<worktree-root>\UtilitiesCS\Threading\TimeOutTask.cs
```

(The report stores an absolute path; the machine- and account-specific prefix is written as
`<worktree-root>` so this artifact carries no host path. The identifying segment
`UtilitiesCS\Threading\TimeOutTask.cs` is verbatim.)

## Recorded Hit Counts

| Line | Role | Element count | `hits` of each element | **Recorded (max)** |
| --- | --- | --- | --- | --- |
| **219** (L_GUARD) | first statement in the widened clause body | 2 | 1, 1 | **1** |
| **199** (L_CTOR) | `using var timeoutSource = (` | 2 | 1, 1 | **1** |
| **200** (L_CTOR + 1) | `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` | 2 | 1, 1 | **1** |
| **201** (L_CTOR + 2) | `)(milliseconds);` | 2 | 1, 1 | **1** |
| **217** (L_FILTER) | the widened catch clause itself | 2 | 1, 1 | **1** |

### L_FILTER presence statement

**A `<line>` element does exist at L_FILTER (line 217), and its recorded hit count is 1**, derived
from 2 matching elements with `hits` values 1 and 1.

L_GUARD is used as the coverage proxy for the filter clause because a `when` filter expression may
emit no `<line>` element of its own; the clause body executing proves the filter matched. In this
report the filter line happens to carry its own element as well, so both the proxy and the direct
reading agree.

## Comparison Against the P0-T13 Baseline

The two baseline lines were renumbered by the change (pre-change 189 and 202 became post-change
199-201 and 219 respectively), so they are compared by role rather than by number.

| Role | Baseline (P0-T13) | Post-change (P3-T7) | Movement |
| --- | --- | --- | --- |
| Timeout-source construction | line 189, recorded hits **1** | lines 199-201, recorded hits **1, 1, 1** | covered to covered |
| First statement in the retry clause body | line 202, recorded hits **1** | line 219, recorded hits **1** | covered to covered |

**No changed line moved from covered to uncovered.** Both baseline figures were 1 and both
post-change figures are 1 or greater.

## Repository-Wide Figure (reported only)

| Report | Root `line-rate` | Percentage |
| --- | --- | --- |
| P0-T10 baseline | 0.7082975641163215 | 70.83% |
| **P3-T7 post-change** | **0.7084082070537749** | **70.84%** |

Coverage rose slightly. **No repository-wide coverage percentage threshold is asserted by this plan;**
the percentage is recorded as a reported figure only.

Output Summary: L_FILTER = 217, L_GUARD = 219, L_CTOR = 199. The recorded maximum hit count at
L_GUARD is 1, which is greater than 0, proving the widened clause body executed. All three lines in
the closed range 199-201 have a recorded maximum hit count of 1, so at least one is greater than 0,
proving the replaced timeout-source construction executed. A `<line>` element exists at L_FILTER with
1 hit. Compared against the P0-T13 baselines of 1 and 1, no changed line moved from covered to
uncovered. Root line-rate 70.84% post-change against 70.83% baseline.

Acceptance: met. The recorded maximum hit count at L_GUARD (1) is greater than 0; at least one line
in the closed range L_CTOR through L_CTOR + 2 has a recorded maximum hit count greater than 0 (all
three are 1); the artifact carries the L_FILTER presence statement; and both post-change values are
recorded alongside the two P0-T13 baseline values, showing no changed line moved from covered to
uncovered.
