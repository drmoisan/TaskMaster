# Upstream 825 precondition halt-gate (issue #826, [P0-T4])

Timestamp: 2026-09-09T19-02

Command: five `@(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch <token>).Count` invocations
against `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, plus two location-listing
`Select-String` invocations for the first two tokens, run as one `pwsh -NoProfile -Command` block with
the plan's C2 preamble branch guard active.

EXIT_CODE: 0

## Counts

| # | `-SimpleMatch` token | Count | Required | Holds |
|---|---|---|---|---|
| 1 | `Console.WriteLine` | 2 | 2 | yes |
| 2 | `Task timed out on try` | 2 | 2 | yes |
| 3 | `timeoutSourceFactory` | 4 | at least 1 | yes |
| 4 | `catch (TaskCanceledException)` | 1 | 1 | yes |
| 5 | `catch (TimeoutException)` | 1 | 1 | yes |

## Line numbers reported for the first two tokens

Both tokens report the same two lines, because the two statements are the same statement text:

- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:96` —
  `Console.WriteLine($"Task timed out on try {counter}");`
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:115` — identical text.

These are the post-825 positions. The pre-825 observations recorded in `issue.md` (78, 96) and in
`spec.md` correction 2 (79, 97) are both superseded, which is why plan decision D1 forbids anchoring any
acceptance condition on a line number in this file. Every item-2 edit is located by literal statement
text and enclosing catch clause.

Output Summary: all five precondition counts hold. The two `Console.WriteLine` statements this feature
owns survived feature 825 byte-identical, and the `timeoutSourceFactory` seam parameter still exists.
No `HALT: 825 RECONCILE` condition is present; execution proceeds.

Note on count 4: the value 1 is the exact-token count for `catch (TaskCanceledException)`. Verified
directly against the file in this task, the three `catch (TaskCanceledException` clauses it contains sit
at lines 88, 228 and 304, and only line 88 carries the parameterless spelling. The two clauses at 228
and 304 are spelled `catch (TaskCanceledException e)` and correctly do not match this `-SimpleMatch`
token; the plan's required value of 1 is therefore the value that discriminates the parameterless clause
this feature's first edit sits inside.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
