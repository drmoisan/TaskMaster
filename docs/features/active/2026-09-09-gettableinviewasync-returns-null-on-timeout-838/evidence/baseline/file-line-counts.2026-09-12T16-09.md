# P0-T22 — Baseline file line counts and assertion-line count

Timestamp: 2026-09-13T02-46

Command: a single pwsh payload printing `(Get-Content -LiteralPath <file>).Count` for the two existing Write Set C# files, the count of lines in the clock test file containing the case-sensitive fixed literal `.Should()` through the plan's fixed search-gate form, and an existence test for each of the two files this plan creates.

EXIT_CODE: 0

```
LINES_TABLEACCESS=452
LINES_CLOCKTESTS=289
SHOULD_LINES_CLOCKTESTS=10
FAILURES_EXISTS=False
CONTRACTTESTS_EXISTS=False
```

Output Summary: all five values equal the values the plan's acceptance clause requires, so the delivered-source shape, which is expressed in terms of the line numbers of `OlTableExtensions.TableAccess.cs` as it stands at 452 lines, applies to this tree without adjustment. No divergence fired and the run does not halt. The assertion-line count of 10 is the value P3-T5's assertion-preservation gate compares against, so a later edit to the clock test file that added, removed or reorganised an assertion would be detected. Both files this plan creates are confirmed absent before Phase 1 and Phase 2 create them, which is what makes their creation gates false-before and true-after.
