---
name: pester5-result-shape-container-tests-and-ci-codecoverage
description: Pester 5.6.1 container objects have no Tests property (so $_.Tests.Count is silently 0) and Invoke-Pester -CI cannot be combined with -CodeCoverage
metadata:
  type: project
---

Two verified facts about direct `Invoke-Pester` (Pester 5.6.1, the version installed here) that turn
plausible-looking gates into gates that can never pass:

1. A container result object exposes
   `Name,Type,Item,Data,Blocks,Result,Duration,FailedCount,PassedCount,SkippedCount,InconclusiveCount,NotRunCount,TotalCount,ErrorRecord,...`
   — there is **no** `Tests` property. `"$($_.Tests.Count)"` therefore renders `0` for every file even
   when tests ran and passed (no StrictMode error, because the run happens in a plain `-Command` scope).
   Use `$_.TotalCount` (and `$_.FailedCount`) for the per-file inventory, or group the flattened
   `$r.Tests` by `$_.ScriptBlock.File`.
2. `-CodeCoverage` lives only in the **Legacy** parameter set; `-CI` lives in **Simple**. Combining them
   fails with "Parameter set cannot be resolved using the specified named parameters." For a scoped
   per-file coverage figure, use a configuration object (`$c.CodeCoverage.Path = @('<one file>')`) and
   read `$r.CodeCoverage.CoveragePercent`.

Also confirmed: `$r.CodeCoverage.CoveragePercent` is real; `$_.Item.FullName` is real; Pester discovers
test files under a dot-prefixed parent such as `tests/.claude/hooks/` without `-Force`; and the second
figure in the "Covered X% / Y%" console line is `CoveragePercentTarget`, not a branch metric.

**Why:** an atomic plan required an "executed-file inventory with a non-zero test count per named file"
and a `-CI -CodeCoverage` fallback; both were unsatisfiable as written across three tasks.
**How to apply:** before accepting any plan clause that reads a number off a Pester result object, run
the expression once against a throwaway fixture. See [[project_poshqc_pester_mcp_exit_minus1]] for the
MCP-route counterpart (no counts, no exit code at all).

3. **Failure-message shape for two strings.** `Should -Be` and `Should -BeExactly` with a string actual
   and a string expected print `Expected strings to be the same, because <text>, but they were different.`,
   then lengths, the differing index, `Expected: '<x>'` and `But was:  '<y>'`. They never print
   `Expected exactly <x>, ..., but got <y>.`; that form is for non-string operands, as is
   `Expected <x>, because ..., but got <y>.` Observed: #929 p1-t2 fail-before (`-Be`) and #911 rc2-t4
   (`-BeExactly '1.0.2'` against `''`). The #973 r0 plan expected `Expected exactly ` for a
   `($list -join ',') | Should -BeExactly` failure, and it also declared every other message shape a test
   defect. On a correct test that pair of clauses forces an endless fix loop. In preflight, classify the
   operand types of every expected-failure message assertion.
