# P3-T8 — Post-Change Source Census on `UtilitiesCS/Threading/TimeOutTask.cs`

Timestamp: 2026-09-01T08-28

Command: the same PowerShell census used by P0-T12 — `Get-Content -LiteralPath <path>` for line
counts, `-match` against the stated anchored regular expressions for anchored counts, and ordinal
`String.IndexOf` scanning for simple-match (occurrence) counts. Run after the final format pass.

EXIT_CODE: 0

## Census, Post-Change Beside Baseline

| Measurement | P0-T12 baseline | **Post-change** | Required by P3-T8 | Met |
| --- | --- | --- | --- | --- |
| Anchored `^\s*catch \(TaskCanceledException\)\s*$` | 9 | **9** | 9 | yes |
| Anchored `^\s*catch \(TimeoutException\)\s*$` | 4 | **3** | 3 | yes |
| Anchored `^\s*catch \(System\.Exception e\)\s*$` | 10 | **10** | 10 | yes |
| Simple-match `when (e is TaskCanceledException \|\| e is TimeoutException)` | 0 | **1** | 1 | yes |
| Simple-match `OperationCanceledException` | 0 | **0** | 0 | yes |
| Anchored `^\s*using var timeoutSource = new CancellationTokenSource\(milliseconds\);\s*$` | 9 | **8** | 8 | yes |
| Simple-match `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` | 2 | **4** | 4 | yes |
| Simple-match `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` | 1 | **2** | 2 | yes |
| Simple-match bare token `timeoutSourceFactory` | 5 | **10** | 10 | yes |
| Total line count | 993 | 1011 | (not asserted here; see P3-T10) | — |

**All ten assertions are met.**

## Reading of the Deltas

- **`catch (TimeoutException)` fell 4 to 3, and exactly one filtered clause appeared.** Precisely one
  clause was converted. The three surviving unfiltered `TimeoutException` clauses are the ones the
  spec's Non-Goals place out of scope: the clause in the implementation at former line 272 and the
  two dead clauses in the `TimeoutAfter` wrappers at former lines 818 and 914. None was touched.
- **`catch (TaskCanceledException)` is unchanged at 9 and `catch (System.Exception e)` is unchanged
  at 10.** No sibling clause anywhere in the file was edited. This is the census criterion the spec
  names as the detector for scope creep into the four out-of-scope defects that live in the same
  file. Note that the new filtered clause does not inflate the `catch (System.Exception e)` count,
  because the anchored pattern requires the line to end after the closing parenthesis and the
  filtered clause continues with ` when (...)`.
- **`OperationCanceledException` remains 0.** The handler was widened to `TaskCanceledException`
  only, not to the broader `OperationCanceledException`. Widening to the broader type would have
  diverged from every sibling and routed unrelated caller-thrown cancellations into the retry ladder.
- **The anchored one-line constructor statement fell 9 to 8**, exactly the one occurrence replaced by
  the three-line seam construction. It did not fall further, so no sibling overload's timeout
  construction was disturbed.
- **The seam literals rose 2 to 4, 1 to 2, and 5 to 10.** The parameter declaration was added to both
  the public wrapper and the private implementation (+2); the coalesce construction was added once
  (+1); and the bare token gained 5 occurrences — the two new parameter declarations, the one new
  coalesce occurrence, the wrapper's forwarding argument, and the recursion's forwarding argument.

This artifact is the evidence cited by the AC5, AC6, and AC7 check-offs at P4-T5, P4-T6, and P4-T7.

Output Summary: The post-change census returns 9 / 3 / 10 / 1 / 0 / 8 / 4 / 2 / 10 for the nine
required measurements plus the filter-clause count. Every value equals the figure P3-T8 requires.
Exactly one catch clause was converted, no sibling clause was edited, the handler was not widened to
`OperationCanceledException`, and the determinism seam is present on both the wrapper and the
implementation and is forwarded through the retry recursion.

Acceptance: met on all ten assertions.
